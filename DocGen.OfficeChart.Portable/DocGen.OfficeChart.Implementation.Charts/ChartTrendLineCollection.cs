using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartTrendLineCollection : CollectionBaseEx<IOfficeChartTrendLine>, IOfficeChartTrendLines
{
	private ChartSerieImpl m_parentSerie;

	public new IOfficeChartTrendLine this[int iIndex]
	{
		get
		{
			if (iIndex >= base.List.Count || iIndex < 0)
			{
				throw new ArgumentOutOfRangeException("Index is out of bounds of collection.");
			}
			CheckSeriesType();
			IOfficeChartTrendLine officeChartTrendLine = base.List[iIndex];
			if (!IsParsed)
			{
				CheckNegativeValues(officeChartTrendLine.Type);
			}
			return officeChartTrendLine;
		}
	}

	private bool IsParsed => m_parentSerie.ParentChart.IsParsed;

	public ChartTrendLineCollection(IApplication application, object parent)
		: base(application, parent)
	{
		m_parentSerie = (ChartSerieImpl)FindParent(typeof(ChartSerieImpl));
		if (m_parentSerie == null)
		{
			throw new ApplicationException("Cannot find parent objects.");
		}
	}

	public IOfficeChartTrendLine Add()
	{
		return Add(OfficeTrendLineType.Linear);
	}

	public IOfficeChartTrendLine Add(OfficeTrendLineType type)
	{
		CheckSeriesType();
		CheckNegativeValues(type);
		ChartTrendLineImpl chartTrendLineImpl = new ChartTrendLineImpl(base.Application, this);
		chartTrendLineImpl.Type = type;
		base.Add(chartTrendLineImpl);
		return chartTrendLineImpl;
	}

	public new void RemoveAt(int index)
	{
		if (index < 0 || index >= base.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		CheckSeriesType();
		base.RemoveAt(index);
	}

	[CLSCompliant(false)]
	public void Serialize(IList<IBiffStorage> records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			((ChartTrendLineImpl)base.List[i]).Serialize(records);
		}
	}

	private void CheckNegativeValues(OfficeTrendLineType type)
	{
		if ((type != OfficeTrendLineType.Power && type != OfficeTrendLineType.Exponential) || m_parentSerie.ValuesIRange is ExternalRange)
		{
			return;
		}
		IRange[] cells = m_parentSerie.ValuesIRange.Cells;
		int i = 0;
		for (int num = cells.Length; i < num; i++)
		{
			IRange range = cells[i];
			if (range.HasNumber && range.Number <= 0.0)
			{
				throw new NotSupportedException("Cannot perform current operation becouse one ofseries values is less or equal zero.");
			}
		}
	}

	public void Add(ChartTrendLineImpl trend)
	{
		if (trend == null)
		{
			throw new ArgumentNullException("trend");
		}
		base.Add(trend);
	}

	public void CheckSeriesType()
	{
		if (!IsParsed && Array.IndexOf(ChartImpl.DEF_SUPPORT_TREND_LINES, m_parentSerie.SerieType) == -1)
		{
			throw new ArgumentNullException("Current serie type doesnot support trend lines.");
		}
	}

	public void MarkUsedReferences(bool[] usedItems)
	{
		List<IOfficeChartTrendLine> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			((ChartTrendLineImpl)innerList[i]).MarkUsedReferences(usedItems);
		}
	}

	public void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		List<IOfficeChartTrendLine> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			((ChartTrendLineImpl)innerList[i]).UpdateReferenceIndexes(arrUpdatedIndexes);
		}
	}

	public ChartTrendLineCollection Clone(object parent, Dictionary<int, int> dicFontIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ChartTrendLineCollection chartTrendLineCollection = new ChartTrendLineCollection(base.Application, parent);
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			ChartTrendLineImpl chartTrendLineImpl = (ChartTrendLineImpl)base.List[i];
			chartTrendLineCollection.Add(chartTrendLineImpl.Clone(chartTrendLineCollection, dicFontIndexes, dicNewSheetNames));
		}
		return chartTrendLineCollection;
	}
}
