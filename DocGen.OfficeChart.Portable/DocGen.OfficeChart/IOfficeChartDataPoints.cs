using System.Collections;

namespace DocGen.OfficeChart;

public interface IOfficeChartDataPoints : IParentApplication, IEnumerable
{
	IOfficeChartDataPoint DefaultDataPoint { get; }

	IOfficeChartDataPoint this[int index] { get; }
}
