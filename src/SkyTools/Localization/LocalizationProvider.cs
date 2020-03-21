// <copyright file="LocalizationProvider.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace SkyTools.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using SkyTools.Tools;
    using static Constants;

    /// <summary>Manages the mod's localization.</summary>
    public sealed class LocalizationProvider : ILocalizationProvider
    {
        private readonly string modName;
        private readonly string localeStorage;
        private readonly Dictionary<string, string> translation = new Dictionary<string, string>();
        private readonly Dictionary<string, Dictionary<string, string>> overrides = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>Initializes a new instance of the <see cref="LocalizationProvider"/> class.</summary>
        /// <param name="modName">The name of the mod.</param>
        /// <param name="dataPath">The root path where the mod stores its data.</param>
        /// <exception cref="ArgumentException">Thrown when any argument is null or empty string.</exception>
        public LocalizationProvider(string modName, string dataPath)
        {
            if (string.IsNullOrEmpty(modName))
            {
                throw new ArgumentException("The mod name cannot be null or an empty string", nameof(modName));
            }

            if (string.IsNullOrEmpty(dataPath))
            {
                throw new ArgumentException("The data path cannot be null or empty string", nameof(dataPath));
            }

            localeStorage = Path.Combine(dataPath, LocaleFolder);
            this.modName = modName;
        }

        private enum LoadingResult
        {
            None,
            Success,
            Failure,
            AlreadyLoaded,
        }

        /// <summary>Gets the current culture that is used for translation.</summary>
        public CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentCulture;

        /// <summary>Translates a value that has the specified ID.</summary>
        /// <param name="id">The value ID.</param>
        /// <returns>The translated string value or an empty string when no translation is found.</returns>
        public string Translate(string id)
        {
            return translation.TryGetValue(id, out string value)
                ? value
                : string.Empty;
        }

        /// <summary>Loads the translation data for the specified language.</summary>
        /// <param name="language">The language to load the translation for.</param>
        /// <returns><c>true</c> when the translation data was loaded; otherwise, <c>false</c>.</returns>
        public bool LoadTranslation(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return false;
            }

            var result = Load(language);
            if (result == LoadingResult.Failure)
            {
                result = Load("en");
            }

            return result == LoadingResult.Success;
        }

        /// <summary>
        /// Sets the state of the flag that forces the English-US culture to be applied if <see cref="CurrentCulture" />
        /// is English. When loading another language, this will be automatically reset back.
        /// </summary>
        /// <param name="isEnabled"><c>true</c> to enable English-US formats, <c>false</c> to use English-GB.</param>
        public void SetEnglishUSFormatsState(bool isEnabled)
        {
            if (CurrentCulture.TwoLetterISOLanguageName != "en")
            {
                return;
            }

            string locale = isEnabled ? "en-US" : "en-GB";
            if (CurrentCulture.Name != locale)
            {
                CurrentCulture = new CultureInfo(locale);
            }
        }

        /// <summary>Gets a dictionary representing the game's translations that should be overridden
        /// by this mod. Can return null.</summary>
        /// <param name="type">The overridden translations type string.</param>
        /// <returns>A map of key-value pairs for translations to override, or null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public IDictionary<string, string> GetOverriddenTranslations(string type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            overrides.TryGetValue(type, out var result);
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "No security issues here")]
        private static string GetLocaleNameFromLanguage(string language)
        {
            language = language.ToLowerInvariant();
            switch (language)
            {
                case "en":
                    return "en-GB";

                case "de":
                    return "de-DE";

                case "es":
                    return "es-ES";

                case "fr":
                    return "fr-FR";

                case "ko":
                    return "ko-KR";

                case "pl":
                    return "pl-PL";

                case "pt":
                    return "pt-PT";

                case "ru":
                    return "ru-RU";

                case "zh":
                    return "zh-CN";
            }

            language += "-";
            var specificCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            var culture = Array.Find(specificCultures, c => c.Name.StartsWith(language));
            return culture?.Name ?? "en-GB";
        }

        private LoadingResult Load(string language)
        {
            if (CurrentCulture.TwoLetterISOLanguageName == language && translation.Count != 0)
            {
                return LoadingResult.AlreadyLoaded;
            }

            translation.Clear();
            overrides.Clear();

            string path = Path.Combine(localeStorage, language + FileExtension);
            if (!File.Exists(path))
            {
                Log.Error($"The '{modName}' mod cannot find a required localization file '{path}'");
                return LoadingResult.Failure;
            }

            try
            {
                CurrentCulture = new CultureInfo(GetLocaleNameFromLanguage(language));

                var doc = new XmlDocument();
                doc.Load(path);

                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case XmlTranslationNodeName:
                            translation[node.Attributes[XmlKeyAttribute].Value] = node.Attributes[XmlValueAttribute].Value;
                            break;

                        case XmlOverrideNodeName when node.HasChildNodes:
                            ReadOverrides(node);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"The '{modName}' mod cannot load data from localization file '{path}', error message: {ex}");
                translation.Clear();
                overrides.Clear();
                return LoadingResult.Failure;
            }

            return LoadingResult.Success;
        }

        private void ReadOverrides(XmlNode overridesNode)
        {
            string type = overridesNode.Attributes[XmlOverrideTypeAttribute]?.Value;
            if (type == null)
            {
                return;
            }

            if (!overrides.TryGetValue(type, out var typeOverrides))
            {
                typeOverrides = new Dictionary<string, string>();
                overrides[type] = typeOverrides;
            }

            foreach (XmlNode node in overridesNode.ChildNodes)
            {
                if (node.Name == XmlTranslationNodeName)
                {
                    typeOverrides[node.Attributes[XmlKeyAttribute].Value] = node.Attributes[XmlValueAttribute].Value;
                }
            }
        }
    }
}