using ColossalFramework.UI;

using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public static class UIFonts
{
	private static UIFont s_regular;

	private static UIFont s_semiBold;

	public static UIFont Regular
	{
		get
		{
			if (s_regular == null)
			{
				s_regular = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault((UIFont f) => f.name == "OpenSans-Regular");
			}

			return s_regular;
		}
	}

	public static UIFont SemiBold
	{
		get
		{
			if (s_semiBold == null)
			{
				s_semiBold = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault((UIFont f) => f.name == "OpenSans-Semibold");
			}

			return s_semiBold;
		}
	}
}