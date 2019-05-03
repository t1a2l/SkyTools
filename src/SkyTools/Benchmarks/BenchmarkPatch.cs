// <copyright file="BenchmarkPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using SkyTools.Patching;

    /// <summary>
    /// A class that defines the method patches for benchmarking.
    /// </summary>
    /// <seealso cref="PatchBase" />
    internal sealed class BenchmarkPatch : PatchBase
    {
        private static readonly object SyncObject = new object();
        private static readonly Dictionary<MethodInfo, long> Ticks = new Dictionary<MethodInfo, long>();

        /// <summary>Initializes a new instance of the <see cref="BenchmarkPatch"/> class.</summary>
        /// <param name="type">The type that holds the method to benchmark.</param>
        /// <param name="methodName">Name of the method to benchmark.</param>
        /// <param name="parameters">The method parameters.</param>
        public BenchmarkPatch(Type type, string methodName, IEnumerable<Type> parameters = null)
        {
            Method = type.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                null,
                parameters?.ToArray() ?? new Type[0],
                new ParameterModifier[0]);
        }

        /// <summary>Gets or sets the data collector shared instance for all benchmarking method patches.</summary>
        public static DataCollector DataCollector { get; set; }

        /// <summary>Gets the method this patch measures.</summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// Gets the <see cref="MethodInfo" /> instance of the method to patch.
        /// </summary>
        /// <returns>A <see cref="MethodInfo" /> instance of the method to patch.</returns>
        protected override MethodInfo GetMethod() => Method;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
        private static void Prefix(MethodInfo __originalMethod)
        {
            long started = Stopwatch.GetTimestamp();
            if (__originalMethod == null)
            {
                return;
            }

            lock (SyncObject)
            {
                Ticks[__originalMethod] = started;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
        private static void Postfix(MethodInfo __originalMethod)
        {
            long stopped = Stopwatch.GetTimestamp();
            if (__originalMethod == null || DataCollector == null)
            {
                return;
            }

            long started;
            lock (SyncObject)
            {
                Ticks.TryGetValue(__originalMethod, out started);
            }

            if (started == 0 || started > stopped)
            {
                return;
            }

            long elapsed = stopped - started;
            DataCollector.RecordSample(__originalMethod, elapsed);
        }
    }
}
