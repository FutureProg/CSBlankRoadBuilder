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
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using UnityEngine;

using static RenderManager;

namespace BlankRoadBuilder.Patches;

[HarmonyPatch(typeof(SaveAssetPanel), "Awake", new Type[] { })]
public class SavePanelPatch
{
	public static bool IsAssetLoaded => !ModOptions.DisableAutoFillInTheSavePanel && LastLoadedRoad != null;
	public static bool IsAssetNew => AssetDataExtension.WasLastLoaded == false;
	public static RoadInfo? LastLoadedRoad => RoadBuilderUtil.CurrentRoad;

	public static void Postfix(SaveAssetPanel __instance)
	{
		if (typeof(SaveAssetPanel)
			.GetField("m_SnapShotSprite", BindingFlags.Instance | BindingFlags.NonPublic)
			.GetValue(__instance) is UITextureSprite m_SnapShotSprite)
		{
			m_SnapShotSprite.relativePosition = new UnityEngine.Vector3(m_SnapShotSprite.relativePosition.x + (m_SnapShotSprite.width - m_SnapShotSprite.height) / 2, m_SnapShotSprite.relativePosition.y, m_SnapShotSprite.relativePosition.z);
			m_SnapShotSprite.width = m_SnapShotSprite.height;
		}
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

	public static void PatchThumbnails(string? folder)
	{
		if (folder == null || !Directory.Exists(folder))
			return;

		foreach (var item in _patchingFiles)
		{
			File.WriteAllBytes(Path.Combine(folder, item.Key), item.Value());
		}
	}
}

[HarmonyPatch(typeof(SaveAssetPanel), "FetchSnapshots", new Type[] { })]
public class SavePanelPatch_FetchSnapshots
{
	public static void Prefix(SaveAssetPanel __instance)
	{
		if (!SavePanelPatch.IsAssetLoaded)
			return;

		typeof(SaveAssetPanel)
			.GetMethod("PrepareStagingArea", BindingFlags.Instance | BindingFlags.NonPublic)
			.Invoke(__instance, null);

		var m_StagingPath = typeof(SaveAssetPanel)
			.GetField("m_StagingPath", BindingFlags.Instance | BindingFlags.NonPublic)
			.GetValue(__instance) as string;

		var snapShotPath = ToolsModifierControl.GetTool<SnapshotTool>()?.snapShotPath;

		SavePanelPatch.PatchThumbnails(m_StagingPath);
		SavePanelPatch.PatchThumbnails(snapShotPath);
	}
}

[HarmonyPatch(typeof(SaveAssetPanel), "InitializeThumbnails", new Type[] { })]
public class SavePanelPatch_InitializeThumbnails
{
	public static bool Prefix()
	{
		return !SavePanelPatch.IsAssetLoaded;
	}
}

[HarmonyPatch(typeof(SaveAssetPanel), "OnVisibilityChanged", new Type[] { typeof(UIComponent), typeof(bool) })]
public class SavePanelPatch_OnVisibilityChanged
{
	public static void Prefix(bool visible)
	{
		if (!visible || !SavePanelPatch.IsAssetLoaded)
			return;

		if (SavePanelPatch.IsAssetNew)
		{
			AdaptiveNetworksUtil.RenameEditNet(SavePanelPatch.LastLoadedRoad.Name, false);

			SaveAssetPanel.lastLoadedName = SavePanelPatch.LastLoadedRoad.Name;
		}

		SaveAssetPanel.lastLoadedAsset = SavePanelPatch.LastLoadedRoad.Name;
		SaveAssetPanel.lastAssetDescription = SavePanelPatch.LastLoadedRoad.Description;
	}
}

[HarmonyPatch(typeof(SaveAssetPanel), "CheckCompulsoryShots", new Type[] { })]
public class SavePanelPatch_CheckCompulsoryShots
{
	public static bool Prefix()
	{
		return !SavePanelPatch.IsAssetLoaded;
	}
}

[HarmonyPatch(typeof(SaveAssetPanel), "Refresh", new Type[] { typeof(object), typeof(ReporterEventArgs) })]
public class SavePanelPatch_Refresh
{
	public static bool Prefix()
	{
		return !SavePanelPatch.IsAssetLoaded;
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