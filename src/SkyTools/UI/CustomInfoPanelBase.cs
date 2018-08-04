// <copyright file="CustomInfoPanelBase.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace SkyTools.UI
{
    using System;
    using System.Linq;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>A base class for the customized world info panels.</summary>
    /// <typeparam name="T">The type of the game world info panel to customize.</typeparam>
    internal abstract class CustomInfoPanelBase<T>
        where T : WorldInfoPanel
    {
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
            ItemsPanel = null;
            DisableCore();
        }

        /// <summary>When implemented in derived classes, updates the custom information in this panel.</summary>
        /// <param name="citizenInstance">The game object instance to get the information from.</param>
        public abstract void UpdateCustomInfo(ref InstanceID citizenInstance);

        /// <summary>Initializes this instance and builds up the custom UI objects.</summary>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        protected bool Initialize()
        {
            ItemsPanel = GetItemsPanel(InfoPanelName);
            if (ItemsPanel == null)
            {
                return false;
            }

            return InitializeCore();
        }

        /// <summary>Sets the custom panel's visibility in the info panel.</summary>
        /// <param name="customPanel">The custom panel to set visibility of.</param>
        /// <param name="visible">if set to <c>true</c>, the custom panel must be visible.</param>
        protected void SetCustomPanelVisibility(UIPanel customPanel, bool visible)
        {
            UIComponent parent = ItemsPanel.parent;
            if (parent == null || !parent.isVisible || customPanel.isVisible == visible)
            {
                return;
            }

            customPanel.isVisible = visible;
            float delta = visible ? customPanel.height : -customPanel.height;
            parent.height += delta;
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

            T infoPanel = panelGameObject.GetComponent<T>();
            if (infoPanel == null)
            {
                Debug.LogWarning($"Failed to customize the info panel '{infoPanelName}'. No game object's component of type '{typeof(T).Name}' found.");
                return null;
            }

            return infoPanel.component.components.OfType<UIPanel>().FirstOrDefault();
        }
    }
}