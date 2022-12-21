namespace BlankRoadBuilder;

using AdaptiveRoads.LifeCycle;

using AlgernonCommons.UI;
using BlankRoadBuilder.UI.Options;

using CitiesHarmony.API;

using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.UI;

using ICities;

using JetBrains.Annotations;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

public class BlankRoadBuilderMod : IUserMod
{
	public string Name => "Blank Road Builder v" + VersionString;
	public string Description => "Tool that allows you to create roads without dealing with the tedious asset editor";
	public static Version ModVersion => typeof(BlankRoadBuilderMod).Assembly.GetName().Version;
	public static string VersionString => ModVersion.ToString(3);

	public static string BuilderFolder => Path.Combine(DataLocation.localApplicationData, "RoadBuilder");
	public static string ImportFolder => Path.Combine(BuilderFolder, "Import");
	public static string ThumbnailMakerFolder => Path.Combine(BuilderFolder, "Thumbnail Maker");
	public static string MeshesFolder => Path.Combine(ModFolder, "Meshes");
	public static string TexturesFolder => Path.Combine(ModFolder, "Textures");
	public static string? ModFolder { get; private set; }

	public void OnEnabled()
	{
		Directory.CreateDirectory(BuilderFolder);

		ModFolder = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly())?.modPath;

		try
		{ CopyThumbnailMaker(); }
		catch { }

		try
		{
			DeleteAll(ImportFolder);

			if (Directory.Exists(Path.Combine(DataLocation.localApplicationData, "BlankRoadBuilder")))
			{
				var roads = Path.Combine(Path.Combine(DataLocation.localApplicationData, "BlankRoadBuilder"), "Roads");

				if (Directory.Exists(roads) && Directory.GetFiles(roads, "*.xml").Length > 0)
					Process.Start(Path.Combine(ThumbnailMakerFolder, "ThumbnailMaker.exe"), "update");
				else
					DeleteAll(Path.Combine(DataLocation.localApplicationData, "BlankRoadBuilder"));
			}
		}
		catch { }

		HarmonyHelper.DoOnHarmonyReady(Patcher.PatchAll);
	}

	static BlankRoadBuilderMod()
	{
		if (GameSettings.FindSettingsFileByName(nameof(BlankRoadBuilder)) == null)
		{
			GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = nameof(BlankRoadBuilder) } });
		}
	}

	public void OnSettingsUI(UIHelperBase helper)
	{
		var s_optionsParentPanel = ((UIHelper)helper).self as UIScrollablePanel;

		if (s_optionsParentPanel == null)
			return;

		s_optionsParentPanel.autoLayout = false;

		var tabStrip = AutoTabstrip.AddTabstrip(s_optionsParentPanel, 0f, 0f, s_optionsParentPanel.width , s_optionsParentPanel.height-15F, out _, tabHeight: 32f);

		new GeneralOptions(tabStrip, 0);
		new LaneSizeOptions(tabStrip, 1);
		new MarkingsOptions(tabStrip, 2);
		new FillerOptions(tabStrip, 3);

		tabStrip.selectedIndex = -1;
		tabStrip.selectedIndex = 0;
	}

	private static void CopyThumbnailMaker()
	{
		var currentMakerFolde = Path.Combine(ModFolder, "Thumbnail Maker");
		var thumbnailMakerFolder = ThumbnailMakerFolder;

		Directory.CreateDirectory(thumbnailMakerFolder);
		Directory.CreateDirectory(Path.Combine(thumbnailMakerFolder, "Resources"));

		foreach (var item in Directory.GetFiles(currentMakerFolde))
		{
			File.Copy(item, Path.Combine(thumbnailMakerFolder, Path.GetFileName(item).Replace("_", ".")), true);
		}

		foreach (var item in Directory.GetFiles(Path.Combine(currentMakerFolde, "Resources")))
		{
			var target = new FileInfo(Path.Combine(Path.Combine(thumbnailMakerFolder, "Resources"), Path.GetFileName(item)));

			if (!target.Exists || target.LastWriteTime < new FileInfo(item).LastWriteTime)
				File.Copy(item, Path.Combine(Path.Combine(thumbnailMakerFolder, "Resources"), Path.GetFileName(item)), true);
		}
	}

	public static void DeleteAll(string directory)
	{
		if (!Directory.Exists(directory))
			return;
		foreach (var file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
			File.Delete(file);

		foreach (var file in Directory.GetDirectories(directory, "*", SearchOption.AllDirectories))
			Directory.Delete(file);

		Directory.Delete(directory);
	}

	public void OnDisabled()
	{
		if (HarmonyHelper.IsHarmonyInstalled)
		{
			Patcher.UnpatchAll();
		}
	}
}