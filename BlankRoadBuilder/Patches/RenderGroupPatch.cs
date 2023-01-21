using BlankRoadBuilder.Util;

using HarmonyLib;

using System;

namespace BlankRoadBuilder.Patches;

[HarmonyPatch(typeof(RenderGroup), "UpdateMeshData", new Type[] { })]
public class RenderGroupPatch_UpdateMeshData
{
	public static bool Prefix()
	{
		return !RoadBuilderUtil.IsBuilding;
	}
}
