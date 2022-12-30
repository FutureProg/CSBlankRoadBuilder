using System;
using System.ComponentModel;

namespace BlankRoadBuilder.Domain;

[Flags]
public enum MarkingsSource
{
	IMTWithANFillers = IMTMarkings | ANFillers,
	IMTWithHiddenANMarkings = IMTMarkings | HiddenANMarkings,

	IMTMarkings = 8,
	ANMarkings = 4,
	HiddenANMarkings = 2,
	ANFillers = 1,
	NoMarkings = 0
}