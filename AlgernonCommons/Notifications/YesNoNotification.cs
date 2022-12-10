// <copyright file="YesNoNotification.cs" company="algernon (K. Algernon A. Sheppard)">
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
    public class YesNoNotification : ListNotification
    {
        // Don't Show Again button.
        private UIButton _noButton;
        private UIButton _yesButton;

        /// <summary>
        /// Gets the 'No' button (button 1) instance.
        /// </summary>
        public UIButton NoButton => _noButton;

        /// <summary>
        /// Gets the 'Yes' button (button 2) instance.
        /// </summary>
        public UIButton YesButton => _yesButton;

        /// <summary>
        /// Gets the number of buttons for this panel (for layout).
        /// </summary>
        protected override int NumButtons => 2;

        /// <summary>
        /// Adds buttons to the message box.
        /// </summary>
        public override void AddButtons()
        {
            // Add no button.
            _noButton = AddButton(1, NumButtons, Translations.Translate("NO"), Close);

            // Add yes button.
            _yesButton = AddButton(2, NumButtons, Translations.Translate("YES"), Close);
        }
    }
}