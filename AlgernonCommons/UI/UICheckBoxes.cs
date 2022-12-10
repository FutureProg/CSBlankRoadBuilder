// <copyright file="UICheckBoxes.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.UI
{
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// UI checkboxes.
    /// </summary>
    public static class UICheckBoxes
    {
        /// <summary>
        /// Adds a checkbox with a descriptive text label immediately to the right.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative x position.</param>
        /// <param name="yPos">Relative y position.</param>
        /// <param name="text">Descriptive label text.</param>
        /// <param name="size">Checkbox size (default 16f).</param>
        /// <param name="textScale">Text scale of label (default 0.8).</param>
        /// <param name="tooltip">Tooltip, if any.</param>
        /// <returns>New UI checkbox with attached labels.</returns>
        public static UICheckBox AddLabelledCheckBox(UIComponent parent, float xPos, float yPos, string text, float size = 16f, float textScale = 0.8f, string tooltip = null)
        {
            // Create base checkbox.
            UICheckBox checkBox = AddCheckBox(parent, xPos, yPos, size, tooltip);

            // Label.
            checkBox.label = checkBox.AddUIComponent<UILabel>();
            checkBox.label.verticalAlignment = UIVerticalAlignment.Middle;
            checkBox.label.textScale = textScale;
            checkBox.label.autoSize = true;
            checkBox.label.text = text;

            // Dynamic width to accomodate label.
            checkBox.width = checkBox.label.width + 21f;
            checkBox.label.relativePosition = new Vector2(21f, ((checkBox.height - checkBox.label.height) / 2f) + 1f);

            return checkBox;
        }

        /// <summary>
        /// Adds a checkbox without a label.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative x position.</param>
        /// <param name="yPos">Relative y position.</param>
        /// <param name="size">Checkbox size (default 16f).</param>
        /// <param name="tooltip">Tooltip, if any.</param>
        /// <returns>New UI checkbox *without* attached labels.</returns>
        public static UICheckBox AddCheckBox(UIComponent parent, float xPos, float yPos, float size = 16f, string tooltip = null)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();

            // Size and position.
            checkBox.width = size;
            checkBox.height = size;
            checkBox.clipChildren = false;
            checkBox.relativePosition = new Vector2(xPos, yPos);

            // Unchecked sprite.
            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            sprite.atlas = UITextures.InGameAtlas;
            sprite.spriteName = "check-unchecked";
            sprite.size = checkBox.size;
            sprite.relativePosition = Vector2.zero;

            // Checked sprite.
            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).atlas = UITextures.InGameAtlas;
            ((UISprite)checkBox.checkedBoxObject).spriteName = "check-checked";
            checkBox.checkedBoxObject.size = new Vector2(size, size);
            checkBox.checkedBoxObject.relativePosition = Vector2.zero;

            // Add tooltip.
            if (tooltip != null)
            {
                checkBox.tooltip = tooltip;
            }

            return checkBox;
        }

        /// <summary>
        /// Adds a plain checkbox using the game's option panel checkbox template.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="text">Descriptive label text.</param>
        /// <returns>New checkbox using the game's option panel template.</returns>
        public static UICheckBox AddPlainCheckBox(UIComponent parent, string text)
        {
            UICheckBox checkBox = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsCheckBoxTemplate")) as UICheckBox;

            // Set text.
            checkBox.text = text;

            return checkBox;
        }

        /// <summary>
        /// Adds a plain checkbox using the game's option panel checkbox template.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative x position.</param>
        /// <param name="yPos">Relative y position.</param>
        /// <param name="text">Descriptive label text.</param>
        /// <returns>New checkbox using the game's option panel template.</returns>
        public static UICheckBox AddPlainCheckBox(UIComponent parent, float xPos, float yPos, string text)
        {
            UICheckBox checkBox = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsCheckBoxTemplate")) as UICheckBox;

            // Set text.
            checkBox.text = text;

            // Set relative position.
            checkBox.relativePosition = new Vector2(xPos, yPos);

            return checkBox;
        }

        /// <summary>
        /// Adss a plain checkbox using the game's option panel checkbox template, with word wrapping for the label.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative x position.</param>
        /// <param name="yPos">Relative y position.</param>
        /// <param name="text">Descriptive label text.</param>
        /// <param name="labelWidth">Label width.</param>
        /// <returns>New checkbox using the game's option panel template.</returns>
        public static UICheckBox AddPlainCheckBox(UIComponent parent, float xPos, float yPos, string text, float labelWidth)
        {
            // Create checkbox.
            UICheckBox checkBox = AddPlainCheckBox(parent, xPos, yPos, text);

            // Find label.
            UILabel label = checkBox.Find<UILabel>("Label");
            if (label != null)
            {
                // Apply label fixed width.
                label.autoSize = false;
                label.autoHeight = true;
                label.width = labelWidth;
                label.wordWrap = true;

                // Update label parent layout.
                checkBox.PerformLayout();

                // Update parent size.
                if (checkBox.height < label.height)
                {
                    checkBox.height = label.height + 2f;
                }
            }

            return checkBox;
        }

        /// <summary>
        /// Adds an icon toggle checkbox.
        /// Additions kindly provided by ST-Apps (Stefano Tenuta).
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative x position.</param>
        /// <param name="yPos">Relative y position.</param>
        /// <param name="atlas">Icon atlas.</param>
        /// <param name="checkedSpriteName">Checked sprite name.</param>
        /// <param name="uncheckedSpriteName">Unchecked sprite name.</param>
        /// <param name="backgroundAtlas">Background atlas.</param>
        /// <param name="backgroundSprite">Background sprite name.</param>
        /// <param name="width">Toggle width (default 35).</param>
        /// <param name="height">Toggle height (default 35).</param>
        /// <param name="tooltip">Tooltip, if any.</param>
        /// <returns>New UICheckbox.</returns>
        public static UICheckBox AddIconToggle(UIComponent parent, float xPos, float yPos, string atlas, string checkedSpriteName, string uncheckedSpriteName, string backgroundAtlas = "InGame", string backgroundSprite = "IconPolicyBaseRect", float width = 35f, float height = 35f, string tooltip = null)
        {
            // Create checkbox.
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();

            // Set size and position.
            checkBox.width = width;
            checkBox.height = height;
            checkBox.clipChildren = true;
            checkBox.relativePosition = new Vector2(xPos, yPos);

            // Add background panel.
            UIPanel panel = checkBox.AddUIComponent<UIPanel>();
            panel.atlas = UITextures.GetTextureAtlas(backgroundAtlas);
            panel.backgroundSprite = backgroundSprite;
            panel.size = checkBox.size;
            panel.relativePosition = Vector2.zero;

            // Event handler to toggle background state on check change.
            checkBox.eventCheckChanged += (c, isChecked) => panel.backgroundSprite = isChecked ? $"{backgroundSprite}Focused" : backgroundSprite;

            // Event handler to toggle background state on hover.
            checkBox.eventMouseEnter += (c, p) =>
            {
                // We don't have to change anything if we're in a disabled state.
                if (checkBox.isEnabled)
                {
                    panel.backgroundSprite = $"{backgroundSprite}Hovered";
                }
            };

            // Event handler to toggle background state on de-hover.
            checkBox.eventMouseLeave += (c, p) =>
            {
                // We don't have to change anything if we're in a disabled state.
                if (checkBox.isEnabled)
                {
                    panel.backgroundSprite = checkBox.isChecked ? $"{backgroundSprite}Focused" : backgroundSprite;
                }
            };

            // Event handler to toggle background state on isEnabled change.
            checkBox.eventIsEnabledChanged += (c, p) =>
            {
                panel.backgroundSprite = !p ? $"{backgroundSprite}Disabled" : backgroundSprite;
                panel.isEnabled = p;
            };

            // Unchecked sprite.
            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            sprite.atlas = UITextures.GetTextureAtlas(atlas);
            sprite.spriteName = uncheckedSpriteName;
            sprite.size = checkBox.size;
            sprite.relativePosition = Vector2.zero;

            // Checked sprite.
            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).atlas = UITextures.GetTextureAtlas(atlas);
            ((UISprite)checkBox.checkedBoxObject).spriteName = checkedSpriteName;
            checkBox.checkedBoxObject.size = checkBox.size;
            checkBox.checkedBoxObject.relativePosition = Vector2.zero;

            // Tooltip.
            if (tooltip != null)
            {
                checkBox.tooltip = tooltip;
            }

            return checkBox;
        }
    }
}
