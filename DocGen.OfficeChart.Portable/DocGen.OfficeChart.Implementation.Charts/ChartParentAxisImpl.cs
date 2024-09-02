using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartParentAxisImpl : CommonObject
{
	private ChartAxisParentRecord m_parentAxis;

	private ChartPosRecord m_position;

	private ChartCategoryAxisImpl m_categoryAxis;

	private ChartValueAxisImpl m_valueAxis;

	private ChartSeriesAxisImpl m_seriesAxis;

	internal ChartImpl m_parentChart;

	private ChartGlobalFormatsCollection m_globalFormats;

	internal ChartAxisParentRecord ParentAxisRecord
	{
		get
		{
			if (m_parentAxis == null)
			{
				m_parentAxis = (ChartAxisParentRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAxisParent);
			}
			return m_parentAxis;
		}
	}

	public ChartFormatCollection ChartFormats
	{
		get
		{
			if (IsPrimary)
			{
				return m_globalFormats.PrimaryFormats;
			}
			return m_globalFormats.SecondaryFormats;
		}
	}

	public bool IsPrimary => m_parentAxis.AxesIndex == 0;

	public ChartCategoryAxisImpl CategoryAxis
	{
		get
		{
			return m_categoryAxis;
		}
		set
		{
			m_categoryAxis = value;
		}
	}

	public ChartValueAxisImpl ValueAxis
	{
		get
		{
			return m_valueAxis;
		}
		set
		{
			m_valueAxis = value;
		}
	}

	public ChartSeriesAxisImpl SeriesAxis
	{
		get
		{
			return m_seriesAxis;
		}
		set
		{
			m_seriesAxis = value;
		}
	}

	public ChartImpl ParentChart => (FindParent(typeof(ChartImpl)) ?? throw new ArgumentException("cannot find parent object.")) as ChartImpl;

	public ChartGlobalFormatsCollection Formats => m_globalFormats;

	public ChartParentAxisImpl(IApplication application, object parent)
		: this(application, parent, isPrimary: true)
	{
	}

	public ChartParentAxisImpl(IApplication application, object parent, bool isPrimary)
		: base(application, parent)
	{
		m_parentAxis = (ChartAxisParentRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAxisParent);
		if (isPrimary)
		{
			m_categoryAxis = new ChartCategoryAxisImpl(application, this, OfficeAxisType.Category, IsPrimary);
			m_valueAxis = new ChartValueAxisImpl(application, this, OfficeAxisType.Value, IsPrimary);
		}
		if (!IsPrimary)
		{
			m_parentAxis.AxesIndex = 1;
		}
		SetParents();
	}

	private void SetParents()
	{
		m_parentChart = (ChartImpl)FindParent(typeof(ChartImpl));
		if (m_parentChart == null)
		{
			throw new ArgumentException("Can't find parent objects.");
		}
	}

	[CLSCompliant(false)]
	public void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		m_parentAxis = (ChartAxisParentRecord)data[iPos];
		iPos++;
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.Begin);
		iPos++;
		int num = 1;
		while (num != 0)
		{
			biffRecordRaw = data[iPos];
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.Begin:
				iPos = BiffRecordRaw.SkipBeginEndBlock(data, iPos) - 1;
				break;
			case TBIFFRecord.End:
				num--;
				break;
			case TBIFFRecord.ChartPos:
				m_position = (ChartPosRecord)biffRecordRaw;
				break;
			case TBIFFRecord.ChartAxis:
				ParseAxes(data, ref iPos);
				break;
			case TBIFFRecord.ChartChartFormat:
				ParseChartFormat(data, ref iPos);
				break;
			case TBIFFRecord.ChartPlotArea:
				m_parentChart.PlotArea = new ChartPlotAreaImpl(base.Application, m_parentChart, data, ref iPos);
				break;
			case TBIFFRecord.ChartText:
				ParseChartText(data, ref iPos);
				break;
			}
			iPos++;
		}
	}

	private void ParseAxes(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		data[iPos].CheckTypeCode(TBIFFRecord.ChartAxis);
		switch (((ChartAxisRecord)data[iPos]).AxisType)
		{
		case ChartAxisRecord.ChartAxisType.CategoryAxis:
			m_categoryAxis = new ChartCategoryAxisImpl(base.Application, this, data, ref iPos, m_parentAxis.AxesIndex == 0);
			break;
		case ChartAxisRecord.ChartAxisType.ValueAxis:
			m_valueAxis = new ChartValueAxisImpl(base.Application, this, data, ref iPos, m_parentAxis.AxesIndex == 0);
			break;
		case ChartAxisRecord.ChartAxisType.SeriesAxis:
			m_seriesAxis = new ChartSeriesAxisImpl(base.Application, this, data, ref iPos, m_parentAxis.AxesIndex == 0);
			break;
		default:
			throw new ArgumentOutOfRangeException("Unknown chart axis type");
		}
		iPos--;
	}

	private void ParseChartText(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		ChartTextAreaImpl chartTextAreaImpl = new ChartTextAreaImpl(base.Application, this);
		iPos = chartTextAreaImpl.Parse(data, iPos) - 1;
		switch (chartTextAreaImpl.ObjectLink.LinkObject)
		{
		case ExcelObjectTextLink.XAxis:
			if (m_categoryAxis != null)
			{
				m_categoryAxis.SetTitle(chartTextAreaImpl);
			}
			break;
		case ExcelObjectTextLink.YAxis:
			if (m_valueAxis != null)
			{
				m_valueAxis.SetTitle(chartTextAreaImpl);
			}
			break;
		case ExcelObjectTextLink.ZAxis:
			if (m_seriesAxis != null)
			{
				m_seriesAxis.SetTitle(chartTextAreaImpl);
			}
			break;
		}
	}

	private void ParseChartFormat(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		ChartFormatCollection chartFormatCollection = (IsPrimary ? m_globalFormats.PrimaryFormats : m_globalFormats.SecondaryFormats);
		ChartFormatImpl chartFormatImpl = new ChartFormatImpl(base.Application, chartFormatCollection);
		chartFormatImpl.Parse(data, ref iPos);
		chartFormatCollection.Add(chartFormatImpl, bCanReplace: false);
		iPos--;
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_parentAxis != null)
		{
			bool flag = Array.IndexOf(ChartImpl.DEF_SUPPORT_SERIES_AXIS, m_parentChart.ChartType) != -1;
			records.Add((BiffRecordRaw)m_parentAxis.Clone());
			records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
			if (m_position != null && IsPrimary)
			{
				records.Add((BiffRecordRaw)m_position.Clone());
			}
			if (m_categoryAxis != null)
			{
				m_categoryAxis.Serialize(records);
			}
			if (m_valueAxis != null)
			{
				m_valueAxis.Serialize(records);
			}
			if (m_seriesAxis != null && m_parentAxis.AxesIndex == 0 && flag)
			{
				m_seriesAxis.Serialize(records);
			}
			if (m_categoryAxis != null)
			{
				m_categoryAxis.SerializeAxisTitle(records);
			}
			if (m_valueAxis != null)
			{
				m_valueAxis.SerializeAxisTitle(records);
			}
			if (m_seriesAxis != null && m_parentAxis.AxesIndex == 0 && flag)
			{
				m_seriesAxis.SerializeAxisTitle(records);
			}
			if (IsPrimary)
			{
				m_parentChart.SerializePlotArea(records);
				m_globalFormats.PrimaryFormats.Serialize(records);
			}
			else
			{
				m_globalFormats.SecondaryFormats.Serialize(records);
			}
			records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
		}
	}

	public void CreatePrimaryFormats()
	{
		ChartGlobalFormatsCollection chartGlobalFormatsCollection = new ChartGlobalFormatsCollection(base.Application, ParentChart.PrimaryParentAxis, ParentChart.SecondaryParentAxis);
		ParentChart.PrimaryParentAxis.m_globalFormats = chartGlobalFormatsCollection;
		ParentChart.SecondaryParentAxis.m_globalFormats = chartGlobalFormatsCollection;
		if (!m_parentChart.ParentWorkbook.IsWorkbookOpening)
		{
			ChartFormatImpl formatToAdd = new ChartFormatImpl(base.Application, chartGlobalFormatsCollection.PrimaryFormats);
			chartGlobalFormatsCollection.PrimaryFormats.Add(formatToAdd, bCanReplace: false);
		}
	}

	public void UpdateSecondaryAxis(bool bCreateAxis)
	{
		m_parentAxis = (ChartAxisParentRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAxisParent);
		m_parentAxis.AxesIndex = 1;
		m_position = (ChartPosRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPos);
		if (bCreateAxis)
		{
			if (m_categoryAxis == null)
			{
				m_categoryAxis = new ChartCategoryAxisImpl(base.Application, this, OfficeAxisType.Category, bIsPrimary: false);
			}
			if (m_valueAxis == null)
			{
				m_valueAxis = new ChartValueAxisImpl(base.Application, this, OfficeAxisType.Value, bIsPrimary: false);
			}
		}
	}

	public ChartParentAxisImpl Clone(object parent, Dictionary<int, int> dicFontIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		ChartParentAxisImpl chartParentAxisImpl = (ChartParentAxisImpl)MemberwiseClone();
		chartParentAxisImpl.SetParent(parent);
		chartParentAxisImpl.SetParents();
		chartParentAxisImpl.m_parentAxis = (ChartAxisParentRecord)CloneUtils.CloneCloneable(m_parentAxis);
		chartParentAxisImpl.m_position = (ChartPosRecord)CloneUtils.CloneCloneable(m_position);
		if (IsPrimary)
		{
			if (m_globalFormats != null)
			{
				chartParentAxisImpl.m_globalFormats = m_globalFormats.CloneForPrimary(chartParentAxisImpl);
			}
		}
		else if (m_globalFormats != null)
		{
			chartParentAxisImpl.m_globalFormats = chartParentAxisImpl.ParentChart.PrimaryParentAxis.Formats;
			m_globalFormats.CloneForSecondary(chartParentAxisImpl.m_globalFormats, chartParentAxisImpl);
		}
		if (m_seriesAxis != null)
		{
			chartParentAxisImpl.m_seriesAxis = (ChartSeriesAxisImpl)m_seriesAxis.Clone(chartParentAxisImpl, dicFontIndexes, dicNewSheetNames);
		}
		if (m_valueAxis != null)
		{
			chartParentAxisImpl.m_valueAxis = (ChartValueAxisImpl)m_valueAxis.Clone(chartParentAxisImpl, dicFontIndexes, dicNewSheetNames);
		}
		if (m_categoryAxis != null)
		{
			chartParentAxisImpl.m_categoryAxis = (ChartCategoryAxisImpl)m_categoryAxis.Clone(chartParentAxisImpl, dicFontIndexes, dicNewSheetNames);
		}
		return chartParentAxisImpl;
	}

	public void ClearGridLines()
	{
		if (m_categoryAxis != null)
		{
			m_categoryAxis.HasMajorGridLines = false;
			m_categoryAxis.HasMinorGridLines = false;
		}
		if (m_valueAxis != null)
		{
			m_categoryAxis.HasMajorGridLines = false;
			m_categoryAxis.HasMinorGridLines = false;
		}
		if (m_seriesAxis != null)
		{
			m_categoryAxis.HasMajorGridLines = false;
			m_categoryAxis.HasMinorGridLines = false;
		}
	}

	public void MarkUsedReferences(bool[] usedItems)
	{
		if (m_valueAxis != null)
		{
			m_valueAxis.MarkUsedReferences(usedItems);
		}
		if (m_categoryAxis != null)
		{
			m_categoryAxis.MarkUsedReferences(usedItems);
		}
		if (m_seriesAxis != null)
		{
			m_seriesAxis.MarkUsedReferences(usedItems);
		}
	}

	public void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		if (m_valueAxis != null)
		{
			m_valueAxis.UpdateReferenceIndexes(arrUpdatedIndexes);
		}
		if (m_categoryAxis != null)
		{
			m_categoryAxis.UpdateReferenceIndexes(arrUpdatedIndexes);
		}
		if (m_seriesAxis != null)
		{
			m_seriesAxis.UpdateReferenceIndexes(arrUpdatedIndexes);
		}
	}

	internal void RemoveAxis(bool isCategory)
	{
		if (isCategory)
		{
			m_categoryAxis = null;
		}
		else
		{
			m_valueAxis = null;
		}
	}
}
