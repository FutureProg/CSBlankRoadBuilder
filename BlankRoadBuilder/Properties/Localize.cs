namespace BlankRoadBuilder
{
	public class Localize
	{
		public static System.Globalization.CultureInfo Culture {get; set;}
		public static ModsCommon.LocalizeManager LocaleManager {get;} = new ModsCommon.LocalizeManager("Localize", typeof(Localize).Assembly);

		/// <summary>
		/// [NEW] The generated thumbnail has been redesigned to be more realistic and visually accurate.
		/// </summary>
		public static string Mod_WhatsNewMessage1_0_0 => LocaleManager.GetString("Mod_WhatsNewMessage1_0_0", Culture);

		/// <summary>
		/// [NEW] Added an option to remove the curb texture from flat roads.
		/// </summary>
		public static string Mod_WhatsNewMessage1_0_1 => LocaleManager.GetString("Mod_WhatsNewMessage1_0_1", Culture);

		/// <summary>
		/// [UPDATED] Traffic lights' drivable area calculations no longer stop on Empty lanes.
		/// </summary>
		public static string Mod_WhatsNewMessage1_0_2 => LocaleManager.GetString("Mod_WhatsNewMessage1_0_2", Culture);

		/// <summary>
		/// [NEW] A new 'Fire Hydrant' add-on option has been added.
		/// </summary>
		public static string Mod_WhatsNewMessage1_0_3 => LocaleManager.GetString("Mod_WhatsNewMessage1_0_3", Culture);

		/// <summary>
		/// [NEW] Added AN toggles for curbless and nodeless variants.
		/// </summary>
		public static string Mod_WhatsNewMessage1_0_4 => LocaleManager.GetString("Mod_WhatsNewMessage1_0_4", Culture);

		/// <summary>
		/// [NEW] AN markings have been replaced with Vanilla markings.
		/// </summary>
		public static string Mod_WhatsNewMessage1_1_0 => LocaleManager.GetString("Mod_WhatsNewMessage1_1_0", Culture);

		/// <summary>
		/// [NEW] Added a limit of 250 lanes while generating a road, roads with more lanes than this limit brea
		/// </summary>
		public static string Mod_WhatsNewMessage1_1_1 => LocaleManager.GetString("Mod_WhatsNewMessage1_1_1", Culture);

		/// <summary>
		/// [UPDATED] Improved the save panel integration.
		/// </summary>
		public static string Mod_WhatsNewMessage1_1_2 => LocaleManager.GetString("Mod_WhatsNewMessage1_1_2", Culture);

		/// <summary>
		/// [NEW] Adding flower pots to lanes with grass on them now adds planted flowers instead.
		/// </summary>
		public static string Mod_WhatsNewMessage1_2_0 => LocaleManager.GetString("Mod_WhatsNewMessage1_2_0", Culture);

		/// <summary>
		/// [NEW] Added a reset button for the general options in-game.
		/// </summary>
		public static string Mod_WhatsNewMessage1_2_1 => LocaleManager.GetString("Mod_WhatsNewMessage1_2_1", Culture);

		/// <summary>
		/// [NEW] Added a TM option to display the road width in units when possible.
		/// </summary>
		public static string Mod_WhatsNewMessage1_2_2 => LocaleManager.GetString("Mod_WhatsNewMessage1_2_2", Culture);

		/// <summary>
		/// [NEW] Added a reset button in-game for lane sizes.
		/// </summary>
		public static string Mod_WhatsNewMessage1_2_3 => LocaleManager.GetString("Mod_WhatsNewMessage1_2_3", Culture);

		/// <summary>
		/// [NEW] Added a small label on the thumbnails in-game to indicate which version the road was generated
		/// </summary>
		public static string Mod_WhatsNewMessage1_2_4 => LocaleManager.GetString("Mod_WhatsNewMessage1_2_4", Culture);

		/// <summary>
		/// [NEW] Added a better status filter in the build panel.
		/// </summary>
		public static string Mod_WhatsNewMessage1_2_5 => LocaleManager.GetString("Mod_WhatsNewMessage1_2_5", Culture);

		/// <summary>
		/// [NEW] Added a new section in the mod’s settings to completely customise the props, trees & pillars u
		/// </summary>
		public static string Mod_WhatsNewMessage1_3_0 => LocaleManager.GetString("Mod_WhatsNewMessage1_3_0", Culture);

		/// <summary>
		/// [NEW] Added a pillar offset option for bridge pillars.
		/// </summary>
		public static string Mod_WhatsNewMessage1_3_1 => LocaleManager.GetString("Mod_WhatsNewMessage1_3_1", Culture);

		/// <summary>
		/// [UPDATED] Minimum distance to a transit stop now takes in consideration parking lanes.
		/// </summary>
		public static string Mod_WhatsNewMessage1_3_2 => LocaleManager.GetString("Mod_WhatsNewMessage1_3_2", Culture);

		/// <summary>
		/// [NEW] Added the option to customize the minimum distance for a stop value.
		/// </summary>
		public static string Mod_WhatsNewMessage1_3_3 => LocaleManager.GetString("Mod_WhatsNewMessage1_3_3", Culture);

		/// <summary>
		/// [NEW] Added IMT marking damage, enabled by default.
		/// </summary>
		public static string Mod_WhatsNewMessage1_3_4 => LocaleManager.GetString("Mod_WhatsNewMessage1_3_4", Culture);

		/// <summary>
		/// [NEW] Reworked how stop lanes are calculated, the mod will now analyze and map out your road to accu
		/// </summary>
		public static string Mod_WhatsNewMessage1_3_5 => LocaleManager.GetString("Mod_WhatsNewMessage1_3_5", Culture);

		/// <summary>
		/// [UPDATED] Added an information label advising users to use Fillers over Pedestrian lanes for Transit
		/// </summary>
		public static string Mod_WhatsNewMessage1_3_6 => LocaleManager.GetString("Mod_WhatsNewMessage1_3_6", Culture);

		/// <summary>
		/// [UPDATED] Large stop props on Curb lanes will now follow the same condition as Fillers for them to s
		/// </summary>
		public static string Mod_WhatsNewMessage1_3_7 => LocaleManager.GetString("Mod_WhatsNewMessage1_3_7", Culture);

		/// <summary>
		/// [NEW] Loading into a save-game is now disabled while Road Builder is active, disable road builder to
		/// </summary>
		public static string Mod_WhatsNewMessage1_3_8 => LocaleManager.GetString("Mod_WhatsNewMessage1_3_8", Culture);

		/// <summary>
		/// [FIXED] Compatibility with Patch 1.16.1-f2.
		/// </summary>
		public static string Mod_WhatsNewMessage1_3_9 => LocaleManager.GetString("Mod_WhatsNewMessage1_3_9", Culture);

		/// <summary>
		/// [NEW] Tunnels can now be generated in the editor.
		/// </summary>
		public static string Mod_WhatsNewMessage1_4_0 => LocaleManager.GetString("Mod_WhatsNewMessage1_4_0", Culture);

		/// <summary>
		/// [NEW] Added an option to autmatically remove pedestrian lanes from non-ground levels.
		/// </summary>
		public static string Mod_WhatsNewMessage1_4_1 => LocaleManager.GetString("Mod_WhatsNewMessage1_4_1", Culture);

		/// <summary>
		/// [UPDATED] Raised the tunnels' roof.
		/// </summary>
		public static string Mod_WhatsNewMessage1_4_2 => LocaleManager.GetString("Mod_WhatsNewMessage1_4_2", Culture);

		/// <summary>
		/// [UPDATED] Added an indicator on the currently being edited configuration in the list in TM.
		/// </summary>
		public static string Mod_WhatsNewMessage1_4_3 => LocaleManager.GetString("Mod_WhatsNewMessage1_4_3", Culture);
	}
}