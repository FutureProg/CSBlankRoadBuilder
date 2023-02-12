using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;

using System.Xml.Serialization;

namespace BlankRoadBuilder.Util.Props.Templates;

public class StopProp : PropTemplate
{
	public override PropCategory Category => PropCategory.Stops;

	public StopProp(string propName, bool isTree = false, bool isBuilding = false) : base(propName, isTree, isBuilding) { }

	[PropOption(0, "Base Angle", "Used to compensate for a custom prop's different base angle", 0, 360, 1, "°")]
	public float StartAngle { get => Angle; set => Angle = value; }

	[PropOption(0, "Relative Position", "Determines the offset from the default position of the stop", MeasurementUnit = "m"), XmlIgnore]
	public CustomVector3 RelativePosition { get => Position; set => Position = value; }

	[PropOption(true)]
	public string SavedPosition { get => RelativePosition; set => RelativePosition = value; }


}
