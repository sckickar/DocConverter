using System.Collections;

namespace DocGen.Office;

public interface IVbaModules : IEnumerable
{
	int Count { get; }

	IVbaModule this[int index] { get; }

	IVbaModule this[string name] { get; }

	IVbaModule Add(string name, VbaModuleType type);

	void Remove(string name);

	void RemoveAt(int index);

	void Clear();
}
