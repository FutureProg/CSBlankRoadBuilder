namespace BlankRoadBuilder.Util;

using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using UnityEngine;

using static EconomyManager;

public static class ThumbnailMakerUtil
{
	public static RoadInfo? GetRoadInfo(string file)
	{
		try
		{
			var road = Path.Combine(Path.Combine(BlankRoadBuilderMod.BuilderFolder, "Roads"), file);

			var x = new XmlSerializer(typeof(RoadInfo));

			using (var stream = File.OpenRead(road))
			{
				return (RoadInfo)x.Deserialize(stream);
			}
		}
		catch (Exception ex)
		{ Debug.LogException(ex); }

		return null;
	}

	public static RoadInfo? ProcessRoadInfo(RoadInfo? roadInfo)
	{
		if (roadInfo == null)
		{
			Debug.LogError("Road provided was null");
			return null;
		}

		if (roadInfo.Lanes == null)
		{
			Debug.LogError("Road lanes provided were null");
			return null;
		}

		if (roadInfo.Lanes.Count == 0)
		{
			Debug.LogError("No road lanes were provided");
			return null;
		}

		if (roadInfo.SpeedLimit <= 0)
		{
			roadInfo.SpeedLimit = 40F;
		}

		if (roadInfo.Width <= 0F)
		{
			roadInfo.Width = 0F;
		}

		roadInfo.BufferSize *= 2F;

		roadInfo.OneWay = IsOneWay(roadInfo.Lanes);
		roadInfo.DiagonalParking = roadInfo.Lanes.Any(x => x.DiagonalParking);
		roadInfo.InvertedDiagonalParking = roadInfo.Lanes.Any(x => x.InvertedDiagonalParking);
		roadInfo.HorizontalParking = roadInfo.Lanes.Any(x => x.HorizontalParking);
		roadInfo.ContainsWiredLanes = roadInfo.Lanes.Any(x => (x.Type & (LaneType.Tram | LaneType.Trolley)) != 0);

		FixLanes(roadInfo);

		CreateMissingLanes(roadInfo);

		FillLaneTags(roadInfo);

		roadInfo.WiredLanesAreNextToMedians = roadInfo.Lanes
			.Where(x => (x.Type & (LaneType.Tram | LaneType.Trolley)) != 0)
			.All(x => x.Tags.HasFlag(LaneTag.MedianOnLeft) || x.Tags.HasFlag(LaneTag.MedianOnRight)
				|| x.LeftLane?.Type == LaneType.Pedestrian || x.RightLane?.Type == LaneType.Pedestrian);

		if (roadInfo.ContainsWiredLanes)
		{
			roadInfo.Lanes.Insert(0, new LaneInfo
			{
				Type = LaneType.Empty,
				Tags = LaneTag.StackedLane
			});
		}

		return roadInfo;
	}

	private static void FixLanes(RoadInfo roadInfo)
	{
		foreach (var lane in roadInfo.Lanes)
		{
			if (lane.Type == LaneType.Pedestrian)
			{
				lane.Direction = LaneDirection.Both;
			}
		}
	}

	private static void CreateMissingLanes(RoadInfo roadInfo)
	{
		if (roadInfo.RoadType == RoadType.Road || roadInfo.RoadType == RoadType.Flat)
		{
			var leftPed = roadInfo.Lanes.FirstOrDefault();

			if (leftPed?.Type == LaneType.Pedestrian)
			{
				leftPed.Tags = LaneTag.Sidewalk;

				if (leftPed.Direction == LaneDirection.None)
				{
					leftPed.Direction = LaneDirection.Both;
				}
			}
			else
			{
				roadInfo.Lanes.Insert(0, pedestrianLane());
			}

			var rightPed = roadInfo.Lanes.LastOrDefault();

			if (rightPed?.Type == LaneType.Pedestrian)
			{
				rightPed.Tags = LaneTag.Sidewalk;

				if (rightPed.Direction == LaneDirection.None)
				{
					rightPed.Direction = LaneDirection.Both;
				}
			}
			else
			{
				roadInfo.Lanes.Add(pedestrianLane());
			}
		}

		roadInfo.Lanes.Insert(0, bufferLane(LaneDirection.Backwards));
		roadInfo.Lanes.Add(bufferLane(LaneDirection.Forward));

		roadInfo.Lanes.Insert(0, new LaneInfo
		{
			Type = LaneType.Empty,
			Tags = LaneTag.Damage | LaneTag.StackedLane
		});

		static LaneInfo pedestrianLane() => new()
		{
			Type = LaneType.Pedestrian,
			Direction = LaneDirection.Both,
			Tags = LaneTag.Sidewalk,
			CustomWidth = 2F
		};

		static LaneInfo bufferLane(LaneDirection direction) => new()
		{
			Type = LaneType.Empty,
			Direction = direction,
			Tags = LaneTag.Buffer,
			CustomWidth = 2F
		};
	}

	private static void FillLaneTags(RoadInfo roadInfo)
	{
		var stoppableVehicleLanes = LaneType.Car | LaneType.Bus | LaneType.Trolley | LaneType.Tram;

		for (var i = 0; i < roadInfo.Lanes.Count; i++)
		{
			if (roadInfo.Lanes[i].IsFiller() && !roadInfo.Lanes[i].Tags.HasFlag(LaneTag.Buffer))
			{
				roadInfo.Lanes[i].Direction = LaneDirection.Both;
			}

			if (roadInfo.Lanes[i].Type == LaneType.Parking)
			{
				roadInfo.Lanes[i].DiagonalParking = roadInfo.DiagonalParking;
				roadInfo.Lanes[i].InvertedDiagonalParking = roadInfo.InvertedDiagonalParking;
				roadInfo.Lanes[i].HorizontalParking = roadInfo.HorizontalParking;
			}

			fillTag(roadInfo.Lanes[i]
				, i == 0 ? null : roadInfo.Lanes[i - 1]
				, i == roadInfo.Lanes.Count - 1 ? null : roadInfo.Lanes[i + 1]);
		}

		if (roadInfo.ContainsWiredLanes)
		{
			var rightMostWiredLane = roadInfo.Lanes.Where(x => (x.Type & (LaneType.Tram | LaneType.Trolley)) != 0).LastOrDefault();
			var leftMostWiredLane = roadInfo.Lanes.Where(x => (x.Type & (LaneType.Tram | LaneType.Trolley)) != 0).FirstOrDefault();

			var right = rightMostWiredLane.RightLane;
			var left = leftMostWiredLane.LeftLane;

			while (right != null)
			{
				if ((right.IsFiller() && right.FillerSize >= 50) || right.Type == LaneType.Pedestrian)
				{
					right.Tags |= LaneTag.WirePoleLane;
					break;
				}

				right = right.RightLane;
			}

			while (left != null)
			{
				if ((left.IsFiller() && left.FillerSize >= 30) || left.Type == LaneType.Pedestrian)
				{
					left.Tags |= LaneTag.WirePoleLane;
					break;
				}

				left = left.LeftLane;
			}
		}

		void fillTag(LaneInfo lane, LaneInfo? left, LaneInfo? right)
		{
			lane.LeftLane = left;
			lane.RightLane = right;

			if (left != null)
			{
				if (left.IsFiller() && left.FillerSize > 20)
				{
					lane.Tags |= LaneTag.MedianOnLeft;
				}

				if ((left.Type & stoppableVehicleLanes) != 0)
				{
					lane.Tags |= LaneTag.StoppableVehicleOnLeft;
				}
			}

			if (right != null)
			{
				if (right.IsFiller() && right.FillerSize > 20)
				{
					lane.Tags |= LaneTag.MedianOnRight;
				}

				if ((right.Type & stoppableVehicleLanes) != 0)
				{
					lane.Tags |= LaneTag.StoppableVehicleOnRight;
				}
			}

			if (lane.IsFiller()
				&& lane.FillerSize >= 70
				&& lane.Tags.HasFlag(LaneTag.StoppableVehicleOnLeft | LaneTag.StoppableVehicleOnRight)
				&& left?.Direction != right?.Direction)
			{
				roadInfo.ContainsCenterMedian = true;

				lane.Tags |= LaneTag.CenterMedian;
			}
		}
	}

	private static bool? IsOneWay(List<LaneInfo> lanes)
	{
		var car = lanes.FirstOrDefault(x => x.Type.HasFlag(LaneType.Car) || x.Type.HasFlag(LaneType.Highway));
		var bus = lanes.FirstOrDefault(x => x.Type.HasFlag(LaneType.Bus));
		var bike = lanes.FirstOrDefault(x => x.Type.HasFlag(LaneType.Bike));

		if (car != null)
		{
			return car.Direction != LaneDirection.Both && lanes
				.Where(x => x.Type.HasFlag(LaneType.Car) || x.Type.HasFlag(LaneType.Highway))
				.All(x => x.Direction == car.Direction);
		}

		if (bus != null)
		{
			return bus.Direction != LaneDirection.Both && lanes
				.Where(x => x.Type.HasFlag(LaneType.Bus))
				.All(x => x.Direction == bus.Direction);
		}

		if (bike != null)
		{
			return bike.Direction != LaneDirection.Both && lanes
				.Where(x => x.Type.HasFlag(LaneType.Bike))
				.All(x => x.Direction == bike.Direction);
		}

		return null;
	}

	public static NetInfo.LaneType GetLaneType(LaneType laneType, LaneInfo lane, RoadInfo road)
	{
		switch (laneType)
		{
			case LaneType.Car:
			case LaneType.Bike:
			case LaneType.Highway:
			case LaneType.Tram:
			case LaneType.Emergency:
			case LaneType.Trolley:
				return NetInfo.LaneType.Vehicle;

			case LaneType.Parking:
				return NetInfo.LaneType.Parking;

			case LaneType.Pedestrian:
				if (lane.Tags.HasFlag(LaneTag.Sidewalk))
				{
					if ((lane.Position < 0 && (road.Options?.RemovePedestrianLaneOnTheLeftSidewalk ?? false))
						|| (lane.Position > 0 && (road.Options?.RemovePedestrianLaneOnTheRightSidewalk ?? false)))
					{ 
						return NetInfo.LaneType.None;
					}
				}

				return NetInfo.LaneType.Pedestrian;

			case LaneType.Bus:
				return NetInfo.LaneType.TransportVehicle;
		}

		return NetInfo.LaneType.None;
	}

	public static VehicleInfo.VehicleType GetVehicleType(LaneType laneType, LaneInfo lane)
	{
		if (lane.Tags.HasFlag(LaneTag.StackedLane))
		{
			return VehicleInfo.VehicleType.None;
		}

		switch (laneType)
		{
			case LaneType.Empty:
			case LaneType.Grass:
			case LaneType.Pavement:
			case LaneType.Gravel:
			case LaneType.Trees:
			case LaneType.Car:
			case LaneType.Parking:
			case LaneType.Highway:
			case LaneType.Bus:
				return VehicleInfo.VehicleType.Car;

			case LaneType.Bike:
				return VehicleInfo.VehicleType.Bicycle;

			case LaneType.Tram:
				return VehicleInfo.VehicleType.Tram;

			case LaneType.Trolley:
				return VehicleInfo.VehicleType.Trolleybus;
		}

		return VehicleInfo.VehicleType.None;
	}

	public static VehicleInfo.VehicleType GetStopType(LaneType type, LaneInfo lane, RoadInfo road, out bool? forward)
	{
		forward = null;

		switch (type)
		{
			case LaneType.Pedestrian:
				break;

			case LaneType.Car:
			case LaneType.Highway:
			case LaneType.Bus:
				return VehicleInfo.VehicleType.Car;

			case LaneType.Tram:
				return VehicleInfo.VehicleType.Tram;

			case LaneType.Trolley:
				return VehicleInfo.VehicleType.Trolleybus;

			default:
				return VehicleInfo.VehicleType.None;
		}

		if (lane.Tags.HasFlag(LaneTag.Sidewalk))
		{
			forward = lane.Position > 0;

			if (forward == true && (road.Options?.DisableRightSidewalkStop ?? false))
				forward = null;

			if (forward == false && (road.Options?.DisableLeftSidewalkStop ?? false))
				forward = null;
		}
		else if (lane.Tags.HasFlag(LaneTag.StoppableVehicleOnLeft))
		{
			if (lane.Tags.HasFlag(LaneTag.StoppableVehicleOnRight))
			{
				if (lane.LeftLane?.Direction != LaneDirection.Backwards)
					forward = true;
				else if (lane.RightLane?.Direction != LaneDirection.Forward)
					forward = false;
			}
			else
			{
				forward = true;
			}
		}
		else if (lane.Tags.HasFlag(LaneTag.StoppableVehicleOnRight))
		{
			forward = false;
		}

		if (forward == null)
		{
			return VehicleInfo.VehicleType.None;
		}

		var stopType = VehicleInfo.VehicleType.Car;

		if (road.Lanes.Any(x => x.Type.HasFlag(LaneType.Trolley)))
			stopType |= VehicleInfo.VehicleType.Trolleybus;

		if (road.Lanes.Any(x => x.Type.HasFlag(LaneType.Tram)))
			stopType |= VehicleInfo.VehicleType.Tram;
		
		return stopType;
	}

	public static float GetStopOffset(LaneType type, LaneInfo lane)
	{
		if (type != LaneType.Bus && type != LaneType.Car)
		{
			return 0F;
		}

		if (lane.RightLane?.Type == LaneType.Pedestrian)
		{
			return type == LaneType.Bus ? -0.3F : -0.1F;
		}
		else if (lane.LeftLane?.Type == LaneType.Pedestrian)
		{
			return type == LaneType.Bus ? 0.3F : 0.1F;
		}

		if (lane.RightLane?.Type == LaneType.Parking)
		{
			return (type == LaneType.Bus ? 0.5F : 0F) + lane.RightLane.Width;
		}
		else if (lane.LeftLane?.Type == LaneType.Parking)
		{
			return -((type == LaneType.Bus ? 0.5F : 0F) + lane.LeftLane.Width);
		}

		return 0F;
	}

	public static float GetLanePosition(LaneType type, LaneInfo lane, RoadInfo road)
	{
		if (type != LaneType.Pedestrian || !lane.Tags.HasFlag(LaneTag.Asphalt))
			return lane.Position;

		var stopType = GetStopType(type, lane, road, out var forward);

		if (stopType != VehicleInfo.VehicleType.None && forward != null)
		{
			var stoppingLane = (bool)forward ? lane.LeftLane : lane.RightLane;

			if (stoppingLane?.Type.HasFlag(LaneType.Bus) ?? false)
				return (float)Math.Round(lane.Position + ((bool)forward ? -0.3F : 0.3F), 3);

			if (stoppingLane?.Type.HasFlag(LaneType.Car) ?? false)
				return (float)Math.Round(lane.Position + ((bool)forward ? -0.1F : 0.1F), 3);
		}

		return lane.Position;
	}

	public static float GetLaneWidth(LaneType type, LaneInfo lane)
	{
		switch (type)
		{
			case LaneType.Empty:
			case LaneType.Grass:
			case LaneType.Pavement:
			case LaneType.Gravel:
			case LaneType.Trees:
				return (float)Math.Round(Math.Ceiling(0.04 * ModOptions.LaneSizes[type] * lane.FillerSize) / 4, 2);

			case LaneType.Parking:
				return 
					lane.HorizontalParking ? ModOptions.LaneSizes.DiagonalParkingSize :
					lane.DiagonalParking || lane.InvertedDiagonalParking ? ModOptions.LaneSizes.DiagonalParkingSize : 
					ModOptions.LaneSizes[type];
		}

		return ModOptions.LaneSizes[type];
	}

	public static float GetLaneSpeedLimit(LaneType type, LaneInfo lane, RoadInfo road)
	{
		if (lane.SpeedLimit != null && lane.SpeedLimit > 0F)
		{
			return (float)lane.SpeedLimit / 50F;
		}

		switch (type)
		{
			case LaneType.Car:
			case LaneType.Tram:
			case LaneType.Bus:
			case LaneType.Trolley:
			case LaneType.Highway:
				return road.SpeedLimit / 50F;

			case LaneType.Pedestrian:
				return 0.25F;

			case LaneType.Bike:
				return lane.Type == LaneType.Bike ? 2F : 1.2F;

			case LaneType.Emergency:
				return 2F;
		}

		return 1;
	}

	public static NetInfo.Direction GetLaneDirection(LaneInfo lane)
	{
		switch (lane.Direction)
		{
			case LaneDirection.Both:
				return NetInfo.Direction.Both;
			case LaneDirection.Backwards:
				return NetInfo.Direction.Backward;
			case LaneDirection.Forward:
				return NetInfo.Direction.Forward;
			default:
				return NetInfo.Direction.None;
		}
	}

	public static float GetLaneVerticalOffset(LaneInfo lane, RoadInfo road)
	{
		if (lane.Tags.HasFlag(LaneTag.Sidewalk))
		{
			return 0F;
		}

		if (lane.Elevation == null)
		{
			return road.RoadType == RoadType.Road ? -0.3F : 0F;
		}

		return Math.Min((float)lane.Elevation, 0F);
	}

	public static VehicleInfo.VehicleCategoryPart1 GetVehicleCategory1(LaneType laneType)
	{
		if (laneType == LaneType.Bus)
			return VehicleInfo.VehicleCategoryPart1.Bus;

		if (laneType == LaneType.Emergency)
			return VehicleInfo.VehicleCategoryPart1.None;

		return VehicleInfo.VehicleCategoryPart1.All;
	}

	public static VehicleInfo.VehicleCategoryPart2 GetVehicleCategory2(LaneType laneType)
	{
		if (laneType == LaneType.Emergency)
			return VehicleInfo.VehicleCategoryPart2.Ambulance | VehicleInfo.VehicleCategoryPart2.Police | VehicleInfo.VehicleCategoryPart2.FireTruck;

		if (laneType == LaneType.Bus)
			return VehicleInfo.VehicleCategoryPart2.None;

		return VehicleInfo.VehicleCategoryPart2.All;
	}

	public static float GetLaneCost(LaneType laneType)
	{
		switch (laneType)
		{
			case LaneType.Grass:
			case LaneType.Gravel:
				return 8F;
			case LaneType.Pavement:
				return 1F;
			case LaneType.Trees:
				return 10F;
			case LaneType.Car:
			case LaneType.Bus:
			case LaneType.Emergency:
				return 10F;
			case LaneType.Pedestrian:
				return 1F;
			case LaneType.Highway:
				return 25F;
			case LaneType.Bike:
				return 2F;
			case LaneType.Tram:
				return 15F;
			case LaneType.Trolley:
				return 5F;
			case LaneType.Parking:
				return 2.5F;
			default:
				return 0F;
		}
	}

	public static float GetLaneMaintenanceCost(LaneType laneType)
	{
		switch (laneType)
		{
			case LaneType.Grass:
			case LaneType.Gravel:
				return 0.06F;
			case LaneType.Pavement:
				return 0.01F;
			case LaneType.Trees:
				return 0.1F;
			case LaneType.Car:
			case LaneType.Bus:
			case LaneType.Emergency:
				return 0.08F;
			case LaneType.Pedestrian:
				return 0.01F;
			case LaneType.Highway:
				return 0.2F;
			case LaneType.Bike:
				return 0.02F;
			case LaneType.Tram:
				return 0.15F;
			case LaneType.Trolley:
				return 0.05F;
			case LaneType.Parking:
				return 0.02F;
			default:
				return 0F;
		}
	}
}
