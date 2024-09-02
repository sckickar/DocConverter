using System;
using System.Collections;

namespace DocGen.DocIO.DLS;

public class WDropDownCollection : CollectionImpl, IEnumerable
{
	public WDropDownItem this[int index] => (WDropDownItem)base.InnerList[index];

	public WDropDownCollection(WordDocument doc)
		: base(doc, null)
	{
	}

	public WDropDownItem Add(string text)
	{
		if (base.InnerList.Count > 24)
		{
			throw new ArgumentOutOfRangeException("InnerList", "You can have no more than 25 items in your drop-down list box");
		}
		WDropDownItem wDropDownItem = new WDropDownItem(base.Document);
		wDropDownItem.Text = text;
		base.InnerList.Add(wDropDownItem);
		return wDropDownItem;
	}

	public void Remove(int index)
	{
		if (index >= base.InnerList.Count)
		{
			throw new ArgumentException("DropDownItem with such index doesn't exist.");
		}
		WDropDownItem value = (WDropDownItem)base.InnerList[index];
		base.InnerList.Remove(value);
	}

	public void Clear()
	{
		base.InnerList.Clear();
	}

	internal int Add(WDropDownItem item)
	{
		return base.InnerList.Add(item);
	}

	internal void CloneTo(WDropDownCollection destColl)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			destColl.Add(this[i].Clone());
		}
	}

	internal override void Close()
	{
		if (base.InnerList != null && base.InnerList.Count > 0)
		{
			while (base.InnerList.Count > 0)
			{
				if (base.InnerList[0] is WDropDownItem wDropDownItem)
				{
					if (wDropDownItem.OwnerBase == null)
					{
						base.InnerList.Remove(wDropDownItem);
					}
					else
					{
						Remove(0);
					}
					wDropDownItem.Close();
				}
			}
		}
		base.Close();
	}
}
