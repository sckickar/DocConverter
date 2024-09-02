using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartTrendLineImpl : CommonObject, IOfficeChartTrendLine
{
	private const int DEF_ORDER_MAX_VALUE = 6;

	private static Dictionary<OfficeTrendLineType, string> m_hashNames;

	private ChartSerAuxTrendRecord m_record;

	private ShadowImpl m_shadow;

	private ChartBorderImpl m_border;

	private ChartSerieImpl m_serie;

	private OfficeTrendLineType m_type = OfficeTrendLineType.Linear;

	private bool m_isAutoName = true;

	private string m_strName = "";

	private ChartTextAreaImpl m_textArea;

	private int m_iIndex;

	private ThreeDFormatImpl m_3D;

	private ChartLegendEntryImpl chartLegendEntry;

	internal ChartLegendEntryImpl LegendEntry
	{
		get
		{
			return chartLegendEntry;
		}
		set
		{
			chartLegendEntry = value;
		}
	}

	public IOfficeChartBorder Border => m_border;

	public IShadow Shadow
	{
		get
		{
			if (m_shadow == null)
			{
				m_shadow = new ShadowImpl(base.Application, this);
			}
			return m_shadow;
		}
	}

	public bool HasShadowProperties
	{
		get
		{
			return m_shadow != null;
		}
		internal set
		{
			if (value)
			{
				_ = Shadow;
			}
			else
			{
				m_shadow = null;
			}
		}
	}

	public IThreeDFormat Chart3DOptions
	{
		get
		{
			if (m_3D == null)
			{
				m_3D = new ThreeDFormatImpl(base.Application, this);
			}
			return m_3D;
		}
	}

	public bool Has3dProperties
	{
		get
		{
			return m_3D != null;
		}
		internal set
		{
			if (value)
			{
				_ = Chart3DOptions;
			}
			else
			{
				m_3D = null;
			}
		}
	}

	public double Backward
	{
		get
		{
			return m_record.NumBackcast;
		}
		set
		{
			if (value != Backward)
			{
				CheckRecordProprties();
				CheckBackward(value);
				m_record.NumBackcast = value;
			}
		}
	}

	public double Forward
	{
		get
		{
			return m_record.NumForecast;
		}
		set
		{
			if (Forward != value)
			{
				CheckRecordProprties();
				if (value < 0.0)
				{
					throw new ArgumentOutOfRangeException("Forward");
				}
				m_record.NumForecast = value;
			}
		}
	}

	public bool DisplayEquation
	{
		get
		{
			return m_record.IsEquation;
		}
		set
		{
			if (value != DisplayEquation)
			{
				CheckRecordProprties();
				m_record.IsEquation = value;
				UpdateDataLabels(value);
			}
		}
	}

	public bool DisplayRSquared
	{
		get
		{
			return m_record.IsRSquared;
		}
		set
		{
			if (value != DisplayRSquared)
			{
				CheckRecordProprties();
				m_record.IsRSquared = value;
				UpdateDataLabels(value);
			}
		}
	}

	public double Intercept
	{
		get
		{
			return m_record.NumIntercept;
		}
		set
		{
			if (Intercept != value)
			{
				CheckRecordProprties();
				CheckIntercept();
				if (Type == OfficeTrendLineType.Exponential && value <= 0.0)
				{
					throw new ArgumentOutOfRangeException("Intercept");
				}
				m_record.NumIntercept = value;
			}
		}
	}

	public bool InterceptIsAuto
	{
		get
		{
			return double.IsNaN(Intercept);
		}
		set
		{
			if (InterceptIsAuto != value)
			{
				CheckRecordProprties();
				CheckIntercept();
				if (value)
				{
					Intercept = ChartSerAuxTrendRecord.DEF_NAN_VALUE;
				}
				else
				{
					Intercept = ((Type == OfficeTrendLineType.Exponential) ? 1 : 0);
				}
			}
		}
	}

	public OfficeTrendLineType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
			OnTypeChanging(value);
		}
	}

	public int Order
	{
		get
		{
			return m_record.Order;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("Order");
			}
			m_record.Order = (byte)value;
		}
	}

	public bool NameIsAuto
	{
		get
		{
			return m_isAutoName;
		}
		set
		{
			if (NameIsAuto != value)
			{
				if (value)
				{
					m_strName = string.Empty;
				}
				m_isAutoName = value;
			}
		}
	}

	public string Name
	{
		get
		{
			if (!NameIsAuto)
			{
				return m_strName;
			}
			OfficeTrendLineType type = Type;
			string text = m_hashNames[type];
			if (type == OfficeTrendLineType.Moving_Average)
			{
				text = Order + text;
			}
			return text + "(" + m_serie.Name + ")";
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Name");
			}
			m_strName = value;
			NameIsAuto = false;
		}
	}

	public IOfficeChartTextArea DataLabel
	{
		get
		{
			if (m_textArea == null)
			{
				throw new NotSupportedException("Cannot return data label.");
			}
			return m_textArea;
		}
	}

	public int Index
	{
		get
		{
			return m_iIndex;
		}
		set
		{
			m_iIndex = value;
		}
	}

	static ChartTrendLineImpl()
	{
		m_hashNames = new Dictionary<OfficeTrendLineType, string>(6);
		m_hashNames.Add(OfficeTrendLineType.Exponential, "Expon. ");
		m_hashNames.Add(OfficeTrendLineType.Linear, "Linear ");
		m_hashNames.Add(OfficeTrendLineType.Logarithmic, "Log. ");
		m_hashNames.Add(OfficeTrendLineType.Moving_Average, " per. Mov. Avg. ");
		m_hashNames.Add(OfficeTrendLineType.Polynomial, "Poly. ");
		m_hashNames.Add(OfficeTrendLineType.Power, "Power ");
	}

	public ChartTrendLineImpl(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
		m_border = new ChartBorderImpl(application, this);
		m_border.HasLineProperties = true;
		m_record = (ChartSerAuxTrendRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSerAuxTrend);
	}

	private void FindParents()
	{
		m_serie = (ChartSerieImpl)FindParent(typeof(ChartSerieImpl));
		if (m_serie == null)
		{
			throw new NotSupportedException("Cannot find parent objects");
		}
	}

	public ChartTrendLineImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos, out ChartLegendEntryImpl entry)
		: base(application, parent)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		FindParents();
		entry = null;
		Parse(data, ref iPos, ref entry);
	}

	public void ClearFormats()
	{
		m_border.AutoFormat = true;
		m_type = OfficeTrendLineType.Linear;
		m_isAutoName = true;
		m_strName = "";
		m_record.Order = 1;
		m_record.IsRSquared = false;
		m_record.IsEquation = false;
		m_textArea = null;
		m_record.NumForecast = 0.0;
		m_record.NumBackcast = 0.0;
		m_record.NumIntercept = ChartSerAuxTrendRecord.DEF_NAN_VALUE;
	}

	public void MarkUsedReferences(bool[] usedItems)
	{
		if (m_textArea != null)
		{
			m_textArea.MarkUsedReferences(usedItems);
		}
	}

	public void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		if (m_textArea != null)
		{
			m_textArea.UpdateReferenceIndexes(arrUpdatedIndexes);
		}
	}

	private void Parse(IList<BiffRecordRaw> data, ref int iPos, ref ChartLegendEntryImpl entry)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		if (biffRecordRaw.TypeCode != TBIFFRecord.ChartSeries)
		{
			throw new ArgumentOutOfRangeException("iPos");
		}
		iPos += 2;
		int num = 1;
		ChartImpl parentChart = m_serie.ParentChart;
		bool hasLegend = parentChart.HasLegend;
		ChartLegendEntriesColl parent = null;
		if (hasLegend)
		{
			parent = (ChartLegendEntriesColl)parentChart.Legend.LegendEntries;
		}
		while (num > 0)
		{
			biffRecordRaw = data[iPos];
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.Begin:
				num++;
				break;
			case TBIFFRecord.End:
				num--;
				break;
			case TBIFFRecord.ChartSerAuxTrend:
				m_record = (ChartSerAuxTrendRecord)biffRecordRaw;
				break;
			case TBIFFRecord.ChartDataFormat:
				m_iIndex = ((ChartDataFormatRecord)biffRecordRaw).SeriesIndex;
				break;
			case TBIFFRecord.ChartLineFormat:
				m_border = new ChartBorderImpl(base.Application, this, (ChartLineFormatRecord)biffRecordRaw);
				break;
			case TBIFFRecord.ChartLegendxn:
				if (hasLegend)
				{
					int trendIndex = m_serie.ParentSeries.TrendIndex;
					entry = new ChartLegendEntryImpl(base.Application, parent, trendIndex, data, ref iPos);
					iPos--;
				}
				break;
			case TBIFFRecord.ChartSeriesText:
			{
				ChartSeriesTextRecord chartSeriesTextRecord = (ChartSeriesTextRecord)biffRecordRaw;
				Name = chartSeriesTextRecord.Text;
				break;
			}
			}
			iPos++;
		}
		iPos--;
		UpdateType();
	}

	[CLSCompliant(false)]
	public void Serialize(IList<IBiffStorage> records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		int index = m_serie.Index;
		ChartSeriesCollection parentSeries = m_serie.ParentSeries;
		ChartErrorBarsImpl.SerializeSerieRecord(records, 0);
		BiffRecordRaw record = BiffRecordFactory.GetRecord(TBIFFRecord.Begin);
		records.Add(record);
		SerializeChartAi(records);
		ChartErrorBarsImpl.SerializeDataFormatRecords(records, m_border, index, parentSeries.TrendErrorBarIndex, null);
		ChartSerParentRecord chartSerParentRecord = (ChartSerParentRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSerParent);
		SerializeDataLabels(parentSeries.TrendLabels);
		chartSerParentRecord.Series = (ushort)(index + 1);
		records.Add(chartSerParentRecord);
		m_record.UpdateType(m_type);
		records.Add(m_record);
		SerializeLegendEntry(records);
		parentSeries.TrendIndex++;
		parentSeries.TrendErrorBarIndex++;
		record = BiffRecordFactory.GetRecord(TBIFFRecord.End);
		records.Add(record);
	}

	private void SerializeChartAi(IList<IBiffStorage> records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		ChartAIRecord chartAIRecord = (ChartAIRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAI);
		chartAIRecord.IndexIdentifier = ChartAIRecord.LinkIndex.LinkToTitleOrText;
		chartAIRecord.Reference = ChartAIRecord.ReferenceType.EnteredDirectly;
		records.Add(chartAIRecord);
		if (!NameIsAuto)
		{
			ChartSeriesTextRecord chartSeriesTextRecord = (ChartSeriesTextRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSeriesText);
			chartSeriesTextRecord.Text = m_strName;
			records.Add(chartSeriesTextRecord);
		}
		chartAIRecord = (ChartAIRecord)chartAIRecord.Clone();
		chartAIRecord.IndexIdentifier = ChartAIRecord.LinkIndex.LinkToValues;
		records.Add(chartAIRecord);
		chartAIRecord = (ChartAIRecord)chartAIRecord.Clone();
		chartAIRecord.IndexIdentifier = ChartAIRecord.LinkIndex.LinkToCategories;
		records.Add(chartAIRecord);
		chartAIRecord = (ChartAIRecord)chartAIRecord.Clone();
		chartAIRecord.IndexIdentifier = ChartAIRecord.LinkIndex.LinkToBubbles;
		records.Add(chartAIRecord);
	}

	private void SerializeLegendEntry(IList<IBiffStorage> records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		ChartImpl parentChart = m_serie.ParentChart;
		if (parentChart.HasLegend)
		{
			int trendIndex = m_serie.ParentSeries.TrendIndex;
			((ChartLegendEntryImpl)parentChart.Legend.LegendEntries[trendIndex]).Serialize(records);
		}
	}

	private void SerializeDataLabels(IList<IBiffStorage> records)
	{
		if (m_textArea != null)
		{
			m_textArea.ObjectLink.SeriesNumber = (ushort)m_serie.ParentSeries.TrendErrorBarIndex;
			m_textArea.Serialize(records);
		}
	}

	private void CheckRecordProprties()
	{
		if (Type == OfficeTrendLineType.Moving_Average)
		{
			throw new NotSupportedException("This property doesnot support on current trend line type.");
		}
	}

	private void CheckIntercept()
	{
		OfficeTrendLineType type = Type;
		if (type == OfficeTrendLineType.Logarithmic || type == OfficeTrendLineType.Power)
		{
			throw new NotSupportedException("This property doesnot support in current trend line type.");
		}
	}

	private void CheckBackward(double value)
	{
		if (value < 0.0)
		{
			throw new ArgumentOutOfRangeException("Backward");
		}
		string startSerieType = ChartFormatImpl.GetStartSerieType(m_serie.SerieType);
		if ((startSerieType == "Bar" || startSerieType == "Column" || startSerieType == "Line") && value > 0.5)
		{
			throw new ArgumentOutOfRangeException("The value must be between 0 and 0.5");
		}
		if (startSerieType == "Area")
		{
			throw new NotSupportedException("This property doesnot supported on current trendline object.");
		}
	}

	private void OnTypeChanging(OfficeTrendLineType type)
	{
		bool flag = type == OfficeTrendLineType.Moving_Average;
		if (flag && m_serie.PointNumber < 3 && !m_serie.ParentBook.IsWorkbookOpening)
		{
			throw new NotSupportedException("This trendline type is supported only if data points count is greater than 2");
		}
		m_record.Order = (byte)((!flag && type != 0) ? 1u : 2u);
		m_record.NumIntercept = ChartSerAuxTrendRecord.DEF_NAN_VALUE;
	}

	private void CheckOrder(int value)
	{
		if (m_type == OfficeTrendLineType.Polynomial || m_type == OfficeTrendLineType.Moving_Average)
		{
			int num = 6;
			if (m_type != OfficeTrendLineType.Moving_Average)
			{
				num = m_serie.PointNumber - 1;
			}
			if (value < 2 || value > num)
			{
				throw new ArgumentOutOfRangeException("Order");
			}
			return;
		}
		throw new NotSupportedException("This property doesnot support in current trendline type.");
	}

	private void UpdateDataLabels(bool value)
	{
		if (value && m_textArea == null)
		{
			m_textArea = new ChartTextAreaImpl(base.Application, this);
			m_textArea.IsTrend = true;
		}
		else if (!DisplayEquation && !DisplayRSquared)
		{
			m_textArea = null;
		}
	}

	private void UpdateType()
	{
		bool num = m_record.RegressionType == ChartSerAuxTrendRecord.TRegression.Polynomial;
		ChartSeriesCollection parentSeries = m_serie.ParentSeries;
		if (num && m_record.Order < 2)
		{
			m_type = OfficeTrendLineType.Linear;
		}
		else
		{
			m_type = (OfficeTrendLineType)m_record.RegressionType;
		}
		parentSeries.TrendIndex++;
	}

	public void SetDataLabel(ChartTextAreaImpl area)
	{
		if (area == null)
		{
			throw new ArgumentNullException("area");
		}
		m_textArea = area;
		m_textArea.IsTrend = true;
	}

	public ChartTrendLineImpl Clone(object parent, Dictionary<int, int> dicFontIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ChartTrendLineImpl chartTrendLineImpl = (ChartTrendLineImpl)MemberwiseClone();
		chartTrendLineImpl.SetParent(parent);
		chartTrendLineImpl.FindParents();
		chartTrendLineImpl.m_border = m_border.Clone(chartTrendLineImpl);
		if (m_record != null)
		{
			chartTrendLineImpl.m_record = (ChartSerAuxTrendRecord)CloneUtils.CloneCloneable(m_record);
		}
		if (m_textArea != null)
		{
			chartTrendLineImpl.m_textArea = (ChartTextAreaImpl)m_textArea.Clone(chartTrendLineImpl, dicFontIndexes, dicNewSheetNames);
		}
		chartTrendLineImpl.m_type = m_type;
		if (m_shadow != null)
		{
			chartTrendLineImpl.m_shadow = m_shadow.Clone(chartTrendLineImpl);
		}
		if (m_3D != null)
		{
			chartTrendLineImpl.m_3D = m_3D.Clone(chartTrendLineImpl);
		}
		return chartTrendLineImpl;
	}
}
