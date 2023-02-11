using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using ColossalFramework.UI;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace BlankRoadBuilder.UI.Options;
internal class LaneSizeOptions : OptionsPanelBase
{
	public override string TabName { get; } = "Lane Sizes";

	private readonly List<Action> ResetActions = new List<Action>();

	public LaneSizeOptions(UITabstrip tabStrip, int tabIndex, int tabCount) : base(tabStrip, tabIndex, tabCount)
	{
		_panel.scrollPadding = new RectOffset(15, 0, 30, 0);

		var i = 0;
		var y = 0F;

		foreach (LaneType laneType in Enum.GetValues(typeof(LaneType)))
		{
			y = yPos;
			var slider = AddSlider(laneType.ToString(), 0.1F, 10F, 0.1F, ModOptions.LaneSizes[laneType], "m", 350, i % 2 == 0 ? LeftMargin : (350 + (LeftMargin * 2)));

			if (i++ % 2 == 0)
				yPos = y;

			ResetActions.Add(() => slider.value = ModOptions.LaneSizes[laneType]);

			slider.eventValueChanged += (s, v) =>
			{
				ModOptions.LaneSizes[laneType] = v;

				ModOptions.LaneSizes.Save();
			};
		}

		y = yPos;
		var diagonalParking = AddSlider("Diagonal Parking", 0.1F, 10F, 0.1F, ModOptions.LaneSizes.DiagonalParkingSize, "m", 350, i % 2 == 0 ? LeftMargin : (350 + (LeftMargin * 2)));

		if (i++ % 2 == 0)
			yPos = y;

		ResetActions.Add(() => diagonalParking.value = ModOptions.LaneSizes.DiagonalParkingSize);

		diagonalParking.eventValueChanged += (s, v) =>
		{
			ModOptions.LaneSizes.DiagonalParkingSize = v;

			ModOptions.LaneSizes.Save();
		};

		y = yPos;
		var horizontalParking = AddSlider("Horizontal Parking", 0.1F, 10F, 0.1F, ModOptions.LaneSizes.HorizontalParkingSize, "m", 350, i % 2 == 0 ? LeftMargin : (350 + (LeftMargin * 2)));

		if (i++ % 2 == 0)
			yPos = y;

		ResetActions.Add(() => horizontalParking.value = ModOptions.LaneSizes.HorizontalParkingSize);

		horizontalParking.eventValueChanged += (s, v) =>
		{
			ModOptions.LaneSizes.HorizontalParkingSize = v;

			ModOptions.LaneSizes.Save();
		};

		var resetButton = _panel.AddUIComponent<SlickButton>();
		resetButton.size = new Vector2(230, 30);
		resetButton.relativePosition = new Vector2(25, yPos + Margin);
		resetButton.text = "Reset all lane widths";
		resetButton.SetIcon("I_Undo.png");
		resetButton.eventClicked += ResetButton_eventClicked;
	}

	private void ResetButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
	{
		ModOptions.LaneSizes.Update(true);

		ModOptions.LaneSizes.Save();

		foreach (var item in ResetActions)
		{
			item();
		}
	}
}