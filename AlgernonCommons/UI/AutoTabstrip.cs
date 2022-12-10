// <copyright file="AutoTabstrip.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.UI
{
    using System.Linq;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Tabstrip class with custom tab button height and auto-resizing tabs.
    /// Use AutoTabstrip.AddTabStrip to add.
    /// </summary>
    public class AutoTabstrip : UITabstrip
    {
        // Tab row height.
        private float _tabHeight;

        /// <summary>
        /// Gets the tab height.
        /// </summary>
        public float TabHeight => _tabHeight;

        /// <summary>
        /// Adds an AutoTabstrip panel to the specified parent.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="posX">Relative X postion.</param>
        /// <param name="posY">Relative Y position.</param>
        /// <param name="width">Tabstrip panel width.</param>
        /// <param name="height">Tabstrip panel height (including tabs).</param>
        /// <param name="container">Tabstrip tab container instance.</param>
        /// <param name="tabHeight">Tabstrip tab height (default 25f).</param>
        /// <returns>New AutoTabstrip.</returns>
        public static AutoTabstrip AddTabstrip(UIComponent parent, float posX, float posY, float width, float height, out UITabContainer container, float tabHeight = 25f)
        {
            AutoTabstrip newTabstrip = UITabstrips.AddTabstrip<AutoTabstrip>(parent, posX, posY, width, height, out container, tabHeight);
            newTabstrip._tabHeight = tabHeight;
            return newTabstrip;
        }

        /// <summary>
        /// Equalizes tab widths across the full tabstrip width.
        /// </summary>
        public void EqualizeTabs()
        {
            // Calculate new width.
            float targetWidth = Mathf.Floor(width / tabCount);

            // Apply to each tab button.
            foreach (UIButton tabButton in m_ChildComponents.OfType<UIButton>())
            {
                tabButton.width = targetWidth;
            }
        }

        /// <summary>
        /// Called by the game whenever a child component is added.
        /// Used here to automatically resize tabs.
        /// </summary>
        /// <param name="child">New child component.</param>
        protected override void OnComponentAdded(UIComponent child)
        {
            base.OnComponentAdded(child);

            // Resize tabs if appropriate.
            if (child is UIButton newButton)
            {
                // Assign height and enable wordwrapping.
                newButton.height = _tabHeight;
                newButton.wordWrap = true;

                // Calculate new width.
                float targetWidth = Mathf.Floor(width / tabCount);

                // Apply to each tab button.
                foreach (UIButton tabButton in m_ChildComponents.OfType<UIButton>())
                {
                    tabButton.width = targetWidth;
                }
            }
        }
    }
}