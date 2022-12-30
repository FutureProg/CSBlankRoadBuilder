using AlgernonCommons.UI;

using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using ColossalFramework.UI;

using System;
using System.Reflection;

namespace BlankRoadBuilder.UI.Options;
internal class IMTOptions : OptionsPanelBase
{
	public override string TabName { get; } = "IMT Markings";

	public IMTOptions(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex, 0)
	{
		var _tabStrip = AutoTabstrip.AddTabstrip(_panel, 0f, 0f, _panel.width, _panel.height - 15F, out _, tabHeight: 32f);

		new LineOptions(Util.Markings.MarkingType.IMT, _tabStrip, 0);
		new FillerOptions(Util.Markings.MarkingType.IMT, _tabStrip, 1);

		_tabStrip.selectedIndex = -1;
		_tabStrip.selectedIndex = 0;
	}
}

internal class ANOptions : OptionsPanelBase
{
	public override string TabName { get; } = "AN Markings";

	public ANOptions(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex, 0)
	{
		var _tabStrip = AutoTabstrip.AddTabstrip(_panel, 0f, 0f, _panel.width, _panel.height - 15F, out _, tabHeight: 32f);

		new LineOptions(Util.Markings.MarkingType.AN, _tabStrip, 0);
		new FillerOptions(Util.Markings.MarkingType.AN, _tabStrip, 1);

		_tabStrip.selectedIndex = -1;
		_tabStrip.selectedIndex = 0;
	}
}