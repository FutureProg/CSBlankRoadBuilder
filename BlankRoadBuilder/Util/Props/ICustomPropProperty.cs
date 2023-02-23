namespace BlankRoadBuilder.Util.Props;
internal interface ICustomPropProperty
{
	object AsPrimitive();
	void FromPrimitive(object primiteValue);
}
