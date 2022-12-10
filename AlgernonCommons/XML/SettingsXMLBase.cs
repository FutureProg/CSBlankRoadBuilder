// <copyright file="SettingsXMLBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.XML
{
    using System.Xml.Serialization;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;

    /// <summary>
    /// XML settings file base class.
    /// </summary>
    public abstract class SettingsXMLBase
    {
        /// <summary>
        /// Gets or sets the user language selection.
        /// </summary>
        [XmlElement("Language")]
        public string Language { get => Translations.CurrentLanguage; set => Translations.CurrentLanguage = value; }

        /// <summary>
        /// Gets or sets the "What's new" notification last notified version.
        /// </summary>
        [XmlElement("WhatsNewVersion")]
        public string WhatsNewVersion { get => WhatsNew.LastNotifiedVersionString; set => WhatsNew.LastNotifiedVersionString = value; }
    }
}