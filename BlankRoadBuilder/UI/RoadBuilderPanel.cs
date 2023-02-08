using AlgernonCommons.UI;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util;

using ColossalFramework.UI;

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
	private readonly StringUITextField _searchTextBox;
	private readonly UICheckBox _hideBuiltCheckBox;
	private readonly UICheckBox _hideUpdatedCheckBox;
	private readonly UIScrollablePanel _scrollPanel;
	private readonly RoadTypeFilterDropDown _roadTypeDropDown;
	private readonly RoadSizeFilterDropDown _roadSizeDropDown;

	private List<RoadConfigControl>? listData;
	private readonly List<TagButton> listTags = new List<TagButton>();
	private static bool hideBuilt;
	private static bool hideUpdated;

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
		var uIButton = AddUIComponent<UIButton>();
		uIButton.size = new Vector2(30, 30);
		uIButton.relativePosition = new Vector2(width - 35f, 5f);
		uIButton.atlas = closeAtlas;
		uIButton.normalBgSprite = "normal";
		uIButton.hoveredBgSprite = "hovered";
		uIButton.pressedBgSprite = "pressed";
		uIButton.eventClick += delegate
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

		_continueButton = AddUIComponent<SlickButton>();
		_continueButton.text = "Build Road";
		_continueButton.relativePosition = new Vector2(width - _continueButton.width - 10, height - 40);
		_continueButton.SetIcon("I_Tools.png");
		_continueButton.eventClick += _continueButton_eventClick;

		//_refreshButton = AddUIComponent<SlickButton>();
		//_refreshButton.size = new Vector2(30, 30);
		//_refreshButton.relativePosition = new Vector2(_continueButton.relativePosition.x - 40, height - 40);
		//_refreshButton.SetIcon("I_Refresh.png");
		//_refreshButton.eventClick += _refreshButton_eventClick;

		_hideBuiltCheckBox = UICheckBoxes.AddLabelledCheckBox(this, 0, 0, "Hide generated roads", tooltip: "Only shows road configs that you haven't generated & saved yet");
		_hideBuiltCheckBox.isChecked = hideBuilt;
		_hideBuiltCheckBox.relativePosition = new Vector2(10, height - 15 - _hideBuiltCheckBox.height / 2 - 10);
		_hideBuiltCheckBox.eventCheckChanged += _hideBuiltCheckBox_eventCheckChanged;

		_hideUpdatedCheckBox = UICheckBoxes.AddLabelledCheckBox(this, 0, 0, "Hide updated roads", tooltip: "Only shows road configs that you haven't updated yet");
		_hideUpdatedCheckBox.isChecked = hideUpdated;
		_hideUpdatedCheckBox.relativePosition = new Vector2(30 + _hideBuiltCheckBox.width, height - 15 - _hideUpdatedCheckBox.height / 2 - 10);
		_hideUpdatedCheckBox.eventCheckChanged += _hideUpdatedCheckBox_eventCheckChanged;

		_searchTextBox = AddUIComponent<StringUITextField>();
		_searchTextBox.SetDefaultStyle();
		_searchTextBox.size = new Vector2(PanelWidth / 2, 22);
		_searchTextBox.relativePosition = new Vector2(10, 76);
		_searchTextBox.textScale = 0.8F;
		_searchTextBox.textColor = Color.black;
		_searchTextBox.padding = new RectOffset(4, 4, 4, 4);
		_searchTextBox.horizontalAlignment = UIHorizontalAlignment.Left;
		_searchTextBox.eventTextChanged += (s, _) => RefreshView();

		_roadTypeDropDown = AddUIComponent<RoadTypeFilterDropDown>();
		_roadTypeDropDown.relativePosition = new Vector3(width - _roadTypeDropDown.width - 10, 48);
		_roadTypeDropDown.eventSelectedIndexChanged += (s, _) => RefreshView();
		_roadTypeDropDown.BringToFront();

		_roadSizeDropDown = AddUIComponent<RoadSizeFilterDropDown>();
		_roadSizeDropDown.relativePosition = new Vector3(width - _roadSizeDropDown.width - 10, 76);
		_roadSizeDropDown.eventSelectedIndexChanged += (s, _) => RefreshView();
		_roadSizeDropDown.BringToFront();

		AddLabel(_roadTypeDropDown, "Road Type", TextAlignment.Left);
		AddLabel(_roadSizeDropDown, "Road Size", TextAlignment.Left);
		AddLabel(_searchTextBox, "Search", TextAlignment.Center);

		ReadXMLFiles();

		_searchTextBox.Focus();
	}

	private void AddLabel(UIComponent comp, string text, TextAlignment alignment)
	{
		var label = AddUIComponent<UILabel>();
		label.text = text;
		label.textScale = 0.75F;

		switch (alignment)
		{
			case TextAlignment.Left:
				label.relativePosition = new Vector2(comp.relativePosition.x - label.width - 6, comp.relativePosition.y + comp.height / 2 - label.height / 2 + 3);
				break;
			case TextAlignment.Center:
				label.relativePosition = new Vector2(comp.relativePosition.x, comp.relativePosition.y - label.height - 3);
				break;
		}
	}

	private void _hideBuiltCheckBox_eventCheckChanged(UIComponent component, bool value)
	{
		hideBuilt = _hideBuiltCheckBox.isChecked;
		RefreshView();
	}

	private void _hideUpdatedCheckBox_eventCheckChanged(UIComponent component, bool value)
	{
		hideUpdated = _hideUpdatedCheckBox.isChecked;
		RefreshView();
	}

	private void _refreshButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
	{
		ReadXMLFiles();
	}

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

		RefreshView();
	}

	private void AddTags()
	{
		var tags = listData.SelectMany(x => x.RoadInfo?.AutoTags.Concat(x.RoadInfo?.Tags)).Distinct((x, y) => x.Equals(y, StringComparison.CurrentCultureIgnoreCase)).ToList();

		var x = 10F;
		var y = 100 + 10F;
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
		if (listData == null)
		{
			return;
		}

		_scrollBar.value = 0;

		var x = 0F;
		var y = 0F;

		var sortedData = ModOptions.LaneSizes.SortMode switch
		{
			RoadSortMode.RoadName => listData.OrderBy(x => $"{(int)x.RoadInfo.RoadType}{x.RoadInfo.Name.RegexReplace("^RB[RHFP] ", "")?.RegexReplace(@"\d+([.,]\d+)?[um] ", "")}"),
			RoadSortMode.RoadTypeAndSize => listData.OrderBy(x => x.RoadInfo.TotalRoadWidth + 10000 * (int)x.RoadInfo.RoadType),
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

	private bool SearchCheck(RoadConfigControl item)
	{
		if (_hideBuiltCheckBox.isChecked && item.AssetMatch != null)
		{
			return false;
		}

		if (_hideUpdatedCheckBox.isChecked && item.AssetMatch != null && item.AssetMatch.DateGenerated >= BlankRoadBuilderMod.CurrentVersionDate)
		{
			return false;
		}

		if (_roadTypeDropDown.SelectedObject != RoadTypeFilter.AnyRoadType && item.RoadInfo.RoadType != (RoadType)((int)_roadTypeDropDown.SelectedObject - 1))
		{
			return false;
		}

		if (_roadSizeDropDown.SelectedObject != RoadSizeFilter.AnyRoadSize & !Match(item.RoadInfo, _roadSizeDropDown.SelectedObject))
		{
			return false;
		}

		var selectedTags = listTags.Where(x => x.Selected).Select(x => x.text).ToList();
		if (selectedTags.Any(x => !item.RoadInfo.Tags.Concat(item.RoadInfo.AutoTags).Any(y => y.Equals(x, StringComparison.CurrentCultureIgnoreCase))))
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

		switch (selectedValue)
		{
			case RoadSizeFilter.Tiny:
				return width <= 16F;
			case RoadSizeFilter.Small:
				return width > 16F && width <= 24F;
			case RoadSizeFilter.Medium:
				return width > 24F && width <= 32F;
			case RoadSizeFilter.Large:
				return width > 32F && width <= 48F;
			case RoadSizeFilter.VeryLarge:
				return width > 48F;
			default:
				return true;
		}
	}

	private class RoadTypeFilterDropDown : EnumDropDown<RoadTypeFilter> { }
	private class RoadSizeFilterDropDown : EnumDropDown<RoadSizeFilter> { }
}
