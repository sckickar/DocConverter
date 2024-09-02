namespace DocGen.Office;

internal class OfficeMathMatrixRows : CollectionImpl, IOfficeMathMatrixRows, ICollectionBase, IOfficeMathEntity
{
	public IOfficeMathMatrixRow this[int index] => base.InnerList[index] as OfficeMathMatrixRow;

	public new void Remove(IOfficeMathEntity item)
	{
		if (item is OfficeMathMatrixRow)
		{
			OfficeMathMatrixRow officeMathMatrixRow = item as OfficeMathMatrixRow;
			OfficeMathMatrix officeMathMatrix = base.OwnerMathEntity as OfficeMathMatrix;
			if (officeMathMatrixRow.RowIndex > -1)
			{
				officeMathMatrix.RemoveMatrixItems(0, officeMathMatrixRow.RowIndex, officeMathMatrix.Columns.Count - 1, officeMathMatrixRow.RowIndex);
			}
		}
		base.Remove(item);
	}

	public IOfficeMathMatrixRow Add(int index)
	{
		OfficeMathMatrixRow officeMathMatrixRow = new OfficeMathMatrixRow(base.OwnerMathEntity);
		m_innerList.Insert(index, officeMathMatrixRow);
		officeMathMatrixRow.OnRowAdded();
		return officeMathMatrixRow;
	}

	public IOfficeMathMatrixRow Add()
	{
		OfficeMathMatrixRow officeMathMatrixRow = new OfficeMathMatrixRow(base.OwnerMathEntity);
		m_innerList.Add(officeMathMatrixRow);
		officeMathMatrixRow.OnRowAdded();
		return officeMathMatrixRow;
	}

	internal void CloneItemsTo(OfficeMathMatrixRows items)
	{
		for (int i = 0; i < base.Count; i++)
		{
			OfficeMathMatrixRow item = (this[i] as OfficeMathMatrixRow).Clone(items.OwnerMathEntity);
			items.Add(item);
		}
	}

	internal OfficeMathMatrixRows(IOfficeMathEntity owner)
		: base(owner)
	{
	}
}
