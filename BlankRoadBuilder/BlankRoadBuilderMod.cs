
using AlgernonCommons.UI;

using BlankRoadBuilder.UI.Options;

using CitiesHarmony.API;

using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.UI;

using ICities;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace BlankRoadBuilder;
public class BlankRoadBuilderMod : IUserMod
{
	public string Name => "Road Builder v" + VersionString;
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
		_ = Directory.CreateDirectory(BuilderFolder);

		ModFolder = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly())?.modPath;

		try
		{ CopyThumbnailMaker(); }
		catch { }

		try
		{
			DeleteAll(ImportFolder);

			var assetsMatcherXML = Path.Combine(Path.Combine(DataLocation.localApplicationData, "BlankRoadBuilder"), "SavedAssets.xml");
			if (File.Exists(assetsMatcherXML))
			{
				File.Move(assetsMatcherXML, Path.Combine(BuilderFolder, "SavedAssets.xml"));
			}

			if (Directory.Exists(Path.Combine(DataLocation.localApplicationData, "BlankRoadBuilder")))
			{
				var roads = Path.Combine(Path.Combine(DataLocation.localApplicationData, "BlankRoadBuilder"), "Roads");

				if (Directory.Exists(roads) && Directory.GetFiles(roads, "*.xml", SearchOption.AllDirectories).Length > 0)
					_ = Process.Start(Path.Combine(ThumbnailMakerFolder, "ThumbnailMaker.exe"), "update");
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

		var tabStrip = AutoTabstrip.AddTabstrip(s_optionsParentPanel, 0f, 0f, s_optionsParentPanel.width, s_optionsParentPanel.height - 15F, out _, tabHeight: 32f);

		_ = new GeneralOptions(tabStrip, 0);
		_ = new LaneSizeOptions(tabStrip, 1);
		_ = new IMTOptionsPanel(tabStrip, 2);
		_ = new VanillaOptionsPanel(tabStrip, 3);

		tabStrip.selectedIndex = -1;
		tabStrip.selectedIndex = 0;
	}

	private static void CopyThumbnailMaker()
	{
		var currentMakerFolde = Path.Combine(ModFolder, "Thumbnail Maker");
		var thumbnailMakerFolder = ThumbnailMakerFolder;

		_ = Directory.CreateDirectory(thumbnailMakerFolder);
		_ = Directory.CreateDirectory(Path.Combine(thumbnailMakerFolder, "Resources"));

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

		foreach (var file in Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly))
			File.Delete(file);

		foreach (var file in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
			DeleteAll(file);

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