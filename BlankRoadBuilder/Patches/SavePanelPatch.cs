using AdaptiveRoads.LifeCycle;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.UI;
using BlankRoadBuilder.Util;

using ColossalFramework.IO;
using ColossalFramework.Packaging;
using ColossalFramework.UI;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BlankRoadBuilder.Patches;

[HarmonyPatch(typeof(SaveAssetPanel), "Awake", new Type[] { })]
public class SavePanelPatch
{
	private static RoadInfo? _lastLoadedRoad;

	public static RoadInfo? LastLoadedRoad { get => ModOptions.DisableAutoFillInTheSavePanel ? null : _lastLoadedRoad; set { _lastLoadedRoad = value; StagingPatched = false; } }
	public static bool StagingPatched { get; private set; }

	public static void Postfix(SaveAssetPanel __instance)
	{
		if (LastLoadedRoad == null || StagingPatched)
			return;

		if (AssetDataExtension.WasLastLoaded == false)
		{
			AdaptiveNetworksUtil.RenameEditNet(LastLoadedRoad.Name, false);

			SaveAssetPanel.lastLoadedName = LastLoadedRoad.Name;
		}

		SaveAssetPanel.lastLoadedAsset = LastLoadedRoad.Name;
		SaveAssetPanel.lastAssetDescription = LastLoadedRoad.Description;

		StagingPatched = true;
	}

	private static readonly Dictionary<string, Func<byte[]?>> _patchingFiles = new()
	{
		{ "asset_thumb.png", () => LastLoadedRoad?.SmallThumbnail },
		{ "previewimage.png", () => LastLoadedRoad?.LargeThumbnail },
		{ "snapshot.png", () => LastLoadedRoad?.LargeThumbnail },
		{ "thumbnail.png", () => LastLoadedRoad?.LargeThumbnail },
		{ "tooltip.png", () => LastLoadedRoad?.TooltipImage },
		{ "asset_tooltip.png", () => LastLoadedRoad?.TooltipImage },
	};

	public static bool PatchThumbnails(string? folder)
	{
		if (folder == null || !Directory.Exists(folder))
			return false;

		var filesFound = false;
		foreach (var image in new DirectoryInfo(folder).GetFiles())
		{
			var name = image.Name.ToLower();

			if (_patchingFiles.ContainsKey(name))
			{
				if (image.Length != _patchingFiles[name].Invoke()?.Length)
					File.WriteAllBytes(image.FullName, _patchingFiles[name]());

				filesFound = true;
			}
		}

		return filesFound;
	}
}

[HarmonyPatch(typeof(SaveAssetPanel), "FetchSnapshots", new Type[] { })]
public class SavePanelPatch_FetchSnapshots
{
	public static void Postfix()
	{
		if (SavePanelPatch.LastLoadedRoad == null)
			return;

		var snapShotPath = ToolsModifierControl.GetTool<SnapshotTool>().snapShotPath;

		SavePanelPatch.PatchThumbnails(snapShotPath);
	}
}

[HarmonyPatch(typeof(SaveAssetPanel), "InitializeThumbnails", new Type[] { })]
public class SavePanelPatch_InitializeThumbnails
{
	public static bool Prefix(SaveAssetPanel __instance)
	{
		if (SavePanelPatch.LastLoadedRoad == null || AssetDataExtension.WasLastLoaded == true)
			return true;

		typeof(SaveAssetPanel)
			.GetField("m_IgnoreChangesTimeStamp", BindingFlags.Instance | BindingFlags.NonPublic)
			.SetValue(__instance, 0L);

		var m_ThumbPath = typeof(SaveAssetPanel)
			.GetField("m_ThumbPath", BindingFlags.Instance | BindingFlags.NonPublic)
			.GetValue(__instance) as string;

		if (!string.IsNullOrEmpty(m_ThumbPath))
			SavePanelPatch.PatchThumbnails(Directory.GetParent(m_ThumbPath).FullName);

		typeof(SaveAssetPanel)
			.GetMethod("PrepareStagingArea", BindingFlags.Instance | BindingFlags.NonPublic)
			.Invoke(__instance, null);

		return false;
	}
}

[HarmonyPatch(typeof(SaveAssetPanel), "CheckCompulsoryShots", new Type[] { })]
public class SavePanelPatch_CheckCompulsoryShots
{
	public static bool Prefix()
	{
		return SavePanelPatch.LastLoadedRoad == null;
	}
}

[HarmonyPatch]
public class SavePanelPatch_SaveRoutine
{

	public static MethodBase TargetMethod()
	{
		var type = typeof(SaveAssetPanel);
		return AccessTools.FirstMethod(type, method => method.Name == "SaveRoutine");
	}

	public static bool Prefix(SaveAssetPanel __instance)
	{
		UnityEngine.Debug.Log("SavePanelPatch_OnSave");
		if (RoadBuilderPanel.LastLoadedRoadFileName != null)
		{
			var saveNameField = (UITextField)typeof(SaveAssetPanel)
				.GetField("m_SaveName", BindingFlags.Instance | BindingFlags.NonPublic)
				.GetValue(__instance);
			var saveFile = PathEscaper.Escape(saveNameField.text) + PackageManager.packageExtension;
			var roadConfigFileName = RoadBuilderPanel.LastLoadedRoadFileName;
			var lastRoadOptions = RoadOptions.LastSelected;
			AssetMatchingUtil.SetMatch(saveFile, roadConfigFileName, lastRoadOptions);
		}
		return true;
	}
}