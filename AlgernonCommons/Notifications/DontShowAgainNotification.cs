// <copyright file="DontShowAgainNotification.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Notifications
{
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;

    /// <summary>
    /// Notification with separate pargaraphs and/or lists of dot points, with 'close' and 'dont show again' buttons.
    /// </summary>
    public class DontShowAgainNotification : ListNotification
    {
        // Don't Show Again button.
        private UIButton _dsaButton;

        /// <summary>
        /// Gets the "dont show again" button instance.
        /// </summary>
        public UIButton DSAButton => _dsaButton;

        /// <summary>
        /// Gets the number of buttons for this panel (for layout).
        /// </summary>
        protected override int NumButtons => 2;

        /// <summary>
        /// Adds buttons to the notification panel.
        /// </summary>
        public override void AddButtons()
        {
            base.AddButtons();

            // Add don't show again button.
            _dsaButton = this.AddButton(2, this.NumButtons, Translations.Translate("NOTE_DONTSHOWAGAIN"), this.Close);
        }
    }
}