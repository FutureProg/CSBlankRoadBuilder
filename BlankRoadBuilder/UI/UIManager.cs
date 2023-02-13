using ColossalFramework.UI;

using ModsCommon;

using System.IO;

using UnityEngine;

namespace BlankRoadBuilder.UI;
public class UIManager : MonoBehaviour
{
	private static EditorPanel? currentPanel;

	public void Awake()
	{
		Init();
	}

	public void Init()
	{
		var current = UIView.GetAView().transform.GetComponent<EditorPanel>();

		if (current != null || !(currentPanel?.Destroyed ?? true))
			return;

		var gameObject = new GameObject(typeof(EditorPanel).Name);
		gameObject.transform.parent = UIView.GetAView().transform;
		currentPanel = gameObject.AddComponent<EditorPanel>();
		SingletonMod<BlankRoadBuilderMod>.Instance.ShowWhatsNew();
	}
}
