// <copyright file="UISpacers.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.UI
{
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// UI layout spacers.
    /// </summary>
    public static class UISpacers
    {
        /// <summary>
        /// Adds an options-panel-style spacer bar.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative x-position.</param>
        /// <param name="yPos">Relative y-position.</param>
        /// <param name="width">Spacer width.</param>
        public static void AddOptionsSpacer(UIComponent parent, float xPos, float yPos, float width)
        {
            UIPanel spacerPanel = parent.AddUIComponent<UIPanel>();
            spacerPanel.width = width;
            spacerPanel.height = 5f;
            spacerPanel.relativePosition = new Vector2(xPos, yPos);
            spacerPanel.atlas = UITextures.InGameAtlas;
            spacerPanel.backgroundSprite = "ContentManagerItemBackground";
        }

        /// <summary>
        /// Adds an options-panel-style spacer bar with an attached title label.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative x-position.</param>
        /// <param name="yPos">Relative y-position.</param>
        /// <param name="width">Spacer width.</param>
        /// <param name="title">Title text.</param>
        /// <returns>Title label.</returns>
        public static UILabel AddTitleSpacer(UIComponent parent, float xPos, float yPos, float width, string title)
        {
            AddOptionsSpacer(parent, xPos, yPos, width);
            UILabel label = UILabels.AddLabel(parent, xPos, yPos + 20f, title, textScale: 1.2f);
            label.font = UIFonts.SemiBold;
            return label;
        }
    }
}