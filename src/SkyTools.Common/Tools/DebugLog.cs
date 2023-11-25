// <copyright file="DebugLog.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Tools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Timers;
    using UnityEngine;

    /// <summary>
    /// A class that manages the thread-safe debug logging.
    /// </summary>
    internal static class DebugLog
    {
        /// <summary>File write interval in milliseconds.</summary>
        private const int FileWriteInterval = 2000;

        private static readonly HashSet<Enum> ActiveCategories = new HashSet<Enum>();

        private static readonly object SyncObject = new object();
        private static readonly Queue<string> Storage = new Queue<string>();

        private static readonly Timer FlushTimer = new Timer(FileWriteInterval) { AutoReset = false };

        private static volatile bool isInitialized;

        /// <summary>
        /// Note: the official Unity 5 docs state that the ThreadStaticAttribute will cause the engine to crash.
        /// However, this doesn't occur on my system. Anyway, this is only compiled in debug builds and won't affect the mod users.
        /// </summary>
        [ThreadStatic]
        private static StringBuilder messageBuilder;

        /// <summary>Sets up the logging for specified logging categories.
        /// This method can be called only once.</summary>
        /// <param name="logFileName">The name of the log file without extension. Cannot be null or empty.</param>
        /// <param name="categories">The logging categories to activate. Cannot be null.</param>
        /// <exception cref="InvalidOperationException">Thrown when this method is called more than once.</exception>
        public static void SetupDebug(string logFileName, Enum[] categories)
        {
            lock (SyncObject)
            {
                if (isInitialized)
                {
                    throw new InvalidOperationException("This method can be only called once.");
                }

                string dir = Path.Combine(Application.dataPath, "Logs");
                string logFilePath = Path.Combine(dir, logFileName + ".log");
                FlushTimer.Elapsed += (s, e) => WriteLogFile(logFilePath);
                FlushTimer.Start();

                ActiveCategories.Clear();
                foreach (var item in categories)
                {
                    ActiveCategories.Add(item);
                }

                isInitialized = true;
            }
        }

        /// <summary>
        /// Logs the specified <paramref name="text"/> to the internal debug log.
        /// The message will be appended to the specified <paramref name="category"/>.
        /// </summary>
        /// <param name="text">The text message to log.</param>
        /// <param name="category">The message category.</param>
        public static void Log(string text, Enum category)
        {
            if (!isInitialized)
            {
                return;
            }

            // We do not sync the multithreaded access here because we only read
            if (!ActiveCategories.Contains(category))
            {
                return;
            }

            if (messageBuilder == null)
            {
                messageBuilder = new StringBuilder(1024);
            }

            messageBuilder.Length = 0;
            messageBuilder.Append(DateTime.Now.ToString("HH:mm:ss.ffff"));
            messageBuilder.Append('\t');
            messageBuilder.AppendFormat("{0,-10}", category);
            messageBuilder.Append("\t");
            messageBuilder.Append(text);
            string message = messageBuilder.ToString();
            lock (SyncObject)
            {
                Storage.Enqueue(message);
            }
        }

        private static void WriteLogFile(string logFilePath)
        {
            List<string> storageCopy;
            lock (SyncObject)
            {
                if (Storage.Count == 0)
                {
                    FlushTimer.Start();
                    return;
                }

                storageCopy = Storage.ToList();
                Storage.Clear();
            }

            try
            {
                using (var writer = File.AppendText(logFilePath))
                {
                    foreach (string line in storageCopy)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Error writing to the log file: " + ex.Message);
            }

            FlushTimer.Start();
        }
    }
}
