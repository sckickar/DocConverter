using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartSerieImpl : CommonObject, IOfficeChartSerie, IParentApplication, ISerializableNamedObject, INamedObject, IReparse
{
	public const int DEF_FORMAT_ALLPOINTS_INDEX = 65535;

	private const int DEF_SURFACE_POINT_NUMBER = 65532;

	public const int DEF_CHART_GROUP = -1;

	private const string DEF_RADAR_START_TYPE = "Radar";

	public const string DEF_TRUE = "TRUE";

	public const string DEF_FALSE = "FALSE";

	private Dictionary<int, object> m_dataLabelCellsValues;

	private string m_serieName;

	private IRange m_ValueRange;

	private IRange m_CategoryRange;

	private IRange m_BubbleRange;

	private string m_strName;

	private bool m_isReversed;

	private Ptg[] m_nameTokens;

	private Dictionary<ChartAIRecord.LinkIndex, ChartAIRecord> m_hashAi = new Dictionary<ChartAIRecord.LinkIndex, ChartAIRecord>();

	private int m_iChartGroup;

	private WorkbookImpl m_book;

	private ChartSeriesRecord m_series;

	private ChartImpl m_chart;

	private ChartSeriesCollection m_seriesColl;

	private int m_iIndex;

	private int m_iOrder;

	private bool m_bDefaultName = true;

	private ChartDataPointsCollection m_dataPoints;

	private OfficeChartType m_serieType;

	private List<BiffRecordRaw> m_valueEnteredDirectly = new List<BiffRecordRaw>();

	private List<BiffRecordRaw> m_categoryEnteredDirectly = new List<BiffRecordRaw>();

	private List<BiffRecordRaw> m_bubbleEnteredDirectly = new List<BiffRecordRaw>();

	private object[] m_enteredDirectlyValue;

	private object[] m_enteredDirectlyCategory;

	private object[] m_enteredDirectlyBubble;

	private IRange m_nameRange;

	private string m_seriesText;

	private ChartErrorBarsImpl m_errorBarY;

	private ChartErrorBarsImpl m_errorBarX;

	private ChartTrendLineCollection m_trendLines;

	private bool? m_bInvertIfNegative;

	private string m_strRefFormula;

	private string m_numRefFormula;

	private string m_MulLvlStrRefFormula;

	private Stream m_dropLinesStream;

	private bool m_IsFiltered;

	private string m_categoryFilteredRange;

	private string m_categoryValue;

	private string m_grouping;

	private int m_gapWidth;

	private int m_overlap;

	private bool m_bShowGapWidth;

	private int[] m_serieIndexList;

	private int[] m_categoryIndexList;

	private string m_formatCodeForNum;

	internal Stream extentsStream;

	private bool _hasColumnShape;

	private ChartColor m_invertIfNegativeColor = ColorExtension.White;

	internal Stream m_invertFillFormatStream;

	private Dictionary<int, object[]> m_multiLevelStringCache;

	private Dictionary<int, string> m_formatValueCodes;

	private Dictionary<int, string> m_formatCategoryCodes;

	private int m_pointCount;

	private bool m_hasLeaderLines;

	private ChartBorderImpl m_leaderLines;

	private bool m_IsValidValueRange = true;

	private bool m_IsValidCategoryRange = true;

	private int m_existingOrder;

	private int m_paretoLineFormatIndex = -1;

	private bool m_isParetoLineHidden;

	private ChartFrameFormatImpl m_paretoLineFormat;

	private bool m_isRowWiseCategory;

	private bool m_isRowWiseSeries;

	private string m_formatCode;

	private string m_categoryFormatCode;

	public string SerieName
	{
		get
		{
			return m_serieName;
		}
		set
		{
			m_serieName = value;
		}
	}

	internal int ExistingOrder
	{
		get
		{
			return m_existingOrder;
		}
		set
		{
			m_existingOrder = value;
		}
	}

	internal bool Reversed
	{
		get
		{
			return m_isReversed;
		}
		set
		{
			m_isReversed = value;
		}
	}

	public string Name
	{
		get
		{
			return DetectSerieName();
		}
		set
		{
			if (!(m_strName != value))
			{
				return;
			}
			OnNameChanged(value);
			m_strName = value;
			m_bDefaultName = false;
			if (value != null && value.Length > 0 && value[0] == '=' && ChartImpl.TryAndModifyToValidFormula(m_strName))
			{
				m_nameTokens = GetNameTokens();
			}
			if (!m_chart.Loading)
			{
				SerieName = value;
			}
			if (!m_chart.Loading && m_chart.ChartTitle != null && (m_chart.ChartTitle == "Chart Title" || m_chart.ChartTitle.Length == 0))
			{
				ChartFormatImpl.GetStartSerieType(m_chart.ChartType);
				if (!m_bDefaultName && m_chart.HasAutoTitle == false && ParentSeries.Count == 1)
				{
					m_chart.ChartTitle = Name;
				}
			}
		}
	}

	public IRange NameRangeIRange
	{
		get
		{
			DetectSerieName();
			return m_nameRange;
		}
	}

	public IOfficeDataRange NameRange => new ChartDataRange(m_chart)
	{
		Range = NameRangeIRange
	};

	public IRange ValuesIRange
	{
		get
		{
			return m_ValueRange;
		}
		set
		{
			if (m_ValueRange != value)
			{
				m_valueEnteredDirectly.Clear();
				ValueChangedEventArgs e = new ValueChangedEventArgs(m_ValueRange, value, "ValueRange");
				m_ValueRange = value;
				UpdateChartExSerieRangesMembers(isValues: true);
				OnValueRangeChanged(e);
			}
		}
	}

	internal bool IsValidValueRange
	{
		get
		{
			return m_IsValidValueRange;
		}
		set
		{
			m_IsValidValueRange = value;
		}
	}

	public IOfficeDataRange Values
	{
		get
		{
			return new ChartDataRange(m_chart)
			{
				Range = ValuesIRange
			};
		}
		set
		{
			ChartDataRange chartDataRange = value as ChartDataRange;
			int firstRow = value.FirstRow;
			int firstColumn = value.FirstColumn;
			int lastRow = value.LastRow;
			int lastColumn = value.LastColumn;
			ValuesIRange = chartDataRange.SheetImpl[firstRow, firstColumn, lastRow, lastColumn];
		}
	}

	public IRange CategoryLabelsIRange
	{
		get
		{
			return m_CategoryRange;
		}
		set
		{
			if (m_CategoryRange == value)
			{
				return;
			}
			m_categoryEnteredDirectly.Clear();
			m_CategoryRange = value;
			if (!(m_CategoryRange is ExternalRange))
			{
				if (!InnerWorkbook.IsWorkbookOpening && !ParentChart.IsSeriesInRows && value != null && value.AddressLocal != null && value.Columns.Length > 1 && value.Rows.Length > 1)
				{
					m_MulLvlStrRefFormula = value.AddressGlobal;
					(m_CategoryRange as RangeImpl).IsMultiReference = true;
				}
				else if (!InnerWorkbook.IsWorkbookOpening && (m_MulLvlStrRefFormula != null || value == null))
				{
					m_MulLvlStrRefFormula = null;
				}
			}
			UpdateChartExSerieRangesMembers(isValues: false);
			OnCategoryRangeChanged();
		}
	}

	internal bool IsValidCategoryRange
	{
		get
		{
			return m_IsValidCategoryRange;
		}
		set
		{
			m_IsValidCategoryRange = value;
		}
	}

	public IOfficeDataRange CategoryLabels
	{
		get
		{
			return new ChartDataRange(m_chart)
			{
				Range = CategoryLabelsIRange
			};
		}
		set
		{
			ChartDataRange chartDataRange = value as ChartDataRange;
			if (chartDataRange.Range == null)
			{
				CategoryLabelsIRange = null;
				return;
			}
			int firstRow = value.FirstRow;
			int firstColumn = value.FirstColumn;
			int lastRow = value.LastRow;
			int lastColumn = value.LastColumn;
			CategoryLabelsIRange = chartDataRange.SheetImpl[firstRow, firstColumn, lastRow, lastColumn];
		}
	}

	public IRange BubblesIRange
	{
		get
		{
			return m_BubbleRange;
		}
		set
		{
			if (m_BubbleRange != value)
			{
				m_bubbleEnteredDirectly.Clear();
				m_BubbleRange = value;
				OnBubbleRangeChanged();
			}
		}
	}

	public IOfficeDataRange Bubbles
	{
		get
		{
			return new ChartDataRange(m_chart)
			{
				Range = BubblesIRange
			};
		}
		set
		{
			ChartDataRange chartDataRange = value as ChartDataRange;
			int firstRow = value.FirstRow;
			int firstColumn = value.FirstColumn;
			int lastRow = value.LastRow;
			int lastColumn = value.LastColumn;
			BubblesIRange = chartDataRange.SheetImpl[firstRow, firstColumn, lastRow, lastColumn];
		}
	}

	public int RealIndex
	{
		get
		{
			return Index;
		}
		set
		{
			Index = value;
		}
	}

	public IOfficeChartDataPoints DataPoints
	{
		get
		{
			if (m_dataPoints == null)
			{
				m_dataPoints = new ChartDataPointsCollection(base.Application, this);
			}
			return m_dataPoints;
		}
	}

	public IOfficeChartSerieDataFormat SerieFormat => m_dataPoints.DefaultDataPoint.DataFormat;

	public OfficeChartType SerieType
	{
		get
		{
			return DetectSerieType();
		}
		set
		{
			ChangeSeriesType(value, isSeriesCreation: false);
			(SerieFormat as ChartSerieDataFormatImpl).HasMarkerProperties = true;
		}
	}

	public bool UsePrimaryAxis
	{
		get
		{
			return GetCommonSerieFormat().IsPrimaryAxis;
		}
		set
		{
			if (Array.IndexOf(ChartImpl.DEF_CHANGE_SERIE, SerieType) == -1)
			{
				throw new NotSupportedException("Property not supported for current serie type");
			}
			if (value != UsePrimaryAxis)
			{
				ChangeAxis(value);
				m_chart.IsManuallyFormatted = true;
			}
			if (!value)
			{
				m_chart.SecondaryParentAxis.UpdateSecondaryAxis(bCreateAxis: true);
			}
			if (!m_seriesColl.HasSecondary())
			{
				m_chart.RemoveSecondaryAxes();
			}
		}
	}

	internal object[] EnteredDirectlyValues
	{
		get
		{
			if (m_enteredDirectlyValue == null)
			{
				m_enteredDirectlyValue = GetEnteredDirectlyValues(m_valueEnteredDirectly);
			}
			return m_enteredDirectlyValue;
		}
		set
		{
			m_enteredDirectlyValue = value;
		}
	}

	internal object[] EnteredDirectlyCategoryLabels
	{
		get
		{
			if (m_enteredDirectlyCategory == null)
			{
				m_enteredDirectlyCategory = GetEnteredDirectlyValues(m_categoryEnteredDirectly);
			}
			return m_enteredDirectlyCategory;
		}
		set
		{
			m_enteredDirectlyCategory = value;
		}
	}

	internal object[] EnteredDirectlyBubbles
	{
		get
		{
			if (m_enteredDirectlyBubble == null)
			{
				m_enteredDirectlyBubble = GetEnteredDirectlyValues(m_bubbleEnteredDirectly);
			}
			return m_enteredDirectlyBubble;
		}
		set
		{
			m_enteredDirectlyBubble = value;
		}
	}

	internal Dictionary<int, object> DataLabelCellsValues
	{
		get
		{
			return m_dataLabelCellsValues;
		}
		set
		{
			m_dataLabelCellsValues = value;
		}
	}

	internal bool HasColumnShape
	{
		get
		{
			return _hasColumnShape;
		}
		set
		{
			_hasColumnShape = value;
		}
	}

	public IOfficeChartErrorBars ErrorBarsY
	{
		get
		{
			if (m_errorBarY == null)
			{
				throw new ApplicationException("Use HasErrorBarsY property to create error bars.");
			}
			return m_errorBarY;
		}
	}

	public bool HasErrorBarsY
	{
		get
		{
			return m_errorBarY != null;
		}
		set
		{
			if (HasErrorBarsY == value)
			{
				return;
			}
			if (!value)
			{
				m_errorBarY = null;
				return;
			}
			string startSerieType = ChartFormatImpl.GetStartSerieType(SerieType);
			if (m_chart.IsChart3D || Array.IndexOf(ChartImpl.DEF_SUPPORT_ERROR_BARS, startSerieType) == -1)
			{
				throw new NotSupportedException("Current serie doesnot support Y error bars.");
			}
			if (m_errorBarY == null)
			{
				m_errorBarY = new ChartErrorBarsImpl(base.Application, this, bIsY: true);
			}
		}
	}

	public IOfficeChartErrorBars ErrorBarsX
	{
		get
		{
			if (m_errorBarX == null)
			{
				throw new ApplicationException("Use HasErrorBarsX property to create error bars.");
			}
			return m_errorBarX;
		}
	}

	public bool HasErrorBarsX
	{
		get
		{
			return m_errorBarX != null;
		}
		set
		{
			if (HasErrorBarsX == value)
			{
				return;
			}
			if (!value)
			{
				m_errorBarX = null;
				return;
			}
			if (ChartFormatImpl.GetStartSerieType(SerieType) != "Scatter" && ChartFormatImpl.GetStartSerieType(SerieType) != "Bubble")
			{
				throw new NotSupportedException("Current serie doesnot support X error bars.");
			}
			if (m_errorBarX == null)
			{
				m_errorBarX = new ChartErrorBarsImpl(base.Application, this, bIsY: false);
			}
		}
	}

	public IOfficeChartTrendLines TrendLines => m_trendLines;

	internal string Grouping
	{
		get
		{
			return m_grouping;
		}
		set
		{
			m_grouping = value;
		}
	}

	internal int GapWidth
	{
		get
		{
			return m_gapWidth;
		}
		set
		{
			m_gapWidth = value;
		}
	}

	internal int Overlap
	{
		get
		{
			return m_overlap;
		}
		set
		{
			m_overlap = value;
		}
	}

	internal bool ShowGapWidth
	{
		get
		{
			return m_bShowGapWidth;
		}
		set
		{
			m_bShowGapWidth = value;
		}
	}

	public string FormatCode
	{
		get
		{
			return m_formatCodeForNum;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				m_formatCodeForNum = value;
			}
		}
	}

	internal Dictionary<int, string> FormatValueCodes
	{
		get
		{
			if (m_formatValueCodes == null)
			{
				m_formatValueCodes = new Dictionary<int, string>();
			}
			return m_formatValueCodes;
		}
		set
		{
			m_formatValueCodes = value;
		}
	}

	internal Dictionary<int, string> FormatCategoryCodes
	{
		get
		{
			if (m_formatCategoryCodes == null)
			{
				m_formatCategoryCodes = new Dictionary<int, string>();
			}
			return m_formatCategoryCodes;
		}
		set
		{
			m_formatCategoryCodes = value;
		}
	}

	internal Dictionary<int, object[]> MultiLevelStrCache
	{
		get
		{
			if (m_multiLevelStringCache == null)
			{
				m_multiLevelStringCache = new Dictionary<int, object[]>();
			}
			return m_multiLevelStringCache;
		}
		set
		{
			m_multiLevelStringCache = value;
		}
	}

	internal int PointCount
	{
		get
		{
			return m_pointCount;
		}
		set
		{
			m_pointCount = value;
		}
	}

	public IOfficeChartFrameFormat ParetoLineFormat
	{
		get
		{
			return m_paretoLineFormat;
		}
		internal set
		{
			m_paretoLineFormat = value as ChartFrameFormatImpl;
		}
	}

	internal bool HasLeaderLines
	{
		get
		{
			return m_hasLeaderLines;
		}
		set
		{
			m_hasLeaderLines = value;
			if (ParentChart != null && ParentChart.IsChartPie)
			{
				DataPoints.DefaultDataPoint.DataLabels.ShowLeaderLines = value;
			}
		}
	}

	internal IOfficeChartBorder LeaderLines
	{
		get
		{
			if (m_leaderLines == null)
			{
				m_leaderLines = new ChartBorderImpl(base.Application, this);
			}
			m_leaderLines.HasLineProperties = true;
			return m_leaderLines;
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
			m_dataPoints.UpdateSerieIndex();
		}
	}

	public bool IsFiltered
	{
		get
		{
			return m_IsFiltered;
		}
		set
		{
			m_IsFiltered = value;
		}
	}

	public int Number
	{
		get
		{
			return m_iOrder;
		}
		set
		{
			m_iOrder = value;
		}
	}

	public int ChartGroup
	{
		get
		{
			return m_iChartGroup;
		}
		set
		{
			m_iChartGroup = value;
		}
	}

	public ChartImpl InnerChart => m_chart;

	public bool IsDefaultName
	{
		get
		{
			return m_bDefaultName;
		}
		set
		{
			m_bDefaultName = value;
		}
	}

	public int PointNumber
	{
		get
		{
			string startSerieType = ChartFormatImpl.GetStartSerieType(m_chart.ChartType);
			if ("Surface" == startSerieType)
			{
				return 65532;
			}
			if (m_ValueRange == null)
			{
				return 0;
			}
			return ((ICombinedRange)m_ValueRange).CellsCount;
		}
	}

	public WorkbookImpl InnerWorkbook => m_book;

	public string FilteredCategory
	{
		get
		{
			return m_categoryFilteredRange;
		}
		set
		{
			m_categoryFilteredRange = value;
		}
	}

	public string FilteredValue
	{
		get
		{
			return m_categoryValue;
		}
		set
		{
			m_categoryValue = value;
		}
	}

	public string StartType => DetectSerieTypeStart();

	public string ParseSerieNotDefaultText => m_seriesText;

	public ChartSeriesCollection ParentSeries => m_seriesColl;

	public ChartImpl ParentChart => m_chart;

	internal WorkbookImpl ParentBook => m_book;

	public bool IsPie => ChartImpl.GetIsChartPie(SerieType);

	public string NameOrFormula
	{
		get
		{
			if (m_nameRange != null && m_strName.Substring(1) != m_nameRange.AddressGlobal)
			{
				m_strName = "=" + m_nameRange.AddressGlobal;
			}
			if (m_strName == null || m_strName.Length <= 0)
			{
				return m_strName;
			}
			if (m_strName[0] != '=' || m_nameTokens == null)
			{
				return m_strName;
			}
			return "=" + m_book.FormulaUtil.ParsePtgArray(m_nameTokens);
		}
	}

	public bool InvertIfNegative
	{
		get
		{
			if (!m_bInvertIfNegative.HasValue)
			{
				return true;
			}
			return m_bInvertIfNegative.Value;
		}
		set
		{
			m_bInvertIfNegative = value;
		}
	}

	internal bool? InvertNegative => m_bInvertIfNegative;

	public string StrRefFormula
	{
		get
		{
			return m_strRefFormula;
		}
		set
		{
			m_strRefFormula = value;
		}
	}

	internal string NumRefFormula
	{
		get
		{
			return m_numRefFormula;
		}
		set
		{
			m_numRefFormula = value;
		}
	}

	internal string MulLvlStrRefFormula
	{
		get
		{
			return m_MulLvlStrRefFormula;
		}
		set
		{
			m_MulLvlStrRefFormula = value;
		}
	}

	public ChartColor InvertIfNegativeColor
	{
		get
		{
			if (m_invertIfNegativeColor == ColorExtension.White && m_invertFillFormatStream != null)
			{
				m_invertIfNegativeColor = ChartParser.ParseInvertSolidFillFormat(m_invertFillFormatStream, this);
			}
			return m_invertIfNegativeColor;
		}
		set
		{
			if (SerieFormat.Fill.FillType == OfficeFillType.SolidColor && (SerieType == OfficeChartType.Bar_Clustered || SerieType == OfficeChartType.Bar_Clustered_3D || SerieType == OfficeChartType.Bar_Stacked || SerieType == OfficeChartType.Bar_Stacked_100 || SerieType == OfficeChartType.Bar_Stacked_100_3D || SerieType == OfficeChartType.Bar_Stacked_3D || SerieType == OfficeChartType.Column_3D || SerieType == OfficeChartType.Column_Clustered || SerieType == OfficeChartType.Column_Clustered_3D || SerieType == OfficeChartType.Column_Stacked || SerieType == OfficeChartType.Column_Stacked_100 || SerieType == OfficeChartType.Column_Stacked_100_3D || SerieType == OfficeChartType.Column_Stacked_3D))
			{
				m_invertIfNegativeColor = value;
				return;
			}
			throw new ArgumentException("Property not supported for current serie type");
		}
	}

	internal Stream DropLinesStream
	{
		get
		{
			return m_dropLinesStream;
		}
		set
		{
			m_dropLinesStream = value;
		}
	}

	internal bool IsParetoLineHidden
	{
		get
		{
			return m_isParetoLineHidden;
		}
		set
		{
			m_isParetoLineHidden = value;
		}
	}

	internal bool IsSeriesHidden
	{
		get
		{
			return m_IsFiltered;
		}
		set
		{
			m_IsFiltered = value;
		}
	}

	internal int ParetoLineFormatIndex
	{
		get
		{
			return m_paretoLineFormatIndex;
		}
		set
		{
			m_paretoLineFormatIndex = value;
		}
	}

	internal bool IsRowWiseCategory
	{
		get
		{
			return m_isRowWiseCategory;
		}
		set
		{
			m_isRowWiseCategory = value;
		}
	}

	internal string CategoriesFormatCode
	{
		get
		{
			return m_categoryFormatCode;
		}
		set
		{
			m_categoryFormatCode = value;
		}
	}

	internal bool IsRowWiseSeries
	{
		get
		{
			return m_isRowWiseSeries;
		}
		set
		{
			m_isRowWiseSeries = value;
		}
	}

	public event ValueChangedEventHandler ValueRangeChanged;

	public event ValueChangedEventHandler NameChanged;

	public ChartSerieImpl(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
		InitializeCollections();
	}

	[CLSCompliant(false)]
	public ChartSerieImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: this(application, parent)
	{
		Parse(data, ref iPos);
	}

	public IOfficeChartErrorBars ErrorBar(bool bIsY)
	{
		return ErrorBar(bIsY, OfficeErrorBarInclude.Both);
	}

	public IOfficeChartErrorBars ErrorBar(bool bIsY, OfficeErrorBarInclude include)
	{
		return ErrorBar(bIsY, include, OfficeErrorBarType.Fixed);
	}

	public IOfficeChartErrorBars ErrorBar(bool bIsY, OfficeErrorBarInclude include, OfficeErrorBarType type)
	{
		double numberValue = ((!bIsY) ? 1 : 10);
		return ErrorBar(bIsY, include, type, numberValue);
	}

	public IOfficeChartErrorBars ErrorBar(bool bIsY, OfficeErrorBarInclude include, OfficeErrorBarType type, double numberValue)
	{
		if (type == OfficeErrorBarType.Custom)
		{
			throw new ArgumentException("For sets custom type use another overload method");
		}
		ChartErrorBarsImpl chartErrorBarsImpl = null;
		if (bIsY)
		{
			HasErrorBarsY = true;
			chartErrorBarsImpl = m_errorBarY;
		}
		else
		{
			HasErrorBarsX = true;
			chartErrorBarsImpl = m_errorBarX;
		}
		chartErrorBarsImpl.Type = type;
		chartErrorBarsImpl.Include = include;
		chartErrorBarsImpl.NumberValue = numberValue;
		chartErrorBarsImpl.Border.AutoFormat = true;
		chartErrorBarsImpl.HasCap = true;
		return chartErrorBarsImpl;
	}

	public IOfficeChartErrorBars ErrorBar(bool bIsY, IOfficeDataRange plusRange, IOfficeDataRange minusRange)
	{
		bool num = plusRange != null;
		bool flag = minusRange != null;
		if (!num && !flag)
		{
			throw new ArgumentException("Plus range and minus range are null referance.");
		}
		ChartErrorBarsImpl chartErrorBarsImpl = null;
		if (bIsY)
		{
			HasErrorBarsY = true;
			chartErrorBarsImpl = m_errorBarY;
			chartErrorBarsImpl.NumberValue = 10.0;
		}
		else
		{
			HasErrorBarsX = true;
			chartErrorBarsImpl = m_errorBarX;
			chartErrorBarsImpl.NumberValue = 1.0;
		}
		if (num)
		{
			chartErrorBarsImpl.PlusIRange = (plusRange as ChartDataRange).Range;
			List<object> list = new List<object>();
			object[] array = null;
			for (int i = 0; i < (plusRange as ChartDataRange).Range.Count; i++)
			{
				list.Add(double.Parse((plusRange as ChartDataRange).Range.Cells[i].DisplayText));
			}
			array = list.ToArray();
			chartErrorBarsImpl.PlusRangeValues = array;
		}
		if (flag)
		{
			chartErrorBarsImpl.MinusIRange = (minusRange as ChartDataRange).Range;
			List<object> list2 = new List<object>();
			object[] array2 = null;
			for (int j = 0; j < (minusRange as ChartDataRange).Range.Count; j++)
			{
				list2.Add(double.Parse((minusRange as ChartDataRange).Range.Cells[j].DisplayText));
			}
			array2 = list2.ToArray();
			chartErrorBarsImpl.MinusRangeValues = array2;
		}
		chartErrorBarsImpl.Border.AutoFormat = true;
		chartErrorBarsImpl.HasCap = true;
		return chartErrorBarsImpl;
	}

	private void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartSeries);
		ParseSeriesRecord((ChartSeriesRecord)data[iPos]);
		iPos++;
		biffRecordRaw = data[iPos];
		if (biffRecordRaw.TypeCode != TBIFFRecord.Begin)
		{
			throw new ArgumentOutOfRangeException("Begin record was expected.");
		}
		iPos++;
		m_hashAi.Clear();
		m_dataPoints.Clear();
		biffRecordRaw = data[iPos];
		while (biffRecordRaw.TypeCode != TBIFFRecord.End)
		{
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.ChartAI:
				ParseAIRecord(data, ref iPos);
				break;
			case TBIFFRecord.ChartDataFormat:
			{
				m_iOrder = ((ChartDataFormatRecord)biffRecordRaw).SeriesNumber;
				ChartSerieDataFormatImpl chartSerieDataFormatImpl = new ChartSerieDataFormatImpl(base.Application, this);
				iPos = chartSerieDataFormatImpl.Parse(data, iPos);
				_ = chartSerieDataFormatImpl.DataFormat.PointNumber;
				SetDataFormat(chartSerieDataFormatImpl);
				break;
			}
			case TBIFFRecord.ChartSertocrt:
				ParseSertoCrt(data, ref iPos);
				break;
			case TBIFFRecord.ChartLegendxn:
				ParseLegendEntries(data, ref iPos);
				break;
			case TBIFFRecord.ChartSeriesText:
				if (m_seriesColl.Count == 0)
				{
					m_seriesText = ((ChartSeriesTextRecord)biffRecordRaw).Text;
				}
				iPos++;
				break;
			default:
				iPos++;
				break;
			}
			biffRecordRaw = data[iPos];
		}
		m_seriesColl.TrendIndex++;
		iPos++;
		Reparse();
	}

	private void ParseSeriesRecord(ChartSeriesRecord series)
	{
		m_series = series;
	}

	private void ParseAIRecord(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		if (biffRecordRaw.TypeCode != TBIFFRecord.ChartAI)
		{
			throw new ArgumentOutOfRangeException("ChartAI record was expected.");
		}
		ChartAIRecord chartAIRecord = (ChartAIRecord)biffRecordRaw;
		iPos++;
		if (m_hashAi.ContainsKey(chartAIRecord.IndexIdentifier))
		{
			throw new ArgumentException("AI record with such IndexIdentifier was already read.");
		}
		if (chartAIRecord.IndexIdentifier == ChartAIRecord.LinkIndex.LinkToTitleOrText)
		{
			GetTitle(chartAIRecord, data, ref iPos);
		}
		m_hashAi.Add(chartAIRecord.IndexIdentifier, chartAIRecord);
	}

	private void ParseSertoCrt(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartSertocrt);
		m_iChartGroup = ((ChartSertocrtRecord)biffRecordRaw).ChartGroup;
		iPos++;
	}

	private void GetTitle(ChartAIRecord recordAi, IList<BiffRecordRaw> data, ref int iPos)
	{
		if (recordAi.Reference == ChartAIRecord.ReferenceType.EnteredDirectly || recordAi.Reference == ChartAIRecord.ReferenceType.NotUsed)
		{
			BiffRecordRaw biffRecordRaw = data[iPos];
			if (biffRecordRaw.TypeCode == TBIFFRecord.ChartSeriesText)
			{
				m_strName = ((ChartSeriesTextRecord)biffRecordRaw).Text;
				m_seriesText = m_strName;
				m_bDefaultName = false;
				iPos++;
			}
		}
		if (recordAi.Reference == ChartAIRecord.ReferenceType.Worksheet)
		{
			Ptg[] parsedExpression = recordAi.ParsedExpression;
			m_strName = "=" + m_book.FormulaUtil.ParsePtgArray(parsedExpression);
			m_nameTokens = parsedExpression;
			m_bDefaultName = false;
		}
	}

	private void ParseLegendEntries(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		m_chart.HasLegend = true;
		ChartLegendEntriesColl chartLegendEntriesColl = (ChartLegendEntriesColl)m_chart.Legend.LegendEntries;
		ChartLegendEntryImpl chartLegendEntryImpl = new ChartLegendEntryImpl(base.Application, chartLegendEntriesColl, 0);
		chartLegendEntryImpl.Parse(data, ref iPos);
		int iIndex = ((chartLegendEntryImpl.LegendEntityIndex == 65535) ? m_seriesColl.Count : chartLegendEntryImpl.LegendEntityIndex);
		chartLegendEntriesColl.Add(iIndex, chartLegendEntryImpl);
	}

	public void ParseErrorBars(IList<BiffRecordRaw> data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		ChartErrorBarsImpl chartErrorBarsImpl = new ChartErrorBarsImpl(base.Application, this, data);
		if (chartErrorBarsImpl.IsY)
		{
			UpdateErrorBar(chartErrorBarsImpl, ref m_errorBarY);
		}
		else
		{
			UpdateErrorBar(chartErrorBarsImpl, ref m_errorBarX);
		}
	}

	internal void SetLeaderLines(bool value)
	{
		m_hasLeaderLines = value;
	}

	private void SetParents()
	{
		object obj = FindParent(typeof(WorkbookImpl));
		if (obj == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
		m_book = (WorkbookImpl)obj;
		obj = FindParent(typeof(ChartImpl));
		if (obj == null)
		{
			throw new ArgumentNullException("Can't find parent chart.");
		}
		m_chart = (ChartImpl)obj;
		obj = FindParent(typeof(ChartSeriesCollection));
		if (obj == null)
		{
			throw new ArgumentNullException("Can't find parent series collection.");
		}
		m_seriesColl = (ChartSeriesCollection)obj;
	}

	private void InitializeHashAIMember()
	{
		ChartAIRecord chartAIRecord = (ChartAIRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAI);
		chartAIRecord.IndexIdentifier = ChartAIRecord.LinkIndex.LinkToTitleOrText;
		chartAIRecord.Reference = ChartAIRecord.ReferenceType.NotUsed;
		m_hashAi.Add(chartAIRecord.IndexIdentifier, chartAIRecord);
		chartAIRecord = (ChartAIRecord)chartAIRecord.Clone();
		chartAIRecord.IndexIdentifier = ChartAIRecord.LinkIndex.LinkToCategories;
		chartAIRecord.Reference = ChartAIRecord.ReferenceType.DefaultCategories;
		m_hashAi.Add(chartAIRecord.IndexIdentifier, chartAIRecord);
		chartAIRecord = (ChartAIRecord)chartAIRecord.Clone();
		chartAIRecord.IndexIdentifier = ChartAIRecord.LinkIndex.LinkToValues;
		chartAIRecord.Reference = ChartAIRecord.ReferenceType.NotUsed;
		m_hashAi.Add(chartAIRecord.IndexIdentifier, chartAIRecord);
		chartAIRecord = (ChartAIRecord)chartAIRecord.Clone();
		chartAIRecord.IndexIdentifier = ChartAIRecord.LinkIndex.LinkToBubbles;
		chartAIRecord.Reference = ChartAIRecord.ReferenceType.NotUsed;
		m_hashAi.Add(chartAIRecord.IndexIdentifier, chartAIRecord);
	}

	private void InitializeCollections()
	{
		InitializeHashAIMember();
		m_series = (ChartSeriesRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSeries);
		ChartSerieDataFormatImpl chartSerieDataFormatImpl = new ChartSerieDataFormatImpl(base.Application, this);
		chartSerieDataFormatImpl.DataFormat.PointNumber = ushort.MaxValue;
		SetDataFormat(chartSerieDataFormatImpl);
		if (!ParentBook.IsLoaded)
		{
			switch (m_chart.ChartType)
			{
			case OfficeChartType.TreeMap:
			case OfficeChartType.SunBurst:
				DataPoints.DefaultDataPoint.DataLabels.IsCategoryName = true;
				DataPoints.DefaultDataPoint.DataLabels.Delimiter = ",";
				DataPoints.DefaultDataPoint.DataLabels.Position = OfficeDataLabelPosition.Inside;
				break;
			case OfficeChartType.Pareto:
				m_paretoLineFormat = new ChartFrameFormatImpl(base.Application, this);
				break;
			}
		}
		m_trendLines = new ChartTrendLineCollection(base.Application, this);
	}

	private void SetDataFormat(ChartSerieDataFormatImpl dataFormat)
	{
		if (dataFormat == null)
		{
			throw new ArgumentNullException("dataFormat");
		}
		int pointNumber = dataFormat.DataFormat.PointNumber;
		ChartDataPointImpl chartDataPointImpl = (ChartDataPointImpl)DataPoints[pointNumber];
		chartDataPointImpl.InnerDataFormat = dataFormat;
		dataFormat.SetParent(chartDataPointImpl);
	}

	[CLSCompliant(false)]
	public Chart3DDataFormatRecord Get3DDataFormat()
	{
		Chart3DDataFormatRecord chart3DDataFormatRecord = (Chart3DDataFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3DDataFormat);
		switch (m_chart.ChartType)
		{
		case OfficeChartType.Cylinder_Clustered:
		case OfficeChartType.Cylinder_Stacked:
		case OfficeChartType.Cylinder_Stacked_100:
		case OfficeChartType.Cylinder_Bar_Clustered:
		case OfficeChartType.Cylinder_Bar_Stacked:
		case OfficeChartType.Cylinder_Bar_Stacked_100:
		case OfficeChartType.Cylinder_Clustered_3D:
			chart3DDataFormatRecord.DataFormatBase = OfficeBaseFormat.Circle;
			chart3DDataFormatRecord.DataFormatTop = OfficeTopFormat.Straight;
			break;
		case OfficeChartType.Cone_Stacked_100:
		case OfficeChartType.Cone_Bar_Stacked_100:
			chart3DDataFormatRecord.DataFormatBase = OfficeBaseFormat.Circle;
			chart3DDataFormatRecord.DataFormatTop = OfficeTopFormat.Trunc;
			break;
		case OfficeChartType.Cone_Clustered:
		case OfficeChartType.Cone_Stacked:
		case OfficeChartType.Cone_Bar_Clustered:
		case OfficeChartType.Cone_Bar_Stacked:
		case OfficeChartType.Cone_Clustered_3D:
			chart3DDataFormatRecord.DataFormatBase = OfficeBaseFormat.Circle;
			chart3DDataFormatRecord.DataFormatTop = OfficeTopFormat.Sharp;
			break;
		case OfficeChartType.Pyramid_Stacked_100:
		case OfficeChartType.Pyramid_Bar_Stacked_100:
			chart3DDataFormatRecord.DataFormatBase = OfficeBaseFormat.Rectangle;
			chart3DDataFormatRecord.DataFormatTop = OfficeTopFormat.Trunc;
			break;
		case OfficeChartType.Pyramid_Clustered:
		case OfficeChartType.Pyramid_Stacked:
		case OfficeChartType.Pyramid_Bar_Clustered:
		case OfficeChartType.Pyramid_Bar_Stacked:
		case OfficeChartType.Pyramid_Clustered_3D:
			chart3DDataFormatRecord.DataFormatBase = OfficeBaseFormat.Rectangle;
			chart3DDataFormatRecord.DataFormatTop = OfficeTopFormat.Sharp;
			break;
		}
		return chart3DDataFormatRecord;
	}

	private void OnNameChanged(string value)
	{
		if (value != null && value.Length > 0 && value[0] == '=')
		{
			value = value.Substring(1);
			try
			{
				IRangeGetter rangeGetter = (IRangeGetter)m_book.FormulaUtil.ParseString(value)[0];
				m_nameRange = rangeGetter.GetRange(m_book, null);
			}
			catch
			{
			}
			if (m_nameRange != null && m_nameRange.Row != m_nameRange.LastRow && m_nameRange.Column != m_nameRange.LastColumn)
			{
				throw new NotSupportedException("Reference must be a single cell, row, or column.");
			}
		}
		else if (m_nameRange != null && !(m_nameRange is ExternalRange))
		{
			m_nameRange.Value2 = value;
		}
	}

	private void OnValueRangeChanged(ValueChangedEventArgs e)
	{
		SetAIRange(m_hashAi[ChartAIRecord.LinkIndex.LinkToValues], m_ValueRange, ChartAIRecord.ReferenceType.EnteredDirectly);
		if (this.ValueRangeChanged != null)
		{
			this.ValueRangeChanged(this, e);
		}
	}

	private void OnCategoryRangeChanged()
	{
		SetAIRange(m_hashAi[ChartAIRecord.LinkIndex.LinkToCategories], m_CategoryRange, ChartAIRecord.ReferenceType.DefaultCategories);
	}

	private void OnBubbleRangeChanged()
	{
		SetAIRange(m_hashAi[ChartAIRecord.LinkIndex.LinkToBubbles], m_BubbleRange, ChartAIRecord.ReferenceType.DefaultCategories);
	}

	public ChartSerieImpl Clone(object parent, Dictionary<string, string> hashNewNames, Dictionary<int, int> dicFontIndexes)
	{
		ChartSerieImpl chartSerieImpl = new ChartSerieImpl(base.Application, parent);
		chartSerieImpl.m_bDefaultName = m_bDefaultName;
		chartSerieImpl.m_bIsDisposed = m_bIsDisposed;
		chartSerieImpl.m_hashAi = new Dictionary<ChartAIRecord.LinkIndex, ChartAIRecord>();
		chartSerieImpl.InitializeHashAIMember();
		IRange serieNameRange = GetSerieNameRange();
		if (serieNameRange != null)
		{
			chartSerieImpl.m_nameRange = ((ICombinedRange)m_nameRange).Clone(chartSerieImpl, hashNewNames, chartSerieImpl.m_book);
		}
		if (m_ValueRange != null)
		{
			serieNameRange = ((ICombinedRange)m_ValueRange).Clone(chartSerieImpl, hashNewNames, chartSerieImpl.m_book);
			chartSerieImpl.ValuesIRange = serieNameRange;
			if (m_ValueRange is RangeImpl)
			{
				(chartSerieImpl.ValuesIRange as RangeImpl).IsMultiReference = (m_ValueRange as RangeImpl).IsMultiReference;
				(chartSerieImpl.ValuesIRange as RangeImpl).IsNumReference = (m_ValueRange as RangeImpl).IsNumReference;
				(chartSerieImpl.ValuesIRange as RangeImpl).IsStringReference = (m_ValueRange as RangeImpl).IsStringReference;
			}
			else if (m_ValueRange is NameImpl)
			{
				(chartSerieImpl.ValuesIRange as NameImpl).IsMultiReference = (m_ValueRange as NameImpl).IsMultiReference;
				(chartSerieImpl.ValuesIRange as NameImpl).IsNumReference = (m_ValueRange as NameImpl).IsNumReference;
				(chartSerieImpl.ValuesIRange as NameImpl).IsStringReference = (m_ValueRange as NameImpl).IsStringReference;
			}
			else if (m_ValueRange is ExternalRange)
			{
				(chartSerieImpl.ValuesIRange as ExternalRange).IsMultiReference = (m_ValueRange as ExternalRange).IsMultiReference;
				(chartSerieImpl.ValuesIRange as ExternalRange).IsNumReference = (m_ValueRange as ExternalRange).IsNumReference;
				(chartSerieImpl.ValuesIRange as ExternalRange).IsStringReference = (m_ValueRange as ExternalRange).IsStringReference;
			}
		}
		if (m_BubbleRange != null)
		{
			chartSerieImpl.BubblesIRange = ((ICombinedRange)m_BubbleRange).Clone(chartSerieImpl, hashNewNames, chartSerieImpl.m_book);
		}
		if (m_CategoryRange != null)
		{
			chartSerieImpl.CategoryLabelsIRange = ((ICombinedRange)m_CategoryRange).Clone(chartSerieImpl, hashNewNames, chartSerieImpl.m_book);
			if (m_CategoryRange is RangeImpl)
			{
				(chartSerieImpl.CategoryLabelsIRange as RangeImpl).IsMultiReference = (m_CategoryRange as RangeImpl).IsMultiReference;
				(chartSerieImpl.CategoryLabelsIRange as RangeImpl).IsNumReference = (m_CategoryRange as RangeImpl).IsNumReference;
				(chartSerieImpl.CategoryLabelsIRange as RangeImpl).IsStringReference = (m_CategoryRange as RangeImpl).IsStringReference;
			}
			else if (m_CategoryRange is NameImpl)
			{
				(chartSerieImpl.CategoryLabelsIRange as NameImpl).IsMultiReference = (m_CategoryRange as NameImpl).IsMultiReference;
				(chartSerieImpl.CategoryLabelsIRange as NameImpl).IsNumReference = (m_CategoryRange as NameImpl).IsNumReference;
				(chartSerieImpl.CategoryLabelsIRange as NameImpl).IsStringReference = (m_CategoryRange as NameImpl).IsStringReference;
			}
			else if (m_CategoryRange is ExternalRange)
			{
				(chartSerieImpl.CategoryLabelsIRange as ExternalRange).IsMultiReference = (m_CategoryRange as ExternalRange).IsMultiReference;
				(chartSerieImpl.CategoryLabelsIRange as ExternalRange).IsNumReference = (m_CategoryRange as ExternalRange).IsNumReference;
				(chartSerieImpl.CategoryLabelsIRange as ExternalRange).IsStringReference = (m_CategoryRange as ExternalRange).IsStringReference;
			}
		}
		if (m_nameRange != null)
		{
			chartSerieImpl.m_nameRange = ((ICombinedRange)m_nameRange).Clone(chartSerieImpl, hashNewNames, chartSerieImpl.m_book);
		}
		if (m_dataPoints != null)
		{
			chartSerieImpl.m_dataPoints = (ChartDataPointsCollection)m_dataPoints.Clone(chartSerieImpl, chartSerieImpl.m_book, dicFontIndexes, hashNewNames);
		}
		chartSerieImpl.m_valueEnteredDirectly = CloneUtils.CloneCloneable(m_valueEnteredDirectly);
		chartSerieImpl.m_categoryEnteredDirectly = CloneUtils.CloneCloneable(m_categoryEnteredDirectly);
		chartSerieImpl.m_bubbleEnteredDirectly = CloneUtils.CloneCloneable(m_bubbleEnteredDirectly);
		chartSerieImpl.m_enteredDirectlyValue = CloneUtils.CloneArray(m_enteredDirectlyValue);
		chartSerieImpl.m_enteredDirectlyCategory = CloneUtils.CloneArray(m_enteredDirectlyCategory);
		chartSerieImpl.m_enteredDirectlyBubble = CloneUtils.CloneArray(m_enteredDirectlyBubble);
		chartSerieImpl.m_formatCodeForNum = m_formatCodeForNum;
		chartSerieImpl.NumRefFormula = m_numRefFormula;
		chartSerieImpl.m_bInvertIfNegative = m_bInvertIfNegative;
		chartSerieImpl.m_iChartGroup = m_iChartGroup;
		chartSerieImpl.m_iIndex = m_iIndex;
		chartSerieImpl.m_iOrder = m_iOrder;
		chartSerieImpl.m_existingOrder = m_existingOrder;
		chartSerieImpl.m_series = (ChartSeriesRecord)m_series.Clone();
		chartSerieImpl.m_strName = m_strName;
		chartSerieImpl.m_trendLines = m_trendLines.Clone(chartSerieImpl, dicFontIndexes, hashNewNames);
		chartSerieImpl.m_serieName = m_serieName;
		chartSerieImpl.m_overlap = m_overlap;
		chartSerieImpl.m_gapWidth = m_gapWidth;
		if (m_errorBarX != null)
		{
			chartSerieImpl.m_errorBarX = m_errorBarX.Clone(chartSerieImpl, hashNewNames);
		}
		if (m_leaderLines != null)
		{
			chartSerieImpl.m_leaderLines = m_leaderLines.Clone(chartSerieImpl);
		}
		if (m_errorBarY != null)
		{
			chartSerieImpl.m_errorBarY = m_errorBarY.Clone(chartSerieImpl, hashNewNames);
		}
		if (m_nameTokens != null)
		{
			chartSerieImpl.m_nameTokens = CloneUtils.ClonePtgArray(m_nameTokens);
		}
		if (extentsStream != null)
		{
			extentsStream.Position = 0L;
			chartSerieImpl.extentsStream = CloneUtils.CloneStream(extentsStream);
		}
		if (m_invertFillFormatStream != null)
		{
			m_invertFillFormatStream.Position = 0L;
			chartSerieImpl.m_invertFillFormatStream = CloneUtils.CloneStream(m_invertFillFormatStream);
		}
		if (m_dropLinesStream != null)
		{
			m_dropLinesStream.Position = 0L;
			chartSerieImpl.m_dropLinesStream = CloneUtils.CloneStream(m_dropLinesStream);
		}
		if (m_multiLevelStringCache != null)
		{
			chartSerieImpl.m_multiLevelStringCache = CloneUtils.CloneHash(m_multiLevelStringCache);
		}
		if (m_invertIfNegativeColor != null)
		{
			chartSerieImpl.m_invertIfNegativeColor = m_invertIfNegativeColor.Clone();
		}
		chartSerieImpl.m_serieType = m_serieType;
		chartSerieImpl.m_hasLeaderLines = m_hasLeaderLines;
		chartSerieImpl.m_pointCount = m_pointCount;
		chartSerieImpl.m_IsValidCategoryRange = m_IsValidCategoryRange;
		chartSerieImpl.m_IsValidValueRange = m_IsValidValueRange;
		chartSerieImpl.m_IsFiltered = m_IsFiltered;
		chartSerieImpl.m_MulLvlStrRefFormula = m_MulLvlStrRefFormula;
		return chartSerieImpl;
	}

	private string GetWorkSheetNameByAddress(string strAddress)
	{
		if (strAddress == null)
		{
			throw new ArgumentNullException("strAddress");
		}
		int num = strAddress.IndexOf("'!");
		return strAddress.Substring(1, num - 1);
	}

	private void ChangeAxis(bool bToPrimary)
	{
		if (!bToPrimary && m_seriesColl.Count == 1)
		{
			throw new ArgumentException("Can't set current serie to secondary axis");
		}
		int chartGroup = ChartGroup;
		ChartFormatImpl commonSerieFormat = GetCommonSerieFormat();
		int newOrder = GetNewOrder(bToPrimary);
		int countOfSeriesWithSameDrawingOrder = m_seriesColl.GetCountOfSeriesWithSameDrawingOrder(chartGroup);
		int countOfSeriesWithSameStartType = m_seriesColl.GetCountOfSeriesWithSameStartType(SerieType);
		if (newOrder < 0)
		{
			throw new ApplicationException("Can't set current serie to secondary axis");
		}
		if (countOfSeriesWithSameDrawingOrder != countOfSeriesWithSameStartType)
		{
			OfficeChartType serieType = SerieType;
			ChartFormatCollection chartFormatCollection = (bToPrimary ? m_chart.PrimaryFormats : m_chart.SecondaryFormats);
			ChartFormatImpl formatToAdd = (ChartFormatImpl)commonSerieFormat.Clone(chartFormatCollection);
			formatToAdd = chartFormatCollection.FindOrAdd(formatToAdd);
			ChartGroup = formatToAdd.DrawingZOrder;
			if (countOfSeriesWithSameDrawingOrder == 1)
			{
				m_chart.PrimaryParentAxis.Formats.Remove(commonSerieFormat);
			}
			if (Array.IndexOf(ChartImpl.DEF_CHANGE_INTIMATE, serieType) != -1)
			{
				SerieType = serieType;
			}
			else if (m_chart.ChartType == OfficeChartType.Combination_Chart)
			{
				ChangeSeriesType(serieType, isSeriesCreation: false, forceChange: true);
			}
		}
		else
		{
			bool flag = countOfSeriesWithSameDrawingOrder != 1;
			m_chart.PrimaryParentAxis.Formats.ChangeShallowAxis(bToPrimary, chartGroup, flag, newOrder);
			if (flag)
			{
				ChartGroup = newOrder;
			}
		}
	}

	private int GetNewOrder(bool bToPrimary)
	{
		ChartGlobalFormatsCollection formats = m_chart.PrimaryParentAxis.Formats;
		ChartFormatCollection primaryFormats = formats.PrimaryFormats;
		ChartFormatCollection secondaryFormats = formats.SecondaryFormats;
		int result = -1;
		for (int i = 0; i < 8; i++)
		{
			if (!primaryFormats.ContainsIndex(i) && !secondaryFormats.ContainsIndex(i))
			{
				result = i;
				break;
			}
		}
		return result;
	}

	[CLSCompliant(false)]
	public void AddEnteredRecord(int siIndex, ICellPositionFormat record)
	{
		if (siIndex > 3 || siIndex < 1)
		{
			throw new ArgumentOutOfRangeException("siIndex");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		switch (siIndex)
		{
		case 1:
			if (m_ValueRange == null)
			{
				m_valueEnteredDirectly.Add(record as BiffRecordRaw);
			}
			break;
		case 2:
			if (m_CategoryRange == null)
			{
				m_categoryEnteredDirectly.Add(record as BiffRecordRaw);
			}
			break;
		case 3:
			if (m_BubbleRange == null)
			{
				m_bubbleEnteredDirectly.Add(record as BiffRecordRaw);
			}
			break;
		}
	}

	public List<BiffRecordRaw> GetArray(int siIndex)
	{
		if (siIndex > 3 || siIndex < 1)
		{
			throw new ArgumentOutOfRangeException("siIndex");
		}
		List<BiffRecordRaw> list = null;
		switch (siIndex)
		{
		case 1:
			list = m_valueEnteredDirectly;
			break;
		case 2:
			list = m_categoryEnteredDirectly;
			break;
		case 3:
			list = m_bubbleEnteredDirectly;
			break;
		}
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		return list;
	}

	public object[] GetEnteredDirectlyValues(List<BiffRecordRaw> array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int count = array.Count;
		if (count == 0)
		{
			return null;
		}
		List<object> list = new List<object>(count);
		for (int i = 0; i < count; i++)
		{
			BiffRecordRaw biffRecordRaw = array[i];
			object item = null;
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.Number:
				item = ((NumberRecord)biffRecordRaw).Value;
				break;
			case TBIFFRecord.Label:
				item = ((LabelRecord)biffRecordRaw).Label;
				break;
			}
			list.Add(item);
		}
		return list.ToArray();
	}

	private bool GetEnteredDirectlyType(object[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (array[i] is string)
			{
				return false;
			}
		}
		return true;
	}

	private List<BiffRecordRaw> GetArrayRecordsByValues(bool bIsNumber, object[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		int num = values.Length;
		List<BiffRecordRaw> list = new List<BiffRecordRaw>(num);
		for (int i = 0; i < num; i++)
		{
			object obj = values[i];
			if (obj == null)
			{
				throw new ApplicationException("Null referance value in values array at " + i + " position");
			}
			if (obj is bool && !bIsNumber)
			{
				obj = (((bool)obj) ? "TRUE" : "FALSE");
			}
			ICellPositionFormat cellPositionFormat;
			if (bIsNumber)
			{
				NumberRecord obj2 = (NumberRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Number);
				obj2.Value = Convert.ToDouble(obj);
				cellPositionFormat = obj2;
			}
			else
			{
				LabelRecord obj3 = (LabelRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Label);
				obj3.Label = Convert.ToString(obj);
				cellPositionFormat = obj3;
			}
			cellPositionFormat.Column = (ushort)Index;
			cellPositionFormat.Row = (ushort)i;
			list.Add((BiffRecordRaw)cellPositionFormat);
		}
		return list;
	}

	private void UpdateSerieIndexesInEnteredDirectlyValues(List<BiffRecordRaw> arrayToUpdate)
	{
		if (arrayToUpdate == null)
		{
			throw new ArgumentNullException("arrayToUpdate");
		}
		int index = Index;
		int i = 0;
		for (int count = arrayToUpdate.Count; i < count; i++)
		{
			((ICellPositionFormat)arrayToUpdate[i]).Column = (ushort)index;
		}
	}

	private string DetectSerieName()
	{
		if (m_strName == null || m_strName.Length == 0 || m_strName[0] != '=')
		{
			return m_strName;
		}
		string text = null;
		if (m_nameRange != null)
		{
			return GetTextRangeValue(m_nameRange);
		}
		m_strName.Substring(1);
		try
		{
			if (m_nameTokens != null)
			{
				IRangeGetter rangeGetter = (IRangeGetter)m_nameTokens[0];
				m_nameRange = rangeGetter.GetRange(m_book, null);
				if (m_nameRange == null)
				{
					return "#REF!";
				}
				return GetTextRangeValue(m_nameRange);
			}
			return m_strName = "#REF!";
		}
		catch
		{
			return m_strName = "#REF!";
		}
	}

	private string GetTextRangeValue(IRange range)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		string text = "";
		int column = range.Column;
		int lastColumn = range.LastColumn;
		int row = range.Row;
		int lastRow = range.LastRow;
		if (range.Worksheet is WorksheetImpl)
		{
			IMigrantRange migrantRange = range.Worksheet.MigrantRange;
			int i = column;
			for (int num = lastColumn; i <= num; i++)
			{
				int j = row;
				for (int num2 = lastRow; j <= num2; j++)
				{
					migrantRange.ResetRowColumn(j, i);
					string value = migrantRange.Value;
					if (value != null && value.Length > 0)
					{
						text = text + value + " ";
					}
				}
			}
		}
		else if (column != -1 && row != -1)
		{
			int k = column;
			for (int num3 = lastColumn; k <= num3; k++)
			{
				int l = row;
				for (int num4 = lastRow; l <= num4; l++)
				{
					string value2 = range[l, k].Value;
					if (value2 != null && value2.Length > 0)
					{
						text = text + value2 + " ";
					}
				}
			}
		}
		if (!(text == ""))
		{
			return text.Substring(0, text.Length - 1);
		}
		return "";
	}

	internal void UpdateChartExSerieRangesMembers(bool isValues)
	{
		if (isValues)
		{
			if (m_ValueRange != null && !(m_ValueRange is ExternalRange))
			{
				if (m_ValueRange.LastColumn - m_ValueRange.Column > 0 || (m_ValueRange.Column == m_ValueRange.LastColumn && m_ValueRange.Row == m_ValueRange.LastRow))
				{
					m_isRowWiseSeries = true;
				}
				else
				{
					m_isRowWiseSeries = false;
				}
			}
		}
		else if (m_CategoryRange != null && !(m_CategoryRange is ExternalRange))
		{
			int num = m_CategoryRange.LastRow - m_CategoryRange.Row;
			if (m_CategoryRange.LastColumn - m_CategoryRange.Column > num)
			{
				m_isRowWiseCategory = true;
			}
			else
			{
				m_isRowWiseCategory = false;
			}
		}
	}

	public void SetDefaultName(string strName, bool isClearNameRange)
	{
		if (strName == null || strName.Length == 0)
		{
			throw new ArgumentNullException("strName");
		}
		m_strName = strName;
		m_bDefaultName = true;
		if (isClearNameRange)
		{
			m_nameRange = null;
		}
	}

	public IRange GetSerieNameRange()
	{
		_ = Name;
		return m_nameRange;
	}

	public OfficeChartType DetectSerieType()
	{
		string value = DetectSerieTypeString();
		m_serieType = (OfficeChartType)Enum.Parse(typeof(OfficeChartType), value, ignoreCase: true);
		return m_serieType;
	}

	public string DetectSerieTypeString()
	{
		ChartFormatCollection primaryFormats = m_chart.PrimaryFormats;
		if (m_chart.Loading && primaryFormats.Count == 0)
		{
			return "Column_Clustered";
		}
		ChartFormatImpl commonSerieFormat = GetCommonSerieFormat();
		return commonSerieFormat.FormatRecordType switch
		{
			TBIFFRecord.ChartBar => DetectBarSerie(commonSerieFormat), 
			TBIFFRecord.ChartLine => DetectLineSerie(commonSerieFormat), 
			TBIFFRecord.ChartPie => DetectPieSerie(commonSerieFormat), 
			TBIFFRecord.ChartArea => DetectAreaSerie(commonSerieFormat), 
			TBIFFRecord.ChartScatter => DetectScatterSerie(commonSerieFormat), 
			TBIFFRecord.ChartSurface => DetectSurfaceSerie(commonSerieFormat), 
			TBIFFRecord.ChartRadar => DetectRadarSerie(commonSerieFormat), 
			TBIFFRecord.ChartRadarArea => "Radar_Filled", 
			TBIFFRecord.ChartBoppop => DetectBoppopSerie(commonSerieFormat), 
			_ => throw new ArgumentOutOfRangeException("Can't detect serie type."), 
		};
	}

	public string DetectSerieTypeStart()
	{
		ChartFormatCollection primaryFormats = m_chart.PrimaryFormats;
		if (m_chart.Loading && primaryFormats.Count == 0)
		{
			return "Column";
		}
		ChartFormatImpl commonSerieFormat = GetCommonSerieFormat();
		switch (commonSerieFormat.FormatRecordType)
		{
		case TBIFFRecord.ChartBar:
			return GetBarStartString(commonSerieFormat);
		case TBIFFRecord.ChartLine:
			return "Line";
		case TBIFFRecord.ChartPie:
			if (commonSerieFormat.DoughnutHoleSize == 0)
			{
				return "Pie";
			}
			return "Doughnut";
		case TBIFFRecord.ChartArea:
			return "Area";
		case TBIFFRecord.ChartScatter:
			if ((commonSerieFormat.DataFormatOrNull == null || commonSerieFormat.DataFormatOrNull.SerieFormatOrNull == null || !commonSerieFormat.DataFormatOrNull.Is3DBubbles) && !commonSerieFormat.IsBubbles)
			{
				return "Scatter";
			}
			return "Bubble";
		case TBIFFRecord.ChartSurface:
			return "Surface";
		case TBIFFRecord.ChartRadar:
		case TBIFFRecord.ChartRadarArea:
			return "Radar";
		case TBIFFRecord.ChartBoppop:
			return "Pie";
		default:
			throw new ArgumentOutOfRangeException("Can't detect serie type.");
		}
	}

	private string DetectBarSerie(ChartFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.FormatRecordType != TBIFFRecord.ChartBar)
		{
			throw new ArgumentException("format");
		}
		string empty = string.Empty;
		empty = GetBarStartString(format);
		if (format.IsChartExType)
		{
			return empty;
		}
		if (format.StackValuesBar)
		{
			empty += "_Stacked";
		}
		else
		{
			if (empty == "Column" && format.Is3D && !format.IsClustered)
			{
				return "Column_3D";
			}
			empty += "_Clustered";
		}
		if (format.ShowAsPercentsBar)
		{
			empty += "_100";
		}
		bool flag = empty.IndexOf("Cone") != -1 || empty.IndexOf("Cylinder") != -1 || empty.IndexOf("Pyramid") != -1;
		if (format.Is3D && !flag)
		{
			empty += "_3D";
		}
		if (flag && !format.IsClustered && !format.StackValuesBar)
		{
			empty += "_3D";
		}
		return empty;
	}

	private string GetBarStartString(ChartFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.FormatRecordType != TBIFFRecord.ChartBar)
		{
			throw new ArgumentException("format");
		}
		string result = null;
		if (format.IsChartExType)
		{
			return m_chart.m_chartType.ToString();
		}
		if (!ParentChart.Loading && (DataPoints as ChartDataPointsCollection).DefPointFormatOrNull != null && (DataPoints as ChartDataPointsCollection).DefPointFormatOrNull.IsFormatted && ((DataPoints as ChartDataPointsCollection).DefPointFormatOrNull.BarShapeBase != 0 || (DataPoints as ChartDataPointsCollection).DefPointFormatOrNull.BarShapeTop != 0))
		{
			return GetBarShapeString(format, result, SerieFormat as ChartSerieDataFormatImpl);
		}
		if (format.DataFormatOrNull != null && format.DataFormatOrNull.Serie3DdDataFormatOrNull != null)
		{
			return GetBarShapeString(format, result, format.DataFormatOrNull);
		}
		return format.IsHorizontalBar ? "Bar" : "Column";
	}

	private string GetBarShapeString(ChartFormatImpl format, string result, ChartSerieDataFormatImpl dataFormat)
	{
		if (dataFormat.BarShapeBase == OfficeBaseFormat.Circle)
		{
			result = ((dataFormat.BarShapeTop == OfficeTopFormat.Straight) ? "Cylinder" : "Cone");
			if (format.IsHorizontalBar)
			{
				result += "_Bar";
			}
		}
		else if (dataFormat.BarShapeTop == OfficeTopFormat.Straight)
		{
			result = (format.IsHorizontalBar ? "Bar" : "Column");
		}
		else
		{
			result = "Pyramid";
			if (format.IsHorizontalBar)
			{
				result += "_Bar";
			}
		}
		return result;
	}

	private string DetectPieSerie(ChartFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.FormatRecordType != TBIFFRecord.ChartPie)
		{
			throw new ArgumentException("format");
		}
		string text = ((m_chart.m_chartType == OfficeChartType.Doughnut || m_chart.m_chartType == OfficeChartType.Doughnut_Exploded) ? "Doughnut" : "Pie");
		if (!ParentChart.Loading && text == "Doughnut" && ParentSeries.Count != 0 && (ParentSeries[ParentSeries.Count - 1].DataPoints as ChartDataPointsCollection).DefPointFormatOrNull != null && (ParentSeries[ParentSeries.Count - 1].DataPoints as ChartDataPointsCollection).DefPointFormatOrNull.Percent > 0)
		{
			text += "_Exploded";
		}
		else if (!ParentChart.Loading && text == "Pie" && (DataPoints as ChartDataPointsCollection).DefPointFormatOrNull != null && (DataPoints as ChartDataPointsCollection).DefPointFormatOrNull.Percent > 0)
		{
			text += "_Exploded";
		}
		else if (format.DataFormatOrNull != null)
		{
			ChartSerieDataFormatImpl dataFormatOrNull = format.DataFormatOrNull;
			if (dataFormatOrNull.PieFormatOrNull != null && dataFormatOrNull.Percent > 0)
			{
				text += "_Exploded";
			}
		}
		if (format.Is3D)
		{
			text += "_3D";
		}
		return text;
	}

	private string DetectAreaSerie(ChartFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.FormatRecordType != TBIFFRecord.ChartArea)
		{
			throw new ArgumentException("format");
		}
		if (format.Is3D && !format.RightAngleAxes && !format.IsStacked)
		{
			return "Area_3D";
		}
		string text = "Area";
		if (format.IsStacked)
		{
			text += "_Stacked";
		}
		if (format.IsCategoryBrokenDown)
		{
			text += "_100";
		}
		if (format.Is3D)
		{
			text += "_3D";
		}
		return text;
	}

	private string DetectSurfaceSerie(ChartFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.FormatRecordType != TBIFFRecord.ChartSurface)
		{
			throw new ArgumentException("format");
		}
		string text = "Surface";
		if (!format.IsFillSurface)
		{
			text += "_NoColor";
		}
		if (format.Rotation == 0 && format.Elevation == 90 && format.Perspective == 0)
		{
			return text + "_Contour";
		}
		return text + "_3D";
	}

	private string DetectBoppopSerie(ChartFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.FormatRecordType != TBIFFRecord.ChartBoppop)
		{
			throw new ArgumentException("format");
		}
		return format.PieChartType switch
		{
			OfficePieType.Normal => "Pie", 
			OfficePieType.Bar => "Pie_Bar", 
			OfficePieType.Pie => "PieOfPie", 
			_ => throw new ApplicationException("Can't detect boppop serie type."), 
		};
	}

	private string DetectRadarSerie(ChartFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.FormatRecordType != TBIFFRecord.ChartRadar)
		{
			throw new ArgumentException("format");
		}
		string text = "Radar";
		if (format.IsMarker && (format.DataFormatOrNull != null || !ParentChart.m_bIsRadarTypeChanged))
		{
			text += "_Markers";
		}
		ChartSerieDataFormatImpl defPointFormatOrNull = ((ChartDataPointsCollection)DataPoints).DefPointFormatOrNull;
		if (defPointFormatOrNull != null && defPointFormatOrNull.ContainsLineProperties)
		{
			text = "Radar";
			if (defPointFormatOrNull.IsMarker)
			{
				text += "_Markers";
			}
		}
		return text;
	}

	private string DetectLineSerie(ChartFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.FormatRecordType != TBIFFRecord.ChartLine)
		{
			throw new ArgumentException("format");
		}
		if (format.Is3D)
		{
			return "Line_3D";
		}
		bool flag = false;
		ChartSerieDataFormatImpl defPointFormatOrNull = ((ChartDataPointsCollection)DataPoints).DefPointFormatOrNull;
		if (defPointFormatOrNull != null && defPointFormatOrNull.ContainsLineProperties)
		{
			flag = true;
		}
		string text = "Line";
		ChartImpl parentChart = ParentChart;
		if ((parentChart != null && parentChart.LineChartCount == 1 && parentChart.Series[0].Equals(this) && ((!flag && format.IsMarker) || (flag && defPointFormatOrNull.IsMarker))) || (parentChart.LineChartCount == 1 && parentChart.Series.Count > 0 && !parentChart.Series[0].Equals(this) && (parentChart.Series[0] as ChartSerieImpl).m_serieType.ToString().Contains("Markers")) || (parentChart.LineChartCount != 1 && ((!flag && format.IsMarker) || (flag && defPointFormatOrNull.IsMarker))))
		{
			text += "_Markers";
		}
		if (format.StackValuesLine)
		{
			text += "_Stacked";
		}
		if (format.ShowAsPercentsLine)
		{
			text += "_100";
		}
		return text;
	}

	private string DetectScatterSerie(ChartFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.FormatRecordType != TBIFFRecord.ChartScatter)
		{
			throw new ArgumentException("format");
		}
		if (format.DataFormatOrNull != null)
		{
			ChartSerieDataFormatImpl dataFormatOrNull = format.DataFormatOrNull;
			if (dataFormatOrNull.SerieFormatOrNull != null && dataFormatOrNull.Is3DBubbles)
			{
				return "Bubble_3D";
			}
		}
		if (format.IsBubbles)
		{
			return "Bubble";
		}
		ChartSerieDataFormatImpl defPointFormatOrNull = ((ChartDataPointsCollection)DataPoints).DefPointFormatOrNull;
		bool containsLineProperties = defPointFormatOrNull.ContainsLineProperties;
		string text = "Scatter";
		if ((containsLineProperties && defPointFormatOrNull.IsSmoothed) || (!containsLineProperties && format.IsSmoothed))
		{
			text += "_SmoothedLine";
		}
		else if ((containsLineProperties && defPointFormatOrNull.IsLine) || (!containsLineProperties && format.IsLine))
		{
			text += "_Line";
		}
		ChartImpl parentChart = ParentChart;
		if ((containsLineProperties && defPointFormatOrNull.IsMarker) || (!containsLineProperties && format.IsMarker) || (parentChart != null && parentChart.Series.Count > 0 && (parentChart.Series[0] as ChartSerieImpl).m_serieType.ToString().Contains("Markers")))
		{
			text += "_Markers";
		}
		if (text == "Scatter")
		{
			text = OfficeChartType.Scatter_Line.ToString();
		}
		return text;
	}

	internal void ChangeSeriesType(OfficeChartType seriesType, bool isSeriesCreation)
	{
		ChangeSeriesType(seriesType, isSeriesCreation, forceChange: false);
	}

	internal void ChangeSeriesType(OfficeChartType seriesType, bool isSeriesCreation, bool forceChange)
	{
		OfficeChartType serieType = SerieType;
		if (m_book.IsWorkbookOpening || serieType != seriesType || forceChange)
		{
			m_chart.TypeChanging = true;
			OnSerieTypeChange(seriesType, serieType, isSeriesCreation, forceChange);
			m_serieType = seriesType;
			m_chart.TypeChanging = false;
		}
	}

	private void OnSerieTypeChange(OfficeChartType type, OfficeChartType oldType, bool isSeriesCreation, bool forceChange)
	{
		if (ChartImpl.IsChartExSerieType(type))
		{
			m_chart.ChangeToChartExType(SerieType, type, isSeriesCreation);
			return;
		}
		if (isSeriesCreation || (type != oldType && (m_chart.Loading || forceChange || ChartImpl.IsChartExSerieType(oldType) || Array.IndexOf(ChartImpl.DEF_NOT_3D, type) != -1 || Array.IndexOf(ChartImpl.DEF_NOT_SUPPORT_GRIDLINES, type) != -1 || Array.IndexOf(ChartImpl.DEF_NEED_VIEW_3D, type) != -1)))
		{
			m_dataPoints.Clear();
		}
		else if (type != oldType)
		{
			m_dataPoints.ClearWithExistingFormats(type);
		}
		if (!ChartImpl.CheckDataTablePossibility(ChartFormatImpl.GetStartSerieType(type), bThrowException: false))
		{
			m_chart.HasDataTable = false;
		}
		HasErrorBarsX = false;
		HasErrorBarsY = false;
		m_trendLines.Clear();
		OfficeChartType chartType = m_chart.ChartType;
		if (Array.IndexOf(ChartImpl.DEF_COMBINATION_CHART, chartType) != -1)
		{
			ChangeCombinationType(type);
			return;
		}
		if (Array.IndexOf(ChartImpl.DEF_CHANGE_SERIE, chartType) == -1 || m_chart.Series.Count == 1)
		{
			m_chart.ChangeChartType(type, isSeriesCreation);
			return;
		}
		if (Array.IndexOf(ChartImpl.DEF_CHANGE_SERIE, type) == -1)
		{
			throw new ArgumentException("Cannot change serie type.");
		}
		ChartFormatImpl commonSerieFormat = GetCommonSerieFormat();
		if (!ChangeIntimateType(commonSerieFormat, type))
		{
			if (type == OfficeChartType.Bubble || type == OfficeChartType.Bubble_3D || chartType == OfficeChartType.Bubble_3D || chartType == OfficeChartType.Bubble)
			{
				throw new ArgumentException("Cannot change serie type.");
			}
			if (Array.IndexOf(ChartImpl.DEF_NOT_SUPPORT_GRIDLINES, type) != -1)
			{
				m_chart.PrimaryParentAxis.ClearGridLines();
			}
			ChangeNotIntimateType(type);
		}
	}

	private void ChangeNotIntimateType(OfficeChartType type)
	{
		m_chart.IsManuallyFormatted = true;
		bool usePrimaryAxis = UsePrimaryAxis;
		ChartFormatImpl chartFormatImpl = m_chart.PrimaryParentAxis.Formats.ChangeNotIntimateSerieType(type, SerieType, base.Application, m_chart, this);
		ChartGroup = chartFormatImpl.DrawingZOrder;
		if (m_chart.TypeChanging && !UsePrimaryAxis && usePrimaryAxis != UsePrimaryAxis)
		{
			m_chart.SecondaryParentAxis.UpdateSecondaryAxis(bCreateAxis: true);
		}
		if (chartFormatImpl.DataFormatOrNull != null && chartFormatImpl.DataFormatOrNull.ParentSerie == null && chartFormatImpl.DataFormatOrNull.DataFormat != null && chartFormatImpl.DataFormatOrNull.DataFormat.SeriesIndex != ChartGroup)
		{
			chartFormatImpl.DataFormatOrNull.DataFormat.SeriesIndex = (ushort)ChartGroup;
		}
	}

	private bool ChangeIntimateType(ChartFormatImpl format, OfficeChartType TypeToChange)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		string startSerieType = ChartFormatImpl.GetStartSerieType(SerieType);
		string startSerieType2 = ChartFormatImpl.GetStartSerieType(TypeToChange);
		if (startSerieType != startSerieType2)
		{
			return false;
		}
		startSerieType2 = TypeToChange.ToString();
		ChartDataPointImpl chartDataPointImpl = (ChartDataPointImpl)m_dataPoints.DefaultDataPoint;
		ChartSerieDataFormatImpl dataFormatOrNull = chartDataPointImpl.DataFormatOrNull;
		if (startSerieType == "Radar" && SerieType != OfficeChartType.Radar_Filled && TypeToChange != OfficeChartType.Radar_Filled)
		{
			dataFormatOrNull.ChangeRadarDataFormat(TypeToChange);
			return true;
		}
		switch (startSerieType)
		{
		case "Scatter":
			dataFormatOrNull.ChangeScatterDataFormat(TypeToChange);
			return true;
		case "Bubble":
			chartDataPointImpl.ChangeIntimateBuble(TypeToChange);
			return true;
		case "Line":
			ChangeIntimateLine(format, TypeToChange, startSerieType2);
			return true;
		default:
			if (SerieType != TypeToChange)
			{
				format.ChangeSerieType(TypeToChange, isSeriesCreation: false);
			}
			return true;
		}
	}

	private void ChangeIntimateLine(ChartFormatImpl format, OfficeChartType TypeToChange, string strTypeToChange)
	{
		bool flag = strTypeToChange.IndexOf("_Markers") != -1;
		if (format.IsMarker == flag)
		{
			format.ChangeSerieType(TypeToChange, isSeriesCreation: false);
			return;
		}
		Dictionary<OfficeChartType, OfficeChartType> dictionary = new Dictionary<OfficeChartType, OfficeChartType>(7);
		InitalizeChangeLineTypeHash(dictionary);
		format.ChangeSerieType(dictionary[TypeToChange], isSeriesCreation: false);
		((ChartSerieDataFormatImpl)((ChartDataPointImpl)DataPoints.DefaultDataPoint).DataFormat).ChangeLineDataFormat(TypeToChange);
	}

	private void InitalizeChangeLineTypeHash(Dictionary<OfficeChartType, OfficeChartType> hashToInit)
	{
		if (hashToInit == null)
		{
			throw new ArgumentNullException("hashToInit");
		}
		hashToInit.Add(OfficeChartType.Line, OfficeChartType.Line_Markers);
		hashToInit.Add(OfficeChartType.Line_Stacked, OfficeChartType.Line_Markers_Stacked);
		hashToInit.Add(OfficeChartType.Line_Stacked_100, OfficeChartType.Line_Markers_Stacked_100);
		hashToInit.Add(OfficeChartType.Line_Markers_Stacked, OfficeChartType.Line_Stacked);
		hashToInit.Add(OfficeChartType.Line_Markers_Stacked_100, OfficeChartType.Line_Stacked_100);
		hashToInit.Add(OfficeChartType.Line_Markers, OfficeChartType.Line);
	}

	private void ChangeCombinationType(OfficeChartType type)
	{
		ChartFormatImpl chartFormatImpl = FindIntimateFormatByType(type, UsePrimaryAxis, bPreferSameAxis: true);
		ChartGlobalFormatsCollection formats = m_chart.PrimaryParentAxis.Formats;
		ChartFormatImpl chartFormatImpl2 = null;
		int chartGroup = ChartGroup;
		if (formats.PrimaryFormats.ContainsIndex(chartGroup) || formats.SecondaryFormats.ContainsIndex(chartGroup))
		{
			chartFormatImpl2 = GetCommonSerieFormat();
		}
		if (chartFormatImpl != null)
		{
			ChartGroup = chartFormatImpl.DrawingZOrder;
			if (m_seriesColl.GetCountOfSeriesWithSameDrawingOrder(chartGroup) == 0 && chartFormatImpl2 != null)
			{
				m_chart.PrimaryParentAxis.Formats.Remove(chartFormatImpl2);
			}
			ChangeIntimateType(chartFormatImpl, type);
			if (m_seriesColl.GetCountOfSeriesWithSameType(type, UsePrimaryAxis) == m_seriesColl.Count)
			{
				m_chart.ChartType = SerieType;
			}
			return;
		}
		bool num = m_seriesColl.GetCountOfSeriesWithSameDrawingOrder(chartGroup) == 1;
		bool num2 = Array.IndexOf(ChartImpl.DEF_NEED_SECONDARY_AXIS, type) != -1;
		int count = m_chart.SecondaryFormats.Count;
		if (num2 && count != 0)
		{
			throw new ArgumentException("cannot change serie type.");
		}
		ChartFormatCollection chartFormatCollection;
		if (num2)
		{
			chartFormatCollection = m_chart.SecondaryFormats;
			m_chart.SecondaryParentAxis.UpdateSecondaryAxis(bCreateAxis: true);
		}
		else
		{
			chartFormatCollection = m_chart.PrimaryFormats;
		}
		chartFormatImpl = new ChartFormatImpl(base.Application, chartFormatCollection);
		if (num)
		{
			ChartGroup = ((ChartGroup == 0) ? 1 : 0);
			m_chart.PrimaryParentAxis.Formats.Remove(chartFormatImpl2);
		}
		chartFormatImpl.ChangeSerieType(type, isSeriesCreation: false);
		chartFormatImpl.DrawingZOrder = m_seriesColl.FindOrderByType(type);
		chartFormatCollection.Add(chartFormatImpl, bCanReplace: false);
		ChartGroup = chartFormatImpl.DrawingZOrder;
	}

	public ChartFormatImpl FindIntimateFormatByType(OfficeChartType type, bool bPrimaryAxis, bool bPreferSameAxis)
	{
		string startSerieType = ChartFormatImpl.GetStartSerieType(type);
		List<int> list = new List<int>(6);
		ChartFormatImpl result = null;
		int i = 0;
		for (int count = m_seriesColl.Count; i < count; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)m_seriesColl[i];
			int chartGroup = chartSerieImpl.ChartGroup;
			if (list.Contains(chartGroup))
			{
				continue;
			}
			if (startSerieType == ChartFormatImpl.GetStartSerieType(chartSerieImpl.SerieType))
			{
				if (startSerieType != "Radar")
				{
					result = chartSerieImpl.GetCommonSerieFormat();
				}
				if ((bPreferSameAxis && bPrimaryAxis == chartSerieImpl.UsePrimaryAxis) || !bPreferSameAxis)
				{
					break;
				}
			}
			list.Add(chartGroup);
		}
		return result;
	}

	public void UpdateFormula(int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect)
	{
		IsValidCategoryRange = m_IsValidCategoryRange;
		IsValidValueRange = m_IsValidValueRange;
		if (m_ValueRange != null)
		{
			ChartAIRecord chartAI = m_hashAi[ChartAIRecord.LinkIndex.LinkToValues];
			ValuesIRange = UpdateRange(chartAI, iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect);
		}
		if (m_CategoryRange != null)
		{
			ChartAIRecord chartAI = m_hashAi[ChartAIRecord.LinkIndex.LinkToCategories];
			CategoryLabelsIRange = UpdateRange(chartAI, iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect);
		}
		if (m_BubbleRange != null)
		{
			ChartAIRecord chartAI = m_hashAi[ChartAIRecord.LinkIndex.LinkToBubbles];
			BubblesIRange = UpdateRange(chartAI, iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect);
		}
	}

	private IRange UpdateRange(ChartAIRecord chartAI, int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect)
	{
		if (chartAI == null)
		{
			return null;
		}
		Ptg[] parsedExpression = chartAI.ParsedExpression;
		List<Ptg> list = new List<Ptg>();
		int i = 0;
		for (int num = parsedExpression.Length; i < num; i++)
		{
			Ptg token = parsedExpression[i];
			Ptg[] collection = UpdateToken(token, iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect);
			list.AddRange(collection);
		}
		chartAI.ParsedExpression = list.ToArray();
		return GetRange(chartAI);
	}

	private Ptg[] UpdateToken(Ptg token, int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect)
	{
		if (token == null)
		{
			throw new ArgumentNullException("token");
		}
		if (token is IReference reference)
		{
			int refIndex = reference.RefIndex;
			if (refIndex != iSourceIndex && refIndex == iDestIndex)
			{
				return new Ptg[1] { token };
			}
			if (token is IRectGetter rectGetter)
			{
				Rectangle rectangle = rectGetter.GetRectangle();
				if (refIndex == iSourceIndex)
				{
					if (sourceRect.Contains(rectangle))
					{
						bool bChanged;
						return new Ptg[1] { token.Offset(iCurIndex, -1, -1, iSourceIndex, sourceRect, iDestIndex, destRect, out bChanged, m_book) };
					}
					if (iSourceIndex == iDestIndex && UtilityMethods.Intersects(sourceRect, rectangle))
					{
						Ptg ptg = PartialTokenMove(token, iSourceIndex, rectangle, sourceRect, destRect);
						return new Ptg[1] { ptg };
					}
				}
				if (refIndex == iDestIndex)
				{
					if (destRect.Contains(rectangle))
					{
						return new Ptg[0];
					}
					if (UtilityMethods.Intersects(destRect, rectangle))
					{
						Ptg ptg2 = PartialRemove(token, refIndex, rectangle, sourceRect, destRect);
						return new Ptg[1] { ptg2 };
					}
				}
				return new Ptg[1] { token };
			}
			return new Ptg[1] { token };
		}
		return new Ptg[1] { token };
	}

	private Ptg PartialTokenMove(Ptg token, int iSourceIndex, Rectangle rectRange, Rectangle sourceRect, Rectangle destRect)
	{
		bool flag = UtilityMethods.Contains(sourceRect, rectRange.Left, rectRange.Top);
		bool flag2 = UtilityMethods.Contains(sourceRect, rectRange.Right, rectRange.Bottom);
		if (!flag && !flag2)
		{
			return token;
		}
		int num = destRect.Left - sourceRect.Left;
		int num2 = destRect.Top - sourceRect.Top;
		if (rectRange.Height == 0 && num2 == 0)
		{
			int num3;
			int num4;
			if (flag)
			{
				num3 = Math.Min(rectRange.Left + num, sourceRect.Right + 1);
				num4 = Math.Max(rectRange.Left + num, rectRange.Right);
			}
			else
			{
				num3 = Math.Min(rectRange.Right + num, rectRange.Left);
				num4 = Math.Max(rectRange.Right + num, sourceRect.Left - 1);
			}
			return FormulaUtil.CreatePtg(token.TokenCode, iSourceIndex, rectRange.Top, num3, rectRange.Bottom, num4, (byte)0, (byte)0);
		}
		if (rectRange.Width == 0 && num == 0)
		{
			int num5;
			int num6;
			if (flag)
			{
				num5 = Math.Min(rectRange.Top + num2, sourceRect.Bottom + 1);
				num6 = Math.Max(rectRange.Top + num2, rectRange.Bottom);
			}
			else
			{
				num5 = Math.Min(rectRange.Bottom + num2, rectRange.Top);
				num6 = Math.Max(rectRange.Bottom + num2, sourceRect.Top - 1);
			}
			return FormulaUtil.CreatePtg(token.TokenCode, iSourceIndex, num5, rectRange.Left, num6, rectRange.Right, (byte)0, (byte)0);
		}
		return token;
	}

	private Ptg PartialRemove(Ptg token, int iSheetIndex, Rectangle rectRange, Rectangle sourceRect, Rectangle destRect)
	{
		if (token == null)
		{
			throw new ArgumentNullException("token");
		}
		bool flag = UtilityMethods.Contains(destRect, rectRange.Left, rectRange.Top);
		bool flag2 = UtilityMethods.Contains(destRect, rectRange.Right, rectRange.Bottom);
		if (!flag && !flag2)
		{
			return token;
		}
		int num = rectRange.Top;
		int num2 = rectRange.Bottom;
		int num3 = rectRange.Left;
		int num4 = rectRange.Right;
		if (rectRange.Left == rectRange.Right)
		{
			if (flag)
			{
				num = destRect.Bottom + 1;
			}
			else
			{
				num2 = destRect.Top - 1;
			}
		}
		else if (rectRange.Top == rectRange.Bottom)
		{
			if (flag)
			{
				num3 = destRect.Right + 1;
			}
			else
			{
				num4 = destRect.Left - 1;
			}
		}
		return FormulaUtil.CreatePtg(token.TokenCode, iSheetIndex, num, num3, num2, num4, (byte)0, (byte)0);
	}

	private void UpdateErrorBar(ChartErrorBarsImpl bar, ref ChartErrorBarsImpl barToUpdate)
	{
		if (bar == null)
		{
			throw new ArgumentNullException("bar");
		}
		if (barToUpdate == null)
		{
			barToUpdate = bar;
		}
		else if (bar.Type == OfficeErrorBarType.Custom)
		{
			if (barToUpdate.Type != OfficeErrorBarType.Custom)
			{
				throw new ApplicationException("Cannot parse error bars. Different types.");
			}
			IRange plusIRange = bar.PlusIRange;
			IRange minusIRange = bar.MinusIRange;
			if (minusIRange != null)
			{
				barToUpdate.MinusIRange = minusIRange;
			}
			if (plusIRange != null)
			{
				barToUpdate.PlusIRange = plusIRange;
			}
			if ((bar.Include == OfficeErrorBarInclude.Minus && barToUpdate.Include == OfficeErrorBarInclude.Plus) || (bar.Include == OfficeErrorBarInclude.Plus && barToUpdate.Include == OfficeErrorBarInclude.Minus))
			{
				barToUpdate.Include = OfficeErrorBarInclude.Both;
			}
		}
		else
		{
			barToUpdate.Include = OfficeErrorBarInclude.Both;
		}
	}

	public ChartFormatImpl GetCommonSerieFormat()
	{
		if (m_chart.SecondaryFormats.ContainsIndex(m_iChartGroup))
		{
			return m_chart.SecondaryFormats[m_iChartGroup];
		}
		return m_chart.PrimaryFormats[m_iChartGroup];
	}

	public void MarkUsedReferences(bool[] usedItems)
	{
		foreach (ChartAIRecord value in m_hashAi.Values)
		{
			FormulaUtil.MarkUsedReferences(value.ParsedExpression, usedItems);
		}
		if (m_errorBarX != null)
		{
			m_errorBarX.MarkUsedReferences(usedItems);
		}
		if (m_errorBarY != null)
		{
			m_errorBarY.MarkUsedReferences(usedItems);
		}
		if (m_trendLines != null)
		{
			m_trendLines.MarkUsedReferences(usedItems);
		}
	}

	public void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		foreach (ChartAIRecord value in m_hashAi.Values)
		{
			Ptg[] parsedExpression = value.ParsedExpression;
			if (FormulaUtil.UpdateReferenceIndexes(parsedExpression, arrUpdatedIndexes))
			{
				value.ParsedExpression = parsedExpression;
			}
		}
		if (m_errorBarX != null)
		{
			m_errorBarX.UpdateReferenceIndexes(arrUpdatedIndexes);
		}
		if (m_errorBarY != null)
		{
			m_errorBarY.UpdateReferenceIndexes(arrUpdatedIndexes);
		}
		if (m_trendLines != null)
		{
			m_trendLines.UpdateReferenceIndexes(arrUpdatedIndexes);
		}
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		SerializeSerie(records);
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		SerializeChartAi(records);
		m_dataPoints.SerializeDataFormats(records);
		ChartSertocrtRecord chartSertocrtRecord = (ChartSertocrtRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSertocrt);
		chartSertocrtRecord.ChartGroup = (ushort)ChartGroup;
		records.Add(chartSertocrtRecord);
		SerializeLegendEntries(records);
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
		if (m_valueEnteredDirectly.Count != 0)
		{
			UpdateSerieIndexesInEnteredDirectlyValues(m_valueEnteredDirectly);
		}
		if (m_categoryEnteredDirectly.Count != 0)
		{
			UpdateSerieIndexesInEnteredDirectlyValues(m_categoryEnteredDirectly);
		}
		if (m_bubbleEnteredDirectly.Count != 0)
		{
			UpdateSerieIndexesInEnteredDirectlyValues(m_bubbleEnteredDirectly);
		}
		if (HasErrorBarsY)
		{
			m_errorBarY.Serialize(m_seriesColl.TrendErrorList);
		}
		if (HasErrorBarsX)
		{
			m_errorBarX.Serialize(m_seriesColl.TrendErrorList);
		}
		m_trendLines.Serialize(m_seriesColl.TrendErrorList);
	}

	private void SerializeChartAi(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		SeriealizeSerieName(records);
		ChartAIRecord chartAIRecord = m_hashAi[ChartAIRecord.LinkIndex.LinkToValues];
		SetAIRange(chartAIRecord, m_ValueRange, ChartAIRecord.ReferenceType.EnteredDirectly);
		if (m_ValueRange != null)
		{
			chartAIRecord.NumberFormatIndex = 2;
		}
		if (m_valueEnteredDirectly.Count != 0)
		{
			chartAIRecord.ParsedExpression = null;
			chartAIRecord.Reference = ChartAIRecord.ReferenceType.EnteredDirectly;
		}
		records.Add((BiffRecordRaw)chartAIRecord.Clone());
		chartAIRecord = m_hashAi[ChartAIRecord.LinkIndex.LinkToCategories];
		SetAIRange(chartAIRecord, m_CategoryRange, ChartAIRecord.ReferenceType.DefaultCategories);
		if (m_categoryEnteredDirectly.Count != 0)
		{
			chartAIRecord.ParsedExpression = null;
			chartAIRecord.Reference = ChartAIRecord.ReferenceType.EnteredDirectly;
		}
		records.Add((BiffRecordRaw)chartAIRecord.Clone());
		chartAIRecord = m_hashAi[ChartAIRecord.LinkIndex.LinkToBubbles];
		SetAIRange(chartAIRecord, m_BubbleRange, ChartAIRecord.ReferenceType.EnteredDirectly);
		if (m_bubbleEnteredDirectly.Count != 0)
		{
			chartAIRecord.ParsedExpression = null;
			chartAIRecord.Reference = ChartAIRecord.ReferenceType.EnteredDirectly;
		}
		records.Add((BiffRecordRaw)chartAIRecord.Clone());
	}

	private void SetAIRange(ChartAIRecord record, IRange range, ChartAIRecord.ReferenceType onNull)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (range == null)
		{
			record.Reference = onNull;
			record.ParsedExpression = null;
			return;
		}
		record.Reference = ChartAIRecord.ReferenceType.Worksheet;
		if (range.GetType() == typeof(RangeImpl))
		{
			((RangeImpl)range).SetWorkbook(m_book);
		}
		record.ParsedExpression = ((INativePTG)range).GetNativePtg();
	}

	private void SerializeSerie(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		m_series.StdX = ChartSeriesRecord.DataType.Numeric;
		m_series.StdY = ChartSeriesRecord.DataType.Numeric;
		m_series.BubbleDataType = ChartSeriesRecord.DataType.Numeric;
		if (m_valueEnteredDirectly.Count == 0)
		{
			m_series.ValuesCount = (ushort)((m_ValueRange != null) ? ((ushort)m_ValueRange.Count) : 0);
		}
		else
		{
			m_series.ValuesCount = (ushort)m_valueEnteredDirectly.Count;
			if (m_valueEnteredDirectly[0] is LabelRecord)
			{
				m_series.StdY = ChartSeriesRecord.DataType.Text;
			}
		}
		if (m_categoryEnteredDirectly.Count == 0)
		{
			m_series.CategoriesCount = ((m_CategoryRange != null) ? ((ushort)m_CategoryRange.Count) : m_series.ValuesCount);
		}
		else
		{
			m_series.CategoriesCount = (ushort)m_categoryEnteredDirectly.Count;
			if (m_categoryEnteredDirectly[0] is LabelRecord)
			{
				m_series.StdX = ChartSeriesRecord.DataType.Text;
			}
		}
		if (m_bubbleEnteredDirectly.Count == 0)
		{
			m_series.BubbleSeriesCount = (ushort)((m_BubbleRange != null) ? ((ushort)m_BubbleRange.Count) : 0);
		}
		else
		{
			m_series.BubbleSeriesCount = (ushort)m_bubbleEnteredDirectly.Count;
			if (m_bubbleEnteredDirectly[0] is LabelRecord)
			{
				m_series.BubbleDataType = ChartSeriesRecord.DataType.Text;
			}
		}
		CheckLimits();
		records.Add((BiffRecordRaw)m_series.Clone());
	}

	public void CheckLimits()
	{
		int num = ((m_categoryEnteredDirectly.Count != 0) ? m_categoryEnteredDirectly.Count : ((m_CategoryRange != null) ? ((ushort)m_CategoryRange.Count) : m_series.ValuesCount));
		OfficeVersion version = m_book.Version;
		if ((version == OfficeVersion.Excel97to2003 || version == OfficeVersion.Excel2007) && ((m_chart.IsChart3D && num > 4000) || num > 32000))
		{
			throw new ApplicationException("The maximum number of data points you can use in a data series for a 2-D chart is 32000, for a 3-D chart is 4000.If you want to use more data points, you must create two or more series or use Excel 2010.");
		}
	}

	[CLSCompliant(false)]
	public void SerializeDataLabels(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_dataPoints != null)
		{
			m_dataPoints.SerializeDataLabels(records);
		}
	}

	private void SerializeLegendEntries(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (!m_chart.HasLegend)
		{
			return;
		}
		string startSerieType = ChartFormatImpl.GetStartSerieType(m_chart.ChartType);
		ChartLegendEntriesColl chartLegendEntriesColl = (ChartLegendEntriesColl)m_chart.Legend.LegendEntries;
		if (Array.IndexOf(ChartImpl.DEF_LEGEND_NEED_DATA_POINT, startSerieType) == -1)
		{
			if (chartLegendEntriesColl.Contains(Index))
			{
				((ChartLegendEntryImpl)chartLegendEntriesColl[Index]).Serialize(records);
			}
		}
		else
		{
			if (Index != 0)
			{
				return;
			}
			int i = 0;
			for (int count = chartLegendEntriesColl.Count; i < count; i++)
			{
				if (chartLegendEntriesColl.Contains(i))
				{
					((ChartLegendEntryImpl)chartLegendEntriesColl[i]).Serialize(records);
				}
			}
		}
	}

	private void SeriealizeSerieName(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		string name = Name;
		ChartAIRecord chartAIRecord = m_hashAi[ChartAIRecord.LinkIndex.LinkToTitleOrText];
		if (((m_strName == null || m_strName.Length == 0) && m_nameRange == null) || m_strName[0] != '=')
		{
			chartAIRecord.ParsedExpression = null;
			chartAIRecord.Reference = ChartAIRecord.ReferenceType.EnteredDirectly;
		}
		else
		{
			Ptg[] nameTokens = GetNameTokens();
			chartAIRecord.ParsedExpression = nameTokens;
			chartAIRecord.Reference = ChartAIRecord.ReferenceType.Worksheet;
		}
		records.Add((BiffRecordRaw)chartAIRecord.Clone());
		if (!m_bDefaultName)
		{
			ChartSeriesTextRecord chartSeriesTextRecord = (ChartSeriesTextRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSeriesText);
			chartSeriesTextRecord.Text = ((name == null) ? "" : name);
			records.Add(chartSeriesTextRecord);
		}
	}

	private Ptg[] GetNameTokens()
	{
		if (m_nameRange == null && m_strName != null && m_strName[0] == '=')
		{
			string strFormula = UtilityMethods.RemoveFirstCharUnsafe(m_strName);
			return m_book.FormulaUtil.ParseString(strFormula);
		}
		return ((INativePTG)m_nameRange).GetNativePtg();
	}

	public void Reparse()
	{
		ChartAIRecord chartAi = m_hashAi[ChartAIRecord.LinkIndex.LinkToValues];
		m_ValueRange = GetRange(chartAi);
		chartAi = m_hashAi[ChartAIRecord.LinkIndex.LinkToBubbles];
		m_BubbleRange = GetRange(chartAi);
		chartAi = m_hashAi[ChartAIRecord.LinkIndex.LinkToCategories];
		m_CategoryRange = GetRange(chartAi);
	}

	[CLSCompliant(false)]
	public IRange GetRange(ChartAIRecord chartAi)
	{
		if (chartAi == null)
		{
			throw new ArgumentNullException("chartAi");
		}
		if (chartAi.ParsedExpression == null || chartAi.ParsedExpression.Length <= 1)
		{
			if (chartAi.Reference == ChartAIRecord.ReferenceType.Worksheet || chartAi.ParsedExpression != null)
			{
				Ptg currentPtg = chartAi.ParsedExpression[0];
				return GetRangeFromOnePTG(currentPtg);
			}
			return null;
		}
		IRanges ranges = null;
		int i = 0;
		for (int num = chartAi.ParsedExpression.Length; i < num; i++)
		{
			Ptg ptg = chartAi.ParsedExpression[i];
			if (!(ptg is IRangeGetter))
			{
				continue;
			}
			IRange rangeFromOnePTG = GetRangeFromOnePTG(ptg);
			if (rangeFromOnePTG != null)
			{
				if (ranges == null && rangeFromOnePTG.Worksheet != null)
				{
					ranges = rangeFromOnePTG.Worksheet.CreateRangesCollection();
				}
				ranges?.Add(rangeFromOnePTG);
			}
		}
		if (ranges != null && ranges.Count != 0)
		{
			return ranges;
		}
		return null;
	}

	private IRange GetRangeFromOnePTG(Ptg currentPtg)
	{
		if (currentPtg == null)
		{
			throw new ArgumentNullException("currentPtg");
		}
		if (!(currentPtg is IRangeGetter))
		{
			throw new ParseException("currentPtg");
		}
		if (!currentPtg.IsOperation && currentPtg.ToString(m_book.FormulaUtil, 0, 0, bR1C1: false).IndexOf("#REF") != -1)
		{
			return null;
		}
		return ((IRangeGetter)currentPtg).GetRange(m_book, null);
	}

	internal void SetInvertIfNegative(bool value)
	{
		m_bInvertIfNegative = value;
	}

	internal bool? GetInvertIfNegative()
	{
		return m_bInvertIfNegative;
	}
}
