// <copyright file="Log.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Timers;

    /// <summary>
    /// Manages the logging. In 'Release' mode, only logs to the Unity's debug log.
    /// Also, the 'Log.Debug(...)' method calls will be eliminated in this mode.
    /// In 'Debug' mode logs additionally to a text file that is located on the Desktop.
    /// </summary>
    public static class Log
    {
#if DEBUG
        private const int FileWriteInterval = 1000; // ms
        private const string LogFileName = "SkyTools.log";

        private static readonly HashSet<Enum> ActiveCategories = new HashSet<Enum>();

        private static readonly object SyncObject = new object();
        private static readonly Queue<string> Storage = new Queue<string>();

        private static readonly Timer FlushTimer = new Timer(FileWriteInterval) { AutoReset = false };
        private static readonly string LogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), LogFileName);

        // Note: the official Unity 5 docs state that the ThreadStaticAttribute will cause the engine to crash.
        // However, this doesn't occur on my system. Anyway, this is only compiled in debug builds and won't affect the mod users.
        [ThreadStatic]
        private static StringBuilder messageBuilder;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Special debug configuration")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Debug build only")]
        static Log()
        {
            FlushTimer.Elapsed += FlushTimer_Elapsed;
            FlushTimer.Start();
        }

        private enum InternalCategories
        {
            Default,
        }

        /// <summary>Sets up the logging for specified logging categories.</summary>
        /// <param name="categories">The logging categories to activate.</param>
        public static void Setup(params Enum[] categories)
        {
            ActiveCategories.Clear();
            if (categories == null || categories.Length == 0)
            {
                return;
            }

            foreach (var item in categories)
            {
                ActiveCategories.Add(item);
            }
        }

#endif

        /// <summary>
        /// Logs a debug information. This method won't be compiled in the 'Release' mode.
        /// </summary>
        ///
        /// <param name="category">The log category of this log entry.</param>
        /// <param name="text">The text to log.</param>
        [Conditional("DEBUG")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1163", Justification = "Conditional compilation")]
        public static void Debug(Enum category, string text)
        {
#if DEBUG
            if (ActiveCategories.Contains(category))
            {
                DebugLog(text, category);
            }
#endif
        }

        /// <summary>
        /// Logs a debug information. This method won't be compiled in the 'Release' mode.
        /// </summary>
        ///
        /// <param name="category">The log category of this log entry.</param>
        /// <param name="gameTime">The current date and time in the game.</param>
        /// <param name="text">The text to log.</param>
        [Conditional("DEBUG")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1163", Justification = "Conditional compilation")]
        public static void Debug(Enum category, DateTime gameTime, string text)
        {
#if DEBUG
            if (ActiveCategories.Contains(category))
            {
                DebugLog(gameTime.ToString("dd.MM.yy HH:mm") + " --> " + text, category);
            }
#endif
        }

        /// <summary>
        /// Logs a debug information. This method won't be compiled in the 'Release' mode.
        /// </summary>
        ///
        /// <param name="condition">A condition whether the debug logging should occur.</param>
        /// <param name="category">The log category of this log entry.</param>
        /// <param name="text">The text to log.</param>
        [Conditional("DEBUG")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1163", Justification = "Conditional compilation")]
        public static void DebugIf(bool condition, Enum category, string text)
        {
#if DEBUG
            if (condition && ActiveCategories.Contains(category))
            {
                DebugLog(text, category);
            }
#endif
        }

        /// <summary>
        /// Logs an information text.
        /// </summary>
        ///
        /// <param name="text">The text to log.</param>
        public static void Info(string text)
        {
            UnityEngine.Debug.Log(text);

#if DEBUG
            DebugLog(text, InternalCategories.Default);
#endif
        }

        /// <summary>
        /// Logs a warning text.
        /// </summary>
        ///
        /// <param name="text">The text to log.</param>
        public static void Warning(string text)
        {
            UnityEngine.Debug.LogWarning(text);

#if DEBUG
            DebugLog(text, InternalCategories.Default);
#endif
        }

        /// <summary>
        /// Logs an error text.
        /// </summary>
        ///
        /// <param name="text">The text to log.</param>
        public static void Error(string text)
        {
            UnityEngine.Debug.LogError(text);

#if DEBUG
            DebugLog(text, InternalCategories.Default);
#endif
        }

#if DEBUG
        private static void DebugLog(string text, Enum category)
        {
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

        private static void FlushTimer_Elapsed(object sender, ElapsedEventArgs e)
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
                using (StreamWriter writer = File.AppendText(LogFilePath))
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
#endif
    }
}
