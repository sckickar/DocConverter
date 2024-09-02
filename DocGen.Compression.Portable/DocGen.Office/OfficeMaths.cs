using System;

namespace DocGen.Office;

internal class OfficeMaths : CollectionImpl, IOfficeMaths, ICollectionBase, IOfficeMathEntity
{
	public IOfficeMath this[int index] => (OfficeMath)base.InnerList[index];

	internal OfficeMaths(IOfficeMathEntity owner)
		: base(owner)
	{
	}

	public IOfficeMath Add(int index)
	{
		OnBeforeInsert();
		OfficeMath officeMath = new OfficeMath(base.OwnerMathEntity);
		m_innerList.Insert(index, officeMath);
		return officeMath;
	}

	public IOfficeMath Add()
	{
		OnBeforeInsert();
		OfficeMath officeMath = new OfficeMath(base.OwnerMathEntity);
		m_innerList.Add(officeMath);
		return officeMath;
	}

	internal void Add(OfficeMath item)
	{
		OnBeforeInsert();
		Add((object)item);
	}

	private void OnBeforeInsert()
	{
		if (base.OwnerMathEntity is IOfficeMathMatrixRow || base.OwnerMathEntity is IOfficeMathMatrixColumn)
		{
			throw new NotSupportedException("New arguments cannot be added directly into matrix rows or columns.");
		}
	}

	internal void CloneItemsTo(OfficeMaths items)
	{
		for (int i = 0; i < base.Count; i++)
		{
			OfficeMath value = (this[i] as OfficeMath).CloneImpl(items.OwnerMathEntity);
			items.InnerList.Add(value);
		}
	}

	internal override void Close()
	{
		if (m_innerList != null)
		{
			for (int i = 0; i < m_innerList.Count; i++)
			{
				(m_innerList[i] as OfficeMath).Close();
			}
		}
		base.Close();
	}
}
