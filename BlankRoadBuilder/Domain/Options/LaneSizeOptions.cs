using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace BlankRoadBuilder.Domain.Options;
public class LaneSizeOptions
{
    private Dictionary<LaneType, float> _sizes;
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

    public LaneSizeOptions() { Update(); }

    public void Update()
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
            Version = 1,
            DiagonalParkingSize = _diagonalParkingSize,
            HorizontalParkingSize = _horizontalParkingSize,
            LaneTypes = _sizes.Keys.Cast<int>().ToList(),
            LaneSizes = _sizes.Values.ToList(),
        });
    }

    public static float GetDefaultLaneWidth(LaneType type)
	{
		switch (type)
		{
			case LaneType.Filler:
				return 3F;

			case LaneType.Tram:
			case LaneType.Car:
			case LaneType.Bus:
			case LaneType.Emergency:
				return 3F;

			case LaneType.Pedestrian:
				return 2F;

			case LaneType.Bike:
				return 2F;

			case LaneType.Parking:
				return 2F;

			case LaneType.Train:
				return 4F;

			case LaneType.Curb:
			case LaneType.Empty:
				return 1F;
		}

		return 3F;
	}

    public class SavedSettings
    {
		public int Version { get; set; }
        public float DiagonalParkingSize { get; set; }
        public float HorizontalParkingSize { get; set; }
        public List<int> LaneTypes { get; set; }
        public List<float> LaneSizes { get; set; }
    }
}
