using BlankRoadBuilder.Util;
using BlankRoadBuilder.Util.Props;
using BlankRoadBuilder.Util.Props.Templates;

using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlankRoadBuilder.UI.Options;
internal class CustomPropsOptions : OptionsPanelBase
{
	public override string TabName { get; } = "Custom Props";

	private readonly List<CustomPropOptionControl> Controls = new();
	private UILabel blockPanel;

	public CustomPropsOptions(UITabstrip tabStrip, int tabIndex, int tabCount) : base(tabStrip, tabIndex, tabCount)
	{
		var props = Enum.GetValues(typeof(Prop));
		var values = props.Cast<Prop>().ToDictionary(x => x, PropUtil.GetProp);

		yPos += 7;

		var resetButton = _panel.AddUIComponent<SlickButton>();
		resetButton.size = new Vector2(230, 30);
		resetButton.relativePosition = new Vector2(_panel.width - resetButton.width - Margin, yPos + ((30 - 32) / 2));
		resetButton.text = "Reset all prop settings";
		resetButton.SetIcon("I_Undo.png");
		resetButton.eventClicked += ResetButton_eventClicked;

		foreach (var grp in values.GroupBy(x => x.Value.Category).OrderBy(x => x.Key))
		{
			if (grp.Key == PropCategory.None)
				continue;

			var icon = _panel.AddUIComponent<UISprite>();
			icon.atlas = ResourceUtil.GetAtlas(GetIcon(grp.Key));
			icon.spriteName = "normal";
			icon.size = new Vector2(32, 32);
			icon.relativePosition = new Vector2(Margin, yPos + ((30 - 32) / 2));

			var title = _panel.AddUIComponent<UILabel>();
			title.text = grp.Key.ToString().FormatWords().ToUpper();
			title.textScale = 1.4F;
			title.font = UIFonts.SemiBold;
			title.autoSize = true;
			title.relativePosition = new Vector2(32 + (2 * Margin), yPos + ((30 - title.height) / 2));

			yPos += 50;

			var ind = 0;
			var heldPosition = 0F;
			foreach (var option in grp.OrderBy(x => (int)x.Key))
			{
				var ctrl = _panel.AddUIComponent<CustomPropOptionControl>();
				ctrl.Init(option.Key, option.Value);

				Controls.Add(ctrl);

				if (ind % 2 == 0)
				{
					ctrl.relativePosition = new Vector2(40, yPos);
					heldPosition = ctrl.height + Margin;
				}
				else
				{
					ctrl.relativePosition = new Vector2(40 + Margin + ctrl.width, yPos);
					yPos += ctrl.height + Margin;
					heldPosition = 0;
				}

				ind++;
			}

			yPos += heldPosition + Margin;
		}

		GenerateBlockPanel();
	}

	private void GenerateBlockPanel()
	{
		blockPanel = _panel.parent.AddUIComponent<UILabel>();
		blockPanel.text = "This section only works while in the editor or in-game";
		blockPanel.textScale = 1.4F;
		blockPanel.textAlignment = UIHorizontalAlignment.Center;
		blockPanel.textColor = new(229, 155, 49, 255);
		blockPanel.padding = new RectOffset(50, 50, 100, 100);
		blockPanel.wordWrap = true;
		blockPanel.autoSize = false;
		blockPanel.autoHeight = false;
		blockPanel.size = _panel.parent.size;
		blockPanel.relativePosition = Vector3.zero;
		blockPanel.atlas = ResourceUtil.GetAtlas("I_Seperator.png");
		blockPanel.backgroundSprite = "normal";
		blockPanel.color = Color.black;
		blockPanel.BringToFront();

		_panel.parent.eventVisibilityChanged += (s, v) =>
		{
			blockPanel.isVisible = false;//!Utilities.InGame;
		};
	}

	private string GetIcon(PropCategory key)
	{
		return key switch
		{
			PropCategory.TrafficLights => "I_TrafficLights.png",
			PropCategory.Arrows => "I_Arrows.png",
			PropCategory.RoadSigns => "I_TrafficSigns.png",
			PropCategory.SpeedSigns => "I_SpeedSigns.png",
			PropCategory.BridgePillars => "I_Pillar.png",
			PropCategory.Lights => "I_Props.png",
			PropCategory.Stops => "I_BusStop.png",
			PropCategory.Decorations => "I_Decorations.png",
			PropCategory.LevelCrossingBarriers => "I_LevelBarrier.png",
			PropCategory.LaneDecals => "I_Paint.png",
			PropCategory.TramPoles => "I_TramPole.png",
			_ => ""
		};
	}

	private void ResetButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
	{
		PropUtil.ResetSettings();

		foreach (var item in Controls)
		{
			item.UpdateData(PropUtil.GetProp(item.Prop));
		}
	}
}