// <copyright file="IViewItem.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.UI
{
    using SkyTools.Localization;

    /// <summary>A base interface for the view items.</summary>
    public interface IViewItem
    {
        /// <summary>Gets this item's ID.</summary>
        string Id { get; }

        /// <summary>Translates this view item using the specified localization provider.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
        void Translate(ILocalizationProvider localizationProvider);
    }
}