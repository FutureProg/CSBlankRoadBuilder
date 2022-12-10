// <copyright file="OptionsMod.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons
{
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using ICities;

    /// <summary>
    /// Base mod class with settings file and an options panel.
    /// </summary>
    /// <typeparam name="TOptionsPanel">Options panel type for main menu.</typeparam>
    public abstract class OptionsMod<TOptionsPanel> : ModBase
        where TOptionsPanel : UIPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsMod{TOptionsPanel}"/> class.
        /// </summary>
        public OptionsMod()
        {
            // Set instance reference.
            OptionsInstance = this;
        }

        /// <summary>
        /// Gets the active instance reference.
        /// </summary>
        public static OptionsMod<TOptionsPanel> OptionsInstance { get; private set; }

        /// <summary>
        /// Called by the game when the mod is enabled.
        /// </summary>
        public override void OnEnabled()
        {
            base.OnEnabled();

            // Add the options panel event handler for the start screen (to enable/disable options panel based on visibility).
            // First, check to see if UIView is ready.
            if (UIView.GetAView() != null)
            {
                // It's ready - attach the hook now.
                OptionsPanelManager<TOptionsPanel>.OptionsEventHook();
            }
            else
            {
                // Otherwise, queue the hook for when the intro's finished loading.
                LoadingManager.instance.m_introLoaded += OptionsPanelManager<TOptionsPanel>.OptionsEventHook;
            }
        }

        /// <summary>
        /// Called by the game when the mod options panel is setup.
        /// </summary>
        /// <param name="helper">UI helper instance.</param>
        public void OnSettingsUI(UIHelperBase helper)
        {
            // Create options panel.
            OptionsPanelManager<TOptionsPanel>.Setup(helper);
        }
    }
}