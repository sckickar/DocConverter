using System.ComponentModel;

namespace DocGen.Chart;

internal enum ChartSeriesCollectionChangeType
{
	Added = 0,
	[EditorBrowsable(EditorBrowsableState.Never)]
	Inserted = 0,
	Removed = 1,
	Changed = 2,
	Reset = 3
}
