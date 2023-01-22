using AdaptiveRoads.Data.NetworkExtensions;
using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain;

using System.Collections.Generic;
using System.Reflection;

namespace BlankRoadBuilder.Util;

public class AnFlags
{
	public NetSegmentExt.Flags S_LowCurbOnTheRight = NetSegmentExt.Flags.Custom0;
	public NetSegmentExt.Flags S_LowCurbOnTheLeft = NetSegmentExt.Flags.Custom1;
	public NetSegmentExt.Flags S_AddRoadDamage = NetSegmentExt.Flags.Custom2;
	public NetSegmentExt.Flags S_RemoveRoadClutter = NetSegmentExt.Flags.Custom3;
	public NetSegmentExt.Flags S_RemoveTramSupports = NetSegmentExt.Flags.Custom4;
	public NetSegmentExt.Flags S_RemoveMarkings = NetSegmentExt.Flags.Custom5;
	public NetSegmentExt.Flags S_Curbless = NetSegmentExt.Flags.Custom7;

	public NetNodeExt.Flags N_FullLowCurb = NetNodeExt.Flags.Custom0;
	public NetNodeExt.Flags N_ForceHighCurb = NetNodeExt.Flags.Custom1;
	public NetNodeExt.Flags N_RemoveLaneArrows = NetNodeExt.Flags.Custom2;
	public NetNodeExt.Flags N_RemoveTramWires = NetNodeExt.Flags.Custom3;
	public NetNodeExt.Flags N_RemoveTramTracks = NetNodeExt.Flags.Custom4;
	public NetNodeExt.Flags N_HideTreesCloseToIntersection = NetNodeExt.Flags.Custom5;
	public NetNodeExt.Flags N_Nodeless = NetNodeExt.Flags.Custom7;

	public NetLaneExt.Flags L_RemoveTrees = NetLaneExt.Flags.Custom1;
	public NetLaneExt.Flags L_RemoveStreetLights = NetLaneExt.Flags.Custom2;

	public NetLaneExt.Flags L_RemoveTramTracks = NetLaneExt.Flags.Custom3;
	public NetLaneExt.Flags L_TramTracks_1 = NetLaneExt.Flags.Custom4;
	public NetLaneExt.Flags L_TramTracks_2 = NetLaneExt.Flags.Custom5;

	public NetLaneExt.Flags L_RemoveBarrier = NetLaneExt.Flags.Custom3;
	public NetLaneExt.Flags L_Barrier_1 = NetLaneExt.Flags.Custom4;
	public NetLaneExt.Flags L_Barrier_2 = NetLaneExt.Flags.Custom5;
	public NetLaneExt.Flags L_Barrier_3 = NetLaneExt.Flags.Custom6;
	public NetLaneExt.Flags L_Barrier_4 = NetLaneExt.Flags.Custom7;

	public NetSegmentExt.Flags S_AnyStop = NetSegmentExt.Flags.Expression0;
	public NetSegmentExt.Flags S_StepForward = NetSegmentExt.Flags.Expression1;
	public NetSegmentExt.Flags S_StepBackward = NetSegmentExt.Flags.Expression2;
	public NetSegmentExt.Flags S_Asym = NetSegmentExt.Flags.Expression3;
	public NetSegmentExt.Flags S_AsymInverted = NetSegmentExt.Flags.Expression4;

	public NetSegmentEnd.Flags S_HighCurb = NetSegmentEnd.Flags.Expression0;
	public NetSegmentEnd.Flags S_IsTailNode = NetSegmentEnd.Flags.Expression1;
	public NetSegmentEnd.Flags N_Asym = NetSegmentEnd.Flags.Expression2;
	public NetSegmentEnd.Flags N_AsymInverted = NetSegmentEnd.Flags.Expression3;

	public LaneTransition.Flags T_Markings = LaneTransition.Flags.Expression0;
	public LaneTransition.Flags T_TrolleyWires = LaneTransition.Flags.Expression1;

	public NetNodeExt.Flags N_FlatTransition = NetNodeExt.Flags.Expression0;
}

public static class RoadUtils
{
	private static AnFlags? _flags;
	public static AnFlags Flags => _flags ??= new AnFlags();
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
