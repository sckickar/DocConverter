using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartDataPointsCollection : CommonObject, IOfficeChartDataPoints, IParentApplication, IEnumerable
{
	private ChartDataPointImpl m_dataPointDefault;

	internal Dictionary<int, ChartDataPointImpl> m_hashDataPoints = new Dictionary<int, ChartDataPointImpl>();

	private ChartSerieImpl m_series;

	private ChartImpl m_chart;

	public IOfficeChartDataPoint this[int index]
	{
		get
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (index == 65535)
			{
				return DefaultDataPoint;
			}
			WorksheetImpl worksheetImpl = FindParent(typeof(WorksheetImpl)) as WorksheetImpl;
			if (!IsLoading && (worksheetImpl == null || !worksheetImpl.IsParsing))
			{
				if (m_series != null)
				{
					int pointNumber = m_series.PointNumber;
					if (index >= pointNumber)
					{
						throw new ArgumentOutOfRangeException("index");
					}
				}
				else if (m_chart != null && m_chart.Series.Count > 0)
				{
					int num = (m_chart.Series[0] as ChartSerieImpl).PointNumber;
					for (int i = 1; i < m_chart.Series.Count; i++)
					{
						int pointNumber2 = (m_chart.Series[i] as ChartSerieImpl).PointNumber;
						if (num < pointNumber2)
						{
							num = pointNumber2;
						}
					}
					if (index >= num)
					{
						throw new ArgumentOutOfRangeException("index");
					}
				}
			}
			ChartDataPointImpl chartDataPointImpl;
			if (m_hashDataPoints.ContainsKey(index))
			{
				chartDataPointImpl = m_hashDataPoints[index];
			}
			else
			{
				chartDataPointImpl = new ChartDataPointImpl(base.Application, this, index);
				Add(chartDataPointImpl);
			}
			if (m_chart == null && m_series != null)
			{
				ChartSerieDataFormatImpl dataFormatOrNull = ((ChartDataPointImpl)DefaultDataPoint).DataFormatOrNull;
				if (dataFormatOrNull != null && dataFormatOrNull.IsFormatted)
				{
					chartDataPointImpl.CloneDataFormat(dataFormatOrNull);
				}
			}
			return chartDataPointImpl;
		}
	}

	public IOfficeChartDataPoint DefaultDataPoint
	{
		get
		{
			if (m_chart == null && m_series != null && !m_series.InnerChart.Loading && !m_series.InnerChart.TypeChanging)
			{
				ChartFormatImpl commonSerieFormat = m_series.GetCommonSerieFormat();
				m_dataPointDefault.CloneDataFormat(commonSerieFormat.DataFormatOrNull);
			}
			return m_dataPointDefault;
		}
	}

	public bool IsLoading
	{
		get
		{
			if (m_series != null)
			{
				return m_series.InnerWorkbook.IsWorkbookOpening;
			}
			return m_chart.InnerWorkbook.IsWorkbookOpening;
		}
	}

	public ChartSerieDataFormatImpl DefPointFormatOrNull => m_dataPointDefault.DataFormatOrNull;

	public int DeninedDPCount => m_hashDataPoints.Count;

	public ChartDataPointsCollection(IApplication application, object parent)
		: base(application, parent)
	{
		m_dataPointDefault = new ChartDataPointImpl(base.Application, this, 65535);
		if (parent is ChartSerieImpl)
		{
			SetParents();
			m_chart = null;
		}
		else
		{
			SetParentChart();
			m_series = null;
		}
	}

	private void SetParents()
	{
		m_series = FindParent(typeof(ChartSerieImpl)) as ChartSerieImpl;
		if (m_series == null)
		{
			throw new ArgumentNullException("Can't find parent series.");
		}
	}

	private void SetParentChart()
	{
		m_chart = FindParent(typeof(ChartImpl)) as ChartImpl;
		if (m_chart == null)
		{
			throw new ArgumentNullException("Can't find parent chart.");
		}
	}

	[CLSCompliant(false)]
	public void SerializeDataLabels(OffsetArrayList records)
	{
		foreach (ChartDataPointImpl value in m_hashDataPoints.Values)
		{
			value.SerializeDataLabels(records);
		}
		if (m_dataPointDefault != null)
		{
			m_dataPointDefault.SerializeDataLabels(records);
		}
	}

	[CLSCompliant(false)]
	public void SerializeDataFormats(OffsetArrayList records)
	{
		foreach (ChartDataPointImpl value in m_hashDataPoints.Values)
		{
			value.SerializeDataFormat(records);
		}
		if (m_dataPointDefault != null)
		{
			m_dataPointDefault.SerializeDataFormat(records);
		}
	}

	public object Clone(object parent, WorkbookImpl book, Dictionary<int, int> dicFontIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		ChartDataPointsCollection chartDataPointsCollection = (ChartDataPointsCollection)MemberwiseClone();
		chartDataPointsCollection.SetParent(parent);
		if (parent is ChartSerieImpl)
		{
			chartDataPointsCollection.SetParents();
		}
		else if (parent is ChartImpl)
		{
			chartDataPointsCollection.SetParentChart();
		}
		int count = m_hashDataPoints.Count;
		chartDataPointsCollection.m_hashDataPoints = new Dictionary<int, ChartDataPointImpl>(count);
		if (m_dataPointDefault != null)
		{
			chartDataPointsCollection.m_dataPointDefault = (ChartDataPointImpl)m_dataPointDefault.Clone(chartDataPointsCollection, dicFontIndexes, dicNewSheetNames);
		}
		if (count > 0)
		{
			foreach (ChartDataPointImpl value in m_hashDataPoints.Values)
			{
				ChartDataPointImpl point = (ChartDataPointImpl)value.Clone(chartDataPointsCollection, dicFontIndexes, dicNewSheetNames);
				chartDataPointsCollection.Add(point);
			}
		}
		return chartDataPointsCollection;
	}

	public void Add(ChartDataPointImpl point)
	{
		if (point == null)
		{
			throw new ArgumentNullException("point");
		}
		int index = point.Index;
		m_hashDataPoints[index] = point;
	}

	public void Clear()
	{
		m_hashDataPoints.Clear();
		m_dataPointDefault = new ChartDataPointImpl(base.Application, this, 65535);
		if (m_series != null)
		{
			m_dataPointDefault.DataFormat.BarShapeBase = OfficeBaseFormat.Rectangle;
			m_dataPointDefault.DataFormat.BarShapeTop = OfficeTopFormat.Straight;
		}
	}

	internal bool CheckDPDataLabels()
	{
		if (m_hashDataPoints.Count == 0)
		{
			return false;
		}
		bool result = false;
		foreach (int key in m_hashDataPoints.Keys)
		{
			if (m_hashDataPoints[key].HasDataLabels)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public void UpdateSerieIndex()
	{
		_ = m_series.Index;
		m_dataPointDefault.UpdateSerieIndex();
		foreach (ChartDataPointImpl value in m_hashDataPoints.Values)
		{
			value.UpdateSerieIndex();
		}
	}

	public void ClearDataFormats(ChartSerieDataFormatImpl format)
	{
		m_dataPointDefault.ClearDataFormats(format);
		if (m_hashDataPoints.Count == 0)
		{
			return;
		}
		foreach (ChartDataPointImpl value in m_hashDataPoints.Values)
		{
			value.ClearDataFormats(format);
		}
	}

	internal void ClearWithExistingFormats(OfficeChartType destinationType)
	{
		int count = m_hashDataPoints.Count;
		Dictionary<int, ChartDataPointImpl> dictionary = null;
		bool isInteriorSupported = ChartSerieDataFormatImpl.GetIsInteriorSupported(destinationType);
		if (count > 0)
		{
			dictionary = new Dictionary<int, ChartDataPointImpl>(count);
			foreach (int key in m_hashDataPoints.Keys)
			{
				ChartDataPointImpl chartDataPointImpl = m_hashDataPoints[key];
				ChartDataPointImpl chartDataPointImpl2 = new ChartDataPointImpl(base.Application, this, chartDataPointImpl.Index);
				chartDataPointImpl2.CloneDataFormat(chartDataPointImpl.DataFormatOrNull);
				if (isInteriorSupported)
				{
					chartDataPointImpl.DataFormatOrNull.CopyFillBackForeGroundColorObjects(chartDataPointImpl2.DataFormatOrNull);
				}
				if (chartDataPointImpl2.DataFormatOrNull != null)
				{
					chartDataPointImpl2.DataFormatOrNull.SetDefaultValuesForSerieRecords();
				}
				dictionary.Add(key, chartDataPointImpl2);
			}
			m_hashDataPoints.Clear();
		}
		else
		{
			m_hashDataPoints.Clear();
		}
		ChartSerieDataFormatImpl chartSerieDataFormatImpl = null;
		if (m_dataPointDefault.DataFormatOrNull != null)
		{
			chartSerieDataFormatImpl = m_dataPointDefault.DataFormatOrNull.Clone(m_dataPointDefault);
			if (isInteriorSupported)
			{
				m_dataPointDefault.DataFormatOrNull.CopyFillBackForeGroundColorObjects(chartSerieDataFormatImpl);
			}
		}
		m_dataPointDefault = new ChartDataPointImpl(base.Application, this, 65535);
		if (chartSerieDataFormatImpl != null)
		{
			chartSerieDataFormatImpl.SetDefaultValuesForSerieRecords();
			m_dataPointDefault.CloneDataFormat(chartSerieDataFormatImpl);
			if (isInteriorSupported)
			{
				chartSerieDataFormatImpl.CopyFillBackForeGroundColorObjects(m_dataPointDefault.DataFormatOrNull);
			}
		}
		if (dictionary != null)
		{
			m_hashDataPoints = dictionary;
		}
	}

	public IEnumerator GetEnumerator()
	{
		return m_hashDataPoints.Values.GetEnumerator();
	}
}
