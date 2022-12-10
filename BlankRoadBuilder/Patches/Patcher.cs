namespace BlankRoadBuilder;

using AlgernonCommons;
using AlgernonCommons.Patching;

using CitiesHarmony.API;

using HarmonyLib;

public static class Patcher
{
	public static string HarmonyID { get; } = "Trejak.BlankRoadBuilder";
	public static bool Patched { get; private set; }

	public static void PatchAll()
	{
		if (!Patched)
		{
			if (HarmonyHelper.IsHarmonyInstalled)
			{
				var harmony = new Harmony(HarmonyID);
				harmony.PatchAll(typeof(Patcher).Assembly);
				Patched = true;
			}
		}
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