using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlankRoadBuilder.Util.Markings;
public class MarkingsInfo
{
	public List<FillerMarking> Fillers { get; } = new List<FillerMarking>();
	public Dictionary<MarkingPoint, LineMarking> Lines { get; } = new Dictionary<MarkingPoint, LineMarking>();
}
