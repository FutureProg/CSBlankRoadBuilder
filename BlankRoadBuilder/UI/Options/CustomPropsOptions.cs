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
		var props = Enum.GetValues(typeof(Prop));
		var values = props.Cast<Prop>().ToDictionary(x => x, PropUtil.GetProp);

		var resetButton = _panel.AddUIComponent<SlickButton>();
		resetButton.size = new Vector2(230, 30);
		resetButton.relativePosition = new Vector2(_panel.width - resetButton.width, 0);
		resetButton.text = "Reset all General settings";
		resetButton.SetIcon("I_Undo.png");
		resetButton.eventClicked += ResetButton_eventClicked;

		yPos += 7;

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
			foreach (var option in grp.OrderBy(x => x.Key.ToString()))
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