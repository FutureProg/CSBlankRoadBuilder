using AlgernonCommons.UI;

using ColossalFramework.Importers;
using ColossalFramework.UI;

using System.IO;

using UnityEngine;

namespace BlankRoadBuilder.UI;
public class SlickButton : UIButton
{
	private UIButton? Icon;

	public SlickButton()
	{
		size = new Vector2(160, 30);
		atlas = UITextures.InGameAtlas;
		normalBgSprite = "ButtonWhite";
		hoveredBgSprite = "ButtonWhite";
		focusedBgSprite = "ButtonWhite";
		pressedBgSprite = "ButtonWhite";
		disabledBgSprite = "ButtonWhiteDisabled";
		color = Color.white;
		focusedColor = Color.white;
		textColor = new(50, 50, 50, 255);
		focusedTextColor = new(50, 50, 50, 255);
		disabledTextColor = Color.grey;
		hoveredColor = new Color32(197, 216, 235, 255);
		hoveredTextColor = new Color32(30, 37, 44, 255);
		pressedColor = new Color32(39, 130, 224, 255);
		pressedTextColor = new Color32(255, 255, 255, 255);
		textScale = 0.9F;
		textPadding = new RectOffset(0, 6, 4, 0);
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

		Icon.relativePosition = new Vector2((height - 16) / 2, (height - 16) / 2);
		Icon.size = new Vector2(16, 16);
		Icon.atlas = GetIconAtlas(file);
		Icon.normalBgSprite = "normal";
		Icon.pressedBgSprite = "pressed";
		Icon.tooltip = tooltip;

		Icon.eventClick += IconClicked;
		Icon.eventButtonStateChanged += IconStateChanged;

		textPadding = new RectOffset(24, 6, 4, 0);
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
		if (string.IsNullOrEmpty(BlankRoadBuilderMod.ModFolder))
			return new UITextureAtlas();

		var iconTexture = new Image(Path.Combine(Path.Combine(BlankRoadBuilderMod.ModFolder, "Icons"), file)).CreateTexture();
		var texture = new Texture2D(iconTexture.width, 2 * iconTexture.height, TextureFormat.ARGB32, mipmap: false, linear: false);
		var pixels = iconTexture.GetPixels32();
		var newPixels = new Color32[pixels.Length * 2];

		for (var i = 0; i < pixels.Length; i++)
		{
			newPixels[i] = new Color32(55, 55, 70, pixels[i].a);
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
