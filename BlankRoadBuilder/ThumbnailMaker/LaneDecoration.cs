using System;

namespace BlankRoadBuilder.ThumbnailMaker;

[Flags]
public enum LaneDecoration
{
	[StyleIdentity(0, 50, 50, 50)]
	None = 0,

	[StyleIdentity(12, 64, 162, 237)]
	Filler = 1,

	[StyleIdentity(1, 154, 203, 96)]
	Grass = 2,

	[StyleIdentity(2, 99, 102, 107)]
	Pavement = 4,

	[StyleIdentity(3, 79, 61, 55)]
	Gravel = 8,

	[StyleIdentity(4, 72, 161, 73)]
	Tree = 16,

	[StyleIdentity(7, 194, 145, 87)]
	Benches = 32,

	[StyleIdentity(8, 204, 69, 96)]
	FlowerPots = 64,

	[StyleIdentity(9, 123, 133, 143)]
	StreetLight = 128,

	[StyleIdentity(10, 123, 133, 143)]
	DoubleStreetLight = 256,

	[StyleIdentity(11, 214, 196, 105)]
	Bollard = 512,

	[StyleIdentity(13, 145, 186, 97)]
	Hedge = 1024,

	[StyleIdentity(14, 237, 186, 64)]
	TransitStop = 2048,

	[StyleIdentity(15, 194, 161, 83)]
	TrashBin = 4096,

	[StyleIdentity(16, 111, 187, 217)]
	BikeParking = 8192,

	[StyleIdentity(17, 132, 138, 177)]
	StreetAds = 16384,

	[StyleIdentity(18, 128, 147, 156)]
	Barrier = 32768,

	[StyleIdentity(19, 200, 50, 60)]
	FireHydrant = 65536,
}