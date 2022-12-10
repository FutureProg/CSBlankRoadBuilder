using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlankRoadBuilder.Domain;

// Warning: Modifying this class may break the xml deserialization for SavedAssets.xml 
public class RoadOptions
{
	public static RoadOptions LastSelected { get; set; } = new RoadOptions();

	public bool DisableLeftSidewalkStop { get; set; }
	public bool DisableRightSidewalkStop { get; set; }
	public bool RemovePedestrianLaneOnTheLeftSidewalk { get; set; }
	public bool RemovePedestrianLaneOnTheRightSidewalk { get; set; }
	public bool DoNotChangeTheRoadMeshes { get; set; }
}
