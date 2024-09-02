namespace DocGen.Office;

internal class OfficeMathBreaks : CollectionImpl, IOfficeMathBreaks, ICollectionBase, IOfficeMathEntity
{
	public IOfficeMathBreak this[int index] => base.InnerList[index] as OfficeMathBreak;

	internal OfficeMathBreaks(IOfficeMathEntity owner)
		: base(owner)
	{
	}

	public IOfficeMathBreak Add(int index)
	{
		OfficeMathBreak officeMathBreak = new OfficeMathBreak(base.OwnerMathEntity);
		m_innerList.Insert(index, officeMathBreak);
		return officeMathBreak;
	}

	public IOfficeMathBreak Add()
	{
		OfficeMathBreak officeMathBreak = new OfficeMathBreak(base.OwnerMathEntity);
		m_innerList.Add(officeMathBreak);
		return officeMathBreak;
	}

	internal void CloneItemsTo(OfficeMathBreaks items)
	{
		for (int i = 0; i < base.Count; i++)
		{
			OfficeMathBreak officeMathBreak = (this[i] as OfficeMathBreak).Clone();
			officeMathBreak.SetOwner(items.OwnerMathEntity);
			items.Add(officeMathBreak);
		}
	}

	internal override void Close()
	{
		if (m_innerList != null)
		{
			for (int i = 0; i < m_innerList.Count; i++)
			{
				(m_innerList[i] as OfficeMathBreak).Close();
			}
		}
		base.Close();
	}
}
