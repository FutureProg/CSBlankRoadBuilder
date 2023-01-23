using BlankRoadBuilder.ThumbnailMaker;

using System;

namespace BlankRoadBuilder.Util.Markings;
public struct MarkingPoint : IEquatable<MarkingPoint>
{
	public LaneInfo? LeftLane { get; }
	public LaneInfo? RightLane { get; }
	public float X =>
		LeftLane != null && LeftLane.Type != LaneType.Curb ? (LeftLane.Position + (LeftLane.LaneWidth / 2)) :
		RightLane != null && RightLane.Type != LaneType.Curb ? (RightLane.Position - (RightLane.LaneWidth / 2)) : 0F;

	public MarkingPoint(LaneInfo? leftLane, LaneInfo? rightLane)
	{
		LeftLane = leftLane;
		RightLane = rightLane;
	}

	public bool Equals(MarkingPoint other)
	{
		return other.X == X;
	}

	public override bool Equals(object obj)
	{
		return obj is MarkingPoint p && p.X == X;
	}

	public override int GetHashCode()
	{
		return -1830369473 + X.GetHashCode();
	}

	public static bool operator ==(MarkingPoint left, MarkingPoint right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(MarkingPoint left, MarkingPoint right)
	{
		return !left.Equals(right);
	}
}
