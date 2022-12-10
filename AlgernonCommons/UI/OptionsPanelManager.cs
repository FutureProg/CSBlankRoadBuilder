// <copyright file="OptionsPanelManager.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.UI
{
    using System;
    using ColossalFramework.Globalization;
    using ColossalFramework.UI;
    using ICities;
    using UnityEngine;

    /// <summary>
    /// Class to handle the mod's options panel.
    /// </summary>
    /// <typeparam name="TPanel">Mod option panel type.</typeparam>
    public abstract class OptionsPanelManager<TPanel>
        where TPanel : UIPanel
    {
        // Panel margin.
        private const float PanelMargin = 10f;

        // Parent UI panel references.
        private static UIScrollablePanel s_optionsParentPanel;
        private static UIPanel s_gameOptionsPanel;

        // Instance references.
        private static GameObject s_optionsGameObject;
        private static TPanel s_panel;

        /// <summary>
        /// Gets the panel width.
        /// </summary>
        public static float PanelWidth => s_optionsParentPanel?.width - (PanelMargin * 2f) ?? 744f;

        /// <summary>
        /// Gets the panel height.
        /// </summary>
        public static float PanelHeight => s_optionsParentPanel?.height - 15f ?? 753f;

        /// <summary>
        /// Gets a value indicating whether the options panel is currently active.
        /// </summary>
        public static bool IsOpen => s_panel != null;

        /// <summary>
        /// Options panel setup.
        /// </summary>
        /// <param name="helper">UIHelperBase parent.</param>
        public static void Setup(UIHelperBase helper)
        {
            // Set up parent panel.
            s_optionsParentPanel = ((UIHelper)helper).self as UIScrollablePanel;
            s_optionsParentPanel.autoLayout = false;
        }

        /// <summary>
        /// Attaches an event hook to options panel visibility, to enable/disable mod hokey when the panel is open.
        /// </summary>
        public static void OptionsEventHook()
        {
            // Get options panel instance.
            s_gameOptionsPanel = UIView.library.Get<UIPanel>("OptionsPanel");

            if (s_gameOptionsPanel == null)
            {
                Logging.Error("couldn't find OptionsPanel");
            }
            else
            {
                // Simple event hook to create/destroy GameObject based on appropriate visibility.
                s_gameOptionsPanel.eventVisibilityChanged += (c, isVisible) =>
                {
                    // Create/destroy based on whether or not we're now visible.
                    if (isVisible)
                    {
                        Create();
                    }
                    else
                    {
                        Close();
                    }
                };

                // Recreate panel on system locale change.
                LocaleManager.eventLocaleChanged += LocaleChanged;
            }
        }

        /// <summary>
        /// Refreshes the options panel (destroys and rebuilds) on a locale change when the options panel is open.
        /// </summary>
        public static void LocaleChanged()
        {
            if (s_gameOptionsPanel != null && s_gameOptionsPanel.isVisible)
            {
                Logging.KeyMessage("changing locale");

                Close();
                Create();
            }
        }

        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        private static void Create()
        {
            try
            {
                // If no instance already set, create one.
                if (s_optionsGameObject == null)
                {
                    // Create parent GameObject.
                    s_optionsGameObject = new GameObject(typeof(TPanel).Name);
                    s_optionsGameObject.transform.parent = s_optionsParentPanel.transform;

                    // Create a base panel attached to our game object, perfectly overlaying the game options panel.
                    s_panel = s_optionsGameObject.AddComponent<TPanel>();
                    s_panel.width = PanelWidth;
                    s_panel.height = PanelHeight;

                    // Needed to ensure position is consistent if we regenerate after initial opening (e.g. on language change).
                    s_panel.relativePosition = new Vector2(PanelMargin, PanelMargin);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception creating options panel");
            }
        }

        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        private static void Close()
        {
            // Save settings on close.
            ModBase.Instance?.SaveSettings();

            // We're no longer visible - destroy our game object.
            if (s_optionsGameObject != null)
            {
                UnityEngine.Object.Destroy(s_panel.gameObject);
                UnityEngine.Object.Destroy(s_panel);
                UnityEngine.Object.Destroy(s_optionsGameObject);

                // Release references.
                s_panel = null;
                s_optionsGameObject = null;
            }
        }
    }
}