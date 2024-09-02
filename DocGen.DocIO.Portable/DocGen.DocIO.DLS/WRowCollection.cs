using System;

namespace DocGen.DocIO.DLS;

public class WRowCollection : EntityCollection
{
	private static readonly Type[] DEF_ELEMENT_TYPES = new Type[1] { typeof(WTableRow) };

	public new WTableRow this[int index] => base.InnerList[index] as WTableRow;

	protected override Type[] TypesOfElement => DEF_ELEMENT_TYPES;

	public WRowCollection(WTable owner)
		: base(owner.Document, owner)
	{
	}

	public int Add(WTableRow row)
	{
		return Add((IEntity)row);
	}

	public void Insert(int index, WTableRow row)
	{
		Insert(index, (IEntity)row);
	}

	public int IndexOf(WTableRow row)
	{
		return IndexOf((IEntity)row);
	}

	public void Remove(WTableRow row)
	{
		Remove((IEntity)row);
	}
}
