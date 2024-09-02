using System;
using System.Collections.Generic;
using DocGen.DocIO.DLS;

namespace DocGen.Layouting;

internal class TabsLayoutInfo : LayoutInfo
{
	internal class LayoutTab
	{
		private TabJustification m_jc;

		private TabLeader m_tlc;

		private float m_tabPosition;

		public TabJustification Justification
		{
			get
			{
				return m_jc;
			}
			set
			{
				if (value != m_jc)
				{
					m_jc = value;
				}
			}
		}

		public TabLeader TabLeader
		{
			get
			{
				return m_tlc;
			}
			set
			{
				if (value != m_tlc)
				{
					m_tlc = value;
				}
			}
		}

		public float Position
		{
			get
			{
				return m_tabPosition;
			}
			set
			{
				if (value != m_tabPosition)
				{
					m_tabPosition = value;
				}
			}
		}

		internal LayoutTab()
			: this(0f, TabJustification.Left, TabLeader.NoLeader)
		{
		}

		internal LayoutTab(float position, TabJustification justification, TabLeader leader)
		{
			m_tabPosition = position;
			m_jc = justification;
			m_tlc = leader;
		}
	}

	protected double m_defaultTabWidth;

	protected double m_pageMarginLeft;

	protected double m_pageMarginRight;

	internal List<LayoutTab> m_list = new List<LayoutTab>();

	internal LayoutTab m_currTab = new LayoutTab();

	private float m_tabWidth;

	private bool m_isTabWidthUpdatedBasedOnIndent;

	public double DefaultTabWidth => m_defaultTabWidth;

	internal double PageMarginLeft
	{
		get
		{
			return m_pageMarginLeft;
		}
		set
		{
			m_pageMarginLeft = value;
		}
	}

	internal double PageMarginRight
	{
		get
		{
			return m_pageMarginRight;
		}
		set
		{
			m_pageMarginRight = value;
		}
	}

	internal bool IsTabWidthUpdatedBasedOnIndent
	{
		get
		{
			return m_isTabWidthUpdatedBasedOnIndent;
		}
		set
		{
			m_isTabWidthUpdatedBasedOnIndent = value;
		}
	}

	internal float TabWidth
	{
		get
		{
			return m_tabWidth;
		}
		set
		{
			m_tabWidth = value;
		}
	}

	public TabLeader CurrTabLeader => m_currTab.TabLeader;

	public TabJustification CurrTabJustification => m_currTab.Justification;

	internal List<LayoutTab> LayoutTabList => m_list;

	public TabsLayoutInfo(ChildrenLayoutDirection childLayoutDirection)
		: base(childLayoutDirection)
	{
	}

	public double GetNextTabPosition(double position)
	{
		double num = position;
		bool flag = false;
		if (m_list.Count > 0)
		{
			for (int num2 = m_list.Count - 1; num2 > -1; num2--)
			{
				if (!(Math.Round(m_list[num2].Position, 2) > Math.Round(position, 2)) && (num2 <= 0 || !((double)m_list[num2 - 1].Position > Math.Round(position, 2))))
				{
					if (num2 != m_list.Count - 1)
					{
						if (m_list[num2 + 1].Justification != TabJustification.Bar)
						{
							num = m_list[num2 + 1].Position;
							m_currTab = m_list[num2 + 1];
						}
						else
						{
							num = position;
						}
					}
					flag = true;
					break;
				}
			}
			if (!flag && m_list[0].Justification != TabJustification.Bar)
			{
				num = m_list[0].Position;
				m_currTab = m_list[0];
			}
		}
		bool flag2 = false;
		if (num == position)
		{
			m_currTab = new LayoutTab();
			flag2 = true;
			if (DefaultTabWidth > 0.0)
			{
				float num3 = (float)Math.Round(position * 100.0 % (DefaultTabWidth * 100.0) / 100.0, 2);
				num = ((position - (double)num3) / DefaultTabWidth + 1.0) * DefaultTabWidth;
			}
		}
		if (Math.Round(num, 1) == Math.Round(position, 1) && Math.Round(num - position, 2) <= 0.01)
		{
			return DefaultTabWidth;
		}
		if (Math.Round(num - position, 1) > Math.Round(DefaultTabWidth, 1) && flag2)
		{
			return (num - position) % DefaultTabWidth;
		}
		return num - position;
	}

	public void AddTab(float position, TabJustification justification, TabLeader leader)
	{
		m_list.Add(new LayoutTab(position, justification, leader));
	}

	internal void SortParagraphTabsCollection(WParagraphFormat paragraphFormat, TabCollection listTabCollection, int tabLevelIndex)
	{
		Dictionary<int, TabCollection> dictionary = new Dictionary<int, TabCollection>();
		int num = 0;
		int num2 = 0;
		bool flag = true;
		for (WParagraphFormat wParagraphFormat = paragraphFormat; wParagraphFormat != null; wParagraphFormat = wParagraphFormat.BaseFormat as WParagraphFormat)
		{
			if (wParagraphFormat.Tabs.Count > 0)
			{
				if (num2 < wParagraphFormat.Tabs.Count)
				{
					num2 = wParagraphFormat.Tabs.Count;
				}
				dictionary.Add(num, wParagraphFormat.Tabs);
				num++;
			}
			if (listTabCollection != null && flag && num == tabLevelIndex && listTabCollection.Count > 0)
			{
				if (num2 < listTabCollection.Count)
				{
					num2 = listTabCollection.Count;
				}
				dictionary.Add(num, listTabCollection);
				flag = false;
				num++;
			}
		}
		UpdateTabs(dictionary, num2);
		dictionary.Clear();
	}

	private void UpdateTabs(Dictionary<int, TabCollection> tabCollection, int count)
	{
		int[] array = new int[tabCollection.Count];
		int currLevelIndex = 0;
		Dictionary<float, int> dictionary = new Dictionary<float, int>();
		List<int> list = new List<int>();
		for (int i = 0; i < count; i++)
		{
			Tab tab = null;
			int num = 0;
			for (int j = 0; j < tabCollection.Count; j++)
			{
				if (i < tabCollection[j].Count && array[j] <= i && (tab == null || tab.Position < tabCollection[j][i].Position))
				{
					tab = tabCollection[j][i];
					num = j;
				}
			}
			bool flag = false;
			while (!flag && tab != null && array[num] <= i)
			{
				bool flag2 = false;
				Tab tab2 = null;
				List<Tab> list2 = new List<Tab>();
				List<int> list3 = new List<int>();
				for (int k = 0; k < tabCollection.Count; k++)
				{
					TabCollection tabCollection2 = tabCollection[k];
					if (k != num && array[k] < tabCollection2.Count && tabCollection[num][i].Position > tabCollection2[array[k]].Position)
					{
						list2.Add(tabCollection2[array[k]]);
						list3.Add(k);
						flag2 = true;
					}
				}
				if (list2.Count > 0)
				{
					for (int l = 1; l < list2.Count; l++)
					{
						if (list2[0].Position > list2[l].Position)
						{
							list2[0] = list2[l];
							list3[0] = list3[l];
						}
					}
					tab2 = list2[0];
					array[list3[0]]++;
					currLevelIndex = list3[0];
				}
				if (!flag2)
				{
					tab2 = tabCollection[num][i];
					while (array[num] < i + 1)
					{
						array[num]++;
					}
					flag = true;
					currLevelIndex = num;
				}
				if (tab2 != null)
				{
					UpdateTabsCollection(tab2, currLevelIndex, array, dictionary, list, tabCollection);
				}
			}
		}
		ClearDeleteTabPositions(dictionary, list);
		dictionary.Clear();
		list.Clear();
	}

	private void ClearDeleteTabPositions(Dictionary<float, int> delPosition, List<int> tabLevels)
	{
		if (m_list.Count <= 0 || delPosition.Count <= 0)
		{
			return;
		}
		List<float> list = new List<float>(delPosition.Keys);
		for (int i = 0; i < m_list.Count; i++)
		{
			if (list.Contains((float)Math.Truncate(m_list[i].Position)) && delPosition[(float)Math.Truncate(m_list[i].Position)] < tabLevels[i])
			{
				list.Remove((float)Math.Truncate(m_list[i].Position));
				m_list.RemoveAt(i);
				tabLevels.RemoveAt(i);
				i--;
				if (list.Count == 0)
				{
					break;
				}
			}
		}
	}

	private void UpdateTabsCollection(Tab tab, int currLevelIndex, int[] levelIndexes, Dictionary<float, int> delPosition, List<int> tabLevels, Dictionary<int, TabCollection> tabCollection)
	{
		bool flag = false;
		int index = 0;
		if (m_list.Count != 0)
		{
			for (int i = 0; i < m_list.Count; i++)
			{
				int num = levelIndexes[currLevelIndex] - 1;
				if (num < tabCollection[tabLevels[i]].Count && currLevelIndex != tabLevels[i] && Math.Truncate(tabCollection[tabLevels[i]][num].Position) == Math.Truncate(tabCollection[currLevelIndex][num].Position))
				{
					flag = true;
					index = i;
					break;
				}
			}
		}
		if ((tab.Position != 0f || tab.DeletePosition == 0f) && !flag)
		{
			AddTab((tab.Position != 0f) ? tab.Position : (tab.DeletePosition / 20f), (TabJustification)tab.Justification, (TabLeader)tab.TabLeader);
			tabLevels.Add(currLevelIndex);
		}
		else if (tab.DeletePosition != 0f && !delPosition.ContainsKey((float)Math.Truncate(tab.DeletePosition / 20f)))
		{
			delPosition.Add((float)Math.Truncate(tab.DeletePosition / 20f), currLevelIndex);
		}
		if (flag && tabLevels[index] > currLevelIndex)
		{
			m_list[index].Justification = (TabJustification)tab.Justification;
			m_list[index].Position = ((tab.Position != 0f) ? tab.Position : (tab.DeletePosition / 20f));
			m_list[index].TabLeader = (TabLeader)tab.TabLeader;
		}
	}
}
