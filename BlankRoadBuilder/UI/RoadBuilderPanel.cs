using AlgernonCommons.UI;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util;

using ColossalFramework.UI;

using ModsCommon;
using ModsCommon.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public class RoadBuilderPanel : UIPanel
{
	public float PanelWidth = 581F;
	public float PanelHeight = 750F;

	private readonly UIScrollbar _scrollBar;
	private readonly SlickButton _continueButton;
	//private readonly SlickButton _refreshButton;
	private readonly SlickButton _clearFiltersButton;
	private readonly StringUITextField _searchTextBox;
	private readonly UIScrollablePanel _scrollPanel;
	private readonly RoadTypeFilterDropDown _roadTypeDropDown;
	private readonly RoadSizeFilterDropDown _roadSizeDropDown;
	private readonly RoadStatusFilterDropDown _roadStatusDropDown;

	private List<RoadConfigControl>? listData;
	private bool refreshPaused = true;
	private readonly List<TagButton> listTags = new List<TagButton>();

	private static string? savedSearch;
	private static RoadTypeFilter savedRoadType;
	private static RoadSizeFilter savedRoadSize;
	private static RoadStatusFilter savedRoadStatus;
	private static List<string>? savedSelectedTags;
	private static List<string>? savedInvertedTags;

	public static string? LastLoadedRoadFileName { get; private set; }

	public event Action? EventClose;

	public RoadBuilderPanel()
	{
		autoLayout = false;
		canFocus = true;
		isInteractive = true;
		size = new Vector2(PanelWidth, PanelHeight);
		atlas = ResourceUtil.GetAtlas("PanelBack.png");
		backgroundSprite = "normal";
		relativePosition = new Vector2(Mathf.Floor((GetUIView().fixedWidth - base.width) / 2f), Mathf.Floor((GetUIView().fixedHeight - base.height) / 2f));

		var uIDragHandle = AddUIComponent<UIDragHandle>();
		uIDragHandle.size = new Vector2(width, 48);
		uIDragHandle.relativePosition = Vector3.zero;
		uIDragHandle.target = this;

		var closeAtlas = ResourceUtil.GetAtlas("I_Close.png", new UITextureAtlas.SpriteInfo[]
		{
			new()
			{
				name = "normal",
				region = new Rect(0f, 0.5f, 0.5f, 0.5f)
			},
			new()
			{
				name = "hovered",
				region = new Rect(0.5f, 0.5f, 0.5f, 0.5f)
			},
			new()
			{
				name = "pressed",
				region = new Rect(0f, 0f, 0.5f, 0.5f)
			},
		});
		var closeButton = AddUIComponent<UIButton>();
		closeButton.size = new Vector2(30, 30);
		closeButton.relativePosition = new Vector2(width - 35f, 5f);
		closeButton.atlas = closeAtlas;
		closeButton.normalBgSprite = "normal";
		closeButton.hoveredBgSprite = "hovered";
		closeButton.pressedBgSprite = "pressed";
		closeButton.eventClick += delegate
		{
			EventClose?.Invoke();
		};

		_scrollPanel = AddUIComponent<UIScrollablePanel>();
		_scrollPanel.autoSize = false;
		_scrollPanel.autoLayout = false;
		_scrollPanel.width = PanelWidth - 10 - 15;
		_scrollPanel.clipChildren = true;
		_scrollPanel.builtinKeyNavigation = true;
		_scrollPanel.scrollWheelDirection = UIOrientation.Vertical;
		_scrollBar = UIScrollbars.AddScrollbar(this, _scrollPanel);
		(_scrollBar.thumbObject as UISlicedSprite)!.atlas =
		(_scrollBar.trackObject as UISlicedSprite)!.atlas = ResourceUtil.GetAtlas("Scrollbar.png", new UITextureAtlas.SpriteInfo[]
		{
			new()
			{
				name = "bar",
				region = new Rect(0F, 0F, 0.5F, 1F)
			},
			new()
			{
				name = "thumb",
				region = new Rect(0.5F, 0F, 0.5F, 1F)
			},
		});
		(_scrollBar.thumbObject as UISlicedSprite)!.spriteName = "thumb";
		(_scrollBar.trackObject as UISlicedSprite)!.spriteName = "bar";
		_scrollBar.thumbObject.eventMouseDown += (s, _) => (s as UISlicedSprite)!.color = new Color32(39, 130, 224, 255);
		_scrollBar.thumbObject.eventMouseUp += (s, _) => (s as UISlicedSprite)!.color = Color.white;

		_continueButton = AddUIComponent<SlickButton>();
		_continueButton.relativePosition = new Vector2(width - _continueButton.width - 10, height - 40);
		_continueButton.text = "Build Road";
		_continueButton.SetIcon("I_Tools.png");
		_continueButton.eventClick += _continueButton_eventClick;

		//_refreshButton = AddUIComponent<SlickButton>();
		//_refreshButton.size = new Vector2(30, 30);
		//_refreshButton.relativePosition = new Vector2(_continueButton.relativePosition.x - 40, height - 40);
		//_refreshButton.SetIcon("I_Refresh.png");
		//_refreshButton.eventClick += _refreshButton_eventClick;

		_clearFiltersButton = AddUIComponent<SlickButton>();
		_clearFiltersButton.relativePosition = new Vector2(10, height - 40);
		_clearFiltersButton.text = "Clear Filters";
		_clearFiltersButton.SetIcon("I_ClearFilter.png");
		_clearFiltersButton.eventClick += _clearFiltersButton_eventClick;

		_searchTextBox = AddUIComponent<StringUITextField>();
		_searchTextBox.SetDefaultStyle();
		_searchTextBox.size = new Vector2(PanelWidth / 2, 22);
		_searchTextBox.relativePosition = new Vector2(10, 104);
		_searchTextBox.textScale = 0.8F;
		_searchTextBox.textColor = Color.black;
		_searchTextBox.padding = new RectOffset(4, 4, 4, 4);
		_searchTextBox.horizontalAlignment = UIHorizontalAlignment.Left;
		_searchTextBox.eventTextChanged += (s, _) => RefreshView();

		_roadTypeDropDown = AddUIComponent<RoadTypeFilterDropDown>();
		_roadTypeDropDown.relativePosition = new Vector3(width - _roadTypeDropDown.width - 10, 48);
		_roadTypeDropDown.eventSelectedIndexChanged += (s, _) => RefreshView();

		_roadSizeDropDown = AddUIComponent<RoadSizeFilterDropDown>();
		_roadSizeDropDown.relativePosition = new Vector3(width - _roadSizeDropDown.width - 10, 76);
		_roadSizeDropDown.eventSelectedIndexChanged += (s, _) => RefreshView();

		_roadStatusDropDown = AddUIComponent<RoadStatusFilterDropDown>();
		_roadStatusDropDown.relativePosition = new Vector3(width - _roadSizeDropDown.width - 10, 104);
		_roadStatusDropDown.eventSelectedIndexChanged += (s, _) => RefreshView();

		AddLabel(_roadTypeDropDown, "Road Type", TextAlignment.Left);
		AddLabel(_roadSizeDropDown, "Road Size", TextAlignment.Left);
		AddLabel(_roadStatusDropDown, "Road Status", TextAlignment.Left);
		AddLabel(_searchTextBox, "Search", TextAlignment.Center);

		ReadXMLFiles();

		LoadSettings();

		refreshPaused = false;

		RefreshView();

		uIDragHandle.BringToFront();
		closeButton.BringToFront();
		_searchTextBox.Focus();
	}

	private void _clearFiltersButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
	{
		refreshPaused = true;
		_searchTextBox.text = string.Empty;
		_roadTypeDropDown.SelectedObject = RoadTypeFilter.AnyRoadType;
		_roadSizeDropDown.SelectedObject = RoadSizeFilter.AnyRoadSize;
		_roadStatusDropDown.SelectedObject = RoadStatusFilter.AnyStatus;

		foreach (var item in listTags)
		{
			item.InvertSelected = item.Selected = false;
			item.Invalidate();
		}

		refreshPaused = false;
		RefreshView();
	}

	private void AddLabel(UIComponent comp, string text, TextAlignment alignment)
	{
		var label = AddUIComponent<UILabel>();
		label.text = text;
		label.textScale = 0.75F;

		switch (alignment)
		{
			case TextAlignment.Left:
				label.relativePosition = new Vector2(comp.relativePosition.x - label.width - 6, comp.relativePosition.y + (comp.height / 2) - (label.height / 2) + 3);
				break;
			case TextAlignment.Center:
				label.relativePosition = new Vector2(comp.relativePosition.x, comp.relativePosition.y - label.height - 3);
				break;
		}
	}

	//private void _refreshButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
	//{
	//	ReadXMLFiles();
	//}

	private void _continueButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
	{
		var road = listData.FirstOrDefault(x => x.Selected);

		if (road?.RoadInfo == null)
		{
			return;
		}

		LastLoadedRoadFileName = road.FileName;
		EventClose?.Invoke();
		GenerationPanel.Create(road);
	}

	private void ReadXMLFiles()
	{
		if (listData != null)
		{
			foreach (var item in listData)
			{
				item.OnDestroy();
			}
		}

		listData = new();
		var roadDir = Path.Combine(BlankRoadBuilderMod.BuilderFolder, "Roads");

		if (!Directory.Exists(roadDir))
		{
			return;
		}

		foreach (var file in Directory.GetFiles(roadDir, "*.xml", SearchOption.AllDirectories))
		{
			var roadInfo = ThumbnailMakerUtil.GetRoadInfo(file);

			if (roadInfo == null)
			{
				continue;
			}

			var ctrl = _scrollPanel.AddUIComponent<RoadConfigControl>();
			ctrl.SetData(roadInfo, file);
			ctrl.eventClick += RoadSelected;
			ctrl.eventDoubleClick += RoadBuild;

			listData.Add(ctrl);
		}

		AddTags();

		if (!string.IsNullOrEmpty(LastLoadedRoadFileName))
		{
			var road = listData.FirstOrDefault(x => x.FileName == LastLoadedRoadFileName);

			if (road != null)
			{
				road.Selected = true;

				_scrollPanel.ScrollIntoView(road);
			}
		}
	}

	private void AddTags()
	{
		var tags = listData.SelectMany(x => x.RoadInfo?.AutoTags.Concat(x.RoadInfo?.Tags)).Distinct((x, y) => x.Equals(y, StringComparison.CurrentCultureIgnoreCase)).ToList();

		var x = 10F;
		var y = 128 + 10F;
		var lastHeight = 0F;

		//foreach (var item in listTags)
		//{
		//	item.OnDestroy();
		//}

		listTags.Clear();

		foreach (var item in tags)
		{
			var ctrl = AddUIComponent<TagButton>();
			ctrl.text = item;

			if (x + ctrl.width + 6 > width)
			{
				x = 10;
				y += ctrl.height + 6;
			}

			ctrl.relativePosition = new Vector2(x, y);
			ctrl.SelectedChanged += (s, _) => RefreshView();

			listTags.Add(ctrl);

			lastHeight = ctrl.height;

			x += ctrl.width + 6;
		}

		if (lastHeight > 0)
		{
			y += lastHeight + 10;
		}

		_scrollPanel.relativePosition = new Vector2(10, y);
		_scrollPanel.height = PanelHeight - y - 48;
	}

	private void RoadBuild(UIComponent component, UIMouseEventParameter eventParam)
	{
		RoadSelected(component, eventParam);
		_continueButton_eventClick(component, eventParam);
	}

	private void RoadSelected(UIComponent component, UIMouseEventParameter eventParam)
	{
		if (listData == null)
		{
			return;
		}

		foreach (var ctrl in listData)
		{
			ctrl.Selected = component == ctrl;
		}
	}

	private void RefreshView()
	{
		if (listData == null || refreshPaused)
		{
			return;
		}

		SaveSettings();

		_scrollBar.value = 0;

		var x = 0F;
		var y = 0F;

		var sortedData = ModOptions.LaneSizes.SortMode switch
		{
			RoadSortMode.RoadName => listData.OrderBy(x => $"{(int)x.RoadInfo.RoadType}{x.RoadInfo.Name.RegexReplace("^RB[RHFP] ", "")?.RegexReplace(@"\d+([.,]\d+)?[um] ", "")}"),
			RoadSortMode.RoadTypeAndSize => listData.OrderBy(x => x.RoadInfo.TotalRoadWidth + (10000 * (int)x.RoadInfo.RoadType)),
			RoadSortMode.DateCreated or _ => listData.OrderByDescending(x => x.RoadInfo.DateCreated)
		};

		foreach (var ctrl in sortedData)
		{
			if (!SearchCheck(ctrl))
			{
				ctrl.isVisible = false;
				ctrl.Selected = false;
				ctrl.relativePosition = Vector2.zero;
			}
			else
			{
				if (x + ctrl.width + 6 > _scrollPanel.width)
				{
					x = 0;
					y += ctrl.height + 10;
				}

				ctrl.relativePosition = new Vector2(x, y);

				x += ctrl.width + 10;

				ctrl.isVisible = true;
			}
		}
	}

	private void SaveSettings()
	{
		savedSearch = _searchTextBox.text;
		savedRoadType = _roadTypeDropDown.SelectedObject;
		savedRoadSize = _roadSizeDropDown.SelectedObject;
		savedRoadStatus = _roadStatusDropDown.SelectedObject;

		savedSelectedTags = new List<string>();
		savedInvertedTags = new List<string>();

		foreach (var item in listTags)
		{
			if (item.Selected)
				savedSelectedTags.Add(item.text);
			if (item.InvertSelected)
				savedInvertedTags.Add(item.text);
		}
	}

	private void LoadSettings()
	{
		_searchTextBox.Value = savedSearch ?? string.Empty;
		_roadTypeDropDown.SelectedObject = savedRoadType;
		_roadSizeDropDown.SelectedObject = savedRoadSize;
		_roadStatusDropDown.SelectedObject = savedRoadStatus;

		if (savedSelectedTags == null || savedInvertedTags == null)
			return;

		foreach (var item in listTags)
		{
			if (savedSelectedTags.Contains(item.text))
			{
				item.Selected = true;
				item.Invalidate();
			}

			if (savedInvertedTags.Contains(item.text))
			{
				item.InvertSelected = true;
				item.Invalidate();
			}
		}
	}

	private bool SearchCheck(RoadConfigControl item)
	{
		if (_roadTypeDropDown.SelectedObject != RoadTypeFilter.AnyRoadType && item.RoadInfo.RoadType != (RoadType)((int)_roadTypeDropDown.SelectedObject - 1))
		{
			return false;
		}

		if (_roadSizeDropDown.SelectedObject != RoadSizeFilter.AnyRoadSize & !Match(item.RoadInfo, _roadSizeDropDown.SelectedObject))
		{
			return false;
		}

		if (_roadStatusDropDown.SelectedObject != RoadStatusFilter.AnyStatus & !Match(item.AssetMatch, _roadStatusDropDown.SelectedObject))
		{
			return false;
		}

		var selectedTags = listTags.Where(x => x.Selected).Select(x => x.text).ToList();
		if (selectedTags.Any(x => !item.RoadInfo.Tags.Concat(item.RoadInfo.AutoTags).Any(y => y.Equals(x, StringComparison.CurrentCultureIgnoreCase))))
		{
			return false;
		}

		var invertedTags = listTags.Where(x => x.InvertSelected).Select(x => x.text).ToList();
		if (invertedTags.Any(x => item.RoadInfo.Tags.Concat(item.RoadInfo.AutoTags).Any(y => y.Equals(x, StringComparison.CurrentCultureIgnoreCase))))
		{
			return false;
		}

		if (string.IsNullOrEmpty(_searchTextBox.text))
		{
			return true;
		}

		var split = _searchTextBox.text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

		return (item.RoadInfo?.Name.SearchCheck(_searchTextBox.text) ?? false)
			|| (item.RoadInfo?.Description.SearchCheck(_searchTextBox.text) ?? false)
			|| split.All(x => item.RoadInfo?.Name.SearchCheck(x) ?? false)
			|| split.All(x => item.RoadInfo?.Description.SearchCheck(x) ?? false);
	}

	private bool Match(RoadInfo road, RoadSizeFilter selectedValue)
	{
		var width = road.TotalRoadWidth;

		return selectedValue switch
		{
			RoadSizeFilter.Tiny => width <= 16F,
			RoadSizeFilter.Small => width is > 16F and <= 24F,
			RoadSizeFilter.Medium => width is > 24F and <= 32F,
			RoadSizeFilter.Large => width is > 32F and <= 48F,
			RoadSizeFilter.VeryLarge => width > 48F,
			_ => true,
		};
	}

	private bool Match(AssetMatchingUtil.Asset? data, RoadStatusFilter selectedValue)
	{

		switch (selectedValue)
		{
			case RoadStatusFilter.UpToDate:
				return data != null && data.DateGenerated >= BlankRoadBuilderMod.CurrentVersionDate;
			case RoadStatusFilter.NeedsUpdating:
				return data != null && data.DateGenerated < BlankRoadBuilderMod.CurrentVersionDate;
			case RoadStatusFilter.Missing:
				return data != null && data.DateGenerated == DateTime.MinValue;
			case RoadStatusFilter.NeverBuilt:
				return data == null;
			case RoadStatusFilter.BuiltBeforeLastMajorUpdate:
				var version = data == null ? null : SingletonMod<BlankRoadBuilderMod>.Instance.Versions.FirstOrDefault(x => x.Date <= data.DateGenerated.Date).Number;
				return version != null && !(version.Major == BlankRoadBuilderMod.ModVersion.Major && version.Minor == BlankRoadBuilderMod.ModVersion.Minor);
		}

		return true;
	}

	private class RoadTypeFilterDropDown : EnumDropDown<RoadTypeFilter> { }
	private class RoadSizeFilterDropDown : EnumDropDown<RoadSizeFilter> { }
	private class RoadStatusFilterDropDown : EnumDropDown<RoadStatusFilter> { }
}
