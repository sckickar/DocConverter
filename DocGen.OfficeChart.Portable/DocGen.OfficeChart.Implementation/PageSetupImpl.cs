using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation;

internal class PageSetupImpl : PageSetupBaseImpl, IPageSetup, IPageSetupBase, IParentApplication
{
	internal static readonly string DEF_AREA_XlS = NameRecord.PREDEFINED_NAMES[6];

	internal static readonly string DEF_AREA_XlSX = NameRecord.PREDEFINED_NAMES[15];

	private static readonly string DEF_TITLE_XLS = NameRecord.PREDEFINED_NAMES[7];

	private static readonly string DEF_TITLE_XLSX = NameRecord.PREDEFINED_NAMES[14];

	private static readonly FormulaToken[] DEF_PRINT_AREA_TOKENS = new FormulaToken[7]
	{
		FormulaToken.tRef3d1,
		FormulaToken.tRef3d2,
		FormulaToken.tRef3d3,
		FormulaToken.tArea3d1,
		FormulaToken.tArea3d2,
		FormulaToken.tArea3d3,
		FormulaToken.tCellRangeList
	};

	private ushort m_usPrintHeaders;

	private ushort m_usPrintGridlines;

	private ushort m_usGridset = 1;

	private GutsRecord m_Guts;

	private DefaultRowHeightRecord m_DefRowHeight;

	private WSBoolRecord m_WSBool;

	private WorksheetImpl m_worksheet;

	private string m_strRelationId;

	public bool PrintGridlines
	{
		get
		{
			return m_usPrintGridlines == 1;
		}
		set
		{
			ushort num = (value ? ((ushort)1) : ((ushort)0));
			if (m_usPrintGridlines != num)
			{
				m_usPrintGridlines = num;
				m_usGridset = 1;
				SetChanged();
			}
		}
	}

	public bool PrintHeadings
	{
		get
		{
			return m_usPrintHeaders != 0;
		}
		set
		{
			ushort num = (value ? ((ushort)1) : ((ushort)0));
			if (m_usPrintHeaders != num)
			{
				m_usPrintHeaders = num;
				SetChanged();
			}
		}
	}

	public string PrintArea
	{
		get
		{
			return ExtractPrintArea();
		}
		set
		{
			if (value != ExtractPrintArea())
			{
				ParsePrintAreaExpression(value);
			}
		}
	}

	public string PrintTitleColumns
	{
		get
		{
			return ExtractPrintTitleRowColumn(bRowExtract: false);
		}
		set
		{
			if (value != ExtractPrintTitleRowColumn(bRowExtract: false))
			{
				ParsePrintTitleColumns(value);
			}
		}
	}

	public string PrintTitleRows
	{
		get
		{
			return ExtractPrintTitleRowColumn(bRowExtract: true);
		}
		set
		{
			if (value != ExtractPrintTitleRowColumn(bRowExtract: true))
			{
				ParsePrintTitleRows(value);
			}
		}
	}

	public override bool IsFitToPage
	{
		get
		{
			return m_WSBool.IsFitToPage;
		}
		set
		{
			if (m_WSBool.IsFitToPage != value)
			{
				m_WSBool.IsFitToPage = value;
				SetChanged();
			}
		}
	}

	public bool IsSummaryRowBelow
	{
		get
		{
			return m_WSBool.IsRowSumsBelow;
		}
		set
		{
			m_WSBool.IsRowSumsBelow = value;
		}
	}

	public bool IsSummaryColumnRight
	{
		get
		{
			return m_WSBool.IsRowSumsRight;
		}
		set
		{
			m_WSBool.IsRowSumsRight = value;
		}
	}

	public int DefaultRowHeight
	{
		get
		{
			return m_DefRowHeight.Height;
		}
		set
		{
			m_DefRowHeight.Height = (ushort)value;
		}
	}

	public bool DefaultRowHeightFlag
	{
		get
		{
			return (m_DefRowHeight.OptionFlags & 1) == 1;
		}
		set
		{
			if (value)
			{
				m_DefRowHeight.OptionFlags = (ushort)(m_DefRowHeight.OptionFlags | 1u);
			}
			else if (m_worksheet.IsZeroHeight)
			{
				m_DefRowHeight.OptionFlags = (ushort)(m_DefRowHeight.OptionFlags | 2u);
			}
			else
			{
				m_DefRowHeight.OptionFlags = (ushort)(m_DefRowHeight.OptionFlags & 0u);
			}
		}
	}

	public string RelationId
	{
		get
		{
			return m_strRelationId;
		}
		set
		{
			m_strRelationId = value;
		}
	}

	public WorksheetImpl Worksheet => m_worksheet;

	public override int GetStoreSize(OfficeVersion version)
	{
		FillGutsRecord();
		return base.GetStoreSize(version) + 2 + 4 + 2 + 4 + 2 + 4 + m_Guts.GetStoreSize(version) + 4 + m_DefRowHeight.GetStoreSize(version) + 4 + m_WSBool.GetStoreSize(version) + 4;
	}

	public PageSetupImpl(IApplication application, object parent)
		: base(application, parent)
	{
		InitializeCollections();
		CreateNecessaryRecords();
	}

	[CLSCompliant(false)]
	public PageSetupImpl(IApplication application, object parent, BiffReader reader)
		: base(application, parent)
	{
		InitializeCollections();
		Parse(reader);
	}

	[CLSCompliant(false)]
	public PageSetupImpl(IApplication application, object parent, BiffRecordRaw[] data, int position)
		: base(application, parent)
	{
		InitializeCollections();
		Parse(data, position);
	}

	public PageSetupImpl(IApplication application, object parent, List<BiffRecordRaw> data, int position)
		: base(application, parent)
	{
		InitializeCollections();
		Parse(data, position);
		CreateNecessaryRecords();
	}

	protected override void FindParents()
	{
		base.FindParents();
		object obj = FindParent(typeof(WorksheetImpl));
		if (obj == null)
		{
			throw new ArgumentException("PageSetup class must be a leaf of Worksheet object tree");
		}
		m_worksheet = (WorksheetImpl)obj;
	}

	private void CreateNecessaryRecords()
	{
		if (m_Guts == null)
		{
			m_Guts = (GutsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Guts);
		}
		if (m_DefRowHeight == null)
		{
			m_DefRowHeight = (DefaultRowHeightRecord)BiffRecordFactory.GetRecord(TBIFFRecord.DefaultRowHeight);
		}
		else if (m_DefRowHeight.OptionFlags == 2)
		{
			m_worksheet.IsZeroHeight = true;
		}
		if (m_WSBool == null)
		{
			m_WSBool = (WSBoolRecord)BiffRecordFactory.GetRecord(TBIFFRecord.WSBool);
		}
	}

	[CLSCompliant(false)]
	protected override bool ParseRecord(BiffRecordRaw record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		bool flag = base.ParseRecord(record);
		if (!flag)
		{
			flag = true;
			switch (record.TypeCode)
			{
			case TBIFFRecord.PrintHeaders:
			{
				PrintHeadersRecord printHeadersRecord = (PrintHeadersRecord)record;
				m_usPrintHeaders = printHeadersRecord.IsPrintHeaders;
				break;
			}
			case TBIFFRecord.PrintGridlines:
			{
				PrintGridlinesRecord printGridlinesRecord = (PrintGridlinesRecord)record;
				m_usPrintGridlines = printGridlinesRecord.IsPrintGridlines;
				break;
			}
			case TBIFFRecord.Gridset:
			{
				GridsetRecord gridsetRecord = (GridsetRecord)record;
				m_usGridset = gridsetRecord.GridsetFlag;
				break;
			}
			case TBIFFRecord.Guts:
				m_Guts = (GutsRecord)record;
				break;
			case TBIFFRecord.DefaultRowHeight:
				m_DefRowHeight = (DefaultRowHeightRecord)record;
				break;
			case TBIFFRecord.WSBool:
				m_WSBool = (WSBoolRecord)record;
				break;
			case TBIFFRecord.HorizontalPageBreaks:
				_ = (HorizontalPageBreaksRecord)record;
				break;
			case TBIFFRecord.VerticalPageBreaks:
				_ = (VerticalPageBreaksRecord)record;
				break;
			default:
				flag = false;
				break;
			}
		}
		return flag;
	}

	[CLSCompliant(false)]
	public void Parse(BiffReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		throw new NotImplementedException();
	}

	private void SkipUnknownRecords(IList data, ref int pos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (pos < 0 || pos > data.Count)
		{
			throw new ArgumentOutOfRangeException("pos", "Value cannot be less than 0 and greater than data.Count");
		}
		while (data[pos] is UnknownRecord)
		{
			pos++;
		}
	}

	[CLSCompliant(false)]
	protected override void SerializeStartRecords(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_Guts == null)
		{
			throw new ArgumentNullException("m_Guts");
		}
		if (m_DefRowHeight == null)
		{
			throw new ArgumentNullException("m_DefRowHeight");
		}
		if (m_WSBool == null)
		{
			throw new ArgumentNullException("m_WSBool");
		}
		FillGutsRecord();
		PrintHeadersRecord printHeadersRecord = (PrintHeadersRecord)BiffRecordFactory.GetRecord(TBIFFRecord.PrintHeaders);
		printHeadersRecord.IsPrintHeaders = m_usPrintHeaders;
		records.Add(printHeadersRecord);
		PrintGridlinesRecord printGridlinesRecord = (PrintGridlinesRecord)BiffRecordFactory.GetRecord(TBIFFRecord.PrintGridlines);
		printGridlinesRecord.IsPrintGridlines = m_usPrintGridlines;
		records.Add(printGridlinesRecord);
		GridsetRecord gridsetRecord = (GridsetRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Gridset);
		gridsetRecord.GridsetFlag = m_usGridset;
		records.Add(gridsetRecord);
		records.Add(m_Guts);
		records.Add(m_DefRowHeight);
		records.Add(m_WSBool);
	}

	protected void FillGutsRecord()
	{
		m_Guts.MaxRowLevel = 0;
		m_Guts.MaxColumnLevel = 0;
		int firstRow = m_worksheet.FirstRow;
		if (firstRow > 0)
		{
			int i = firstRow;
			for (int lastRow = m_worksheet.LastRow; i <= lastRow; i++)
			{
				IOutline rowOutline = WorksheetHelper.GetRowOutline(m_worksheet, i);
				if (rowOutline != null && rowOutline.OutlineLevel > m_Guts.MaxRowLevel)
				{
					m_Guts.MaxRowLevel = rowOutline.OutlineLevel;
				}
			}
		}
		ColumnInfoRecord[] columnInformation = m_worksheet.ColumnInformation;
		foreach (IOutline outline in columnInformation)
		{
			if (outline != null && outline.OutlineLevel > m_Guts.MaxColumnLevel)
			{
				m_Guts.MaxColumnLevel = outline.OutlineLevel;
			}
		}
		if (m_Guts.MaxRowLevel != 0)
		{
			m_Guts.MaxRowLevel++;
			m_Guts.LeftRowGutter = (ushort)(m_Guts.MaxRowLevel * 14 - 1);
		}
		else
		{
			m_Guts.LeftRowGutter = 0;
		}
		if (m_Guts.MaxColumnLevel != 0)
		{
			m_Guts.MaxColumnLevel++;
			m_Guts.TopColumnGutter = (ushort)(m_Guts.MaxColumnLevel * 14 - 1);
		}
		else
		{
			m_Guts.TopColumnGutter = 0;
		}
	}

	private void InitializeCollections()
	{
	}

	protected string ConvertTo3dRangeName(string value)
	{
		Match match = FormulaUtil.CellRangeRegex.Match(value);
		if (match.Success)
		{
			return "'" + m_worksheet.Name + "'!" + match.Result("${Column1}${Row1}:${Column2}${Row2}");
		}
		Match match2 = FormulaUtil.CellRegex.Match(value);
		if (match2.Success)
		{
			return "'" + m_worksheet.Name + "'!" + match2.Result("${Column1}${Row1}");
		}
		match = FormulaUtil.FullRowRangeRegex.Match(value);
		if (match.Success)
		{
			return "'" + m_worksheet.Name + "'!" + value;
		}
		match = FormulaUtil.FullColumnRangeRegex.Match(value);
		if (match.Success)
		{
			return "'" + m_worksheet.Name + "'!" + value;
		}
		return null;
	}

	protected void ParsePrintAreaExpression(string value)
	{
		if (value == null || value.Length == 0)
		{
			m_worksheet.Names.Remove(DEF_AREA_XlS);
			return;
		}
		NameImpl nameImpl = ((m_worksheet.Workbook.Version == OfficeVersion.Excel97to2003) ? m_worksheet.InnerNames.GetOrCreateName(DEF_AREA_XlS) : m_worksheet.InnerNames.GetOrCreateName(DEF_AREA_XlSX));
		NameRecord record = nameImpl.Record;
		int num = 0;
		WorkbookImpl parentWorkbook = m_worksheet.ParentWorkbook;
		FormulaUtil formulaUtil = parentWorkbook.FormulaUtil;
		bool r1C1ReferenceMode = parentWorkbook.CalculationOptions.R1C1ReferenceMode;
		int iSheetReference = parentWorkbook.AddSheetReference(m_worksheet);
		Dictionary<Type, ReferenceIndexAttribute> dictionary = new Dictionary<Type, ReferenceIndexAttribute>();
		dictionary.Add(typeof(Area3DPtg), new ReferenceIndexAttribute(1));
		dictionary.Add(typeof(Ref3DPtg), new ReferenceIndexAttribute(1));
		dictionary.Add(typeof(AreaPtg), new ReferenceIndexAttribute(1));
		dictionary.Add(typeof(RefPtg), new ReferenceIndexAttribute(1));
		OfficeParseFormulaOptions options = (r1C1ReferenceMode ? (OfficeParseFormulaOptions.InName | OfficeParseFormulaOptions.UseR1C1) : OfficeParseFormulaOptions.InName);
		Ptg[] array = formulaUtil.ParseString(value, m_worksheet, dictionary, 0, null, options, 0, 0);
		int num2 = array.Length;
		Ptg[] array2 = new Ptg[num2];
		OfficeVersion version = m_worksheet.ParentWorkbook.Version;
		for (int i = 0; i < num2; i++)
		{
			Ptg ptg = array[i];
			if (ptg is IToken3D token3D)
			{
				ptg = token3D.Get3DToken(iSheetReference);
			}
			if (Array.IndexOf(DEF_PRINT_AREA_TOKENS, ptg.TokenCode) == -1)
			{
				throw new ArgumentException("Print area has incorrect format");
			}
			array2[i] = ptg;
			num += ptg.GetSize(version);
		}
		record.FormulaTokens = array2;
		((IParseable)nameImpl).Parse();
	}

	protected void ParsePrintTitleColumns(string value)
	{
		bool flag = value != null && value.Length > 0;
		NameRecord record = ((m_worksheet.Workbook.Version == OfficeVersion.Excel97to2003) ? m_worksheet.InnerNames.GetOrCreateName(DEF_TITLE_XLS) : m_worksheet.InnerNames.GetOrCreateName(DEF_TITLE_XLSX)).Record;
		Ptg[] formulaTokens = record.FormulaTokens;
		Area3DPtg area3DPtg = null;
		if (formulaTokens != null)
		{
			switch (formulaTokens.Length)
			{
			case 4:
				area3DPtg = formulaTokens[2] as Area3DPtg;
				break;
			case 1:
			{
				Area3DPtg area3DPtg2 = formulaTokens[0] as Area3DPtg;
				IWorkbook workbook = m_worksheet.Workbook;
				if (area3DPtg2.FirstRow != 0 || area3DPtg2.LastRow != workbook.MaxRowCount - 1)
				{
					area3DPtg = area3DPtg2;
				}
				break;
			}
			}
		}
		Ptg ptg = null;
		if (flag)
		{
			value = ConvertTo3dRangeName(value);
			ptg = m_worksheet.ParentWorkbook.FormulaUtil.ParseString(value)[0];
			ptg.TokenCode = FormulaToken.tArea3d1;
		}
		List<Ptg> list = new List<Ptg>();
		if (area3DPtg != null && flag)
		{
			WorkbookImpl parentWorkbook = m_worksheet.ParentWorkbook;
			FormulaUtil formulaUtil = parentWorkbook.FormulaUtil;
			OfficeVersion version = parentWorkbook.Version;
			Ptg ptg2 = FormulaUtil.CreatePtg(FormulaToken.tCellRangeList, formulaUtil.OperandsSeparator);
			int size = ptg.GetSize(version) + area3DPtg.GetSize(version) + ptg2.GetSize(version);
			list.AddRange(new Ptg[4]
			{
				new MemFuncPtg(size),
				ptg,
				area3DPtg,
				ptg2
			});
		}
		else if (area3DPtg != null && !flag)
		{
			list.Add(area3DPtg);
		}
		else
		{
			if (!flag)
			{
				m_worksheet.Names.Remove(DEF_TITLE_XLS);
				return;
			}
			list.Add(ptg);
		}
		record.FormulaTokens = list.ToArray();
	}

	protected void ParsePrintTitleRows(string value)
	{
		bool flag = value != null && value.Length > 0;
		NameRecord record = ((m_worksheet.Workbook.Version == OfficeVersion.Excel97to2003) ? m_worksheet.InnerNames.GetOrCreateName(DEF_TITLE_XLS) : m_worksheet.InnerNames.GetOrCreateName(DEF_TITLE_XLSX)).Record;
		Ptg[] formulaTokens = record.FormulaTokens;
		Area3DPtg area3DPtg = null;
		if (formulaTokens != null)
		{
			switch (formulaTokens.Length)
			{
			case 4:
				area3DPtg = formulaTokens[1] as Area3DPtg;
				break;
			case 1:
			{
				Area3DPtg area3DPtg2 = formulaTokens[0] as Area3DPtg;
				IWorkbook workbook = m_worksheet.Workbook;
				if (area3DPtg2.FirstRow == 0 || area3DPtg2.LastRow == workbook.MaxRowCount - 1)
				{
					area3DPtg = area3DPtg2;
				}
				break;
			}
			}
		}
		Ptg ptg = null;
		if (flag)
		{
			value = ConvertTo3dRangeName(value);
			ptg = m_worksheet.ParentWorkbook.FormulaUtil.ParseString(value)[0];
			ptg.TokenCode = FormulaToken.tArea3d1;
		}
		List<Ptg> list = new List<Ptg>();
		if (area3DPtg != null && flag)
		{
			WorkbookImpl parentWorkbook = m_worksheet.ParentWorkbook;
			FormulaUtil formulaUtil = parentWorkbook.FormulaUtil;
			OfficeVersion version = parentWorkbook.Version;
			Ptg ptg2 = FormulaUtil.CreatePtg(FormulaToken.tCellRangeList, formulaUtil.OperandsSeparator);
			int size = area3DPtg.GetSize(version) + ptg.GetSize(version) + ptg2.GetSize(version);
			list.AddRange(new Ptg[4]
			{
				new MemFuncPtg(size),
				area3DPtg,
				ptg,
				ptg2
			});
		}
		else if (area3DPtg != null && !flag)
		{
			list.Add(area3DPtg);
		}
		else
		{
			if (!flag)
			{
				m_worksheet.Names.Remove(DEF_TITLE_XLS);
				return;
			}
			list.Add(ptg);
		}
		record.FormulaTokens = list.ToArray();
	}

	protected string ExtractPrintArea()
	{
		INames names = m_worksheet.Names;
		NameImpl nameImpl = ((m_worksheet.Workbook.Version == OfficeVersion.Excel97to2003) ? ((NameImpl)names[DEF_AREA_XlS]) : ((NameImpl)names[DEF_AREA_XlSX]));
		if (nameImpl != null)
		{
			NameRecord record = nameImpl.Record;
			return GetAddressGlobalWithoutName(record.FormulaTokens);
		}
		return null;
	}

	protected string ExtractPrintTitleRowColumn(bool bRowExtract)
	{
		INames names = m_worksheet.Names;
		NameImpl nameImpl = ((m_worksheet.Workbook.Version == OfficeVersion.Excel97to2003) ? ((NameImpl)names[DEF_TITLE_XLS]) : ((NameImpl)names[DEF_TITLE_XLSX]));
		if (nameImpl != null)
		{
			Ptg[] formulaTokens = nameImpl.Record.FormulaTokens;
			if (formulaTokens.Length == 0 || formulaTokens.Length > 4)
			{
				throw new ArgumentOutOfRangeException("Print_Titles Name record", "Print_Titles Name record has wrong quantity of formula tokens.");
			}
			if (formulaTokens.Length == 4)
			{
				if (bRowExtract)
				{
					return GetAddressGlobalWithoutName(new Ptg[1] { formulaTokens[2] });
				}
				return GetAddressGlobalWithoutName(new Ptg[1] { formulaTokens[1] });
			}
			if (formulaTokens.Length == 3)
			{
				if (bRowExtract)
				{
					return GetAddressGlobalWithoutName(new Ptg[1] { formulaTokens[1] });
				}
				return GetAddressGlobalWithoutName(new Ptg[1] { formulaTokens[0] });
			}
			if (formulaTokens.Length == 1)
			{
				string addressGlobalWithoutName = GetAddressGlobalWithoutName(formulaTokens);
				Area3DPtg area3DPtg = formulaTokens[0] as Area3DPtg;
				if (bRowExtract)
				{
					if (area3DPtg.FirstRow == 0 && area3DPtg.LastRow == m_worksheet.ParentWorkbook.MaxRowCount - 1)
					{
						return null;
					}
					return addressGlobalWithoutName;
				}
				if (area3DPtg.FirstRow == 0 && area3DPtg.LastRow == m_worksheet.ParentWorkbook.MaxRowCount - 1)
				{
					return addressGlobalWithoutName;
				}
				return null;
			}
		}
		return null;
	}

	protected string GetAddressGlobalWithoutName(Ptg[] token)
	{
		return m_worksheet.ParentWorkbook.FormulaUtil.ParsePtgArray(token, 0, 0, bR1C1: false, null, isForSerialization: true);
	}

	public PageSetupImpl Clone(object parent)
	{
		PageSetupImpl obj = (PageSetupImpl)MemberwiseClone();
		obj.SetParent(parent);
		obj.FindParents();
		m_Guts = (GutsRecord)CloneUtils.CloneCloneable(m_Guts);
		m_DefRowHeight = (DefaultRowHeightRecord)CloneUtils.CloneCloneable(m_DefRowHeight);
		m_WSBool = (WSBoolRecord)CloneUtils.CloneCloneable(m_WSBool);
		m_unknown = (PrinterSettingsRecord)CloneUtils.CloneCloneable(m_unknown);
		m_setup = (PrintSetupRecord)CloneUtils.CloneCloneable(m_setup);
		if (m_headerFooter != null)
		{
			m_headerFooter = (HeaderAndFooterRecord)CloneUtils.CloneCloneable(m_headerFooter);
		}
		m_arrHeaders = CloneUtils.CloneStringArray(m_arrHeaders);
		m_arrFooters = CloneUtils.CloneStringArray(m_arrFooters);
		return obj;
	}

	public override void Dispose()
	{
		base.Dispose();
		if (dictPaperHeight != null)
		{
			dictPaperHeight.Clear();
			dictPaperHeight = null;
		}
		if (dictPaperWidth != null)
		{
			dictPaperWidth.Clear();
			dictPaperWidth = null;
		}
		m_arrHeaders = null;
		if (m_backgroundImage != null)
		{
			m_backgroundImage.Dispose();
		}
		m_bIsDisposed = true;
	}
}
