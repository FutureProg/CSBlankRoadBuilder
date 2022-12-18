using AlgernonCommons.UI;
using BlankRoadBuilder.ThumbnailMaker;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlankRoadBuilder.UI;
public class TransitStopOptionsPanel : StandalonePanel
{
    public override float PanelWidth => 700f;

    public override float PanelHeight => 800f;

    protected override string PanelTitle => "BRB - Transit Stop Options";

    public enum StopPosition
    {
        None = 0,
        Left,
        Right
    };

    UILabel _roadNameLabel;
    UIDropDown _leftPedLane;
    UIDropDown _rightPedLane;
    UIButton _addStopButton;
    UIList _stopsUIList;

    protected static string[] LaneLabels;    

    static Dictionary<string, VehicleInfo.VehicleType> StopTypeOptions;
    static Dictionary<string, StopPosition> StopPositions;

    List<ListData> _listData;
    List<LaneInfo?> _lanes;
    LaneInfo _leftSidewalk;
    LaneInfo _rightSidewalk;

    static TransitStopOptionsPanel() {
        StopTypeOptions = new Dictionary<string, VehicleInfo.VehicleType>();
        StopTypeOptions["None"] = VehicleInfo.VehicleType.None;
        StopTypeOptions["Bus"] = VehicleInfo.VehicleType.Car;
        StopTypeOptions["Tram"] = VehicleInfo.VehicleType.Tram;
        StopTypeOptions["Bus + Tram"] = VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.Tram;

        StopPositions = new Dictionary<string, StopPosition>();
        StopPositions["None"] = StopPosition.None;
        StopPositions["Left"] = StopPosition.Left;
        StopPositions["Right"] = StopPosition.Right;
        StopPositions["Both"] = StopPosition.Left | StopPosition.Right;
    }

    public TransitStopOptionsPanel()
    {
        var rowMargin = 10f;
        var headerHeight = 40f;
        var dropdownHeight = 25f;
        var dropdownLabelHeight = 8f;
        var stopTypeArr = StopTypeOptions.Keys.ToArray<string>();
        _listData = new List<ListData>();
        _lanes = new List<LaneInfo>();

        _roadNameLabel = UILabels.AddLabel(this, Margin, headerHeight, "Select a road", width: PanelWidth - Margin * 2);

        var y = headerHeight + 10f + rowMargin + dropdownLabelHeight;
        _leftPedLane = UIDropDowns.AddLabelledDropDown(this, Margin, y, "Left Sidewalk", height: dropdownHeight);
        _leftPedLane.items = stopTypeArr;

        y += dropdownHeight + rowMargin + dropdownLabelHeight;
        _rightPedLane = UIDropDowns.AddLabelledDropDown(this, Margin, y, "Right Sidewalk", height: dropdownHeight);
        _rightPedLane.items = stopTypeArr;

        y += dropdownHeight + rowMargin;
        _addStopButton = UIButtons.AddSmallerButton(this, Margin, y, "Add Stop");
        _addStopButton.eventClick += _addStopButton_eventClick;

        y += 30f;
        _stopsUIList = UIList.AddUIList<ListRow>(this, Margin, y, PanelWidth - Margin, PanelHeight - y - Margin, rowHeight: ListRow.DefaultRowHeight);        
        

        _leftPedLane.Disable();
        _rightPedLane.Disable();
        _addStopButton.Disable();

        var roadListPanel = StandalonePanelManager<RoadBuilderPanel>.Panel;
        if (roadListPanel != null)
        {
            roadListPanel.EventClose += () => this.Close();
        }
    }

    /**
     * Get the relative position (left, right, both) and type of transit stop for the lane. (not tested <_<)
     */
    public bool GetStopsForLane(LaneInfo lane, out ResultsPackage[] results)
    {
        List<ResultsPackage> resultList = new List<ResultsPackage>();
        if (_lanes.Contains(lane) && lane != null) {            
            foreach(var item in _listData)
            {
                if (item.laneInfo == lane)
                {
                    if (item.stopPosition == StopPosition.None 
                        || item.selectedStopType == VehicleInfo.VehicleType.None)
                    {
                        continue;
                    }
                    var r = new ResultsPackage
                    {
                        stopPositions = item.stopPosition,
                        vehicleTypes = item.selectedStopType
                    };
                    resultList.Add(r);
                }
            }
        }
        
        if (lane == _leftSidewalk && _leftPedLane.selectedIndex != 0)
        {
            var r = new ResultsPackage
            {
                stopPositions = StopPosition.Right,
                vehicleTypes = StopTypeOptions[_leftPedLane.selectedValue]
            };
            resultList.Add(r);
        }
        if (lane == _rightSidewalk && _rightPedLane.selectedIndex != 0)
        {
            resultList.Add(new ResultsPackage {
                stopPositions = StopPosition.Left,
                vehicleTypes = StopTypeOptions[_rightPedLane.selectedValue] 
            });
        }

        results = resultList.ToArray();
        return results.Length > 0;
    }

    private void _addStopButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
    {
        // Add New Stop
        var nListData = new ListData();
        _listData.Add(nListData);
        RefreshListView();
    }    

    public void SetRoadInfo(string? file, RoadInfo? roadInfo)
    { 
        if (roadInfo.RoadType != RoadType.Road)
        {
            throw new NotImplementedException("Transit Stop Options Panel isn't setup for road type " + roadInfo.RoadType.ToString());
        }
        _listData.Clear();
        _lanes.Clear();
        _stopsUIList.Clear();

        List<string> laneLabels = new List<string>();

        // Get the lane text etc.
        int startIndex = 3; // ignore the left sidewalk, filler lane, and decoration lane
        int endIndex = roadInfo.Lanes.Count - 2; // ignore the right sidewalk and the filler lane
        _leftSidewalk = roadInfo.Lanes[startIndex-1];
        _rightSidewalk = roadInfo.Lanes[endIndex];
        _lanes.Add(null);
        laneLabels.Add("None");
        for (int i = startIndex; i < endIndex; i++)
        {
            LaneInfo lane = roadInfo.Lanes[i];
            if (((lane.Type & LaneType.Pedestrian) != 0 ||  (lane.Type & LaneType.Parking) != 0) && // if it's a pedestrian, filler, or parking lane
                ((lane.Tags & LaneTag.StoppableVehicleOnRight) != 0 || (lane.Tags & LaneTag.StoppableVehicleOnLeft) != 0)) // if there's a vehicle stop on the left or right
            {
                laneLabels.Add(lane.Type.ToString() + " Lane, " + (i - startIndex) + " from the left sidewalk.");
                _lanes.Add(lane);                
            }
        }
        LaneLabels = laneLabels.ToArray<string>();
        _leftPedLane.Enable();
        _leftPedLane.selectedIndex = 0;
        _rightPedLane.Enable();
        _rightPedLane.selectedIndex = 0;
        _addStopButton.Enable();
        _roadNameLabel.text = "Road: " + roadInfo.Name;
        this.RefreshListView();
    }

    public void RefreshListView()
    {
        _stopsUIList.Data = new FastList<object>()
        {
            m_buffer = _listData.ToArray(),
            m_size = _listData.Count
        };
        _stopsUIList.Refresh();
    }

    private static void OnRemoveRow(int rowIndex)
    {
        var instance = StandalonePanelManager<TransitStopOptionsPanel>.Panel;
        instance._listData.RemoveAt(rowIndex);
        instance.RefreshListView();
    }

    private static void OnChooseLane(int rowIndex, int laneIndex)
    {
        var instance = StandalonePanelManager<TransitStopOptionsPanel>.Panel;
        ListData ld = instance._listData[rowIndex];
        ld.laneInfo = instance._lanes[laneIndex];
        ld.selectedLaneIndex = laneIndex;
        instance._stopsUIList.Refresh();
    }

    private static void OnSetStopSide(int rowIndex, int selectedIndex, StopPosition stopPosition)
    {
        var instance = StandalonePanelManager<TransitStopOptionsPanel>.Panel;
        ListData ld = instance._listData[rowIndex];
        ld.stopPosition = stopPosition;
        ld.selectedStopPositionIndex = selectedIndex;
        instance._stopsUIList.Refresh();
    }

    private static void OnSetStopType(int rowIndex, int selectedIndex, VehicleInfo.VehicleType vehicleType)
    {
        var instance = StandalonePanelManager<TransitStopOptionsPanel>.Panel;
        ListData ld = instance._listData[rowIndex];
        ld.selectedStopTypeIndex = selectedIndex;
        ld.selectedStopType = vehicleType;
        instance._stopsUIList.Refresh();
    }

    public class ResultsPackage
    {
        public VehicleInfo.VehicleType vehicleTypes;
        public StopPosition stopPositions;
    }

    private class ListData
    {
        public int selectedLaneIndex = -1;
        public VehicleInfo.VehicleType selectedStopType;
        public int selectedStopTypeIndex = -1;
        public StopPosition stopPosition;
        public int selectedStopPositionIndex = -1;
        public LaneInfo? laneInfo;

        public override bool Equals(object? obj)
        {
            return obj is ListData data &&
                   selectedLaneIndex == data.selectedLaneIndex &&
                   selectedStopTypeIndex == data.selectedStopTypeIndex &&
                   selectedStopPositionIndex == data.selectedStopPositionIndex;
        }
    }

    private class ListRow : UIListRow
    {

        public static float DefaultRowHeight = 35 * 3 + Margin + 8f + 34f;

        UIDropDown _laneDropdown;
        UIDropDown _stopTypeDropdown;
        UIDropDown _stopPositionDropdown;
        UIButton _removeButton;

        ListData _currentData;
        int _rowIndex;

        public ListRow()
        {
            canFocus = false;
            isInteractive = false;            
        }

        public override void Display(object data, int rowIndex)
        {
            if (data is not ListData)
            {
                return;
            }
            var ldata = data as ListData;

            var y = Margin + 8f;
            var x = Margin;
            _laneDropdown ??= UIDropDowns.AddLabelledDropDown(this, x, y, "Select a Lane", width: 240);

            y += 35f;
            _stopTypeDropdown ??= UIDropDowns.AddLabelledDropDown(this, x, y, "Vehicle Type");

            y += 35f;
            _stopPositionDropdown ??= UIDropDowns.AddLabelledDropDown(this, x, y, "Stop Position");

            y += 35f;
            _removeButton ??= UIButtons.AddSmallerButton(this, x, y, "Delete");            
            

            _laneDropdown.items = LaneLabels;
            _stopTypeDropdown.items = StopTypeOptions.Keys.ToArray();

            FastList<string> options = new FastList<string>();
            options.Add("None");
            if (ldata.laneInfo != null)
            {
                if ((ldata.laneInfo.Tags & LaneTag.StoppableVehicleOnLeft) != 0)
                {
                    options.Add("Left");
                }
                if ((ldata.laneInfo.Tags & LaneTag.StoppableVehicleOnRight) != 0)
                {
                    options.Add("Right");
                }
                if (ldata.laneInfo.Tags.HasFlag(LaneTag.StoppableVehicleOnRight) && ldata.laneInfo.Tags.HasFlag(LaneTag.StoppableVehicleOnLeft))
                {
                    options.Add("Both");
                }
            }
            _stopPositionDropdown.items = options.ToArray();

            
            if (_currentData == null || !ldata.Equals(_currentData) || rowIndex != _rowIndex)
            {
                // Remove Delegates before we update
                _removeButton.eventClick -= _removeButton_eventClick;
                _laneDropdown.eventSelectedIndexChanged -= _laneDropdown_eventSelectedIndexChanged;
                _stopTypeDropdown.eventSelectedIndexChanged -= _stopTypeDropdown_eventSelectedIndexChanged;
                _stopPositionDropdown.eventSelectedIndexChanged -= _stopPositionDropdown_eventSelectedIndexChanged;

                // Update the values
                _laneDropdown.selectedIndex = ldata.selectedLaneIndex > -1 ? ldata.selectedLaneIndex : 0;
                _stopTypeDropdown.selectedIndex = ldata.laneInfo != null && ldata.selectedStopTypeIndex > -1 ? ldata.selectedStopTypeIndex : 0;
                _stopPositionDropdown.selectedIndex = ldata.laneInfo != null && ldata.selectedStopPositionIndex > -1 ? ldata.selectedStopPositionIndex : 0;

                // Determine if the dropdowns should be visible
                _stopTypeDropdown.enabled = ldata.laneInfo != null;
                _stopPositionDropdown.enabled = _stopTypeDropdown.enabled && _stopTypeDropdown.selectedValue != "None";

                // Add the delegates back
                _removeButton.eventClick += _removeButton_eventClick;
                _laneDropdown.eventSelectedIndexChanged += _laneDropdown_eventSelectedIndexChanged;
                _stopTypeDropdown.eventSelectedIndexChanged += _stopTypeDropdown_eventSelectedIndexChanged;
                _stopPositionDropdown.eventSelectedIndexChanged += _stopPositionDropdown_eventSelectedIndexChanged;

                _currentData = ldata;
                _rowIndex = rowIndex;
            }            
        }

        private void _stopPositionDropdown_eventSelectedIndexChanged(UIComponent component, int value)
        {
            TransitStopOptionsPanel.OnSetStopSide(_rowIndex, _stopPositionDropdown.selectedIndex, StopPositions[_stopPositionDropdown.selectedValue]);
        }

        private void _stopTypeDropdown_eventSelectedIndexChanged(UIComponent component, int value)
        {
            TransitStopOptionsPanel.OnSetStopType(_rowIndex, _stopTypeDropdown.selectedIndex, StopTypeOptions[_stopTypeDropdown.selectedValue]);
        }

        private void _removeButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            TransitStopOptionsPanel.OnRemoveRow(_rowIndex);
        }

        private void _laneDropdown_eventSelectedIndexChanged(UIComponent component, int value)
        {
            TransitStopOptionsPanel.OnChooseLane(_rowIndex, _laneDropdown.selectedIndex);
        }
    }

}


//var rowMargin = 10f;
//var headerHeight = 40f;
//var dropdownHeight = 25f;
//var dropdownLabelHeight = 8f;
//var dropdownWidth = 220f;
//var startY = headerHeight + dropdownHeight * 2 + rowMargin * 2 + dropdownLabelHeight * 2;
//var dY = dropdownHeight + rowMargin + dropdownLabelHeight;
//int startIndex = 3; // ignore the left sidewalk, filler lane, and decoration lane
//int endIndex = roadInfo.Lanes.Count - 2; // ignore the right sidewalk and the filler lane
//for (int i = startIndex; i < endIndex; i++)
//{
//    LaneInfo lane = roadInfo.Lanes[i];
//    if (((lane.Type & LaneType.Pedestrian) != 0 || lane.IsFiller()) && // if it's a pedestrian or filler lane
//        (lane.Tags & (LaneTag.StoppableVehicleOnRight | LaneTag.StoppableVehicleOnLeft)) != 0) // if there's a vehicle stop on the left or right
//    {
//        var dropDown = UIDropDowns.AddLabelledDropDown(this, Margin, startY + (dY * i), lane.Type.ToString() + " Lane, " + (i - startIndex) + " from the left sidewalk.");
//        _islandStops.Add(dropDown);
//        _islandStopIndices.Add(i);


//        dropDown = UIDropDowns.AddDropDown(this, Margin, startY + (dY * i) + dropdownWidth + 5f);                
//        dropDown.items = new string[] { "On Left", "On Right", "Both Sides" };
//    }
//}