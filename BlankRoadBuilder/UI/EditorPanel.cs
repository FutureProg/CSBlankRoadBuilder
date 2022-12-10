using AlgernonCommons.UI;

using ColossalFramework;
using ColossalFramework.UI;

using System;
using System.Diagnostics;
using System.IO;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public class EditorPanel : UIPanel
{
	private SlickButton BuildButton;
	private SlickButton TMButton;
	private SlickButton TMFolderButton;
	private SlickButton RoadFolderButton;

	public bool Destroyed { get; private set; }

	public EditorPanel()
	{
		autoLayout = false;
		canFocus = true;
		isInteractive = true;
		atlas = UITextures.InGameAtlas;
		backgroundSprite = "UnlockingPanel2";
		color = new Color32(210, 229, 247, 255);
		size = new Vector2(475, 155);

		var uIDragHandle = AddUIComponent<UIDragHandle>();
		uIDragHandle.width = width;
		uIDragHandle.height = height;
		uIDragHandle.relativePosition = Vector3.zero;
		uIDragHandle.target = this;
		uIDragHandle.SendToBack();

		var icon = AddUIComponent<UISprite>();
		icon.relativePosition = new Vector2(8, 12);
		icon.size = new Vector2(128, 128);
		icon.atlas = UITextures.LoadSprite(Path.Combine(Path.Combine(BlankRoadBuilderMod.ModFolder, "Icons"), "Icon"));
		icon.spriteName = "normal";
		icon.SendToBack();

		BuildButton = AddUIComponent<SlickButton>();
		BuildButton.size = new Vector2(250, 36);
		BuildButton.relativePosition = new Vector2(145, 30);
		BuildButton.text = "Build Blank Road";
		BuildButton.tooltip = "Import a road you've created in Thumbnail Maker";
		BuildButton.SetIcon("I_Tools.png");
		BuildButton.eventClick += BuildButton_Click;

		TMButton = AddUIComponent<SlickButton>();
		TMButton.size = new Vector2(250, 36);
		TMButton.relativePosition = new Vector2(145, 90);
		TMButton.text = "Open Thumbnail Maker";
		TMButton.tooltip = "Opens the Thumbnail Maker tool used to create road configurations";
		TMButton.SetIcon("I_Brush.png");
		TMButton.eventClick += TMButton_Click;

		RoadFolderButton = AddUIComponent<SlickButton>();
		RoadFolderButton.size = new Vector2(36, 36);
		RoadFolderButton.relativePosition = new Vector2(415, 30);
		RoadFolderButton.text = "RoadFolderButton";
		RoadFolderButton.tooltip = "Open the folder where road configurations are stored";
		RoadFolderButton.SetIcon("I_Folder.png");
		RoadFolderButton.eventClick += RoadFolderButton_Click;

		TMFolderButton = AddUIComponent<SlickButton>();
		TMFolderButton.size = new Vector2(36, 36);
		TMFolderButton.relativePosition = new Vector2(415, 90);
		TMFolderButton.text = "TMFolderButton";
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
		var openTMs = Process.GetProcessesByName("ThumbnailMaker");

		if (openTMs.Length > 0)
			File.WriteAllText(Path.Combine(BlankRoadBuilderMod.ThumbnailMakerFolder, "Wake"), "It's time to wake up");
		else
			Process.Start(Path.Combine(BlankRoadBuilderMod.ThumbnailMakerFolder, "ThumbnailMaker.exe"));
	}

	private void BuildButton_Click(UIComponent component, UIMouseEventParameter eventParam)
	{
		StandalonePanelManager<RoadBuilderPanel>.Create();
		StandalonePanelManager<RoadBuilderPanel>.Panel.RefreshList(true);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		Destroyed = true;
	}
}
