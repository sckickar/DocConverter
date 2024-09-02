using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartSeriesCollection : CollectionBaseEx<IOfficeChartSerie>, IOfficeChartSeries, IParentApplication, ICollection<IOfficeChartSerie>, IEnumerable<IOfficeChartSerie>, IEnumerable, ICloneParent, IList<IOfficeChartSerie>
{
	public const string DEF_START_SERIE_NAME = "Serie";

	private ChartImpl m_chart;

	private IList<IBiffStorage> m_arrTrendError = new List<IBiffStorage>();

	private IList<IBiffStorage> m_arrTrendLabels = new List<IBiffStorage>();

	private int m_trendErrorBarsIndex;

	private int m_trendsIndex;

	private List<IOfficeChartSerie> m_additionOrder = new List<IOfficeChartSerie>();

	public new IOfficeChartSerie this[int index]
	{
		get
		{
			return base.List[index];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public IOfficeChartSerie this[string name]
	{
		get
		{
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				IOfficeChartSerie officeChartSerie = base.List[i];
				if (officeChartSerie.Name == name)
				{
					return officeChartSerie;
				}
			}
			return null;
		}
	}

	internal List<IOfficeChartSerie> AdditionOrder => m_additionOrder;

	internal IList<IBiffStorage> TrendErrorList => m_arrTrendError;

	internal int TrendErrorBarIndex
	{
		get
		{
			return m_trendErrorBarsIndex;
		}
		set
		{
			m_trendErrorBarsIndex = value;
		}
	}

	internal IList<IBiffStorage> TrendLabels => m_arrTrendLabels;

	internal int TrendIndex
	{
		get
		{
			return m_trendsIndex;
		}
		set
		{
			m_trendsIndex = value;
		}
	}

	public ChartSeriesCollection(IApplication application, object parent)
		: base(application, parent)
	{
		m_chart = (ChartImpl)FindParent(typeof(ChartImpl));
		if (m_chart == null)
		{
			throw new ApplicationException("cannot find parent chart.");
		}
	}

	public IOfficeChartSerie Add()
	{
		ChartSerieImpl chartSerieImpl = new ChartSerieImpl(base.Application, this);
		chartSerieImpl.IsDefaultName = true;
		return Add(chartSerieImpl);
	}

	public IOfficeChartSerie Add(string name)
	{
		ChartSerieImpl chartSerieImpl = new ChartSerieImpl(base.Application, this);
		chartSerieImpl.Name = name;
		if (m_chart.HasTitle && m_chart.ChartTitle == null)
		{
			m_chart.ChartTitle = name;
		}
		return Add(chartSerieImpl);
	}

	public IOfficeChartSerie Add(OfficeChartType serieType)
	{
		ChartSerieImpl obj = (ChartSerieImpl)Add();
		obj.ChangeSeriesType(serieType, isSeriesCreation: true);
		return obj;
	}

	public IOfficeChartSerie Add(string name, OfficeChartType serieType)
	{
		IOfficeChartSerie officeChartSerie = Add(name);
		officeChartSerie.SerieType = serieType;
		return officeChartSerie;
	}

	public new void RemoveAt(int index)
	{
		int count = base.List.Count;
		if (index < 0 || index >= count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		ChartSerieImpl chartSerieImpl = (ChartSerieImpl)this[index];
		if (m_chart.HasLegend)
		{
			bool flag = false;
			ChartLegendEntriesColl chartLegendEntriesColl = (ChartLegendEntriesColl)m_chart.Legend.LegendEntries;
			foreach (KeyValuePair<int, ChartLegendEntryImpl> hashEntry in chartLegendEntriesColl.HashEntries)
			{
				if (hashEntry.Key == chartSerieImpl.ExistingOrder)
				{
					chartLegendEntriesColl.Remove(hashEntry.Key);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				chartLegendEntriesColl.Remove(chartSerieImpl.ExistingOrder);
			}
		}
		base.RemoveAt(index);
		if (GetCountOfSeriesWithSameDrawingOrder(chartSerieImpl.ChartGroup) == 0 && count != 1)
		{
			m_chart.RemoveFormat(chartSerieImpl.GetCommonSerieFormat());
		}
		UpdateSerieIndexAfterRemove(index);
		UpdateExistingOrderIndexAfterRemove(chartSerieImpl.ExistingOrder);
		if (!HasSecondary())
		{
			m_chart.RemoveSecondaryAxes();
		}
	}

	public void Remove(string serieName)
	{
		if (serieName == null)
		{
			throw new ArgumentException("serieName");
		}
		int i = 0;
		for (int num = base.Count; i < num; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)base.List[i];
			if (chartSerieImpl.Name == serieName)
			{
				RemoveAt(chartSerieImpl.Index);
				i--;
				num--;
			}
		}
	}

	public void ParseSiIndex(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		if (biffRecordRaw.TypeCode != TBIFFRecord.ChartSiIndex)
		{
			throw new ArgumentOutOfRangeException("ChartSiIndex record was expected.");
		}
		int numIndex = ((ChartSiIndexRecord)biffRecordRaw).NumIndex;
		iPos++;
		biffRecordRaw = data[iPos];
		while (biffRecordRaw.TypeCode == TBIFFRecord.Number || biffRecordRaw.TypeCode == TBIFFRecord.Label || biffRecordRaw.TypeCode == TBIFFRecord.Blank)
		{
			ICellPositionFormat cellPositionFormat = (ICellPositionFormat)biffRecordRaw;
			iPos++;
			biffRecordRaw = data[iPos];
			if (cellPositionFormat.Column < base.Count)
			{
				AddEnteredRecord(numIndex, cellPositionFormat);
			}
		}
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		m_arrTrendError.Clear();
		m_arrTrendLabels.Clear();
		m_trendErrorBarsIndex = base.Count;
		m_trendsIndex = base.Count;
		foreach (ChartSerieImpl item in base.List)
		{
			item.Serialize(records);
		}
		records.AddRange(m_arrTrendError);
	}

	[CLSCompliant(false)]
	public void SerializeDataLabels(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			((ChartSerieImpl)base.InnerList[i]).SerializeDataLabels(records);
		}
	}

	public IOfficeChartSerie Add(ChartSerieImpl serieToAdd)
	{
		if (serieToAdd == null)
		{
			throw new ArgumentNullException("serieToAdd");
		}
		if (serieToAdd.IsDefaultName)
		{
			serieToAdd.SetDefaultName(GetDefSerieName(), isClearNameRange: false);
		}
		base.Add(serieToAdd);
		serieToAdd.Index = base.List.Count - 1;
		if (!m_chart.ParentWorkbook.IsWorkbookOpening)
		{
			serieToAdd.Number = serieToAdd.Index;
			m_additionOrder.Clear();
		}
		else
		{
			m_additionOrder.Add(serieToAdd);
		}
		return serieToAdd;
	}

	protected override void OnClear()
	{
		base.OnClear();
	}

	public ChartSeriesCollection Clone(object parent, Dictionary<string, string> hashNewNames, Dictionary<int, int> dicFontIndexes)
	{
		ChartSeriesCollection chartSeriesCollection = new ChartSeriesCollection(base.Application, parent);
		int i = 0;
		for (int count = base.InnerList.Count; i < count; i++)
		{
			ChartSerieImpl serieToAdd = (base.InnerList[i] as ChartSerieImpl).Clone(chartSeriesCollection, hashNewNames, dicFontIndexes);
			chartSeriesCollection.Add(serieToAdd);
		}
		if (m_arrTrendError != null)
		{
			chartSeriesCollection.m_arrTrendError = m_arrTrendError;
		}
		if (m_arrTrendLabels != null)
		{
			chartSeriesCollection.m_arrTrendLabels = m_arrTrendLabels;
		}
		return chartSeriesCollection;
	}

	public override object Clone(object parent)
	{
		ChartSeriesCollection chartSeriesCollection = new ChartSeriesCollection(base.Application, parent);
		List<IOfficeChartSerie> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			ChartSerieImpl serieToAdd = (innerList[i] as ChartSerieImpl).Clone(chartSeriesCollection, null, null);
			chartSeriesCollection.Add(serieToAdd);
		}
		return chartSeriesCollection;
	}

	public int GetCountOfSeriesWithSameDrawingOrder(int order)
	{
		int num = 0;
		int i = 0;
		for (int count = base.List.Count; i < count; i++)
		{
			if (((ChartSerieImpl)base.List[i]).ChartGroup == order)
			{
				num++;
			}
		}
		return num;
	}

	public List<ChartSerieImpl> GetSeriesWithDrawingOrder(int order)
	{
		List<ChartSerieImpl> list = new List<ChartSerieImpl>();
		int i = 0;
		for (int count = base.List.Count; i < count; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)base.List[i];
			if (chartSerieImpl.ChartGroup == order)
			{
				list.Add(chartSerieImpl);
			}
		}
		return list;
	}

	public int GetCountOfSeriesWithSameType(OfficeChartType type, bool usePrimaryAxis)
	{
		int num = 0;
		int i = 0;
		for (int count = base.List.Count; i < count; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)base.List[i];
			if (chartSerieImpl.SerieType == type && chartSerieImpl.UsePrimaryAxis == usePrimaryAxis)
			{
				num++;
			}
		}
		return num;
	}

	public int GetCountOfSeriesWithSameStartType(OfficeChartType type)
	{
		string startSerieType = ChartFormatImpl.GetStartSerieType(type);
		int num = 0;
		int i = 0;
		for (int count = base.List.Count; i < count; i++)
		{
			if (((ChartSerieImpl)base.List[i]).StartType == startSerieType)
			{
				num++;
			}
		}
		return num;
	}

	internal void ClearSeriesForChangeChartType(bool preserveFormats, OfficeChartType type)
	{
		List<ChartDataLabelsImpl> list = null;
		if (!m_chart.Loading)
		{
			list = new List<ChartDataLabelsImpl>(base.Count);
		}
		for (int i = 0; i < base.Count; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)base.List[i];
			chartSerieImpl.ChartGroup = 0;
			if (list != null)
			{
				ChartDataPointImpl chartDataPointImpl = chartSerieImpl.DataPoints.DefaultDataPoint as ChartDataPointImpl;
				if (chartDataPointImpl.HasDataLabels)
				{
					ChartDataLabelsImpl chartDataLabelsImpl = (chartDataPointImpl.DataLabels as ChartDataLabelsImpl).Clone(chartDataPointImpl, null, null) as ChartDataLabelsImpl;
					if (type == OfficeChartType.Doughnut)
					{
						chartDataLabelsImpl.Position = OfficeDataLabelPosition.Automatic;
					}
					list.Add(chartDataLabelsImpl);
				}
				else
				{
					list.Add(null);
				}
			}
			if (preserveFormats)
			{
				((ChartDataPointsCollection)chartSerieImpl.DataPoints).ClearWithExistingFormats(m_chart.DestinationType);
			}
			else
			{
				((ChartDataPointsCollection)chartSerieImpl.DataPoints).Clear();
			}
		}
		if (list == null)
		{
			return;
		}
		for (int j = 0; j < base.Count; j++)
		{
			if (list[j] != null)
			{
				(base.List[j].DataPoints.DefaultDataPoint as ChartDataPointImpl).DataLabels = list[j];
			}
		}
		list.Clear();
		list = null;
	}

	public void ClearSeriesForChangeChartType()
	{
		ClearSeriesForChangeChartType(preserveFormats: false, OfficeChartType.Column_Clustered);
	}

	public int FindOrderByType(OfficeChartType type)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<int, object> dictionary2 = new Dictionary<int, object>(5);
		string startSerieType;
		for (int i = 0; i < base.Count; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)base.List[i];
			int chartGroup = chartSerieImpl.ChartGroup;
			if (!dictionary2.ContainsKey(chartGroup))
			{
				dictionary2.Add(chartGroup, null);
				startSerieType = ChartFormatImpl.GetStartSerieType(chartSerieImpl.SerieType);
				dictionary[startSerieType] = null;
			}
		}
		startSerieType = ChartFormatImpl.GetStartSerieType(type);
		int num = 0;
		int j = 0;
		for (int num2 = ChartImpl.DEF_PRIORITY_START_TYPES.Length; j < num2; j++)
		{
			string text = ChartImpl.DEF_PRIORITY_START_TYPES[j];
			if (startSerieType == text)
			{
				return num;
			}
			if (dictionary.ContainsKey(text))
			{
				num++;
			}
		}
		throw new ApplicationException("Cannot find order.");
	}

	public void UpdateDataPointForCylConePurChartType(OfficeBaseFormat baseFormat, OfficeTopFormat topFormat)
	{
		for (int i = 0; i < base.Count; i++)
		{
			IOfficeChartSerieDataFormat dataFormat = ((ChartSerieImpl)base.List[i]).DataPoints.DefaultDataPoint.DataFormat;
			dataFormat.BarShapeBase = baseFormat;
			dataFormat.BarShapeTop = topFormat;
		}
	}

	private void AddEnteredRecord(int siIndex, ICellPositionFormat record)
	{
		if (siIndex > 3 || siIndex < 1)
		{
			throw new ArgumentOutOfRangeException("siIndex");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		((ChartSerieImpl)base.List[record.Column]).AddEnteredRecord(siIndex, record);
	}

	public List<BiffRecordRaw> GetEnteredRecords(int siIndex)
	{
		if (siIndex > 3 || siIndex < 1)
		{
			throw new ArgumentOutOfRangeException("siIndex");
		}
		List<BiffRecordRaw> list = new List<BiffRecordRaw>();
		List<List<BiffRecordRaw>> arrays = GetArrays(siIndex);
		if (arrays == null)
		{
			return null;
		}
		int num = arrays[0].Count;
		int i = 1;
		for (int count = arrays.Count; i < count; i++)
		{
			num = Math.Max(num, arrays[i].Count);
		}
		for (int j = 0; j < num; j++)
		{
			int k = 0;
			for (int count2 = arrays.Count; k < count2; k++)
			{
				List<BiffRecordRaw> list2 = arrays[k];
				if (list2.Count > j)
				{
					list.Add(list2[j]);
				}
			}
		}
		return list;
	}

	private List<List<BiffRecordRaw>> GetArrays(int siIndex)
	{
		List<List<BiffRecordRaw>> list = new List<List<BiffRecordRaw>>();
		for (int i = 0; i < base.Count; i++)
		{
			List<BiffRecordRaw> array = ((ChartSerieImpl)base.List[i]).GetArray(siIndex);
			if (array != null)
			{
				list.Add(array);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list;
	}

	public void UpdateSerieIndexAfterRemove(int iRemoveIndex)
	{
		if (iRemoveIndex < 0 || iRemoveIndex > base.List.Count)
		{
			throw new ArgumentOutOfRangeException("iRemoveIndex");
		}
		int i = iRemoveIndex;
		for (int count = base.List.Count; i < count; i++)
		{
			((ChartSerieImpl)base.List[i]).Index--;
		}
	}

	internal void UpdateExistingOrderIndexAfterRemove(int iRemoveIndex)
	{
		if (iRemoveIndex < 0 || iRemoveIndex > base.List.Count)
		{
			throw new ArgumentOutOfRangeException("iRemoveIndex");
		}
		int i = 0;
		for (int count = base.List.Count; i < count; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)base.List[i];
			if (chartSerieImpl.ExistingOrder > iRemoveIndex)
			{
				chartSerieImpl.ExistingOrder--;
			}
		}
	}

	public OfficeChartType GetTypeByOrder(int iOrder)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)base.List[i];
			if (chartSerieImpl.ChartGroup == iOrder)
			{
				return chartSerieImpl.SerieType;
			}
		}
		throw new ArgumentOutOfRangeException("iOrder");
	}

	public void ClearDataFormats(ChartSerieDataFormatImpl format)
	{
		int i = 0;
		for (int count = base.List.Count; i < count; i++)
		{
			((ChartDataPointsCollection)((ChartSerieImpl)base.List[i]).DataPoints).ClearDataFormats(format);
		}
	}

	public string GetDefSerieName()
	{
		return CollectionBaseEx<IOfficeChartSerie>.GenerateDefaultName(base.List, "Serie");
	}

	public string GetDefSerieName(int iSerieIndex)
	{
		int count = base.List.Count;
		if (iSerieIndex > count || iSerieIndex < 0)
		{
			throw new ArgumentOutOfRangeException("iSerieIndex");
		}
		IList<IOfficeChartSerie> list;
		if (iSerieIndex == base.List.Count)
		{
			list = base.List;
		}
		else
		{
			list = new List<IOfficeChartSerie>(iSerieIndex);
			for (int i = 0; i < iSerieIndex; i++)
			{
				list.Add(base.List[i]);
			}
		}
		return CollectionBaseEx<IOfficeChartSerie>.GenerateDefaultName(list, "Serie");
	}

	public void UpdateFormula(int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect)
	{
		List<IOfficeChartSerie> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			((ChartSerieImpl)innerList[i]).UpdateFormula(iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect);
		}
	}

	public int GetLegendEntryOffset(int iSerIndex)
	{
		if (iSerIndex >= base.Count)
		{
			throw new ArgumentOutOfRangeException("iSerIndex");
		}
		int num = 0;
		for (int i = 0; i < iSerIndex; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)base.List[i];
			num += chartSerieImpl.TrendLines.Count;
		}
		return num + base.Count;
	}

	public void AssignTrendDataLabel(ChartTextAreaImpl area)
	{
		if (area == null)
		{
			throw new ArgumentNullException("area");
		}
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			IOfficeChartTrendLines trendLines = base.List[i].TrendLines;
			int j = 0;
			for (int count2 = trendLines.Count; j < count2; j++)
			{
				ChartTrendLineImpl chartTrendLineImpl = (ChartTrendLineImpl)trendLines[j];
				if (chartTrendLineImpl.Index == area.ObjectLink.SeriesNumber)
				{
					chartTrendLineImpl.SetDataLabel(area);
					return;
				}
			}
		}
	}

	internal void ClearErrorBarsAndTrends()
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			IOfficeChartSerie officeChartSerie = base.List[i];
			IOfficeChartTrendLines trendLines = officeChartSerie.TrendLines;
			officeChartSerie.HasErrorBarsX = false;
			officeChartSerie.HasErrorBarsY = false;
			trendLines.Clear();
		}
	}

	internal void ResortSeries(Dictionary<int, int> dictSeriesAxis, List<int> markerSeriesList)
	{
		int count = base.Count;
		bool flag = markerSeriesList != null && markerSeriesList.Count > 0;
		if (count < 1)
		{
			return;
		}
		List<IOfficeChartSerie> innerList = base.InnerList;
		SortedList<int, ChartSerieImpl> sortedList = new SortedList<int, ChartSerieImpl>();
		List<int> list = new List<int>(count);
		for (int i = 0; i < count; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)innerList[i];
			int index = chartSerieImpl.Index;
			if (sortedList.ContainsKey(index) && (chartSerieImpl.IsFiltered || sortedList[index].IsFiltered))
			{
				list.Add(i);
			}
			else
			{
				sortedList.Add(index, chartSerieImpl);
			}
		}
		if (list.Count > 0)
		{
			int j = 0;
			for (int k = 0; k < list.Count; k++)
			{
				ChartSerieImpl chartSerieImpl2 = (ChartSerieImpl)innerList[list[k]];
				for (; sortedList.ContainsKey(j) && j < 65535; j++)
				{
					chartSerieImpl2.Index = j;
				}
				sortedList.Add(j, chartSerieImpl2);
			}
		}
		IList<ChartSerieImpl> values = sortedList.Values;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int l = 0; l < count; l++)
		{
			ChartSerieImpl chartSerieImpl3 = (ChartSerieImpl)(innerList[l] = values[l]);
			int index2 = chartSerieImpl3.Index;
			dictionary[l] = index2;
			chartSerieImpl3.Index = l;
		}
		for (int m = 0; m < count; m++)
		{
			int index2 = dictionary[m];
			if (dictSeriesAxis.TryGetValue(index2, out var value))
			{
				ChartAxisImpl chartAxisImpl = m_chart.PrimaryCategoryAxis as ChartAxisImpl;
				ChartAxisImpl chartAxisImpl2 = m_chart.PrimaryValueAxis as ChartAxisImpl;
				if (value != chartAxisImpl.AxisId && value != chartAxisImpl2.AxisId)
				{
					((ChartSerieImpl)innerList[m]).UsePrimaryAxis = false;
				}
			}
			if (flag && markerSeriesList.Contains(index2))
			{
				ChartMarkerFormatRecord markerFormat = ((ChartSerieDataFormatImpl)((ChartSerieImpl)innerList[m]).SerieFormat).MarkerFormat;
				((ChartSerieDataFormatImpl)((ChartSerieImpl)innerList[m]).GetCommonSerieFormat().SerieDataFormat).MarkerFormat.MarkerType = markerFormat.MarkerType;
			}
		}
	}

	public void MarkUsedReferences(bool[] usedItems)
	{
		List<IOfficeChartSerie> innerList = base.InnerList;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			((ChartSerieImpl)innerList[i]).MarkUsedReferences(usedItems);
		}
	}

	public void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		List<IOfficeChartSerie> innerList = base.InnerList;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			((ChartSerieImpl)innerList[i]).UpdateReferenceIndexes(arrUpdatedIndexes);
		}
	}

	internal bool HasSecondary()
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (!this[i].UsePrimaryAxis)
			{
				return true;
			}
		}
		return false;
	}
}
