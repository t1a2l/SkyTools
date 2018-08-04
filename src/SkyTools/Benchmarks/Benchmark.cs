// <copyright file="Benchmark.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SkyTools.Patching;
    using SkyTools.Tools;

    /// <summary>
    /// A method execution benchmark. Activate it after the game level is loaded.
    /// Call the <see cref="MakeSnapshot"/> method periodically to gather the performance data snapshots.
    /// The calling frequency should correlate with the data window size you specify when creating an instance.
    /// </summary>
    public sealed class Benchmark
    {
        private const string HarmonyId = "com.cities_skylines.dymanoid.skytools.benchmarks";

        private readonly List<IPatch> patches = new List<IPatch>();
        private MethodPatcher patcher;
        private bool isRunning;

        private Benchmark()
        {
        }

        /// <summary>Creates a new instance of the benchmark.</summary>
        /// <param name="windowSize">The Size of the data window to use for data collection.</param>
        /// <returns>A new instance of the <see cref="Benchmark"/> class.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the window size is less than 16.</exception>
        public static Benchmark Create(int windowSize)
        {
            if (windowSize < 16)
            {
                throw new ArgumentOutOfRangeException(nameof(windowSize), windowSize, "The window size must be greater than or equal to 16");
            }

            BenchmarkPatch.DataCollector = new DataCollector(windowSize);
            return new Benchmark();
        }

        /// <summary>Adds the specified method to this benchmark.</summary>
        /// <param name="type">The type that contains the method.</param>
        /// <param name="methodName">Name of the method to benchmark.</param>
        /// <param name="methodParameters">The method parameters.</param>
        /// <returns><c>true</c> if the method was added to the benchmark; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="methodName"/>> is null or an empty string.</exception>
        public bool BenchmarkMethod(Type type, string methodName, params Type[] methodParameters)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentException("The method name cannot be null or an empty string", nameof(methodName));
            }

            try
            {
                var patch = new BenchmarkPatch(type, methodName, methodParameters);
                patches.Add(patch);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Cannot benchmark the specified method {type.FullName}.{methodName}: {ex}");
                return false;
            }
        }

        /// <summary>Starts the benchmarking by applying the corresponding method patches.</summary>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public bool Start()
        {
            BenchmarkPatch.DataCollector.Clear();
            patcher = new MethodPatcher(HarmonyId, patches);

            try
            {
                patcher.Apply();
                isRunning = true;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to start benchmarking: " + ex);
                return false;
            }
        }

        /// <summary>Stops the benchmarking if it is running.</summary>
        public void Stop()
        {
            if (!isRunning)
            {
                return;
            }

            patcher.Revert();
            isRunning = false;
        }

        /// <summary>Makes a snapshot of currently recorded performance data.
        /// This method should be called periodically while the benchmarking is running.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Public interface, must be instance method")]
        public void MakeSnapshot()
        {
            BenchmarkPatch.DataCollector.MakeSnapshot();
        }

        /// <summary>Dumps the currently available performance data to the game's log file.</summary>
        public void Dump()
        {
            if (patches.Count == 0)
            {
                Log.Info("SkyTools benchmark has recorded no data. Make sure at least one snapshot has been created.");
                return;
            }

            var methods = patches.Cast<BenchmarkPatch>().Select(p => p.Method).ToList();
            string[] methodNames = methods.Select(m => m.ToFullString() + ";;;").ToArray();

            const string columns = "Count;Average;Median;Maximum;";
            string headers = string.Join(string.Empty, Enumerable.Repeat(columns, methodNames.Length).ToArray());

            string header =
                "------------------------------------------------------------------" + Environment.NewLine +
                "-----                       SkyTools Benchmark             -------" + Environment.NewLine +
                "------------------------------------------------------------------" + Environment.NewLine +
                string.Join(";", methodNames) + Environment.NewLine +
                headers + Environment.NewLine;

            string data = BenchmarkPatch.DataCollector.Dump(methods);
            Log.Info(header + data);
        }
    }
}
