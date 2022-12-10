// <copyright file="WhatsNew.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Notifications
{
    using System;

    /// <summary>
    /// "What's new" notification.  Based on macsergey's code in Intersection Marking Tool (Node Markup) mod.
    /// </summary>
    public static class WhatsNew
    {
        /// <summary>
        /// Gets or sets the last notified version.
        /// </summary>
        internal static Version LastNotifiedVersion { get; set; } = new Version("0.0");

        /// <summary>
        /// Gets or sets the last notified version as a string (empty string if none).
        /// </summary>
        internal static string LastNotifiedVersionString
        {
            // If no version is set, return an empty string.
            get => LastNotifiedVersion?.ToString() ?? string.Empty;

            set
            {
                // Don't do anything if there's an empty string.
                if (!string.IsNullOrEmpty(value))
                {
                    // Catch any exceptions due to invalid input.
                    try
                    {
                        LastNotifiedVersion = new Version(value);
                    }
                    catch (Exception e)
                    {
                        // Don't really care - just leave things unchanged.
                        Logging.LogException(e, "exception setting last notified version with ", value);
                    }
                }
            }
        }

        /// <summary>
        /// Check if there's been an update since the last notification, and if so, show the update.
        /// </summary>
        public static void ShowWhatsNew()
        {
            // Get messages from mod instance.
            if (ModBase.Instance?.WhatsNewMessages is WhatsNewMessage[] messages && messages.Length > 0)
            {
                // Don't show notification if we're already up to (or ahead of) the first what's new message.
                if (LastNotifiedVersion < messages[0].Version)
                {
                    // Show notification.
                    Logging.Message("showing what's new notification: last notified version was ", LastNotifiedVersionString);
                    WhatsNewNotification notification = NotificationBase.ShowNotification<WhatsNewNotification>();
                    if (notification != null)
                    {
                        notification.Title = ModBase.Instance.Name;
                        notification.DSAButton.eventClicked += (component, clickEvent) => DontShowAgain();
                        notification.SetMessages(LastNotifiedVersion, messages);
                    }
                }
            }
        }

        /// <summary>
        /// 'Don't show again' button action.
        /// </summary>
        internal static void DontShowAgain()
        {
            LastNotifiedVersion = AssemblyUtils.CurrentVersion;
            ModBase.Instance?.SaveSettings();
        }
    }
}