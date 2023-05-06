using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util;

public static class StopsUtil
{
	public static void ProcessStopsInfo(RoadInfo roadInfo)
	{
		var stoppableVehicleLanes = LaneType.Car | LaneType.Bus | LaneType.Trolley | LaneType.Tram;

		foreach (var item in roadInfo.Lanes)
		{
			if ((item.Type & stoppableVehicleLanes) != 0)
				processVehicleLane(item);
		}

		foreach (var grp in roadInfo.Lanes.GroupBy(x => new { Lane = x.Stops.CanStopAt, Left = x.Stops.CanStopAt?.Position < x.Position }))
		{
			foreach (var item in grp.OrderBy(x => x.Stops.Distance).Skip(1))
			{
				item.Stops = new StopsInfo();
			}
		}

		foreach (var item in roadInfo.Lanes)
		{
			if (item.Type.HasFlag(LaneType.Pedestrian) || item.Decorations.HasFlag(LaneDecoration.TransitStop))
				processPedestrianLane(item);
		}

		void processVehicleLane(LaneInfo lane)
		{
			if (lane.Type.HasFlag(LaneType.Car) && !lane.Type.HasFlag(LaneType.Bus) && ModOptions.DisableCarStopsWithBuses && roadInfo.Lanes.Any(x => x.Type.HasFlag(LaneType.Bus)))
				return;

			var validLanes = roadInfo.Lanes.Select(l =>
			{
				if (!l.Type.HasFlag(LaneType.Pedestrian) && !l.Decorations.HasFlag(LaneDecoration.TransitStop))
					return null;

				var distance = (double)Math.Abs(l.Position - lane.Position) - ((lane.LaneWidth + l.LaneWidth) / 2);

				if (lane.Type.HasAnyFlag(LaneType.Car, LaneType.Bus))
				{
					var lanesBetween = roadInfo.Lanes.Where(x =>
						x.Position.IsWithin(l.Position, lane.Position) &&
						(x.Type is LaneType.Parking or LaneType.Empty || x.Decorations.HasFlag(LaneDecoration.BusBay)) &&
						!x.Tags.HasFlag(LaneTag.StackedLane));

					foreach (var item in lanesBetween)
					{
						distance -= item.LaneWidth;
					}
				}

				distance = Math.Round(distance, 4);

				if (distance > ModOptions.MaximumStopDistance || distance < 0)
					return null;

				return new { Distance = distance, Lane = l };
			})
			.Where(x => x != null)
			.OrderBy(x => x!.Distance);

			if (!validLanes.Any())
			{
				return;
			}

			var stopType = VehicleInfo.VehicleType.None;

			if (lane.Type.HasFlag(LaneType.Trolley))
				stopType |= VehicleInfo.VehicleType.Trolleybus;

			if (lane.Type.HasFlag(LaneType.Tram))
				stopType |= VehicleInfo.VehicleType.Tram;

			if (lane.Type.HasAnyFlag(LaneType.Bus, LaneType.Car))
				stopType |= VehicleInfo.VehicleType.Car;

			var forward = lane.Direction is LaneDirection.Forward;

			var stopLane = lane.Direction switch
			{
				LaneDirection.Forward => validLanes.FirstOrAny(x => x!.Lane!.Position > lane.Position),
				LaneDirection.Backwards => validLanes.FirstOrAny(x => x!.Lane!.Position < lane.Position),
				_ => validLanes.FirstOrDefault()
			};

			if (stopLane != null)
			{
				lane.Stops = new StopsInfo
				{
					StopType = stopType,
					CanStopAt = stopLane.Lane,
					Distance = stopLane.Distance
				};
			}
		}

		void processPedestrianLane(LaneInfo lane)
		{
			var validLanes = roadInfo.Lanes.Where(x => x.Stops.CanStopAt == lane);
			var validTypes = validLanes.Aggregate(LaneType.Empty, (s, l) => s | (l.Type & stoppableVehicleLanes));

			if (validTypes == LaneType.Empty)
			{
				return;
			}

			if (lane.Type.HasFlag(LaneType.Pedestrian))
			{
				lane.Stops = new StopsInfo
				{
					StopType = getStopType(validLanes, null),
					BusSide = getStopSide(validLanes.Where(x => x.Type.HasAnyFlag(LaneType.Bus, LaneType.Car))),
					TramSide = getStopSide(validLanes.Where(x => x.Type.HasFlag(LaneType.Tram))),
					TrolleySide = getStopSide(validLanes.Where(x => x.Type.HasFlag(LaneType.Trolley))),
				};
			}
			else
			{
				lane.Stops = new StopsInfo
				{
					LeftStopType = getStopType(validLanes, false),
					RightStopType = getStopType(validLanes, true),
					LeftBusSide = StopsInfo.Side.Left,
					LeftTramSide = StopsInfo.Side.Left,
					LeftTrolleySide = StopsInfo.Side.Left,
					RightBusSide = StopsInfo.Side.Right,
					RightTramSide = StopsInfo.Side.Right,
					RightTrolleySide = StopsInfo.Side.Right,
				};
			}

			VehicleInfo.VehicleType getStopType(IEnumerable<LaneInfo> lanes, bool? right)
			{
				if (right != null)
					lanes = lanes.Where(x => right.Value ? x.Position > lane.Position : x.Position < lane.Position);

				var stop = VehicleInfo.VehicleType.None;

				foreach (var validTypes in lanes.Select(x => x.Type))
				{
					if (validTypes.HasFlag(LaneType.Trolley))
						stop |= VehicleInfo.VehicleType.Trolleybus;

					if (validTypes.HasFlag(LaneType.Tram))
						stop |= VehicleInfo.VehicleType.Tram;

					if (validTypes.HasAnyFlag(LaneType.Bus, LaneType.Car, LaneType.Parking))
						stop |= VehicleInfo.VehicleType.Car;
				}

				return stop;
			}

			StopsInfo.Side getStopSide(IEnumerable<LaneInfo> lanes) =>
				lanes.All(x => x.Position < lane.Position) ? StopsInfo.Side.Left :
				lanes.All(x => x.Position > lane.Position) ? StopsInfo.Side.Right :
				StopsInfo.Side.Both;
		}
	}

	public static VehicleInfo.VehicleType GetStopType(LaneInfo lane, LaneType type)
	{
		if (type is LaneType.Car or LaneType.Bus)
			return lane.Stops.StopType & VehicleInfo.VehicleType.Car;

		if (type is LaneType.Tram)
			return lane.Stops.StopType & VehicleInfo.VehicleType.Tram;

		if (type is LaneType.Trolley)
			return lane.Stops.StopType & VehicleInfo.VehicleType.Trolleybus;

		return lane.Stops.StopType;
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

		if (lane.RightLane?.Type is LaneType.Parking or LaneType.Empty || (lane.RightLane?.Decorations.HasFlag(LaneDecoration.BusBay) ?? false))
		{
			return lane.RightLane.LaneWidth;
		}
		else if (lane.LeftLane?.Type is LaneType.Parking or LaneType.Empty || (lane.LeftLane?.Decorations.HasFlag(LaneDecoration.BusBay) ?? false))
		{
			return -lane.LeftLane.LaneWidth;
		}

		return 0F;
	}
}