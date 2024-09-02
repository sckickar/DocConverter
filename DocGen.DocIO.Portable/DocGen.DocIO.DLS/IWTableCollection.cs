using System.Collections;

namespace DocGen.DocIO.DLS;

public interface IWTableCollection : IEntityCollectionBase, ICollectionBase, IEnumerable
{
	new IWTable this[int index] { get; }

	int Add(IWTable table);

	int IndexOf(IWTable table);

	bool Contains(IWTable table);
}
