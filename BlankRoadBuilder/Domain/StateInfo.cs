using System;

namespace BlankRoadBuilder.Domain;

public struct StateInfo
{
	public string Info { get; set; }
	public ElevationType ElevationStep { get; }
	public Exception? Exception { get; set; }

	public StateInfo(string info, ElevationType elevationStep = (ElevationType)(-1))
	{
		Info = info;
		ElevationStep = elevationStep;
		Exception = null;
	}

	public StateInfo(Exception exception) : this(exception.Message)
	{
		Exception = exception;
	}
}
