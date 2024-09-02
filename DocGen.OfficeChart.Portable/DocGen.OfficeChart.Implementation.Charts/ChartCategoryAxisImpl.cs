using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartCategoryAxisImpl : ChartValueAxisImpl, IOfficeChartCategoryAxis, IOfficeChartValueAxis, IOfficeChartAxis
{
	private const string DEF_NOTSUPPORTED_PROPERTY = "This property is not supported for the current chart type";

	private const int DEF_AXIS_OFFSET = 100;

	private const int DEF_MONTH_COUNT = 12;

	private static readonly DateTime DEF_MIN_DATE = new DateTime(1900, 1, 1);

	internal bool m_xmlTKLabelSkipFrt;

	private bool m_isChartAxisOffsetRecord;

	internal HistogramAxisFormat m_histogramAxisFormat;

	private ChartCatserRangeRecord m_chartCatser;

	private UnknownRecord m_chartMlFrt;

	private ChartAxcextRecord m_axcetRecord = (ChartAxcextRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAxcext);

	private OfficeCategoryType m_categoryType = OfficeCategoryType.Automatic;

	private int m_iOffset = 100;

	private bool m_bAutoTickLabelSpacing = true;

	private bool m_bnoMultiLvlLbl;

	internal bool m_showNoMultiLvlLbl;

	private bool m_majorUnitIsAuto;

	private bool m_minorUnitIsAuto;

	private bool m_changeDateTimeAxisValue;

	public override bool IsMaxCross
	{
		get
		{
			if (IsChartBubbleOrScatter)
			{
				return base.IsMaxCross;
			}
			return m_chartCatser.IsMaxCross;
		}
		set
		{
			if (IsChartBubbleOrScatter)
			{
				base.IsMaxCross = value;
			}
			else
			{
				m_chartCatser.IsMaxCross = value;
			}
		}
	}

	public override double CrossesAt
	{
		get
		{
			if (!IsChartBubbleOrScatter)
			{
				return (int)m_chartCatser.CrossingPoint;
			}
			return base.CrossesAt;
		}
		set
		{
			if (IsChartBubbleOrScatter)
			{
				base.CrossesAt = value;
				return;
			}
			if ((value < 1.0 || value > 31999.0) && !base.ParentWorkbook.IsWorkbookOpening)
			{
				throw new ArgumentOutOfRangeException("For current chart type valid number must be between 1 to 3199");
			}
			m_chartCatser.CrossingPoint = (ushort)value;
			IsAutoCross = false;
		}
	}

	internal bool ChangeDateTimeAxisValue
	{
		get
		{
			return m_changeDateTimeAxisValue;
		}
		set
		{
			m_changeDateTimeAxisValue = value;
		}
	}

	internal bool HasAutoTickLabelSpacing
	{
		get
		{
			if (EnteredDirectlyCategoryLabels != null)
			{
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				object[] enteredDirectlyCategoryLabels = EnteredDirectlyCategoryLabels;
				foreach (object obj in enteredDirectlyCategoryLabels)
				{
					if (obj != null)
					{
						double result2;
						if (DateTime.TryParse(obj.ToString(), out var _))
						{
							flag = true;
						}
						else if (double.TryParse(obj.ToString(), out result2))
						{
							flag2 = true;
						}
						else
						{
							flag3 = true;
						}
					}
				}
				if (flag && !flag3 && CategoryType != 0)
				{
					return true;
				}
				if (flag2 && !flag3 && CategoryType == OfficeCategoryType.Time)
				{
					return true;
				}
				return false;
			}
			return false;
		}
	}

	public new bool AutoTickLabelSpacing
	{
		get
		{
			if (TickLabelSpacing != 1 || HasAutoTickLabelSpacing)
			{
				return false;
			}
			return m_bAutoTickLabelSpacing;
		}
		set
		{
			if (value)
			{
				TickLabelSpacing = 1;
			}
			m_bAutoTickLabelSpacing = value;
		}
	}

	internal int LabelFrequency
	{
		get
		{
			return TickLabelSpacing;
		}
		set
		{
			TickLabelSpacing = value;
		}
	}

	public int TickLabelSpacing
	{
		get
		{
			return m_chartCatser.LabelsFrequency;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "Value cannot be less than 0");
			}
			m_chartCatser.LabelsFrequency = (ushort)value;
			AutoTickLabelSpacing = false;
		}
	}

	internal int TickMarksFrequency
	{
		get
		{
			return TickMarkSpacing;
		}
		set
		{
			TickMarkSpacing = value;
			base.AutoTickMarkSpacing = false;
		}
	}

	public int TickMarkSpacing
	{
		get
		{
			return m_chartCatser.TickMarksFrequency;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "Value cannot be less than 0");
			}
			base.AutoTickMarkSpacing = false;
			m_chartCatser.TickMarksFrequency = (ushort)value;
		}
	}

	[CLSCompliant(false)]
	protected override ExcelObjectTextLink TextLinkType => ExcelObjectTextLink.XAxis;

	public bool IsBetween
	{
		get
		{
			return CatserRecord.IsBetween;
		}
		set
		{
			CatserRecord.IsBetween = value;
		}
	}

	public override bool ReversePlotOrder
	{
		get
		{
			if (IsChartBubbleOrScatter)
			{
				return base.ReversePlotOrder;
			}
			return CatserRecord.IsReverse;
		}
		set
		{
			if (IsChartBubbleOrScatter)
			{
				base.ReversePlotOrder = value;
				return;
			}
			CatserRecord.IsReverse = value;
			base.ReversePlotOrder = value;
		}
	}

	public IOfficeDataRange CategoryLabels
	{
		get
		{
			return new ChartDataRange(base.ParentAxis.ParentChart)
			{
				Range = CategoryLabelsIRange
			};
		}
		set
		{
			if ((value as ChartDataRange).Range == null)
			{
				CategoryLabelsIRange = null;
				return;
			}
			int firstRow = value.FirstRow;
			int firstColumn = value.FirstColumn;
			int lastRow = value.LastRow;
			int lastColumn = value.LastColumn;
			CategoryLabelsIRange = base.ParentAxis.ParentChart.Workbook.Worksheets[0][firstRow, firstColumn, lastRow, lastColumn];
		}
	}

	public IRange CategoryLabelsIRange
	{
		get
		{
			return (base.ParentChart.Series[0].CategoryLabels as ChartDataRange).Range;
		}
		set
		{
			ChartSeriesCollection chartSeriesCollection = (ChartSeriesCollection)base.ParentChart.Series;
			int i = 0;
			for (int count = chartSeriesCollection.Count; i < count; i++)
			{
				if (value == null)
				{
					chartSeriesCollection[i].CategoryLabels = new ChartDataRange(base.ParentAxis.ParentChart);
					((ChartDataRange)chartSeriesCollection[i].CategoryLabels).Range = null;
				}
				else
				{
					chartSeriesCollection[i].CategoryLabels = base.ParentAxis.ParentChart.ChartData[value.Row, value.Column, value.LastRow, value.LastColumn];
				}
			}
		}
	}

	public object[] DirectCategoryLabels
	{
		get
		{
			return EnteredDirectlyCategoryLabels;
		}
		set
		{
			EnteredDirectlyCategoryLabels = value;
		}
	}

	public object[] EnteredDirectlyCategoryLabels
	{
		get
		{
			if (base.ParentChart.Series.Count == 0)
			{
				return null;
			}
			return (base.ParentChart.Series[0] as ChartSerieImpl).EnteredDirectlyCategoryLabels;
		}
		set
		{
			ChartSeriesCollection chartSeriesCollection = (ChartSeriesCollection)base.ParentChart.Series;
			int i = 0;
			for (int count = chartSeriesCollection.Count; i < count; i++)
			{
				(chartSeriesCollection[i] as ChartSerieImpl).EnteredDirectlyCategoryLabels = value;
			}
		}
	}

	public OfficeCategoryType CategoryType
	{
		get
		{
			return m_categoryType;
		}
		set
		{
			m_categoryType = value;
		}
	}

	public int Offset
	{
		get
		{
			return m_iOffset;
		}
		set
		{
			if (value < 0 || value > 1000)
			{
				throw new ArgumentOutOfRangeException("The value can be from 0 through 1000.");
			}
			m_iOffset = value;
		}
	}

	public OfficeChartBaseUnit BaseUnit
	{
		get
		{
			CheckTimeScaleProperties();
			return m_axcetRecord.BaseUnits;
		}
		set
		{
			CheckTimeScaleProperties();
			m_axcetRecord.BaseUnits = value;
			m_axcetRecord.UseDefaultBaseUnits = false;
		}
	}

	public bool BaseUnitIsAuto
	{
		get
		{
			CheckTimeScaleProperties();
			return m_axcetRecord.UseDefaultBaseUnits;
		}
		set
		{
			CheckTimeScaleProperties();
			m_axcetRecord.UseDefaultBaseUnits = value;
		}
	}

	internal bool MajorUnitScaleIsAuto
	{
		get
		{
			return m_majorUnitIsAuto;
		}
		set
		{
			m_majorUnitIsAuto = false;
		}
	}

	internal bool MinorUnitScaleIsAuto
	{
		get
		{
			return m_minorUnitIsAuto;
		}
		set
		{
			m_minorUnitIsAuto = false;
		}
	}

	public override bool IsAutoMajor
	{
		get
		{
			if (IsChartBubbleOrScatter)
			{
				return base.IsAutoMajor;
			}
			if (IsCategoryType)
			{
				return true;
			}
			return m_axcetRecord.UseDefaultMajorUnits;
		}
		set
		{
			if (!IsChartBubbleOrScatter && !IsCategoryType && !base.ParentWorkbook.IsWorkbookOpening && !base.ParentWorkbook.IsLoaded && !base.ParentWorkbook.IsCreated && value != IsAutoMajor)
			{
				throw new NotSupportedException("This property is not supported for the current chart type");
			}
			base.IsAutoMajor = value;
			m_axcetRecord.UseDefaultMajorUnits = value;
		}
	}

	public override bool IsAutoMinor
	{
		get
		{
			if (IsChartBubbleOrScatter)
			{
				return base.IsAutoMinor;
			}
			if (IsCategoryType)
			{
				return true;
			}
			return m_axcetRecord.UseDefaultMinorUnits;
		}
		set
		{
			if (!IsChartBubbleOrScatter && !IsCategoryType && !base.ParentWorkbook.IsWorkbookOpening && !base.ParentWorkbook.IsLoaded && value != IsAutoMinor)
			{
				throw new NotSupportedException("This property is not supported for the current chart type");
			}
			base.IsAutoMinor = value;
			m_axcetRecord.UseDefaultMinorUnits = value;
		}
	}

	public override bool IsAutoCross
	{
		get
		{
			if (IsChartBubbleOrScatter)
			{
				return base.IsAutoCross;
			}
			return m_axcetRecord.UseDefaultCrossPoint;
		}
		set
		{
			base.IsAutoCross = value;
			if (!IsChartBubbleOrScatter)
			{
				m_axcetRecord.UseDefaultCrossPoint = value;
				base.IsAutoCross = value;
			}
		}
	}

	public override bool IsAutoMax
	{
		get
		{
			if (IsChartBubbleOrScatter)
			{
				return base.IsAutoMax;
			}
			if (IsCategoryType)
			{
				return true;
			}
			return m_axcetRecord.UseDefaultMaximum;
		}
		set
		{
			SetAutoMax(check: false, value);
			m_axcetRecord.UseDefaultMaximum = value;
			if (!IsChartBubbleOrScatter && IsCategoryType && !base.ParentWorkbook.IsWorkbookOpening)
			{
				throw new NotSupportedException("This property is not supported for the current chart type");
			}
		}
	}

	public override bool IsAutoMin
	{
		get
		{
			if (IsChartBubbleOrScatter)
			{
				return base.IsAutoMin;
			}
			if (IsCategoryType)
			{
				return true;
			}
			return m_axcetRecord.UseDefaultMinimum;
		}
		set
		{
			SetAutoMin(check: false, value);
			m_axcetRecord.UseDefaultMinimum = value;
			if (!IsChartBubbleOrScatter && IsCategoryType && !base.ParentWorkbook.IsWorkbookOpening)
			{
				throw new NotSupportedException("This property is not supported for the current chart type");
			}
		}
	}

	public override double MajorUnit
	{
		get
		{
			if (IsChartBubbleOrScatter)
			{
				return base.MajorUnit;
			}
			if (IsCategoryType)
			{
				throw new NotSupportedException("This property is not supported for the current chart type");
			}
			return (int)m_axcetRecord.Major;
		}
		set
		{
			if (IsChartBubbleOrScatter)
			{
				base.MajorUnit = value;
				return;
			}
			if (IsCategoryType)
			{
				throw new NotSupportedException("This property is not supported for the current chart type");
			}
			if (value < 1.0 || (!IsAutoMinor && value < MinorUnit))
			{
				throw new ArgumentOutOfRangeException("MajorUnit");
			}
			m_majorUnitIsAuto = false;
			m_axcetRecord.Major = (ushort)value;
			m_axcetRecord.UseDefaultMajorUnits = false;
		}
	}

	public override double MinorUnit
	{
		get
		{
			if (IsChartBubbleOrScatter)
			{
				return base.MinorUnit;
			}
			if (IsCategoryType)
			{
				throw new NotSupportedException("This property is not supported for the current chart type");
			}
			return (int)m_axcetRecord.Minor;
		}
		set
		{
			if (IsChartBubbleOrScatter)
			{
				base.MinorUnit = value;
				return;
			}
			if (IsCategoryType)
			{
				throw new NotSupportedException("This property is not supported for the current chart type");
			}
			if ((value < 1.0 || (!IsAutoMajor && value > MajorUnit)) && !base.ParentWorkbook.IsWorkbookOpening)
			{
				throw new ArgumentOutOfRangeException("MinorUnit");
			}
			m_axcetRecord.Minor = (ushort)value;
			m_axcetRecord.UseDefaultMinorUnits = false;
			m_minorUnitIsAuto = false;
		}
	}

	public OfficeChartBaseUnit MajorUnitScale
	{
		get
		{
			CheckTimeScaleProperties();
			return m_axcetRecord.MajorUnits;
		}
		set
		{
			CheckTimeScaleProperties();
			if (!IsAutoMinor && value < MinorUnitScale)
			{
				throw new ArgumentOutOfRangeException("This property is not supported for the current chart type");
			}
			m_majorUnitIsAuto = false;
			m_axcetRecord.MajorUnits = value;
		}
	}

	public OfficeChartBaseUnit MinorUnitScale
	{
		get
		{
			CheckTimeScaleProperties();
			return m_axcetRecord.MinorUnits;
		}
		set
		{
			CheckTimeScaleProperties();
			if (!IsAutoMajor && MajorUnitScale < value)
			{
				throw new ArgumentOutOfRangeException("This property is not supported for the current chart type");
			}
			m_minorUnitIsAuto = false;
			m_axcetRecord.MinorUnits = value;
		}
	}

	public bool NoMultiLevelLabel
	{
		get
		{
			return m_bnoMultiLvlLbl;
		}
		set
		{
			m_bnoMultiLvlLbl = value;
		}
	}

	public bool IsBinningByCategory
	{
		get
		{
			return HistogramAxisFormatProperty.IsBinningByCategory;
		}
		set
		{
			HistogramAxisFormatProperty.IsBinningByCategory = value;
		}
	}

	public bool HasAutomaticBins
	{
		get
		{
			return HistogramAxisFormatProperty.HasAutomaticBins;
		}
		set
		{
			HistogramAxisFormatProperty.HasAutomaticBins = value;
		}
	}

	public int NumberOfBins
	{
		get
		{
			return HistogramAxisFormatProperty.NumberOfBins;
		}
		set
		{
			if (!base.ParentWorkbook.IsLoaded && value > 31999)
			{
				value = 31999;
			}
			HistogramAxisFormatProperty.NumberOfBins = value;
		}
	}

	public double BinWidth
	{
		get
		{
			return HistogramAxisFormatProperty.BinWidth;
		}
		set
		{
			HistogramAxisFormatProperty.BinWidth = value;
		}
	}

	public double UnderflowBinValue
	{
		get
		{
			return HistogramAxisFormatProperty.UnderflowBinValue;
		}
		set
		{
			HistogramAxisFormatProperty.UnderflowBinValue = value;
		}
	}

	public double OverflowBinValue
	{
		get
		{
			return HistogramAxisFormatProperty.OverflowBinValue;
		}
		set
		{
			HistogramAxisFormatProperty.OverflowBinValue = value;
		}
	}

	internal HistogramAxisFormat HistogramAxisFormatProperty
	{
		get
		{
			if (m_histogramAxisFormat == null)
			{
				m_histogramAxisFormat = new HistogramAxisFormat();
			}
			return m_histogramAxisFormat;
		}
	}

	private ChartCatserRangeRecord CatserRecord
	{
		get
		{
			if (m_chartCatser == null)
			{
				m_chartCatser = (ChartCatserRangeRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartCatserRange);
			}
			return m_chartCatser;
		}
	}

	internal bool IsChartBubbleOrScatter
	{
		get
		{
			string text = string.Empty;
			IOfficeChartSeries series = base.ParentChart.Series;
			if (series.Count == 0)
			{
				text = ChartFormatImpl.GetStartSerieType(base.ParentChart.ChartType);
			}
			else
			{
				bool isPrimary = (base.Parent as ChartParentAxisImpl).ChartFormats.IsPrimary;
				for (int i = 0; i < series.Count; i++)
				{
					if (isPrimary == series[i].UsePrimaryAxis)
					{
						string startType = (series[i] as ChartSerieImpl).StartType;
						if (text == string.Empty)
						{
							text = startType;
						}
						else if (text != startType)
						{
							text = ChartFormatImpl.GetStartSerieType(OfficeChartType.Combination_Chart);
							break;
						}
					}
				}
			}
			if (!(text == "Bubble"))
			{
				return text == "Scatter";
			}
			return true;
		}
	}

	private bool IsCategoryType => m_categoryType == OfficeCategoryType.Category;

	public ChartCategoryAxisImpl(IApplication application, object parent)
		: base(application, parent)
	{
		base.AxisId = (base.IsPrimary ? 59983360 : 62908672);
		m_majorUnitIsAuto = true;
		m_minorUnitIsAuto = true;
	}

	public ChartCategoryAxisImpl(IApplication application, object parent, OfficeAxisType axisType)
		: this(application, parent, axisType, bIsPrimary: true)
	{
		m_majorUnitIsAuto = true;
		m_minorUnitIsAuto = true;
	}

	public ChartCategoryAxisImpl(IApplication application, object parent, OfficeAxisType axisType, bool bIsPrimary)
		: base(application, parent, axisType, bIsPrimary)
	{
		base.AxisId = (base.IsPrimary ? 59983360 : 62908672);
		if (!base.IsPrimary)
		{
			base.Visible = false;
		}
		m_majorUnitIsAuto = true;
		m_minorUnitIsAuto = true;
	}

	[CLSCompliant(false)]
	public ChartCategoryAxisImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: this(application, parent, data, ref iPos, isPrimary: true)
	{
		m_majorUnitIsAuto = true;
		m_minorUnitIsAuto = true;
	}

	[CLSCompliant(false)]
	public ChartCategoryAxisImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos, bool isPrimary)
		: base(application, parent, data, ref iPos, isPrimary)
	{
		base.AxisId = (base.IsPrimary ? 59983360 : 62908672);
		m_majorUnitIsAuto = true;
		m_minorUnitIsAuto = true;
	}

	[CLSCompliant(false)]
	protected override void ParseData(BiffRecordRaw record, IList<BiffRecordRaw> data, ref int iPos)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		switch (record.TypeCode)
		{
		case TBIFFRecord.ChartAxcext:
			m_axcetRecord = (ChartAxcextRecord)record;
			ParseCategoryType(m_axcetRecord);
			break;
		case TBIFFRecord.ChartAxisOffset:
			m_iOffset = ((ChartAxisOffsetRecord)record).Offset;
			break;
		case TBIFFRecord.ChartValueRange:
		case TBIFFRecord.ChartCatserRange:
			ParseMaxCross(record);
			break;
		case TBIFFRecord.ChartMlFrt:
			m_chartMlFrt = (UnknownRecord)record;
			break;
		default:
			base.ParseData(record, data, ref iPos);
			break;
		}
	}

	[CLSCompliant(false)]
	protected override void ParseMaxCross(BiffRecordRaw record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		switch (record.TypeCode)
		{
		case TBIFFRecord.ChartCatserRange:
			m_chartCatser = (ChartCatserRangeRecord)record;
			break;
		case TBIFFRecord.ChartValueRange:
			base.ChartValueRange = (ChartValueRangeRecord)record;
			break;
		default:
			throw new ApplicationException("Unknown record type");
		}
	}

	protected override void ParseWallsOrFloor(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		base.ParentChart.Walls = new ChartWallOrFloorImpl(base.Application, base.ParentChart, bWalls: true, data, ref iPos);
	}

	private void ParseCategoryType(ChartAxcextRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (record.UseDefaultDateSettings)
		{
			m_categoryType = OfficeCategoryType.Automatic;
		}
		else
		{
			m_categoryType = (record.IsDateAxis ? OfficeCategoryType.Time : OfficeCategoryType.Category);
		}
	}

	[CLSCompliant(false)]
	public override void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (IsChartBubbleOrScatter)
		{
			Serialize(records, ChartAxisRecord.ChartAxisType.CategoryAxis);
		}
		else
		{
			SerializeCategory(records);
		}
	}

	private void SerializeCategory(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		ChartAxisRecord chartAxisRecord = (ChartAxisRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAxis);
		chartAxisRecord.AxisType = ChartAxisRecord.ChartAxisType.CategoryAxis;
		records.Add(chartAxisRecord);
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		records.Add((BiffRecordRaw)m_chartCatser.Clone());
		SerializeAxcetRecord(records);
		SerializeNumberFormat(records);
		ChartAxisOffsetRecord chartAxisOffsetRecord = (ChartAxisOffsetRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAxisOffset);
		chartAxisOffsetRecord.Offset = Offset;
		records.Add(chartAxisOffsetRecord);
		SerializeTickRecord(records);
		SerializeFont(records);
		SerializeAxisBorder(records);
		if (m_chartMlFrt != null)
		{
			records.Add(m_chartMlFrt);
		}
		else if (AutoTickLabelSpacing)
		{
			UnknownRecord unknownRecord = new UnknownRecord();
			unknownRecord.RecordCode = 2206;
			unknownRecord.m_data = new byte[32]
			{
				158, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 12, 0, 0, 0, 0, 0, 4, 0,
				4, 0, 81, 0, 1, 0, 0, 0, 0, 0,
				0, 0
			};
			unknownRecord.Length = unknownRecord.m_data.Length;
			records.Add(unknownRecord);
		}
		else
		{
			UnknownRecord unknownRecord2 = new UnknownRecord();
			unknownRecord2.RecordCode = 2206;
			unknownRecord2.m_data = new byte[32]
			{
				158, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 12, 0, 0, 0, 0, 0, 4, 0,
				4, 0, 82, 0, 1, 0, 0, 0, 0, 0,
				0, 0
			};
			unknownRecord2.Length = unknownRecord2.m_data.Length;
			records.Add(unknownRecord2);
		}
		if (base.IsPrimary)
		{
			SerializeGridLines(records);
			SerializeWallsOrFloor(records);
		}
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
	}

	[CLSCompliant(false)]
	protected override void SerializeWallsOrFloor(OffsetArrayList records)
	{
		base.ParentChart.SerializeWalls(records);
	}

	private void SerializeAxcetRecord(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		m_axcetRecord.UseDefaultDateSettings = m_categoryType == OfficeCategoryType.Automatic;
		m_axcetRecord.IsDateAxis = m_categoryType == OfficeCategoryType.Time;
		records.Add(m_axcetRecord);
	}

	protected override void InitializeVariables()
	{
		m_chartCatser = (ChartCatserRangeRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartCatserRange);
		base.ChartValueRange = (ChartValueRangeRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartValueRange);
		base.InitializeVariables();
	}

	protected override bool CheckValueRangeRecord(bool throwException)
	{
		bool isChartBubbleOrScatter = IsChartBubbleOrScatter;
		if (throwException && !isChartBubbleOrScatter)
		{
			throw new NotSupportedException("This property is not supported for the current chart type");
		}
		return isChartBubbleOrScatter;
	}

	public override ChartAxisImpl Clone(object parent, Dictionary<int, int> dicFontIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		ChartCategoryAxisImpl chartCategoryAxisImpl = (ChartCategoryAxisImpl)base.Clone(parent, dicFontIndexes, dicNewSheetNames);
		if (m_chartCatser != null)
		{
			chartCategoryAxisImpl.m_chartCatser = (ChartCatserRangeRecord)m_chartCatser.Clone();
		}
		if (m_axcetRecord != null)
		{
			chartCategoryAxisImpl.m_axcetRecord = (ChartAxcextRecord)m_axcetRecord.Clone();
		}
		chartCategoryAxisImpl.m_categoryType = m_categoryType;
		if (m_chartMlFrt != null)
		{
			chartCategoryAxisImpl.m_chartMlFrt = (UnknownRecord)m_chartMlFrt.Clone();
		}
		return chartCategoryAxisImpl;
	}

	private string GetStartChartType()
	{
		IOfficeChartSeries series = base.ParentChart.Series;
		if (series.Count == 0)
		{
			return ChartFormatImpl.GetStartSerieType(base.ParentChart.ChartType);
		}
		string startType = (series[0] as ChartSerieImpl).StartType;
		int i = 1;
		for (int count = series.Count; i < count; i++)
		{
			if (((ChartSerieImpl)series[i]).StartType != startType)
			{
				return ChartFormatImpl.GetStartSerieType(OfficeChartType.Combination_Chart);
			}
		}
		return startType;
	}

	private void CheckTimeScaleProperties()
	{
		if (IsCategoryType || IsChartBubbleOrScatter)
		{
			throw new NotSupportedException("Current chart doesnot support this property.");
		}
	}

	internal void SwapAxisValues()
	{
		if (m_chartCatser.IsMaxCross)
		{
			base.IsMaxCross = m_chartCatser.IsMaxCross;
		}
		else if (m_axcetRecord.UseDefaultCrossPoint)
		{
			base.IsAutoCross = m_axcetRecord.UseDefaultCrossPoint;
		}
		else
		{
			base.CrossesAt = (int)m_chartCatser.CrossingPoint;
		}
		base.ReversePlotOrder = m_chartCatser.IsReverse;
	}
}
