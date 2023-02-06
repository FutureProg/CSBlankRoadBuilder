using BlankRoadBuilder.Util.Markings;

using ColossalFramework.UI;

namespace BlankRoadBuilder.UI.Options;
internal partial class FillerOptions : MarkingsOptions
{
	public override string TabName => "Custom Filler Settings";

	public FillerOptions(MarkingType markingType, UITabstrip tabStrip, int tabIndex) : base(markingType, tabStrip, tabIndex)
	{
		var ind = 0;
		foreach (var option in MarkingStyleUtil.CustomFillerMarkings[markingType])
		{
			var ctrl = _panel.AddUIComponent<FillerMarkingOptionControl>();
			ctrl.Init(option.Key, option.Value);

			if (ind % 2 == 0)
			{
				ctrl.relativePosition = new UnityEngine.Vector2(Margin, yPos);
			}
			else
			{
				ctrl.relativePosition = new UnityEngine.Vector2(2 * Margin + ctrl.width, yPos);
				yPos += ctrl.height + Margin;
			}

			ind++;
			//	AddFillerOption(option.Key, option.Value);
		}
	}
}

internal partial class LineOptions : MarkingsOptions
{
	public override string TabName => "Custom Line Settings";

	public LineOptions(MarkingType markingType, UITabstrip tabStrip, int tabIndex) : base(markingType, tabStrip, tabIndex)
	{
		var ind = 0;
		foreach (var option in MarkingStyleUtil.CustomLineMarkings[markingType])
		{
			var ctrl = _panel.AddUIComponent<LineMarkingOptionControl>();
			ctrl.Init(option.Key, option.Value);

			if (ind % 2 == 0)
			{
				ctrl.relativePosition = new UnityEngine.Vector2(Margin, yPos);
			}
			else
			{
				ctrl.relativePosition = new UnityEngine.Vector2(2 * Margin + ctrl.width, yPos);
				yPos += ctrl.height + Margin;
			}

			ind++;
			//AddLineOption(option.Key, option.Value);
		}
	}
}