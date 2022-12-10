// <copyright file="Translator.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using ColossalFramework;
    using ColossalFramework.Globalization;

    /// <summary>
    /// Language changed event.
    /// </summary>
    /// <param name="languageCode">New language code.</param>
    public delegate void LanguageChangedEventHandler(string languageCode);

    /// <summary>
    /// Handles translations.  Framework by algernon, inspired by BloodyPenguin's original XML framework.
    /// </summary>
    public class Translator
    {
        // Default language.
        private readonly string _defaultLanguage = "en-EN";

        // Language management variables.
        private readonly SortedList<string, Language> _languages;
        private string _systemLangaugeCode;
        private Language _systemLanguage = null;
        private int _currentIndex = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Translator"/> class.
        /// Loads languages from available translation files.
        /// </summary>
        public Translator()
        {
            // Initialise languages list.
            _languages = new SortedList<string, Language>();

            // Load translation files.
            LoadLanguages();

            // Set initial system language reference.
            _systemLangaugeCode = string.Empty;
        }

        /// <summary>
        /// Invoked when the current language changes.
        /// </summary>
        public event LanguageChangedEventHandler OnLanguageChanged;

        /// <summary>
        /// Gets the current zero-based index number of the current language setting.
        /// Less than zero is 'use system setting'.
        /// </summary>
        public int Index => _currentIndex;

        /// <summary>
        /// Gets the current language code stting; if none is set, "default" will be returned instead.
        /// </summary>
        public string CurrentLanguage => _currentIndex < 0 ? "default" : _languages.Values[_currentIndex].Code;

        /// <summary>
        /// Gets the active language code.
        /// </summary>
        public string ActiveLangagueCode => ActiveLangague.Code;

        /// <summary>
        /// Gets an alphabetically-sorted (by code) array of language display names, with an additional "system settings" item as the first item.
        /// </summary>
        /// <returns>Readable language names in alphabetical order by unique name (language code) as string array.</returns>
        public string[] LanguageList
        {
            get
            {
                // Get list of readable language names.
                List<string> readableNames = _languages.Values.Select((language) => language.Name).ToList();

                // Insert system settings item at the start.
                readableNames.Insert(0, Translate("LANGUAGE_GAME"));

                // Return out list as a string array.
                return readableNames.ToArray();
            }
        }

        /// <summary>
        /// Gets the active language.
        /// </summary>
        private Language ActiveLangague
        {
            get
            {
                // Are we using the game language setting?
                if (_currentIndex < 0)
                {
                    // Using game language - initialise system language if we haven't already, or if the system language has changed since last time.
                    if (LocaleManager.exists & (LocaleManager.instance.language != _systemLangaugeCode | _systemLanguage == null))
                    {
                        SetSystemLanguage();
                    }

                    return _systemLanguage;
                }
                else
                {
                    // Using a manuall-set language.
                    return _languages.Values[_currentIndex];
                }
            }
        }

        /// <summary>
        /// Returns the translation for the given key in the current language.
        /// If no translation is available in the given language, fallback languages will be attempted (looking for language codes starting with the same two letters).
        /// If no fallback languages are available, the default langauge will be used instead.
        /// Finally, if all else fails, the raw key will be returned.
        /// </summary>
        /// <param name="key">Translation key to transate.</param>
        /// <returns>Translation, fallback translation, (or key if no translation was available).</returns>
        public string Translate(string key)
        {
            // Check that a valid current language is set.
            Language activeLanguage = ActiveLangague;
            if (activeLanguage != null)
            {
                // Check that the current key is included in the translation.
                if (activeLanguage.TranslationDictionary.ContainsKey(key))
                {
                    // All good!  Return translation.
                    return activeLanguage.TranslationDictionary[key];
                }
                else
                {
                    // Lookup failed - fallack translation.
                    Logging.Message("no translation for language ", activeLanguage.Code, " found for key " + key);
                    return FallbackTranslation(activeLanguage.Code, key);
                }
            }
            else
            {
                Logging.Error("no current language when translating key ", key);
            }

            // If we've made it this far, something went wrong; just return the key.
            return key;
        }

        /// <summary>
        /// Sets the current language to the provided language code.
        /// If the key isn't in the list of loaded translations, then the system default is assigned instead.
        /// </summary>
        /// <param name="languageCode">Language code.</param>
        public void SetLanguage(string languageCode)
        {
            // Null check.
            if (languageCode.IsNullOrWhiteSpace())
            {
                Logging.Error("empty language code passed tp Translator.SetLanguage");
                return;
            }

            Logging.Message("setting language to ", languageCode);

            // Default (game) language.
            if (languageCode == "default")
            {
                SetLanguage(-1);
                return;
            }

            // Try for direct match.
            if (_languages.ContainsKey(languageCode))
            {
                SetLanguage(_languages.IndexOfKey(languageCode));
                return;
            }

            // No direct match found - attempt to find any other suitable translation file (code matches first two letters).
            string shortCode = languageCode.Substring(0, 2);
            foreach (KeyValuePair<string, Language> entry in _languages)
            {
                if (entry.Key.StartsWith(shortCode))
                {
                    // Found an alternative.
                    Logging.Message("using language ", entry.Key, " as replacement for unknown language code ", languageCode);
                    SetLanguage(_languages.IndexOfKey(entry.Key));
                    return;
                }
            }

            // If we got here, no match was found; revert to system language.
            Logging.Message("no suitable translation file for language ", languageCode, " was found; reverting to game default");
            SetLanguage(-1);
        }

        /// <summary>
        /// Sets the current language to the supplied index number.
        /// If index number is invalid (out-of-bounds) then current language is set to -1 (system language setting).
        /// </summary>
        /// <param name="index">1-based language index number (negative values will use system language settings instead).</param>
        public void SetLanguage(int index)
        {
            // Don't do anything if no languages have been loaded.
            if (_languages != null && _languages.Count > 0)
            {
                // Bounds check; if out of bounds, use -1 (system language) instead.
                int newIndex = index >= _languages.Count ? -1 : index;

                // Change the language if what we've done is new.
                if (newIndex != _currentIndex)
                {
                    _currentIndex = newIndex;

                    // Trigger language changed event.
                    OnLanguageChanged?.Invoke(CurrentLanguage);
                }
            }
        }

        /// <summary>
        /// Sets the current system language; sets to null if none.
        /// </summary>
        private void SetSystemLanguage()
        {
            // Make sure Locale Manager is ready before calling it.
            if (LocaleManager.exists)
            {
                // Try to set our system language from system settings.
                try
                {
                    // Get new locale id.
                    string newLanguageCode = LocaleManager.instance.language;

                    // If we've already been set to this locale, do nothing.
                    if (_systemLanguage != null & _systemLangaugeCode == newLanguageCode)
                    {
                        return;
                    }

                    // Set the new system language,
                    Logging.Message("game language is ", newLanguageCode);
                    _systemLangaugeCode = newLanguageCode;
                    _systemLanguage = FindLanguage(newLanguageCode);

                    // If we're using system language, invoke the language changed event.
                    if (_currentIndex < 0)
                    {
                        OnLanguageChanged?.Invoke(_systemLangaugeCode);
                    }

                    // All done.
                    return;
                }
                catch (Exception e)
                {
                    // Don't really care.
                    Logging.LogException(e, "exception setting system language");
                }
            }

            // If we made it here, there's no valid system language.
            _systemLanguage = null;
        }

        /// <summary>
        /// Attempts to find the most appropriate translation file for the specified language code.
        /// An exact match is attempted first; then a match with the first available language with the same two intial characters.
        /// e.g. 'zh' will match to 'zh', 'zh-CN' or 'zh-TW' (in that order), or 'zh-CN' will match to 'zh-CN', 'zh' or 'zh-TW' (in that order).
        /// If no match is made,the default language will be returned.
        /// </summary>
        /// <param name="languageCode">Language code to match.</param>
        /// <returns>Matched language code correspondign to a loaded translation file.</returns>
        private Language FindLanguage(string languageCode)
        {
            // First attempt to find the language code as-is.
            if (_languages.TryGetValue(languageCode, out Language language))
            {
                return language;
            }

            // If that fails, take the first two characters of the provided code and match with the first language code we have starting with those two letters.
            // This will automatically prioritise any translations with only two letters (e.g. 'en' takes priority over 'en-US'),
            KeyValuePair<string, Language> firstMatch = _languages.FirstOrDefault(x => x.Key.StartsWith(languageCode.Substring(0, 2)));
            if (!string.IsNullOrEmpty(firstMatch.Key))
            {
                // Found one - return translation.
                Logging.KeyMessage("using translation file ", firstMatch.Key, " for language ", languageCode);
                return firstMatch.Value;
            }

            // Fall back to default language.
            Logging.Error("no translation file found for language ", languageCode, "; reverting to ", _defaultLanguage);
            return _languages[_defaultLanguage];
        }

        /// <summary>
        /// Attempts to find a fallback language translation after the primary one fails (for whatever reason).
        /// </summary>
        /// <param name="attemptedLanguage">Language code that was previously attempted.</param>
        /// <param name="key">Translation key being attempted.</param>
        /// <returns>Fallback translation if successful, or raw key if failed.  </returns>
        private string FallbackTranslation(string attemptedLanguage, string key)
        {
            try
            {
                // Attempt to find any other suitable translation file (code matches first two letters).
                string shortCode = attemptedLanguage.Substring(0, 2);
                foreach (KeyValuePair<string, Language> entry in _languages)
                {
                    if (entry.Key.StartsWith(shortCode) && entry.Value.TranslationDictionary.TryGetValue(key, out string result))
                    {
                        // Found an alternative.
                        return result;
                    }
                }

                // No alternative was found - return default language.
                return _languages[_defaultLanguage].TranslationDictionary[key];
            }
            catch (Exception e)
            {
                // Don't care.  Just log the exception, as we really should have a default language.
                Logging.LogException(e, "exception attempting fallback translation");
            }

            // At this point we've failed; just return the key.
            return key;
        }

        /// <summary>
        /// Loads languages from CSV files.
        /// </summary>
        private void LoadLanguages()
        {
            // Clear existing dictionary.
            _languages.Clear();

            // Get the current assembly path and append our locale directory name.
            string assemblyPath = AssemblyUtils.AssemblyPath;
            if (assemblyPath.IsNullOrWhiteSpace())
            {
                Logging.Error("assembly path was empty");
                return;
            }

            string translationsPath = Path.Combine(assemblyPath, "Translations");

            // Ensure that the directory exists before proceeding.
            if (!Directory.Exists(translationsPath))
            {
                Logging.Error("translations directory not found");
                return;
            }

            // Load translation files in top directory.
            LoadLanguages(Directory.GetFiles(translationsPath, "*.csv", SearchOption.TopDirectoryOnly), true);

            // Load translation files in lower directories, but only add entries if there's a corresponding top-level translation..
            foreach (string directory in Directory.GetDirectories(translationsPath))
            {
                LoadLanguages(Directory.GetFiles(directory, "*.csv", SearchOption.AllDirectories), false);
            }
        }

        /// <summary>
        /// Loads languages from CSV files.
        /// </summary>
        /// <param name="translationFiles">List of files to translate.</param>
        /// <param name="createNew">True if new translations can be created from this list of files, false to only add to new translations.</param>
        private void LoadLanguages(string[] translationFiles, bool createNew)
        {
            // Load each file and attempt to deserialise as a translation file.
            foreach (string translationFile in translationFiles)
            {
                // Read file.
                try
                {
                    FileStream fileStream = new FileStream(translationFile, FileMode.Open, FileAccess.Read);
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        // Language code is filename.
                        string languageCode = Path.GetFileNameWithoutExtension(translationFile);

                        // Check for existing entry for this language.
                        if (!_languages.TryGetValue(languageCode, out Language thisLanguage))
                        {
                            // No existing language - create a new language instance for this file.
                            thisLanguage = new Language
                            {
                                Code = languageCode,
                            };
                        }

                        // Entry counter.
                        int addedEntries = 0;

                        // Parsing fields.
                        StringBuilder builder = new StringBuilder();
                        string key = null;
                        bool quoting = false, parseKey = true;

                        // Iterate through each line of file.
                        string line = reader.ReadLine();
                        while (line != null)
                        {
                            // Iterate through each character in line.
                            for (int i = 0; i < line.Length; ++i)
                            {
                                // Local reference.
                                char thisChar = line[i];

                                // Are we parsing quoted text?
                                if (quoting)
                                {
                                    // Is this character a quote?
                                    if (thisChar == '"')
                                    {
                                        // Is this a double quote?
                                        int j = i + 1;
                                        if (j < line.Length && line[j] == '"')
                                        {
                                            // Yes - append single quote to output and continue.
                                            i = j;
                                            builder.Append('"');
                                            continue;
                                        }

                                        // It's a single quote - stop quoting here.
                                        quoting = false;

                                        // If we're parsing a value, this is also the end of parsing this line (discard everything else).
                                        if (!parseKey)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // Not a closing quote - just append character to our parsed value.
                                        builder.Append(thisChar);
                                    }
                                }
                                else
                                {
                                    // Not parsing quoted text - is this a comma?
                                    if (thisChar == ',')
                                    {
                                        // Comma - if we're parsing a value, this is also the end of parsing this line (discard everything else).
                                        if (!parseKey)
                                        {
                                            break;
                                        }

                                        // Otherwise, what we've parsed is the key - store value and reset the builder.
                                        parseKey = false;
                                        key = builder.ToString();
                                        builder.Length = 0;
                                    }
                                    else if (thisChar == '"' & builder.Length == 0)
                                    {
                                        // If this is a quotation mark at the start of a field (immediately after comma), then we start parsing this as quoted text.
                                        quoting = true;
                                    }
                                    else
                                    {
                                        // Otherwise, just append character to our parsed string.
                                        builder.Append(thisChar);
                                    }
                                }
                            }

                            // Finished looping through chars - are we still parsing quoted text?
                            if (quoting)
                            {
                                // Yes; continue, after adding a newline.
                                builder.AppendLine();
                                goto NextLine;
                            }

                            // Was key empty?
                            if (key.IsNullOrWhiteSpace())
                            {
                                Logging.Error("invalid key in line ", line);
                                goto Reset;
                            }

                            // Did we get two delimited fields (key and value?)
                            if (parseKey | builder.Length == 0)
                            {
                                Logging.Error("no value field found in line ", line);
                                goto Reset;
                            }

                            // Convert value to string and reset builder.
                            string value = builder.ToString();
                            builder.Length = 0;

                            // Check if this entry is the language entry.
                            if (key.Equals(Language.NameKey))
                            {
                                // Language readable name.
                                thisLanguage.Name = value;
                            }
                            else
                            {
                                // Normal entry - check for duplicates.
                                if (!thisLanguage.TranslationDictionary.ContainsKey(key))
                                {
                                    thisLanguage.TranslationDictionary.Add(key, value);
                                    ++addedEntries;
                                }
                                else
                                {
                                    // Ignore duplicates for language name key.
                                    if (key != Language.NameKey)
                                    {
                                        Logging.Error("duplicate translation key ", key, " in file ", translationFile);
                                    }
                                }
                            }

                        Reset:

                            // Reset for next line.
                            parseKey = true;

                        NextLine:

                            // Read next line.
                            line = reader.ReadLine();
                        }

                        // Did we get a valid dictionary from this?
                        if (thisLanguage.Code != null && addedEntries > 0)
                        {
                            Logging.Message("read translation file ", translationFile, " with language ", thisLanguage.Code, " (", thisLanguage.Name, ") with ", addedEntries, " added entries");

                            // Do we have an existing entry for this language?
                            if (!_languages.ContainsKey(thisLanguage.Code))
                            {
                                // No - if we're not creating new language entries, then skip this and go to the next file.
                                if (!createNew)
                                {
                                    continue;
                                }

                                // Add new language here (done here instead of earlier to avoid adding any languages without any valid translations).
                                {
                                    Logging.Message("adding new language entry");

                                    // If we didn't get a readable name, use the key instead.
                                    if (thisLanguage.Name.IsNullOrWhiteSpace())
                                    {
                                        Logging.Error("no language name provided; using language code instead");
                                        thisLanguage.Name = thisLanguage.Code;
                                    }
                                }

                                // Add language.
                                _languages.Add(thisLanguage.Code, thisLanguage);
                            }
                        }
                        else
                        {
                            Logging.Error("file ", translationFile, " did not produce a valid translation dictionary");
                        }
                    }
                }
                catch (Exception e)
                {
                    // Don't let a single exception stop us; keep going through remaining files.
                    Logging.LogException(e, "exception reading translation file ", translationFile);
                }
            }
        }
    }
}
