using ColossalFramework;

using static BlankRoadBuilder.Domain.Options.OptionCategory;

namespace BlankRoadBuilder.Domain.Options;
public static class ModOptions
{
	[ModOptions(MARKINGS, (int)MarkingsSource.IMTWithMeshFillers, "Automatic Markings", "Changes what kind of markings are generated with the road")]
	public static MarkingsSource MarkingsGenerated { get => (MarkingsSource)_markings.value; set => _markings.value = (int)value; }

	[ModOptions(MARKINGS, (int)MarkingStyle.Vanilla, "Global Markings Style", "Changes the style of lane markings used when generating a road")]
	public static MarkingStyle MarkingsStyle { get => (MarkingStyle)_markingsStyle.value; set => _markingsStyle.value = (int)value; }

	[ModOptions(MARKINGS, (int)CrosswalkStyle.Zebra, "Vanilla crosswalks style", "Changes how the vanilla crosswalks are generated")]
	public static CrosswalkStyle VanillaCrosswalkStyle { get => (CrosswalkStyle)_vanillaCrosswalkStyle.value; set => _vanillaCrosswalkStyle.value = (int)value; }

	[ModOptions(PROPS, false, "Place traffic lights on the opposite side of the junction", "Traffic lights' positions will be at the start of the lanes instead of at the end")]
	public static bool FlipTrafficLights { get => _flipTrafficLights; set => _flipTrafficLights.value = value; }

	[ModOptions(PROPS, true, "Add lane arrows", "Generates lane arrow decals with the road")]
	public static bool AddLaneArrows { get => _addLaneArrows; set => _addLaneArrows.value = value; }

	[ModOptions(PROPS, true, "Add lane decals", "Generates lane decals like bus & bike decals with the road")]
	public static bool AddLaneDecals { get => _addLaneDecals; set => _addLaneDecals.value = value; }

	[ModOptions(PROPS, true, "Add grass props to grass lanes", "Enabling this adds repeating grass props to any lane with grass decorations on top of the generated filler")]
	public static bool AddGrassPropsToGrassLanes { get => _addGrassPropsToGrassLanes; set => _addGrassPropsToGrassLanes.value = value; }

	[ModOptions(DESIGN, false, "Remove curb on Flat Roads", "Removes the curb texture on the edge of the asphalt of flat roads")]
	public static bool RemoveCurbOnFlatRoads { get => _flatRoadsHaveNoCurb; set => _flatRoadsHaveNoCurb.value = value; }

	[ModOptions(DESIGN, false, "Disable low curbs on nodes", "Only allows the high curb to appear on a node")]
	public static bool OnlyUseHighCurb { get => _onlyUseHighCurb; set => _onlyUseHighCurb.value = value; }

	[ModOptions(DESIGN, true, "Always add ghost lanes for pedestrian and curb lanes", "Disabling this removes the extra ghost lanes added when you're not using IMT markings")]
	public static bool AlwaysAddGhostLanes { get => _alwaysAddGhostLanes; set => _alwaysAddGhostLanes.value = value; }

	[ModOptions(PROPS, false, "Hide road clutter by default", "Road clutter includes signs, parking meters, etc. They can still be shown using their AN toggle")]
	public static bool HideRoadClutter { get => _hideRoadClutter; set => _hideRoadClutter.value = value; }

	[ModOptions(DESIGN, true, "Hide road damage by default", "Road damage are random decals scattered around your road. They can still be shown using their AN toggle")]
	public static bool HideRoadDamage { get => _hideRoadDamage; set => _hideRoadDamage.value = value; }

	[ModOptions(DESIGN, false, "Remove parking lanes on non-ground levels", "Warning, results might vary based on your road's settings")]
	public static bool GroundOnlyParking { get => _groundOnlyParking; set => _groundOnlyParking.value = value; }

	[ModOptions(DESIGN, false, "Convert grass lanes to pavement on non-ground levels", "")]
	public static bool GroundOnlyGrass { get => _groundOnlyGrass; set => _groundOnlyGrass.value = value; }

	[ModOptions(DESIGN, false, "Disable transit stops on non-ground levels", "")]
	public static bool GroundOnlyStops { get => _groundOnlyStops; set => _groundOnlyStops.value = value; }

	[ModOptions(DESIGN, 3, "Minimum distance to stop", "Determines the minimum distance between a pedestrian and vehicle lane in order for a stop to appear.", 0.1F, 12, 0.1F, "m")]
	public static float MinimumStopDistance { get => _minimumStopDistance; set => _minimumStopDistance.value = value; }

	[ModOptions(DESIGN, (int)TramTracks.Rev0, "Default Tram tracks", "Changes the default style of tracks used for Trams, other options will remain available with AN toggles")]
	public static TramTracks TramTracks { get => (TramTracks)_tramTracks.value; set => _tramTracks.value = (int)value; }

	[ModOptions(DESIGN, (int)StepSteepness.ModerateSlope, "Elevated step slope", "Changes the steepness of elevated step transitions at intersections")]
	public static StepSteepness StepTransition { get => (StepSteepness)_stepTransition.value; set => _stepTransition.value = (int)value; }

	[ModOptions(PROPS, false, "Use vanilla streetlight placement", "Generates the lights with a standard repeat distance")]
	public static bool VanillaStreetLightPlacement { get => _vanillaStreetLightPlacement; set => _vanillaStreetLightPlacement.value = value; }

	[ModOptions(PROPS, false, "Randomize tree distances")]
	public static bool RandomizeTreeDistance { get => _randomiseTreeDistance; set => _randomiseTreeDistance.value = value; }

	[ModOptions(PROPS, false, "Use vanilla tree placement", "Generates the trees with a standard repeat distance")]
	public static bool VanillaTreePlacement { get => _vanillaTreePlacement; set => _vanillaTreePlacement.value = value; }

	[ModOptions(OTHER, (int)RoadSortMode.DateCreated, "Road sorting mode", "Changes the sorting of the road configurations")]
	public static RoadSortMode RoadSortMode { get => LaneSizes.SortMode; set => LaneSizes.SortMode = value; }

	[ModOptions(OTHER, false, "Disable the auto-fill of information and thumbnails in the save panel")]
	public static bool DisableAutoFillInTheSavePanel { get => _disableAutoFillInTheSavePanel; set => _disableAutoFillInTheSavePanel.value = value; }

	public static LaneSizeOptions LaneSizes { get; } = new LaneSizeOptions();

	private static readonly SavedBool _disableAutoFillInTheSavePanel = new(nameof(_disableAutoFillInTheSavePanel), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _vanillaTreePlacement = new(nameof(_vanillaTreePlacement), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _vanillaStreetLightPlacement = new(nameof(_vanillaStreetLightPlacement), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _flipTrafficLights = new(nameof(_flipTrafficLights), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _flatRoadsHaveNoCurb = new(nameof(_flatRoadsHaveNoCurb), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _hideRoadClutter = new(nameof(_hideRoadClutter), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _randomiseTreeDistance = new(nameof(_randomiseTreeDistance), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _groundOnlyParking = new(nameof(_groundOnlyParking), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _groundOnlyGrass = new(nameof(_groundOnlyGrass), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _groundOnlyStops = new(nameof(_groundOnlyStops), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _onlyUseHighCurb = new(nameof(_onlyUseHighCurb), nameof(BlankRoadBuilder), false);

	private static readonly SavedBool _addLaneDecals = new(nameof(_addLaneDecals), nameof(BlankRoadBuilder), true);
	private static readonly SavedBool _addLaneArrows = new(nameof(_addLaneArrows), nameof(BlankRoadBuilder), true);
	private static readonly SavedBool _addGrassPropsToGrassLanes = new(nameof(_addGrassPropsToGrassLanes), nameof(BlankRoadBuilder), true);
	private static readonly SavedBool _alwaysAddGhostLanes = new(nameof(_alwaysAddGhostLanes), nameof(BlankRoadBuilder), true);
	private static readonly SavedBool _hideRoadDamage = new(nameof(_hideRoadDamage), nameof(BlankRoadBuilder), true);

	private static readonly SavedFloat _minimumStopDistance = new(nameof(_minimumStopDistance), nameof(BlankRoadBuilder), 3F);

	private static readonly SavedInt _markingsStyle = new(nameof(_markingsStyle), nameof(BlankRoadBuilder), (int)MarkingStyle.Vanilla);
	private static readonly SavedInt _tramTracks = new(nameof(_tramTracks), nameof(BlankRoadBuilder), (int)TramTracks.Rev0);
	private static readonly SavedInt _markings = new(nameof(_markings), nameof(BlankRoadBuilder), (int)MarkingsSource.IMTWithMeshFillers);
	private static readonly SavedInt _stepTransition = new(nameof(_stepTransition), nameof(BlankRoadBuilder), (int)StepSteepness.ModerateSlope);
	private static readonly SavedInt _vanillaCrosswalkStyle = new(nameof(_vanillaCrosswalkStyle), nameof(BlankRoadBuilder), (int)CrosswalkStyle.Zebra);
}