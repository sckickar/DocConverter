using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Interfaces.Charts;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartTextAreaImpl : CommonObject, IOfficeChartTextArea, IParentApplication, IOfficeFont, IOptimizedUpdate, IOfficeChartDataLabels, ISerializable, IInternalOfficeChartTextArea, IInternalFont
{
	private bool m_isBaselineWithPercentage;

	private bool m_isValueFromCells;

	private IOfficeDataRange m_valueFromCellsRange;

	private FontWrapper m_font;

	internal ChartTextRecord m_chartText = (ChartTextRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartText);

	private WorkbookImpl m_book;

	private ChartFrameFormatImpl m_frame;

	private string m_strText;

	private ChartObjectLinkRecord m_link = (ChartObjectLinkRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartObjectLink);

	private ChartDataLabelsRecord m_dataLabels = (ChartDataLabelsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartDataLabels);

	private ChartAIRecord m_chartAi;

	private ChartAlrunsRecord m_chartAlRuns;

	private ChartPosRecord m_pos;

	private ChartAttachedLabelLayoutRecord m_attachedLabelLayout;

	private bool m_bIsTrend;

	private IOfficeChartLayout m_layout;

	private ChartParagraphType m_paraType;

	private bool m_bShowTextProperties = true;

	private bool m_bShowSizeProperties;

	private bool m_bShowBoldProperties;

	private bool m_bShowItalicProperties;

	private bool m_bIsFormula;

	protected IOfficeChartRichTextString m_rtfString;

	private Excel2007TextRotation m_TextRotation;

	private bool m_bIsTextParsed;

	private string[] m_stringCache;

	private bool m_bOverlay;

	internal IList<IInternalOfficeChartTextArea> DefaultParagarphProperties = new List<IInternalOfficeChartTextArea>();

	public bool Bold
	{
		get
		{
			return m_font.Bold;
		}
		set
		{
			m_font.Bold = value;
			if (!ParentWorkbook.IsWorkbookOpening)
			{
				ShowBoldProperties = true;
			}
		}
	}

	public OfficeKnownColors Color
	{
		get
		{
			return m_font.Color;
		}
		set
		{
			if (m_chartText.ColorIndex != value)
			{
				m_chartText.ColorIndex = value;
				m_font.Color = value;
				m_chartText.IsAutoColor = false;
			}
		}
	}

	public Color RGBColor
	{
		get
		{
			return m_font.RGBColor;
		}
		set
		{
			m_font.RGBColor = value;
		}
	}

	public bool Italic
	{
		get
		{
			return m_font.Italic;
		}
		set
		{
			m_font.Italic = value;
		}
	}

	public bool MacOSOutlineFont
	{
		get
		{
			return m_font.MacOSOutlineFont;
		}
		set
		{
			m_font.MacOSOutlineFont = value;
		}
	}

	public bool MacOSShadow
	{
		get
		{
			return m_font.MacOSShadow;
		}
		set
		{
			m_font.MacOSShadow = value;
		}
	}

	public double Size
	{
		get
		{
			return m_font.Size;
		}
		set
		{
			m_font.Size = value;
			if (!ParentWorkbook.IsWorkbookOpening)
			{
				if (base.Parent is ChartLegendImpl)
				{
					(base.Parent as ChartLegendImpl).IsDefaultTextSettings = false;
				}
				ShowSizeProperties = true;
			}
		}
	}

	public bool Strikethrough
	{
		get
		{
			return m_font.Strikethrough;
		}
		set
		{
			m_font.Strikethrough = value;
		}
	}

	internal int Baseline
	{
		get
		{
			return m_font.Baseline;
		}
		set
		{
			m_font.Baseline = value;
		}
	}

	public bool Subscript
	{
		get
		{
			return m_font.Subscript;
		}
		set
		{
			m_font.Subscript = value;
		}
	}

	public bool Superscript
	{
		get
		{
			return m_font.Superscript;
		}
		set
		{
			m_font.Superscript = value;
		}
	}

	public OfficeUnderline Underline
	{
		get
		{
			return m_font.Underline;
		}
		set
		{
			m_font.Underline = value;
		}
	}

	public string FontName
	{
		get
		{
			return m_font.FontName;
		}
		set
		{
			m_font.FontName = value;
		}
	}

	public OfficeFontVerticalAlignment VerticalAlignment
	{
		get
		{
			return m_font.VerticalAlignment;
		}
		set
		{
			m_font.VerticalAlignment = value;
		}
	}

	internal bool HasCapOrCharacterSpaceOrKerning
	{
		get
		{
			return m_font.HasCapOrCharacterSpaceOrKerning;
		}
		set
		{
			m_font.HasCapOrCharacterSpaceOrKerning = value;
		}
	}

	internal bool IsCapitalize
	{
		get
		{
			return m_font.IsCapitalize;
		}
		set
		{
			m_font.IsCapitalize = value;
		}
	}

	internal double CharacterSpacingValue
	{
		get
		{
			return m_font.CharacterSpacingValue;
		}
		set
		{
			m_font.CharacterSpacingValue = value;
		}
	}

	internal double KerningValue
	{
		get
		{
			return m_font.KerningValue;
		}
		set
		{
			m_font.KerningValue = value;
		}
	}

	internal bool IsBaselineWithPercentage
	{
		get
		{
			return m_isBaselineWithPercentage;
		}
		set
		{
			m_isBaselineWithPercentage = value;
		}
	}

	public string Text
	{
		get
		{
			return m_strText;
		}
		set
		{
			if (base.Parent is ChartLegendImpl)
			{
				throw new NotSupportedException("Text cannot be set to the chart's legend");
			}
			if (m_link.LinkObject == ExcelObjectTextLink.Chart && base.Parent is ChartImpl && base.Parent is ChartImpl chartImpl)
			{
				if (value == null)
				{
					chartImpl.m_bHasChartTitle = false;
				}
				else
				{
					chartImpl.m_bHasChartTitle = true;
				}
			}
			m_strText = value;
			if (m_bIsTrend || m_link.LinkObject == ExcelObjectTextLink.DisplayUnit)
			{
				m_chartText.IsAutoText = false;
			}
			m_chartText.IsDeleted = value == null;
			if (m_chartAlRuns != null && m_chartAlRuns.Runs != null && m_chartAlRuns.Runs.Length != 0)
			{
				m_chartAlRuns = null;
			}
			if (m_chartAi != null && IsFormula)
			{
				m_chartAi.ParsedExpression = GetNameTokens();
				m_chartAi.Reference = ChartAIRecord.ReferenceType.Worksheet;
			}
		}
	}

	public IOfficeChartRichTextString RichText
	{
		get
		{
			CheckDisposed();
			if (m_rtfString == null)
			{
				CreateRichTextString();
			}
			return m_rtfString;
		}
	}

	public IOfficeChartFrameFormat FrameFormat
	{
		get
		{
			if (m_frame == null)
			{
				InitFrameFormat();
			}
			return m_frame;
		}
	}

	[CLSCompliant(false)]
	public ChartObjectLinkRecord ObjectLink => m_link;

	public int TextRotationAngle
	{
		get
		{
			if (m_chartText.TextRotation > 90)
			{
				return m_chartText.TextRotation - 90;
			}
			return -m_chartText.TextRotation;
		}
		set
		{
			if (value > 0)
			{
				m_chartText.TextRotation = (short)(90 + (short)value);
			}
			else
			{
				m_chartText.TextRotation = (short)(-value);
			}
		}
	}

	public bool HasTextRotation => m_chartText.TextRotationOrNull.HasValue;

	internal Excel2007TextRotation TextRotation
	{
		get
		{
			return m_TextRotation;
		}
		set
		{
			m_TextRotation = value;
		}
	}

	[CLSCompliant(false)]
	public ChartTextRecord TextRecord => m_chartText;

	public string NumberFormat
	{
		get
		{
			int numberFormatIndex = NumberFormatIndex;
			return m_book.InnerFormats[numberFormatIndex].FormatString;
		}
		set
		{
			int num = m_book.InnerFormats.FindOrCreateFormat(value);
			ChartAI.NumberFormatIndex = (ushort)num;
		}
	}

	public int NumberFormatIndex
	{
		get
		{
			if (m_chartAi == null)
			{
				return 0;
			}
			return m_chartAi.NumberFormatIndex;
		}
	}

	[CLSCompliant(false)]
	public ChartAIRecord ChartAI
	{
		get
		{
			if (m_chartAi == null)
			{
				m_chartAi = (ChartAIRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAI);
			}
			return m_chartAi;
		}
	}

	[CLSCompliant(false)]
	public ChartAlrunsRecord ChartAlRuns
	{
		get
		{
			if (m_chartAlRuns == null)
			{
				m_chartAlRuns = (ChartAlrunsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAlruns);
			}
			return m_chartAlRuns;
		}
	}

	public bool ContainDataLabels => !(m_dataLabels == null);

	internal OfficeChartBackgroundMode BackgroundMode
	{
		get
		{
			return m_chartText.BackgroundMode;
		}
		set
		{
			m_chartText.BackgroundMode = value;
			IsAutoMode = false;
		}
	}

	internal bool IsAutoMode
	{
		get
		{
			return m_chartText.IsAutoMode;
		}
		set
		{
			m_chartText.IsAutoMode = value;
		}
	}

	public bool IsTrend
	{
		get
		{
			return m_bIsTrend;
		}
		set
		{
			m_bIsTrend = value;
			if (m_strText == null || m_strText.Length == 0)
			{
				m_chartText.IsAutoText = true;
			}
		}
	}

	public bool IsAutoColor => m_chartText.IsAutoColor;

	public IOfficeChartLayout Layout
	{
		get
		{
			if (m_layout == null)
			{
				m_layout = new ChartLayoutImpl(base.Application, this, base.Parent);
			}
			return m_layout;
		}
		set
		{
			m_layout = value;
		}
	}

	public WorkbookImpl ParentWorkbook => m_book;

	internal bool Overlay
	{
		get
		{
			return m_bOverlay;
		}
		set
		{
			m_bOverlay = value;
		}
	}

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

	internal bool ShowTextProperties
	{
		get
		{
			return m_bShowTextProperties;
		}
		set
		{
			m_bShowTextProperties = value;
		}
	}

	internal bool ShowSizeProperties
	{
		get
		{
			return m_bShowSizeProperties;
		}
		set
		{
			m_bShowSizeProperties = value;
		}
	}

	internal bool ShowBoldProperties
	{
		get
		{
			return m_bShowBoldProperties;
		}
		set
		{
			m_bShowBoldProperties = value;
		}
	}

	internal bool ShowItalicProperties
	{
		get
		{
			return m_bShowItalicProperties;
		}
		set
		{
			m_bShowItalicProperties = value;
		}
	}

	internal string[] StringCache
	{
		get
		{
			return m_stringCache;
		}
		set
		{
			m_stringCache = value;
		}
	}

	protected virtual bool ShouldSerialize
	{
		get
		{
			if (!HasText && !m_bIsTrend && m_link.LinkObject != ExcelObjectTextLink.DataLabel && m_link.LinkObject != ExcelObjectTextLink.DisplayUnit && !m_chartText.IsDeleted && m_chartText.IsAutoColor && m_chartText.IsAutoMode)
			{
				return !m_chartText.IsAutoText;
			}
			return true;
		}
	}

	public bool HasText => m_strText != null;

	internal bool IsFormula
	{
		get
		{
			return m_bIsFormula;
		}
		set
		{
			m_bIsFormula = value;
		}
	}

	internal bool IsTextParsed
	{
		get
		{
			return m_bIsTextParsed;
		}
		set
		{
			m_bIsTextParsed = value;
		}
	}

	public int FontIndex
	{
		get
		{
			if (m_font == null)
			{
				return 0;
			}
			return m_font.Index;
		}
	}

	public bool IsValueFromCells
	{
		get
		{
			return m_isValueFromCells;
		}
		set
		{
			m_isValueFromCells = value;
		}
	}

	public IOfficeDataRange ValueFromCellsRange
	{
		get
		{
			return m_valueFromCellsRange;
		}
		set
		{
			m_valueFromCellsRange = value;
		}
	}

	public bool IsSeriesName
	{
		get
		{
			if (!(m_dataLabels != null))
			{
				return false;
			}
			return m_dataLabels.IsSeriesName;
		}
		set
		{
			m_dataLabels.IsSeriesName = value;
		}
	}

	public bool IsCategoryName
	{
		get
		{
			if (!(m_dataLabels != null))
			{
				return false;
			}
			return m_dataLabels.IsCategoryName;
		}
		set
		{
			m_dataLabels.IsCategoryName = value;
		}
	}

	public bool IsValue
	{
		get
		{
			if (!(m_dataLabels != null))
			{
				return false;
			}
			return m_dataLabels.IsValue;
		}
		set
		{
			if (m_dataLabels == null)
			{
				CreateDataLabels();
			}
			m_dataLabels.IsValue = value;
			TextRecord.IsShowValue = value;
		}
	}

	public bool IsPercentage
	{
		get
		{
			if (!(m_dataLabels != null))
			{
				return false;
			}
			return m_dataLabels.IsPercentage;
		}
		set
		{
			m_dataLabels.IsPercentage = value;
			TextRecord.IsShowPercent = value;
		}
	}

	public bool IsBubbleSize
	{
		get
		{
			if (!(m_dataLabels != null))
			{
				return false;
			}
			return m_dataLabels.IsBubbleSize;
		}
		set
		{
			m_dataLabels.IsBubbleSize = value;
		}
	}

	public bool ShowLeaderLines
	{
		get
		{
			throw new NotSupportedException("Liner lines are not supported.");
		}
		set
		{
		}
	}

	public string Delimiter
	{
		get
		{
			if (!(m_dataLabels != null))
			{
				return null;
			}
			return m_dataLabels.Delimiter;
		}
		set
		{
			m_dataLabels.Delimiter = value;
		}
	}

	public bool IsLegendKey
	{
		get
		{
			return TextRecord.IsShowKey;
		}
		set
		{
			TextRecord.IsShowKey = value;
		}
	}

	public OfficeDataLabelPosition Position
	{
		get
		{
			return m_chartText.DataLabelPlacement;
		}
		set
		{
			m_chartText.DataLabelPlacement = value;
		}
	}

	public ChartAttachedLabelLayoutRecord AttachedLabelLayout
	{
		get
		{
			if (m_attachedLabelLayout == null)
			{
				m_attachedLabelLayout = (ChartAttachedLabelLayoutRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAttachedLabelLayout);
			}
			return m_attachedLabelLayout;
		}
	}

	public bool IsShowLabelPercent
	{
		get
		{
			return TextRecord.IsShowLabelPercent;
		}
		set
		{
			TextRecord.IsShowLabelPercent = value;
		}
	}

	public ChartColor ColorObject => m_font.ColorObject;

	public int Index => m_font.Index;

	public FontImpl Font => m_font.Font;

	[CLSCompliant(false)]
	public static BiffRecordRaw UnwrapRecord(BiffRecordRaw record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (record.TypeCode == TBIFFRecord.ChartWrapper)
		{
			return ((ChartWrapperRecord)record).Record;
		}
		return record;
	}

	public ChartTextAreaImpl(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
		ChartImpl chartImpl = (ChartImpl)FindParent(typeof(ChartImpl));
		SetFontIndex(chartImpl.DefaultTextIndex);
		m_chartText.IsAutoMode = true;
		m_chartText.IsGenerated = false;
		m_chartText.IsAutoText = false;
		m_chartText.IsAutoColor = true;
		m_chartText.HorzAlign = ExcelChartHorzAlignment.Center;
		m_chartText.VertAlign = ExcelChartVertAlignment.Center;
		m_link.LinkObject = ExcelObjectTextLink.DataLabel;
		m_chartAi = (ChartAIRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAI);
		m_chartAi.Reference = ChartAIRecord.ReferenceType.EnteredDirectly;
		m_chartAlRuns = (ChartAlrunsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAlruns);
		m_pos = (ChartPosRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPos);
		m_pos.TopLeft = 2;
		m_pos.BottomRight = 2;
		m_paraType = ChartParagraphType.Default;
		if (!m_book.IsWorkbookOpening && (ChartLegendImpl)FindParent(typeof(ChartLegendImpl)) != null && ChartImpl.IsChartExSerieType(chartImpl.ChartType))
		{
			Font.Size = 9.0;
		}
	}

	[CLSCompliant(false)]
	public ChartTextAreaImpl(IApplication application, object parent, ExcelObjectTextLink textLink)
		: this(application, parent)
	{
		m_link.LinkObject = textLink;
	}

	public ChartTextAreaImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: this(application, parent)
	{
		iPos = Parse(data, iPos);
	}

	private void SetParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
	}

	public Font GenerateNativeFont()
	{
		return m_font.GenerateNativeFont();
	}

	[CLSCompliant(false)]
	public int Parse(IList<BiffRecordRaw> data, int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (iPos < 0 || iPos >= data.Count)
		{
			throw new ArgumentOutOfRangeException("iPos", "Value cannot be less than 0 and greater than data.Length");
		}
		m_chartText = (ChartTextRecord)UnwrapRecord(data[iPos]);
		iPos++;
		BiffRecordRaw biffRecordRaw = UnwrapRecord(data[iPos]);
		iPos++;
		biffRecordRaw.CheckTypeCode(TBIFFRecord.Begin);
		m_dataLabels = null;
		while (biffRecordRaw.TypeCode != TBIFFRecord.End)
		{
			biffRecordRaw = data[iPos];
			biffRecordRaw = UnwrapRecord(biffRecordRaw);
			iPos++;
			iPos = ParseRecord(biffRecordRaw, data, iPos);
		}
		return iPos;
	}

	private void ParseFontx(ChartFontxRecord fontx)
	{
		if (fontx == null)
		{
			throw new ArgumentNullException("fontx");
		}
		SetFontIndex(fontx.FontIndex);
	}

	[CLSCompliant(false)]
	protected int ParseRecord(BiffRecordRaw record, IList<BiffRecordRaw> data, int iPos)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		switch (record.TypeCode)
		{
		case TBIFFRecord.ChartFontx:
			ParseFontx((ChartFontxRecord)record);
			ParagraphType = ChartParagraphType.CustomDefault;
			break;
		case TBIFFRecord.ChartAlruns:
			m_chartAlRuns = (ChartAlrunsRecord)record;
			break;
		case TBIFFRecord.ChartAI:
			m_chartAi = (ChartAIRecord)record;
			if (m_chartAi != null && m_chartAi.FormulaSize > 0)
			{
				IsFormula = true;
			}
			break;
		case TBIFFRecord.ChartSeriesText:
		{
			ChartSeriesTextRecord chartSeriesTextRecord = (ChartSeriesTextRecord)record;
			if (IsFormula && m_chartAi != null && m_chartAi.ParsedExpression != null)
			{
				m_strText = m_book.FormulaUtil.ParsePtgArray(m_chartAi.ParsedExpression);
			}
			else
			{
				m_strText = chartSeriesTextRecord.Text;
			}
			break;
		}
		case TBIFFRecord.ChartFrame:
			iPos--;
			InitFrameFormat();
			m_frame.Parse(data, ref iPos);
			break;
		case TBIFFRecord.ChartObjectLink:
			m_link = (ChartObjectLinkRecord)record;
			break;
		case TBIFFRecord.ChartDataLabels:
			m_dataLabels = (ChartDataLabelsRecord)record;
			break;
		case TBIFFRecord.ChartPos:
			m_pos = (ChartPosRecord)record;
			break;
		case TBIFFRecord.ChartAttachedLabelLayout:
			if (m_attachedLabelLayout == null)
			{
				m_attachedLabelLayout = (Layout.ManualLayout as ChartManualLayoutImpl).AttachedLabelLayout;
			}
			m_attachedLabelLayout = (ChartAttachedLabelLayoutRecord)record;
			break;
		}
		m_font.ColorObject.SetIndexed(m_chartText.ColorIndex);
		return iPos;
	}

	[CLSCompliant(false)]
	public virtual void Serialize(IList<IBiffStorage> records)
	{
		Serialize(records, bIsLegendEntry: false);
	}

	[CLSCompliant(false)]
	public void Serialize(IList<IBiffStorage> records, bool bIsLegendEntry)
	{
		Serialize(records, bIsLegendEntry, bSerializeFontX: true);
	}

	[CLSCompliant(false)]
	public void Serialize(IList<IBiffStorage> records, bool bIsLegendEntry, bool bSerializeFontX)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (!ShouldSerialize)
		{
			return;
		}
		if (m_bIsTrend)
		{
			UpdateAsTrend();
		}
		m_chartText.ColorIndex = m_font.ColorObject.GetIndexed(m_book);
		SerializeRecord(records, m_chartText);
		SerializeRecord(records, BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		bool flag = m_link.LinkObject == ExcelObjectTextLink.DataLabel;
		if (!flag)
		{
			m_chartText.DataLabelPlacement = OfficeDataLabelPosition.Automatic;
		}
		if (m_pos != null)
		{
			SerializeRecord(records, m_pos);
		}
		if (bSerializeFontX)
		{
			SerializeFontx(records);
		}
		if (m_chartAlRuns != null && m_chartAlRuns.Runs.Length >= 3 && m_chartAlRuns.Runs.Length <= 256)
		{
			SerializeRecord(records, m_chartAlRuns);
		}
		SerializeRecord(records, m_chartAi);
		if (bIsLegendEntry)
		{
			SerializeRecord(records, BiffRecordFactory.GetRecord(TBIFFRecord.End));
			return;
		}
		if (m_strText != null)
		{
			ChartSeriesTextRecord chartSeriesTextRecord = (ChartSeriesTextRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSeriesText);
			chartSeriesTextRecord.Text = m_strText;
			SerializeRecord(records, chartSeriesTextRecord);
		}
		if (m_frame != null)
		{
			m_frame.Serialize(records);
		}
		SerializeRecord(records, m_link);
		if (m_attachedLabelLayout != null)
		{
			SerializeRecord(records, m_attachedLabelLayout);
		}
		if (!m_bIsTrend && flag && m_dataLabels != null)
		{
			SerializeRecord(records, m_dataLabels);
		}
		SerializeRecord(records, BiffRecordFactory.GetRecord(TBIFFRecord.End));
	}

	private void SerializeFontx(IList<IBiffStorage> records)
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
		FontWrapper font = m_font;
		int num = list?[font.Wrapped.Index] ?? font.Wrapped.Index;
		if (num > 0)
		{
			ChartFontxRecord chartFontxRecord = (ChartFontxRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartFontx);
			chartFontxRecord.FontIndex = (ushort)num;
			SerializeRecord(records, chartFontxRecord);
		}
	}

	[CLSCompliant(false)]
	protected virtual void SerializeRecord(IList<IBiffStorage> records, BiffRecordRaw record)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (record != null)
		{
			records.Add((BiffRecordRaw)record.Clone());
		}
	}

	protected virtual ChartFrameFormatImpl CreateFrameFormat()
	{
		return new ChartFrameFormatImpl(base.Application, this);
	}

	protected void InitFrameFormat()
	{
		m_frame = CreateFrameFormat();
		m_frame.FrameRecord.AutoSize = true;
		m_frame.Border.LinePattern = OfficeChartLinePattern.None;
		m_frame.Border.AutoFormat = false;
		m_frame.Interior.UseAutomaticFormat = false;
		m_frame.Interior.Pattern = OfficePattern.None;
	}

	internal void SetFontIndex(int index)
	{
		DetachEvents();
		FontImpl wrapped = (FontImpl)m_book.InnerFonts[index];
		if (m_font == null)
		{
			m_font = new FontWrapper();
		}
		m_font.Wrapped = wrapped;
		AttachEvents();
	}

	private void CreateDataLabels()
	{
		if (m_dataLabels == null)
		{
			m_dataLabels = new ChartDataLabelsRecord();
		}
	}

	public object Clone(object parent, Dictionary<int, int> dicFontIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		ChartTextAreaImpl chartTextAreaImpl = (ChartTextAreaImpl)MemberwiseClone();
		chartTextAreaImpl.SetParent(parent);
		chartTextAreaImpl.SetParents();
		chartTextAreaImpl.m_bIsDisposed = m_bIsDisposed;
		chartTextAreaImpl.m_chartText = (ChartTextRecord)CloneUtils.CloneCloneable(m_chartText);
		if (m_font != null)
		{
			chartTextAreaImpl.m_font = m_font.Clone(chartTextAreaImpl.m_book, chartTextAreaImpl, dicFontIndexes);
		}
		if (m_frame != null)
		{
			chartTextAreaImpl.m_frame = m_frame.Clone(chartTextAreaImpl);
		}
		chartTextAreaImpl.m_link = (ChartObjectLinkRecord)CloneUtils.CloneCloneable(m_link);
		if (m_chartAi != null)
		{
			chartTextAreaImpl.m_chartAi = (ChartAIRecord)m_chartAi.Clone();
			chartTextAreaImpl.NumberFormat = NumberFormat;
			Ptg[] parsedExpression = chartTextAreaImpl.m_chartAi.ParsedExpression;
			int num = ((parsedExpression != null) ? parsedExpression.Length : 0);
			for (int i = 0; i < num; i++)
			{
				if (!(parsedExpression[i] is ISheetReference sheetReference))
				{
					continue;
				}
				int refIndex = sheetReference.RefIndex;
				int num2 = refIndex;
				if (!m_book.IsExternalReference(refIndex))
				{
					string text = m_book.GetSheetNameByReference(refIndex);
					if (dicNewSheetNames != null && dicNewSheetNames.ContainsKey(text))
					{
						text = dicNewSheetNames[text];
					}
					num2 = chartTextAreaImpl.m_book.AddSheetReference(text);
				}
				sheetReference.RefIndex = (ushort)num2;
			}
		}
		chartTextAreaImpl.m_bOverlay = m_bOverlay;
		if (m_layout != null)
		{
			chartTextAreaImpl.m_layout = m_layout;
		}
		if (m_chartAlRuns != null)
		{
			chartTextAreaImpl.m_chartAlRuns = (ChartAlrunsRecord)m_chartAlRuns.Clone();
		}
		chartTextAreaImpl.m_strText = m_strText;
		chartTextAreaImpl.m_paraType = m_paraType;
		chartTextAreaImpl.m_TextRotation = m_TextRotation;
		if (m_pos != null)
		{
			chartTextAreaImpl.m_pos = (ChartPosRecord)CloneUtils.CloneCloneable(m_pos);
		}
		chartTextAreaImpl.BackgroundMode = m_chartText.BackgroundMode;
		return chartTextAreaImpl;
	}

	public object Clone(object parent)
	{
		return Clone(parent, null, null);
	}

	public void UpdateSerieIndex(int iNewIndex)
	{
		m_link.SeriesNumber = (ushort)iNewIndex;
	}

	public void UpdateAsTrend()
	{
		ObjectLink.DataPointNumber = ushort.MaxValue;
		m_chartText.IsShowLabel = true;
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

	private void AttachEvents()
	{
		if (m_font != null)
		{
			m_font.ColorObject.AfterChange += ColorChangeEventHandler;
		}
	}

	internal void DetachEvents()
	{
		if (m_font != null)
		{
			m_font.ColorObject.AfterChange -= ColorChangeEventHandler;
		}
	}

	private void ColorChangeEventHandler()
	{
		m_chartText.IsAutoColor = false;
	}

	private Ptg[] GetNameTokens()
	{
		Ptg[] result = null;
		string text = m_strText;
		if (text != null && text.Length > 0)
		{
			if (text[0] == '=')
			{
				text = UtilityMethods.RemoveFirstCharUnsafe(text);
			}
			if (ChartImpl.TryAndModifyToValidFormula(text))
			{
				result = m_book.FormulaUtil.ParseString(text);
			}
		}
		return result;
	}

	public void BeginUpdate()
	{
	}

	public void EndUpdate()
	{
	}

	protected void CreateRichTextString()
	{
		m_rtfString = new ChartRichTextString(base.Application, this);
	}
}
