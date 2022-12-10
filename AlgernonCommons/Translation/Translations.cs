// <copyright file="Translations.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Translation
{
    /// <summary>
    /// A basic translation interface.
    /// </summary>
    public static class Translations
    {
        // Instance reference.
        private static Translator _translator;

        /// <summary>
        /// Gets or sets the currently active language code.
        /// </summary>
        public static string CurrentLanguage
        {
            get => Instance.CurrentLanguage;
            set => Instance.SetLanguage(value);
        }

        /// <summary>
        /// Gets the instance's list of available languages as a string array of display names.
        /// Returns an alphabetically-sorted (by unique name) string array of language display names, with an additional "system settings" item as the first item.
        /// Useful for automatically populating drop-down language selection menus; works in conjunction with Index.
        /// </summary>
        public static string[] LanguageList => Instance.LanguageList;

        /// <summary>
        /// Gets or sets the current language index number (equals the index number of the language names list provied by LanguageList).
        /// Useful for easy automatic drop-down language selection menus, working in conjunction with LanguageList:
        /// Set to set the language to the equivalent LanguageList index.
        /// Get to return the LanguageList index of the current languge.
        /// </summary>
        public static int Index
        {
            // Internal index is one less than here.
            // I.e. internal index is -1 for system and 0 for first language, here we want 0 for system and 1 for first language.
            // So we add one when getting and subtract one when setting.
            get => Instance.Index + 1;

            set => Instance.SetLanguage(value - 1);
        }

        /// <summary>
        /// Gets the active translator instance.
        /// If none is currently active, a new one will be instantiated.
        /// </summary>
        /// <returns>Translator instance.</returns>
        private static Translator Instance
        {
            get
            {
                if (_translator == null)
                {
                    _translator = new Translator();
                }

                return _translator;
            }
        }

        /// <summary>
        /// Translates the provided key.
        /// </summary>
        /// <param name="key">Key to translate.</param>
        /// <returns>Translation (or key if translation failed).</returns>
        public static string Translate(string key) => Instance.Translate(key);
    }
}