// <copyright file="KeyOnlyBinding.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Keybinding
{
    using ColossalFramework;
    using UnityEngine;

    /// <summary>
    /// Basic keybinding class - keycode only (no modifier keys).
    /// </summary>
    public class KeyOnlyBinding : KeybindingBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyOnlyBinding"/> class.
        /// </summary>
        public KeyOnlyBinding()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyOnlyBinding"/> class.
        /// </summary>
        /// <param name="keyCode">Key code to set.</param>
        public KeyOnlyBinding(KeyCode keyCode)
            : base(keyCode)
        {
        }

        /// <summary>
        /// Encode keybinding as saved input key for UUI.
        /// </summary>
        /// <returns>Saved input key.</returns>
        public override InputKey Encode() => SavedInputKey.Encode(m_key, false, false, false);

        /// <summary>
        /// Sets the keybinding from the provided ColossalFramework InputKey.
        /// </summary>
        /// <param name="inputKey">InputKey to set from.</param>
        public override void SetKey(InputKey inputKey)
        {
            m_key = (KeyCode)(inputKey & 0xFFFFFFF);
        }

        /// <summary>
        /// Checks to see if the designated key is pressed.
        /// </summary>
        /// <returns>True if pressed, false otherwise.</returns>
        public override bool IsPressed() => Input.GetKey(m_key);

        /// <summary>
        /// Checks to see if the designated key is pressed.
        /// </summary>
        /// <param name="e">Event parameter to check.</param>
        /// <returns>True if pressed, false otherwise.</returns>
        public override bool IsPressed(Event e)
        {
            // Only interested in keydown events.
            if (e.type != EventType.KeyDown)
            {
                return false;
            }

            // Check primary key.
            return e.keyCode == m_key;
        }
    }
}
