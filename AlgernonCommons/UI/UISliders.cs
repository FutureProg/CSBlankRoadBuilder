// <copyright file="UISliders.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.UI
{
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// UI sliders.
    /// </summary>
    public static class UISliders
    {
        /// <summary>
        /// Adds an options panel-style slider with a descriptive text label above.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative x position.</param>
        /// <param name="yPos">Relative y position.</param>
        /// <param name="text">Descriptive label text.</param>
        /// <param name="min">Slider minimum value.</param>
        /// <param name="max">Slider maximum value.</param>
        /// <param name="step">Slider minimum step.</param>
        /// <param name="defaultValue">Slider initial value.</param>
        /// <param name="width">Slider width (excluding value label to right) (default 600).</param>
        /// <returns>New UI slider with attached labels.</returns>
        public static UISlider AddPlainSlider(UIComponent parent, float xPos, float yPos, string text, float min, float max, float step, float defaultValue, float width = 600f)
        {
            // Add slider component.
            UIPanel sliderPanel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsSliderTemplate")) as UIPanel;

            // Panel layout.
            sliderPanel.autoLayout = false;
            sliderPanel.autoSize = false;
            sliderPanel.width = width + 50f;
            sliderPanel.height = 65f;

            // Label.
            UILabel sliderLabel = sliderPanel.Find<UILabel>("Label");
            sliderLabel.autoHeight = true;
            sliderLabel.width = width;
            sliderLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            sliderLabel.relativePosition = Vector3.zero;
            sliderLabel.text = text;

            // Slider configuration.
            UISlider newSlider = sliderPanel.Find<UISlider>("Slider");
            newSlider.minValue = min;
            newSlider.maxValue = max;
            newSlider.stepSize = step;
            newSlider.value = defaultValue;

            // Move default slider position to match resized label.
            newSlider.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            newSlider.relativePosition = UILayout.PositionUnder(sliderLabel);

            newSlider.width = width;

            // Set position.
            newSlider.parent.relativePosition = new Vector2(xPos, yPos);

            return newSlider;
        }

        /// <summary>
        /// Adds an options panel-style slider with a descriptive text label above and an automatically updating value label immediately to the right.
        /// </summary>
        /// <param name="parent">Panel to add the control to.</param>
        /// <param name="xPos">Relative x position.</param>
        /// <param name="yPos">Relative y position.</param>
        /// <param name="text">Descriptive label text.</param>
        /// <param name="min">Slider minimum value.</param>
        /// <param name="max">Slider maximum value.</param>
        /// <param name="step">Slider minimum step.</param>
        /// <param name="defaultValue">Slider initial value.</param>
        /// <param name="width">Slider width (excluding value label to right) (default 600).</param>
        /// <returns>New UI slider with attached labels.</returns>
        public static UISlider AddPlainSliderWithValue(UIComponent parent, float xPos, float yPos, string text, float min, float max, float step, float defaultValue, float width = 600f)
        {
            // Add slider component.
            UISlider newSlider = AddPlainSlider(parent, xPos, yPos, text, min, max, step, defaultValue, width);
            UIPanel sliderPanel = (UIPanel)newSlider.parent;

            // Value label.
            UILabel valueLabel = sliderPanel.AddUIComponent<UILabel>();
            valueLabel.name = "ValueLabel";
            valueLabel.text = newSlider.value.ToString();
            valueLabel.relativePosition = UILayout.PositionRightOf(newSlider, 8f, 1f);

            // Event handler to update value label.
            newSlider.eventValueChanged += (component, value) =>
            {
                valueLabel.text = value.ToString();
            };

            return newSlider;
        }

        /// <summary>
        /// Adds a budget-style slider to the specified component.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative X position.</param>
        /// <param name="yPos">Relative Y position.</param>
        /// <param name="width">Slider width.</param>
        /// <param name="maxValue">Slider maximum value.</param>>
        /// <param name="tooltip">Tooltip text (null for none).</param>
        /// <returns>New UISlider.</returns>
        public static UISlider AddBudgetSlider(UIComponent parent, float xPos, float yPos, float width, float maxValue, string tooltip = null)
        {
            // Layout constants.
            const float SliderHeight = 18f;

            // Slider control.
            UISlider newSlider = parent.AddUIComponent<UISlider>();
            newSlider.size = new Vector2(width, SliderHeight);
            newSlider.relativePosition = new Vector2(xPos, yPos);

            // Tooltip.
            if (tooltip != null)
            {
                newSlider.tooltip = tooltip;
            }

            // Slider track.
            UISlicedSprite sliderSprite = newSlider.AddUIComponent<UISlicedSprite>();
            sliderSprite.atlas = UITextures.InGameAtlas;
            sliderSprite.spriteName = "BudgetSlider";
            sliderSprite.size = new Vector2(newSlider.width, 9f);
            sliderSprite.relativePosition = new Vector2(0f, 4f);

            // Slider thumb.
            UISlicedSprite sliderThumb = newSlider.AddUIComponent<UISlicedSprite>();
            sliderThumb.atlas = UITextures.InGameAtlas;
            sliderThumb.spriteName = "SliderBudget";
            newSlider.thumbObject = sliderThumb;

            // Set initial values.
            newSlider.stepSize = 1f;
            newSlider.minValue = 1f;

            return newSlider;
        }
    }
}