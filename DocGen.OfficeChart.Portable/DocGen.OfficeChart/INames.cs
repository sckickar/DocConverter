using System.Collections;

namespace DocGen.OfficeChart;

internal interface INames : IEnumerable
{
	IApplication Application { get; }

	int Count { get; }

	object Parent { get; }

	IName this[int index] { get; }

	IName this[string name] { get; }

	IWorksheet ParentWorksheet { get; }

	IName Add(string name);

	IName Add(string name, IRange namedObject);

	IName Add(IName name);

	void Remove(string name);

	void RemoveAt(int index);

	bool Contains(string name);
}
