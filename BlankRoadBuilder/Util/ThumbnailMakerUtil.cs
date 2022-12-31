
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using UnityEngine;

namespace BlankRoadBuilder.Util;
public static class ThumbnailMakerUtil
{
	public static RoadInfo? GetRoadInfo(string file)
	{
		try
		{
			var road = Path.Combine(Path.Combine(BlankRoadBuilderMod.BuilderFolder, "Roads"), file);

			var x = new XmlSerializer(typeof(RoadInfo));

			using var stream = File.OpenRead(road);
			return (RoadInfo)x.Deserialize(stream);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}

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

		roadInfo.ContainsWiredLanes = roadInfo.Lanes.Any(x => (x.Type & (LaneType.Tram | LaneType.Trolley)) != 0);

		roadInfo.Lanes.Insert(0, new LaneInfo // Add damage & markings lanes
		{
			Type = LaneType.Empty,
			Tags = LaneTag.Damage | LaneTag.StackedLane
		});

		FillLaneTags(roadInfo);

		roadInfo.WiredLanesAreNextToMedians = roadInfo.Lanes
			.Where(x => (x.Type & (LaneType.Tram | LaneType.Trolley)) != 0)
			.All(x => (x.LeftLane != null && x.LeftLane.Type.HasAnyFlag(LaneType.Filler, LaneType.Curb, LaneType.Pedestrian) && x.LeftLane.LaneWidth > 0.5F)
				|| (x.RightLane != null && x.RightLane.Type.HasAnyFlag(LaneType.Filler, LaneType.Curb, LaneType.Pedestrian) && x.RightLane.LaneWidth > 0.5F));

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

	private static void FillLaneTags(RoadInfo roadInfo)
	{
		var stoppableVehicleLanes = LaneType.Car | LaneType.Bus | LaneType.Trolley | LaneType.Tram;
		var curbToggle = true;

		for (var i = 0; i < roadInfo.Lanes.Count; i++)
		{
			roadInfo.Lanes[i].Road = roadInfo;

			fillTag(roadInfo.Lanes[i]
				, i == 0 ? null : roadInfo.Lanes[i - 1]
				, i == roadInfo.Lanes.Count - 1 ? null : roadInfo.Lanes[i + 1]);

			if ((curbToggle || roadInfo.Lanes[i].Type == LaneType.Curb) && !roadInfo.Lanes[i].Tags.HasAnyFlag(LaneTag.StackedLane))
			{
				roadInfo.Lanes[i].Tags |= LaneTag.Sidewalk;
				roadInfo.Lanes[i].Tags &= ~LaneTag.Asphalt;
			}

			if (roadInfo.Lanes[i].Type == LaneType.Curb)
			{
				curbToggle = !curbToggle;
			}
		}

		if (roadInfo.ContainsWiredLanes)
		{
			var rightMostWiredLane = roadInfo.Lanes.Where(x => (x.Type & (LaneType.Tram | LaneType.Trolley)) != 0).LastOrDefault();
			var leftMostWiredLane = roadInfo.Lanes.Where(x => (x.Type & (LaneType.Tram | LaneType.Trolley)) != 0).FirstOrDefault();

			var right = rightMostWiredLane.RightLane;
			var left = leftMostWiredLane.LeftLane;

			while (right != null)
			{
				if (right.Type.HasAnyFlag(LaneType.Filler, LaneType.Pedestrian, LaneType.Curb) && right.LaneWidth >= 0.75F)
				{
					right.Tags |= LaneTag.WirePoleLane;
					break;
				}

				right = right.RightLane;
			}

			while (left != null)
			{
				if (left.Type.HasAnyFlag(LaneType.Filler, LaneType.Pedestrian, LaneType.Curb) && left.LaneWidth >= 0.75F)
				{
					left.Tags |= LaneTag.WirePoleLane;
					break;
				}

				left = left.LeftLane;
			}
		}

		void fillTag(LaneInfo lane, LaneInfo? left, LaneInfo? right)
		{
			if (!(left?.Tags.HasFlag(LaneTag.StackedLane) ?? false))
			{
				lane.LeftLane = left;
			}

			if (!(right?.Tags.HasFlag(LaneTag.StackedLane) ?? false))
			{
				lane.RightLane = right;
			}

			if (left != null && (left.Type & stoppableVehicleLanes) != 0)
			{
				lane.Tags |= LaneTag.StoppableVehicleOnLeft;
			}

			if (right != null && (right.Type & stoppableVehicleLanes) != 0)
			{
				lane.Tags |= LaneTag.StoppableVehicleOnRight;
			}

			if (lane.Type == LaneType.Filler && lane.LaneWidth >= 1.5F)
			{
				lane.Tags |= LaneTag.CenterMedian;
			}
		}
	}

	public static NetInfo.LaneType GetLaneType(LaneType laneType)
	{
		return laneType switch
		{
			LaneType.Car or LaneType.Bike or LaneType.Tram or LaneType.Emergency or LaneType.Trolley => NetInfo.LaneType.Vehicle,
			LaneType.Parking => NetInfo.LaneType.Parking,
			LaneType.Pedestrian => NetInfo.LaneType.Pedestrian,
			LaneType.Bus => NetInfo.LaneType.TransportVehicle,
			_ => NetInfo.LaneType.None,
		};
	}

	public static VehicleInfo.VehicleType GetVehicleType(LaneType laneType, LaneInfo lane)
	{
		if (lane.Tags.HasFlag(LaneTag.StackedLane) && !lane.Tags.HasFlag(LaneTag.Placeholder))
		{
			return VehicleInfo.VehicleType.None;
		}

		return laneType switch
		{
			LaneType.Pedestrian => VehicleInfo.VehicleType.None,
			LaneType.Bike => VehicleInfo.VehicleType.Bicycle,
			LaneType.Tram => VehicleInfo.VehicleType.Tram,
			LaneType.Trolley => VehicleInfo.VehicleType.Trolleybus,
			_ => VehicleInfo.VehicleType.Car,
		};
	}

	public static VehicleInfo.VehicleType GetStopType(LaneType type, LaneInfo lane, RoadInfo road, out bool? forward)
	{
		forward = null;

		switch (type)
		{
			case LaneType.Pedestrian:
				break;

			case LaneType.Car:
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

			//if (forward == true && (road.Options?.DisableRightSidewalkStop ?? false))
			//	forward = null;

			//if (forward == false && (road.Options?.DisableLeftSidewalkStop ?? false))
			//	forward = null;
		}
		else if (lane.Tags.HasFlag(LaneTag.StoppableVehicleOnLeft))
		{
			if (lane.Tags.HasFlag(LaneTag.StoppableVehicleOnRight))
			{
				if (lane.LeftLane?.Direction != LaneDirection.Backwards)
				{
					forward = true;
				}
				else if (lane.RightLane?.Direction != LaneDirection.Forward)
				{
					forward = false;
				}
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
		{
			stopType |= VehicleInfo.VehicleType.Trolleybus;
		}

		if (road.Lanes.Any(x => x.Type.HasFlag(LaneType.Tram)))
		{
			stopType |= VehicleInfo.VehicleType.Tram;
		}

		return stopType;
	}

	public static float GetStopOffset(LaneType type, LaneInfo lane)
	{
		if (type is not LaneType.Bus and not LaneType.Car)
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
			return (type == LaneType.Bus ? 0.5F : 0F) + lane.RightLane.LaneWidth;
		}
		else if (lane.LeftLane?.Type == LaneType.Parking)
		{
			return -((type == LaneType.Bus ? 0.5F : 0F) + lane.LeftLane.LaneWidth);
		}

		return 0F;
	}

	public static float GetLanePosition(LaneType type, LaneInfo lane, RoadInfo road)
	{
		if (type != LaneType.Pedestrian || !lane.Tags.HasFlag(LaneTag.Asphalt))
		{
			return lane.Position;
		}

		var stopType = GetStopType(type, lane, road, out var forward);

		if (stopType != VehicleInfo.VehicleType.None && forward != null)
		{
			var stoppingLane = (bool)forward ? lane.LeftLane : lane.RightLane;

			if (stoppingLane?.Type.HasFlag(LaneType.Bus) ?? false)
			{
				return (float)Math.Round(lane.Position + ((bool)forward ? -0.3F : 0.3F), 3);
			}

			if (stoppingLane?.Type.HasFlag(LaneType.Car) ?? false)
			{
				return (float)Math.Round(lane.Position + ((bool)forward ? -0.1F : 0.1F), 3);
			}
		}

		return lane.Position;
	}

	public static float GetLaneSpeedLimit(LaneType type, LaneInfo lane, RoadInfo road)
	{
		if (lane.SpeedLimit is not null and > 0F)
		{
			return (float)lane.SpeedLimit / 50F;
		}

		return type switch
		{
			LaneType.Car or LaneType.Tram or LaneType.Bus or LaneType.Trolley => road.SpeedLimit / 50F,
			LaneType.Pedestrian => 0.25F,
			LaneType.Bike => lane.Type == LaneType.Bike ? 2F : 1.2F,
			LaneType.Emergency => 2F,
			_ => 1,
		};
	}

	public static NetInfo.Direction GetLaneDirection(LaneInfo lane)
	{
		if (lane.Type <= LaneType.Pedestrian)
		{
			return NetInfo.Direction.Both;
		}

		return lane.Direction switch
		{
			LaneDirection.Backwards => NetInfo.Direction.Backward,
			LaneDirection.Forward => NetInfo.Direction.Forward,
			_ => NetInfo.Direction.Both,
		};
	}

	public static float GetLaneVerticalOffset(LaneInfo lane, RoadInfo road)
	{
		lane.SurfaceElevation = (!lane.Tags.HasFlag(LaneTag.Sidewalk) || lane.Type == LaneType.Curb) && road.RoadType == RoadType.Road ? -0.3F : 0F;

		if (lane.Elevation != null)
		{
			return lane.LaneElevation = Math.Max((float)lane.Elevation, -0.3F);
		}

		var elevation = 0F;

		if (!lane.Tags.HasFlag(LaneTag.Sidewalk) && road.RoadType == RoadType.Road)
		{
			elevation = -0.3F;
		}

		if (lane.Decorations.HasAnyFlag(LaneDecoration.Grass, LaneDecoration.Gravel, LaneDecoration.Pavement))
		{
			elevation = elevation == 0F ? 0.2F : 0F;
		}

		return lane.LaneElevation = elevation;
	}

	public static VehicleInfo.VehicleCategoryPart1 GetVehicleCategory1(LaneType laneType)
	{
		if (laneType == LaneType.Bus)
		{
			return VehicleInfo.VehicleCategoryPart1.Bus;
		}

		return laneType == LaneType.Emergency ? VehicleInfo.VehicleCategoryPart1.None : VehicleInfo.VehicleCategoryPart1.All;
	}

	public static VehicleInfo.VehicleCategoryPart2 GetVehicleCategory2(LaneType laneType)
	{
		if (laneType == LaneType.Emergency)
		{
			return VehicleInfo.VehicleCategoryPart2.Ambulance | VehicleInfo.VehicleCategoryPart2.Police | VehicleInfo.VehicleCategoryPart2.FireTruck;
		}

		return laneType == LaneType.Bus ? VehicleInfo.VehicleCategoryPart2.None : VehicleInfo.VehicleCategoryPart2.All;
	}

	public static float GetLaneCost(LaneType laneType)
	{
		return laneType switch
		{
			LaneType.Car or LaneType.Bus or LaneType.Emergency => 10F,
			LaneType.Pedestrian => 1F,
			LaneType.Bike => 2F,
			LaneType.Tram => 15F,
			LaneType.Trolley => 5F,
			LaneType.Parking => 2.5F,
			_ => 0F,
		};
	}

	public static float GetLaneMaintenanceCost(LaneType laneType)
	{
		return laneType switch
		{
			LaneType.Car or LaneType.Bus or LaneType.Emergency => 0.08F,
			LaneType.Pedestrian => 0.01F,
			LaneType.Bike => 0.02F,
			LaneType.Tram => 0.15F,
			LaneType.Trolley => 0.05F,
			LaneType.Parking => 0.02F,
			_ => 0F,
		};
	}
}
