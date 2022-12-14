using AlgernonCommons.UI;

using BlankRoadBuilder.ThumbnailMaker;

using ColossalFramework.Importers;
using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;

namespace BlankRoadBuilder.UI;
public class SlickButton : UIButton
{
	private UIButton? Icon;

	public SlickButton()
	{
		size = new Vector2(220, 36);
		atlas = UITextures.InGameAtlas;
		normalBgSprite = "ButtonWhite";
		hoveredBgSprite = "ButtonWhite";
		focusedBgSprite = "ButtonWhite";
		pressedBgSprite = "ButtonWhitePressed";
		disabledBgSprite = "ButtonWhiteDisabled";
		color = Color.white;
		focusedColor = Color.white;
		textColor = Color.black;
		focusedTextColor = Color.black;
		disabledTextColor = Color.grey;
		hoveredColor = new Color32(197, 216, 235, 255);
		hoveredTextColor = new Color32(30, 37, 44, 255);
		pressedColor = new Color32(39, 130, 224, 255);
		pressedTextColor = new Color32(255, 255, 255, 255);
		textScale = 1F;
		textPadding = new RectOffset(0, 0, 4, 0);
		textVerticalAlignment = UIVerticalAlignment.Middle;
		textHorizontalAlignment = UIHorizontalAlignment.Center;
	}

	public void SetIcon(string? file)
	{
		if (Icon != null)
		{
			Destroy(Icon.gameObject);
		}

		Icon = AddUIComponent<UIButton>();

		Icon.relativePosition = new Vector2(6, 6);
		Icon.size = new Vector2(24, 24);
		Icon.atlas = GetIconAtlas(file);
		Icon.normalBgSprite = "normal";
		Icon.pressedBgSprite = "pressed";
		Icon.tooltip = tooltip;

		Icon.eventClick += IconClicked;
		Icon.eventButtonStateChanged += IconStateChanged;

		textPadding = new RectOffset(36, 0, 4, 0);
	}

	private void IconClicked(UIComponent component, UIMouseEventParameter eventParam)
	{
		OnClick(new UIMouseEventParameter(this));
	}

	private void IconStateChanged(UIComponent component, ButtonState value)
	{
		if (state != value)
			state = value;
	}

	protected override void OnButtonStateChanged(ButtonState value)
	{
		base.OnButtonStateChanged(value);

		if (Icon != null && Icon.state != value)
			Icon.state = value;
	}

	private static UITextureAtlas GetIconAtlas(string? file)
	{
		var iconTexture = new Image(Path.Combine(Path.Combine(BlankRoadBuilderMod.ModFolder, "Icons"), file)).CreateTexture();
		var texture = new Texture2D(iconTexture.width, 2 * iconTexture.height, TextureFormat.ARGB32, mipmap: false, linear: false);
		var pixels = iconTexture.GetPixels32();
		var newPixels = new Color32[pixels.Length * 2];

		for (var i = 0; i < pixels.Length; i++)
		{
			newPixels[i] = new Color32(0, 0, 0, pixels[i].a);
			newPixels[i + pixels.Length] = new Color32(255, 255, 255, pixels[i].a);
		}

		var newAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();

		texture.SetPixels32(newPixels);
		texture.Apply(updateMipmaps: false);

		newAtlas.name = file;
		newAtlas.material = Instantiate(UIView.GetAView().defaultAtlas.material);
		newAtlas.material.mainTexture = texture;
		newAtlas.AddSprite(new UITextureAtlas.SpriteInfo
		{
			name = "normal",
			region = new Rect(0f, 0f, 1f, 0.5f)
		});
		newAtlas.AddSprite(new UITextureAtlas.SpriteInfo
		{
			name = "pressed",
			region = new Rect(0f, 0.5f, 1f, .5f)
		});

		return newAtlas; 
	}
}
