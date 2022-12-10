using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util;

using ColossalFramework;

using System.Collections.Generic;
using System.Linq;

namespace BlankRoadBuilder.Domain.Options;
public static class ModOptions
{
    private static readonly SavedFloat _fillerHeight = new(nameof(_fillerHeight), nameof(BlankRoadBuilder), 0.3F);
    private static readonly SavedBool _disableAutoFillInTheSavePanel = new(nameof(_disableAutoFillInTheSavePanel), nameof(BlankRoadBuilder), false);
    private static readonly SavedBool _keepMarkingsHiddenByDefault = new(nameof(_keepMarkingsHiddenByDefault), nameof(BlankRoadBuilder), false);
    private static readonly SavedInt _markingsStyle = new(nameof(_markingsStyle), nameof(BlankRoadBuilder), (int)MarkingStyle.Vanilla);
    private static readonly SavedInt _tramTracks = new(nameof(_tramTracks), nameof(BlankRoadBuilder), (int)TramTracks.Rev0);

    [ModOptions("Keep markings hidden by default", "Sets the default state of automatically generated markings OFF by default, they can still be toggled ON with Adaptive Networks")]
    public static bool KeepMarkingsHiddenByDefault { get => _keepMarkingsHiddenByDefault; set => _keepMarkingsHiddenByDefault.value = value; }

    [ModOptions("Disable the auto-fill of information and thumbnails in the save panel")]
    public static bool DisableAutoFillInTheSavePanel { get => _disableAutoFillInTheSavePanel; set => _disableAutoFillInTheSavePanel.value = value; }

    [ModOptions("Filler Height", "Changes the default height of filler lanes' generated median", 0.05F, 0.3F, 0.01F, "m")]
    public static float FillerHeight { get => _fillerHeight; set => _fillerHeight.value = value; }

    [ModOptions("Global Markings Style", "Changes the style of lane markings used when generating a road")]
    public static MarkingStyle MarkingsStyle { get => (MarkingStyle)_markingsStyle.value; set => _markingsStyle.value = (int)value; }

    [ModOptions("Default Tram Tracks", "Changes the default style of tracks used for Trams, other options will remain available with AN toggles")]
    public static TramTracks TramTracks { get => (TramTracks)_tramTracks.value; set => _tramTracks.value = (int)value; }

    public static LaneSizeOptions LaneSizes { get; } = new LaneSizeOptions();
}
