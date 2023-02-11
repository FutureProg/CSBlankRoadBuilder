using UnityEngine.SceneManagement;

namespace BlankRoadBuilder.Util;
internal class Utilities
{
	public static bool InGame => !OnStartup && !OnMenu;
	public static bool OnGame => SceneManager.GetActiveScene().name is string scene && scene == "Game";
	public static bool OnMenu => SceneManager.GetActiveScene().name is string scene && scene == "MainMenu";
	public static bool OnStartup => SceneManager.GetActiveScene().name is string scene && scene == "Startup";
}
