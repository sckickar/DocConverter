using System.Collections;

namespace DocGen.DocIO.DLS;

public class WParagraphCollection : EntitySubsetCollection, IWParagraphCollection, IEntityCollectionBase, ICollectionBase, IEnumerable
{
	public new WParagraph this[int index]
	{
		get
		{
			ClearIndexes();
			return (WParagraph)GetByIndex(index);
		}
	}

	internal ITextBody OwnerTextBody => base.Owner as ITextBody;

	public WParagraphCollection(BodyItemCollection bodyItems)
		: base(bodyItems, EntityType.Paragraph)
	{
	}

	public int Add(IWParagraph paragraph)
	{
		base.Document.EnsureParagraphStyle(paragraph);
		return InternalAdd((Entity)paragraph);
	}

	public bool Contains(IWParagraph paragraph)
	{
		return InternalContains((Entity)paragraph);
	}

	public void Insert(int index, IWParagraph paragraph)
	{
		base.Document.EnsureParagraphStyle(paragraph);
		InternalInsert(index, (Entity)paragraph);
	}

	public int IndexOf(IWParagraph paragraph)
	{
		return InternalIndexOf((Entity)paragraph);
	}

	public void Remove(IWParagraph paragraph)
	{
		InternalRemove((Entity)paragraph);
	}

	public void RemoveAt(int index)
	{
		InternalRemoveAt(index);
	}
}
