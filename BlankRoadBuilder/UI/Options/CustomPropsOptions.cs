using AlgernonCommons.UI;

using BlankRoadBuilder.Util;
using BlankRoadBuilder.Util.Props.Templates;

using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.UI.Options;
internal class CustomPropsOptions : OptionsPanelBase
{
	public override string TabName { get; } = "Custom Props";

	private readonly List<CustomPropOptionControl> Controls = new();

	public CustomPropsOptions(UITabstrip tabStrip, int tabIndex, int tabCount) : base(tabStrip, tabIndex, tabCount)
	{
		var first = true;
		var props = Enum.GetValues(typeof(Prop));
		var values = props.Cast<Prop>().ToDictionary(x => x, PropUtil.GetProp);

		foreach (var grp in values.GroupBy(x => x.Value.Category))
		{
			if (first)
			{
				first = false;
			}
			else
			{
				yPos += 7;
			}

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
			foreach (var option in grp)
			{
				var ctrl = _panel.AddUIComponent<CustomPropOptionControl>();
				ctrl.Init(option.Key, option.Value);

				Controls.Add(ctrl);

				if (ind % 2 == 0)
				{
					ctrl.relativePosition = new Vector2(40, yPos);
				}
				else
				{
					ctrl.relativePosition = new Vector2(40 + Margin + ctrl.width, yPos);
					yPos += ctrl.height + Margin;
				}

				ind++;
			}
		}

		var resetButton = _panel.AddUIComponent<SlickButton>();
		resetButton.size = new Vector2(230, 30);
		resetButton.relativePosition = new Vector2(32 + (2 * Margin), yPos - 50);
		resetButton.text = "Reset all General settings";
		resetButton.SetIcon("I_Undo.png");
		resetButton.eventClicked += ResetButton_eventClicked;
	}

	private string GetIcon(PropCategory key)
	{
		return key switch
		{
			PropCategory.TrafficLights => "I_TrafficLights.png",
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