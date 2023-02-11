using UnityEngine;

namespace BlankRoadBuilder.UI;

internal interface IMarginedComponent
{
	RectOffset Margin { get; }
}