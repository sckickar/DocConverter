using System.Collections;

namespace DocGen.DocIO.DLS;

public interface IWSectionCollection : IEntityCollectionBase, ICollectionBase, IEnumerable
{
	new WSection this[int index] { get; }

	int Add(IWSection section);

	int IndexOf(IWSection section);
}
