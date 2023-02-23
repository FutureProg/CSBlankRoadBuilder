using BlankRoadBuilder.Util;

using ColossalFramework;
using ColossalFramework.UI;

using System;
using System.Diagnostics;
using System.IO;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public class EditorPanel : UIPanel
{
	private readonly SlickButton BuildButton;
	private readonly SlickButton TMButton;
	private readonly SlickButton TMFolderButton;
	private readonly SlickButton RoadFolderButton;

	public bool Destroyed { get; private set; }

	public EditorPanel()
	{
		autoLayout = false;
		canFocus = false;
		isInteractive = true;
		atlas = ResourceUtil.GetAtlas("EditorPanelBack.png");
		backgroundSprite = "normal";
		size = new Vector2(300, 100);

		var uIDragHandle = AddUIComponent<UIDragHandle>();
		uIDragHandle.width = width;
		uIDragHandle.height = height;
		uIDragHandle.relativePosition = Vector3.zero;
		uIDragHandle.target = this;
		uIDragHandle.SendToBack();

		BuildButton = AddUIComponent<SlickButton>();
		BuildButton.textScale = 0.85F;
		BuildButton.size = new Vector2(175, 30);
		BuildButton.relativePosition = new Vector2(72, 15);
		BuildButton.text = "Build Road";
		BuildButton.tooltip = "Import a road you've created in Thumbnail Maker";
		BuildButton.SetIcon("I_Tools.png");
		BuildButton.eventClick += BuildButton_Click;

		TMButton = AddUIComponent<SlickButton>();
		TMButton.textScale = 0.85F;
		TMButton.size = new Vector2(175, 30);
		TMButton.relativePosition = new Vector2(72, 55);
		TMButton.text = "Thumbnail Maker";
		TMButton.tooltip = "Opens the Thumbnail Maker tool used to create road configurations";
		TMButton.SetIcon("I_Brush.png");
		TMButton.eventClick += TMButton_Click;

		RoadFolderButton = AddUIComponent<SlickButton>();
		RoadFolderButton.size = new Vector2(30, 30);
		RoadFolderButton.relativePosition = new Vector2(260, 15);
		RoadFolderButton.tooltip = "Open the folder where road configurations are stored";
		RoadFolderButton.SetIcon("I_Folder.png");
		RoadFolderButton.eventClick += RoadFolderButton_Click;

		TMFolderButton = AddUIComponent<SlickButton>();
		TMFolderButton.size = new Vector2(30, 30);
		TMFolderButton.relativePosition = new Vector2(260, 55);
		TMFolderButton.tooltip = "Open the folder where Thumbnail Maker is stored";
		TMFolderButton.SetIcon("I_Folder.png");
		TMFolderButton.eventClick += TMFolderButton_Click;

		relativePosition = new Vector2(30, 30);
	}

	private void TMFolderButton_Click(UIComponent component, UIMouseEventParameter eventParam)
	{
		Utils.OpenInFileBrowser(BlankRoadBuilderMod.ThumbnailMakerFolder);
	}

	private void RoadFolderButton_Click(UIComponent component, UIMouseEventParameter eventParam)
	{
		Directory.CreateDirectory(Path.Combine(BlankRoadBuilderMod.BuilderFolder, "Roads"));

		Utils.OpenInFileBrowser(Path.Combine(BlankRoadBuilderMod.BuilderFolder, "Roads"));
	}

	private void TMButton_Click(UIComponent component, UIMouseEventParameter eventParam)
	{
		var tm = Path.Combine(BlankRoadBuilderMod.ThumbnailMakerFolder, "ThumbnailMaker.exe");
		var openTMs = false;

		try
		{
			openTMs = Process.GetProcessesByName("ThumbnailMaker").Length > 0;
		}
		catch { }

		try
		{
			if (openTMs)
				File.WriteAllText(Path.Combine(BlankRoadBuilderMod.ThumbnailMakerFolder, "Wake"), "It's time to wake up");
			else if (File.Exists(tm))
				Process.Start(tm);
			else
			{
				var panel = UIView.library.ShowModal<ExceptionPanel>("ThumbnailMakerMissing");
				panel.SetMessage("Thumbnail Maker Missing", "The thumbnail maker application is missing from your computer.\r\n\r\nThis may be caused by missing files in the mod folder, or that your antivirus removed it.", true);
			}
		}
		catch (Exception ex) { UnityEngine.Debug.LogException(ex); }
	}

	private void BuildButton_Click(UIComponent component, UIMouseEventParameter eventParam)
	{
		try
		{
			if (s_gameObject == null)
			{
				s_gameObject = new GameObject(typeof(RoadBuilderPanel).Name);
				s_gameObject.transform.parent = UIView.GetAView().transform;
				s_panel = s_gameObject.AddComponent<RoadBuilderPanel>();
				s_panel.EventClose += DestroyPanel;
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}


	private static GameObject s_gameObject;

	private static RoadBuilderPanel s_panel;

	private static void DestroyPanel()
	{
		if (!(s_panel == null))
		{
			Destroy(s_panel);
			Destroy(s_gameObject);
			s_panel = null;
			s_gameObject = null;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		Destroyed = true;
	}
}
