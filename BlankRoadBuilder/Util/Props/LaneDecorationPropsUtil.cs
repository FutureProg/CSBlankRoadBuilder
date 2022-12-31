using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;

using UnityEngine;

using static AdaptiveRoads.Manager.NetInfoExtionsion;

namespace BlankRoadBuilder.Util.Props;

public static partial class LanePropsUtil
{
	private static IEnumerable<NetLaneProps.Prop> GetDecorationProps(LaneInfo lane, RoadInfo road)
	{
		foreach (var deco in lane.Decorations.GetValues())
		{
			switch (deco)
			{
				case LaneDecoration.Grass:
					if (lane.LaneWidth >= 2 && ModOptions.AddGrassPropsToGrassLanes)
					{
						foreach (var prop in GetGrassProps(lane))
						{
							yield return prop;
						}
					}

					break;
				case LaneDecoration.Tree:
					foreach (var prop in GetTrees(lane))
					{
						yield return prop;
					}

					break;
				case LaneDecoration.Benches:
					foreach (var prop in GetBenches(lane))
					{
						yield return prop;
					}

					break;
				case LaneDecoration.FlowerPots:
					foreach (var prop in GetFlowerPots(lane))
					{
						yield return prop;
					}

					break;
				case LaneDecoration.Bollard:
					foreach (var prop in GetBollards(lane))
					{
						yield return prop;
					}

					break;
				case LaneDecoration.Hedge:
					foreach (var prop in GetHedge(lane))
					{
						yield return prop;
					}

					break;
				case LaneDecoration.StreetLight:
				case LaneDecoration.DoubleStreetLight:
					foreach (var prop in GetStreetLights(lane, road, deco))
					{
						yield return prop;
					}

					break;
				case LaneDecoration.BikeParking:
					foreach (var prop in GetBikeParking(lane))
					{
						yield return prop;
					}

					break;
				case LaneDecoration.TrashBin:
					foreach (var prop in GetTrashBin(lane))
					{
						yield return prop;
					}

					break;
				case LaneDecoration.StreetAds:
					foreach (var prop in GetStreetAds(lane))
					{
						yield return prop;
					}

					break;
			}
		}
	}

	private static IEnumerable<NetLaneProps.Prop> GetBikeParking(LaneInfo lane)
	{
		var prop = GetProp(Prop.BicycleParking);

		yield return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_finalProp = prop,
			m_probability = 100,
			m_angle = propAngle(lane) * 90,
			m_repeatDistance = lane.Decorations.HasFlag(LaneDecoration.Benches) ? 15F : 30F,
			m_position = new Vector3(0, 0, lane.Decorations.HasFlag(LaneDecoration.Benches) ? -5F : 1F)
		}.Extend(prop => new LaneProp(prop) { JunctionDistance = 15F });
	}

	private static IEnumerable<NetLaneProps.Prop> GetTrashBin(LaneInfo lane)
	{
		var prop = GetProp(Prop.TrashBin);

		yield return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_finalProp = prop,
			m_probability = 100,
			m_angle = propAngle(lane) * 90,
			m_repeatDistance = 15F,
			m_position = new Vector3(0, 0, 1.5F)
		}.Extend(prop => new LaneProp(prop) { JunctionDistance = 15F });
	}

	private static IEnumerable<NetLaneProps.Prop> GetStreetAds(LaneInfo lane)
	{
		var prop = GetProp(Prop.StreetAd);
		var hasOtherDecos = lane.Decorations.HasAnyFlag(LaneDecoration.Benches, LaneDecoration.FlowerPots);

		yield return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_finalProp = prop,
			m_probability = 100,
			m_angle = 0,
			m_repeatDistance = 25F,
			m_position = new Vector3(hasOtherDecos ? propAngle(lane) * lane.LaneWidth / 2 : 0, 0, 0)
		}.Extend(prop => new LaneProp(prop) { JunctionDistance = 5F });
	}

	private static IEnumerable<NetLaneProps.Prop> GetStreetLights(LaneInfo lane, RoadInfo road, LaneDecoration decoration)
	{
		var lightProp = GetProp(decoration == LaneDecoration.DoubleStreetLight ? Prop.DoubleStreetLight : Prop.SingleStreetLight);
		var xPos = decoration == LaneDecoration.DoubleStreetLight ? 0 : (-lane.LaneWidth / 2) + 0.5F;

		yield return getLight(road.ContainsWiredLanes ? 2F : 0F);

		if (ModOptions.VanillaStreetLightPlacement)
		{
			yield break;
		}

		for (var i = 24; i <= 24 * 4; i += 24)
		{
			yield return getLight(i);
			yield return getLight(-i);
		}

		NetLaneProps.Prop getLight(float position) => new NetLaneProps.Prop
		{
			m_prop = lightProp,
			m_finalProp = lightProp,
			m_minLength = ModOptions.VanillaTreePlacement ? 10 : ((Math.Abs(position) * 2F) + 10F),
			m_repeatDistance = ModOptions.VanillaTreePlacement ? 24 : 0,
			m_probability = 100,
			m_position = new Vector3(xPos, 0, position)
		}.Extend(prop => new LaneProp(prop)
		{
			LaneFlags = new LaneInfoFlags
			{ Forbidden = RoadUtils.L_RemoveStreetLights }
		}).ToggleForwardBackward(propAngle(lane) < 0);
	}

	private static IEnumerable<NetLaneProps.Prop> GetBenches(LaneInfo lane)
	{
		var position = lane.Type != LaneType.Curb ? 0 : lane.Direction == LaneDirection.Both && lane.Position < 0 ? -0.25F : 0.25F;
		var prop = GetProp(Prop.Bench);

		yield return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_finalProp = prop,
			m_probability = 100,
			m_angle = propAngle(lane) * 90,
			m_repeatDistance = 15F,
			m_position = new Vector3(position, 0, 0)
		}.Extend(prop => new LaneProp(prop) { JunctionDistance = 15F });
	}

	private static IEnumerable<NetLaneProps.Prop> GetHedge(LaneInfo lane)
	{
		var position = propAngle(lane) * 0.05F;
		var prop = GetProp(Prop.Hedge);

		if (lane.Decorations != LaneDecoration.Hedge)
		{
			var hasOtherDecos = lane.Decorations.HasFlag(LaneDecoration.Bollard) && lane.Decorations.HasAnyFlag(LaneDecoration.Benches, LaneDecoration.FlowerPots, LaneDecoration.StreetAds, LaneDecoration.BikeParking);

			position = propAngle(lane) * ((-lane.LaneWidth / 2) + (hasOtherDecos ? 0.75F : 0.55F));
		}

		yield return new NetLaneProps.Prop
		{
			m_tree = prop,
			m_finalTree = prop,
			m_prop = prop,
			m_finalProp = prop,
			m_probability = 100,
			m_repeatDistance = 3.25F,
			m_position = new Vector3(position, 0, 0)
		}.Extend(prop => new LaneProp(prop) { JunctionDistance = 3.25F });
	}

	private static IEnumerable<NetLaneProps.Prop> GetBollards(LaneInfo lane)
	{
		var prop = GetProp(Prop.Bollard);
		var hasOtherDecos = lane.Decorations.HasAnyFlag(LaneDecoration.Benches, LaneDecoration.FlowerPots, LaneDecoration.StreetAds, LaneDecoration.BikeParking);

		yield return new NetLaneProps.Prop
		{
			m_tree = prop,
			m_finalTree = prop,
			m_prop = prop,
			m_finalProp = prop,
			m_probability = 100,
			m_repeatDistance = 2F,
			m_position = new Vector3(hasOtherDecos ? propAngle(lane) * lane.LaneWidth / -2 : 0, 0.01F, 0)
		}.Extend(prop => new LaneProp(prop) { JunctionDistance = 2F });
	}

	private static IEnumerable<NetLaneProps.Prop> GetFlowerPots(LaneInfo lane)
	{
		var prop = GetProp(Prop.FlowerPot);

		if (lane.Decorations.HasFlag(LaneDecoration.Benches))
		{
			yield return new NetLaneProps.Prop
			{
				m_prop = prop,
				m_finalProp = prop,
				m_probability = 100,
				m_angle = propAngle(lane) * 90,
				m_repeatDistance = 15F,
				m_position = new Vector3(0, 0, lane.Decorations.HasFlag(LaneDecoration.TrashBin) ? 2.75F : 2F)
			}.Extend(prop => new LaneProp(prop) { JunctionDistance = 15F });

			yield return new NetLaneProps.Prop
			{
				m_prop = prop,
				m_finalProp = prop,
				m_probability = 100,
				m_angle = propAngle(lane) * 90,
				m_repeatDistance = 15F,
				m_position = new Vector3(0, 0, -2F)
			}.Extend(prop => new LaneProp(prop) { JunctionDistance = 15F });

			yield break;
		}

		yield return new NetLaneProps.Prop
		{
			m_tree = prop,
			m_finalTree = prop,
			m_prop = prop,
			m_finalProp = prop,
			m_probability = 100,
			m_angle = propAngle(lane) * 90,
			m_repeatDistance = 5F,
			m_position = new Vector3(0, 0, 0)
		}.Extend(prop => new LaneProp(prop) { JunctionDistance = 5F });
	}

	public static IEnumerable<NetLaneProps.Prop> GetGrassProps(LaneInfo lane)
	{
		var prop = GetProp(Prop.Grass);
		var odd = (int)lane.LaneWidth % 2 == 1;
		var numLines = Math.Max((int)Math.Ceiling(lane.LaneWidth) - 1, 1);
		var pos = numLines == 1 ? (0) : (1 - (lane.LaneWidth / 2));

		for (var i = 0; i < numLines; i++)
		{
			if (i > 0)
			{
				pos += (lane.LaneWidth - 2) / (numLines - 1);
			}

			yield return new NetLaneProps.Prop
			{
				m_tree = prop,
				m_finalTree = prop,
				m_prop = prop,
				m_finalProp = prop,
				m_probability = 100,
				m_repeatDistance = 1.25F,
				m_position = new Vector3(pos, -0.4F, 0)
			}.Extend(prop => new LaneProp(prop)
			{
				JunctionDistance = 2.5F,
				VanillaSegmentFlags = new VanillaSegmentInfoFlags
				{
					Forbidden = lane.Decorations.HasFlag(LaneDecoration.TransitStop) ? NetSegment.Flags.StopAll : NetSegment.Flags.None
				},
				SegmentFlags = new SegmentInfoFlags
				{
					Required = !ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.ANFillers, MarkingsSource.IMTMarkings) &&(ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenANMarkings)) ? RoadUtils.S_RemoveMarkings : NetSegmentExt.Flags.None,
					Forbidden = !ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.ANFillers, MarkingsSource.IMTMarkings) && !(ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenANMarkings)) ? RoadUtils.S_RemoveMarkings : NetSegmentExt.Flags.None,
				}
			});
		}
	}

	public static IEnumerable<NetLaneProps.Prop> GetTrees(LaneInfo lane)
	{
		var tree = GetProp(Prop.Tree);
		var planter = GetProp(Prop.TreePlanter);
		var hasOtherDecos = lane.Decorations.HasAnyFlag(LaneDecoration.Benches, LaneDecoration.FlowerPots);

		if (ModOptions.VanillaTreePlacement)
		{
			yield return getTree(2);

			if (planter != null && !lane.Decorations.HasAnyFlag(LaneDecoration.Grass, LaneDecoration.Gravel))
				yield return getPlanter(2);

			yield break;
		}

		for (var i = 6; i <= 6 + (12 * 7); i += 12)
		{
			yield return getTree(i);
			yield return getTree(-i);

			if (planter != null && !lane.Decorations.HasAnyFlag(LaneDecoration.Grass, LaneDecoration.Gravel))
			{
				yield return getPlanter(i);
				yield return getPlanter(-i);
			}
		}

		NetLaneProps.Prop getTree(float position) => new NetLaneProps.Prop
		{
			m_tree = tree,
			m_finalTree = tree,
			m_minLength = ModOptions.VanillaTreePlacement ? 10 : ((Math.Abs(position) * 2F) + 10F),
			m_upgradable = true,
			m_probability = 100,
			m_repeatDistance = ModOptions.VanillaTreePlacement ? 12 : 0,
			m_position = new Vector3(hasOtherDecos ? propAngle(lane) * -Math.Min(1, lane.LaneWidth / 2) : 0, 0, position)
		}.Extend(prop => new LaneProp(prop)
		{
			EndNodeFlags = new NodeInfoFlags
			{ Forbidden = position > 0 && lane.Tags.HasFlag(LaneTag.Asphalt) && !ModOptions.VanillaTreePlacement ? RoadUtils.N_HideTreesCloseToIntersection : NetNodeExt.Flags.None },
			StartNodeFlags = new NodeInfoFlags
			{ Forbidden = position < 0 && lane.Tags.HasFlag(LaneTag.Asphalt) && !ModOptions.VanillaTreePlacement ? RoadUtils.N_HideTreesCloseToIntersection : NetNodeExt.Flags.None },
			LaneFlags = new LaneInfoFlags
			{ Forbidden = RoadUtils.L_RemoveTrees },
			VanillaSegmentFlags = new VanillaSegmentInfoFlags
			{ Forbidden = Math.Abs(position) <= 18 && !ModOptions.VanillaTreePlacement ? NetSegment.Flags.StopAll : NetSegment.Flags.None }
		});

		NetLaneProps.Prop getPlanter(float position) => new NetLaneProps.Prop
		{
			m_prop = planter,
			m_finalProp = planter,
			m_minLength = ModOptions.VanillaTreePlacement ? 10 : ((Math.Abs(position) * 2F) + 10F),
			m_upgradable = true,
			m_probability = 100,
			m_repeatDistance = ModOptions.VanillaTreePlacement ? 12 : 0,
			m_position = new Vector3((hasOtherDecos ? propAngle(lane) * -Math.Min(1, lane.LaneWidth / 2) : 0) + (propAngle(lane) * 0.01F), 0, position)
		}.Extend(prop => new LaneProp(prop)
		{
			EndNodeFlags = new NodeInfoFlags
			{ Forbidden = position > 0 && lane.Tags.HasFlag(LaneTag.Asphalt) && !ModOptions.VanillaTreePlacement ? RoadUtils.N_HideTreesCloseToIntersection : NetNodeExt.Flags.None },
			StartNodeFlags = new NodeInfoFlags
			{ Forbidden = position < 0 && lane.Tags.HasFlag(LaneTag.Asphalt) && !ModOptions.VanillaTreePlacement ? RoadUtils.N_HideTreesCloseToIntersection : NetNodeExt.Flags.None },
			LaneFlags = new LaneInfoFlags
			{ Forbidden = RoadUtils.L_RemoveTrees },
			VanillaSegmentFlags = new VanillaSegmentInfoFlags
			{ Forbidden = Math.Abs(position) <= 18 && !ModOptions.VanillaTreePlacement ? NetSegment.Flags.StopAll : NetSegment.Flags.None }
		});
	}

	private static int propAngle(LaneInfo lane)
	{
		return (lane.Position < 0 ? -1 : 1) * (lane.PropAngle == PropAngle.Right == (lane.Direction != LaneDirection.Backwards || lane.Type == LaneType.Curb) ? 1 : -1);
	}
}
