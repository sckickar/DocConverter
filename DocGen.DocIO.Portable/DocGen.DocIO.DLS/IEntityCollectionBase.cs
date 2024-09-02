using System.Collections;

namespace DocGen.DocIO.DLS;

public interface IEntityCollectionBase : ICollectionBase, IEnumerable
{
	Entity this[int index] { get; }
}
