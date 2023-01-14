using AdaptiveRoads.Data.NetworkExtensions;
using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain;

using System.Collections.Generic;
using System.Reflection;

namespace BlankRoadBuilder.Util;
public static class RoadUtils
{
	public const NetSegmentExt.Flags S_LowCurbOnTheRight = NetSegmentExt.Flags.Custom0;
	public const NetSegmentExt.Flags S_LowCurbOnTheLeft = NetSegmentExt.Flags.Custom1;
	public const NetSegmentExt.Flags S_AddRoadDamage = NetSegmentExt.Flags.Custom2;
	public const NetSegmentExt.Flags S_RemoveRoadClutter = NetSegmentExt.Flags.Custom3;
	public const NetSegmentExt.Flags S_RemoveTramSupports = NetSegmentExt.Flags.Custom4;
	public const NetSegmentExt.Flags S_RemoveMarkings = NetSegmentExt.Flags.Custom5;
	public const NetSegmentExt.Flags S_Curbless = NetSegmentExt.Flags.Custom7;

	public const NetNodeExt.Flags N_FullLowCurb = NetNodeExt.Flags.Custom0;
	public const NetNodeExt.Flags N_ForceHighCurb = NetNodeExt.Flags.Custom1;
	public const NetNodeExt.Flags N_RemoveLaneArrows = NetNodeExt.Flags.Custom2;
	public const NetNodeExt.Flags N_RemoveTramWires = NetNodeExt.Flags.Custom3;
	public const NetNodeExt.Flags N_RemoveTramTracks = NetNodeExt.Flags.Custom4;
	public const NetNodeExt.Flags N_HideTreesCloseToIntersection = NetNodeExt.Flags.Custom5;
	public const NetNodeExt.Flags N_Nodeless = NetNodeExt.Flags.Custom7;

	public const NetLaneExt.Flags L_RemoveTrees = NetLaneExt.Flags.Custom1;
	public const NetLaneExt.Flags L_RemoveStreetLights = NetLaneExt.Flags.Custom2;

	public const NetLaneExt.Flags L_RemoveTramTracks = NetLaneExt.Flags.Custom3;
	public const NetLaneExt.Flags L_TramTracks_1 = NetLaneExt.Flags.Custom4;
	public const NetLaneExt.Flags L_TramTracks_2 = NetLaneExt.Flags.Custom5;

	public const NetLaneExt.Flags L_RemoveBarrier = NetLaneExt.Flags.Custom3;
	public const NetLaneExt.Flags L_Barrier_1 = NetLaneExt.Flags.Custom4;
	public const NetLaneExt.Flags L_Barrier_2 = NetLaneExt.Flags.Custom5;
	public const NetLaneExt.Flags L_Barrier_3 = NetLaneExt.Flags.Custom6;
	public const NetLaneExt.Flags L_Barrier_4 = NetLaneExt.Flags.Custom7;

	public const NetSegmentExt.Flags S_AnyStop = NetSegmentExt.Flags.Expression0;
	public const NetSegmentEnd.Flags S_HighCurb = NetSegmentEnd.Flags.Expression1;
	public const LaneTransition.Flags T_Markings = LaneTransition.Flags.Expression0;
	public const NetNodeExt.Flags N_FlatTransition = NetNodeExt.Flags.Expression0;
	public const NetNodeExt.Flags N_Asym = NetNodeExt.Flags.Expression2;
	public const NetNodeExt.Flags N_AsymInverted = NetNodeExt.Flags.Expression3;

	public static Dictionary<ElevationType, NetInfo> GetElevations(this NetInfo ground)
	{
		var dic = new Dictionary<ElevationType, NetInfo>();

		if (ground == null)
		{
			return dic;
		}

		dic.Add(ElevationType.Basic, ground);

		var elevated = AssetEditorRoadUtils.TryGetElevated(ground);
		if (elevated != null)
		{
			dic.Add(ElevationType.Elevated, elevated);
		}

		var bridge = AssetEditorRoadUtils.TryGetBridge(ground);
		if (bridge != null)
		{
			dic.Add(ElevationType.Bridge, bridge);
		}

		var slope = AssetEditorRoadUtils.TryGetSlope(ground);
		if (slope != null)
		{
			dic.Add(ElevationType.Slope, slope);
		}

		var tunnel = AssetEditorRoadUtils.TryGetTunnel(ground);
		if (tunnel != null)
		{
			dic.Add(ElevationType.Tunnel, tunnel);
		}

		return dic;
	}

	public static void SetNetAi(NetInfo info, string fieldName, object? value)
	{
		if (info == null)
			return;

		var component = info.GetComponent<NetAI>();

		if (component == null)
			return;

		var field = component.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);

		if (field == null)
			return;

		field.SetValue(component, value);
	}
}
