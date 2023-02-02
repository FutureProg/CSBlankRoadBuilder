﻿using ColossalFramework;

namespace BlankRoadBuilder.Domain.Options;
public static class ModOptions
{
	private static readonly SavedBool _disableAutoFillInTheSavePanel = new(nameof(_disableAutoFillInTheSavePanel), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _vanillaTreePlacement = new(nameof(_vanillaTreePlacement), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _vanillaStreetLightPlacement = new(nameof(_vanillaStreetLightPlacement), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _flipTrafficLights = new(nameof(_flipTrafficLights), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _flatRoadsHaveNoCurb = new(nameof(_flatRoadsHaveNoCurb), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _hideRoadClutter = new(nameof(_hideRoadClutter), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _randomiseTreeDistance = new(nameof(_randomiseTreeDistance), nameof(BlankRoadBuilder), false);
	private static readonly SavedBool _noVanillaCrosswalks = new(nameof(_noVanillaCrosswalks), nameof(BlankRoadBuilder), false);

	private static readonly SavedBool _addLaneDecals = new(nameof(_addLaneDecals), nameof(BlankRoadBuilder), true);
	private static readonly SavedBool _addLaneArrows = new(nameof(_addLaneArrows), nameof(BlankRoadBuilder), true);
	private static readonly SavedBool _addGrassPropsToGrassLanes = new(nameof(_addGrassPropsToGrassLanes), nameof(BlankRoadBuilder), true);
	private static readonly SavedBool _alwaysAddGhostLanes = new(nameof(_alwaysAddGhostLanes), nameof(BlankRoadBuilder), true);
	private static readonly SavedBool _groundOnlyTrees = new(nameof(_groundOnlyTrees), nameof(BlankRoadBuilder), true);

	private static readonly SavedInt _markingsStyle = new(nameof(_markingsStyle), nameof(BlankRoadBuilder), (int)MarkingStyle.Vanilla);
	private static readonly SavedInt _tramTracks = new(nameof(_tramTracks), nameof(BlankRoadBuilder), (int)TramTracks.Rev0);
	private static readonly SavedInt _markings = new(nameof(_markings), nameof(BlankRoadBuilder), (int)MarkingsSource.IMTWithMeshFillers);
	private static readonly SavedInt _stepTransition = new(nameof(_stepTransition), nameof(BlankRoadBuilder), (int)StepSteepness.ModerateSlope);

	private static readonly SavedFloat _lightRepeatDistance = new(nameof(_lightRepeatDistance), nameof(BlankRoadBuilder), 24);
	private static readonly SavedFloat _treeRepeatDistance = new(nameof(_treeRepeatDistance), nameof(BlankRoadBuilder), 12);

	[ModOptions("Automatic Markings", "Changes what kind of markings are generated with the road")]
	public static MarkingsSource MarkingsGenerated { get => (MarkingsSource)_markings.value; set => _markings.value = (int)value; }

	[ModOptions("Global Markings Style", "Changes the style of lane markings used when generating a road")]
	public static MarkingStyle MarkingsStyle { get => (MarkingStyle)_markingsStyle.value; set => _markingsStyle.value = (int)value; }

	[ModOptions("Place traffic lights on the opposite side of the junction", "Traffic lights' positions will be at the start of the lanes instead of at the end")]
	public static bool FlipTrafficLights { get => _flipTrafficLights; set => _flipTrafficLights.value = value; }

	[ModOptions("Add lane arrows", "Generates lane arrow decals with the road")]
	public static bool AddLaneArrows { get => _addLaneArrows; set => _addLaneArrows.value = value; }

	[ModOptions("Add lane decals", "Generates lane decals like bus & bike decals with the road")]
	public static bool AddLaneDecals { get => _addLaneDecals; set => _addLaneDecals.value = value; }

	[ModOptions("Add grass props to grass lanes", "Enabling this adds repeating grass props to any lane with grass decorations on top of the generated filler")]
	public static bool AddGrassPropsToGrassLanes { get => _addGrassPropsToGrassLanes; set => _addGrassPropsToGrassLanes.value = value; }

	[ModOptions("Remove curb on Flat Roads", "Removes the curb texture on the edge of the asphalt of flat roads")]
	public static bool RemoveCurbOnFlatRoads { get => _flatRoadsHaveNoCurb; set => _flatRoadsHaveNoCurb.value = value; }

	[ModOptions("No vanilla crosswalks", "Stops generating vanilla crosswalks in roads")]
	public static bool NoVanillaCrosswalks { get => _noVanillaCrosswalks; set => _noVanillaCrosswalks.value = value; }

	[ModOptions("Always add ghost lanes for pedestrian and curb lanes", "Disabling this removes the extra ghost lanes added when you're not using IMT markings")]
	public static bool AlwaysAddGhostLanes { get => _alwaysAddGhostLanes; set => _alwaysAddGhostLanes.value = value; }

	[ModOptions("Hide road clutter by default", "Road clutter includes signs, parking meters, etc. They can still be shown using their AN toggle")]
	public static bool HideRoadClutter { get => _hideRoadClutter; set => _hideRoadClutter.value = value; }

	[ModOptions("Default Tram tracks", "Changes the default style of tracks used for Trams, other options will remain available with AN toggles")]
	public static TramTracks TramTracks { get => (TramTracks)_tramTracks.value; set => _tramTracks.value = (int)value; }

	[ModOptions("Elevated step slope", "Changes the steepness of elevated step transitions at intersections")]
	public static StepSteepness StepTransition { get => (StepSteepness)_stepTransition.value; set => _stepTransition.value = (int)value; }

	[ModOptions("Streetlight repeat distance", "Changes the distance between each street light", 6, 64, 0.5F, "m")]
	public static float LightRepeatDistance { get => _lightRepeatDistance.value; set => _lightRepeatDistance.value = value; }

	[ModOptions("Use vanilla streetlight placement", "Generates the lights with a standard repeat distance")]
	public static bool VanillaStreetLightPlacement { get => _vanillaStreetLightPlacement; set => _vanillaStreetLightPlacement.value = value; }

	[ModOptions("Streetlight repeat distance", "Changes the distance between each street light", 6, 64, 0.5F, "m")]
	public static float TreeRepeatDistance { get => _treeRepeatDistance.value; set => _treeRepeatDistance.value = value; }

	[ModOptions("Randomize tree distances")]
	public static bool RandomizeTreeDistance { get => _randomiseTreeDistance; set => _randomiseTreeDistance.value = value; }

	[ModOptions("Use vanilla tree placement", "Generates the trees with a standard repeat distance")]
	public static bool VanillaTreePlacement { get => _vanillaTreePlacement; set => _vanillaTreePlacement.value = value; }

	[ModOptions("Only add trees to ground elevation", "Only adds the trees on the ground elevation")]
	public static bool GroundOnlyTrees { get => _groundOnlyTrees; set => _groundOnlyTrees.value = value; }

	[ModOptions("Disable the auto-fill of information and thumbnails in the save panel")]
	public static bool DisableAutoFillInTheSavePanel { get => _disableAutoFillInTheSavePanel; set => _disableAutoFillInTheSavePanel.value = value; }

	public static LaneSizeOptions LaneSizes { get; } = new LaneSizeOptions();
}
