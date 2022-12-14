using AdaptiveRoads.Manager;

using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props;

public static partial class LanePropsUtil
{
	private static IEnumerable<NetLaneProps.Prop> GetWirePoleProps(LaneInfo lane, RoadInfo road)
	{
		if (!road.ContainsWiredLanes)
			yield break;

		getLaneTramInfo(lane, road, out var tramLanesAreNextToMedians, out var leftTram, out var rightTram);

		var angle = !leftTram ? 0 : 180;

		var position = 0F;//lane.Tags.HasFlag(LaneTag.Asphalt) ? 0 : lane.Position < 0 ? 1F : -1F;
		var verticalOffset = lane.Tags.HasFlag(LaneTag.Asphalt) ? -0.45F : -0.75F;

		PropInfo poleProp;

		if (!tramLanesAreNextToMedians && lane.Tags.HasFlag(LaneTag.WirePoleLane))
		{
			poleProp = GetProp(Prop.TramPole);

			verticalOffset = lane.Tags.HasFlag(LaneTag.Asphalt) ? -0.1F : -0.4F;
		}
		else if (leftTram && rightTram)
		{
			if (lane.LaneWidth > 4F)
			{
				poleProp = GetProp(Prop.TramSidePole);

				position = -(float)Math.Round(Math.Max(0F, lane.LaneWidth - 4F) / 2F, 3);

				yield return pole(-1F);
				yield return pole(0F);
				yield return pole(1F, true);

				position *= -1;
				angle = 0;
			}
			else
			{
				poleProp = GetProp(Prop.TramCenterPole);
			}
		}
		else if (leftTram || rightTram)
		{
			var tramLane = leftTram ? lane.LeftLane : lane.RightLane;
			var nextLane = leftTram ? tramLane?.LeftLane : tramLane?.RightLane;

			if (nextLane?.Type.HasAnyFlag(LaneType.Filler, LaneType.Curb, LaneType.Pedestrian) ?? false)
			{
				getLaneTramInfo(nextLane, road, out _, out var l, out var r);

				if ((l && r) || (l && rightTram))
					yield break;
			}

			if (rightTram && nextLane != null && ((nextLane.Type & LaneType.Tram) != 0))
			{
				var nextNextLane = leftTram ? nextLane?.LeftLane : nextLane?.RightLane;

				if (nextNextLane?.Type.HasAnyFlag(LaneType.Filler, LaneType.Curb, LaneType.Pedestrian) ?? false)
					yield break;
			}

			poleProp = GetProp(Prop.TramSidePole);
			position = nextLane != null && nextLane.Type.HasFlag(LaneType.Tram)
				? (float)Math.Round(Math.Max(0F, (lane.Type == LaneType.Curb ? 0.1F : 0F) + ((lane.LaneWidth - 1F) / 2F)), 3) * (leftTram ? -1F : 1F)
				: (float)Math.Round(Math.Max(0F, lane.LaneWidth - 4F) / 2F, 3) * (leftTram ? -1F : 1F);
		}
		else
		{
			yield break;
		}

		yield return pole(-1F);
		yield return pole(0F);
		yield return pole(1F, true);

		NetLaneProps.Prop pole(float segment, bool end = false)
		{
			return new NetLaneProps.Prop
			{
				m_prop = poleProp,
				m_finalProp = poleProp,
				m_endFlagsForbidden = end ? NetNode.Flags.Middle : NetNode.Flags.None,
				m_cornerAngle = 0.75F,
				m_minLength = segment != 0 || verticalOffset == -0.2F ? 0F : 22F,
				m_repeatDistance = 0F,
				m_segmentOffset = segment,
				m_angle = angle,
				m_probability = 100,
				m_position = new Vector3(position, verticalOffset, 0)
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags { Forbidden = RoadUtils.S_RemoveTramSupports }
			});
		}

		static void getLaneTramInfo(LaneInfo lane, RoadInfo road, out bool tramLanesAreNextToMedians, out bool leftTram, out bool rightTram)
		{
			tramLanesAreNextToMedians = road.WiredLanesAreNextToMedians /*&& road.AsphaltWidth > 10F*/;
			leftTram = tramLanesAreNextToMedians && lane.LeftLane != null && ((lane.LeftLane.Type & (LaneType.Tram | LaneType.Trolley)) != 0);
			rightTram = tramLanesAreNextToMedians && lane.RightLane != null && ((lane.RightLane.Type & (LaneType.Tram | LaneType.Trolley)) != 0);
		}
	}

	private static IEnumerable<NetLaneProps.Prop> GetParkingProps(LaneInfo lane)
	{
		if (lane.LeftLane?.Type == LaneType.Parking)
		{
			var parkingMeter = GetProp(Prop.ParkingMeter);

			yield return new NetLaneProps.Prop
			{
				m_prop = parkingMeter,
				m_finalProp = parkingMeter,
				m_angle = 90,
				m_minLength = 22,
				m_segmentOffset = -0.65F,
				m_probability = 100,
				m_position = new Vector3((float)Math.Round((lane.LaneWidth - (lane.Tags.HasFlag(LaneTag.Sidewalk) ? 1F : 1.4F)) / -2, 3), 0, 4F),
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{ Forbidden = RoadUtils.S_RemoveRoadClutter }
			});
		}

		if (lane.RightLane?.Type == LaneType.Parking)
		{
			var parkingMeter = GetProp(Prop.ParkingMeter);

			yield return new NetLaneProps.Prop
			{
				m_prop = parkingMeter,
				m_finalProp = parkingMeter,
				m_angle = 270,
				m_minLength = 22,
				m_segmentOffset = 0.65F,
				m_probability = 100,
				m_position = new Vector3((float)Math.Round((lane.LaneWidth - (lane.Tags.HasFlag(LaneTag.Sidewalk) ? 1F : 1.4F)) / 2, 3), 0, -4F),
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{ Forbidden = RoadUtils.S_RemoveRoadClutter }
			});
		}
	}

	private static IEnumerable<NetLaneProps.Prop> GetLights(LaneInfo lane, RoadInfo road)
	{
		if (road.Lanes.Any(x => x.Decorations.HasAnyFlag(LaneDecoration.StreetLight, LaneDecoration.StreetLight)))
			return new NetLaneProps.Prop[0];

		if (lane.Tags.HasFlag(LaneTag.CenterMedian))
		{
			if (!lane.Decorations.HasFlag(LaneDecoration.DoubleStreetLight))
			{
				return GetStreetLights(lane, road, LaneDecoration.DoubleStreetLight);
			}
		}
		else if (lane.Type == LaneType.Curb)
		{
			if ((!road.ContainsCenterMedian || road.AsphaltWidth >= 30F)
				&& (road.ContainsCenterMedian || road.AsphaltWidth >= 20F || lane.Position >= 0)
				&& !lane.Decorations.HasFlag(LaneDecoration.StreetLight))
			{
				return GetStreetLights(lane, road, LaneDecoration.StreetLight);
			}
		}

		return new NetLaneProps.Prop[0];
	}
}
