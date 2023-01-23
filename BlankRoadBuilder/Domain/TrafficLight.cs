using BlankRoadBuilder.Util.Props;

namespace BlankRoadBuilder.Domain;

public struct TrafficLight
{
	public Prop LeftForward { get; set; }
	public Prop RightForward { get; set; }
	public Prop LeftBackward { get; set; }
	public Prop RightBackward { get; set; }

	public float LeftForwardSpace { get; set; }
	public float RightForwardSpace { get; set; }
	public float LeftBackwardSpace { get; set; }
	public float RightBackwardSpace { get; set; }
}