using BlankRoadBuilder.Util.Markings;

using ColossalFramework.UI;

namespace BlankRoadBuilder.UI.Options;
internal partial class FillerOptions : OptionsPanelBase
{
	public override string TabName => "Custom Filler Settings";

	public FillerOptions(MarkingType markingType, UITabstrip tabStrip, int tabIndex, int tabCount) : base(tabStrip, tabIndex, tabCount)
	{
		var ind = 0;
		foreach (var option in MarkingStyleUtil.CustomFillerMarkings[markingType])
		{
			var ctrl = _panel.AddUIComponent<FillerMarkingOptionControl>();
			ctrl.Init(markingType, option.Key, option.Value);

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
		}
	}
}

internal partial class LineOptions : OptionsPanelBase
{
	public override string TabName => "Custom Line Settings";

	public LineOptions(MarkingType markingType, UITabstrip tabStrip, int tabIndex, int tabCount) : base(tabStrip, tabIndex, tabCount)
	{
		var ind = 0;
		foreach (var option in MarkingStyleUtil.CustomLineMarkings[markingType])
		{
			var ctrl = _panel.AddUIComponent<LineMarkingOptionControl>();
			ctrl.Init(markingType, option.Key, option.Value);

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
		}
	}
}