// <copyright file="Language.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Translation
{
    using System.Collections.Generic;

    /// <summary>
    /// Translation language class.
    /// </summary>
    public class Language
    {
        // Private fields.
        private Dictionary<string, string> _translationDictionary = new Dictionary<string, string>();

        /// <summary>
        /// Gets the translation key that identifies the file's readable language name.
        /// </summary>
        public static string NameKey => "LANGUAGE";

        /// <summary>
        /// Gets or sets the langauage's language code.
        /// </summary>
        public string Code { get; set; } = null;

        /// <summary>
        /// Gets or sets the language's human-readable name (in native language).
        /// </summary>
        public string Name { get; set; } = null;

        /// <summary>
        /// Gets the dictionary of translations for this language.
        /// </summary>
        public Dictionary<string, string> TranslationDictionary => _translationDictionary;
    }
}