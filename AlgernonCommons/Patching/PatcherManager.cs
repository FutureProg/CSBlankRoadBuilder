// <copyright file="PatcherManager.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Patching
{
    /// <summary>
    /// Harmony patcher manager.
    /// </summary>
    /// <typeparam name="TPatcher">Patcher type.</typeparam>
    public static class PatcherManager<TPatcher>
        where TPatcher : PatcherBase, new()
    {
        /// <summary>
        /// Instance reference.
        /// </summary>
        private static TPatcher s_instance;

        /// <summary>
        /// Gets or sets the mod's unique Harmony identfier.
        /// Must be set before any patching activites.
        /// </summary>
        public static string HarmonyID { get; set; }

        /// <summary>
        /// Gets the active instance reference, creating one if needed.
        /// If no valid HarmonyID has yet been set, then null will be returned.
        /// </summary>
        public static TPatcher Instance
        {
            get
            {
                // Check for valid Harmony ID.
                if (string.IsNullOrEmpty(HarmonyID))
                {
                    Logging.Error("null HarmonyID when attempting to call PatcherManager.Instance");
                    return null;
                }

                // Auto-initializing getter.
                if (s_instance == null)
                {
                    s_instance = new TPatcher
                    {
                        HarmonyID = HarmonyID,
                    };
                }

                return s_instance;
            }
        }
    }
}