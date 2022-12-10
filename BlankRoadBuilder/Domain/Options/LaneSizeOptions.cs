﻿using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace BlankRoadBuilder.Domain.Options;
public class LaneSizeOptions
{
    private readonly Dictionary<LaneType, float> _sizes;
    private float _diagonalParkingSize;
    private float _horizontalParkingSize;

    public float DiagonalParkingSize
    {
        get => _diagonalParkingSize;
        set
        {
            _diagonalParkingSize = value;
            Save();
        }
    }

    public float HorizontalParkingSize
    {
        get => _horizontalParkingSize;
        set
        {
            _horizontalParkingSize = value;
            Save();
        }
    }

    public float this[LaneType l]
    {
        get => _sizes[l];
        set
        {
            _sizes[l] = value;
            Save();
        }
    }

    public LaneSizeOptions()
    {
        try
        {
            if (File.Exists(Path.Combine(BlankRoadBuilderMod.BuilderFolder, "LaneSizes.xml")))
            {
                var x = new XmlSerializer(typeof(SavedSettings));

                using var stream = File.OpenRead(Path.Combine(BlankRoadBuilderMod.BuilderFolder, "LaneSizes.xml"));
                var savedSettings = (SavedSettings)x.Deserialize(stream);

                _diagonalParkingSize = savedSettings.DiagonalParkingSize;
                _horizontalParkingSize = savedSettings.HorizontalParkingSize;
                _sizes = new Dictionary<LaneType, float>();

                foreach (LaneType laneType in Enum.GetValues(typeof(LaneType)))
                {
                    var index = savedSettings.LaneTypes.IndexOf((int)laneType);

                    _sizes[laneType] = index != -1 ? savedSettings.LaneSizes[index] : GetDefaultLaneWidth(laneType);
                }

                return;
            }
        }
        catch { }

        _sizes = Enum.GetValues(typeof(LaneType)).Cast<LaneType>().ToDictionary(x => x, GetDefaultLaneWidth);
        _diagonalParkingSize = 4F;
        _horizontalParkingSize = 5.5F;
    }

    public void Save()
    {
        var xML = new XmlSerializer(typeof(SavedSettings));

        using var stream = File.Create(Path.Combine(BlankRoadBuilderMod.BuilderFolder, "LaneSizes.xml"));

        xML.Serialize(stream, new SavedSettings
        {
            DiagonalParkingSize = _diagonalParkingSize,
            HorizontalParkingSize = _horizontalParkingSize,
            LaneTypes = _sizes.Keys.Cast<int>().ToList(),
            LaneSizes = _sizes.Values.ToList(),
        });
    }

    public static float GetDefaultLaneWidth(LaneType type)
    {
        return type switch
        {
            LaneType.Empty or LaneType.Grass or LaneType.Pavement or LaneType.Gravel => 3F,
            LaneType.Trees => 4F,
            LaneType.Tram or LaneType.Car or LaneType.Trolley or LaneType.Emergency => 3F,
            LaneType.Pedestrian => 2F,
            LaneType.Bike => 2F,
            LaneType.Parking => 2F,
            LaneType.Highway or LaneType.Bus or LaneType.Train => 4F,
            _ => 3F,
        };
    }

    public class SavedSettings
    {
        public float DiagonalParkingSize { get; set; }
        public float HorizontalParkingSize { get; set; }
        public List<int> LaneTypes { get; set; }
        public List<float> LaneSizes { get; set; }
    }
}