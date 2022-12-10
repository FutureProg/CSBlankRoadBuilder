// <copyright file="WhatsNewNotification.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Notifications
{
    using System;

    /// <summary>
    /// 'What's new' notification.
    /// </summary>
    public class WhatsNewNotification : DontShowAgainNotification
    {
        /// <summary>
        /// Sets the 'what's new' messages to display.
        /// </summary>
        /// <param name="lastNotifiedVersion">Last notified version (version messages equal to or earlier than this will be minimized).</param>
        /// <param name="messages">Version update messages to display, in order (newest versions first), with a list of items (as translation keys) for each version.</param>
        public void SetMessages(Version lastNotifiedVersion, WhatsNewMessage[] messages)
        {
            // Iterate through each provided version and add it to the notification.
            foreach (WhatsNewMessage message in messages)
            {
                VersionMessage versionMessage = ScrollableContent.AddUIComponent<VersionMessage>();
                versionMessage.width = ContentWidth;
                versionMessage.SetText(message);

                // Add spacer below.
                AddSpacer();

                // Hide version messages that have already been notified (always showing versions with headers).
                if (message.Version <= lastNotifiedVersion)
                {
                    versionMessage.IsCollapsed = true;
                }
            }
        }
    }
}