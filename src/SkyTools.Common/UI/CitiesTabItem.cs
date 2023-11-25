// <copyright file="CitiesTabItem.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace SkyTools.UI
{
    using System;
    using ColossalFramework.UI;
    using SkyTools.Localization;
    using UnityEngine;
    using static TerrainModify;

    /// <summary>A tab item container.</summary>
    /// <seealso cref="CitiesContainerItemBase"/>
    public sealed class CitiesTabItem : CitiesContainerItemBase
    {
        private const float VSCROLLBARWIDTH = 16f;
        private readonly UIButton tabButton;

        private CitiesTabItem(UIButton tabButton, UIHelper tabContainer, string id)
            : base(tabContainer, id)
        {
            this.tabButton = tabButton;
        }

        /// <summary>Creates and sets up an instance of the <see cref="CitiesTabItem"/> class.</summary>
        /// <param name="tabStrip">The tab strip to use as parent for the new tab item.</param>
        /// <param name="id">The unique ID of the new tab item.</param>
        /// <returns>An instance of <see cref="CitiesTabItem"/> or null on failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tabStrip"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or an empty string.</exception>
        public static CitiesTabItem Create(UITabstrip tabStrip, string id)
        {
            if (tabStrip == null)
            {
                throw new ArgumentNullException(nameof(tabStrip));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("The tab item's id cannot be null or an empty string", nameof(id));
            }

            var tabButton = tabStrip.AddTab();
            if (tabButton == null)
            {
                return null;
            }

            tabButton.autoSize = true;
            tabButton.textPadding.left = 10;
            tabButton.textPadding.right = 10;
            tabButton.textPadding.top = 10;
            tabButton.textPadding.bottom = 6;
            tabButton.wordWrap = false;
            tabButton.normalBgSprite = "SubBarButtonBase";
            tabButton.disabledBgSprite = "SubBarButtonBaseDisabled";

            tabButton.focusedBgSprite = "SubBarButtonBaseFocused";
            tabButton.hoveredBgSprite = "SubBarButtonBaseHovered";
            tabButton.pressedBgSprite = "SubBarButtonBasePressed";

            if (!(tabStrip.tabContainer.components[tabStrip.tabCount - 1] is UIPanel tabContainer))
            {
                return null;
            }

            var panel = CreateScrollablePanel(tabContainer, tabStrip);
            return new CitiesTabItem(tabButton, new UIHelper(panel), id);
        }

        /// <summary>Performs the actual view item translation.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation. Guaranteed to be not null.</param>
        ///
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="localizationProvider"/> is <c>null</c>.</exception>
        protected override void TranslateCore(ILocalizationProvider localizationProvider)
        {
            if (localizationProvider == null)
            {
                throw new ArgumentNullException(nameof(localizationProvider));
            }

            tabButton.text = localizationProvider.Translate(Id);
            tabButton.tooltip = tabButton.text;
        }

        private static UIScrollablePanel CreateScrollablePanel(UIPanel panel, UITabstrip tabStrip)
        {
            panel.autoLayout = true;
            panel.padding.top = 30;
            panel.autoLayoutDirection = LayoutDirection.Horizontal;

            var scrollablePanel = panel.AddUIComponent<UIScrollablePanel>();
            scrollablePanel.autoLayout = true;
            scrollablePanel.autoLayoutPadding = new RectOffset(10, 10, 0, 16);
            scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            scrollablePanel.wrapLayout = true;
            scrollablePanel.size = new Vector2(panel.size.x - VSCROLLBARWIDTH, panel.size.y);
            scrollablePanel.autoLayoutDirection = LayoutDirection.Horizontal; // Vertical does not work but why?

            var verticalScrollbar = CreateVerticalScrollbar(panel, scrollablePanel, tabStrip);
            verticalScrollbar.Show();
            verticalScrollbar.Invalidate();
            scrollablePanel.Invalidate();

            return scrollablePanel;
        }

        private static UIScrollbar CreateVerticalScrollbar(UIPanel panel, UIScrollablePanel scrollablePanel, UITabstrip tabStrip)
        {
            var verticalScrollbar = panel.AddUIComponent<UIScrollbar>();
            verticalScrollbar.name = "VerticalScrollbar";
            verticalScrollbar.width = VSCROLLBARWIDTH;
            verticalScrollbar.height = tabStrip.tabPages.height;
            verticalScrollbar.orientation = UIOrientation.Vertical;
            verticalScrollbar.pivot = UIPivotPoint.TopLeft;
            verticalScrollbar.AlignTo(panel, UIAlignAnchor.TopRight);
            verticalScrollbar.minValue = 0;
            verticalScrollbar.value = 0;
            verticalScrollbar.incrementAmount = 50;
            verticalScrollbar.autoHide = true;

            var trackSprite = verticalScrollbar.AddUIComponent<UISlicedSprite>();
            trackSprite.relativePosition = Vector2.zero;
            trackSprite.autoSize = true;
            trackSprite.size = trackSprite.parent.size;
            trackSprite.fillDirection = UIFillDirection.Vertical;
            trackSprite.spriteName = "ScrollbarTrack";
            verticalScrollbar.trackObject = trackSprite;

            var thumbSprite = trackSprite.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Vertical;
            thumbSprite.autoSize = true;
            thumbSprite.width = thumbSprite.parent.width;
            thumbSprite.spriteName = "ScrollbarThumb";
            verticalScrollbar.thumbObject = thumbSprite;

            verticalScrollbar.eventValueChanged += (component, value) =>
            {
                scrollablePanel.scrollPosition = new Vector2(0, value);
            };

            panel.eventMouseWheel += (component, eventParam) =>
            {
                verticalScrollbar.value -= (int)eventParam.wheelDelta * verticalScrollbar.incrementAmount;
            };

            scrollablePanel.eventMouseWheel += (component, eventParam) =>
            {
                verticalScrollbar.value -= (int)eventParam.wheelDelta * verticalScrollbar.incrementAmount;
            };

            scrollablePanel.verticalScrollbar = verticalScrollbar;

            return verticalScrollbar;
        }
    }
}