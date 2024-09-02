using System.Collections;

namespace DocGen.DocIO.DLS;

public class ColumnCollection : CollectionImpl, IEnumerable
{
	public Column this[int index] => (Column)base.InnerList[index];

	internal WSection OwnerSection => base.OwnerBase as WSection;

	internal ColumnCollection(WSection section)
		: base(section.Document, section)
	{
	}

	public int Add(Column column)
	{
		return Add(column, isOpening: false);
	}

	public void Populate(int count, float spacing)
	{
		float num = OwnerSection.PageSetup.ClientWidth / (float)count;
		num -= spacing;
		base.InnerList.Clear();
		for (int i = 0; i < count; i++)
		{
			Column column = new Column(base.Document);
			column.Width = num;
			column.Space = spacing;
			Add(column, isOpening: true);
		}
	}

	internal int Add(Column column, bool isOpening)
	{
		if (!isOpening && base.OwnerBase is WSection)
		{
			(base.OwnerBase as WSection).PageSetup.EqualColumnWidth = false;
		}
		column.SetOwner(base.OwnerBase);
		return base.InnerList.Add(column);
	}

	internal void CloneTo(ColumnCollection coll)
	{
		Column column = null;
		int i = 0;
		for (int count = base.InnerList.Count; i < count; i++)
		{
			column = base.InnerList[i] as Column;
			coll.Add(column.Clone(), isOpening: true);
		}
	}
}
