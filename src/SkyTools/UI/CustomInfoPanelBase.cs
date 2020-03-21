// <copyright file="CustomInfoPanelBase.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace SkyTools.UI
{
    using System;
    using System.Linq;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>A base class for the customized world info panels.</summary>
    /// <typeparam name="T">The type of the game world info panel to customize.</typeparam>
    public abstract class CustomInfoPanelBase<T>
        where T : WorldInfoPanel
    {
        private float originalItemsPanelHeight;
        private float originalInfoPanelHeight;

        /// <summary>Initializes a new instance of the <see cref="CustomInfoPanelBase{T}"/> class.</summary>
        /// <param name="infoPanelName">Name of the game's panel object.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="infoPanelName"/> is null or an empty string.
        /// </exception>
        protected CustomInfoPanelBase(string infoPanelName)
        {
            if (string.IsNullOrEmpty(infoPanelName))
            {
                throw new ArgumentException("The panel name cannot be null or an empty string.", nameof(infoPanelName));
            }

            InfoPanelName = infoPanelName;
        }

        /// <summary>Gets the name of the information panel this instance customizes.</summary>
        protected string InfoPanelName { get; }

        /// <summary>Gets the container items panel of the info panel.</summary>
        protected UIPanel ItemsPanel { get; private set; }

        /// <summary>Disables the custom citizen info panel, if it is enabled.</summary>
        public void Disable()
        {
            DisableCore();
            ItemsPanel = null;
        }

        /// <summary>When implemented in derived classes, updates the custom information in this panel.</summary>
        /// <param name="instance">The game object instance to get the information from.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#", Justification = "Performance")]
        public abstract void UpdateCustomInfo(ref InstanceID instance);

        /// <summary>Initializes this instance and builds up the custom UI objects.</summary>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        protected bool Initialize()
        {
            ItemsPanel = GetItemsPanel(InfoPanelName);
            if (ItemsPanel == null)
            {
                return false;
            }

            originalItemsPanelHeight = ItemsPanel.height;
            originalInfoPanelHeight = ItemsPanel.parent.height;
            return InitializeCore();
        }

        /// <summary>Sets the custom panel's visibility in the info panel.</summary>
        /// <param name="customComponent">The custom UI component to set visibility of.</param>
        /// <param name="visible">if set to <c>true</c>, the custom panel must be visible.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="customComponent"/> is null.</exception>
        protected void SetCustomPanelVisibility(UIComponent customComponent, bool visible)
        {
            if (customComponent == null)
            {
                throw new ArgumentNullException(nameof(customComponent));
            }

            var parent = ItemsPanel.parent;
            if (parent?.isVisible != true || customComponent.isVisible == visible)
            {
                return;
            }

            customComponent.isVisible = visible;
            ItemsPanel.PerformLayout();
            var lastComponent = ItemsPanel.components[ItemsPanel.components.Count - 1];
            float delta = lastComponent.relativePosition.y + lastComponent.height + ItemsPanel.autoLayoutPadding.vertical - originalItemsPanelHeight;
            if (delta < 0)
            {
                delta = 0;
            }

            parent.height = originalInfoPanelHeight + delta;
        }

        /// <summary>When overridden in derive classes, builds up the custom UI objects for the info panel.</summary>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        protected abstract bool InitializeCore();

        /// <summary>When overridden in derived classes, destroys the custom UI objects for the info panel.</summary>
        protected abstract void DisableCore();

        private static UIPanel GetItemsPanel(string infoPanelName)
        {
            var panelGameObject = GameObject.Find(infoPanelName);
            if (panelGameObject == null)
            {
                Debug.LogWarning($"Failed to customize the info panel '{infoPanelName}'. No game object '{infoPanelName}' found.");
                return null;
            }

            var infoPanel = panelGameObject.GetComponent<T>();
            if (infoPanel == null)
            {
                Debug.LogWarning($"Failed to customize the info panel '{infoPanelName}'. No game object's component of type '{typeof(T).Name}' found.");
                return null;
            }

            return infoPanel.component.components.OfType<UIPanel>().FirstOrDefault();
        }
    }
}