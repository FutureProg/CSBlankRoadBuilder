using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlankRoadBuilder.ThumbnailMaker;
public enum LaneVehicleCategory
{
	Unknown,
	Filler,
	Pedestrian,
	Parking,
	Vehicle,
	Bike = 64,
	Tram = 128
}