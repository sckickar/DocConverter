using System.Collections;

namespace DocGen.OfficeChart;

internal interface IRanges : IParentApplication, IRange, IEnumerable
{
	IRange this[int index] { get; }

	void Add(IRange range);

	void Remove(IRange range);
}
