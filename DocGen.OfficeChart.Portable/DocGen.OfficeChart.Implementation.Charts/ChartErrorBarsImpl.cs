using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartErrorBarsImpl : CommonObject, IOfficeChartErrorBars
{
	public const int DEF_NUMBER_X_VALUE = 1;

	public const int DEF_NUMBER_Y_VALUE = 10;

	private ChartBorderImpl m_border;

	private ChartSerAuxErrBarRecord m_errorBarRecord;

	private ShadowImpl m_shadow;

	private OfficeErrorBarInclude m_include;

	private ChartSerieImpl m_serie;

	private IRange m_plusRange;

	private IRange m_minusRange;

	private bool m_bIsY;

	private ChartAIRecord m_chartAi = (ChartAIRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAI);

	private ChartMarkerFormatRecord m_markerFormat;

	private bool m_bChanged;

	private ThreeDFormatImpl m_3D;

	private object[] m_plusRangeValues;

	private object[] m_minusRangeValues;

	private bool m_isPlusNumberLiteral;

	private bool m_isMinusNumberLiteral;

	private string m_formatCode;

	public IOfficeChartBorder Border => m_border;

	public OfficeErrorBarInclude Include
	{
		get
		{
			return m_include;
		}
		set
		{
			if (value != Include)
			{
				if (!m_serie.ParentBook.IsWorkbookOpening && !CheckInclude(value))
				{
					throw new NotSupportedException("Cannot change include value, before set range values.");
				}
				m_include = value;
			}
		}
	}

	public bool HasCap
	{
		get
		{
			return m_errorBarRecord.TeeTop;
		}
		set
		{
			m_errorBarRecord.TeeTop = value;
		}
	}

	public OfficeErrorBarType Type
	{
		get
		{
			return m_errorBarRecord.ErrorBarType;
		}
		set
		{
			m_errorBarRecord.ErrorBarType = value;
		}
	}

	public double NumberValue
	{
		get
		{
			return m_errorBarRecord.NumValue;
		}
		set
		{
			if (value < 0.0)
			{
				throw new ArgumentOutOfRangeException("NumberValue");
			}
			m_errorBarRecord.NumValue = value;
		}
	}

	internal bool IsPlusNumberLiteral
	{
		get
		{
			return m_isPlusNumberLiteral;
		}
		set
		{
			m_isPlusNumberLiteral = value;
		}
	}

	internal bool IsMinusNumberLiteral
	{
		get
		{
			return m_isMinusNumberLiteral;
		}
		set
		{
			m_isMinusNumberLiteral = value;
		}
	}

	public IRange PlusIRange
	{
		get
		{
			return m_plusRange;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("PlusRange");
			}
			if (m_include != 0)
			{
				m_include = ((m_minusRange == null) ? OfficeErrorBarInclude.Plus : OfficeErrorBarInclude.Both);
			}
			m_errorBarRecord.ErrorBarType = OfficeErrorBarType.Custom;
			m_plusRange = value;
			if (!m_serie.ParentBook.IsWorkbookOpening)
			{
				m_chartAi.ParsedExpression = (value as INativePTG).GetNativePtg();
				m_bChanged = true;
			}
		}
	}

	public IOfficeDataRange PlusRange
	{
		get
		{
			return new ChartDataRange(m_serie.ParentChart)
			{
				Range = PlusIRange
			};
		}
		set
		{
			int firstRow = value.FirstRow;
			int firstColumn = value.FirstColumn;
			int lastRow = value.LastRow;
			int lastColumn = value.LastColumn;
			PlusIRange = m_serie.ParentChart.Workbook.Worksheets[0][firstRow, firstColumn, lastRow, lastColumn];
		}
	}

	public IRange MinusIRange
	{
		get
		{
			return m_minusRange;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("PlusRange");
			}
			if (m_include != 0)
			{
				m_include = ((m_plusRange == null) ? OfficeErrorBarInclude.Minus : OfficeErrorBarInclude.Both);
			}
			m_errorBarRecord.ErrorBarType = OfficeErrorBarType.Custom;
			m_minusRange = value;
			if (!m_serie.ParentBook.IsWorkbookOpening)
			{
				m_chartAi.ParsedExpression = (value as INativePTG).GetNativePtg();
				m_bChanged = true;
			}
		}
	}

	public IOfficeDataRange MinusRange
	{
		get
		{
			return new ChartDataRange(m_serie.ParentChart)
			{
				Range = MinusIRange
			};
		}
		set
		{
			int firstRow = value.FirstRow;
			int firstColumn = value.FirstColumn;
			int lastRow = value.LastRow;
			int lastColumn = value.LastColumn;
			MinusIRange = m_serie.ParentChart.Workbook.Worksheets[0][firstRow, firstColumn, lastRow, lastColumn];
		}
	}

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

	public bool IsY => m_bIsY;

	internal object[] PlusRangeValues
	{
		get
		{
			return m_plusRangeValues;
		}
		set
		{
			m_plusRangeValues = value;
		}
	}

	internal object[] MinusRangeValues
	{
		get
		{
			return m_minusRangeValues;
		}
		set
		{
			m_minusRangeValues = value;
		}
	}

	public static void SerializeSerieRecord(IList<IBiffStorage> records, int count)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		ChartSeriesRecord chartSeriesRecord = (ChartSeriesRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSeries);
		chartSeriesRecord.BubbleDataType = ChartSeriesRecord.DataType.Numeric;
		chartSeriesRecord.StdX = ChartSeriesRecord.DataType.Numeric;
		chartSeriesRecord.StdY = ChartSeriesRecord.DataType.Numeric;
		chartSeriesRecord.ValuesCount = (ushort)count;
		records.Add(chartSeriesRecord);
	}

	public static void SerializeDataFormatRecords(IList<IBiffStorage> records, ChartBorderImpl border, int iSerieIndex, int iIndex, ChartMarkerFormatRecord marker)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (border == null)
		{
			throw new ArgumentNullException("border");
		}
		ChartDataFormatRecord chartDataFormatRecord = (ChartDataFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartDataFormat);
		chartDataFormatRecord.PointNumber = ushort.MaxValue;
		chartDataFormatRecord.SeriesIndex = (ushort)iIndex;
		chartDataFormatRecord.SeriesNumber = (ushort)iSerieIndex;
		records.Add(chartDataFormatRecord);
		BiffRecordRaw record = BiffRecordFactory.GetRecord(TBIFFRecord.Begin);
		records.Add(record);
		record = BiffRecordFactory.GetRecord(TBIFFRecord.Chart3DDataFormat);
		records.Add(record);
		border.Serialize(records);
		record = BiffRecordFactory.GetRecord(TBIFFRecord.ChartAreaFormat);
		records.Add(record);
		record = BiffRecordFactory.GetRecord(TBIFFRecord.ChartPieFormat);
		records.Add(record);
		record = ((marker == null) ? BiffRecordFactory.GetRecord(TBIFFRecord.ChartMarkerFormat) : marker);
		records.Add(record);
		record = BiffRecordFactory.GetRecord(TBIFFRecord.End);
		records.Add(record);
	}

	public ChartErrorBarsImpl(IApplication application, object parent, bool bIsY)
		: base(application, parent)
	{
		m_border = new ChartBorderImpl(application, this);
		m_bIsY = bIsY;
		m_errorBarRecord = (ChartSerAuxErrBarRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSerAuxErrBar);
		if (!bIsY)
		{
			NumberValue = 1.0;
		}
		FindParents();
	}

	public ChartErrorBarsImpl(IApplication application, object parent, IList<BiffRecordRaw> data)
		: base(application, parent)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		FindParents();
		Parse(data);
	}

	private void FindParents()
	{
		m_serie = (ChartSerieImpl)FindParent(typeof(ChartSerieImpl));
		if (m_serie == null)
		{
			throw new NotSupportedException("Cannot find parent objects");
		}
	}

	public void ClearFormats()
	{
		Type = OfficeErrorBarType.Fixed;
		Border.AutoFormat = true;
		HasCap = true;
		NumberValue = ((!m_bIsY) ? 1 : 10);
		m_include = OfficeErrorBarInclude.Both;
		m_plusRange = null;
		m_minusRange = null;
	}

	public void Delete()
	{
		if (m_bIsY)
		{
			m_serie.HasErrorBarsY = false;
		}
		else
		{
			m_serie.HasErrorBarsX = false;
		}
	}

	private void Parse(IList<BiffRecordRaw> data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		IRange range = null;
		int i = 0;
		for (int count = data.Count; i < count; i++)
		{
			BiffRecordRaw biffRecordRaw = data[i];
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.ChartSerAuxErrBar:
				m_errorBarRecord = (ChartSerAuxErrBarRecord)biffRecordRaw;
				break;
			case TBIFFRecord.ChartLineFormat:
				m_border = new ChartBorderImpl(base.Application, this, (ChartLineFormatRecord)biffRecordRaw);
				break;
			case TBIFFRecord.ChartAI:
			{
				ChartAIRecord chartAIRecord = (ChartAIRecord)biffRecordRaw;
				if (chartAIRecord.IndexIdentifier == ChartAIRecord.LinkIndex.LinkToValues)
				{
					m_chartAi = chartAIRecord;
					if (m_chartAi.Reference == ChartAIRecord.ReferenceType.Worksheet)
					{
						range = m_serie.GetRange(m_chartAi);
					}
				}
				break;
			}
			case TBIFFRecord.ChartMarkerFormat:
				m_markerFormat = (ChartMarkerFormatRecord)biffRecordRaw;
				break;
			}
		}
		ChartSerAuxErrBarRecord.TErrorBarValue errorBarValue = m_errorBarRecord.ErrorBarValue;
		bool flag = errorBarValue == ChartSerAuxErrBarRecord.TErrorBarValue.XDirectionPlus || errorBarValue == ChartSerAuxErrBarRecord.TErrorBarValue.YDirectionPlus;
		m_bIsY = errorBarValue == ChartSerAuxErrBarRecord.TErrorBarValue.YDirectionMinus || errorBarValue == ChartSerAuxErrBarRecord.TErrorBarValue.YDirectionPlus;
		m_include = (flag ? OfficeErrorBarInclude.Plus : OfficeErrorBarInclude.Minus);
		if (range != null)
		{
			if (flag)
			{
				m_plusRange = range;
			}
			else
			{
				m_minusRange = range;
			}
		}
	}

	public void Serialize(IList<IBiffStorage> records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_errorBarRecord.ValuesNumber == 0)
		{
			m_errorBarRecord.ValuesNumber = 1;
		}
		int index = m_serie.Index;
		if (m_include == OfficeErrorBarInclude.Both)
		{
			SerializeErrorBar(records, m_bIsY, bIsPlus: true, index);
			m_errorBarRecord = (ChartSerAuxErrBarRecord)m_errorBarRecord.Clone();
			SerializeErrorBar(records, m_bIsY, bIsPlus: false, index);
		}
		else
		{
			bool bIsPlus = m_include == OfficeErrorBarInclude.Plus;
			SerializeErrorBar(records, m_bIsY, bIsPlus, index);
		}
	}

	private void SerializeAiRecords(IList<IBiffStorage> records, bool bIsPlus)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		ChartAIRecord chartAIRecord = (ChartAIRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAI);
		chartAIRecord.IndexIdentifier = ChartAIRecord.LinkIndex.LinkToTitleOrText;
		chartAIRecord.Reference = ChartAIRecord.ReferenceType.EnteredDirectly;
		records.Add(chartAIRecord);
		chartAIRecord = (ChartAIRecord)chartAIRecord.Clone();
		chartAIRecord.IndexIdentifier = ChartAIRecord.LinkIndex.LinkToValues;
		records.Add(chartAIRecord);
		if (Type == OfficeErrorBarType.Custom && m_bIsY)
		{
			chartAIRecord.Reference = ChartAIRecord.ReferenceType.Worksheet;
			chartAIRecord.ParsedExpression = GetPtg(bIsPlus);
		}
		chartAIRecord = (ChartAIRecord)chartAIRecord.Clone();
		chartAIRecord.IndexIdentifier = ChartAIRecord.LinkIndex.LinkToCategories;
		if (Type == OfficeErrorBarType.Custom && !m_bIsY)
		{
			chartAIRecord.Reference = ChartAIRecord.ReferenceType.Worksheet;
			chartAIRecord.ParsedExpression = GetPtg(bIsPlus);
		}
		else
		{
			chartAIRecord.ParsedExpression = null;
			chartAIRecord.Reference = ChartAIRecord.ReferenceType.EnteredDirectly;
		}
		records.Add(chartAIRecord);
		chartAIRecord = (ChartAIRecord)chartAIRecord.Clone();
		chartAIRecord.IndexIdentifier = ChartAIRecord.LinkIndex.LinkToBubbles;
		chartAIRecord.ParsedExpression = null;
		chartAIRecord.Reference = ChartAIRecord.ReferenceType.EnteredDirectly;
		records.Add(chartAIRecord);
	}

	private int GetCount(IRange range)
	{
		if (range != null)
		{
			return (range as ICombinedRange).CellsCount;
		}
		if (m_chartAi != null && m_chartAi.ParsedExpression != null && m_chartAi.ParsedExpression.Length != 0)
		{
			return (m_chartAi.ParsedExpression[0] is IRectGetter) ? 1 : 0;
		}
		return 0;
	}

	private void SerializeErrorBar(IList<IBiffStorage> records, bool bIsYAxis, bool bIsPlus, int iSerieIndex)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		int count = GetCount(m_minusRange);
		int count2 = GetCount(m_plusRange);
		SerializeSerieRecord(records, bIsPlus ? count2 : count);
		BiffRecordRaw record = BiffRecordFactory.GetRecord(TBIFFRecord.Begin);
		records.Add(record);
		SerializeAiRecords(records, bIsPlus);
		ChartSeriesCollection parentSeries = m_serie.ParentSeries;
		SerializeDataFormatRecords(records, m_border, iSerieIndex, parentSeries.TrendErrorBarIndex, m_markerFormat);
		parentSeries.TrendErrorBarIndex++;
		ChartSerParentRecord chartSerParentRecord = (ChartSerParentRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSerParent);
		chartSerParentRecord.Series = (ushort)(iSerieIndex + 1);
		records.Add(chartSerParentRecord);
		if (bIsYAxis)
		{
			m_errorBarRecord.ErrorBarValue = (bIsPlus ? ChartSerAuxErrBarRecord.TErrorBarValue.YDirectionPlus : ChartSerAuxErrBarRecord.TErrorBarValue.YDirectionMinus);
		}
		else
		{
			m_errorBarRecord.ErrorBarValue = (bIsPlus ? ChartSerAuxErrBarRecord.TErrorBarValue.XDirectionPlus : ChartSerAuxErrBarRecord.TErrorBarValue.XDirectionMinus);
		}
		records.Add(m_errorBarRecord);
		record = BiffRecordFactory.GetRecord(TBIFFRecord.End);
		records.Add(record);
	}

	private Ptg[] GetPtg(bool bIsPlus)
	{
		if (m_bChanged)
		{
			IRange range = (bIsPlus ? m_plusRange : m_minusRange);
			return (range == null) ? null : ((INativePTG)range).GetNativePtg();
		}
		return m_chartAi.ParsedExpression;
	}

	public void MarkUsedReferences(bool[] usedItems)
	{
		if (m_chartAi != null)
		{
			FormulaUtil.MarkUsedReferences(m_chartAi.ParsedExpression, usedItems);
		}
	}

	public void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		if (m_chartAi != null)
		{
			Ptg[] parsedExpression = m_chartAi.ParsedExpression;
			if (FormulaUtil.UpdateReferenceIndexes(parsedExpression, arrUpdatedIndexes))
			{
				m_chartAi.ParsedExpression = parsedExpression;
			}
		}
	}

	private bool CheckInclude(OfficeErrorBarInclude value)
	{
		if (Type != OfficeErrorBarType.Custom)
		{
			return true;
		}
		switch (value)
		{
		case OfficeErrorBarInclude.Plus:
			m_minusRange = null;
			return m_plusRange != null;
		case OfficeErrorBarInclude.Minus:
			m_plusRange = null;
			return m_minusRange != null;
		default:
			if (m_minusRange != null)
			{
				return m_plusRange != null;
			}
			return false;
		}
	}

	public ChartErrorBarsImpl Clone(object parent, Dictionary<string, string> hashNewNames)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ChartErrorBarsImpl chartErrorBarsImpl = (ChartErrorBarsImpl)MemberwiseClone();
		chartErrorBarsImpl.SetParent(parent);
		chartErrorBarsImpl.FindParents();
		WorkbookImpl parentBook = chartErrorBarsImpl.m_serie.ParentBook;
		chartErrorBarsImpl.m_border = m_border.Clone(chartErrorBarsImpl);
		if (m_errorBarRecord != null)
		{
			chartErrorBarsImpl.m_errorBarRecord = (ChartSerAuxErrBarRecord)CloneUtils.CloneCloneable(m_errorBarRecord);
		}
		if (m_plusRange != null)
		{
			chartErrorBarsImpl.m_plusRange = ((ICombinedRange)m_plusRange).Clone(chartErrorBarsImpl, hashNewNames, parentBook);
		}
		if (m_minusRange != null)
		{
			chartErrorBarsImpl.m_minusRange = ((ICombinedRange)m_minusRange).Clone(chartErrorBarsImpl, hashNewNames, parentBook);
		}
		if (m_shadow != null)
		{
			chartErrorBarsImpl.m_shadow = m_shadow.Clone(chartErrorBarsImpl);
		}
		if (m_minusRangeValues != null)
		{
			chartErrorBarsImpl.m_minusRangeValues = CloneUtils.CloneArray(m_minusRangeValues);
		}
		if (m_plusRangeValues != null)
		{
			chartErrorBarsImpl.m_plusRangeValues = CloneUtils.CloneArray(m_plusRangeValues);
		}
		if (m_chartAi != null)
		{
			chartErrorBarsImpl.m_chartAi = (ChartAIRecord)m_chartAi.Clone();
		}
		chartErrorBarsImpl.m_include = m_include;
		if (m_markerFormat != null)
		{
			chartErrorBarsImpl.m_markerFormat = (ChartMarkerFormatRecord)CloneUtils.CloneCloneable(m_markerFormat);
		}
		return chartErrorBarsImpl;
	}
}
