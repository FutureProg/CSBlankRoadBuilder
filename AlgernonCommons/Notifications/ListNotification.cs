// <copyright file="ListNotification.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Notifications
{
    using System.Collections.Generic;
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Simple notification anel with separate pargaraphs and/or lists of dot points.
    /// </summary>
    public class ListNotification : NotificationBase
    {
        // Close button.
        private UIButton _closeButton;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListNotification"/> class.
        /// </summary>
        public ListNotification()
        {
            // Set title.
            Title = ModBase.Instance.Name;

            // Add buttons.
            AddButtons();
        }

        /// <summary>
        /// Gets the close button instance.
        /// </summary>
        public UIButton CloseButton => _closeButton;

        /// <summary>
        /// Gets the number of buttons for this panel (for layout).
        /// </summary>
        protected virtual int NumButtons => 1;

        /// <summary>
        /// Adds buttons to the notification.
        /// </summary>
        public virtual void AddButtons()
        {
            _closeButton = AddButton(1, NumButtons, Translations.Translate("NOTE_CLOSE"), Close);
        }

        /// <summary>
        /// Add paragraphs to the notification.
        /// </summary>
        /// <param name="paragraphs">Text for each paragraph.</param>
        public void AddParas(params string[] paragraphs)
        {
            // Iterate through each provided paragraph and create a separate UI label for each item.
            for (int i = 0; i < paragraphs.Length; ++i)
            {
                // Basic setup.
                UILabel paraMessage = ScrollableContent.AddUIComponent<UILabel>();
                paraMessage.wordWrap = true;
                paraMessage.autoSize = false;
                paraMessage.autoHeight = true;
                paraMessage.width = ContentWidth;

                // Set text while we're autosizing.
                paraMessage.text = paragraphs[i];

                // Now that the height is set after autosizing with the final text, fix the height and add a buffer for the next paragraph.
                paraMessage.autoHeight = false;
                paraMessage.height += 10f;
            }
        }

        /// <summary>
        /// Creates a blank panel spacer.
        /// </summary>
        /// <param name="height">Spacer height (default 10).</param>
        public void AddSpacer(float height = 10f)
        {
            UILabel spacer = ScrollableContent.AddUIComponent<UILabel>();

            spacer.autoSize = false;
            spacer.height = height;
            spacer.width = 10f;
        }

        /// <summary>
        /// Add dot pointed list.
        /// </summary>
        /// <param name="listItems">Array of messages for display as separate dot points.</param>
        public void AddList(params string[] listItems)
        {
            // Iterate through each provided message string and create separate dot point for each item.
            for (int i = 0; i < listItems.Length; ++i)
            {
                ListItem listItem = ScrollableContent.AddUIComponent<ListItem>();
                listItem.width = ContentWidth;
                listItem.Text = listItems[i];
            }

            // Add spacer at end of list.
            AddSpacer();
        }

        /// <summary>
        /// Add dot pointed list.
        /// </summary>
        /// <param name="listItems">List of messages for display as separate dot points.</param>
        public void AddList(List<string> listItems)
        {
            // Iterate through each provided message string and create separate dot point for each item.
            foreach (string item in listItems)
            {
                ListItem listItem = ScrollableContent.AddUIComponent<ListItem>();
                listItem.width = ContentWidth;
                listItem.Text = item;
            }

            // Add spacer at end of list.
            AddSpacer();
        }

        /// <summary>
        /// A dot point list item for display.
        /// </summary>
        public class ListItem : UIPanel
        {
            // Layout constants.
            private const float DotPointX = 20f;
            private const float MessageX = 35f;
            private const float Padding = 3f;

            // Panel components.
            private readonly UILabel _dotPoint;
            private readonly UILabel _textLabel;

            /// <summary>
            /// Initializes a new instance of the <see cref="ListItem"/> class.
            /// </summary>
            public ListItem()
            {
                // Use manual sizing and layout to avoid issues.
                autoLayout = false;
                autoSize = false;

                // Dot point.
                _dotPoint = AddUIComponent<UILabel>();
                _dotPoint.autoSize = true;
                _dotPoint.relativePosition = new Vector2(DotPointX, 0f);
                _dotPoint.text = "-";

                // Text label.
                _textLabel = AddUIComponent<UILabel>();
                _textLabel.relativePosition = new Vector2(MessageX, Padding);
                _textLabel.textScale = 0.8f;
                _textLabel.wordWrap = true;
                _textLabel.autoSize = false;
                _textLabel.autoHeight = true;

                // Set list panel height.
                _textLabel.width = width - MessageX - Padding;
                height = _textLabel.height + (Padding * 2);
            }

            /// <summary>
            /// Gets or  sets the text to display.
            /// </summary>
            public string Text { get => _textLabel.text; set => _textLabel.text = value; }

            /// <summary>
            /// Set width of textlabel and height of panel; called when size is changed, e.g. when text is set/changed.
            /// </summary>
            protected override void OnSizeChanged()
            {
                base.OnSizeChanged();

                if (_textLabel != null)
                {
                    _textLabel.width = width - MessageX - Padding;
                    height = _textLabel.height + (Padding * 2);
                }
            }
        }
    }
}