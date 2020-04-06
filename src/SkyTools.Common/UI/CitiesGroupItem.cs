// <copyright file="CitiesGroupItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.UI
{
    using ColossalFramework.UI;
    using ICities;
    using SkyTools.Localization;
    using UnityEngine;

    /// <summary>A group view item.</summary>
    /// <seealso cref="CitiesContainerItemBase" />
    public sealed class CitiesGroupItem : CitiesContainerItemBase
    {
        private const string LabelName = "Label";
        private const string ContentPanelName = "Content";

        /// <summary>Initializes a new instance of the <see cref="CitiesGroupItem"/> class.</summary>
        /// <param name="group">The game's group item that represents this container.</param>
        /// <param name="id">The item's unique ID.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="id"/> is an empty string.
        /// </exception>
        public CitiesGroupItem(UIHelperBase group, string id)
            : base(group, id)
        {
            if ((group as UIHelper)?.self is UIPanel panel)
            {
                var contentPanel = panel.Find<UIPanel>(ContentPanelName);
                if (contentPanel != null)
                {
                    panel.autoLayoutPadding = new RectOffset(10, 10, 0, 16);
                }
            }
        }

        /// <summary>Performs the actual view item translation.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation. Guaranteed to be not null.</param>
        ///
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="localizationProvider"/> is <c>null</c>.</exception>
        protected override void TranslateCore(ILocalizationProvider localizationProvider)
        {
            if (localizationProvider == null)
            {
                throw new System.ArgumentNullException(nameof(localizationProvider));
            }

            if (!(Container is UIHelper content))
            {
                return;
            }

            var panel = ((UIComponent)content.self).parent;
            if (panel == null)
            {
                return;
            }

            var label = panel.Find<UILabel>(LabelName);
            if (label != null)
            {
                label.text = localizationProvider.Translate(Id);
            }
        }
    }
}