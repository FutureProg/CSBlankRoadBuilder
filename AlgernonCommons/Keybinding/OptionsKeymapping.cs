// <copyright file="OptionsKeymapping.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Keybinding
{
    using AlgernonCommons.Translation;
    using ColossalFramework;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Keycode setting control for mod hotkeys.
    /// </summary>
    public class OptionsKeymapping : UICustomControl
    {
        // Components.
        private readonly UIButton _button;
        private readonly UIPanel _panel;
        private readonly UILabel _label;

        // Private fields.
        private KeybindingBase _binding;
        private bool _isPrimed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsKeymapping"/> class.
        /// </summary>
        public OptionsKeymapping()
        {
            // Get the template from the game and attach it here.
            _panel = component.AttachUIComponent(UITemplateManager.GetAsGameObject("KeyBindingTemplate")) as UIPanel;

            // Find our sub-components.
            _label = _panel.Find<UILabel>("Name");
            _button = _panel.Find<UIButton>("Binding");

            // Attach our event handlers.
            _button.eventKeyDown += (c, keyEvent) => OnKeyDown(keyEvent);
            _button.eventMouseDown += (c, mouseEvent) => OnMouseDown(mouseEvent);
        }

        /// <summary>
        /// Sets the target keybinding instance.
        /// </summary>
        public KeybindingBase Binding
        {
            private get => _binding;

            set
            {
                _binding = value;
                _button.text = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
            }
        }

        /// <summary>
        /// Gets or sets the keybinding control's label text.
        /// </summary>
        public string Label { get => _label.text; set => _label.text = value; }

        /// <summary>
        /// Gets or sets the keybinding control's button text.
        /// </summary>
        public string ButtonLabel { get => _button.text; set => _button.text = value; }

        /// <summary>
        /// Gets the keybinding control's panel.
        /// </summary>
        public UIPanel Panel => _panel;

        /// <summary>
        /// Sets the current keybinding as a ColossalFramework InputKey.
        /// </summary>
        public virtual InputKey InputKey
        {
            set
            {
                // Update key binding.
                KeySetting = value;

                // Update display.
                _button.text = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
            }
        }

        /// <summary>
        /// Gets or sets the current keybinding as a ColossalFramework InputKey.
        /// </summary>
        public virtual InputKey KeySetting
        {
            get => Binding.Encode();

            set
            {
                // Update key binding.
                Binding.SetKey(value);

                // Update display.
                _button.text = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
            }
        }

        /// <summary>
        /// KeyDown event handler to record the new hotkey.
        /// </summary>
        /// <param name="keyEvent">Keypress event parameter.</param>
        public void OnKeyDown(UIKeyEventParameter keyEvent)
        {
            // Only do this if we're primed and the keypress isn't a modifier key.
            if (_isPrimed && !IsModifierKey(keyEvent.keycode))
            {
                // Variables.
                InputKey inputKey;

                // Use the event.
                keyEvent.Use();

                // If escape was entered, we don't change the code.
                if (keyEvent.keycode == KeyCode.Escape)
                {
                    inputKey = KeySetting;
                }
                else
                {
                    // If backspace was pressed, then we blank the input; otherwise, encode the keypress.
                    inputKey = (keyEvent.keycode == KeyCode.Backspace) ? SavedInputKey.Empty : SavedInputKey.Encode(keyEvent.keycode, keyEvent.control, keyEvent.shift, keyEvent.alt);
                }

                // Apply our new key.
                ApplyKey(inputKey);
            }
        }

        /// <summary>
        /// MouseDown event handler to handle mouse clicks; primarily used to prime hotkey entry.
        /// </summary>
        /// <param name="p">Rvent parameter.</param>
        public void OnMouseDown(UIMouseEventParameter p)
        {
            // Use the event.
            p.Use();

            // Check to see if we're already primed for hotkey entry.
            if (_isPrimed)
            {
                // We were already primed; is this a bindable mouse button?
                if (p.buttons == UIMouseButton.Left)
                {
                    // Not a bindable mouse button - set the button text and cancel priming.
                    _button.text = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
                    UIView.PopModal();
                    _isPrimed = false;
                }
                else
                {
                    // Bindable mouse button - do keybinding as if this was a keystroke.
                    KeyCode mouseCode;

                    switch (p.buttons)
                    {
                        // Convert buttons to keycodes (we don't bother with left button as it's non-bindable).
                        case UIMouseButton.Right:
                            mouseCode = KeyCode.Mouse1;
                            break;
                        case UIMouseButton.Middle:
                            mouseCode = KeyCode.Mouse2;
                            break;
                        case UIMouseButton.Special0:
                            mouseCode = KeyCode.Mouse3;
                            break;
                        case UIMouseButton.Special1:
                            mouseCode = KeyCode.Mouse4;
                            break;
                        case UIMouseButton.Special2:
                            mouseCode = KeyCode.Mouse5;
                            break;
                        case UIMouseButton.Special3:
                            mouseCode = KeyCode.Mouse6;
                            break;
                        default:
                            // No valid button pressed: exit without doing anything.
                            return;
                    }

                    // We got a valid mouse button key - apply settings and save.
                    ApplyKey(SavedInputKey.Encode(mouseCode, IsControlDown(), IsShiftDown(), IsAltDown()));
                }
            }
            else
            {
                // We weren't already primed - set our text and focus the button.
                _button.buttonsMask
                    = UIMouseButton.Left
                    | UIMouseButton.Right
                    | UIMouseButton.Middle
                    | UIMouseButton.Special0
                    | UIMouseButton.Special1
                    | UIMouseButton.Special2
                    | UIMouseButton.Special3;
                _button.text = Translations.Translate("PRESS_ANY_KEY");
                _button.Focus();

                // Prime for new keybinding entry.
                _isPrimed = true;
                UIView.PushModal(_button);
            }
        }

        /// <summary>
        /// Applies a valid key to our settings.
        /// </summary>
        /// <param name="key">InputKey to apply.</param>
        private void ApplyKey(InputKey key)
        {
            // Apply key to current settings.
            KeySetting = key;

            // Remove priming.
            UIView.PopModal();
            _isPrimed = false;
        }

        /// <summary>
        /// Checks to see if the given keycode is a modifier key.
        /// </summary>
        /// <param name="keyCode">Keycode to check.</param>
        /// <returns>True if the key is a modifier key, false otherwise.</returns>
        private bool IsModifierKey(KeyCode keyCode)
        {
            return keyCode == KeyCode.LeftControl
                | keyCode == KeyCode.RightControl
                | keyCode == KeyCode.LeftShift
                | keyCode == KeyCode.RightShift
                | keyCode == KeyCode.LeftAlt
                | keyCode == KeyCode.RightAlt
                | keyCode == KeyCode.AltGr;
        }

        /// <summary>
        /// Checks to see if the control key is pressed.
        /// </summary>
        /// <returns>True if the control key is down, false otherwise.</returns>
        private bool IsControlDown()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        /// <summary>
        /// Checks to see if the shift key is pressed.
        /// </summary>
        /// <returns>True if the shift key is down, false otherwise.</returns>
        private bool IsShiftDown()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        /// <summary>
        /// Checks to see if the alt key is pressed.
        /// </summary>
        /// <returns>True if the alt key is down, false otherwise.</returns>
        private bool IsAltDown()
        {
            // Don't worry, Alt.Gr, I still remember you, even if everyone else forgets!
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
        }
    }
}