using AlgernonCommons.UI;

using BlankRoadBuilder.Domain;

using ColossalFramework.UI;

using System;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public class RoadOptionsPanel
{
	private readonly RoadBuilderPanel _roadBuilderPanel;
	private UICheckBox removeLeftSidewalk;
	private UICheckBox removeRightSidewalk;
	private UICheckBox disableLeftSidewalkStop;
	private UICheckBox disableRightSidewalkStop;
	private UICheckBox doNotModifyMeshes;

	public RoadOptionsPanel(RoadBuilderPanel roadBuilderPanel)
	{
		_roadBuilderPanel = roadBuilderPanel;

		GenerateUI();

		FillLastOptions();
	}

	private void FillLastOptions()
	{
		removeLeftSidewalk.isChecked = RoadOptions.LastSelected.RemovePedestrianLaneOnTheLeftSidewalk;
		removeRightSidewalk.isChecked = RoadOptions.LastSelected.RemovePedestrianLaneOnTheRightSidewalk;
		disableLeftSidewalkStop.isChecked = RoadOptions.LastSelected.DisableLeftSidewalkStop;
		disableRightSidewalkStop.isChecked = RoadOptions.LastSelected.DisableRightSidewalkStop;
		doNotModifyMeshes.isChecked = RoadOptions.LastSelected.DoNotChangeTheRoadMeshes;
	}

	private void GenerateUI()
	{
		var yIndex = 60F;

		removeLeftSidewalk = UICheckBoxes.AddLabelledCheckBox(_roadBuilderPanel, 32F, yIndex, nameof(RoadOptions.RemovePedestrianLaneOnTheLeftSidewalk).FormatWords(), 16F, 1F);
		yIndex += 42F;

		removeRightSidewalk = UICheckBoxes.AddLabelledCheckBox(_roadBuilderPanel, 32F, yIndex, nameof(RoadOptions.RemovePedestrianLaneOnTheRightSidewalk).FormatWords(), 16F, 1F);
		yIndex += 42F;

		disableLeftSidewalkStop = UICheckBoxes.AddLabelledCheckBox(_roadBuilderPanel, 32F, yIndex, nameof(RoadOptions.DisableLeftSidewalkStop).FormatWords(), 16F, 1F);
		yIndex += 42F;

		disableRightSidewalkStop = UICheckBoxes.AddLabelledCheckBox(_roadBuilderPanel, 32F, yIndex, nameof(RoadOptions.DisableRightSidewalkStop).FormatWords(), 16F, 1F);
		yIndex += 42F;

		doNotModifyMeshes = UICheckBoxes.AddLabelledCheckBox(_roadBuilderPanel, 32F, yIndex, nameof(RoadOptions.DoNotChangeTheRoadMeshes).FormatWords(), 16F, 1F);
		yIndex += 42F;

		_roadBuilderPanel.height = yIndex + 56F;
		_roadBuilderPanel.ContinueButton.relativePosition = new Vector2(_roadBuilderPanel.ContinueButton.relativePosition.x, yIndex + 10);
	}

	public RoadOptions GetOptions()
	{
		return RoadOptions.LastSelected = new RoadOptions
		{
			RemovePedestrianLaneOnTheLeftSidewalk = removeLeftSidewalk.isChecked,
			RemovePedestrianLaneOnTheRightSidewalk = removeRightSidewalk.isChecked,
			DisableLeftSidewalkStop = disableLeftSidewalkStop.isChecked,
			DisableRightSidewalkStop = disableRightSidewalkStop.isChecked,
			DoNotChangeTheRoadMeshes = doNotModifyMeshes.isChecked,
		};
	}

	public void Clear()
	{
		removeLeftSidewalk.isVisible = false;
		removeRightSidewalk.isVisible = false;
		disableLeftSidewalkStop.isVisible = false;
		disableRightSidewalkStop.isVisible = false;
		doNotModifyMeshes.isVisible = false;
	}
}