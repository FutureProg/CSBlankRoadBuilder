using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BlankRoadBuilder.ThumbnailMaker;

public class LaneInfo
{
	public LaneDirection Direction { get; set; }
	public int FillerSize { get; set; }
	public LaneType Type { get; set; }
	public float CustomWidth { get; set; }
	public float? Elevation { get; set; }
	public float? SpeedLimit { get; set; }
	public bool DiagonalParking { get; set; }
	public bool InvertedDiagonalParking { get; set; }
	public bool HorizontalParking { get; set; }
	public bool AddStopToFiller { get; set; }

	public float Width { get; set; }
	public float Position { get; set; }
	public LaneTag Tags { get; set; } = LaneTag.Asphalt;
	public LaneInfo? LeftLane { get; set; }
	public LaneInfo? RightLane { get; set; }
	public float LeftDrivableArea { get; set; }
	public float RightDrivableArea { get; set; }
	public float LeftInvertedDrivableArea { get; set; }
	public float RightInvertedDrivableArea { get; set; }

	[XmlIgnore]
	public List<NetInfo.Lane> NetLanes { get; set; } = new();

	public bool IsFiller() => Tags.HasFlag(LaneTag.Asphalt) && GetLaneTypes(Type).All(x => x >= LaneType.Empty && x < LaneType.Car);

	public LaneVehicleCategory GetVehicleCategory()
	{
		var types = GetLaneTypes(Type);

		if (types.All(x => x >= LaneType.Grass && x <= LaneType.Trees))
			return LaneVehicleCategory.Filler;

		if (Type == LaneType.Bike)
			return LaneVehicleCategory.Bike;

		if (Type == LaneType.Pedestrian)
			return LaneVehicleCategory.Pedestrian;

		if (Type == LaneType.Parking)
			return LaneVehicleCategory.Parking;

		if (Type.HasFlag(LaneType.Tram))
			return LaneVehicleCategory.Tram;

		if ((Type & (LaneType.Car | LaneType.Highway | LaneType.Bus | LaneType.Trolley | LaneType.Emergency)) != 0)
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
		FillerSize = FillerSize,
		CustomWidth = CustomWidth,
		Elevation = Elevation,
		AddStopToFiller = AddStopToFiller,
		DiagonalParking = DiagonalParking,
		InvertedDiagonalParking = InvertedDiagonalParking,
		HorizontalParking = HorizontalParking,
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