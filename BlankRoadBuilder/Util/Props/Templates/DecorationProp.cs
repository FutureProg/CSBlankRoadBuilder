using BlankRoadBuilder.Domain.Options;

namespace BlankRoadBuilder.Util.Props.Templates;

public class DecorationProp : PropTemplate
{
	public override PropCategory Category => PropCategory.Decorations;

	public DecorationProp(string propName, bool isTree = false, bool isBuilding = false) : base(propName, isTree, isBuilding) { }

	[PropOption(0, "Base Angle", "Used to compensate for a custom prop's different base angle", 0, 360, 1, "°")]
	public float StartAngle { get => Angle; set => Angle = value; }

	[PropOption(0, "Use Trees", "Uses a tree instead of a prop")]
	public bool UseTree { get => IsTree; set => IsTree = value; }
}
