using System.Collections;
using System.Collections.Generic;

namespace DocGen.OfficeChart;

public interface IOfficeChartCategories : IParentApplication, ICollection<IOfficeChartCategory>, IEnumerable<IOfficeChartCategory>, IEnumerable
{
	new int Count { get; }

	IOfficeChartCategory this[int index] { get; }

	IOfficeChartCategory this[string name] { get; }
}
