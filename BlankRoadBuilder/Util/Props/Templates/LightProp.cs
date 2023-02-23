using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;

namespace BlankRoadBuilder.Util.Props.Templates;

public class LightProp : PropTemplate
{
	public override PropCategory Category => PropCategory.Lights;

	public LightProp(string propName) : base(propName) { }

	[PropOption("Angle", "Used to compensate for a custom prop's different base angle", 0, 360, 1, "°")]
	public float StartAngle { get => Angle; set => Angle = value; }

	[PropOption("Repeat Distance", "Makes the prop repeat every X meters on the segment", 0, 64, 0.1F, "m")]
	public float RepeatDistance { get => RepeatInterval; set => RepeatInterval = value; }

	[PropOption("Relative Position", "Determines the offset from the default position of the prop", MeasurementUnit = "m")]
	public CustomVector3 RelativePosition { get => Position; set => Position = value; }
}
