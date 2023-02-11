namespace BlankRoadBuilder.ThumbnailMaker;

internal enum RoadStatusFilter
{
	AnyStatus,
	UpToDate,
	NeedsUpdating,
	Missing,
	NeverBuilt,
	BuiltBeforeLastMajorUpdate,
}