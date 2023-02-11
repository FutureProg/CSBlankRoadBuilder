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
	private RoadSortMode _sortMode;

	public RoadSortMode SortMode
	{
		get => _sortMode;
		set
		{
			_sortMode = value;

			Save();
		}
	}

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

	public void Update(bool reset = false)
	{
		try
		{
			if (!reset && File.Exists(Path.Combine(BlankRoadBuilderMod.BuilderFolder, "LaneSizes.xml")))
			{
				var x = new XmlSerializer(typeof(SavedSettings));

				using var stream = File.OpenRead(Path.Combine(BlankRoadBuilderMod.BuilderFolder, "LaneSizes.xml"));
				var savedSettings = (SavedSettings)x.Deserialize(stream);

				_diagonalParkingSize = savedSettings.DiagonalParkingSize;
				_horizontalParkingSize = savedSettings.HorizontalParkingSize;
				_sortMode = savedSettings.SortMode;
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
		_diagonalParkingSize = 5F;
		_horizontalParkingSize = 6F;
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
			SortMode = _sortMode,
			LaneTypes = _sizes.Keys.Cast<int>().ToList(),
			LaneSizes = _sizes.Values.ToList(),
		});
	}

	public static float GetDefaultLaneWidth(LaneType type)
	{
		return type switch
		{
			LaneType.Filler => 3F,
			LaneType.Tram or LaneType.Car or LaneType.Bus or LaneType.Emergency => 3F,
			LaneType.Pedestrian => 2F,
			LaneType.Bike => 2F,
			LaneType.Parking => 2F,
			LaneType.Train => 4F,
			LaneType.Curb or LaneType.Empty => 1F,
			_ => 3F,
		};
	}

	public class SavedSettings
	{
		public int Version { get; set; }
		public float DiagonalParkingSize { get; set; }
		public float HorizontalParkingSize { get; set; }
		public List<int> LaneTypes { get; set; }
		public List<float> LaneSizes { get; set; }
		public RoadSortMode SortMode { get; set; }
	}
}
