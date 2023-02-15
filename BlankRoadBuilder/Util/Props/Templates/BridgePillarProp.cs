using BlankRoadBuilder.Domain.Options;

namespace BlankRoadBuilder.Util.Props.Templates;
public class BridgePillarProp : PropTemplate
{
	public override PropCategory Category => PropCategory.BridgePillars;

	public BridgePillarProp(string propName) : base(propName) { IsBuilding = true; }
}
