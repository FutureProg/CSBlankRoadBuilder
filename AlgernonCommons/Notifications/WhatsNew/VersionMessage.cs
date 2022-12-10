// <copyright file="VersionMessage.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Notifications
{
    using System;
    using System.Collections.Generic;
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// 'What's new' message for a given version.
    /// </summary>
    public class VersionMessage : UIPanel
    {
        // Components.
        private readonly UIButton _minimizeButton;
        private List<ListNotification.ListItem> _listItems;

        // Version title.
        private string _versionTitle;

        // Visibility state.
        private bool _isExpanded;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionMessage"/> class.
        /// </summary>
        public VersionMessage()
        {
            // Init list before we do anything else.
            _listItems = new List<ListNotification.ListItem>();

            // Basic setup.
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoFitChildrenVertically = true;
            autoLayoutPadding = new RectOffset(0, 0, 2, 2);

            // Add minimize button (which will also be the version label).
            _minimizeButton = AddUIComponent<UIButton>();
            _minimizeButton.height = 20f;
            _minimizeButton.horizontalAlignment = UIHorizontalAlignment.Left;
            _minimizeButton.color = Color.white;
            _minimizeButton.textHorizontalAlignment = UIHorizontalAlignment.Left;

            // Toggle visible (minimized) state when clicked.
            _minimizeButton.eventClick += (component, eventParam) => ToggleExpanded();
        }

        /// <summary>
        /// Sets a value indicating whether the message should be displayed in an expanded or collapsed state.
        /// </summary>
        public bool IsCollapsed
        {
            set
            {
                _isExpanded = value;
                ToggleExpanded();
            }
        }

        /// <summary>
        /// Sets version message text.
        /// </summary>
        /// <param name="message">What's new message to display.</param>
        public void SetText(WhatsNewMessage message)
        {
            // Set version header and message text.
            _versionTitle = ModBase.Instance.BaseName + ' ' + AssemblyUtils.TrimVersion(message.Version);

            // Add message elements as separate list items.
            for (int i = 0; i < message.Messages.Length; ++i)
            {
                try
                {
                    // Message text is either a translation key or direct text, depending on the messageKeys setting.
                    ListNotification.ListItem newMessageLabel = AddUIComponent<ListNotification.ListItem>();
                    _listItems.Add(newMessageLabel);
                    newMessageLabel.Text = message.MessagesAreKeys ? Translations.Translate(message.Messages[i]) : message.Messages[i];

                    // Make sure initial width is set properly.
                    newMessageLabel.width = this.width;
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "exception showing what's new message");
                }
            }

            // Always start maximized.
            _isExpanded = true;

            // Set state indictor.
            UpdateState();
        }

        /// <summary>
        /// Handles size changed events, for e.g. when visibility changes.  Called by game as needed.
        /// </summary>
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            // Set width of button and label to match new width of list item (whose width has been set by the notification panel).
            if (_minimizeButton != null)
            {
                _minimizeButton.width = this.width;
            }

            // Set width of each item label.
            if (_listItems != null)
            {
                foreach (ListNotification.ListItem listItem in _listItems)
                {
                    listItem.width = this.width;
                }
            }
        }

        /// <summary>
        /// Toggles expanded/collapsed state of the update messages.
        /// </summary>
        private void ToggleExpanded()
        {
            // Toggle state and update state indicator.
            _isExpanded = !_isExpanded;
            UpdateState();

            // Show/hide each list item according to state.
            foreach (ListNotification.ListItem listItem in _listItems)
            {
                listItem.isVisible = _isExpanded;
            }
        }

        /// <summary>
        /// Sets expaned/collapsed state indicator.
        /// </summary>
        private void UpdateState() => _minimizeButton.text = (_isExpanded ? "▼ " : "► ") + _versionTitle;
    }
}