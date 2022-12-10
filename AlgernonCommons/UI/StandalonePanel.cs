// <copyright file="StandalonePanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.UI
{
    using System;
    using AlgernonCommons;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Standalone in-game panel template.
    /// </summary>
    public abstract class StandalonePanel : UIPanel
    {
        /// <summary>
        /// Layout margin.
        /// </summary>
        public const float Margin = 5f;

        // Layout constants.
        private const float CloseButtonSize = 35f;

        // Panel components.
        private readonly UILabel _titleLabel;
        private UISprite _iconSprite;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandalonePanel"/> class.
        /// </summary>
        public StandalonePanel()
        {
            // Basic behaviour.
            autoLayout = false;
            canFocus = true;
            isInteractive = true;

            // Appearance.
            atlas = UITextures.InGameAtlas;
            backgroundSprite = "UnlockingPanel2";
            opacity = PanelOpacity;

            // Size.
            size = new Vector2(PanelWidth, PanelHeight);

            // Title label.
            _titleLabel = UILabels.AddLabel(this, TitleXPos, 13f, PanelTitle, PanelWidth - TitleXPos - CloseButtonSize, alignment: UIHorizontalAlignment.Center);
            _titleLabel.text = PanelTitle;

            // Drag bar.
            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.width = width;
            dragHandle.height = height;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.target = this;
            dragHandle.SendToBack();

            // Close button.
            UIButton closeButton = AddUIComponent<UIButton>();
            closeButton.relativePosition = new Vector2(width - CloseButtonSize, 2);
            closeButton.atlas = UITextures.InGameAtlas;
            closeButton.normalBgSprite = "buttonclose";
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.pressedBgSprite = "buttonclosepressed";
            closeButton.eventClick += (component, clickEvent) => Close();

            // Default position - centre in screen.
            relativePosition = new Vector2(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));
        }

        /// <summary>
        /// Close event handler delegate.
        /// </summary>
        public delegate void CloseEventHandler();

        /// <summary>
        /// Exception occured event.
        /// </summary>
        public event CloseEventHandler EventClose;

        /// <summary>
        /// Gets the panel width.
        /// </summary>
        public abstract float PanelWidth { get; }

        /// <summary>
        /// Gets the panel height.
        /// </summary>
        public abstract float PanelHeight { get; }

        /// <summary>
        /// Gets the panel opacity.
        /// </summary>
        protected virtual float PanelOpacity => 1f;

        /// <summary>
        /// Gets the panel's title.
        /// </summary>
        protected abstract string PanelTitle { get; }

        /// <summary>
        /// Sets the panel's title text.
        /// </summary>
        protected string TitleText { set => _titleLabel.text = value; }

        /// <summary>
        /// Gets the title label X-position.
        /// </summary>
        protected virtual float TitleXPos => CloseButtonSize;

        /// <summary>
        /// Closes the panel.
        /// </summary>
        public void Close()
        {
            try
            {
                // Perform any pre-close actions.
                if (!PreClose())
                {
                    // Not yet safe to close; do nothing.
                    return;
                }
            }
            catch (Exception e)
            {
                // We don't want any exceptions stopping us from closing the panel.
                Logging.LogException(e, "exception closing StandalonePanel");
            }

            // Invoke close event.
            EventClose?.Invoke();
        }

        /// <summary>
        /// Sets the icon sprite for the decorative icon (top-left).
        /// </summary>
        /// <param name="spriteAtlas">Sprite icon atlas.</param>
        /// <param name="spriteName">Sprite icon name.</param>
        /// <param name="spriteSize">Spirite size (square, default 32).</param>
        protected void SetIcon(UITextureAtlas spriteAtlas, string spriteName, float spriteSize = 32f)
        {
            // Create sprite if it doesn't already exist.
            if (_iconSprite == null)
            {
                // Decorative icon (top-left).
                _iconSprite = AddUIComponent<UISprite>();
            }

            // Update sprite with new values.
            _iconSprite.relativePosition = new Vector2(Margin, Margin);
            _iconSprite.height = spriteSize;
            _iconSprite.width = spriteSize;
            _iconSprite.atlas = spriteAtlas;
            _iconSprite.spriteName = spriteName;
        }

        /// <summary>
        /// Performs any actions required before closing the panel and checks that it's safe to do so.
        /// </summary>
        /// <returns>True if the panel can close now, false otherwise.</returns>
        protected virtual bool PreClose() => true;
    }
}