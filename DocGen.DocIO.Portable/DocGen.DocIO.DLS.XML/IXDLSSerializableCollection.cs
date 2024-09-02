using System.Collections;

namespace DocGen.DocIO.DLS.XML;

public interface IXDLSSerializableCollection : IEnumerable
{
	string TagItemName { get; }

	int Count { get; }

	IXDLSSerializable AddNewItem(IXDLSContentReader reader);
}
