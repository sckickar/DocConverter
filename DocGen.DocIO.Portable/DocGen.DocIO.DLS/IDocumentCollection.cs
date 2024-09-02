using System.Collections;

namespace DocGen.DocIO.DLS;

public interface IDocumentCollection : ICollectionBase, IEnumerable
{
	IWordDocument this[int index] { get; }
}
