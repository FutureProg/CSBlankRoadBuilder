using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;

using System.Xml.Serialization;

namespace BlankRoadBuilder.Util.Props.Templates;

public class LevelCrossingProp : PropTemplate
{
	public override PropCategory Category => PropCategory.LevelCrossingBarriers;

	public LevelCrossingProp(string propName, bool isTree = false, bool isBuilding = false) : base(propName, isTree, isBuilding) { }

	[PropOption("Angle", "Used to compensate for a custom prop's different base angle", 0, 360, 1, "°")]
	public float StartAngle { get => Angle; set => Angle = value; }

	[PropOption("Relative Position", "Determines the offset from the default position of the prop", MeasurementUnit = "m")]
	public CustomVector3 RelativePosition { get => Position; set => Position = value; }
}
