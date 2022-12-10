// <copyright file="UILayout.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.UI
{
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// UI layout functions.
    /// </summary>
    public static class UILayout
    {
        /// <summary>
        /// Returns a relative position to the right of a specified UI component, suitable for placing an adjacent component.
        /// </summary>
        /// <param name="uIComponent">Original (anchor) UI component.</param>
        /// <param name="margin">Margin between components (default 8).</param>
        /// <param name="verticalOffset">Vertical offset from first to second component (default 0).</param>
        /// <returns>Offset position (to right of original).</returns>
        public static Vector2 PositionRightOf(UIComponent uIComponent, float margin = 8f, float verticalOffset = 0f)
        {
            return new Vector2(uIComponent.relativePosition.x + uIComponent.width + margin, uIComponent.relativePosition.y + verticalOffset);
        }

        /// <summary>
        /// Returns a relative position below a specified UI component, suitable for placing an adjacent component.
        /// </summary>
        /// <param name="uIComponent">Original (anchor) UI component.</param>
        /// <param name="margin">Margin between components (default 8).</param>
        /// <param name="horizontalOffset">Horizontal offset from first to second component (default 0).</param>
        /// <returns>Offset position (below original).</returns>
        public static Vector2 PositionUnder(UIComponent uIComponent, float margin = 8f, float horizontalOffset = 0f)
        {
            return new Vector2(uIComponent.relativePosition.x + horizontalOffset, uIComponent.relativePosition.y + uIComponent.height + margin);
        }
    }
}