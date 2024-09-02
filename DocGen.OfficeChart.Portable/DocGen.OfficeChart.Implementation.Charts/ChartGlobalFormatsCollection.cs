using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartGlobalFormatsCollection
{
	public static readonly OfficeChartType[] DEF_MABY_COMBINATION_TYPES = new OfficeChartType[16]
	{
		OfficeChartType.Scatter_Markers,
		OfficeChartType.Scatter_Line_Markers,
		OfficeChartType.Scatter_Line,
		OfficeChartType.Scatter_SmoothedLine_Markers,
		OfficeChartType.Scatter_SmoothedLine,
		OfficeChartType.Line,
		OfficeChartType.Line_3D,
		OfficeChartType.Line_Markers,
		OfficeChartType.Line_Markers_Stacked,
		OfficeChartType.Line_Markers_Stacked_100,
		OfficeChartType.Line_Stacked,
		OfficeChartType.Line_Stacked_100,
		OfficeChartType.Bubble,
		OfficeChartType.Bubble_3D,
		OfficeChartType.Radar_Markers,
		OfficeChartType.Radar
	};

	public static readonly string[] DEF_MABY_COMBINATION_TYPES_START = new string[4] { "Scatter", "Line", "Bubble", "Radar" };

	private ChartFormatCollection m_primary;

	private ChartFormatCollection m_secondary;

	public ChartFormatCollection PrimaryFormats => m_primary;

	public ChartFormatCollection SecondaryFormats => m_secondary;

	public ChartGlobalFormatsCollection()
	{
	}

	public ChartGlobalFormatsCollection(IApplication application, ChartParentAxisImpl primaryParent, ChartParentAxisImpl secondaryParent)
	{
		m_primary = new ChartFormatCollection(application, primaryParent);
		m_secondary = new ChartFormatCollection(application, secondaryParent);
	}

	public void Parse(IList data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
	}

	public void Remove(ChartFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		int drawingZOrder = format.DrawingZOrder;
		if (m_primary.ContainsIndex(drawingZOrder))
		{
			if (m_primary.Count == 1)
			{
				ChangeCollections();
				m_secondary.Remove(format);
			}
			else
			{
				m_primary.Remove(format);
			}
		}
		else if (m_secondary.ContainsIndex(drawingZOrder))
		{
			m_secondary.Remove(format);
		}
	}

	public void CreateCollection(IApplication application, object parent, bool bIsPrimary)
	{
		if (bIsPrimary)
		{
			m_primary = new ChartFormatCollection(application, parent);
		}
		else
		{
			m_secondary = new ChartFormatCollection(application, parent);
		}
	}

	public void ChangeCollections()
	{
		ChartFormatCollection primary = m_primary;
		object parent = m_primary.Parent;
		object parent2 = m_secondary.Parent;
		m_primary = m_secondary;
		m_primary.SetParent(parent);
		m_primary.SetParents();
		m_secondary = primary;
		m_secondary.SetParent(parent2);
		m_secondary.SetParents();
	}

	public ChartFormatImpl AddFormat(ChartFormatImpl formatToAdd, int order, int index, bool isPrimary)
	{
		ChartFormatCollection currentCollection = GetCurrentCollection(isPrimary);
		if (!m_primary.ContainsIndex(order) && !m_secondary.ContainsIndex(order))
		{
			currentCollection.SetIndex(order, index);
			return formatToAdd;
		}
		for (int num = 7; num >= order; num--)
		{
			if (m_primary.ContainsIndex(num))
			{
				m_primary.UpdateFormatsOnAdding(num);
			}
			else if (m_secondary.ContainsIndex(num))
			{
				m_secondary.UpdateFormatsOnAdding(num);
			}
		}
		currentCollection.SetIndex(order, index);
		return formatToAdd;
	}

	public void RemoveFormat(int indexToRemove, int iOrder, bool isPrimary)
	{
		GetCurrentCollection(isPrimary);
		if (isPrimary)
		{
			m_primary.UpdateIndexesAfterRemove(indexToRemove);
		}
		else
		{
			m_secondary.UpdateIndexesAfterRemove(indexToRemove);
		}
		for (int i = iOrder + 1; i < 8; i++)
		{
			if (m_primary.ContainsIndex(i))
			{
				m_primary.UpdateFormatsOnRemoving(i);
			}
			else if (m_secondary.ContainsIndex(i))
			{
				m_secondary.UpdateFormatsOnRemoving(i);
			}
		}
	}

	private ChartFormatCollection GetCurrentCollection(bool isPrimary)
	{
		if (!isPrimary)
		{
			return m_secondary;
		}
		return m_primary;
	}

	public ChartGlobalFormatsCollection CloneForPrimary(object parent)
	{
		ChartGlobalFormatsCollection chartGlobalFormatsCollection = new ChartGlobalFormatsCollection();
		if (m_primary != null)
		{
			chartGlobalFormatsCollection.m_primary = (ChartFormatCollection)m_primary.Clone(parent);
		}
		return chartGlobalFormatsCollection;
	}

	public void CloneForSecondary(ChartGlobalFormatsCollection result, object parent)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		if (m_secondary != null)
		{
			result.m_secondary = (ChartFormatCollection)m_secondary.Clone(parent);
		}
	}

	public OfficeChartType DetectChartType(ChartSeriesCollection series)
	{
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		int count = m_primary.Count;
		int count2 = series.Count;
		OfficeChartType result = OfficeChartType.Combination_Chart;
		if (count == 0)
		{
			result = OfficeChartType.Column_Clustered;
		}
		else if (count > 1)
		{
			result = OfficeChartType.Combination_Chart;
		}
		else if (m_secondary.Count == 0 || (m_secondary.Count == 1 && m_secondary.IsParetoFormat))
		{
			result = DetectTypeForPrimaryCollOnly(series);
		}
		else if (count2 >= 4 && count2 <= 5 && SecondaryFormats.ContainsIndex(1))
		{
			ChartFormatImpl chartFormatImpl = SecondaryFormats[1];
			if (chartFormatImpl != null && chartFormatImpl.IsChartChartLine && chartFormatImpl.FormatRecordType == TBIFFRecord.ChartLine && chartFormatImpl.HasHighLowLines && (series[0] as ChartSerieImpl).ParentChart.IsStock)
			{
				result = (chartFormatImpl.IsDropBar ? OfficeChartType.Stock_VolumeOpenHighLowClose : OfficeChartType.Stock_VolumeHighLowClose);
			}
		}
		return result;
	}

	private OfficeChartType DetectTypeForPrimaryCollOnly(ChartSeriesCollection series)
	{
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		if (m_secondary.Count != 0 && (m_secondary.Count != 1 || !m_secondary.IsParetoFormat))
		{
			throw new ApplicationException("Can't detect chart type");
		}
		int count = series.Count;
		ChartFormatImpl chartFormatImpl = m_primary[0];
		if (count >= 3 && count <= 4 && chartFormatImpl.FormatRecordType == TBIFFRecord.ChartLine && (series[0] as ChartSerieImpl).ParentChart.IsStock && chartFormatImpl.IsChartChartLine && chartFormatImpl.HasHighLowLines)
		{
			if (!chartFormatImpl.IsDropBar)
			{
				return OfficeChartType.Stock_HighLowClose;
			}
			return OfficeChartType.Stock_OpenHighLowClose;
		}
		ChartSerieImpl chartSerieImpl = series[0] as ChartSerieImpl;
		string text = chartSerieImpl.DetectSerieTypeStart();
		string text2 = chartSerieImpl.DetectSerieTypeString();
		OfficeChartType officeChartType = (OfficeChartType)(-1);
		if (Array.IndexOf(DEF_MABY_COMBINATION_TYPES_START, text) != -1)
		{
			for (int i = 0; i < count; i++)
			{
				ChartSerieImpl chartSerieImpl2 = series[i] as ChartSerieImpl;
				bool flag = text == "Scatter" && chartSerieImpl2.DetectSerieTypeStart() == "Scatter";
				if (chartSerieImpl2.ChartGroup != chartSerieImpl.ChartGroup || (chartSerieImpl2.DetectSerieTypeString() != text2 && !flag))
				{
					officeChartType = OfficeChartType.Combination_Chart;
					break;
				}
			}
		}
		if (officeChartType == (OfficeChartType)(-1))
		{
			officeChartType = chartSerieImpl.SerieType;
		}
		return officeChartType;
	}

	public void Clear()
	{
		m_primary.Clear();
		m_secondary.Clear();
	}

	public ChartFormatImpl ChangeNotIntimateSerieType(OfficeChartType typeToChange, OfficeChartType serieType, IApplication application, ChartImpl chart, ChartSerieImpl serieToChange)
	{
		if (application == null)
		{
			throw new ArgumentNullException("application");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		bool num = Array.IndexOf(ChartImpl.DEF_NEED_SECONDARY_AXIS, typeToChange) != -1;
		ChartSeriesCollection chartSeriesCollection = (ChartSeriesCollection)chart.Series;
		ChartFormatImpl chartFormatImpl = (num ? new ChartFormatImpl(application, m_secondary) : new ChartFormatImpl(application, m_primary));
		chartFormatImpl.ChangeSerieType(typeToChange, isSeriesCreation: false);
		chartFormatImpl.DrawingZOrder = chartSeriesCollection.FindOrderByType(typeToChange);
		List<ChartSerieImpl> seriesWithDrawingOrder = (chart.Series as ChartSeriesCollection).GetSeriesWithDrawingOrder(chartFormatImpl.DrawingZOrder);
		bool bCanReplace = seriesWithDrawingOrder.Count == 1 && seriesWithDrawingOrder[0] == serieToChange;
		if (num)
		{
			chart.IsSecondaryAxes = true;
			m_secondary.Add(chartFormatImpl, bCanReplace);
		}
		else
		{
			if (Array.IndexOf(ChartImpl.DEF_NEED_SECONDARY_AXIS, serieType) != -1)
			{
				chart.ChangePrimaryAxis(isParsing: true);
			}
			m_primary.Add(chartFormatImpl, bCanReplace);
		}
		return chartFormatImpl;
	}

	public void ChangeShallowAxis(bool bToPrimary, int iOrder, bool bAdd, int iNewOrder)
	{
		if (bToPrimary)
		{
			ChangeInAxis(m_secondary, m_primary, iOrder, iNewOrder, bAdd);
		}
		else
		{
			ChangeInAxis(m_primary, m_secondary, iOrder, iNewOrder, bAdd);
		}
	}

	private void ChangeInAxis(ChartFormatCollection from, ChartFormatCollection to, int iOrder, int iNewOrder, bool bAdd)
	{
		bool flag = false;
		ChartFormatImpl format = from.GetFormat(iOrder, !bAdd);
		ChartFormatImpl format2 = null;
		if (!bAdd && format.DataFormatOrNull != null)
		{
			flag = true;
		}
		if (!flag)
		{
			format2 = (ChartFormatImpl)format.Clone(to);
		}
		else
		{
			format.CloneDeletedFormat(to, ref format2, cloneDataFormat: false);
		}
		if (bAdd)
		{
			format2.DrawingZOrder = iNewOrder;
			to.Add(format2, bCanReplace: false);
		}
		else
		{
			to.AddFormat(format2);
		}
		if (flag)
		{
			format.CloneDeletedFormat(to, ref format2, cloneDataFormat: true);
		}
	}
}
