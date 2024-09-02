using System.Collections;

namespace DocGen.DocIO.DLS;

public interface ICollectionBase : IEnumerable
{
	int Count { get; }
}
