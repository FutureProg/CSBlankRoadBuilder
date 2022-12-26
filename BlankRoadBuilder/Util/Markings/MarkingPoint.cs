using BlankRoadBuilder.ThumbnailMaker;

using System;

namespace BlankRoadBuilder.Util.Markings;
public struct MarkingPoint : IEquatable<MarkingPoint>
{
	public float X => LeftLane != null ? (LeftLane.Position + LeftLane.Width / 2) : RightLane != null ? (RightLane.Position - RightLane.Width / 2) : 0F;
	public LaneInfo? LeftLane { get; }
	public LaneInfo? RightLane { get; }

	public MarkingPoint(LaneInfo? leftLane, LaneInfo? rightLane)
	{
		LeftLane = leftLane;
		RightLane = rightLane;
	}

	public bool Equals(MarkingPoint other) => other.X == X;
}
