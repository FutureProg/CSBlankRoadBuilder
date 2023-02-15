using BlankRoadBuilder.Domain.Options;

namespace BlankRoadBuilder.Util.Props.Templates;
public class TrafficLightProp : PropTemplate
{
	public override PropCategory Category => PropCategory.TrafficLights;

	public TrafficLightProp(string propName) : base(propName) { }

	[PropOption("Base Angle", "Used to compensate for a custom prop's different base angle", 0, 360, 1, "°")]
	public float StartAngle { get => Angle; set => Angle = value; }
}
