using ColossalFramework.UI;

namespace BlankRoadBuilder.UI.Options;
internal class IMTOptionsPanel : OptionsPanelBase
{
	public override string TabName { get; } = "IMT Markings";

	public IMTOptionsPanel(UITabstrip tabStrip, int tabIndex, int tabCount) : base(tabStrip, tabIndex, tabCount, 0)
	{
		var _tabStrip = AutoTabstrip.AddTabstrip(_panel, 0f, 0f, _panel.width, _panel.height - 15F, out _, tabHeight: 32f);

		new LineOptions(Util.Markings.MarkingType.IMT, _tabStrip, 0, 2);
		new FillerOptions(Util.Markings.MarkingType.IMT, _tabStrip, 1, 2);

		_tabStrip.selectedIndex = -1;
		_tabStrip.selectedIndex = 0;
	}
}

internal class VanillaOptionsPanel : OptionsPanelBase
{
	public override string TabName { get; } = "Vanilla Markings";

	public VanillaOptionsPanel(UITabstrip tabStrip, int tabIndex, int tabCount) : base(tabStrip, tabIndex, tabCount, 0)
	{
		var _tabStrip = AutoTabstrip.AddTabstrip(_panel, 0f, 0f, _panel.width, _panel.height - 15F, out _, tabHeight: 32f);

		new LineOptions(Util.Markings.MarkingType.AN, _tabStrip, 0, 2);
		new FillerOptions(Util.Markings.MarkingType.AN, _tabStrip, 1, 2);

		_tabStrip.selectedIndex = -1;
		_tabStrip.selectedIndex = 0;
	}
}