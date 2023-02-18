using BlankRoadBuilder.Domain.Options;

namespace BlankRoadBuilder.Util.Props.Templates;
public class BridgePillarProp : PropTemplate
{
	public override PropCategory Category => PropCategory.BridgePillars;

	public BridgePillarProp(string propName) : base(propName) { IsBuilding = true; }

	[PropOption("Pillar Offset", "Used to compensate for a pillar's vertical offset", -100, 100, 1, "m")]
	public float PillarOffset { get; set; }
}
