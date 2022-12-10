namespace BlankRoadBuilder.Patches;

using BlankRoadBuilder.UI;

using HarmonyLib;

using UnityEngine;

[HarmonyPatch(typeof(RoadEditorMainPanel), nameof(RoadEditorMainPanel.Reset))]
public class InitializeUI
{
	//private static UIManager UiManager;

	public static void Postfix()
	{
		Debug.Log("Initializing");

		var gameObject = new GameObject("Blank Road Builder UI Manager");
		
		gameObject.AddComponent<UIManager>();
	}
}

