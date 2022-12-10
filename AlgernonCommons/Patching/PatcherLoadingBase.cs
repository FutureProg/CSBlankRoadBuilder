// <copyright file="PatcherLoadingBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Patching
{
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;
    using ICities;

    /// <summary>
    /// Base class for mod ICities.LoadingExtensionBase with Harmony patching detection.
    /// </summary>
    /// <typeparam name="TOptionsPanel">Options panel type to hook.</typeparam>
    /// <typeparam name="TPatcher">Harmony patcher type.</typeparam>
    public abstract class PatcherLoadingBase<TOptionsPanel, TPatcher> : LoadingBase<TOptionsPanel>
        where TOptionsPanel : UIPanel
        where TPatcher : PatcherBase, new()
    {
        // Harmony status flag.
        private bool _harmonyError;

        /// <summary>
        /// Called by the game when level loading is complete.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.)</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            // Check to see that Harmony 2 was properly loaded.
            // If any mod conflicts were encountered, though, we skip this notification and fall through to base to display the conflict notification instead.
            if (_harmonyError && !WasModConflict)
            {
                // Harmony 2 wasn't loaded; display warning notification and exit.
                ListNotification harmonyNotification = NotificationBase.ShowNotification<ListNotification>();
                if (harmonyNotification != null)
                {
                    // Key text items.
                    harmonyNotification.AddParas(Translations.Translate("HARMONY_ERROR"), Translations.Translate("UNABLE_TO_OPERATE"), Translations.Translate("HARMONY_PROBLEM_CAUSES"));

                    // List of dot points.
                    harmonyNotification.AddList(Translations.Translate("HARMONY_NOT_INSTALLED"), Translations.Translate("HARMONY_MOD_ERROR"), Translations.Translate("HARMONY_MOD_CONFLICT"));
                }

                // Exit - don't call base.
                return;
            }

            // If Harmony was fine (or the issue was a mod conflict), then continue as normal.
            base.OnLevelLoaded(mode);
        }

        /// <summary>
        /// Checks to make sure the mod is okay to proceed with loading.
        /// Called as part of checking prior to executing any OnCreated actions.
        /// </summary>
        /// <returns>True if mod is okay to proceed with OnCreated, false otherwise.</returns>
        protected override bool CreatedChecksPassed()
        {
            _harmonyError = !PatcherManager<TPatcher>.Instance?.Patched ?? true;
            if (_harmonyError)
            {
                Logging.Error("Harmony patches not applied");
                return false;
            }

            // If we got here, all good; return true.
            return true;
        }

        /// <summary>
        /// Performs any actions when the mod is deactivated during OnCreated.
        /// E.g. can be used to revert Harmony patches if a mod conflict is detected.
        /// </summary>
        protected override void CreatedAbortActions() => PatcherManager<TPatcher>.Instance?.UnpatchAll();
    }
}
