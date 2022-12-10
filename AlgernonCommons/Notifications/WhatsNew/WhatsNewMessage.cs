// <copyright file="WhatsNewMessage.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Notifications
{
    using System;

    /// <summary>
    /// "What's new"? version message struct.
    /// </summary>
    public struct WhatsNewMessage
    {
        /// <summary>
        /// The version this message applies to.
        /// </summary>
        public Version Version;

        /// <summary>
        /// Set to true if the provided message strings are translation keys only and need translation before display.
        /// Set to false if the provide message strings should be displayed as-is.
        /// </summary>
        public bool MessagesAreKeys;

        /// <summary>
        /// Array of what's new message strings (each string will be displayed as a separate dot point).
        /// </summary>
        public string[] Messages;
    }
}