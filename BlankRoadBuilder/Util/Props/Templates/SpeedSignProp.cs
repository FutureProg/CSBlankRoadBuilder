using BlankRoadBuilder.Domain.Options;

namespace BlankRoadBuilder.Util.Props.Templates;
public class SpeedSignProp : PropTemplate
{
	public override PropCategory Category => PropCategory.SpeedSigns;

	public SpeedSignProp(string propName, bool isTree = false, bool isBuilding = false) : base(propName, isTree, isBuilding) { }

	[PropOption(0, "Base Angle", "Used to compensate for a custom prop's different base angle", 0, 360, 1, "°")]
	public float StartAngle { get => Angle; set => Angle = value; }
}
