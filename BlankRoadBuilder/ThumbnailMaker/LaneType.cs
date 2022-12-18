using System;

namespace BlankRoadBuilder.ThumbnailMaker;

[Flags]
public enum LaneType
{
	[StyleIdentity(0, 50, 50, 50)]
	Empty = 0,

	[StyleIdentity(1, "M", 130, 204, 96)]
	Filler = 1,

	[StyleIdentity(2, 200, 200, 200)]
	Curb = 2,

	[StyleIdentity(3, "Ped", 92, 97, 102)]
	Pedestrian = 4,

	[StyleIdentity(4, "B", 74, 205, 151)]
	Bike = 16,

	[StyleIdentity(5, "C", 66, 132, 212)]
	Car = 32,

	[StyleIdentity(6, "T", 230, 210, 122)]
	Tram = 64,

	[StyleIdentity(7, "Bus", 170, 62, 48)]
	Bus = 128,

	[StyleIdentity(8, "TBus", 184, 70, 55)]
	Trolley = 256,

	[StyleIdentity(9, "EV", 222, 75, 109)]
	Emergency = 512,

	[StyleIdentity(10, "Train", 194, 146, 74)]
	Train = 1024,

	[StyleIdentity(11, "P", 74, 89, 161)]
	Parking = 2048,
}