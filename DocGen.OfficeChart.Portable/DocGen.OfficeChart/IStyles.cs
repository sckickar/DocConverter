using System.Collections;

namespace DocGen.OfficeChart;

internal interface IStyles : IEnumerable
{
	IApplication Application { get; }

	int Count { get; }

	IStyle this[int Index] { get; }

	IStyle this[string name] { get; }

	object Parent { get; }

	IStyle Add(string Name, object BasedOn);

	IStyle Add(string Name);

	IStyles Merge(object Workbook, bool overwrite);

	IStyles Merge(object Workbook);

	bool Contains(string name);

	void Remove(string styleName);
}
