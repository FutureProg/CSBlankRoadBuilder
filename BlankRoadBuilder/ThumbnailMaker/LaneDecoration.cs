using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

	[StyleIdentity(7, 207, 133, 46)]
	Benches = 32,

	[StyleIdentity(8, 238, 62, 75)]
	FlowerPots = 64,

	[StyleIdentity(9, 148, 157, 166)]
	StreetLight = 128,

	[StyleIdentity(10, 148, 157, 166)]
	DoubleStreetLight = 256,

	[StyleIdentity(11, 255, 232, 118)]
	Bollard = 512,

	[StyleIdentity(13, 154, 203, 96)]
	Hedge = 1024,

	[StyleIdentity(14, 237, 186, 64)]
	TransitStop = 2048,

	[StyleIdentity(15, 194, 161, 83)]
	TrashBin = 4096,

	[StyleIdentity(16, 111, 187, 217)]
	BikeParking = 8192,
}