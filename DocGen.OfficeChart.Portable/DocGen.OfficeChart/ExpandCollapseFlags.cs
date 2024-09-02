using System;

namespace DocGen.OfficeChart;

[Flags]
internal enum ExpandCollapseFlags
{
	Default = 0,
	IncludeSubgroups = 1,
	ExpandParent = 2
}
