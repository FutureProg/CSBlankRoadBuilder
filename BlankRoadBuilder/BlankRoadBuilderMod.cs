using BlankRoadBuilder.UI;
using BlankRoadBuilder.UI.Options;

using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.UI;

using ICities;

using ModsCommon;
using ModsCommon.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;

namespace BlankRoadBuilder;
public class BlankRoadBuilderMod : BasePatcherMod<BlankRoadBuilderMod>
{
	public static Version ModVersion => typeof(BlankRoadBuilderMod).Assembly.GetName().Version;
	public static DateTime CurrentVersionDate => new FileInfo(Path.Combine(ModFolder, $"{nameof(BlankRoadBuilder)}.dll")).LastWriteTimeUtc;

	public static string BuilderFolder => Path.Combine(DataLocation.localApplicationData, "RoadBuilder");
	public static string ImportFolder => Path.Combine(BuilderFolder, "Import");
	public static string ThumbnailMakerFolder => Path.Combine(BuilderFolder, "Thumbnail Maker");
	public static string MeshesFolder => Path.Combine(ModFolder, "Meshes");
	public static string TexturesFolder => Path.Combine(ModFolder, "Textures");
	public static string? ModFolder => PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly())?.modPath;

	protected override Version RequiredGameVersion => new Version(1, 16, 0, 3);
	public override string NameRaw => "Road Builder";
	public override string Description => "Tool that allows you to create roads without dealing with the tedious asset editor";
	protected override ulong StableWorkshopId => 2891132324ul;
	protected override ulong BetaWorkshopId => 2891132324ul;
	protected override string IdRaw => nameof(BlankRoadBuilderMod);
	public override bool IsBeta => false;
	protected override LocalizeManager LocalizeManager { get; } = new LocalizeManager("Localize", typeof(BlankRoadBuilderMod).Assembly);
	public override List<ModVersion> Versions { get; } = new List<ModVersion>
	{
		new ModVersion(new Version("1.3.4"), new DateTime(2023, 2, 21)),
		new ModVersion(new Version("1.3.3"), new DateTime(2023, 2, 20)),
		new ModVersion(new Version("1.3.2"), new DateTime(2023, 2, 19)),
		new ModVersion(new Version("1.3.1"), new DateTime(2023, 2, 18)),
		new ModVersion(new Version("1.3.0"), new DateTime(2023, 2, 16)),
		new ModVersion(new Version("1.2.5"), new DateTime(2023, 2, 12)),
		new ModVersion(new Version("1.2.4"), new DateTime(2023, 2, 10)),
		new ModVersion(new Version("1.2.3"), new DateTime(2023, 2, 9)),
		new ModVersion(new Version("1.2.2"), new DateTime(2023, 2, 8)),
		new ModVersion(new Version("1.2.1"), new DateTime(2023, 2, 7)),
		new ModVersion(new Version("1.2.0"), new DateTime(2023, 2, 5)),
		new ModVersion(new Version("1.1.2"), new DateTime(2023, 1, 29)),
		new ModVersion(new Version("1.1.1"), new DateTime(2023, 1, 23)),
		new ModVersion(new Version("1.1.0"), new DateTime(2023, 1, 22)),
		new ModVersion(new Version("1.0.4"), new DateTime(2023, 1, 6)),
		new ModVersion(new Version("1.0.3"), new DateTime(2023, 1, 1)),
		new ModVersion(new Version("1.0.2"), new DateTime(2022, 12, 31)),
		new ModVersion(new Version("1.0.1"), new DateTime(2022, 12, 31)),
		new ModVersion(new Version("1.0.0"), new DateTime(2022, 12, 31)),
	};

	protected override List<BaseDependencyInfo> DependencyInfos
	{
		get
		{
			var dependencyInfos = base.DependencyInfos;

			dependencyInfos.Add(get("Intersection Marking Tool", 2140418403ul, 2159934925uL));
			dependencyInfos.Add(get("Adaptive Networks", 2414618415ul, 2669938594uL));
			dependencyInfos.Add(get("TM:PE", 1637663252ul, 2489276785uL));
			dependencyInfos.Add(get("Network Anarchy", 2862881785ul, 2917150208uL, DependencyState.Disable));

			return dependencyInfos;

			static NeedDependencyInfo get(string name, ulong id, ulong id2, DependencyState dependency = DependencyState.Enable)
			{
				var allSearcher = IdSearcher.Invalid & new UserModNameSearcher(name, BaseMatchSearcher.Option.AllOptions | BaseMatchSearcher.Option.StartsWidth);
				var anySearcher = new IdSearcher(id) | new IdSearcher(id2);

				return new NeedDependencyInfo(dependency, allSearcher | anySearcher, name, id);
			}
		}
	}

	protected override void Enable()
	{
		Directory.CreateDirectory(BuilderFolder);

		try
		{
			CopyThumbnailMaker();
			DeleteAll(ImportFolder);
		}
		catch { }

		base.Enable();
	}

	protected override bool PatchProcess()
	{
		return Patcher.PatchAll();
	}

	static BlankRoadBuilderMod()
	{
		if (GameSettings.FindSettingsFileByName(nameof(BlankRoadBuilder)) == null)
		{
			GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = nameof(BlankRoadBuilder) } });
		}
	}

	protected override void GetSettings(UIHelperBase helper)
	{
		var s_optionsParentPanel = ((UIHelper)helper).self as UIScrollablePanel;

		if (s_optionsParentPanel == null)
			return;

		s_optionsParentPanel.autoLayout = false;
		s_optionsParentPanel.relativePosition = Vector2.zero;
		s_optionsParentPanel.autoLayoutPadding = new RectOffset();
		s_optionsParentPanel.scrollPadding = new RectOffset();

		var tabStrip = AutoTabstrip.AddTabstrip(s_optionsParentPanel, 0f, 0f, s_optionsParentPanel.width, s_optionsParentPanel.height, out _, tabHeight: 32f);

		new GeneralOptions(tabStrip, 0, 5);
		new CustomPropsOptions(tabStrip, 1, 5);
		new LaneSizeOptions(tabStrip, 2, 5);
		new IMTOptionsPanel(tabStrip, 3, 5);
		new VanillaOptionsPanel(tabStrip, 4, 5);

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

		foreach (var file in Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly))
			File.Delete(file);

		foreach (var file in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
			DeleteAll(file);

		Directory.Delete(directory);
	}
}