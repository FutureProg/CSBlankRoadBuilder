using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BlankRoadBuilder.ThumbnailMaker;

public partial class LaneInfo
{
	[XmlIgnore] public float Width { get; set; }
	[XmlIgnore] public float Position { get; set; }
	[XmlIgnore] public LaneTag Tags { get; set; } = LaneTag.Asphalt;
	[XmlIgnore] public LaneInfo? LeftLane { get; set; }
	[XmlIgnore] public LaneInfo? RightLane { get; set; }
	[XmlIgnore] public float LeftDrivableArea { get; set; }
	[XmlIgnore] public float RightDrivableArea { get; set; }
	[XmlIgnore] public float LeftInvertedDrivableArea { get; set; }
	[XmlIgnore] public float RightInvertedDrivableArea { get; set; }
	[XmlIgnore] public List<NetInfo.Lane> NetLanes { get; set; } = new();

	public LaneVehicleCategory GetVehicleCategory()
	{
		if (Type == LaneType.Filler)
			return LaneVehicleCategory.Filler;

		if (Type == LaneType.Bike)
			return LaneVehicleCategory.Bike;

		if (Type == LaneType.Pedestrian)
			return LaneVehicleCategory.Pedestrian;

		if (Type == LaneType.Parking)
			return LaneVehicleCategory.Parking;

		if (Type.HasFlag(LaneType.Tram))
			return LaneVehicleCategory.Tram;

		if (Type.HasAnyFlag(LaneType.Car, LaneType.Bus, LaneType.Trolley, LaneType.Emergency))
			return LaneVehicleCategory.Vehicle;

		return LaneVehicleCategory.Unknown;
	}

	public static List<LaneType> GetLaneTypes(LaneType laneType)
	{
		if (laneType == LaneType.Empty)
			return new List<LaneType> { LaneType.Empty };

		return Enum
			.GetValues(typeof(LaneType))
			.Cast<LaneType>()
			.Where(e => e != LaneType.Empty && laneType.HasFlag(e))
			.ToList();
	}

	public LaneInfo Duplicate(LaneType newLaneType, float positionDifference = 0F) => new()
	{
		Type = newLaneType,
		Direction = Direction,
		CustomWidth = CustomWidth,
		Elevation = Elevation,
		Decorations = Decorations,
		ParkingAngle = ParkingAngle,
		LeftDrivableArea = LeftDrivableArea,
		RightDrivableArea = RightDrivableArea,
		LeftInvertedDrivableArea = LeftInvertedDrivableArea,
		RightInvertedDrivableArea = RightInvertedDrivableArea,
		LeftLane = LeftLane,
		Position = (float)Math.Round(Position + positionDifference, 3),
		RightLane = RightLane,
		SpeedLimit = SpeedLimit,
		Tags = Tags | LaneTag.StackedLane,
		Width = Width,
		NetLanes = NetLanes
	};
}