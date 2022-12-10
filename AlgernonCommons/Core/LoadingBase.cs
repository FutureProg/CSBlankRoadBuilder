// <copyright file="LoadingBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons
{
    using System.Collections.Generic;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using ICities;

    /// <summary>
    /// Base class for mod ICities.LoadingExtensionBase.
    /// </summary>
    /// <typeparam name="TOptionsPanel">Options panel type for in-game.</typeparam>
    public abstract class LoadingBase<TOptionsPanel> : LoadingExtensionBase
        where TOptionsPanel : UIPanel
    {
        // Status flags.
        private static bool s_isCreated = false;
        private static bool s_isLoaded = false;
        private bool _isModEnabled = false;

        // Conflicting mod list.
        private List<string> _conflictingMods;

        /// <summary>
        /// Gets a value indicating whether the mod has finished loading.
        /// </summary>
        public static bool IsLoaded => s_isLoaded;

        /// <summary>
        /// Gets a list of permitted loading modes.
        /// </summary>
        protected virtual List<AppMode> PermittedModes => new List<AppMode> { AppMode.Game };

        /// <summary>
        /// Gets any text for a trailing confict notification paragraph (e.g. "These mods must be removed before this mod can operate").
        /// </summary>
        protected virtual string ConflictRemovedText => string.Empty;

        /// <summary>
        /// Gets a value indicating whether a mod conflict was detected.
        /// </summary>
        protected bool WasModConflict => _conflictingMods != null && _conflictingMods.Count > 0;

        /// <summary>
        /// Called by the game when the mod is initialised at the start of the loading process.
        /// This is also called in other cases, e.g. when a mod is subscribed in-game (in which case OnCreated will be called again for *all* mods).
        /// </summary>
        /// <param name="loading">Loading mode (e.g. game or editor).</param>
        public override void OnCreated(ILoading loading)
        {
            // Check for duplicate creation calls.
            if (s_isCreated)
            {
                Logging.Message("second OnCreated call detected; skipping");
                return;
            }

            // By default, we're assuming that we're okay to go.
            _isModEnabled = true;

            // Check eligible loading mode.
            if (!PermittedModes.Contains(loading.currentMode))
            {
                _isModEnabled = false;
                Logging.KeyMessage("ineligible loading mode ", loading.currentMode, " detected; skipping activation");
            }
            else
            {
                // Elgible loading mode - peform other checks.
                _conflictingMods = CheckModConflicts();
                if (_conflictingMods != null && _conflictingMods.Count > 0)
                {
                    // Conflict detected.
                    _isModEnabled = false;
                    Logging.Error("mod conflict detected");
                }
                else if (!CreatedChecksPassed())
                {
                    // OnCreated checks failed.
                    _isModEnabled = false;
                    Logging.KeyMessage("OnCreated checks failed");
                }
            }

            // Check to make sure that we've passed all checks.
            if (!_isModEnabled)
            {
                Logging.KeyMessage("aborting load");

                // Perform any deactivation actions before returning.
                CreatedAbortActions();
                return;
            }

            // All good to go at this point.
            s_isCreated = true;

            Logging.KeyMessage("version ", AssemblyUtils.TrimmedCurrentVersion, " loading");

            // Perform any created actions.
            CreatedActions(loading);

            base.OnCreated(loading);
        }

        /// <summary>
        /// Called by the game when level loading is complete.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.)</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            // Check to see if a conflicting mod has been detected.
            if (_conflictingMods != null && _conflictingMods.Count > 0)
            {
                // Mod conflict detected - display warning notification and exit.
                ListNotification modConflictNotification = NotificationBase.ShowNotification<ListNotification>();
                if (modConflictNotification != null)
                {
                    // Key text items.
                    modConflictNotification.AddParas(Translations.Translate("CONFLICT_DETECTED"), Translations.Translate("UNABLE_TO_OPERATE"), Translations.Translate("CONFLICTING_MODS"));

                    // Add conflicting mod name(s).
                    modConflictNotification.AddList(_conflictingMods.ToArray());

                    // Closing para.
                    modConflictNotification.AddParas(ConflictRemovedText);
                }
            }

            // Don't do anything else if mod isn't enabled.
            if (!_isModEnabled)
            {
                return;
            }

            // Perform any post-loading actions.
            LoadedActions(mode);

            // Set up options panel event handler (need to redo this now that options panel has been reset after loading into game).
            OptionsPanelManager<TOptionsPanel>.OptionsEventHook();

            // Display update notification.
            WhatsNew.ShowWhatsNew();

            // Set loaded status flag.
            s_isLoaded = true;

            Logging.KeyMessage("loading complete");
        }

        /// <summary>
        /// Called by the game when exiting a level.
        /// </summary>
        public override void OnLevelUnloading()
        {
            s_isLoaded = false;

            base.OnLevelUnloading();
        }

        /// <summary>
        /// Called by the game when the mod is released.
        /// </summary>
        public override void OnReleased()
        {
            s_isCreated = false;

            base.OnReleased();
        }

        /// <summary>
        /// Determines whether this is a permitted loading mode for this mod (e.g. game only, specific editors, scenario).
        /// </summary>
        /// <param name="mode">Loading mode to check.</param>
        /// <returns>True if this is a permitted mode for this mod, false otherwise.</returns>
        protected virtual bool CheckMode(AppMode mode) => mode == AppMode.Game;

        /// <summary>
        /// Checks for any mod conflicts.
        /// Called as part of checking prior to executing any OnCreated actions.
        /// </summary>
        /// <returns>A list of conflicting mod names (null or empty if none).</returns>
        protected virtual List<string> CheckModConflicts() => null;

        /// <summary>
        /// Checks to make sure the mod is okay to proceed with loading.
        /// Called as part of checking prior to executing any OnCreated actions.
        /// </summary>
        /// <returns>True if mod is okay to proceed with OnCreated, false otherwise.</returns>
        protected virtual bool CreatedChecksPassed() => true;

        /// <summary>
        /// Performs any actions upon successful creation of the mod.
        /// E.g. Can be used to patch any other mods.
        /// </summary>
        /// <param name="loading">Loading mode (e.g. game or editor).</param>
        protected virtual void CreatedActions(ILoading loading)
        {
        }

        /// <summary>
        /// Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected virtual void LoadedActions(LoadMode mode)
        {
        }

        /// <summary>
        /// Performs any actions when the mod is deactivated during OnCreated.
        /// E.g. can be used to revert Harmony patches if a mod conflict is detected.
        /// </summary>
        protected virtual void CreatedAbortActions()
        {
        }
    }
}
