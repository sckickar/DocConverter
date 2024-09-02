using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal abstract class ChartAxisImpl : CommonObject, IOfficeChartAxis
{
	protected const int DEF_NUMBER_FORMAT_INDEX = -1;

	private const int DEF_GENERAL_FORMAT = 0;

	private bool m_isWrapText;

	private OfficeAxisType m_axisType;

	private bool m_bPrimary;

	protected ChartTextAreaImpl m_titleArea;

	private ChartTickRecord m_chartTick;

	private ShadowImpl m_shadow;

	private bool m_bLineFormat;

	private FontWrapper m_font;

	private FontWrapper old_Font;

	protected ChartGridLineImpl m_majorGrid;

	protected ChartGridLineImpl m_minorGrid;

	private bool m_bHasMajor;

	private bool m_bHasMinor;

	private ChartParentAxisImpl m_parentAxis;

	private int m_iNumberFormat = -1;

	private ChartBorderImpl m_border;

	private OfficeAxisTextDirection m_textDirection;

	private int m_iAxisId;

	private bool m_bDeleted;

	private bool m_bAutoTickLabelSpacing = true;

	private bool m_bAutoTickMarkSpacing = true;

	internal AxisLabelAlignment LabelAlign = AxisLabelAlignment.Center;

	private AxisLabelAlignment m_labelTextAlign = AxisLabelAlignment.Center;

	private ThreeDFormatImpl m_3D;

	private ChartAxisPos? m_axisPos;

	private bool m_sourceLinked = true;

	private Stream m_textStream;

	private ChartParagraphType m_paraType;

	private ChartFrameFormatImpl m_axisFormat;

	private bool m_IsDefaultTextSettings;

	private bool m_isChartFont;

	internal Excel2007TextRotation m_textRotation;

	internal bool IsWrapText
	{
		get
		{
			return m_isWrapText;
		}
		set
		{
			m_isWrapText = value;
		}
	}

	public OfficeAxisType AxisType
	{
		get
		{
			return m_axisType;
		}
		set
		{
			m_axisType = value;
		}
	}

	public bool IsPrimary
	{
		get
		{
			return m_bPrimary;
		}
		set
		{
			m_bPrimary = value;
		}
	}

	public string Title
	{
		get
		{
			if (m_titleArea == null)
			{
				return null;
			}
			return m_titleArea.Text;
		}
		set
		{
			TitleArea.Text = value;
		}
	}

	public int TextRotationAngle
	{
		get
		{
			if (ParentWorkbook.Version == OfficeVersion.Excel97to2003)
			{
				return m_chartTick.RotationAngle;
			}
			return -m_chartTick.RotationAngle;
		}
		set
		{
			if (value == 0)
			{
				IsWrapText = true;
			}
			if (value < -90 || value > 90)
			{
				m_chartTick.RotationAngle = 0;
				return;
			}
			bool flag = ParentWorkbook.Version != OfficeVersion.Excel97to2003;
			m_chartTick.RotationAngle = (short)(flag ? (-value) : value);
			m_chartTick.IsAutoRotation = false;
		}
	}

	public bool IsAutoTextRotation => m_chartTick.IsAutoRotation;

	public IOfficeChartTextArea TitleArea
	{
		get
		{
			if (m_titleArea == null)
			{
				m_titleArea = new ChartTextAreaImpl(base.Application, this, TextLinkType);
				m_titleArea.Bold = true;
				m_titleArea.Size = 10.0;
			}
			return m_titleArea;
		}
	}

	public IOfficeFont Font
	{
		get
		{
			if (m_font == null)
			{
				FontImpl font = (FontImpl)ParentWorkbook.InnerFonts[0];
				m_font = new FontWrapper(font);
			}
			if (!IsChartFont)
			{
				IsDefaultTextSettings = false;
				m_paraType = ChartParagraphType.CustomDefault;
			}
			return m_font;
		}
		set
		{
			if (value != m_font)
			{
				IsDefaultTextSettings = false;
				m_font = (FontWrapper)value;
				m_isChartFont = false;
			}
		}
	}

	internal bool IsChartFont
	{
		get
		{
			return m_isChartFont;
		}
		set
		{
			m_isChartFont = value;
		}
	}

	public IOfficeChartGridLine MajorGridLines => m_majorGrid;

	public IOfficeChartGridLine MinorGridLines => m_minorGrid;

	public bool HasMinorGridLines
	{
		get
		{
			return m_bHasMinor;
		}
		set
		{
			if (value != HasMinorGridLines)
			{
				ChartImpl parentChart = m_parentAxis.ParentChart;
				if (!parentChart.TypeChanging && !parentChart.CheckForSupportGridLine())
				{
					throw new ApplicationException("This chart type does not support gridlines.");
				}
				m_bHasMinor = value;
				m_minorGrid = (value ? new ChartGridLineImpl(base.Application, this, ExcelAxisLineIdentifier.MinorGridLine) : null);
			}
		}
	}

	public bool HasMajorGridLines
	{
		get
		{
			return m_bHasMajor;
		}
		set
		{
			if (value != HasMajorGridLines)
			{
				ChartImpl parentChart = m_parentAxis.ParentChart;
				if (!parentChart.TypeChanging && !parentChart.CheckForSupportGridLine())
				{
					throw new ApplicationException("This chart type does not support gridlines.");
				}
				m_bHasMajor = value;
				m_majorGrid = (value ? new ChartGridLineImpl(base.Application, this, ExcelAxisLineIdentifier.MajorGridLine) : null);
			}
		}
	}

	public bool isNumber => m_iNumberFormat != -1;

	internal ChartParentAxisImpl ParentAxis => m_parentAxis;

	public int NumberFormatIndex
	{
		get
		{
			return m_iNumberFormat;
		}
		set
		{
			m_iNumberFormat = value;
		}
	}

	public string NumberFormat
	{
		get
		{
			int iIndex = m_iNumberFormat;
			FormatsCollection innerFormats = ParentWorkbook.InnerFormats;
			if (m_iNumberFormat == -1 || !innerFormats.Contains(m_iNumberFormat))
			{
				iIndex = 0;
			}
			return innerFormats[iIndex].FormatString;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("formatString");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("value - string cannot be empty");
			}
			if (!ParentWorkbook.IsLoaded && IsSourceLinked)
			{
				IsSourceLinked = false;
			}
			m_iNumberFormat = ParentWorkbook.InnerFormats.FindOrCreateFormat(value);
		}
	}

	public OfficeTickMark MinorTickMark
	{
		get
		{
			return m_chartTick.MinorMark;
		}
		set
		{
			m_chartTick.MinorMark = value;
		}
	}

	public OfficeTickMark MajorTickMark
	{
		get
		{
			return m_chartTick.MajorMark;
		}
		set
		{
			m_chartTick.MajorMark = value;
		}
	}

	public IOfficeChartBorder Border
	{
		get
		{
			if (m_border == null)
			{
				m_border = new ChartBorderImpl(base.Application, this);
			}
			return m_border;
		}
	}

	public OfficeTickLabelPosition TickLabelPosition
	{
		get
		{
			return m_chartTick.LabelPos;
		}
		set
		{
			m_chartTick.LabelPos = value;
		}
	}

	public bool Visible
	{
		get
		{
			return !Deleted;
		}
		set
		{
			if (value != Visible)
			{
				Border.LinePattern = ((!value) ? OfficeChartLinePattern.None : OfficeChartLinePattern.Solid);
				Deleted = !value;
			}
		}
	}

	internal OfficeAxisTextDirection Alignment
	{
		get
		{
			return m_textDirection;
		}
		set
		{
			m_textDirection = value;
		}
	}

	public abstract bool ReversePlotOrder { get; set; }

	public int AxisId
	{
		get
		{
			return m_iAxisId;
		}
		internal set
		{
			m_iAxisId = value;
		}
	}

	public ChartImpl ParentChart => m_parentAxis.m_parentChart;

	public bool Deleted
	{
		get
		{
			return m_bDeleted;
		}
		set
		{
			m_bDeleted = value;
		}
	}

	public bool AutoTickLabelSpacing
	{
		get
		{
			return m_bAutoTickLabelSpacing;
		}
		set
		{
			m_bAutoTickLabelSpacing = value;
		}
	}

	public bool AutoTickMarkSpacing
	{
		get
		{
			return m_bAutoTickMarkSpacing;
		}
		set
		{
			m_bAutoTickMarkSpacing = value;
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

	public IShadow ShadowProperties => Shadow;

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

	public IThreeDFormat Chart3DProperties => Chart3DOptions;

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

	internal ChartAxisPos? AxisPosition
	{
		get
		{
			return m_axisPos;
		}
		set
		{
			m_axisPos = value;
		}
	}

	internal bool IsSourceLinked
	{
		get
		{
			return m_sourceLinked;
		}
		set
		{
			m_sourceLinked = value;
		}
	}

	internal Stream TextStream
	{
		get
		{
			return m_textStream;
		}
		set
		{
			m_textStream = value;
		}
	}

	public IOfficeChartFrameFormat FrameFormat
	{
		get
		{
			if (m_axisFormat == null)
			{
				InitFrameFormat();
			}
			return m_axisFormat;
		}
	}

	public bool HasAxisTitle => m_titleArea != null;

	public ChartParagraphType ParagraphType
	{
		get
		{
			return m_paraType;
		}
		set
		{
			m_paraType = value;
		}
	}

	internal bool IsDefaultTextSettings
	{
		get
		{
			return m_IsDefaultTextSettings;
		}
		set
		{
			m_IsDefaultTextSettings = value;
		}
	}

	internal AxisLabelAlignment LabelTextAlign
	{
		get
		{
			return m_labelTextAlign;
		}
		set
		{
			m_labelTextAlign = value;
		}
	}

	protected abstract ExcelObjectTextLink TextLinkType { get; }

	protected WorkbookImpl ParentWorkbook => m_parentAxis.m_parentChart.InnerWorkbook;

	public ChartAxisImpl(IApplication application, object parent)
		: base(application, parent)
	{
		InitializeVariables();
	}

	public ChartAxisImpl(IApplication application, object parent, OfficeAxisType axisType)
		: this(application, parent, axisType, bIsPrimary: true)
	{
	}

	public ChartAxisImpl(IApplication application, object parent, OfficeAxisType axisType, bool bIsPrimary)
		: base(application, parent)
	{
		m_axisType = axisType;
		m_bPrimary = bIsPrimary;
		InitializeVariables();
	}

	[CLSCompliant(false)]
	public ChartAxisImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: this(application, parent, data, ref iPos, isPrimary: true)
	{
	}

	[CLSCompliant(false)]
	public ChartAxisImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos, bool isPrimary)
		: this(application, parent)
	{
		Parse(data, ref iPos, isPrimary);
	}

	private void SetParents()
	{
		m_parentAxis = (ChartParentAxisImpl)FindParent(typeof(ChartParentAxisImpl));
		if (m_parentAxis == null)
		{
			throw new ArgumentNullException("There is no parent axis.");
		}
	}

	[CLSCompliant(false)]
	protected void Parse(IList<BiffRecordRaw> data, ref int iPos, bool isPrimary)
	{
		m_bPrimary = isPrimary;
		ParagraphType = ChartParagraphType.CustomDefault;
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartAxis);
		ChartAxisRecord chartAxisRecord = (ChartAxisRecord)biffRecordRaw;
		iPos++;
		biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.Begin);
		while (biffRecordRaw.TypeCode != TBIFFRecord.End)
		{
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.ChartAxisLineFormat:
				ParseChartAxisLineFormat(data, ref iPos);
				break;
			case TBIFFRecord.ChartIfmt:
				ParseIfmt(biffRecordRaw as ChartIfmtRecord);
				iPos++;
				break;
			case TBIFFRecord.ChartTick:
				ParseTickRecord((ChartTickRecord)data[iPos]);
				iPos++;
				break;
			case TBIFFRecord.ChartFontx:
			{
				ChartFontxRecord fontx = (ChartFontxRecord)data[iPos];
				ParseFontXRecord(fontx);
				iPos++;
				break;
			}
			default:
				ParseData(biffRecordRaw, data, ref iPos);
				iPos++;
				break;
			}
			biffRecordRaw = data[iPos];
		}
		iPos++;
		switch (chartAxisRecord.AxisType)
		{
		case ChartAxisRecord.ChartAxisType.CategoryAxis:
			m_axisType = OfficeAxisType.Category;
			break;
		case ChartAxisRecord.ChartAxisType.ValueAxis:
			m_axisType = OfficeAxisType.Value;
			break;
		case ChartAxisRecord.ChartAxisType.SeriesAxis:
			m_axisType = OfficeAxisType.Serie;
			break;
		}
	}

	private void ParseChartAxisLineFormat(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		switch (((ChartAxisLineFormatRecord)data[iPos]).LineIdentifier)
		{
		case ExcelAxisLineIdentifier.MajorGridLine:
			m_bHasMajor = true;
			m_majorGrid = new ChartGridLineImpl(base.Application, this, data, ref iPos);
			break;
		case ExcelAxisLineIdentifier.MinorGridLine:
			m_bHasMinor = true;
			m_minorGrid = new ChartGridLineImpl(base.Application, this, data, ref iPos);
			break;
		case ExcelAxisLineIdentifier.WallsOrFloor:
			ParseWallsOrFloor(data, ref iPos);
			break;
		case ExcelAxisLineIdentifier.AxisLineItself:
			iPos++;
			if (data[iPos].TypeCode == TBIFFRecord.ChartLineFormat)
			{
				m_border = new ChartBorderImpl(base.Application, this, data, ref iPos);
			}
			break;
		default:
			throw new NotSupportedException("Unknown line identifier.");
		}
	}

	[CLSCompliant(false)]
	protected void ParseFontXRecord(ChartFontxRecord fontx)
	{
		if (fontx == null)
		{
			throw new ArgumentNullException("fontx");
		}
		int fontIndex = fontx.FontIndex;
		FontImpl font = (FontImpl)ParentWorkbook.InnerFonts[fontIndex];
		m_font = new FontWrapper(font);
	}

	protected abstract void ParseWallsOrFloor(IList<BiffRecordRaw> data, ref int iPos);

	[CLSCompliant(false)]
	protected void ParseIfmt(ChartIfmtRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		NumberFormatIndex = record.FormatIndex;
	}

	[CLSCompliant(false)]
	protected virtual void ParseData(BiffRecordRaw record, IList<BiffRecordRaw> data, ref int iPos)
	{
	}

	private void ParseTickRecord(ChartTickRecord chartTick)
	{
		if (chartTick == null)
		{
			throw new ArgumentNullException("chartTick");
		}
		m_chartTick = chartTick;
		m_textDirection = OfficeAxisTextDirection.Context;
		if (chartTick.IsLeftToRight)
		{
			m_textDirection = OfficeAxisTextDirection.LeftToRight;
		}
		else if (chartTick.IsRightToLeft)
		{
			m_textDirection = OfficeAxisTextDirection.RightToLeft;
		}
	}

	[CLSCompliant(false)]
	public virtual void Serialize(OffsetArrayList records)
	{
		throw new NotSupportedException("This method should not be called.");
	}

	[CLSCompliant(false)]
	public void SerializeAxisTitle(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_titleArea != null)
		{
			m_titleArea.Serialize(records);
		}
	}

	[CLSCompliant(false)]
	protected void SerializeFont(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		List<int> list = null;
		if (ParentWorkbook.ArrayFontIndex != null)
		{
			list = ParentWorkbook.ArrayFontIndex;
		}
		if (m_font != null)
		{
			ChartFontxRecord chartFontxRecord = (ChartFontxRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartFontx);
			if (list != null)
			{
				chartFontxRecord.FontIndex = (ushort)list[m_font.Index];
			}
			else
			{
				chartFontxRecord.FontIndex = (ushort)m_font.Index;
			}
			records.Add(chartFontxRecord);
		}
	}

	[CLSCompliant(false)]
	protected void SerializeGridLines(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_majorGrid != null)
		{
			m_majorGrid.Serialize(records);
		}
		if (m_minorGrid != null)
		{
			m_minorGrid.Serialize(records);
		}
	}

	[CLSCompliant(false)]
	protected void SerializeNumberFormat(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (NumberFormatIndex != -1)
		{
			ChartIfmtRecord chartIfmtRecord = (ChartIfmtRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartIfmt);
			chartIfmtRecord.FormatIndex = (ushort)NumberFormatIndex;
			records.Add(chartIfmtRecord);
		}
	}

	[CLSCompliant(false)]
	protected void SerializeAxisBorder(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_border != null)
		{
			ChartAxisLineFormatRecord chartAxisLineFormatRecord = (ChartAxisLineFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAxisLineFormat);
			chartAxisLineFormatRecord.LineIdentifier = ExcelAxisLineIdentifier.AxisLineItself;
			records.Add(chartAxisLineFormatRecord);
			m_border.Serialize(records);
		}
	}

	[CLSCompliant(false)]
	protected void SerializeTickRecord(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		ChartTickRecord chartTickRecord = (ChartTickRecord)m_chartTick.Clone();
		chartTickRecord.IsLeftToRight = false;
		chartTickRecord.IsRightToLeft = false;
		if (Alignment == OfficeAxisTextDirection.LeftToRight)
		{
			chartTickRecord.IsLeftToRight = true;
		}
		else if (Alignment == OfficeAxisTextDirection.RightToLeft)
		{
			chartTickRecord.IsRightToLeft = true;
		}
		records.Add(chartTickRecord);
	}

	protected virtual void InitializeVariables()
	{
		SetParents();
		m_paraType = ChartParagraphType.Default;
		InitializeTickRecord();
	}

	private void InitializeTickRecord()
	{
		m_chartTick = (ChartTickRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartTick);
		m_chartTick.MajorMark = OfficeTickMark.TickMark_Outside;
		m_chartTick.LabelPos = OfficeTickLabelPosition.TickLabelPosition_NextToAxis;
		m_chartTick.IsAutoTextColor = true;
	}

	protected internal void SetTitleArea(ChartTextAreaImpl titleArea)
	{
		if (titleArea == null)
		{
			throw new ArgumentNullException("titleArea");
		}
		m_titleArea = titleArea;
	}

	public virtual ChartAxisImpl Clone(object parent, Dictionary<int, int> dicFontIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		ChartAxisImpl chartAxisImpl = (ChartAxisImpl)MemberwiseClone();
		chartAxisImpl.SetParent(parent);
		chartAxisImpl.SetParents();
		chartAxisImpl.NumberFormat = NumberFormat;
		chartAxisImpl.m_axisType = m_axisType;
		chartAxisImpl.m_bIsDisposed = m_bIsDisposed;
		chartAxisImpl.m_bLineFormat = m_bLineFormat;
		chartAxisImpl.m_bPrimary = m_bPrimary;
		if (m_axisFormat != null)
		{
			chartAxisImpl.m_axisFormat = m_axisFormat.Clone(chartAxisImpl);
		}
		if (m_chartTick != null)
		{
			chartAxisImpl.m_chartTick = (ChartTickRecord)m_chartTick.Clone();
		}
		if (m_border != null)
		{
			chartAxisImpl.m_border = m_border.Clone(chartAxisImpl);
		}
		if (m_titleArea != null)
		{
			chartAxisImpl.m_titleArea = (ChartTextAreaImpl)m_titleArea.Clone(chartAxisImpl, dicFontIndexes, dicNewSheetNames);
		}
		if (m_majorGrid != null)
		{
			chartAxisImpl.m_majorGrid = (ChartGridLineImpl)m_majorGrid.Clone(chartAxisImpl);
		}
		if (m_minorGrid != null)
		{
			chartAxisImpl.m_minorGrid = (ChartGridLineImpl)m_minorGrid.Clone(chartAxisImpl);
		}
		if (m_font != null)
		{
			chartAxisImpl.m_font = m_font.Clone(chartAxisImpl.ParentWorkbook, this, dicFontIndexes);
			_ = m_font.RGBColor;
			chartAxisImpl.m_font.RGBColor = m_font.RGBColor;
		}
		if (m_textStream != null)
		{
			byte[] array = new byte[m_textStream.Length];
			int count;
			while ((count = m_textStream.Read(array, 0, array.Length)) > 0)
			{
				chartAxisImpl.m_textStream.Write(array, 0, count);
			}
		}
		if (m_shadow != null)
		{
			chartAxisImpl.m_shadow = m_shadow.Clone(chartAxisImpl);
		}
		chartAxisImpl.m_textDirection = m_textDirection;
		chartAxisImpl.m_paraType = m_paraType;
		if (m_axisPos.HasValue)
		{
			chartAxisImpl.m_axisPos = m_axisPos;
		}
		return chartAxisImpl;
	}

	public ChartAxisImpl Clone(FontWrapper font)
	{
		ChartAxisImpl chartAxisImpl = MemberwiseClone() as ChartAxisImpl;
		chartAxisImpl.Font = (IOfficeFont)font.Clone(chartAxisImpl);
		return chartAxisImpl;
	}

	public void SetTitle(ChartTextAreaImpl text)
	{
		m_titleArea = text;
	}

	public void UpdateTickRecord(OfficeTickLabelPosition value)
	{
		m_chartTick.LabelPos = value;
	}

	public void MarkUsedReferences(bool[] usedItems)
	{
		m_titleArea.MarkUsedReferences(usedItems);
	}

	public void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		m_titleArea.UpdateReferenceIndexes(arrUpdatedIndexes);
	}

	protected void InitFrameFormat()
	{
		m_axisFormat = CreateFrameFormat();
		m_axisFormat.FrameRecord.AutoSize = true;
		m_axisFormat.Border.LinePattern = OfficeChartLinePattern.None;
		m_axisFormat.Border.AutoFormat = false;
		m_axisFormat.Interior.UseAutomaticFormat = false;
		m_axisFormat.Interior.Pattern = OfficePattern.None;
	}

	protected virtual ChartFrameFormatImpl CreateFrameFormat()
	{
		return new ChartFrameFormatImpl(base.Application, this);
	}

	internal void AssignBorderReference(IOfficeChartBorder border)
	{
		ChartBorderImpl border2 = border as ChartBorderImpl;
		m_border = border2;
	}

	internal void SetDefaultFont(string defaultFont, float defaultFontSize)
	{
		if (m_font == null)
		{
			FontImpl font = (FontImpl)ParentWorkbook.InnerFonts[0];
			m_font = new FontWrapper(font);
		}
		m_font.FontName = defaultFont;
		m_font.Size = defaultFontSize;
	}
}
