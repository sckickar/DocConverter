using System;
using System.Collections;

namespace DocGen.DocIO.DLS;

public class TabCollection : CollectionImpl, IEnumerable
{
	private byte m_bFlags;

	public Tab this[int index] => (Tab)base.InnerList[index];

	internal bool CancelOnChangeEvent
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal TabCollection(WordDocument document)
		: base(document, null)
	{
	}

	internal TabCollection(WordDocument document, FormatBase owner)
		: this(document)
	{
		SetOwner(owner);
	}

	public Tab AddTab()
	{
		return AddTab(0f, TabJustification.Left, TabLeader.NoLeader);
	}

	public Tab AddTab(float position, TabJustification justification, TabLeader leader)
	{
		Tab tab = new Tab(base.Document, position, justification, leader);
		base.InnerList.Add(tab);
		tab.SetOwner(this);
		OnChange();
		return tab;
	}

	public Tab AddTab(float position)
	{
		return AddTab(position, TabJustification.Left, TabLeader.NoLeader);
	}

	public void Clear()
	{
		base.InnerList.Clear();
		OnChange();
	}

	public void RemoveAt(int index)
	{
		base.InnerList.RemoveAt(index);
		OnChange();
	}

	public void RemoveByTabPosition(double position)
	{
		int num = 0;
		while (num < base.Count)
		{
			if ((double)this[num].Position == position)
			{
				base.InnerList.Remove(this[num]);
			}
			else
			{
				num++;
			}
		}
		OnChange();
	}

	internal void AddTab(Tab tab)
	{
		base.InnerList.Add(tab);
		tab.SetOwner(this);
		OnChange();
	}

	internal void OnChange()
	{
		if (!CancelOnChangeEvent && base.OwnerBase != null && base.OwnerBase is WParagraphFormat)
		{
			(base.OwnerBase as WParagraphFormat).ChangeTabs(this);
		}
	}

	internal bool Compare(TabCollection tabs)
	{
		if (base.Count != tabs.Count)
		{
			return false;
		}
		int num = 0;
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Tab tab = (Tab)enumerator.Current;
				Tab tab2 = tabs[num];
				if (tab != null && tab2 != null)
				{
					if (!tab.Compare(tab2))
					{
						return false;
					}
					num++;
				}
				else if ((tab != null && tab2 == null) || (tab == null && tab2 != null))
				{
					return false;
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
		return true;
	}

	internal void UpdateSourceFormatting(TabCollection tabs)
	{
		for (int i = 0; i < base.Count; i++)
		{
			Tab tab = this[i];
			Tab tab2 = new Tab(tabs.Document);
			tab2.Position = 0f;
			tab2.DeletePosition = tab.Position * 20f;
			tabs.InnerList.Insert(i, tab2);
		}
	}

	internal void UpdateTabs(TabCollection tabs)
	{
		if (tabs == null)
		{
			tabs = new TabCollection(base.Document);
		}
		for (int i = 0; i < base.Count; i++)
		{
			tabs.AddTab(this[i].Clone());
		}
	}

	internal bool HasTabPosition(float tabPosition)
	{
		if (base.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < base.Count; i++)
		{
			if (this[i].Position == tabPosition)
			{
				return true;
			}
		}
		return false;
	}

	internal void SortTabs()
	{
		for (int i = 1; i < base.Count; i++)
		{
			for (int j = 0; j < base.Count - 1; j++)
			{
				Tab tab = this[j];
				Tab tab2 = this[j + 1];
				float num = ((tab.Position != 0f) ? tab.Position : ((tab.DeletePosition != 0f) ? tab.DeletePosition : 0f));
				float num2 = ((tab2.Position != 0f) ? tab2.Position : ((tab2.DeletePosition != 0f) ? tab2.DeletePosition : 0f));
				if (num > num2)
				{
					Tab value = this[j];
					base.InnerList[j] = this[j + 1];
					base.InnerList[j + 1] = value;
				}
			}
		}
		OnChange();
	}
}
