using System.Collections;

namespace DocGen.DocIO.DLS;

public class WTableCollection : EntitySubsetCollection, IWTableCollection, IEntityCollectionBase, ICollectionBase, IEnumerable
{
	public new IWTable this[int index]
	{
		get
		{
			ClearIndexes();
			return GetByIndex(index) as IWTable;
		}
	}

	internal ITextBody OwnerTextBody => base.Owner as ITextBody;

	public WTableCollection(BodyItemCollection bodyItems)
		: base(bodyItems, EntityType.Table)
	{
	}

	public int Add(IWTable table)
	{
		return InternalAdd((Entity)table);
	}

	public bool Contains(IWTable table)
	{
		return InternalContains((Entity)table);
	}

	public int IndexOf(IWTable table)
	{
		return InternalIndexOf((Entity)table);
	}

	public int Insert(int index, IWTable table)
	{
		return InternalInsert(index, (Entity)table);
	}

	public void Remove(IWTable table)
	{
		InternalRemove((Entity)table);
	}

	public void RemoveAt(int index)
	{
		InternalRemoveAt(index);
	}
}
