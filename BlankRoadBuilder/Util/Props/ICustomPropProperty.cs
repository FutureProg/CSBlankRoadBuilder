using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlankRoadBuilder.Util.Props;
internal interface ICustomPropProperty
{
	object AsPrimitive();
	void FromPrimitive(object primiteValue);
}
