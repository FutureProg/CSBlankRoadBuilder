using System;

namespace BlankRoadBuilder.Domain;

[Flags]
public enum CrosswalkStyle
{
	None = 0,
	Zebra = 1,
	DoubleSolid = 2,
	Dashed = 4,
	Ladder = Zebra | DoubleSolid,
}
