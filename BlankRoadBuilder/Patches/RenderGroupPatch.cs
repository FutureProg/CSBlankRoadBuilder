﻿using BlankRoadBuilder.Util;

using HarmonyLib;

using System;

namespace BlankRoadBuilder.Patches;

[HarmonyPatch(typeof(RenderGroup), "UpdateMeshData", new Type[] { })]
public class RenderGroupPatch_UpdateMeshData
{
	public static bool Prefix()
	{
		return !ToolsModifierControl.isAssetEditor || !RoadBuilderUtil.IsBuilding;
	}
}

[HarmonyPatch(typeof(NetTool), "SimulationStep", new Type[] { })]
public class NetToolPatch_SimulationStep
{
	public static bool Prefix()
	{
		return !ToolsModifierControl.isAssetEditor || !RoadBuilderUtil.IsBuilding;
	}
}

[HarmonyPatch(typeof(ToolManager), "SimulationStepImpl", new Type[] { typeof(int) })]
public class ToolManagerPatch_SimulationStepImpl
{
	public static bool Prefix()
	{
		return !ToolsModifierControl.isAssetEditor || !RoadBuilderUtil.IsBuilding;
	}
}
