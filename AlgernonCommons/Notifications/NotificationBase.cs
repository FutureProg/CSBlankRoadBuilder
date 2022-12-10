// <copyright file="NotificationBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Notifications
{
    /* Based on macsergey's MessageBox (NodeMarkup/Intersection Marking Tool)
     * Thanks, macsergey!
     * */

    using System;
    using System.Linq;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Base class for displaying modal notification panels.
    /// </summary>
    public abstract class NotificationBase : UIPanel
    {
        // Layout constants.
        private const float Width = 600f;
        private const float Height = 200f;
        private const float TitleBarHeight = 50f;
        private const float ButtonHeight = 45f;
        private const float Padding = 16f;
        private const float ButtonSpacing = 15f;
        private const float MaxContentHeight = 400f;
        private const float ScrollWidth = 12f;

        // Reference constants.
        private const int DefaultButton = 1;

        // Panel components.
        private UILabel _titleLabel;
        private UIDragHandle _titleBar;
        private UIScrollablePanel _mainPanel;
        private UIPanel _buttonPanel;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationBase"/> class.
        /// </summary>
        public NotificationBase()
        {
            // Basic setup.
            autoSize = false;
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = Width;
            height = Height;
            color = new Color32(128, 128, 128, 255);
            atlas = UITextures.InGameAtlas;
            backgroundSprite = "MenuPanel2";

            // Add components.
            AddTitleBar();
            AddMainPanel();
            AddButtonPanel();
            Resize();

            // Event handler for size change.
            _mainPanel.eventSizeChanged += (component, size) => Resize();
        }

        /// <summary>
        /// Gets or sets the notification's title text.
        /// </summary>
        public string Title { get => _titleLabel.text; set => _titleLabel.text = value; }

        /// <summary>
        /// Gets the notification's main (scrollable) panel.
        /// </summary>
        public UIScrollablePanel ScrollableContent => _mainPanel;

        /// <summary>
        /// Gets the current content width.
        /// </summary>
        protected float ContentWidth => ScrollableContent.width - ScrollableContent.autoLayoutPadding.left - ScrollableContent.autoLayoutPadding.right;

        /// <summary>
        /// Show modal notification.
        /// </summary>
        /// <typeparam name="T">Notification type to show.</typeparam>
        /// <returns>New Notification (null if none).</returns>
        public static T ShowNotification<T>()
        where T : NotificationBase
        {
            // Get global view.
            UIView view = UIView.GetAView();
            if (view == null)
            {
                Logging.Error("unable to get UIView at AlgernonCommons.NotificationBase.ShowNotification");
                return null;
            }

            // Create notification.
            GameObject gameObject = new GameObject
            {
                name = "AlgernonCommons.Notification",
            };
            gameObject.transform.parent = view.transform;
            NotificationBase notification = null;
            try
            {
                // Create new gameobject and attach the notification.
                notification = gameObject.AddComponent<T>();

                // Display modal norification.
                UIView.PushModal(notification);
                notification.Show(true);
                notification.Focus();

                // Apply modal view effects.
                if (view.panelsLibraryModalEffect is UIComponent modalEffect)
                {
                    modalEffect.FitTo(null);
                    if (!modalEffect.isVisible || modalEffect.opacity != 1f)
                    {
                        modalEffect.Show(false);
                        ValueAnimator.Animate("ModalEffect67419", (value) => modalEffect.opacity = value, new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut));
                    }
                }

                return notification as T;
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception creating notification type ", typeof(T));

                // Cleanup if possible.
                CloseNotification(notification);

                if (gameObject != null)
                {
                    GameObject.Destroy(gameObject);
                    gameObject = null;
                }

                return null;
            }
        }

        /// <summary>
        ///  Closes the given notification.
        /// </summary>
        /// <param name="notification">Notification instance to close.</param>
        public static void CloseNotification(NotificationBase notification)
        {
            // Null check.
            if (notification == null)
            {
                return;
            }

            // Stop modality.
            UIView.PopModal();

            // Clear modal view effects.
            if (UIView.GetAView()?.panelsLibraryModalEffect is UIComponent modalEffect)
            {
                if (!UIView.HasModalInput())
                {
                    ValueAnimator.Animate("ModalEffect67419", (value) => modalEffect.opacity = value, new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), () => modalEffect.Hide());
                }
                else
                {
                    modalEffect.zOrder = UIView.GetModalComponent().zOrder - 1;
                }
            }

            // Hide notification, clear child event handlers, and destroy game object.
            notification.Hide();
            notification.RemoveEventHandlers();
            Destroy(notification.gameObject);
            Destroy(notification);
        }

        /// <summary>
        /// Closes the notification.
        /// </summary>
        public void Close() => CloseNotification(this);

        /// <summary>
        /// Adds a button to the notification panel's button panel.
        /// </summary>
        /// <param name="buttonNumber">Number of this button (one-based).</param>
        /// <param name="totalButtons">Total number of buttons to add.</param>
        /// <param name="text">Button text.</param>
        /// <param name="action">Action to perform when button pressed.</param>
        /// <returns>New UIButton.</returns>
        protected UIButton AddButton(int buttonNumber, int totalButtons, string text, Action action)
        {
            // Get zero-based button number.
            int zeroedNumber = buttonNumber - 1;

            // Calculate size and position based on this button number and total number of buttons.
            // Width of button is avalable space divided by number of buttons, less spacing at both sides of button.
            float buttonPlaceWidth = this.width / totalButtons;
            float buttonWidth = buttonPlaceWidth - (ButtonSpacing * 2f);
            float buttonXpos = ((buttonNumber - 1) * buttonPlaceWidth) + ButtonSpacing;

            UIButton newButton = UIButtons.AddButton(_buttonPanel, buttonXpos, 0f, text, buttonWidth, ButtonHeight);
            newButton.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => action?.Invoke();
            newButton.horizontalAlignment = UIHorizontalAlignment.Center;
            newButton.verticalAlignment = UIVerticalAlignment.Middle;

            return newButton;
        }

        /// <summary>
        /// Centres the notification on the screen.  Called by the game when the panel size changes.
        /// </summary>
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            if (GetUIView() is UIView uiView)
            {
                relativePosition = new Vector2(Mathf.Floor((uiView.fixedWidth - width) / 2), Mathf.Floor((uiView.fixedHeight - height) / 2));
            }
        }

        /// <summary>
        /// Handles key down events - escape and return.
        /// </summary>
        /// <param name="keyEvent">UI key event.</param>
        protected override void OnKeyDown(UIKeyEventParameter keyEvent)
        {
            // Ensure key hasn't already been used.
            if (!keyEvent.used)
            {
                // Checking for key - escape and enter.
                if (keyEvent.keycode == KeyCode.Escape)
                {
                    // Escape key pressed - use up the event and close the notification.
                    keyEvent.Use();
                    Close();
                }
                else if (keyEvent.keycode == KeyCode.Return)
                {
                    // Enter key pressed - simulate click of first button.
                    keyEvent.Use();
                    if (_buttonPanel.components.OfType<UIButton>().Skip(DefaultButton - 1).FirstOrDefault() is UIButton button)
                    {
                        button.SimulateClick();
                    }
                }
            }
        }

        /// <summary>
        /// Resizes the notification panel to fit all contents.
        /// </summary>
        private void Resize()
        {
            // Main panel height indicator.
            float maximumY = 0f;

            // Set width of each component in main panel to panel width (less padding on either side).
            foreach (UIComponent component in _mainPanel.components)
            {
                component.width = _mainPanel.width - (Padding * 2f);

                // Calculate component bottom relative Y-position.
                float componentBottom = component.relativePosition.y + component.height;
                if (componentBottom > maximumY)
                {
                    // Update panel height indicator.
                    maximumY = componentBottom;
                }
            }

            // Set height.
            _mainPanel.height = Mathf.Min(maximumY, MaxContentHeight);
            height = _titleBar.height + _mainPanel.height + _buttonPanel.height + (Padding * 2f);

            // Position button panel under main panel.
            _buttonPanel.relativePosition = new Vector2(0, _titleBar.height + _mainPanel.height + Padding);
        }

        /// <summary>
        /// Adds the titlebar to the top of the notification.
        /// </summary>
        private void AddTitleBar()
        {
            // Drag handle.
            _titleBar = AddUIComponent<UIDragHandle>();
            _titleBar.relativePosition = Vector2.zero;
            _titleBar.size = new Vector2(Width, TitleBarHeight);

            // Title
            _titleLabel = _titleBar.AddUIComponent<UILabel>();
            _titleLabel.textAlignment = UIHorizontalAlignment.Center;
            _titleLabel.textScale = 1.3f;
            _titleLabel.anchor = UIAnchorStyle.Top;

            // Close button.
            UIButton closeButton = _titleBar.AddUIComponent<UIButton>();
            closeButton.atlas = UITextures.InGameAtlas;
            closeButton.normalBgSprite = "buttonclose";
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.pressedBgSprite = "buttonclosepressed";
            closeButton.size = new Vector2(32f, 32f);
            closeButton.relativePosition = new Vector2(Width - 36f, 4f);

            // Event handler - resize.
            _titleBar.eventSizeChanged += (component, newSize) =>
            {
                _titleLabel.size = newSize;
                _titleLabel.CenterToParent();
            };

            // Event handler - centre title on drag handle when text changes.
            _titleLabel.eventTextChanged += (component, text) => _titleLabel.CenterToParent();

            // Event handler - close button.
            closeButton.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => Close();
        }

        /// <summary>
        /// Adds the main panel to the notification.
        /// </summary>
        private void AddMainPanel()
        {
            // Basic setup.
            _mainPanel = AddUIComponent<UIScrollablePanel>();
            _mainPanel.relativePosition = new Vector2(0, _titleBar.height);
            _mainPanel.minimumSize = new Vector2(Width - ScrollWidth, 5f);
            _mainPanel.maximumSize = new Vector2(Width - ScrollWidth, MaxContentHeight);
            _mainPanel.autoSize = true;
            _mainPanel.autoLayoutStart = LayoutStart.TopLeft;
            _mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            _mainPanel.autoLayoutPadding = new RectOffset((int)Padding, (int)Padding, 0, 0);
            _mainPanel.autoLayout = true;
            _mainPanel.clipChildren = true;
            _mainPanel.builtinKeyNavigation = true;
            _mainPanel.scrollWheelDirection = UIOrientation.Vertical;

            // Add scrollbar.
            UIScrollbars.AddScrollbar(this, _mainPanel);

            // Workaround to prevent scroll movement wrapping.
            _mainPanel.eventSizeChanged += (c, value) =>
            {
                _mainPanel.scrollPadding.top = 1;
            };

            // Event handlers to add/remove event handlers for resizing when child components re resized.
            _mainPanel.eventComponentAdded += (container, child) => AddChildEvents(child);
            _mainPanel.eventComponentRemoved += (container, child) => RemoveChildEvents(child);
        }

        /// <summary>
        ///  Adds the button panel to the notification.
        /// </summary>
        private void AddButtonPanel()
        {
            _buttonPanel = AddUIComponent<UIPanel>();
            _buttonPanel.size = new Vector2(Width, ButtonHeight);
        }

        /// <summary>
        /// Attaches event handlers to child components to ensure that the main panel is resized to fit children when their size changes.
        /// </summary>
        /// <param name="child">Child component.</param>
        private void AddChildEvents(UIComponent child)
        {
            child.eventVisibilityChanged += OnChildVisibilityChanged;
            child.eventSizeChanged += OnChildSizeChanged;
            child.eventPositionChanged += OnChildPositionChanged;
        }

        /// <summary>
        /// Removes event handlers from child components.  Should be called when child components are removed.
        /// </summary>
        /// <param name="child">Child component.</param>
        private void RemoveChildEvents(UIComponent child)
        {
            child.eventVisibilityChanged -= OnChildVisibilityChanged;
            child.eventSizeChanged -= OnChildSizeChanged;
            child.eventPositionChanged -= OnChildPositionChanged;
        }

        /// <summary>
        /// Removes event handlers from all child components of the main panel.
        /// </summary>
        private void RemoveEventHandlers()
        {
            if (_mainPanel != null)
            {
                // Iterate through each child component and remove event handler.
                foreach (UIComponent child in _mainPanel.components)
                {
                    RemoveChildEvents(child);
                }
            }
        }

        /// <summary>
        /// Event handler for child visibility changes.
        /// </summary>
        private void OnChildVisibilityChanged(UIComponent component, bool isVisible) => Resize();

        /// <summary>
        /// Event handler for child size changes.
        /// </summary>
        private void OnChildSizeChanged(UIComponent component, Vector2 newSize) => Resize();

        /// <summary>
        /// Event handler for child position changes.
        /// </summary>
        private void OnChildPositionChanged(UIComponent component, Vector2 position) => Resize();
    }
}