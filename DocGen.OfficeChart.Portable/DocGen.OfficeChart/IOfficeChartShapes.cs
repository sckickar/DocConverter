using System.Collections;

namespace DocGen.OfficeChart;

internal interface IOfficeChartShapes : IEnumerable, IParentApplication
{
	int Count { get; }

	IOfficeChartShape this[int index] { get; }

	IOfficeChartShape Add();

	void RemoveAt(int index);
}
