namespace DocGen.Office;

internal class OfficeMathMatrixColumns : CollectionImpl, IOfficeMathMatrixColumns, ICollectionBase, IOfficeMathEntity
{
	public IOfficeMathMatrixColumn this[int index] => base.InnerList[index] as OfficeMathMatrixColumn;

	public IOfficeMathMatrixColumn Add(int index)
	{
		OfficeMathMatrixColumn officeMathMatrixColumn = new OfficeMathMatrixColumn(base.OwnerMathEntity);
		m_innerList.Insert(index, officeMathMatrixColumn);
		officeMathMatrixColumn.OnColumnAdded();
		return officeMathMatrixColumn;
	}

	public IOfficeMathMatrixColumn Add()
	{
		OfficeMathMatrixColumn officeMathMatrixColumn = new OfficeMathMatrixColumn(base.OwnerMathEntity);
		m_innerList.Add(officeMathMatrixColumn);
		officeMathMatrixColumn.OnColumnAdded();
		return officeMathMatrixColumn;
	}

	internal void CloneItemsTo(OfficeMathMatrixColumns items)
	{
		for (int i = 0; i < base.Count; i++)
		{
			OfficeMathMatrixColumn officeMathMatrixColumn = (this[i] as OfficeMathMatrixColumn).Clone(items.OwnerMathEntity);
			officeMathMatrixColumn.SetOwner(items.OwnerMathEntity);
			items.Add(officeMathMatrixColumn);
		}
	}

	public new void Remove(IOfficeMathEntity item)
	{
		if (item is OfficeMathMatrixColumn)
		{
			OfficeMathMatrixColumn officeMathMatrixColumn = item as OfficeMathMatrixColumn;
			OfficeMathMatrix officeMathMatrix = base.OwnerMathEntity as OfficeMathMatrix;
			if (officeMathMatrixColumn.ColumnIndex > -1)
			{
				officeMathMatrix.RemoveMatrixItems(officeMathMatrixColumn.ColumnIndex, 0, officeMathMatrixColumn.ColumnIndex, officeMathMatrix.Rows.Count - 1);
			}
		}
		base.Remove(item);
	}

	internal OfficeMathMatrixColumns(IOfficeMathEntity owner)
		: base(owner)
	{
	}
}
