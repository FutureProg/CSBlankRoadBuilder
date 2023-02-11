using BlankRoadBuilder.Util;

using ColossalFramework.UI;

using UnityEngine;

namespace BlankRoadBuilder.UI;
public class TagButton : UIPanel
{
	private UISprite? Icon;
	private readonly UILabel _label;

	private bool hovered;
	private bool pressed;
	public string text { get => _label.text; set { _label.text = value; size = _label.size; } }

	public bool Selected { get; set; }
	public bool InvertSelected { get; set; }

	public event PropertyChangedEventHandler<bool>? SelectedChanged;

	public TagButton()
	{
		atlas = ResourceUtil.GetAtlas("TagBack.png");
		backgroundSprite = "normal";
		color = Color.white;

		SetIcon("I_Tag.png");

		_label = AddUIComponent<UILabel>();
		_label.padding = new RectOffset(20, 6, 4, 3);
		_label.textScale = 0.8F;
		_label.relativePosition = Vector3.zero;
		_label.autoSize = true;
		_label.verticalAlignment = UIVerticalAlignment.Middle;
	}

	protected override void OnMouseEnter(UIMouseEventParameter p)
	{
		base.OnMouseEnter(p);

		hovered = true;

		Invalidate();
	}

	protected override void OnMouseLeave(UIMouseEventParameter p)
	{
		base.OnMouseLeave(p);

		hovered = false;

		Invalidate();
	}

	protected override void OnMouseDown(UIMouseEventParameter p)
	{
		base.OnMouseDown(p);

		pressed = true;

		Invalidate();
	}

	protected override void OnMouseUp(UIMouseEventParameter p)
	{
		base.OnMouseUp(p);

		pressed = false;

		if (p.buttons.HasFlag(UIMouseButton.Middle))
		{
			Selected = InvertSelected = false;
		}
		else if (p.buttons.HasFlag(UIMouseButton.Right))
		{
			InvertSelected = !InvertSelected;
			Selected = false;
		}
		else
		{
			Selected = !Selected;
			InvertSelected = false;
		}

		Invalidate();

		SelectedChanged?.Invoke(this, Selected);
	}

	public override void Invalidate()
	{
		if (_label != null)
			if (InvertSelected)
			{
				color = new Color32(148, 51, 35, 255);
				_label.textColor = new Color32(255, 255, 255, 255);
			}
			else if (pressed || Selected)
			{
				color = new Color32(39, 130, 224, 255);
				_label.textColor = new Color32(255, 255, 255, 255);
			}
			else if (hovered)
			{
				color = new Color32(197, 216, 235, 255);
				_label.textColor = new Color32(30, 37, 44, 255);
			}
			else
			{
				color = Color.white;
				_label.textColor = new(50, 50, 50, 255);
			}

		if (Icon != null)
			Icon.spriteName = Selected || InvertSelected || pressed ? "pressed" : "normal";

		base.Invalidate();
	}

	public void SetIcon(string? file)
	{
		if (Icon != null)
		{
			Destroy(Icon.gameObject);
		}

		Icon = AddUIComponent<UISprite>();

		Icon.relativePosition = new Vector2((height - 16) / 2, (height - 16) / 2);
		Icon.size = new Vector2(16, 16);
		Icon.atlas = GetIconAtlas(file);
		Icon.spriteName = "normal";
		Icon.tooltip = tooltip;
	}

	protected override void OnSizeChanged()
	{
		base.OnSizeChanged();

		if (Icon != null)
			Icon.relativePosition = new Vector2(2, (height - 16) / 2);
	}

	private static UITextureAtlas GetIconAtlas(string? file)
	{
		var iconTexture = ResourceUtil.GetImage(file)?.CreateTexture();

		if (iconTexture == null)
			return new UITextureAtlas();

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
