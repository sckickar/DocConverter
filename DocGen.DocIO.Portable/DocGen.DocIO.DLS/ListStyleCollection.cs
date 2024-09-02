using System;
using System.Collections;

namespace DocGen.DocIO.DLS;

public class ListStyleCollection : CollectionImpl, IEnumerable
{
	public ListStyle this[int index] => (ListStyle)base.InnerList[index];

	internal ListStyleCollection(WordDocument doc)
		: base(doc, null)
	{
	}

	public int Add(ListStyle style)
	{
		if (style == null)
		{
			throw new ArgumentNullException("style");
		}
		style.CloneRelationsTo(base.Document, null);
		return base.InnerList.Add(style);
	}

	public ListStyle FindByName(string name)
	{
		return StyleCollection.FindStyleByName(base.InnerList, name) as ListStyle;
	}

	internal bool HasEquivalentStyle(ListStyle listStyle)
	{
		for (int i = 0; i < base.Count; i++)
		{
			ListStyle listStyle2 = this[i];
			if (!base.Document.Comparison.RevisedDocListStyles.Contains(listStyle2.Name) && listStyle2.Compare(listStyle))
			{
				return true;
			}
		}
		return false;
	}

	internal ListStyle GetEquivalentStyle(ListStyle listStyle)
	{
		for (int i = 0; i < base.Count; i++)
		{
			ListStyle listStyle2 = this[i];
			if (listStyle2.Compare(listStyle))
			{
				return listStyle2;
			}
		}
		return null;
	}

	internal bool HasSameListId(ListStyle currentListStyle)
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (((ListStyle)enumerator.Current).ListID == currentListStyle.ListID)
				{
					return true;
				}
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		return false;
	}
}
