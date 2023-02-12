using BlankRoadBuilder.Domain.Options;

namespace BlankRoadBuilder.Util.Props.Templates;
public class BridgePillarProp : PropTemplate
{
	public override PropCategory Category => PropCategory.BridgePillars;

	public BridgePillarProp(string propName, bool isTree = false, bool isBuilding = true) : base(propName, isTree, isBuilding) { }
}
