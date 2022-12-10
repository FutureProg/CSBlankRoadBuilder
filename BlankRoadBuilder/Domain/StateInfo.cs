using Epic.OnlineServices.Presence;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlankRoadBuilder.Domain;

public struct StateInfo
{
	public string Info { get; set; }
	public bool TaskEnded { get; set; }
	public Exception? Exception { get; set; }

	public StateInfo(string info, bool ended = false)
	{
		Info = info;
		TaskEnded = ended;
		Exception = null;
	}

	public StateInfo(Exception exception)
	{
		Info = exception.Message;
		TaskEnded = true;
		Exception = exception;
	}
}
