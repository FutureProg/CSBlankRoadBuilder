﻿using AdaptiveRoads.Manager;

using BlankRoadBuilder.ThumbnailMaker;

using PrefabMetadata.Helpers;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props;

public static partial class LanePropsUtil
{
	private static IEnumerable<NetLaneProps.Prop> GetWirePoleProps(LaneInfo lane, RoadInfo road)
	{
		if (!road.ContainsWiredLanes)
			yield break;

		var tramLanesAreNextToMedians = road.WiredLanesAreNextToMedians && road.Width > 10F;
		var leftTram = tramLanesAreNextToMedians && lane.LeftLane != null && ((lane.LeftLane.Type & (LaneType.Tram | LaneType.Trolley)) != 0);
		var rightTram = tramLanesAreNextToMedians && lane.RightLane != null && ((lane.RightLane.Type & (LaneType.Tram | LaneType.Trolley)) != 0);

		var angle = !leftTram ? 0 : 180;

		var position = lane.Tags.HasFlag(LaneTag.Asphalt) ? 0 : lane.Position < 0 ? 1F : -1F;
		var verticalOffset = lane.Tags.HasFlag(LaneTag.Asphalt) ? -0.45F : -0.75F;

		PropInfo poleProp;

		if (!tramLanesAreNextToMedians && lane.Tags.HasFlag(LaneTag.WirePoleLane))
		{
			poleProp = Prop("Tram Pole Side");

			if (lane.Tags.HasFlag(LaneTag.Sidewalk))
				verticalOffset = -0.2F;
		}
		else if (leftTram && rightTram)
		{
			if (lane.Width > 4F)
			{
				poleProp = Prop("Tram Pole Wide Side");

				position = -(float)Math.Round(Math.Max(0F, lane.Width - 4F) / 2F, 3);

				yield return pole(-1F);
				yield return pole(0F);
				yield return pole(1F, true);

				position *= -1;
				angle = 0;
			}
			else
			{
				poleProp = Prop("Tram Pole Center");
			}
		}
		else if (leftTram || rightTram)
		{
			var tramLane = leftTram ? lane.LeftLane : lane.RightLane;
			var nextLane = leftTram ? tramLane?.LeftLane : tramLane?.RightLane;

			if ((nextLane?.IsFiller() ?? false) || (nextLane?.Type.HasFlag(LaneType.Pedestrian) ?? false))
				yield break;

			if (rightTram && nextLane != null && ((nextLane.Type & LaneType.Tram) != 0))
			{
				var nextNextLane = leftTram ? nextLane?.LeftLane : nextLane?.RightLane;

				if ((nextNextLane?.IsFiller() ?? false) || (nextNextLane?.Type.HasFlag(LaneType.Pedestrian) ?? false))
					yield break;
			}

			poleProp = Prop("Tram Pole Wide Side");
			position = nextLane != null && (nextLane.Type & LaneType.Tram) != 0
				? (float)Math.Round(Math.Max(0F, lane.Tags.HasFlag(LaneTag.Sidewalk) ? (-1.1 - road.BufferSize / 2F) : (lane.Width - 1F)) / 2F, 3) * (leftTram ? -1F : 1F)
				: (float)Math.Round(Math.Max(0F, lane.Width - 4F) / 2F, 3) * (leftTram ? -1F : 1F);
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
	}

	private static IEnumerable<NetLaneProps.Prop> GetParkingProps(LaneInfo lane)
	{
		if (lane.LeftLane?.Type == LaneType.Parking)
		{
			var parkingMeter = Prop("Parking Meter");

			yield return new NetLaneProps.Prop
			{
				m_prop = parkingMeter,
				m_finalProp = parkingMeter,
				m_angle = 90,
				m_minLength = 22,
				m_segmentOffset = -0.65F,
				m_probability = 100,
				m_position = new Vector3((float)Math.Round((lane.Width - (lane.Tags.HasFlag(LaneTag.Sidewalk) ? 1F : 1.4F)) / -2, 3), 0, 4F),
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{ Forbidden = RoadUtils.S_RemoveRoadClutter }
			});
		}

		if (lane.RightLane?.Type == LaneType.Parking)
		{
			var parkingMeter = Prop("Parking Meter");

			yield return new NetLaneProps.Prop
			{
				m_prop = parkingMeter,
				m_finalProp = parkingMeter,
				m_angle = 270,
				m_minLength = 22,
				m_segmentOffset = 0.65F,
				m_probability = 100,
				m_position = new Vector3((float)Math.Round((lane.Width - (lane.Tags.HasFlag(LaneTag.Sidewalk) ? 1F : 1.4F)) / 2, 3), 0, -4F),
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{ Forbidden = RoadUtils.S_RemoveRoadClutter }
			});
		}
	}

	private static IEnumerable<NetLaneProps.Prop> GetTrees()
	{
		var tree = PrefabCollection<TreeInfo>.FindLoaded("mp9-YoungLinden");

		for (var i = 6; i <= 6 + 12 * 7; i += 12)
		{
			yield return getTree(i, false);
			yield return getTree(-i, false);
			yield return getTree(i, true);
			yield return getTree(-i, true);
		}

		NetLaneProps.Prop getTree(float position, bool flag) => new NetLaneProps.Prop
		{
			m_tree = tree,
			m_finalTree = tree,
			m_startFlagsForbidden = position < 0 && !flag ? NetNode.Flags.Junction : NetNode.Flags.None,
			m_endFlagsForbidden = position > 0 && !flag ? NetNode.Flags.Junction : NetNode.Flags.None,
			m_startFlagsRequired = position < 0 && flag ? NetNode.Flags.Junction : NetNode.Flags.None,
			m_endFlagsRequired = position > 0 && flag ? NetNode.Flags.Junction : NetNode.Flags.None,
			m_minLength = Math.Abs(position) * 2F + 10F,
			m_upgradable = true,
			m_probability = 100,
			m_position = new Vector3(0, 0, position)
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			EndNodeFlags = new NetInfoExtionsion.NodeInfoFlags
			{ Required = position > 0 && flag ? RoadUtils.N_ShowTreesCloseToIntersection : NetNodeExt.Flags.None },
			StartNodeFlags = new NetInfoExtionsion.NodeInfoFlags
			{ Required = position < 0 && flag ? RoadUtils.N_ShowTreesCloseToIntersection : NetNodeExt.Flags.None },
			LaneFlags = new NetInfoExtionsion.LaneInfoFlags
			{ Forbidden = RoadUtils.L_RemoveTrees },
			VanillaSegmentFlags = new NetInfoExtionsion.VanillaSegmentInfoFlags
			{ Forbidden = Math.Abs(position) <= 18 ? NetSegment.Flags.StopAll : NetSegment.Flags.None }
		});
	}

	private static IEnumerable<NetLaneProps.Prop> GetLights(LaneInfo lane, RoadInfo road)
	{
		PropInfo lightProp;
		var xPos = 0F;

		if (lane.Tags.HasFlag(LaneTag.CenterMedian) && !lane.Tags.HasFlag(LaneTag.SecondaryCenterMedian))
		{
			lightProp = Prop("Toll Road Light Double");//Prop("Avenue Light");
		}
		else if (lane.Tags.HasFlag(LaneTag.Sidewalk))
		{
			if (road.ContainsCenterMedian && road.Width < 30F)
				yield break;

			if (!road.ContainsCenterMedian && road.Width < 20F && lane.Position < 0)
				yield break;

			xPos = -1F;
			lightProp = Prop("Toll Road Light Single");//Prop("New Street Light Avenue");
		}
		else
			yield break;

		yield return getLight(road.ContainsWiredLanes ? 2F : 0F);

		for (var i = 24; i <= 24 * 4; i += 24)
		{
			yield return getLight(i);
			yield return getLight(-i);
		}

		NetLaneProps.Prop getLight(float position) => new NetLaneProps.Prop
		{
			m_prop = lightProp,
			m_finalProp = lightProp,
			m_minLength = Math.Abs(position) * 2F + 10F,
			m_probability = 100,
			m_position = new Vector3(xPos, 0, position)
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			LaneFlags = new NetInfoExtionsion.LaneInfoFlags
			{ Forbidden = RoadUtils.L_RemoveStreetLights }
		}).ToggleForwardBackward(lane.Position < 0 && lane.Tags.HasFlag(LaneTag.Sidewalk));
	}
}