using System;

namespace BlankRoadBuilder.Domain;

[Flags]
public enum GenericMarkingType
{
	Normal = 1,
	Flipped = 2,
	End = 4,
	Hard = 8,
	Soft = 16,
	Parking = 32,
	Bike = 64,
	Tram = 128,
}