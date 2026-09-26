// <copyright file="CitiesSliderItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.UI
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using ColossalFramework.UI;
    using ICities;
    using SkyTools.Localization;

    /// <summary>A slider view item.</summary>
    public sealed class CitiesSliderItem : CitiesViewItem<UISlider, float>
    {
        private const string LabelName = "Label";
        private const int SliderValueLabelPadding = 20;
        private const float SliderLabelWidth = 240f;

        private readonly UILabel valueLabel;
        private readonly SliderValueType valueType;
        private readonly float displayMultiplier;

        private readonly Func<object> configurationProvider;
        private readonly PropertyInfo minSourceProperty;
        private readonly PropertyInfo maxSourceProperty;
        private readonly float originalMin;
        private readonly float originalMax;

        private CultureInfo currentCulture;

        /// <summary>Initializes a new instance of the <see cref="CitiesSliderItem"/> class.</summary>
        /// <param name="uiHelper">The game's UI helper reference.</param>
        /// <param name="id">The view item's unique ID.</param>
        /// <param name="property">
        /// The property description that specifies the target property where to store the value.
        /// </param>
        /// <param name="configProvider">A method that provides the configuration storage object for the value.</param>
        /// <param name="min">The minimum slider value.</param>
        /// <param name="max">The maximum slider value.</param>
        /// <param name="step">The slider step value. Default is 1.</param>
        /// <param name="valueType">The type of the value to display. Default is <see cref="SliderValueType.Percentage"/>.</param>
        /// <param name="displayMultiplier">A value that will be multiplied with original values for displaying purposes.</param>
        /// <param name="minFrom">The name of a configuration property whose current value determines this slider's minimum.</param>
        /// <param name="maxFrom">The name of a configuration property whose current value determines this slider's maximum.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// thrown when the <paramref name="id"/> is an empty string.
        /// </exception>
        public CitiesSliderItem(
            UIHelperBase uiHelper,
            string id,
            PropertyInfo property,
            Func<object> configProvider,
            float min,
            float max,
            float step,
            SliderValueType valueType,
            float displayMultiplier,
            string minFrom,
            string maxFrom)
            : base(uiHelper, id, property, configProvider)
        {
            UIComponent.minValue = min;
            UIComponent.maxValue = max;
            UIComponent.stepSize = step;
            UIComponent.width = 320;
            this.valueType = valueType;
            this.displayMultiplier = displayMultiplier;
            var parentPanel = UIComponent.parent as UIPanel;
            if (parentPanel != null)
            {
                parentPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                parentPanel.autoFitChildrenHorizontally = true;
                parentPanel.autoFitChildrenVertically = true;

                var label = parentPanel.components.OfType<UILabel>().FirstOrDefault();
                if (label != null)
                {
                    label.width = SliderLabelWidth;
                    label.padding.right = 10;
                }
            }

            if (UIComponent.parent != null)
            {
                valueLabel = UIComponent.parent.AddUIComponent<UILabel>();
                valueLabel.padding.left = SliderValueLabelPadding;
                valueLabel.name = id + LabelName;
                UpdateValueLabel(Value);
            }

            Refresh();

            configurationProvider = configProvider;
            originalMin = min;
            originalMax = max;

            object config = configProvider();
            var configType = config.GetType();

            if (!string.IsNullOrEmpty(minFrom))
            {
                minSourceProperty = configType.GetProperty(minFrom);

                if (minSourceProperty == null || minSourceProperty.PropertyType != typeof(float))
                {
                    throw new ArgumentException($"'{minFrom}' must name a float property.", nameof(minFrom));
                }
            }

            if (!string.IsNullOrEmpty(maxFrom))
            {
                maxSourceProperty = configType.GetProperty(maxFrom);

                if (maxSourceProperty == null || maxSourceProperty.PropertyType != typeof(float))
                {
                    throw new ArgumentException($"'{maxFrom}' must name a float property.", nameof(maxFrom));
                }
            }

            RefreshRange();
        }

        /// <summary>
        /// Occurs after the slider value has been written to its configuration property.
        /// </summary>
        public event Action<float> ValueUpdated;

        /// <summary>
        /// Gets the current value from the bound configuration property.
        /// </summary>
        public float CurrentValue => Value;

        /// <summary>Translates this view item using the specified localization provider.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public override void Translate(ILocalizationProvider localizationProvider)
        {
            if (localizationProvider == null)
            {
                throw new ArgumentNullException(nameof(localizationProvider));
            }

            var panel = UIComponent.parent;
            if (panel == null)
            {
                return;
            }

            panel.tooltip = localizationProvider.Translate(UIComponent.name + Constants.Tooltip);

            var label = panel.Find<UILabel>(LabelName);
            if (label != null)
            {
                label.text = localizationProvider.Translate(UIComponent.name);
            }

            currentCulture = localizationProvider.CurrentCulture;
            UpdateValueLabel(Value);
        }

        /// <summary>Changes the allowed range and clamps the current value if necessary.</summary>
        /// <param name="min">Update the minimum range for the slider.</param>
        /// <param name="max">Update the maximum range for the slider.</param>
        public void SetRange(float min, float max)
        {
            if (max <= min)
            {
                throw new ArgumentException("The maximum value must be greater than the minimum value.");
            }

            UIComponent.minValue = min;
            UIComponent.maxValue = max;

            float clamped = Math.Max(min, Math.Min(max, CurrentValue));

            if (clamped != CurrentValue)
            {
                // Setting UISlider.value may raise ValueChanged. If it does not,
                // explicitly synchronize the bound property below.
                UIComponent.value = clamped;

                if (CurrentValue != clamped)
                {
                    ValueChanged(clamped);
                }
            }

            UpdateValueLabel(UIComponent.value);
        }

        /// <summary>
        /// Refreshes this view item by re-fetching its value from the bound configuration property.
        /// </summary>
        public override void Refresh() => UIComponent.value = Value;

        /// <summary>Creates the view item using the specified <see cref="UIHelperBase"/>.</summary>
        /// <param name="uiHelper">The UI helper to use for item creation.</param>
        /// <param name="defaultValue">The item's default value.</param>
        /// <returns>A newly created view item.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="uiHelper"/> is null.
        /// </exception>
        protected override UISlider CreateItem(UIHelperBase uiHelper, float defaultValue)
        {
            if (uiHelper == null)
            {
                throw new ArgumentNullException(nameof(uiHelper));
            }

            return (UISlider)uiHelper.AddSlider(Constants.Placeholder, defaultValue, defaultValue + 1, 1, defaultValue, ValueChanged);
        }

        /// <summary>Updates the current configuration item value.</summary>
        /// <param name="newValue">The new item value.</param>
        protected override void ValueChanged(float newValue)
        {
            float previousValue = Value;

            base.ValueChanged(newValue);

            if (valueLabel != null)
            {
                UpdateValueLabel(newValue);
            }

            if (previousValue != Value)
            {
                ValueUpdated?.Invoke(Value);
            }
        }

        private void UpdateValueLabel(float value)
        {
            string valueString;
            switch (valueType)
            {
                case SliderValueType.Percentage:
                    valueString = (value * displayMultiplier / 100f).ToString("P0", currentCulture ?? CultureInfo.CurrentCulture);
                    break;

                case SliderValueType.Time:
                    valueString = default(DateTime).AddHours(value).ToString("t", currentCulture ?? CultureInfo.CurrentCulture);
                    break;

                case SliderValueType.Duration:
                    var ts = TimeSpan.FromHours(value);
                    valueString = $"{ts.Hours}:{ts.Minutes:00}";
                    break;

                default:
                    valueString = (value * displayMultiplier).ToString(currentCulture ?? CultureInfo.CurrentCulture);
                    break;
            }

            valueLabel.text = valueString;
        }

        private void RefreshRange()
        {
            object config = configurationProvider();

            float min = minSourceProperty == null ? originalMin : Math.Max(originalMin, (float)minSourceProperty.GetValue(config, null));
            float max = maxSourceProperty == null ? originalMax : Math.Min(originalMax, (float)maxSourceProperty.GetValue(config, null));

            SetRange(min, max);
        }
    }
}