using System.Collections;
using System.Collections.Generic;

namespace DocGen.OfficeChart;

public interface IOfficeChartSeries : IParentApplication, ICollection<IOfficeChartSerie>, IEnumerable<IOfficeChartSerie>, IEnumerable
{
	new int Count { get; }

	IOfficeChartSerie this[int index] { get; }

	IOfficeChartSerie this[string name] { get; }

	IOfficeChartSerie Add();

	IOfficeChartSerie Add(OfficeChartType serieType);

	IOfficeChartSerie Add(string name);

	IOfficeChartSerie Add(string name, OfficeChartType type);

	void RemoveAt(int index);

	void Remove(string serieName);
}
