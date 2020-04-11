// <copyright file="Log.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Tools
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Manages the logging. If DEBUG is defined at compile time, the <see cref="Log.Error"/>, <see cref="Log.Warning"/>,
    /// and <see cref="Log.Info"/> methods will additionally log to a file configured by a call
    /// to <see cref="Log.SetupDebug"/>. If DEBUG is not defined, all calls to methods having 'Debug' in their name
    /// (including <see cref="Log.SetupDebug"/>) will be eliminated.
    /// </summary>
    public static class Log
    {
        private enum InternalCategories
        {
            Default,
        }

        /// <summary>Sets up the logging for specified logging categories.
        /// This method can be called only once.
        /// This method call will only be compiled if DEBUG is defined.</summary>
        /// <param name="logName">The name of the log file without extension.</param>
        /// <param name="categories">The logging categories to activate.</param>
        [Conditional("DEBUG")]
        public static void SetupDebug(string logName, params Enum[] categories)
        {
            if (string.IsNullOrEmpty(logName))
            {
                throw new ArgumentException("The log name cannot be null or empty", nameof(logName));
            }

            if (categories == null)
            {
                categories = new Enum[0];
            }

            DebugLog.SetupDebug(logName, categories);
        }

        /// <summary>
        /// Logs a debug information. This method call will only be compiled if DEBUG is defined.
        /// </summary>
        ///
        /// <param name="category">The log category of this log entry.</param>
        /// <param name="text">The text to log.</param>
        [Conditional("DEBUG")]
        public static void Debug(Enum category, string text) => DebugLog.Log(text, category);

        /// <summary>
        /// Logs a debug information. This method call will only be compiled if DEBUG is defined.
        /// </summary>
        ///
        /// <param name="category">The log category of this log entry.</param>
        /// <param name="gameTime">The current date and time in the game.</param>
        /// <param name="text">The text to log.</param>
        [Conditional("DEBUG")]
        public static void Debug(Enum category, DateTime gameTime, string text)
            => DebugLog.Log(gameTime.ToString("dd.MM.yy HH:mm") + " --> " + text, category);

        /// <summary>
        /// Logs a debug information. This method call will only be compiled if DEBUG is defined.
        /// </summary>
        ///
        /// <param name="condition">A condition whether the debug logging should occur.</param>
        /// <param name="category">The log category of this log entry.</param>
        /// <param name="text">The text to log.</param>
        [Conditional("DEBUG")]
        public static void DebugIf(bool condition, Enum category, string text)
        {
            if (condition)
            {
                DebugLog.Log(text, category);
            }
        }

        /// <summary>
        /// Logs an information text.
        /// </summary>
        ///
        /// <param name="text">The text to log.</param>
        public static void Info(string text)
        {
            UnityEngine.Debug.Log(text);
            DebugLog.Log(text, InternalCategories.Default);
        }

        /// <summary>
        /// Logs a warning text.
        /// </summary>
        ///
        /// <param name="text">The text to log.</param>
        public static void Warning(string text)
        {
            UnityEngine.Debug.LogWarning(text);
            DebugLog.Log(text, InternalCategories.Default);
        }

        /// <summary>
        /// Logs an error text.
        /// </summary>
        ///
        /// <param name="text">The text to log.</param>
        public static void Error(string text)
        {
            UnityEngine.Debug.LogError(text);
            DebugLog.Log(text, InternalCategories.Default);
        }
    }
}
