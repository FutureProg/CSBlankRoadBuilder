using System;

namespace BlankRoadBuilder.ThumbnailMaker;

[Flags]
public enum LaneDecoration
{
	[StyleIdentity(0, 50, 50, 50, Order = 0)]
	None = 0,

	[StyleIdentity(12, 64, 162, 237, Order = 1)]
	Filler = 1 << 1,

	[StyleIdentity(1, 154, 203, 96, Order = 2)]
	Grass = 1 << 2,

	[StyleIdentity(2, 121, 124, 130, Order = 3)]
	Pavement = 1 << 3,

	[StyleIdentity(3, 79, 61, 55, Order = 4)]
	Gravel = 1 << 4,

	[StyleIdentity(4, 72, 161, 73, Order = 5)]
	Tree = 1 << 5,

	[StyleIdentity(7, 194, 145, 87, Order = 11)]
	Benches = 1 << 6,

	[StyleIdentity(8, 204, 69, 96, Order = 6)]
	FlowerPots = 1 << 7,

	[StyleIdentity(9, 123, 133, 143, Order = 7)]
	StreetLight = 1 << 8,

	[StyleIdentity(10, 123, 133, 143, Order = 8)]
	DoubleStreetLight = 1 << 9,

	[StyleIdentity(11, 214, 196, 105, Order = 16)]
	Bollard = 1 << 10,

	[StyleIdentity(13, 145, 186, 97, Order = 17)]
	Hedge = 1 << 11,

	[StyleIdentity(14, 237, 186, 64, Order = 10)]
	TransitStop = 1 << 12,

	[StyleIdentity(15, 194, 161, 83, Order = 12)]
	TrashBin = 1 << 13,

	[StyleIdentity(16, 111, 187, 217, Order = 14)]
	BikeParking = 1 << 14,

	[StyleIdentity(17, 132, 138, 177, Order = 13)]
	StreetAds = 1 << 15,

	[StyleIdentity(18, 128, 147, 156, Order = 18)]
	Barrier = 1 << 16,

	[StyleIdentity(19, 200, 50, 60, Order = 15)]
	FireHydrant = 1 << 17,

	[StyleIdentity(20, 107, 64, 52, Order = 9)]
	LampPost = 1 << 18,
}