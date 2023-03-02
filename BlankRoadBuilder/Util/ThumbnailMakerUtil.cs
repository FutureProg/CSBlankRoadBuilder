using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;
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

		return laneType == LaneType.Curb && !(ModOptions.AlwaysAddGhostLanes || ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.IMTMarkings))
			? VehicleInfo.VehicleType.None
			: laneType switch
			{
				LaneType.Pedestrian => VehicleInfo.VehicleType.None,
				LaneType.Bike => VehicleInfo.VehicleType.Bicycle,
				LaneType.Tram => VehicleInfo.VehicleType.Tram,
				LaneType.Trolley => VehicleInfo.VehicleType.Trolleybus,
				_ => VehicleInfo.VehicleType.Car,
			};
	}

	public static float GetLaneSpeedLimit(LaneType type, LaneInfo lane, RoadInfo road)
	{
		if (lane.SpeedLimit is not null and > 0F)
		{
			return (float)lane.SpeedLimit * (road.RegionType == RegionType.USA ? 1.609F : 1F) / 50F;
		}

		var limit = road.SpeedLimit == 0 ? DefaultSpeedSign(road.RoadType, road.RegionType == RegionType.USA) : road.SpeedLimit;

		return type switch
		{
			LaneType.Car or LaneType.Tram or LaneType.Bus or LaneType.Trolley => limit * (road.RegionType == RegionType.USA ? 1.609F : 1F) / 50F,
			LaneType.Pedestrian => 0.25F,
			LaneType.Bike => lane.Type == LaneType.Bike ? 2F : 1.2F,
			LaneType.Emergency => 2F,
			_ => 1,
		};
	}

	private static int DefaultSpeedSign(RoadType type, bool mph)
	{
		return type switch
		{
			RoadType.Road => mph ? 25 : 40,
			RoadType.Highway => mph ? 55 : 80,
			RoadType.Pedestrian => mph ? 10 : 20,
			RoadType.Flat => mph ? 15 : 30,
			_ => 0,
		};
	}

	public static NetInfo.Direction GetLaneDirection(LaneInfo lane)
	{
		return lane.Type <= LaneType.Pedestrian
			? NetInfo.Direction.Both
			: lane.Direction switch
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
		return (laneType is LaneType.Bus && !ModOptions.AllowAllVehiclesOnBusLanes)
			? VehicleInfo.VehicleCategoryPart1.Bus | VehicleInfo.VehicleCategoryPart1.Taxi
			: laneType is LaneType.Emergency ? VehicleInfo.VehicleCategoryPart1.None : VehicleInfo.VehicleCategoryPart1.All;
	}

	public static VehicleInfo.VehicleCategoryPart2 GetVehicleCategory2(LaneType laneType)
	{
		return laneType is LaneType.Emergency || (laneType is LaneType.Bus && !ModOptions.AllowAllVehiclesOnBusLanes)
			? VehicleInfo.VehicleCategoryPart2.Ambulance | VehicleInfo.VehicleCategoryPart2.Police | VehicleInfo.VehicleCategoryPart2.FireTruck
			: VehicleInfo.VehicleCategoryPart2.All;
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

	public static IEnumerable<string> GetAutoTags(RoadInfo road)
	{
		if (road?.Lanes == null)
		{
			yield break;
		}

		if (road.Lanes.Any(x => x.Type.HasFlag(LaneType.Parking)))
		{
			yield return "Parking";
		}

		if (road.Lanes.Any(x => x.Type.HasFlag(LaneType.Tram)))
		{
			yield return "Tram";
		}

		if (road.Lanes.Any(x => x.Type.HasFlag(LaneType.Trolley)))
		{
			yield return "Trolley";
		}

		if (road.Lanes.Any(x => x.Type.HasFlag(LaneType.Bike)))
		{
			yield return "Bike";
		}

		if (road.Lanes.Any(x => x.Type.HasFlag(LaneType.Bus)))
		{
			yield return "Bus";
		}

		if (IsOneWay(road.Lanes) == true)
		{
			yield return "One-Way";
		}
	}

	public static bool? IsOneWay<T>(IEnumerable<T> lanes) where T : LaneInfo
	{
		var types = new[] { LaneType.Bike, LaneType.Car, LaneType.Bus, LaneType.Tram, LaneType.Trolley, LaneType.Emergency };
		var firstLane = lanes.FirstOrDefault(x => x.Type.HasAnyFlag(types));

		if (firstLane != null)
		{
			return firstLane.Direction != LaneDirection.Both && lanes
				.Where(x => x.Type.HasAnyFlag(types))
				.All(x => x.Direction == firstLane.Direction);
		}

		return null;
	}

	internal static float CalculateRoadSize(RoadInfo roadInfo)
	{
		var sizeLanes = roadInfo.Lanes.Where(x => !x.Tags.HasAnyFlag(LaneTag.StackedLane)).ToList();
		var leftCurb = sizeLanes.FirstOrDefault(x => x.Type == LaneType.Curb);
		var rightCurb = sizeLanes.LastOrDefault(x => x.Type == LaneType.Curb);
		float leftPavementWidth, rightPavementWidth;

		if (leftCurb == null || rightCurb == null)
		{
			leftPavementWidth = rightPavementWidth = 0;
			return 0;
		}

		leftPavementWidth = sizeLanes.Where(x => sizeLanes.IndexOf(x) <= sizeLanes.IndexOf(leftCurb)).Sum(x => x.LaneWidth);
		rightPavementWidth = sizeLanes.Where(x => sizeLanes.IndexOf(x) >= sizeLanes.IndexOf(rightCurb)).Sum(x => x.LaneWidth);
		var asphaltWidth = sizeLanes.Where(x => sizeLanes.IndexOf(x) > sizeLanes.IndexOf(leftCurb) && sizeLanes.IndexOf(x) < sizeLanes.IndexOf(rightCurb)).Sum(x => x.LaneWidth) + (2 * roadInfo.BufferWidth);
		var totalWidth = Math.Max(1.5F, leftPavementWidth) + Math.Max(1.5F, rightPavementWidth) + asphaltWidth;

		if (roadInfo.VanillaWidth)
		{
			totalWidth = (float)(16 * Math.Ceiling((totalWidth - 1F) / 16D));
		}

		return (float)Math.Round(totalWidth, 2);
	}

	public static int LaneTypeImportance(LaneType type)
	{
		return type switch
		{
			LaneType.Train => 21,
			LaneType.Tram => 20,
			LaneType.Emergency => 19,
			LaneType.Trolley => 18,
			LaneType.Bus => 17,
			LaneType.Bike => 16,
			LaneType.Car => 15,
			LaneType.Parking => 14,
			LaneType.Pedestrian => 13,
			_ => 0,
		};
	}
}
