using System;

namespace BlankRoadBuilder.ThumbnailMaker;

[Flags]
public enum LaneType
{
	[LaneIdentity(0)]
	Empty = 0,

	[LaneIdentity(1, "M", 154, 203, 96)]
	Grass = 1,

	[LaneIdentity(2, "M", 99, 102, 107)]
	Pavement = 2,

	[LaneIdentity(3, "M", 79, 61, 55)]
	Gravel = 4,

	[LaneIdentity(4, "M", 72, 161, 73)]
	Trees = 8,

	[LaneIdentity(5, "C", 66, 132, 212)]
	Car = 16,

	[LaneIdentity(6, "Ped", 92, 97, 102)]
	Pedestrian = 32,

	[LaneIdentity(7, "C", 41, 153, 151)]
	Highway = 64,

	[LaneIdentity(8, "B", 74, 205, 151)]
	Bike = 128,

	[LaneIdentity(9, "Tram", 207, 162, 95)]
	Tram = 256,

	[LaneIdentity(10, "Bus", 170, 62, 48)]
	Bus = 512,

	[LaneIdentity(11, "Trolley", 184, 70, 55)]
	Trolley = 1024,

	[LaneIdentity(12, "Emergency", 222, 75, 109)]
	Emergency = 2048,

	[LaneIdentity(13, "Train", 194, 146, 74)]
	Train = 4096,

	[LaneIdentity(14, "P", 74, 89, 161)]
	Parking = 8192
}