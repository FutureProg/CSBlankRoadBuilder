using ColossalFramework;

namespace BlankRoadBuilder.Domain.Options;
public static class ModOptions
{
    private static readonly SavedFloat _fillerHeight = new(nameof(_fillerHeight), nameof(BlankRoadBuilder), 0.3F);
    private static readonly SavedBool _disableAutoFillInTheSavePanel = new(nameof(_disableAutoFillInTheSavePanel), nameof(BlankRoadBuilder), false);
    private static readonly SavedBool _addGrassPropsToGrassLanes = new(nameof(_addGrassPropsToGrassLanes), nameof(BlankRoadBuilder), true);
    private static readonly SavedBool _vanillaTreePlacement = new(nameof(_vanillaTreePlacement), nameof(BlankRoadBuilder), false);
    private static readonly SavedBool _vanillaStreetLightPlacement = new(nameof(_vanillaStreetLightPlacement), nameof(BlankRoadBuilder), false);
    private static readonly SavedBool _flipTrafficLights = new(nameof(_flipTrafficLights), nameof(BlankRoadBuilder), false);
    private static readonly SavedInt _markingsStyle = new(nameof(_markingsStyle), nameof(BlankRoadBuilder), (int)MarkingStyle.Vanilla);
    private static readonly SavedInt _tramTracks = new(nameof(_tramTracks), nameof(BlankRoadBuilder), (int)TramTracks.Rev0);
    private static readonly SavedInt _markings = new(nameof(_markings), nameof(BlankRoadBuilder), (int)MarkingsSource.IMTWithANHelpers);

    [ModOptions("Keep markings hidden by default", "Sets the default state of automatically generated markings OFF by default, they can still be toggled ON with Adaptive Networks")]
    public static MarkingsSource MarkingsGenerated { get => (MarkingsSource)_markings.value; set => _markings.value = (int)value; }

	[ModOptions("Global Markings Style", "Changes the style of lane markings used when generating a road")]
	public static MarkingStyle MarkingsStyle { get => (MarkingStyle)_markingsStyle.value; set => _markingsStyle.value = (int)value; }

	[ModOptions("Place traffic lights on the opposite side of the junction", "Traffic lights' positions will be at the start of the lanes instead of at the end")]
    public static bool FlipTrafficLights { get => _flipTrafficLights; set => _flipTrafficLights.value = value; }

    [ModOptions("Add grass props to grass lanes", "Enabling this adds repeating grass props to any lane with grass decorations on top of the generated filler")]
    public static bool AddGrassPropsToGrassLanes { get => _addGrassPropsToGrassLanes; set => _addGrassPropsToGrassLanes.value = value; }

    [ModOptions("Use vanilla tree placement", "Generates the trees with a standard repeat distance")]
    public static bool VanillaTreePlacement { get => _vanillaTreePlacement; set => _vanillaTreePlacement.value = value; }

    [ModOptions("Use vanilla streetlight placement", "Generates the lights with a standard repeat distance")]
    public static bool VanillaStreetLightPlacement { get => _vanillaStreetLightPlacement; set => _vanillaStreetLightPlacement.value = value; }

    [ModOptions("Disable the auto-fill of information and thumbnails in the save panel")]
    public static bool DisableAutoFillInTheSavePanel { get => _disableAutoFillInTheSavePanel; set => _disableAutoFillInTheSavePanel.value = value; }

    [ModOptions("Default Tram Tracks", "Changes the default style of tracks used for Trams, other options will remain available with AN toggles")]
    public static TramTracks TramTracks { get => (TramTracks)_tramTracks.value; set => _tramTracks.value = (int)value; }

    public static LaneSizeOptions LaneSizes { get; } = new LaneSizeOptions();
}
