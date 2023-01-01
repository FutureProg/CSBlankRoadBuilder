using AlgernonCommons.UI;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util;

using ColossalFramework.Importers;
using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public class RoadBuilderPanel : StandalonePanel
{
	public override float PanelWidth => 600f;
	public override float PanelHeight => (ListRow.DefaultRowHeight * 6) + 40F + 42F + 56F;
	public static string? LastLoadedRoadFileName { get => lastLoadedRoad; }

	protected override float TitleXPos => 9999;
	protected override string PanelTitle => "Road Builder";

	public UIButton ContinueButton => _continueButton;

	private readonly UIList _fileList;
	private readonly SlickButton _continueButton;
	private readonly SlickButton _refreshButton;
	private readonly UITextField _searchTextBox;
	private readonly UICheckBox _hideBuiltCheckBox;

	private ListData? _currentSelection;
	private List<ListData>? listData;

	private static string? lastLoadedRoad;
	private static bool hideBuilt;

	public RoadBuilderPanel()
	{
		color = new Color32(210, 229, 247, 255);

		var topLabel = UILabels.AddLabel(this, (width - 200F) / 2, 13F, PanelTitle, 200F, 1f, UIHorizontalAlignment.Center);

		topLabel.SendToBack();

		_fileList = UIList.AddUIList<ListRow>(this, 0f, 40f + 42F, PanelWidth, PanelHeight - 40f - 42F - 56F, ListRow.DefaultRowHeight);
		_fileList.EventSelectionChanged += _fileList_EventSelectionChanged;
		_fileList.eventDoubleClick += _continueButton_eventClick;

		_continueButton = AddUIComponent<SlickButton>();
		_continueButton.text = "Build";
		_continueButton.size = new Vector2(180, 36);
		_continueButton.relativePosition = new Vector2(width - 180 - 10, height - 46);
		_continueButton.SetIcon("I_Tools.png");
		_continueButton.eventClick += _continueButton_eventClick;

		_refreshButton = AddUIComponent<SlickButton>();
		_refreshButton.text = "Refresh";
		_refreshButton.size = new Vector2(180, 36);
		_refreshButton.relativePosition = new Vector2(10, height - 46);
		_refreshButton.SetIcon("I_Refresh.png");
		_refreshButton.eventClick += _refreshButton_eventClick;

		_hideBuiltCheckBox = UICheckBoxes.AddLabelledCheckBox(this, 0, 0, "Hide generated roads", tooltip: "Only shows road configs that you haven't generated & saved yet");
		_hideBuiltCheckBox.isChecked = hideBuilt;
		_hideBuiltCheckBox.relativePosition = new Vector3(width - _hideBuiltCheckBox.width - 2 * Margin, 52F, 0);
		_hideBuiltCheckBox.eventCheckChanged += _hideBuiltCheckBox_eventCheckChanged;

		_searchTextBox = UITextFields.AddLabelledTextField(this, 10F + 90F, 46F, "Search:", width - 32F - 90F - _hideBuiltCheckBox.width - Margin, 30F, 1.2F, 6);
		_searchTextBox.eventTextChanged += _searchTextBox_eventTextChanged;

		_searchTextBox.Focus();
	}

	private void _hideBuiltCheckBox_eventCheckChanged(UIComponent component, bool value)
	{
		hideBuilt = _hideBuiltCheckBox.isChecked;
		RefreshList(false);
	}

	private void _refreshButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
	{
		_searchTextBox.text = string.Empty;

		RefreshList(true);
	}

	private void _searchTextBox_eventTextChanged(UIComponent component, string value)
	{
		RefreshList(false);
	}

	private void _fileList_EventSelectionChanged(UIComponent component, object value)
	{
		if (value is ListData objVal)
		{
			_currentSelection = objVal;
		}     
    }

	private void _continueButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
	{
		if (_currentSelection?.RoadInfo == null)
		{
			return;
		}

		_fileList.isVisible = false;
		_refreshButton.isVisible = false;
		_searchTextBox.isVisible = false;
		_continueButton.isVisible = false;
		_hideBuiltCheckBox.isVisible = false;

		height = 200;

		StartCoroutine(LoadRoad());
	}

	public System.Collections.IEnumerator LoadRoad()
	{
		lastLoadedRoad = _currentSelection?.FileName;

		var loadingLabel = GenerateLoadingLabel();
		yield return 0;

		foreach (var stateInfo in RoadBuilderUtil.Build(_currentSelection?.RoadInfo))
		{
			if (stateInfo.Exception != null)
			{
				var panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");

				panel.SetMessage("Failed to generate this road", $"{stateInfo.Exception.Message}\r\n\r\n{stateInfo.Exception}", true);

				Destroy(gameObject, 0F);
				yield break;
			}

			loadingLabel.text = stateInfo.Info;
			yield return 0;
		}

		Destroy(gameObject, 0F);
	}

	private UILabel GenerateLoadingLabel()
	{
		var loadingLabel = AddUIComponent<UILabel>();

		loadingLabel.relativePosition = new Vector2(0, height / 2);
		loadingLabel.autoSize = false;
		loadingLabel.textScale = 1.4F;
		loadingLabel.width = PanelWidth - (Margin * 2);
		loadingLabel.height = height - (Margin * 2);
		loadingLabel.textAlignment = UIHorizontalAlignment.Center;
		loadingLabel.text = "Loading... The game may freeze momentarily. Please don't change anything.";

		return loadingLabel;
	}

	public void RefreshList(bool reload)
	{
		if (reload)
			listData = ReadXMLFiles();

		var data = listData.Where(SearchCheck).OrderByDescending(x => x.RoadInfo?.DateCreated).ToList();

		_fileList.Data = new FastList<object>()
		{
			m_buffer = data.ToArray(),
			m_size = data.Count
		};

		if (data.Count > 0)
		{
			var loadedData = lastLoadedRoad == null ? null : data.FirstOrDefault(x => x.FileName?.Equals(lastLoadedRoad,StringComparison.CurrentCultureIgnoreCase) ?? false);

			_fileList.SelectedIndex = loadedData == null ? 0 : data.IndexOf(loadedData);
		}
	}

	private bool SearchCheck(ListData item)
	{
		if (_hideBuiltCheckBox.isChecked && item.assetMatch != null)
			return false;

		if (string.IsNullOrEmpty(_searchTextBox.text))
			return true;

		var split = _searchTextBox.text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

		return (item.RoadInfo?.Name.SearchCheck(_searchTextBox.text) ?? false)
			|| (item.RoadInfo?.Description.SearchCheck(_searchTextBox.text) ?? false)
			|| split.All(x => item.RoadInfo?.Name.SearchCheck(x) ?? false)
			|| split.All(x => item.RoadInfo?.Description.SearchCheck(x) ?? false);
	}

	private List<ListData> ReadXMLFiles()
	{
		var listItems = new List<ListData>();
		var roadDir = Path.Combine(BlankRoadBuilderMod.BuilderFolder, "Roads");

		if (!Directory.Exists(roadDir))
		{
			return listItems;
		}

		foreach (var file in Directory.GetFiles(roadDir, "*.xml", SearchOption.AllDirectories))
		{
			var roadInfo = ThumbnailMakerUtil.GetRoadInfo(file);

			if (roadInfo == null)
			{
				continue;
			}

			listItems.Add(new ListData
			{
				FileName = Path.GetFileNameWithoutExtension(file),
				FilePath = file,
				TextureAtlas = GetThumbnailTextureAtlas(file, roadInfo),
				RoadInfo = roadInfo,
				FileInfo = new FileInfo(file),
				assetMatch = AssetMatchingUtil.GetMatchForRoadConfig(Path.GetFileNameWithoutExtension(file)),
			});
		}

		return listItems;
	}

	public static UITextureAtlas GetThumbnailTextureAtlas(string? file, RoadInfo roadInfo)
	{
		var thumbnailTex = new Image(roadInfo.SmallThumbnail).CreateTexture();
		var newAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
		
		newAtlas.name = Path.GetFileName(file);
		newAtlas.material = Instantiate(UIView.GetAView().defaultAtlas.material);
		newAtlas.material.mainTexture = thumbnailTex;
		newAtlas.AddSprite(new UITextureAtlas.SpriteInfo
		{
			name = "normal",
			texture = thumbnailTex,
			region = new Rect(0f, 0f, 1f, 1f)
		});

		return newAtlas;
	}

	private class ListData
	{
		public string? FileName;
		public string? FilePath;
		public UITextureAtlas? TextureAtlas;
		public AssetMatchingUtil.Asset? assetMatch;
		public RoadInfo? RoadInfo;
		public FileInfo? FileInfo;
	}

	private class ListRow : UIListRow
	{
		public const float ThumbnailWidth = 109;
		public const float ThumbnailHeight = 100;
		public const float DefaultRowHeight = ThumbnailHeight + (Margin * 2);

		public override float RowHeight => DefaultRowHeight;

		private UILabel? _roadNameLabel;
		private UISprite? _thumbnailImage;
		private UISprite? _buildTick;
		private UILabel? _roadDescriptionLabel;
		private UILabel? _roadDateLabel;

		public ListRow()
		{
			SelectedColor = new Color32(48, 165, 242, 255);
		}

		public override void Display(object data, int rowIndex)
		{
			if (data is not ListData listData)
				return;

			_thumbnailImage ??= GetThumbnailIcon();
			_roadNameLabel ??= GetRoadNameLabel();
			_roadDescriptionLabel ??= GetRoadDescriptionLabel();
			_roadDateLabel ??= GetRoadDateLabel();
			_buildTick ??= GetAssetFileStatus();

			_thumbnailImage.atlas = listData.TextureAtlas;
            _thumbnailImage.spriteName = "normal";
            _roadNameLabel.text = listData.RoadInfo?.Name.RegexReplace("^BR[B4] ", "");
            _roadDescriptionLabel.text = listData.RoadInfo?.Description;
            _roadDateLabel.text = listData.FileInfo?.LastWriteTime.ToRelatedString(true);
			_roadDateLabel.relativePosition = new Vector2(width - _roadDateLabel.width - Margin * 2 - 5F, height - _roadDateLabel.height - Margin);
			_buildTick.isVisible = listData.assetMatch != null;

			Deselect(rowIndex);
		}

		private UILabel GetRoadDateLabel()
		{
			var label = AddUIComponent<UILabel>();

			label.autoSize = true;
			label.textScale = 0.8F;
			label.textColor = new Color32(175, 175, 175, 255);

			return label;
		}

		private UILabel GetRoadDescriptionLabel()
		{
			var label = AddUIComponent<UILabel>();
			
			label.autoSize = false;
			label.autoHeight = false;
			label.textScale = 0.8F;			
			label.clipChildren = true;
			label.wordWrap = true;
			label.relativePosition = new Vector2(ThumbnailWidth + Margin * 2, 36F);
			label.width = width - (ThumbnailWidth + Margin * 3) - 10F;
			label.height = DefaultRowHeight - 28F - Margin;
			label.textColor = new Color32(205, 205, 205, 255);

			return label;
		}

		private UISprite GetThumbnailIcon()
		{
			var sprite = AddUIComponent<UISprite>();

			sprite.height = 100;
			sprite.width = 109;
			sprite.relativePosition = new Vector2(Margin, Margin);

			return sprite;
		}

		private UILabel GetRoadNameLabel()
		{
			var label = AddUIComponent<UILabel>();

			label.textScale = 1.2F;
			label.font = UIFonts.SemiBold;
			label.relativePosition = new Vector2(ThumbnailWidth + Margin * 2, 8F);

			return label;
		}

		private UISprite GetAssetFileStatus()
		{
			var sprite = AddUIComponent<UISprite>();
			sprite.size = new Vector2(16, 16);
            sprite.relativePosition = new Vector2(width - sprite.width - Margin * 3, Margin);
			sprite.atlas = UITextures.LoadSprite(Path.Combine(Path.Combine(BlankRoadBuilderMod.ModFolder, "Icons"), "I_BuildTick"));
			sprite.spriteName = "normal";

			return sprite;
        }		
	}
}
