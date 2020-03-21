// <copyright file="LocalizationExtractor.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Localization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using ColossalFramework.Globalization;

    /// <summary>
    /// A helper class that extracts localized strings from the game (using the current game's language).
    /// </summary>
    public static class LocalizationExtractor
    {
        /// <summary>Extracts the localization strings and saves them to the specified file.</summary>
        /// <param name="targetPath">The target file path to save the results to.</param>
        /// <param name="attributeText">The optional string value that will be searched in the localization key attribute.</param>
        /// <exception cref="ArgumentException">Thrown when the target path is null or an empty string.</exception>
        public static void Extract(string targetPath, string attributeText)
        {
            if (string.IsNullOrEmpty(targetPath))
            {
                throw new ArgumentException("The target path cannot be null or an empty string", nameof(targetPath));
            }

            var constants = GetConstants(typeof(LocaleID))
                .Where(fi => fi.FieldType == typeof(string));

            if (!string.IsNullOrEmpty(attributeText))
            {
                constants = constants
                    .Where(fi => fi.GetCustomAttributes(typeof(LocaleAttribute), false)
                        .Cast<LocaleAttribute>()
                        .FirstOrDefault()?
                        .file?
                        .Contains(attributeText) ?? false);
            }

            using (var sw = new StreamWriter(targetPath))
            {
                foreach (var constant in constants)
                {
                    string key = (string)constant.GetValue(null);
                    sw.WriteLine($"<translation id=\"{key}\" value=\"{Locale.Get(key)}\"/>");
                }
            }
        }

        private static IEnumerable<FieldInfo> GetConstants(Type type)
            => type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(fi => fi.IsLiteral && !fi.IsInitOnly);
    }
}
