using System.Collections;

namespace DocGen.OfficeChart;

internal interface ICharts : IEnumerable
{
	int Count { get; }

	IOfficeChart this[int index] { get; }

	IOfficeChart this[string name] { get; }

	IOfficeChart Add();

	IOfficeChart Add(string name);

	IOfficeChart Remove(string name);
}
