using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;

using System.Xml.Serialization;

namespace BlankRoadBuilder.Util.Props.Templates;

public class LaneDecalProp : PropTemplate
{
	public override PropCategory Category => PropCategory.LaneDecals;

	public LaneDecalProp(string propName, bool isTree = false, bool isBuilding = false) : base(propName, isTree, isBuilding) { }

	[PropOption("Base Angle", "Used to compensate for a custom prop's different base angle", 0, 360, 1, "°")]
	public float StartAngle { get => Angle; set => Angle = value; }

	[PropOption("Repeat Distance", "Makes the prop repeat every X meters on the segment", 0, 64, 0.1F, "m")]
	public float RepeatDistance { get => RepeatInterval; set => RepeatInterval = value; }

	[PropOption("Segment Snapping", "Determines the snapping position of the prop based on the direction of the lane")]
	public PropSegmentSnapping SegmentSnapping { get => (PropSegmentSnapping)(int)(SegmentOffset * 100F); set => RepeatInterval = (int)value / 100F; }

	[PropOption("Relative Position", "Determines the offset from the default position of the stop", MeasurementUnit = "m"), XmlIgnore]
	public CustomVector3 RelativePosition { get => Position; set => Position = value; }

	[PropOption(true)]
	public string SavedPosition { get => RelativePosition; set => RelativePosition = value; }
}
