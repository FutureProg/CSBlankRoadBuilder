namespace BlankRoadBuilder.UI;

using AlgernonCommons.UI;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util;
using ColossalFramework.Importers;
using ColossalFramework.UI;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using UnityEngine;

public class UIManager : MonoBehaviour
{
	private static EditorPanel? currentPanel;

	public void Awake() => Init();

	public void Init()
	{
		var current = UIView.GetAView().transform.GetComponent<EditorPanel>();

		if (current != null || !(currentPanel?.Destroyed ?? true))
			return;

		var gameObject = new GameObject(typeof(EditorPanel).Name);
		gameObject.transform.parent = UIView.GetAView().transform;
		currentPanel = gameObject.AddComponent<EditorPanel>();
	}
}
