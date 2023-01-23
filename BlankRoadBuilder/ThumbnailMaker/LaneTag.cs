using System;

namespace BlankRoadBuilder.ThumbnailMaker;

[Flags]
public enum LaneTag
{
	Asphalt = 1, // Used by default to specify the lane is on the asphalt part of the road
				 //Ghost = 2,
	Sidewalk = 4,
	Placeholder = 8,
	StackedLane = 16,
	StoppableVehicleOnLeft = 32,
	StoppableVehicleOnRight = 64,
	CenterMedian = 128,
	//SecondaryCenterMedian = 256,
	//StackedLane = 512,
	WirePoleLane = 1024,
	Damage = 2048,
}