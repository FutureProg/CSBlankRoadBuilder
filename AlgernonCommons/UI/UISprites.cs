// <copyright file="UISprites.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.UI
{
    using ColossalFramework.UI;

    /// <summary>
    /// UI sprites.
    /// </summary>
    public static class UISprites
    {
        /// <summary>
        /// Resizes a sprite to the given maximum size while preserving the aspect ratio.
        /// </summary>
        /// <param name="sprite">Sprite to resize.</param>
        /// <param name="maxWidth">Maximum acceptable sprite width (0 or negative to ignore).</param>
        /// <param name="maxHeight">Maximum acceptable sprite height (0 or negative to ignore).</param>
        public static void ResizeSprite(UISprite sprite, float maxWidth, float maxHeight)
        {
            // Reset sprite size to default.
            sprite.width = sprite.spriteInfo.width;
            sprite.height = sprite.spriteInfo.height;

            // Don't do anything if sprite info height is zero or negative.
            if (sprite.spriteInfo.height <= 0)
            {
                return;
            }

            // Calculate the aspect ratio.
            float aspectRatio = sprite.spriteInfo.width / sprite.spriteInfo.height;

            // Resize to fit maximum width, if applicable.
            if (maxWidth > 0 & sprite.width > maxWidth)
            {
                sprite.width = maxWidth;
                sprite.height = maxWidth / aspectRatio;
            }

            // Resize to fit maximum height, if applicable.
            if (maxHeight > 0 & sprite.height > maxHeight)
            {
                sprite.height = maxHeight;
                sprite.width = maxHeight * aspectRatio;
            }
        }
    }
}