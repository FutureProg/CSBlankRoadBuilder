using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util;

using ColossalFramework.Importers;
using ColossalFramework.UI;

using ModsCommon;
using ModsCommon.Utilities;

using System;
using System.IO;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.UI;
internal class RoadConfigControl : UISprite
{
	public const float ThumbnailWidth = 109;
	public const float ThumbnailHeight = 100;
	public const float Margin = 10;

	private static readonly UITextureAtlas _bgAtlas;

	public bool Selected
	{
		get => selected; set
		{
			selected = value;

			spriteName = (AssetMatch != null ? AssetMatch.DateGenerated == DateTime.MinValue ? "lost" : AssetMatch.DateGenerated < BlankRoadBuilderMod.CurrentVersionDate ? "outdated" : "built" : "normal")
				+ (selected ? "selected" : "");
		}
	}

	static RoadConfigControl()
	{
		_bgAtlas = ResourceUtil.GetAtlas("ConfigBack.png", new UITextureAtlas.SpriteInfo[]
		{
			new()
			{
				name = "normal",
				region = new Rect(0f, 0.5f, 1/4F, 0.5f)
			},
			new()
			{
				name = "built",
				region = new Rect(1/4F, 0.5f, 1/4F, 0.5f)
			},
			new()
			{
				name = "outdated",
				region = new Rect(2/4F, 0.5f, 1/4F, 0.5f)
			},
			new()
			{
				name = "lost",
				region = new Rect(3/4F, 0.5f, 1/4F, 0.5f)
			},
			new()
			{
				name = "normalselected",
				region = new Rect(0f, 0f, 1/4F, 0.5f)
			},
			new()
			{
				name = "builtselected",
				region = new Rect(1/4F, 0f, 1/4F, 0.5f)
			},
			new()
			{
				name = "outdatedselected",
				region = new Rect(2/4F, 0f, 1/4F, 0.5f)
			},
			new()
			{
				name = "lostselected",
				region = new Rect(3/4F, 0f, 1/4F, 0.5f)
			},
		})!;
	}

	protected override void OnMouseEnter(UIMouseEventParameter p)
	{
		base.OnMouseEnter(p);

		color = new Color32(220, 220, 220, 255);
	}

	protected override void OnMouseLeave(UIMouseEventParameter p)
	{
		base.OnMouseLeave(p);

		color = Color.white;
	}

	public void SetData(RoadInfo roadInfo, string file)
	{
		RoadInfo = roadInfo;
		FileInfo = new FileInfo(file);
		FileName = Path.GetFileNameWithoutExtension(file);
		FilePath = file;
		_textureAtlas = GetThumbnailTextureAtlas(file, roadInfo);
		AssetMatch = AssetMatchingUtil.GetMatchForRoadConfig(FileName);

		size = new Vector2(129, 180);
		clipChildren = true;

		atlas = _bgAtlas;
		Selected = false;

		_thumbnailImage = GetThumbnailIcon();
		_versionLabel = GetVersionLabel();
		_roadNameLabel = GetRoadNameLabel();
		_roadDateLabel = GetRoadDateLabel();

		if (AssetMatch == null)
			_versionLabel.isVisible = false;
		else if (!SingletonMod<BlankRoadBuilderMod>.Instance.Versions.Any(x => x.Date <= AssetMatch.DateGenerated.Date))
			_versionLabel.text = "Road missing";
		else if (AssetMatch.DateGenerated >= BlankRoadBuilderMod.CurrentVersionDate)
			_versionLabel.text = "Up to date";
		else
			_versionLabel.text = "Built with v" + SingletonMod<BlankRoadBuilderMod>.Instance.Versions.FirstOrDefault(x => x.Date <= AssetMatch.DateGenerated.Date).Number.GetString();

		_roadNameLabel.text = RoadInfo.Name.Trim().RegexReplace("^RB\\w ", "")!.Replace(" ", " ");
		_roadDateLabel.text = FileInfo?.LastWriteTime.ToRelatedString(true);
		_roadNameLabel.relativePosition = new Vector2(0, ThumbnailHeight + Margin + 3);
		_roadDateLabel.relativePosition = new Vector2(0, height - (_versionLabel.isVisible ? 95 : 79));
	}

	private UISprite GetThumbnailIcon()
	{
		var sprite = AddUIComponent<UISprite>();

		sprite.atlas = _textureAtlas;
		sprite.spriteName = "normal";
		sprite.height = ThumbnailHeight;
		sprite.width = ThumbnailWidth;
		sprite.relativePosition = new Vector2(Margin, Margin);

		return sprite;
	}

	private UILabel GetVersionLabel()
	{
		var label = AddUIComponent<UILabel>();

		label.font = UIFonts.SemiBold;
		label.textColor = Color.white;
		label.textScale = 0.6F;
		label.padding = new RectOffset(0, 0, 2, 0);
		label.autoSize = false;
		label.autoHeight = false;
		label.textAlignment = UIHorizontalAlignment.Center;
		label.verticalAlignment = UIVerticalAlignment.Middle;
		label.size = new Vector2(width, 15);
		label.relativePosition = new Vector2(0, height - 14);

		return label;
	}

	private UILabel GetRoadNameLabel()
	{
		var label = AddUIComponent<UILabel>();

		label.autoSize = false;
		label.autoHeight = true;
		label.clipChildren = true;
		label.wordWrap = true;
		label.textScale = 0.8F;
		label.font = UIFonts.SemiBold;
		label.size = new Vector2(width, 80);
		label.textAlignment = UIHorizontalAlignment.Center;
		label.verticalAlignment = UIVerticalAlignment.Top;
		label.padding = new RectOffset(4, 4, 4, 4);

		return label;
	}

	private UILabel GetRoadDateLabel()
	{
		var label = AddUIComponent<UILabel>();

		label.autoSize = false;
		label.autoHeight = false;
		label.wordWrap = true;
		label.textScale = 0.55F;
		label.textColor = new Color32(200, 200, 200, 255);
		label.size = new Vector2(width, 80);
		label.textAlignment = UIHorizontalAlignment.Center;
		label.verticalAlignment = UIVerticalAlignment.Bottom;
		label.padding = new RectOffset(3, 3, 3, 3);

		return label;
	}

	public string FileName { get; private set; }
	public string FilePath { get; private set; }
	public RoadInfo RoadInfo { get; private set; }
	public FileInfo FileInfo { get; private set; }
	public AssetMatchingUtil.Asset? AssetMatch { get; private set; }

	private UITextureAtlas _textureAtlas;
	private UILabel _roadNameLabel;
	private UILabel _roadDateLabel;
	private UISprite _thumbnailImage;
	private UILabel _versionLabel;
	private bool selected;

	private static UITextureAtlas GetThumbnailTextureAtlas(string? file, RoadInfo roadInfo)
	{
		var thumbnailTex = new Image(roadInfo.SmallThumbnail).CreateTexture();
		var baseText = ResourceUtil.GetImage("RoadThumbBack.png")!.CreateTexture();

		var pixels = baseText.GetPixels32();
		var thumbPixels = thumbnailTex.GetPixels32();

		for (var i = 0; i < pixels.Length; i++)
		{
			pixels[i] = new Color32(thumbPixels[i].r, thumbPixels[i].g, thumbPixels[i].b, pixels[i].a);
		}

		baseText.SetPixels32(pixels);
		baseText.Apply(updateMipmaps: false);

		return ResourceUtil.GetAtlas(baseText)!;
	}
}
