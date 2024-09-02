using System.Collections;

namespace DocGen.DocIO.DLS;

public interface IWParagraphCollection : IEntityCollectionBase, ICollectionBase, IEnumerable
{
	new WParagraph this[int index] { get; }

	int Add(IWParagraph paragraph);

	void Insert(int index, IWParagraph paragraph);

	int IndexOf(IWParagraph paragraph);

	void RemoveAt(int index);
}
