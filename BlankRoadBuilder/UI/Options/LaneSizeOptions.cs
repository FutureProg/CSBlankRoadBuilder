using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using ColossalFramework.UI;

using System;

namespace BlankRoadBuilder.UI.Options;
internal class LaneSizeOptions : OptionsPanelBase
{
	public override string TabName { get; } = "Lane Sizes";

	public LaneSizeOptions(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
	{
		var i = 0;
		var y = 0F;

		foreach (LaneType laneType in Enum.GetValues(typeof(LaneType)))
		{
			y = yPos;
			var slider = AddSlider(laneType.ToString(), 0.1F, 10F, 0.1F, ModOptions.LaneSizes[laneType], "m", 350, i % 2 == 0 ? LeftMargin : (350 + (LeftMargin * 2)));

			if (i++ % 2 == 0)
				yPos = y;

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

		diagonalParking.eventValueChanged += (s, v) =>
		{
			ModOptions.LaneSizes.DiagonalParkingSize = v;

			ModOptions.LaneSizes.Save();
		};

		y = yPos;
		var horizontalParking = AddSlider("Horizontal Parking", 0.1F, 10F, 0.1F, ModOptions.LaneSizes.HorizontalParkingSize, "m", 350, i % 2 == 0 ? LeftMargin : (350 + (LeftMargin * 2)));

		if (i++ % 2 == 0)
			yPos = y;

		horizontalParking.eventValueChanged += (s, v) =>
		 {
			 ModOptions.LaneSizes.HorizontalParkingSize = v;

			 ModOptions.LaneSizes.Save();
		 };
	}
}