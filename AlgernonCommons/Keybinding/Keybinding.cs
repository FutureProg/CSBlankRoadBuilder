// <copyright file="Keybinding.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Keybinding
{
    using System.Xml.Serialization;
    using ColossalFramework;
    using UnityEngine;

    /// <summary>
    /// Basic keybinding class - code and modifiers.
    /// </summary>
    public class Keybinding : KeybindingBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Keybinding"/> class.
        /// </summary>
        public Keybinding()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Keybinding"/> class.
        /// </summary>
        /// <param name="keyCode">Key code.</param>
        /// <param name="control">Control modifier key status.</param>
        /// <param name="shift">Shift modifier key status.</param>
        /// <param name="alt">Alt modifier key status.</param>
        public Keybinding(KeyCode keyCode, bool control, bool shift, bool alt)
        {
            this.m_key = keyCode;
            this.Control = control;
            this.Shift = shift;
            this.Alt = alt;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control key is pressed for this keybinding.
        /// </summary>
        [XmlAttribute("Control")]
        public bool Control { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the shift key is pressed for this keybinding.
        /// </summary>
        [XmlAttribute("Shift")]
        public bool Shift { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the alt key is pressed for this keybinding.
        /// </summary>
        [XmlAttribute("Alt")]
        public bool Alt { get; set; }

        /// <summary>
        /// Encode keybinding as saved input key for UUI.
        /// </summary>
        /// <returns>Saved input key.</returns>
        public override InputKey Encode() => SavedInputKey.Encode(m_key, Control, Shift, Alt);

        /// <summary>
        /// Sets the keybinding from the provided ColossalFramework InputKey.
        /// </summary>
        /// <param name="inputKey">InputKey to set from.</param>
        public override void SetKey(InputKey inputKey)
        {
            m_key = (KeyCode)(inputKey & 0xFFFFFFF);
            Control = (inputKey & 0x40000000) != 0;
            Shift = (inputKey & 0x20000000) != 0;
            Alt = (inputKey & 0x10000000) != 0;
        }

        /// <summary>
        /// Checks to see if the designated key is pressed.
        /// </summary>
        /// <returns>True if pressed, false otherwise.</returns>
        public override bool IsPressed()
        {
            // Check primary key.
            if (!Input.GetKey(m_key))
            {
                return false;
            }

            // Check modifier keys,
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) != Control)
            {
                return false;
            }

            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) != Shift)
            {
                return false;
            }

            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr)) != Alt)
            {
                return false;
            }

            // If we got here, all checks have been passed.
            return true;
        }

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
            if (e.keyCode != m_key)
            {
                return false;
            }

            // Check modifier keys.
            if ((e.modifiers & EventModifiers.Control) != 0 != Control)
            {
                return false;
            }

            if ((e.modifiers & EventModifiers.Shift) != 0 != Shift)
            {
                return false;
            }

            if ((e.modifiers & EventModifiers.Alt) != 0 != Alt)
            {
                return false;
            }

            return true;
        }
    }
}