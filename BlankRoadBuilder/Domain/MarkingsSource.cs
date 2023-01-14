using System;
using System.ComponentModel;

namespace BlankRoadBuilder.Domain;

[Flags]
public enum MarkingsSource
{
	IMTWithMeshFillers = IMTMarkings | MeshFillers,
	IMTWithHiddenVanillaMarkings = IMTMarkings | HiddenVanillaMarkings,
	MeshFillersAndHiddenVanillaMarkings = HiddenVanillaMarkings | MeshFillers,

	IMTMarkings = 8,
	VanillaMarkings = 4,
	HiddenVanillaMarkings = 2,
	MeshFillers = 1,
	NoMarkings = 0
}