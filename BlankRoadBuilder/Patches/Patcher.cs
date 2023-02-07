using BlankRoadBuilder.Patches;

using CitiesHarmony.API;

using ColossalFramework.Math;

using HarmonyLib;

using System;

using UnityEngine;

namespace BlankRoadBuilder;
public static class Patcher
{
	public static string HarmonyID { get; } = "Trejak.BlankRoadBuilder";
	public static bool Patched { get; private set; }

	public static bool PatchAll()
	{
		if (!Patched)
		{
			if (HarmonyHelper.IsHarmonyInstalled)
			{
				try
				{
					var harmony = new Harmony(HarmonyID);
					harmony.PatchAll(typeof(Patcher).Assembly);

					harmony.Patch(typeof(NetManager).GetMethod(nameof(NetManager.CreateSegment), new Type[] { typeof(ushort).MakeByRefType(), typeof(Randomizer).MakeByRefType(), typeof(NetInfo), typeof(TreeInfo), typeof(ushort), typeof(ushort), typeof(Vector3), typeof(Vector3), typeof(uint), typeof(uint), typeof(bool) })
						, postfix: new HarmonyMethod(typeof(Segment.UpdateEndSegments).GetMethod(nameof(Segment.UpdateEndSegments.Postfix), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)));
				
					Patched = true;
				}
				catch { }
			}
		}

		return Patched;
	}

	public static void UnpatchAll()
	{
		if (Patched)
		{
			var harmony = new Harmony(HarmonyID);
			harmony.UnpatchAll(HarmonyID);
			Patched = false;
		}
	}
}