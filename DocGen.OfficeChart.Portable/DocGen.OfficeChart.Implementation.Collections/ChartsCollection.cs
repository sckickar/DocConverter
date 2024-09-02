using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class ChartsCollection : CollectionBaseEx<IOfficeChart>, ICharts, IEnumerable
{
	public const string DEF_CHART_NAME_START = "Chart";

	private Dictionary<string, IOfficeChart> m_hashNames = new Dictionary<string, IOfficeChart>(StringComparer.CurrentCultureIgnoreCase);

	private WorkbookImpl m_book;

	public IOfficeChart this[string name] => m_hashNames[name];

	public ChartsCollection(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
	}

	public new IOfficeChart Add(IOfficeChart chartToAdd)
	{
		AddInternal(chartToAdd);
		m_book.Objects.Add((ISerializableNamedObject)chartToAdd);
		return chartToAdd;
	}

	public IOfficeChart Add()
	{
		ChartImpl chartImpl = new ChartImpl();
		chartImpl.Name = CollectionBaseEx<IOfficeChart>.GenerateDefaultName(base.List, "Chart");
		return Add(chartImpl);
	}

	public IOfficeChart Add(string name)
	{
		ChartImpl chartImpl = new ChartImpl();
		chartImpl.Name = name;
		return Add(chartImpl);
	}

	public IOfficeChart Remove(string name)
	{
		if (m_hashNames.ContainsKey(name))
		{
			IOfficeChart officeChart = m_hashNames[name];
			Remove(officeChart);
			return officeChart;
		}
		return null;
	}

	private void SetParents()
	{
		object obj = FindParent(typeof(WorkbookImpl));
		if (obj == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
		m_book = (WorkbookImpl)obj;
	}

	public void Move(int iOldIndex, int iNewIndex)
	{
		if (iOldIndex != iNewIndex)
		{
			int count = base.InnerList.Count;
			if (iOldIndex < 0 || iOldIndex >= count)
			{
				throw new ArgumentOutOfRangeException("iOldIndex");
			}
			if (iNewIndex < 0 || iNewIndex >= count)
			{
				throw new ArgumentOutOfRangeException("iNewIndex");
			}
			ChartImpl item = base[iOldIndex] as ChartImpl;
			base.InnerList.RemoveAt(iOldIndex);
			base.InnerList.Insert(iNewIndex, item);
		}
	}

	protected override void OnClear()
	{
		base.OnClear();
		m_hashNames.Clear();
	}

	private void MoveInternal(int iOldSheetIndex, int iNewSheetIndex)
	{
		if (iOldSheetIndex != iNewSheetIndex)
		{
			int count = base.InnerList.Count;
			if (iOldSheetIndex < 0 || iOldSheetIndex >= count)
			{
				throw new ArgumentOutOfRangeException("iOldIndex");
			}
			if (iNewSheetIndex < 0 || iNewSheetIndex >= count)
			{
				throw new ArgumentOutOfRangeException("iNewIndex");
			}
			ChartImpl item = base[iOldSheetIndex] as ChartImpl;
			base.InnerList.RemoveAt(iOldSheetIndex);
			base.InnerList.Insert(iNewSheetIndex, item);
			int num = Math.Min(iNewSheetIndex, iOldSheetIndex);
			int num2 = Math.Max(iNewSheetIndex, iOldSheetIndex);
			for (int i = num; i <= num2; i++)
			{
				item = base[i] as ChartImpl;
				item.Index = i;
			}
		}
	}

	public void AddInternal(IOfficeChart chartToAdd)
	{
		if (chartToAdd == null)
		{
			throw new ArgumentNullException("chartToAdd");
		}
		if ((chartToAdd as ChartImpl).Name == null || (chartToAdd as ChartImpl).Name.Length == 0)
		{
			(chartToAdd as ChartImpl).Name = CollectionBaseEx<IOfficeChart>.GenerateDefaultName(base.List, "Chart");
		}
		m_hashNames.Add((chartToAdd as ChartImpl).Name, chartToAdd);
		(chartToAdd as ChartImpl).Index = base.Count;
		base.Add(chartToAdd);
	}
}
