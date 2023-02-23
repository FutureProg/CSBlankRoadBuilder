using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Util;

using ColossalFramework.UI;

using System;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public class GenerationPanel : UIPanel
{
	public float PanelWidth = 581F;
	public float PanelHeight = 200F;

	public event Action? EventClose;
	public GenerationPanel()
	{
		autoLayout = false;
		canFocus = true;
		isInteractive = true;
		size = new Vector2(PanelWidth, PanelHeight);
		atlas = ResourceUtil.GetAtlas("GenerationPanelBack.png", new UITextureAtlas.SpriteInfo[]
		{
			new()
			{
				name = nameof(ElevationType.Basic),
				region = new Rect(0f, 5 / 6f, 1f, 1 / 6f)
			},
			new()
			{
				name = nameof(ElevationType.Elevated),
				region = new Rect(0f, 4 / 6f, 1f, 1 / 6f)
			},
			new()
			{
				name = nameof(ElevationType.Bridge),
				region = new Rect(0f, 3 / 6f, 1f, 1 / 6f)
			},
			new()
			{
				name = nameof(ElevationType.Slope),
				region = new Rect(0f, 2 / 6f, 1f, 1 / 6f)
			},
			new()
			{
				name = nameof(ElevationType.Tunnel),
				region = new Rect(0f, 1 / 6f, 1f, 1 / 6f)
			},
			new()
			{
				name = "Complete",
				region = new Rect(0f, 0f, 1f, 1 / 6f)
			},
		});
		backgroundSprite = nameof(ElevationType.Basic);
		relativePosition = new Vector2(Mathf.Floor((GetUIView().fixedWidth - base.width) / 2f), Mathf.Floor((GetUIView().fixedHeight - base.height) / 2f));

		var uIDragHandle = AddUIComponent<UIDragHandle>();
		uIDragHandle.size = new Vector2(width, 48);
		uIDragHandle.relativePosition = Vector3.zero;
		uIDragHandle.target = this;

		var closeAtlas = ResourceUtil.GetAtlas("I_Close.png", new UITextureAtlas.SpriteInfo[]
		{
			new()
			{
				name = "normal",
				region = new Rect(0f, 0.5f, 0.5f, 0.5f)
			},
			new()
			{
				name = "hovered",
				region = new Rect(0.5f, 0.5f, 0.5f, 0.5f)
			},
			new()
			{
				name = "pressed",
				region = new Rect(0f, 0f, 0.5f, 0.5f)
			},
		});
		var uIButton = AddUIComponent<UIButton>();
		uIButton.size = new Vector2(30, 30);
		uIButton.relativePosition = new Vector2(width - 35f, 5f);
		uIButton.atlas = closeAtlas;
		uIButton.normalBgSprite = "normal";
		uIButton.hoveredBgSprite = "hovered";
		uIButton.pressedBgSprite = "pressed";
		uIButton.eventClick += delegate
		{
			EventClose?.Invoke();
		};
	}

	public System.Collections.IEnumerator LoadRoad()
	{
		var loadingLabel = GenerateLoadingLabel();
		yield return 0;

		foreach (var stateInfo in RoadBuilderUtil.Build(Road.RoadInfo))
		{
			if (stateInfo.Exception != null)
			{
				Debug.LogException(stateInfo.Exception);

				var panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");

				panel.SetMessage("Failed to generate this road", $"{stateInfo.Exception.Message}\r\n\r\n{stateInfo.Exception}", true);

				EventClose?.Invoke();
				yield break;
			}

			backgroundSprite = (int)stateInfo.ElevationStep == -1 ? "Complete" : stateInfo.ElevationStep.ToString();
			loadingLabel.text = stateInfo.Info;
			yield return 0;
		}

		EventClose?.Invoke();
	}

	private UILabel GenerateLoadingLabel()
	{
		var loadingLabel = AddUIComponent<UILabel>();

		loadingLabel.relativePosition = new Vector2(240, 40);
		loadingLabel.autoSize = false;
		loadingLabel.wordWrap = true;
		loadingLabel.textScale = 1.2F;
		loadingLabel.width = width - 250;
		loadingLabel.height = height - 80;
		loadingLabel.textAlignment = UIHorizontalAlignment.Left;
		loadingLabel.verticalAlignment = UIVerticalAlignment.Middle;

		return loadingLabel;
	}

	internal static void Create(RoadConfigControl road)
	{
		try
		{
			if (s_gameObject == null)
			{
				s_gameObject = new GameObject(typeof(GenerationPanel).Name);
				s_gameObject.transform.parent = UIView.GetAView().transform;
				s_panel = s_gameObject.AddComponent<GenerationPanel>();
				s_panel.Road = road;
				s_panel.EventClose += DestroyPanel;
				s_panel.StartCoroutine(s_panel.LoadRoad());
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}


	private static GameObject s_gameObject;

	private static GenerationPanel s_panel;

	internal RoadConfigControl Road { get; private set; }

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
}
