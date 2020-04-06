// <copyright file="ILocalizationProvider.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Localization
{
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// An interface for a class that can handle the localization.
    /// </summary>
    public interface ILocalizationProvider
    {
        /// <summary>Gets the current culture that is used for translation.</summary>
        CultureInfo CurrentCulture { get; }

        /// <summary>Translates a value that has the specified ID.</summary>
        /// <param name="id">The value ID.</param>
        /// <returns>The translated string value or an empty string when no translation is found.</returns>
        string Translate(string id);

        /// <summary>Gets a dictionary representing the game's translations that should be overridden
        /// by this mod. Can return null.</summary>
        /// <param name="type">The overridden translations type string.</param>
        /// <returns>A map of key-value pairs for translations to override, or null.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
        IDictionary<string, string> GetOverriddenTranslations(string type);

        /// <summary>Sets the state of the flag that forces the English-US culture to be applied if <see cref="CurrentCulture"/>
        /// is English. When loading another language, this will be automatically reset back.</summary>
        /// <param name="isEnabled"><c>true</c> to enable English-US formats, <c>false</c> to use English-GB.</param>
        void SetEnglishUSFormatsState(bool isEnabled);
    }
}