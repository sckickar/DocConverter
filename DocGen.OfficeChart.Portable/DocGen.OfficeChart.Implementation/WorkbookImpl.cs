using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Collections.Grouping;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation;

internal class WorkbookImpl : CommonObject, IWorkbook, IParentApplication
{
	public delegate ShapeCollectionBase ShapesGetterMethod(ITabSheet sheet);

	private const string DEF_SUMMARY_INFO = "\u0005SummaryInformation";

	private const string DEF_DOCUMENT_SUMMARY_INFO = "\u0005DocumentSummaryInformation";

	internal const string DEF_STREAM_NAME1 = "Workbook";

	internal const string DEF_STREAM_NAME2 = "Book";

	private const string DEF_VBA_MACROS = "_VBA_PROJECT_CUR";

	private const string DEF_VBA_SUB_STORAGE = "VBA";

	private const char DEF_CHAR_SELF = '\u0002';

	private const char DEF_CHAR_CODED = '\u0001';

	private const char DEF_CHAR_EMPTY = '\0';

	private const char DEF_CHAR_VOLUME = '\u0001';

	private const char DEF_CHAR_SAMEVOLUME = '\u0002';

	private const char DEF_CHAR_DOWNDIR = '\u0003';

	private const char DEF_CHAR_UPDIR = '\u0004';

	private const char DEF_CHAR_LONGVOLUME = '\u0005';

	private const char DEF_CHAR_STARTUPDIR = '\u0006';

	private const char DEF_CHAR_ALTSTARTUPDIR = '\a';

	private const char DEF_CHAR_LIBDIR = '\b';

	private const char DEF_CHAR_NETWORKPATH = '@';

	private const string DEF_NETWORKPATH_START = "\\\\";

	private const int DEF_NOT_PASSWORD_PROTECTION = 0;

	internal const int DEF_REMOVED_SHEET_INDEX = 65535;

	private const string HttpStart = "http:";

	private const string NEW_LINE = "\n";

	internal static readonly Color[] DEF_PALETTE;

	internal static readonly double[] DefaultTints;

	internal static readonly Color[][] ThemeColorPalette;

	private float[] DEF_FONT_HEIGHT_SINGLE_INCR = new float[12]
	{
		6f, 8f, 9f, 12f, 14f, 15f, 18f, 21f, 23f, 24f,
		26f, 27f
	};

	private float[] DEF_FONT_HEIGHT_DOUBLE_INCR = new float[3] { 5f, 17f, 20f };

	private float[] DEF_FONT_WIDTH_SINGLE_INCR = new float[1] { 11f };

	public const int DEF_FIRST_USER_COLOR = 8;

	public const string DEF_BAD_SHEET_NAME = "#REF";

	private const int DEF_FIRST_DEFINED_FONT = 10;

	private const string DEF_RESPONSE_OPEN = "inline";

	private const string DEF_RESPONSE_DIALOG = "attachment";

	private const ushort DEF_REMOVED_INDEX = ushort.MaxValue;

	private const string DEF_EXCEL97_CONTENT_TYPE = "Application/x-msexcel";

	private const string DEF_EXCEL2000_CONTENT_TYPE = "Application/vnd.ms-excel";

	private const string DEF_EXCEL2007_CONTENT_TYPE = "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

	private const string DEF_CSV_CONTENT_TYPE = "text/csv";

	internal const string StandardPassword = "VelvetSweatshop";

	internal const char TextQualifier = '"';

	private static readonly TBIFFRecord[] DEF_PIVOTRECORDS;

	private const RegexOptions DEF_REGEX = RegexOptions.None;

	private static readonly Regex ExternSheetRegEx;

	private const string DEF_BOOK_GROUP = "BookName";

	private const string DEF_SHEET_GROUP = "SheetName";

	internal const int DEF_BOOK_SHEET_INDEX = 65534;

	private const string DEF_FORMAT_STYLE_NAME_START = "Format_";

	private static readonly string[] DEF_STREAM_SKIP_COPYING;

	private static readonly char[] DEF_RESERVED_BOOK_CHARS;

	private static readonly int[] PredefinedStyleOutlines;

	private static readonly int[] PredefinedXFs;

	private const string EvaluationWarning = "This file was created using the evaluation version of DocGen Essential XlsIO.";

	private const string EvaluationSheetName = "Evaluation expired";

	internal static readonly Color[] DefaultThemeColors;

	internal static readonly Color[] DefaultThemeColors2013;

	private const int FirstChartColor = 77;

	private const int LastChartColor = 79;

	private static readonly Color[] m_chartColors;

	private const char SheetRangeSeparator = ':';

	internal const int Date1904SystemDifference = 1462;

	internal const string DEF_EXCEL_2013_THEME_VERSION = "153222";

	internal const string DEF_EXCEL_2007_THEME_VERSION = "124226";

	private bool m_enabledCalcEngine;

	private List<BiffRecordRaw> m_records;

	private WorksheetBaseImpl m_ActiveSheet;

	private WorksheetsCollection m_worksheets;

	private StylesCollection m_styles;

	private FontsCollection m_fonts;

	private ExtendedFormatsCollection m_extFormats;

	private List<NameRecord> m_arrNames;

	private Dictionary<int, int> m_modifiedFormatRecord = new Dictionary<int, int>();

	private FormatsCollection m_rawFormats;

	private List<BoundSheetRecord> m_arrBound;

	private SSTDictionary m_SSTDictionary;

	private ExternSheetRecord m_externSheet = (ExternSheetRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExternSheet);

	private List<ContinueRecord> m_continue;

	private List<IReparse> m_arrReparse;

	private string m_strFullName;

	private bool m_bDate1904;

	private bool m_bPrecisionAsDisplayed;

	private bool m_bReadOnly;

	private bool m_bSaved;

	private bool m_bSelFSUsed;

	private bool m_bWorkbookOpening;

	private bool m_bSaving;

	private bool m_bCellProtect;

	private bool m_bWindowProtect;

	private string m_strCodeName = "ThisWorkbook";

	private bool m_bHidePivotFieldList = true;

	private string m_defaultThemeVersion;

	private bool m_bHasMacros;

	private bool m_bHasSummaryInformation;

	private bool m_bHasDocumentSummaryInformation;

	private bool m_bMacrosDisable;

	private List<Color> m_colors;

	private bool m_hasStandardFont;

	private bool m_bOwnPalette;

	private WindowOneRecord m_windowOne;

	private WorkbookNamesCollection m_names;

	private ChartsCollection m_charts;

	private WorkbookObjectsCollection m_arrObjects;

	private PasswordRecord m_password;

	private PasswordRev4Record m_passwordRev4;

	private ProtectionRev4Record m_protectionRev4;

	private bool m_bThrowInFormula = true;

	private WorkbookShapeDataImpl m_shapesData;

	private int m_iFirstUnusedColor = 8;

	private int m_iCurrentObjectId;

	private int m_iCurrentHeaderId;

	private List<ExtendedFormatRecord> m_arrExtFormatRecords;

	private List<ExtendedXFRecord> m_arrXFExtRecords;

	private List<StyleExtRecord> m_arrStyleExtRecords;

	private bool m_bOptimization;

	private bool m_b3dRangesInDV;

	private ExternBookCollection m_externBooks;

	private WorkbookShapeDataImpl m_headerFooterPictures;

	private CalculationOptionsImpl m_calcution;

	private int[] pivotCacheIndexes;

	private int m_dxfPriority;

	private FormulaUtil m_formulaUtil;

	private WorksheetGroup m_sheetGroup;

	private bool m_bDuplicatedNames;

	internal bool m_bWriteProtection;

	private FileSharingRecord m_fileSharing;

	private bool m_bDetectDateTimeInValue = true;

	private int m_iFirstCharSize = -1;

	private int m_iSecondCharSize = -1;

	private string m_strEncryptionPassword;

	internal ExcelEncryptionType m_encryptionType;

	private byte[] m_arrDocId;

	private int m_iMaxRowCount = 65536;

	private int m_iMaxColumnCount = 256;

	private int m_iMaxXFCount = 4095;

	private int m_iMaxIndent = 250;

	private int m_maxImportColumns;

	private OfficeVersion m_version;

	private int m_iDefaultXFIndex = 15;

	private FileDataHolder m_fileDataHolder;

	private double m_dMaxDigitWidth;

	private nint m_ptrHeapHandle;

	private BiffRecordRaw m_bookExt;

	private List<Color> m_themeColors = new List<Color>(DefaultThemeColors);

	private Stream m_controlsStream;

	private int m_iMaxTableIndex = 1;

	private int m_iCountry = 1;

	private Stream m_CustomTableStylesStream;

	private bool m_bIsLoaded;

	private bool m_bIsCreated;

	private Dictionary<string, FontImpl> m_majorFonts;

	private Dictionary<string, FontImpl> m_minorFonts;

	private bool isEqualColor;

	internal bool m_hasApostrophe;

	private bool? m_isStartsOrEndsWith;

	private bool m_isOleObjectCopied;

	private bool m_hasOleObjects;

	private MSODrawingGroupRecord m_drawGroup;

	private bool m_checkFirst;

	private int m_versioncheck;

	internal bool m_isThemeColorsParsed;

	private CompatibilityRecord m_compatibility;

	private Stream m_sstStream;

	private bool m_hasInlineString;

	private bool m_isConverted;

	private List<Stream> m_preservesPivotCache;

	private List<int> m_arrFontIndexes;

	private OfficeParseOptions m_options;

	internal Dictionary<int, int> m_xfCellCount = new Dictionary<int, int>();

	internal bool IsCRCSucceed;

	internal uint crcValue;

	private int beginversion;

	private int m_iLastPivotTableIndex;

	internal Dictionary<string, List<Stream>> m_childElements = new Dictionary<string, List<Stream>>();

	internal int XmlInvalidCharCount = 1;

	private bool m_IsDisposed;

	private List<BiffRecordRaw> m_externalConnection;

	private bool m_bParseOnDemand;

	private RecalcIdRecord m_reCalcId = new RecalcIdRecord();

	private uint m_uCalcIdentifier = 152511u;

	private bool m_isCellModified;

	internal OfficeVersion originalVersion;

	private List<string> m_preservedExternalLinks;

	private string m_algorithmName;

	private byte[] m_hashValue;

	private byte[] m_saltValue;

	private uint m_spinCount;

	private readonly string[] m_customPatterns = new string[28]
	{
		"m/d", "d/m/yyyy", "d-m-yyyy", "dd/MMM/yyyy", "d/MMM/yyyy", "dd-MMM-yyyy", "d-MMM-yyyy", "dd/MMM/yy", "d/MMM/yy", "dd-MMM-yy",
		"d-MMM-yy", "dd-MM-yyyy", "dd-MM-yy", "d-m-yy", "mm-dd-yy", "yyyy-dd-MM", "d-MMM", "dd-MMM", "d/MMM", "dd/MMM",
		"MMM-dd", "MMM/dd", "MMM/d", "MMM/dd", "MMM-yy", "MMM/yy", "MMM-yyyy", "MMM/yyyy"
	};

	private string[] m_DateTimePatterns;

	private const float ScriptFactor = 1.5f;

	private static Dictionary<OfficeSheetType, string> SheetTypeToName;

	public IWorksheet ActiveSheet
	{
		get
		{
			return m_ActiveSheet as IWorksheet;
		}
		internal set
		{
			m_ActiveSheet = value as WorksheetBaseImpl;
		}
	}

	internal List<string> PreservedExternalLinks
	{
		get
		{
			if (m_preservedExternalLinks == null)
			{
				m_preservedExternalLinks = new List<string>(10);
			}
			return m_preservedExternalLinks;
		}
	}

	public int ActiveSheetIndex
	{
		get
		{
			if (m_ActiveSheet == null)
			{
				return -1;
			}
			return m_ActiveSheet.RealIndex;
		}
		set
		{
			if (value < 0 || value >= ObjectCount)
			{
				throw new ArgumentOutOfRangeException("ActiveSheetIndex");
			}
			WorksheetBaseImpl activeSheet = m_ActiveSheet;
			m_ActiveSheet = Objects[value] as WorksheetBaseImpl;
			ISerializableNamedObject serializableNamedObject = (ISerializableNamedObject)m_ActiveSheet;
			WindowOne.SelectedTab = (ushort)serializableNamedObject.RealIndex;
			activeSheet?.Unselect(bCheckNumber: false);
		}
	}

	public string Author
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string CodeName
	{
		get
		{
			return m_strCodeName;
		}
		set
		{
			m_strCodeName = value;
		}
	}

	public bool HidePivotFieldList
	{
		get
		{
			return m_bHidePivotFieldList;
		}
		set
		{
			m_bHidePivotFieldList = value;
		}
	}

	public string DefaultThemeVersion
	{
		get
		{
			return m_defaultThemeVersion;
		}
		set
		{
			m_defaultThemeVersion = value;
		}
	}

	public bool Date1904
	{
		get
		{
			return m_bDate1904;
		}
		set
		{
			m_bDate1904 = value;
		}
	}

	public bool PrecisionAsDisplayed
	{
		get
		{
			return m_bPrecisionAsDisplayed;
		}
		set
		{
			m_bPrecisionAsDisplayed = value;
		}
	}

	public bool IsCellProtection => m_bCellProtect;

	public bool IsWindowProtection => m_bWindowProtect;

	public INames Names
	{
		[DebuggerStepThrough]
		get
		{
			return m_names;
		}
	}

	internal List<BiffRecordRaw> PreserveExternalConnectionDetails
	{
		get
		{
			if (m_externalConnection == null)
			{
				m_externalConnection = new List<BiffRecordRaw>();
			}
			return m_externalConnection;
		}
	}

	public bool ReadOnly
	{
		get
		{
			return m_bReadOnly;
		}
		internal set
		{
			m_bReadOnly = value;
		}
	}

	public bool Saved
	{
		get
		{
			return m_bSaved;
		}
		set
		{
			m_bSaved = value;
		}
	}

	public IStyles Styles
	{
		[DebuggerStepThrough]
		get
		{
			return m_styles;
		}
	}

	public IWorksheets Worksheets
	{
		[DebuggerStepThrough]
		get
		{
			return m_worksheets;
		}
	}

	public bool HasMacros
	{
		get
		{
			return m_bHasMacros;
		}
		internal set
		{
			m_bHasMacros = value;
		}
	}

	public Color[] Palettte => m_colors.ToArray();

	public Color[] Palette => m_colors.ToArray();

	public int DisplayedTab
	{
		get
		{
			return WindowOne.DisplayedTab;
		}
		set
		{
			if ((value < 0 || value > m_arrObjects.Count) && IsCreated)
			{
				throw new ArgumentOutOfRangeException("DisplayedTab", "Displayed tab must be greater than zero and less than Worksheets count");
			}
			WindowOne.DisplayedTab = (ushort)value;
			WindowOne.SelectedTab = (ushort)value;
		}
	}

	public bool ThrowOnUnknownNames
	{
		get
		{
			return m_bThrowInFormula;
		}
		set
		{
			m_bThrowInFormula = value;
		}
	}

	public bool IsHScrollBarVisible
	{
		get
		{
			return WindowOne.IsHScroll;
		}
		set
		{
			WindowOne.IsHScroll = value;
		}
	}

	public bool IsVScrollBarVisible
	{
		get
		{
			return WindowOne.IsVScroll;
		}
		set
		{
			WindowOne.IsVScroll = value;
		}
	}

	public bool DisableMacrosStart
	{
		get
		{
			return m_bMacrosDisable;
		}
		set
		{
			if (value != m_bMacrosDisable)
			{
				m_bMacrosDisable = value;
				Saved = false;
			}
		}
	}

	public double StandardFontSize
	{
		get
		{
			return ((FontImpl)m_fonts[0]).Size;
		}
		set
		{
			if (value != StandardFontSize)
			{
				m_hasStandardFont = true;
				((FontImpl)m_fonts[0]).Size = (int)value;
				FontWrapper fontWrapper = Styles["Normal"].Font as FontWrapper;
				m_dMaxDigitWidth = -1.0;
				if (fontWrapper.Index < 4)
				{
					fontWrapper.InvokeAfterChange();
				}
			}
		}
	}

	internal bool HasStandardFont => m_hasStandardFont;

	public string StandardFont
	{
		get
		{
			return ((FontImpl)m_fonts[0]).FontName;
		}
		set
		{
			if (value != StandardFont)
			{
				m_hasStandardFont = true;
			}
			for (int i = 0; i < 4; i++)
			{
				((FontImpl)m_fonts[0]).FontName = value;
			}
		}
	}

	public bool Allow3DRangesInDataValidation
	{
		get
		{
			return m_b3dRangesInDV;
		}
		set
		{
			m_b3dRangesInDV = value;
		}
	}

	public ICalculationOptions CalculationOptions => m_calcution;

	public string RowSeparator => FormulaUtil.ArrayRowSeparator;

	public string ArgumentsSeparator => FormulaUtil.OperandsSeparator;

	public IWorksheetGroup WorksheetGroup => m_sheetGroup;

	public bool IsRightToLeft
	{
		get
		{
			return m_worksheets.IsRightToLeft;
		}
		set
		{
			m_worksheets.IsRightToLeft = value;
		}
	}

	public bool DisplayWorkbookTabs
	{
		get
		{
			return WindowOne.IsTabs;
		}
		set
		{
			WindowOne.IsTabs = value;
		}
	}

	public ITabSheets TabSheets => m_arrObjects;

	public bool DetectDateTimeInValue
	{
		get
		{
			return m_bDetectDateTimeInValue;
		}
		set
		{
			m_bDetectDateTimeInValue = value;
		}
	}

	public bool UseFastStringSearching
	{
		get
		{
			return m_SSTDictionary.UseHashForSearching;
		}
		set
		{
			m_SSTDictionary.UseHashForSearching = value;
		}
	}

	public bool ReadOnlyRecommended
	{
		get
		{
			if (m_fileSharing != null)
			{
				return m_fileSharing.RecommendReadOnly != 0;
			}
			return false;
		}
		set
		{
			if (value)
			{
				if (m_fileSharing == null)
				{
					m_fileSharing = (FileSharingRecord)BiffRecordFactory.GetRecord(TBIFFRecord.FileSharing);
				}
				m_fileSharing.RecommendReadOnly = 1;
			}
			else if (m_fileSharing != null)
			{
				m_fileSharing.RecommendReadOnly = 0;
			}
		}
	}

	public string PasswordToOpen
	{
		get
		{
			return m_strEncryptionPassword;
		}
		set
		{
			m_strEncryptionPassword = value;
			if (value == null || value.Length == 0)
			{
				m_encryptionType = ExcelEncryptionType.None;
			}
			else
			{
				m_encryptionType = ExcelEncryptionType.Standard;
			}
		}
	}

	public int MaxRowCount => m_iMaxRowCount;

	public int MaxColumnCount => m_iMaxColumnCount;

	public int MaxXFCount => m_iMaxXFCount;

	public int MaxIndent => m_iMaxIndent;

	public int MaxImportColumns
	{
		get
		{
			return m_maxImportColumns;
		}
		set
		{
			m_maxImportColumns = value;
		}
	}

	internal ICharts Charts => m_charts;

	internal int BookCFPriorityCount
	{
		get
		{
			return m_dxfPriority;
		}
		set
		{
			m_dxfPriority = value;
		}
	}

	internal bool EnabledCalcEngine
	{
		get
		{
			return m_enabledCalcEngine;
		}
		set
		{
			m_enabledCalcEngine = value;
		}
	}

	internal OfficeParseOptions Options
	{
		get
		{
			return m_options;
		}
		set
		{
			m_options = value;
		}
	}

	internal int LastPivotTableIndex
	{
		get
		{
			return m_iLastPivotTableIndex;
		}
		set
		{
			m_iLastPivotTableIndex = value;
		}
	}

	internal List<Stream> PreservesPivotCache
	{
		get
		{
			if (m_preservesPivotCache == null)
			{
				m_preservesPivotCache = new List<Stream>();
			}
			return m_preservesPivotCache;
		}
	}

	internal List<int> ArrayFontIndex
	{
		get
		{
			return m_arrFontIndexes;
		}
		set
		{
			m_arrFontIndexes = value;
		}
	}

	public FileDataHolder DataHolder
	{
		get
		{
			return m_fileDataHolder;
		}
		set
		{
			m_fileDataHolder = value;
		}
	}

	public WorkbookNamesCollection InnerNamesColection
	{
		[DebuggerStepThrough]
		get
		{
			return m_names;
		}
	}

	public string FullFileName
	{
		[DebuggerStepThrough]
		get
		{
			return m_strFullName;
		}
		[DebuggerStepThrough]
		internal set
		{
			m_strFullName = value;
		}
	}

	public FontsCollection InnerFonts
	{
		[DebuggerStepThrough]
		get
		{
			return m_fonts;
		}
	}

	public ExtendedFormatsCollection InnerExtFormats
	{
		[DebuggerStepThrough]
		get
		{
			return m_extFormats;
		}
	}

	public FormatsCollection InnerFormats
	{
		[DebuggerStepThrough]
		get
		{
			return m_rawFormats;
		}
	}

	public SSTDictionary InnerSST
	{
		[DebuggerStepThrough]
		get
		{
			return m_SSTDictionary;
		}
	}

	public bool IsWorkbookOpening
	{
		[DebuggerStepThrough]
		get
		{
			return m_bWorkbookOpening;
		}
		[DebuggerStepThrough]
		set
		{
			m_bWorkbookOpening = value;
		}
	}

	public bool Saving
	{
		[DebuggerStepThrough]
		get
		{
			return m_bSaving;
		}
		[DebuggerStepThrough]
		internal set
		{
			m_bSaving = value;
		}
	}

	[CLSCompliant(false)]
	public WindowOneRecord WindowOne
	{
		get
		{
			if (m_windowOne == null)
			{
				m_windowOne = (WindowOneRecord)BiffRecordFactory.GetRecord(TBIFFRecord.WindowOne);
			}
			return m_windowOne;
		}
	}

	public int ObjectCount => m_arrObjects.Count;

	public double MaxDigitWidth
	{
		get
		{
			if (m_dMaxDigitWidth <= 0.0)
			{
				m_dMaxDigitWidth = GetMaxDigitWidth();
				if (m_dMaxDigitWidth == 0.0)
				{
					m_dMaxDigitWidth = 7.0;
				}
			}
			return m_dMaxDigitWidth;
		}
	}

	internal Stream SSTStream
	{
		get
		{
			return m_sstStream;
		}
		set
		{
			m_sstStream = value;
		}
	}

	internal bool HasInlineStrings
	{
		get
		{
			return m_hasInlineString;
		}
		set
		{
			m_hasInlineString = value;
		}
	}

	internal PasswordRecord Password
	{
		get
		{
			if (m_password == null)
			{
				m_password = (PasswordRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Password);
			}
			return m_password;
		}
		set
		{
			m_password = value;
		}
	}

	[CLSCompliant(false)]
	protected PasswordRev4Record PasswordRev4
	{
		get
		{
			if (m_passwordRev4 == null)
			{
				m_passwordRev4 = (PasswordRev4Record)BiffRecordFactory.GetRecord(TBIFFRecord.PasswordRev4);
			}
			return m_passwordRev4;
		}
		set
		{
			m_passwordRev4 = value;
		}
	}

	[CLSCompliant(false)]
	protected ProtectionRev4Record ProtectionRev4
	{
		get
		{
			if (m_protectionRev4 == null)
			{
				m_protectionRev4 = (ProtectionRev4Record)BiffRecordFactory.GetRecord(TBIFFRecord.ProtectionRev4);
			}
			return m_protectionRev4;
		}
		set
		{
			m_protectionRev4 = value;
		}
	}

	public int CurrentObjectId
	{
		get
		{
			return m_iCurrentObjectId;
		}
		set
		{
			if (value > 0)
			{
				m_iCurrentObjectId = value;
			}
		}
	}

	public int CurrentHeaderId
	{
		get
		{
			return m_iCurrentHeaderId;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("shape id");
			}
			m_iCurrentHeaderId = value;
		}
	}

	protected internal List<ExtendedFormatRecord> InnerExtFormatRecords => m_arrExtFormatRecords;

	protected internal List<ExtendedXFRecord> InnerXFExtRecords => m_arrXFExtRecords;

	protected internal WorkbookObjectsCollection Objects
	{
		get
		{
			return m_arrObjects;
		}
		internal set
		{
			m_arrObjects = value;
		}
	}

	protected internal StylesCollection InnerStyles => m_styles;

	protected internal WorksheetsCollection InnerWorksheets => m_worksheets;

	protected internal ChartsCollection InnerCharts => m_charts;

	public ExternBookCollection ExternWorkbooks => m_externBooks;

	public CalculationOptionsImpl InnerCalculation => m_calcution;

	[CLSCompliant(false)]
	public FormulaUtil FormulaUtil
	{
		get
		{
			if (m_formulaUtil == null)
			{
				m_formulaUtil = new FormulaUtil(base.Application, this);
			}
			return m_formulaUtil;
		}
	}

	public WorksheetGroup InnerWorksheetGroup => m_sheetGroup;

	internal bool? IsStartsOrEndsWith
	{
		get
		{
			return m_isStartsOrEndsWith;
		}
		set
		{
			m_isStartsOrEndsWith = value;
		}
	}

	public bool HasDuplicatedNames
	{
		get
		{
			return m_bDuplicatedNames;
		}
		set
		{
			m_bDuplicatedNames = value;
		}
	}

	public WorkbookShapeDataImpl ShapesData => m_shapesData;

	public WorkbookShapeDataImpl HeaderFooterData => m_headerFooterPictures;

	internal ExternSheetRecord ExternSheet => m_externSheet;

	protected internal bool InternalSaved
	{
		get
		{
			return m_bSaved;
		}
		set
		{
			m_bSaved = value;
		}
	}

	public int FirstCharSize
	{
		get
		{
			return m_iFirstCharSize;
		}
		set
		{
			m_iFirstCharSize = value;
		}
	}

	public int SecondCharSize
	{
		get
		{
			return m_iSecondCharSize;
		}
		set
		{
			m_iSecondCharSize = value;
		}
	}

	internal bool IsConverted => m_isConverted;

	public int BeginVersion
	{
		get
		{
			return beginversion;
		}
		set
		{
			beginversion = value;
		}
	}

	public OfficeVersion Version
	{
		get
		{
			return m_version;
		}
		set
		{
			if (m_checkFirst && m_version == OfficeVersion.Excel97to2003 && value != 0)
			{
				m_isConverted = true;
			}
			else if (!m_checkFirst)
			{
				m_checkFirst = true;
			}
			if (value == OfficeVersion.Excel97to2003 && m_versioncheck == 0)
			{
				beginversion = 1;
				m_versioncheck++;
			}
			else if ((m_versioncheck == 0 || beginversion == 2) && value != 0)
			{
				beginversion = 0;
				m_versioncheck++;
			}
			if (m_version >= OfficeVersion.Excel2007 && value >= OfficeVersion.Excel2007)
			{
				m_version = value;
			}
			else if (m_version != value)
			{
				originalVersion = m_version;
				m_version = value;
				m_bHasMacros = false;
				switch (value)
				{
				case OfficeVersion.Excel97to2003:
					m_iMaxRowCount = 65536;
					m_iMaxColumnCount = 256;
					m_extFormats.SetMaxCount(4075);
					m_iMaxXFCount = 4075;
					m_extFormats.SetMaxCount(4095);
					m_iMaxXFCount = 4095;
					m_iMaxIndent = 15;
					ChangeStylesTo97();
					break;
				case OfficeVersion.Excel2007:
				case OfficeVersion.Excel2010:
				case OfficeVersion.Excel2013:
					m_iMaxRowCount = 1048576;
					m_iMaxColumnCount = 16384;
					m_extFormats.SetMaxCount(65000);
					m_iMaxXFCount = 65000;
					m_iMaxIndent = 250;
					_ = originalVersion;
					m_names.Validate();
					break;
				}
				int i = 0;
				for (int count = m_worksheets.Count; i < count; i++)
				{
					((WorksheetImpl)m_worksheets[i]).Version = value;
				}
				if (value == OfficeVersion.Excel97to2003)
				{
					m_SSTDictionary.RemoveUnnecessaryStrings();
				}
				InnerNamesColection?.ConvertFullRowColumnNames(value);
			}
		}
	}

	public int DefaultXFIndex
	{
		get
		{
			return m_iDefaultXFIndex;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("DefaultXFIndex");
			}
			m_iDefaultXFIndex = value;
		}
	}

	public List<Color> InnerPalette => m_colors;

	public Stream ControlsStream
	{
		get
		{
			return m_controlsStream;
		}
		internal set
		{
			m_controlsStream = value;
		}
	}

	internal Stream CustomTableStylesStream
	{
		get
		{
			return m_CustomTableStylesStream;
		}
		set
		{
			m_CustomTableStylesStream = value;
		}
	}

	public int MaxTableIndex
	{
		get
		{
			return m_iMaxTableIndex;
		}
		set
		{
			m_iMaxTableIndex = value;
		}
	}

	internal bool IsCreated => m_bIsCreated;

	public bool IsLoaded => m_bIsLoaded;

	internal Dictionary<string, FontImpl> MajorFonts
	{
		get
		{
			return m_majorFonts;
		}
		set
		{
			m_majorFonts = value;
		}
	}

	internal Dictionary<string, FontImpl> MinorFonts
	{
		get
		{
			return m_minorFonts;
		}
		set
		{
			m_minorFonts = value;
		}
	}

	public bool CheckCompability
	{
		get
		{
			if (m_compatibility == null)
			{
				return false;
			}
			return m_compatibility.NoComptabilityCheck != 0;
		}
		set
		{
			if (m_compatibility == null)
			{
				m_compatibility = new CompatibilityRecord();
			}
			m_compatibility.NoComptabilityCheck = ((!value) ? 1u : 0u);
		}
	}

	internal bool HasApostrophe
	{
		get
		{
			return m_hasApostrophe;
		}
		set
		{
			m_hasApostrophe = value;
		}
	}

	internal bool ParseOnDemand
	{
		get
		{
			return m_bParseOnDemand;
		}
		set
		{
			m_bParseOnDemand = value;
		}
	}

	internal bool IsCellModified
	{
		get
		{
			return m_isCellModified;
		}
		set
		{
			m_isCellModified = value;
		}
	}

	public string AlgorithmName
	{
		get
		{
			return m_algorithmName;
		}
		set
		{
			m_algorithmName = value;
		}
	}

	public byte[] HashValue
	{
		get
		{
			return m_hashValue;
		}
		set
		{
			m_hashValue = value;
		}
	}

	public byte[] SaltValue
	{
		get
		{
			return m_saltValue;
		}
		set
		{
			m_saltValue = value;
		}
	}

	public uint SpinCount
	{
		get
		{
			return m_spinCount;
		}
		set
		{
			m_spinCount = value;
		}
	}

	internal bool IsEqualColor => isEqualColor;

	public double StandardRowHeight
	{
		get
		{
			if (m_worksheets.Count > 0)
			{
				return m_worksheets[0].StandardHeight;
			}
			return GetMaxDigitHeight();
		}
		set
		{
			if (value != StandardRowHeight)
			{
				int i = 0;
				for (int count = m_worksheets.Count; i < count; i++)
				{
					m_worksheets[i].StandardHeight = value;
				}
			}
		}
	}

	public int StandardRowHeightInPixels
	{
		get
		{
			return (int)ApplicationImpl.ConvertToPixels(StandardRowHeight, MeasureUnits.Point);
		}
		set
		{
			double standardRowHeight = ApplicationImpl.ConvertFromPixel(value, MeasureUnits.Point);
			StandardRowHeight = standardRowHeight;
		}
	}

	public event EventHandler OnFileSaved;

	public event ReadOnlyFileEventHandler OnReadOnlyFile;

	public Color GetThemeColor(int color)
	{
		return m_themeColors[color];
	}

	internal Color GetThemeColor2013(int color)
	{
		return DefaultThemeColors2013[color];
	}

	protected internal IExtendedFormat CreateExtFormat(bool bForceAdd)
	{
		ExtendedFormatImpl extendedFormatImpl = new ExtendedFormatImpl(base.Application, this);
		extendedFormatImpl.Index = (ushort)m_extFormats.Count;
		if (bForceAdd)
		{
			m_extFormats.ForceAdd(extendedFormatImpl);
		}
		else
		{
			m_extFormats.Add(extendedFormatImpl);
		}
		return extendedFormatImpl;
	}

	protected internal IExtendedFormat CreateExtFormat(IExtendedFormat baseFormat, bool bForceAdd)
	{
		if (baseFormat == null)
		{
			throw new ArgumentNullException("baseFormat");
		}
		ExtendedFormatImpl extendedFormatImpl = CreateExtFormatWithoutRegister(baseFormat);
		if (bForceAdd)
		{
			m_extFormats.ForceAdd(extendedFormatImpl);
		}
		else
		{
			extendedFormatImpl = m_extFormats.Add(extendedFormatImpl);
		}
		return extendedFormatImpl;
	}

	protected internal ExtendedFormatImpl CreateExtFormatWithoutRegister(IExtendedFormat baseFormat)
	{
		ShapeFillImpl gradient = null;
		if (baseFormat == null)
		{
			throw new ArgumentNullException("baseFormat");
		}
		ExtendedFormatRecord format;
		ExtendedXFRecord xfExt;
		ExtendedFormatImpl extendedFormatImpl;
		if (baseFormat is ExtendedFormatImpl)
		{
			extendedFormatImpl = (ExtendedFormatImpl)baseFormat;
			format = (ExtendedFormatRecord)extendedFormatImpl.Record.Clone();
			xfExt = (ExtendedXFRecord)extendedFormatImpl.XFRecord.Clone();
		}
		else
		{
			if (!(baseFormat is ExtendedFormatWrapper))
			{
				throw new ArgumentException("baseFormat can be only ExtendedFormatImpl or ExtendedFormatImplWrapper classes");
			}
			extendedFormatImpl = ((ExtendedFormatWrapper)baseFormat).Wrapped;
			format = (ExtendedFormatRecord)extendedFormatImpl.Record.Clone();
			xfExt = (ExtendedXFRecord)extendedFormatImpl.XFRecord.Clone();
		}
		if (extendedFormatImpl.Gradient != null)
		{
			gradient = ((ShapeFillImpl)extendedFormatImpl.Gradient).Clone(extendedFormatImpl);
		}
		ExtendedFormatImpl extendedFormatImpl2 = extendedFormatImpl;
		extendedFormatImpl = (extendedFormatImpl = new ExtendedFormatImpl(base.Application, this, format, xfExt));
		extendedFormatImpl.ColorObject.CopyFrom(extendedFormatImpl2.ColorObject, callEvent: false);
		extendedFormatImpl.PatternColorObject.CopyFrom(extendedFormatImpl2.PatternColorObject, callEvent: false);
		extendedFormatImpl.BottomBorderColor.CopyFrom(extendedFormatImpl2.BottomBorderColor, callEvent: false);
		extendedFormatImpl.TopBorderColor.CopyFrom(extendedFormatImpl2.TopBorderColor, callEvent: false);
		extendedFormatImpl.LeftBorderColor.CopyFrom(extendedFormatImpl2.LeftBorderColor, callEvent: false);
		extendedFormatImpl.RightBorderColor.CopyFrom(extendedFormatImpl2.RightBorderColor, callEvent: false);
		extendedFormatImpl.Gradient = gradient;
		return extendedFormatImpl;
	}

	protected internal ExtendedFormatImpl RegisterExtFormat(ExtendedFormatImpl format)
	{
		return RegisterExtFormat(format, forceAdd: false);
	}

	protected internal ExtendedFormatImpl RegisterExtFormat(ExtendedFormatImpl format, bool forceAdd)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		format = (forceAdd ? m_extFormats.ForceAdd(format) : m_extFormats.Add(format));
		return format;
	}

	protected int InsertSelfSupbook()
	{
		return m_externBooks.InsertSelfSupbook();
	}

	protected internal int AddSheetReference(string sheetName)
	{
		string[] array = sheetName.Split(':');
		if (array.Length > 2)
		{
			throw new ArgumentException("sheetName");
		}
		sheetName = array[0];
		string sheetName2 = array[^1];
		IWorksheet worksheet = Worksheets[sheetName];
		IWorksheet worksheet2 = Worksheets[sheetName2];
		if (worksheet != null && worksheet2 != null)
		{
			return AddSheetReference(worksheet, worksheet2);
		}
		Match match = ExternSheetRegEx.Match(sheetName);
		if (match.Success && match.Value == sheetName)
		{
			string text = match.Groups["BookName"].Value;
			int length = text.Length;
			int num = 0;
			if (length == 0)
			{
				return AddBrokenSheetReference();
			}
			if (length >= 2)
			{
				text = text.Substring(1, length - 2);
			}
			string value = match.Groups["SheetName"].Value;
			return AddExternSheetReference(text, value);
		}
		return 0;
	}

	private int AddExternSheetReference(string strBookName, string strSheetName)
	{
		if (strBookName == null)
		{
			throw new ArgumentNullException("strBookName");
		}
		if (strSheetName == null)
		{
			throw new ArgumentNullException("strSheetName");
		}
		int num = -1;
		int num2 = 65534;
		if (strBookName == null || strBookName.Length == 0)
		{
			strBookName = strSheetName;
			strSheetName = null;
		}
		ExternWorkbookImpl externWorkbookImpl = m_externBooks[strBookName];
		if (externWorkbookImpl == null)
		{
			if (!IsWorkbookOpening || Version == OfficeVersion.Excel97to2003 || !int.TryParse(strBookName, out var result))
			{
				throw new ArgumentNullException("Can't find extern workbook");
			}
			num = result - 1;
			externWorkbookImpl = m_externBooks[num];
		}
		else
		{
			num = externWorkbookImpl.Index;
		}
		if (strSheetName != null)
		{
			num2 = externWorkbookImpl.IndexOf(strSheetName);
		}
		return AddSheetReference(num, num2, num2);
	}

	protected internal int AddSheetReference(IWorksheet sheet)
	{
		return AddSheetReference(sheet, sheet);
	}

	protected internal int AddSheetReference(IWorksheet sheet, IWorksheet lastSheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (sheet.Workbook != this)
		{
			throw new ArgumentException("Can't refer to external worksheets");
		}
		int supIndex = InsertSelfSupbook();
		int realIndex = ((ISerializableNamedObject)sheet).RealIndex;
		int realIndex2 = ((ISerializableNamedObject)lastSheet).RealIndex;
		return m_externSheet.AddReference(supIndex, realIndex, realIndex2);
	}

	protected internal int AddSheetReference(ITabSheet sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (sheet.Workbook != this)
		{
			throw new ArgumentException("Can't refer to external worksheets");
		}
		if (!(sheet is IWorksheet))
		{
			return -1;
		}
		int supIndex = InsertSelfSupbook();
		int realIndex = ((ISerializableNamedObject)sheet).RealIndex;
		return m_externSheet.AddReference(supIndex, realIndex, realIndex);
	}

	protected internal int AddSheetReference(int supIndex, int firstSheetIndex, int lastSheetIndex)
	{
		return m_externSheet.AddReference(supIndex, firstSheetIndex, lastSheetIndex);
	}

	protected internal int AddBrokenSheetReference()
	{
		int supIndex = InsertSelfSupbook();
		return m_externSheet.AddReference(supIndex, 65535, 65535);
	}

	protected internal void DecreaseSheetIndex(int index)
	{
		if (m_externSheet == null || m_externSheet.Refs == null)
		{
			return;
		}
		int firstInternalIndex = m_externBooks.GetFirstInternalIndex();
		ExternSheetRecord.TREF[] refs = m_externSheet.Refs;
		int i = 0;
		for (int num = refs.Length; i < num; i++)
		{
			ExternSheetRecord.TREF tREF = refs[i];
			if (tREF.SupBookIndex == firstInternalIndex)
			{
				int firstSheet = tREF.FirstSheet;
				if (firstSheet > index && firstSheet != 65535 && firstSheet != 65534)
				{
					tREF.FirstSheet--;
				}
				else if (firstSheet == index)
				{
					tREF.FirstSheet = ushort.MaxValue;
				}
				firstSheet = tREF.LastSheet;
				if (firstSheet > index && firstSheet != 65535 && firstSheet != 65534)
				{
					tREF.LastSheet--;
				}
				else if (firstSheet == index)
				{
					tREF.LastSheet = ushort.MaxValue;
				}
			}
		}
	}

	protected internal void IncreaseSheetIndex(int index)
	{
		if (m_externSheet == null || m_externSheet.Refs == null)
		{
			return;
		}
		ExternSheetRecord.TREF[] refs = m_externSheet.Refs;
		int i = 0;
		for (int num = refs.Length; i < num; i++)
		{
			ExternSheetRecord.TREF tREF = refs[i];
			if (tREF.FirstSheet >= index)
			{
				tREF.FirstSheet++;
			}
			if (tREF.LastSheet >= index)
			{
				tREF.LastSheet++;
			}
		}
	}

	protected internal void MoveSheetIndex(int iOldIndex, int iNewIndex)
	{
		if (m_externSheet == null || m_externSheet.Refs == null || iOldIndex == iNewIndex)
		{
			return;
		}
		ExternSheetRecord.TREF[] refs = m_externSheet.Refs;
		int i = 0;
		for (int num = refs.Length; i < num; i++)
		{
			if (IsLocalReference(i))
			{
				ExternSheetRecord.TREF tREF = refs[i];
				tREF.FirstSheet = (ushort)GetMovedSheetIndex(tREF.FirstSheet, iOldIndex, iNewIndex);
				tREF.LastSheet = (ushort)GetMovedSheetIndex(tREF.LastSheet, iOldIndex, iNewIndex);
			}
		}
	}

	protected internal void UpdateActiveSheetAfterMove(int iOldIndex, int iNewIndex)
	{
		int num = ActiveSheetIndex;
		if (iOldIndex == num)
		{
			num = iNewIndex;
		}
		else if (iOldIndex < iNewIndex)
		{
			if (num < iOldIndex && num >= iNewIndex)
			{
				num++;
			}
		}
		else if (num <= iNewIndex && num > iOldIndex)
		{
			num--;
		}
		ActiveSheetIndex = num;
	}

	private int GetMovedSheetIndex(int iCurIndex, int iOldIndex, int iNewIndex)
	{
		if (iOldIndex == iNewIndex)
		{
			return iCurIndex;
		}
		if (iCurIndex == iOldIndex)
		{
			return iNewIndex;
		}
		int num = Math.Min(iOldIndex, iNewIndex);
		int num2 = Math.Max(iOldIndex, iNewIndex);
		if (iCurIndex < num || iCurIndex > num2)
		{
			return iCurIndex;
		}
		if (iOldIndex > iNewIndex)
		{
			return iCurIndex + 1;
		}
		return iCurIndex - 1;
	}

	protected internal string GetSheetNameByReference(int reference)
	{
		return GetSheetNameByReference(reference, throwArgumentOutOfRange: false);
	}

	protected internal string GetSheetNameByReference(int reference, bool throwArgumentOutOfRange)
	{
		string result = null;
		if (m_externSheet.RefCount <= reference || reference < 0)
		{
			if (throwArgumentOutOfRange)
			{
				throw new ArgumentOutOfRangeException("reference");
			}
			return null;
		}
		ExternSheetRecord.TREF tREF = m_externSheet.Refs[reference];
		int supBookIndex = tREF.SupBookIndex;
		if (supBookIndex > m_externBooks.Count)
		{
			throw new ParseException();
		}
		ExternWorkbookImpl externWorkbookImpl = m_externBooks[supBookIndex];
		try
		{
			result = ((!externWorkbookImpl.IsInternalReference) ? GetExternalSheetNameByReference(externWorkbookImpl, tREF, supBookIndex) : GetInternalSheetNameByReference(tREF));
		}
		catch (Exception)
		{
		}
		return result;
	}

	private string GetExternalSheetNameByReference(ExternWorkbookImpl book, ExternSheetRecord.TREF reference, int iSupBook)
	{
		int firstSheet = reference.FirstSheet;
		string directoryName = GetDirectoryName(book.URL);
		string text = null;
		string text2 = book.URL.Split('/')[^1];
		if (m_bSaving)
		{
			int num = 0;
			for (int num2 = iSupBook - 1; num2 >= 0; num2--)
			{
				ExternWorkbookImpl externWorkbookImpl = m_externBooks[num2];
				if (externWorkbookImpl.IsInternalReference || string.IsNullOrEmpty(externWorkbookImpl.URL))
				{
					num++;
				}
			}
			text = $"[{iSupBook - num + 1}]";
		}
		else
		{
			text = directoryName + "[" + text2 + "]";
		}
		return text + book.GetSheetName(firstSheet);
	}

	private string GetInternalSheetNameByReference(ExternSheetRecord.TREF reference)
	{
		string text = null;
		int firstSheet = reference.FirstSheet;
		if (firstSheet == 65535)
		{
			text = "#REF";
		}
		else
		{
			if (ObjectCount <= firstSheet || firstSheet < 0)
			{
				throw new ParseException();
			}
			object obj = Objects[firstSheet];
			if (obj is IWorksheet)
			{
				text = ((IWorksheet)obj).Name;
			}
			if (reference.FirstSheet != reference.LastSheet)
			{
				obj = Objects[reference.LastSheet];
				if (obj is IWorksheet)
				{
					text = text + ":" + ((IWorksheet)obj).Name;
				}
			}
		}
		return text;
	}

	private string GetDirectoryName(string url)
	{
		if (url == null)
		{
			return null;
		}
		string text = null;
		if (url.StartsWith("http://"))
		{
			int num = url.LastIndexOf('/');
			text = url.Substring(0, num + 1);
		}
		else
		{
			text = url;
			if (text != null && text.Length > 0 && text[text.Length - 1] != '\\')
			{
				text += "\\";
			}
		}
		return text;
	}

	protected internal IWorksheet GetSheetByReference(int reference)
	{
		return GetSheetByReference(reference, bThrowExceptions: true);
	}

	protected internal IWorksheet GetSheetByReference(int reference, bool bThrowExceptions)
	{
		if (m_externSheet.RefCount <= reference || reference < 0)
		{
			if (bThrowExceptions)
			{
				throw new ArgumentOutOfRangeException("reference");
			}
			return null;
		}
		ExternSheetRecord.TREF tREF = m_externSheet.Refs[reference];
		int supBookIndex = tREF.SupBookIndex;
		if (supBookIndex > m_externBooks.Count)
		{
			if (bThrowExceptions)
			{
				throw new ParseException();
			}
			return null;
		}
		if (!m_externBooks[supBookIndex].IsInternalReference)
		{
			if (bThrowExceptions)
			{
				throw new ParseException();
			}
			return null;
		}
		int firstSheet = tREF.FirstSheet;
		if (ObjectCount <= firstSheet || firstSheet < 0)
		{
			if (bThrowExceptions)
			{
				throw new ParseException();
			}
			return null;
		}
		object obj = Objects[firstSheet];
		if (obj is IWorksheet)
		{
			return (IWorksheet)obj;
		}
		if (bThrowExceptions)
		{
			throw new ArgumentOutOfRangeException("Can't find worksheet at the specified index");
		}
		return null;
	}

	protected internal void CheckForInternalReference(int iRef)
	{
		if (m_externSheet.RefCount <= iRef || iRef < 0)
		{
			throw new ArgumentOutOfRangeException("iRef");
		}
		int supBookIndex = m_externSheet.Refs[iRef].SupBookIndex;
		_ = m_externSheet.Refs[iRef];
		if (supBookIndex > m_externBooks.Count)
		{
			throw new ParseException();
		}
		if (!m_externBooks[supBookIndex].IsInternalReference)
		{
			throw new NotSupportedException("External indexes are not supported in current version.");
		}
	}

	protected internal bool IsLocalReference(int reference)
	{
		if (m_externSheet.RefCount <= reference || reference < 0)
		{
			return false;
		}
		int supBookIndex = m_externSheet.Refs[reference].SupBookIndex;
		if (supBookIndex > m_externBooks.Count)
		{
			return false;
		}
		return m_externBooks[supBookIndex].IsInternalReference;
	}

	public bool IsExternalReference(int reference)
	{
		if (reference == 65535)
		{
			return false;
		}
		if (m_externSheet.RefCount < 0 || m_externSheet.RefCount <= reference)
		{
			return false;
		}
		ExternSheetRecord.TREF tREF = m_externSheet.Refs[reference];
		if (tREF.FirstSheet == ushort.MaxValue)
		{
			return false;
		}
		int supBookIndex = tREF.SupBookIndex;
		if (supBookIndex < 0 || supBookIndex >= m_externBooks.Count)
		{
			throw new ArgumentOutOfRangeException("supbookIndex");
		}
		return !m_externBooks[supBookIndex].IsInternalReference;
	}

	internal void AddForReparse(IReparse reparse)
	{
		m_arrReparse.Add(reparse);
	}

	protected internal int CurrentStyleNumber(string pre)
	{
		int num = 0;
		IStyles styles = Styles;
		int i = 0;
		for (int count = styles.Count; i < count; i++)
		{
			string name = styles[i].Name;
			int num2 = name.IndexOf(pre);
			if (num2 >= 0 && double.TryParse(name.Substring(num2 + pre.Length, name.Length - pre.Length - num2), NumberStyles.Integer, null, out var result))
			{
				int num3 = (int)result;
				if (num3 > num)
				{
					num = num3;
				}
			}
		}
		return num;
	}

	protected double Sqr(double value)
	{
		return value * value;
	}

	protected internal double ColorDistance(Color color1, Color color2)
	{
		return Math.Sqrt(Sqr(color1.R - color2.R) + Sqr(color1.B - color2.B) + Sqr(color1.G - color2.G));
	}

	public void ClearInternalReferences()
	{
		m_externSheet.Refs = new ExternSheetRecord.TREF[0];
	}

	private void RaiseSavedEvent()
	{
		if (this.OnFileSaved != null)
		{
			this.OnFileSaved(this, EventArgs.Empty);
		}
	}

	public IExtendedFormat GetExtFormat(int index)
	{
		return m_extFormats[index];
	}

	public void UpdateFormula(IRange sourceRange, IRange destRange)
	{
		if (sourceRange == null)
		{
			throw new ArgumentNullException("sourceRange");
		}
		if (destRange == null)
		{
			throw new ArgumentNullException("destRange");
		}
		RangeImpl rangeImpl = (RangeImpl)sourceRange;
		RangeImpl rangeImpl2 = (RangeImpl)destRange;
		WorksheetImpl innerWorksheet = rangeImpl.InnerWorksheet;
		WorksheetImpl innerWorksheet2 = rangeImpl2.InnerWorksheet;
		int iSourceIndex = AddSheetReference(innerWorksheet);
		int iDestIndex = AddSheetReference(innerWorksheet2);
		Rectangle rectSource = Rectangle.FromLTRB(rangeImpl.FirstColumn - 1, rangeImpl.FirstRow - 1, rangeImpl.LastColumn - 1, rangeImpl.LastRow - 1);
		Rectangle rectDest = Rectangle.FromLTRB(rangeImpl2.FirstColumn - 1, rangeImpl2.FirstRow - 1, rangeImpl2.LastColumn - 1, rangeImpl2.LastRow - 1);
		UpdateFormula(iSourceIndex, rectSource, iDestIndex, rectDest);
	}

	public void UpdateFormula(int iSourceIndex, Rectangle rectSource, int iDestIndex, Rectangle rectDest)
	{
		int i = 0;
		for (int count = m_arrObjects.Count; i < count; i++)
		{
			WorksheetBaseImpl worksheetBaseImpl = m_arrObjects[i] as WorksheetBaseImpl;
			int iCurIndex = AddSheetReference(worksheetBaseImpl);
			worksheetBaseImpl.UpdateFormula(iCurIndex, iSourceIndex, rectSource, iDestIndex, rectDest);
		}
	}

	public int GetReferenceIndex(int iNameBookIndex)
	{
		return m_externSheet.GetBookReference(iNameBookIndex);
	}

	public int GetBookIndex(int iReferenceIndex)
	{
		ExternSheetRecord.TREF[] refs = m_externSheet.Refs;
		if (iReferenceIndex < 0 || iReferenceIndex > refs.Length - 1)
		{
			throw new ArgumentOutOfRangeException("iReferenceIndex", "Value cannot be less than 0 and greater than arrRefs.Count - 1");
		}
		return m_externSheet.Refs[iReferenceIndex].SupBookIndex;
	}

	public ExternWorksheetImpl GetExternSheet(int referenceIndex)
	{
		ExternSheetRecord.TREF[] refs = m_externSheet.Refs;
		if (referenceIndex < 0 || referenceIndex > refs.Length - 1)
		{
			throw new ArgumentOutOfRangeException("referenceIndex", "Value cannot be less than 0 and greater than arrRefs.Count - 1");
		}
		ExternWorksheetImpl result = null;
		ExternSheetRecord.TREF tREF = m_externSheet.Refs[referenceIndex];
		int firstSheet;
		if ((firstSheet = tREF.FirstSheet) == tREF.LastSheet && firstSheet != 65535)
		{
			int supBookIndex = tREF.SupBookIndex;
			result = m_externBooks[supBookIndex].Worksheets.Values[firstSheet];
		}
		return result;
	}

	public string EncodeName(string strName)
	{
		if (strName == null || strName.Length == 0)
		{
			return strName;
		}
		if (strName.IndexOfAny(DEF_RESERVED_BOOK_CHARS) != -1 || strName == " ")
		{
			return strName.Replace('|', '\u0003');
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0001');
		bool flag = strName.StartsWith("http:");
		int startIndex = 0;
		int num = strName?.Length ?? 0;
		if (strName.StartsWith("\\\\"))
		{
			stringBuilder.Append('\u0001');
			stringBuilder.Append('@');
			startIndex = "\\\\".Length;
		}
		else if (flag)
		{
			stringBuilder.Append('\u0005');
			stringBuilder.Append((char)strName.Length);
			stringBuilder.Append(strName);
		}
		else if (num > 2 && strName[2] == '\\')
		{
			stringBuilder.Append('\u0001');
			char value = strName[0];
			stringBuilder.Append(value);
			startIndex = 3;
		}
		else if (strName[0] == '\\')
		{
			stringBuilder.Append('\u0006');
			strName = UtilityMethods.RemoveFirstCharUnsafe(strName);
		}
		if (!flag)
		{
			int length = strName.Length;
			strName = strName.Substring(startIndex);
			string[] array = strName.Split('\\', '/');
			length = array.Length;
			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append(array[i]);
				if (i != length - 1)
				{
					stringBuilder.Append('\u0003');
				}
			}
		}
		return stringBuilder.ToString();
	}

	[CLSCompliant(false)]
	public bool ModifyRecordToSkipStyle(BiffRecordRaw record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		FontRecord record2 = ((FontImpl)InnerFonts[0]).Record;
		switch (record.TypeCode)
		{
		case TBIFFRecord.ChartFbi:
		{
			ChartFbiRecord obj = (ChartFbiRecord)record;
			obj.FontIndex = 0;
			obj.AppliedFontHeight = record2.FontHeight;
			return false;
		}
		case TBIFFRecord.ChartAlruns:
		{
			ChartAlrunsRecord.TRuns[] runs = ((ChartAlrunsRecord)record).Runs;
			int i = 0;
			for (int num = runs.Length; i < num; i++)
			{
				runs[i].FontIndex = 0;
			}
			break;
		}
		case TBIFFRecord.ChartFontx:
			((ChartFontxRecord)record).FontIndex = 0;
			break;
		}
		return true;
	}

	[CLSCompliant(false)]
	public void ModifyRecordToSkipStyle(BiffRecordRaw[] arrRecords)
	{
		if (arrRecords == null)
		{
			throw new ArgumentNullException("arrRecords");
		}
		FontRecord record = ((FontImpl)m_fonts[0]).Record;
		int i = 0;
		for (int num = arrRecords.Length; i < num; i++)
		{
			BiffRecordRaw biffRecordRaw = arrRecords[i];
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.ChartFbi:
			{
				ChartFbiRecord obj = (ChartFbiRecord)biffRecordRaw;
				obj.FontIndex = 0;
				obj.AppliedFontHeight = record.FontHeight;
				break;
			}
			case TBIFFRecord.ChartAlruns:
			{
				ChartAlrunsRecord.TRuns[] runs = ((ChartAlrunsRecord)biffRecordRaw).Runs;
				int j = 0;
				for (int num2 = runs.Length; j < num2; j++)
				{
					runs[j].FontIndex = 0;
				}
				break;
			}
			case TBIFFRecord.ChartFontx:
				((ChartFontxRecord)biffRecordRaw).FontIndex = 0;
				break;
			}
		}
	}

	private bool CompareColors(Color color1, Color color2)
	{
		if (color1.R == color2.R && color1.G == color2.G)
		{
			return color1.B == color2.B;
		}
		return false;
	}

	public void RemoveExtenededFormatIndex(int xfIndex)
	{
		Dictionary<int, int> dictFormats = m_extFormats.RemoveAt(xfIndex);
		int i = 0;
		for (int count = m_arrObjects.Count; i < count; i++)
		{
			(m_arrObjects[i] as WorksheetBaseImpl).UpdateExtendedFormatIndex(dictFormats);
		}
		m_styles.UpdateStyleRecords();
	}

	private void AddLicenseWorksheet()
	{
		if (base.AppImplementation.EvalExpired)
		{
			IWorksheet worksheet = Worksheets.Create("Evaluation expired");
			worksheet.TabColorRGB = ColorExtension.Red;
			worksheet["A1"].Text = "This file was created using the evaluation version of DocGen Essential XlsIO.";
			IOfficeFont font = worksheet["A1"].CellStyle.Font;
			font.Size = 14.0;
			font.Bold = true;
			font.RGBColor = ColorExtension.Red;
			string text = string.Empty;
			Random random = new Random((int)DateTime.Now.Ticks);
			for (int i = 0; i < 10; i++)
			{
				byte b = (byte)(random.Next(26) + 65);
				string text2 = text;
				char c = (char)b;
				text = text2 + c;
			}
			worksheet.Protect(text, OfficeSheetProtection.All);
			worksheet.Activate();
		}
	}

	private void CheckLicensingSheet()
	{
		if (base.AppImplementation.EvalExpired)
		{
			m_worksheets["Evaluation expired"]?.Remove();
		}
	}

	private bool CheckProtectionContent(IWorksheet sheet)
	{
		IRange range = sheet["A1"];
		if (range.Text == "This file was created using the evaluation version of DocGen Essential XlsIO." && range.ColumnWidth > 8.0 && range.RowHeight > 10.0 && range.CellStyle.Font.Size > 10.0 && sheet.TopVisibleRow == 1 && sheet.LeftVisibleColumn == 1)
		{
			return sheet.Visibility == OfficeWorksheetVisibility.Visible;
		}
		return false;
	}

	private void OptimizeReferences()
	{
		int refCount = m_externSheet.RefCount;
		bool[] array = new bool[refCount];
		int i = 0;
		for (int count = m_arrObjects.Count; i < count; i++)
		{
			(m_arrObjects[i] as WorksheetBaseImpl).MarkUsedReferences(array);
		}
		m_names.MarkUsedReferences(array);
		int[] array2 = new int[refCount];
		int num = 0;
		for (int j = 0; j < refCount; j++)
		{
			if (array[j])
			{
				array2[j] = j - num;
				continue;
			}
			array2[j] = -1;
			num++;
		}
		UpdateReferenceIndexes(array2);
	}

	private void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		int i = 0;
		for (int count = m_arrObjects.Count; i < count; i++)
		{
			(m_arrObjects[i] as WorksheetBaseImpl).UpdateReferenceIndexes(arrUpdatedIndexes);
		}
		m_names.UpdateReferenceIndexes(arrUpdatedIndexes);
		ExternSheetRecord.TREF[] refs = m_externSheet.Refs;
		List<ExternSheetRecord.TREF> list = new List<ExternSheetRecord.TREF>();
		int j = 0;
		for (int num = refs.Length; j < num; j++)
		{
			if (arrUpdatedIndexes[j] >= 0)
			{
				list.Add(refs[j]);
			}
		}
		m_externSheet.Refs = list.ToArray();
	}

	static WorkbookImpl()
	{
		DEF_PALETTE = new Color[64]
		{
			ColorExtension.Black,
			ColorExtension.White,
			ColorExtension.Red,
			Color.FromArgb(255, 0, 255, 0),
			ColorExtension.Blue,
			ColorExtension.Yellow,
			ColorExtension.Magenta,
			ColorExtension.Cyan,
			Color.FromArgb(255, 0, 0, 0),
			Color.FromArgb(255, 255, 255, 255),
			Color.FromArgb(255, 255, 0, 0),
			Color.FromArgb(255, 0, 255, 0),
			Color.FromArgb(255, 0, 0, 255),
			Color.FromArgb(255, 255, 255, 0),
			Color.FromArgb(255, 255, 0, 255),
			Color.FromArgb(255, 0, 255, 255),
			Color.FromArgb(255, 128, 0, 0),
			Color.FromArgb(255, 0, 128, 0),
			Color.FromArgb(255, 0, 0, 128),
			Color.FromArgb(255, 128, 128, 0),
			Color.FromArgb(255, 128, 0, 128),
			Color.FromArgb(255, 0, 128, 128),
			Color.FromArgb(255, 192, 192, 192),
			Color.FromArgb(255, 128, 128, 128),
			Color.FromArgb(255, 153, 153, 255),
			Color.FromArgb(255, 153, 51, 102),
			Color.FromArgb(255, 255, 255, 204),
			Color.FromArgb(255, 204, 255, 255),
			Color.FromArgb(255, 102, 0, 102),
			Color.FromArgb(255, 255, 128, 128),
			Color.FromArgb(255, 0, 102, 204),
			Color.FromArgb(255, 204, 204, 255),
			Color.FromArgb(255, 0, 0, 128),
			Color.FromArgb(255, 255, 0, 255),
			Color.FromArgb(255, 255, 255, 0),
			Color.FromArgb(255, 0, 255, 255),
			Color.FromArgb(255, 128, 0, 128),
			Color.FromArgb(255, 128, 0, 0),
			Color.FromArgb(255, 0, 128, 128),
			Color.FromArgb(255, 0, 0, 255),
			Color.FromArgb(255, 0, 204, 255),
			Color.FromArgb(255, 204, 255, 255),
			Color.FromArgb(255, 204, 255, 204),
			Color.FromArgb(255, 255, 255, 153),
			Color.FromArgb(255, 153, 204, 255),
			Color.FromArgb(255, 255, 153, 204),
			Color.FromArgb(255, 204, 153, 255),
			Color.FromArgb(255, 255, 204, 153),
			Color.FromArgb(255, 51, 102, 255),
			Color.FromArgb(255, 51, 204, 204),
			Color.FromArgb(255, 153, 204, 0),
			Color.FromArgb(255, 255, 204, 0),
			Color.FromArgb(255, 255, 153, 0),
			Color.FromArgb(255, 255, 102, 0),
			Color.FromArgb(255, 102, 102, 153),
			Color.FromArgb(255, 150, 150, 150),
			Color.FromArgb(255, 0, 51, 102),
			Color.FromArgb(255, 51, 153, 102),
			Color.FromArgb(255, 0, 51, 0),
			Color.FromArgb(255, 51, 51, 0),
			Color.FromArgb(255, 153, 51, 0),
			Color.FromArgb(255, 153, 51, 102),
			Color.FromArgb(255, 51, 51, 153),
			Color.FromArgb(255, 51, 51, 51)
		};
		DefaultTints = new double[16]
		{
			-0.0499893185216834, -0.249977111117893, -0.1499984740745262, -0.3499862666707358, -0.499984740745262, 0.3499862666707358, 0.499984740745262, 0.249977111117893, 0.1499984740745262, 0.0499893185216834,
			0.7999816888943144, 0.5999938962981048, 0.3999755851924192, -0.0999786370433668, -0.749992370372631, -0.8999908444471572
		};
		ThemeColorPalette = new Color[10][]
		{
			new Color[5]
			{
				Color.FromArgb(255, 242, 242, 242),
				Color.FromArgb(255, 191, 191, 191),
				Color.FromArgb(255, 217, 217, 217),
				Color.FromArgb(255, 166, 166, 166),
				Color.FromArgb(255, 128, 128, 128)
			},
			new Color[10]
			{
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 89, 89, 89),
				Color.FromArgb(255, 128, 128, 128),
				Color.FromArgb(255, 64, 64, 64),
				Color.FromArgb(255, 38, 38, 38),
				Color.FromArgb(255, 13, 13, 13)
			},
			new Color[16]
			{
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 196, 189, 151),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 148, 138, 84),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 221, 217, 196),
				Color.FromArgb(255, 73, 69, 41),
				Color.FromArgb(255, 29, 27, 16)
			},
			new Color[13]
			{
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 22, 54, 92),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 15, 36, 62),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 197, 217, 241),
				Color.FromArgb(255, 141, 180, 226),
				Color.FromArgb(255, 83, 141, 213)
			},
			new Color[13]
			{
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 54, 96, 146),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 36, 64, 98),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 220, 230, 241),
				Color.FromArgb(255, 184, 204, 228),
				Color.FromArgb(255, 149, 179, 215)
			},
			new Color[13]
			{
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 150, 54, 52),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 99, 37, 35),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 242, 220, 219),
				Color.FromArgb(255, 230, 184, 183),
				Color.FromArgb(255, 218, 150, 148)
			},
			new Color[13]
			{
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 118, 147, 60),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 79, 98, 40),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 235, 241, 222),
				Color.FromArgb(255, 216, 228, 188),
				Color.FromArgb(255, 196, 215, 155)
			},
			new Color[13]
			{
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 96, 73, 122),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 64, 49, 81),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 228, 223, 236),
				Color.FromArgb(255, 204, 192, 218),
				Color.FromArgb(255, 177, 160, 199)
			},
			new Color[13]
			{
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 49, 134, 155),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 33, 89, 103),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 218, 238, 243),
				Color.FromArgb(255, 183, 222, 232),
				Color.FromArgb(255, 146, 205, 220)
			},
			new Color[13]
			{
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 226, 107, 10),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 151, 71, 6),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 253, 233, 217),
				Color.FromArgb(255, 252, 213, 180),
				Color.FromArgb(255, 250, 191, 143)
			}
		};
		DEF_PIVOTRECORDS = new TBIFFRecord[8]
		{
			TBIFFRecord.StreamId,
			TBIFFRecord.PivotViewSource,
			TBIFFRecord.DCONRef,
			TBIFFRecord.DCONBIN,
			TBIFFRecord.DCONNAME,
			TBIFFRecord.DCON,
			TBIFFRecord.PivotViewAdditionalInfo,
			TBIFFRecord.ExternalSourceInfo
		};
		ExternSheetRegEx = new Regex("(?<BookName>\\[[\\S^ ']+\\])?(?<SheetName>[\\S ]+)", RegexOptions.None);
		DEF_STREAM_SKIP_COPYING = new string[2] { "\u0005SummaryInformation", "\u0005DocumentSummaryInformation" };
		DEF_RESERVED_BOOK_CHARS = new char[9] { '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b', '|' };
		PredefinedStyleOutlines = new int[6] { 0, 3, 4, 5, 6, 7 };
		PredefinedXFs = new int[6] { 0, 16, 18, 20, 17, 19 };
		DefaultThemeColors = new Color[12]
		{
			Color.FromArgb(255, 255, 255, 255),
			Color.FromArgb(255, 0, 0, 0),
			ColorExtension.FromArgb(15658209),
			ColorExtension.FromArgb(2050429),
			ColorExtension.FromArgb(5210557),
			ColorExtension.FromArgb(12603469),
			ColorExtension.FromArgb(10206041),
			ColorExtension.FromArgb(8414370),
			ColorExtension.FromArgb(4959430),
			ColorExtension.FromArgb(16225862),
			ColorExtension.FromArgb(255),
			ColorExtension.FromArgb(8388736)
		};
		DefaultThemeColors2013 = new Color[12]
		{
			Color.FromArgb(255, 255, 255, 255),
			Color.FromArgb(255, 0, 0, 0),
			ColorExtension.FromArgb(15197926),
			ColorExtension.FromArgb(4478058),
			ColorExtension.FromArgb(6003669),
			ColorExtension.FromArgb(15564081),
			ColorExtension.FromArgb(10855845),
			ColorExtension.FromArgb(16760832),
			ColorExtension.FromArgb(4485828),
			ColorExtension.FromArgb(7384391),
			ColorExtension.FromArgb(353217),
			ColorExtension.FromArgb(9785202)
		};
		m_chartColors = new Color[3]
		{
			ColorExtension.ChartForeground,
			ColorExtension.ChartBackground,
			ColorExtension.ChartNeutral
		};
		SheetTypeToName = new Dictionary<OfficeSheetType, string>(5);
		SheetTypeToName.Add(OfficeSheetType.Chart, "Charts");
		SheetTypeToName.Add(OfficeSheetType.DialogSheet, "Dialogs");
		SheetTypeToName.Add(OfficeSheetType.Excel4IntlMacroSheet, "Excel 4.0 Intl Marcos");
		SheetTypeToName.Add(OfficeSheetType.Excel4MacroSheet, "Excel 4.0 Macros");
		SheetTypeToName.Add(OfficeSheetType.Worksheet, "Worksheets");
	}

	public WorkbookImpl(IApplication application, object parent, OfficeVersion version)
		: this(application, parent, application.SheetsInNewWorkbook, version)
	{
	}

	public WorkbookImpl(IApplication application, object parent, int sheetQuantity, OfficeVersion version)
		: base(application, parent)
	{
		InitializeCollections();
		Version = version;
		InsertDefaultFonts();
		InsertDefaultValues();
		m_bReadOnly = false;
		m_bIsCreated = true;
		m_worksheets.EnsureCapacity(sheetQuantity);
		for (int i = 0; i < sheetQuantity; i++)
		{
			m_worksheets.Add($"Sheet{i + 1}");
		}
		m_worksheets[0].Activate();
	}

	public WorkbookImpl(IApplication application, object parent, Stream stream, OfficeParseOptions options, bool bReadOnly, string password, OfficeVersion version)
		: base(application, parent)
	{
		m_bOptimization = application.OptimizeFonts;
		InitializeCollections();
		Version = version;
	}

	public WorkbookImpl(IApplication application, object parent, Stream stream, string separator, int row, int column, OfficeVersion version, string fileName, Encoding encoding)
		: this(application, parent, 1, version)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (separator == null)
		{
			throw new ArgumentNullException("separator");
		}
		if (separator.Length == 0)
		{
			throw new ArgumentException("separator");
		}
		if (encoding == null)
		{
			encoding = Encoding.UTF8;
		}
		m_bIsLoaded = true;
		StreamReader streamToRead = new StreamReader(stream, encoding);
		m_bWorkbookOpening = true;
		if (m_ActiveSheet != null)
		{
			bool isValid = IsValidDocument(stream, encoding, separator);
			((WorksheetImpl)m_ActiveSheet).Parse(streamToRead, separator, row, column, isValid);
		}
		m_bWorkbookOpening = false;
	}

	public WorkbookImpl(IApplication application, object parent, Stream stream, OfficeVersion version)
		: this(application, parent, stream, OfficeParseOptions.Default, version)
	{
	}

	public WorkbookImpl(IApplication application, object parent, Stream stream, OfficeParseOptions options, OfficeVersion version)
		: base(application, parent)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_bOptimization = application.OptimizeFonts;
		m_options = options;
		InitializeCollections();
		Version = version;
		ParseStream(stream, null, version, options);
	}

	public WorkbookImpl(IApplication application, object parent, XmlReader reader, OfficeXmlOpenType openType)
		: base(application, parent)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		InitializeCollections();
		Version = application.DefaultVersion;
		InsertDefaultFonts();
		InsertDefaultValues();
		m_bReadOnly = false;
		if (openType != 0)
		{
			throw new ArgumentOutOfRangeException("cannot specified xml open type.");
		}
	}

	protected void InitializeCollections()
	{
		m_arrObjects = new WorkbookObjectsCollection(base.Application, this);
		m_worksheets = new WorksheetsCollection(base.Application, this);
		m_styles = new StylesCollection(base.Application, this);
		m_colors = new List<Color>(DEF_PALETTE);
		m_names = new WorkbookNamesCollection(base.Application, this);
		m_charts = new ChartsCollection(base.Application, this);
		m_SSTDictionary = new SSTDictionary(this);
		m_fonts = new FontsCollection(base.Application, this);
		m_externBooks = new ExternBookCollection(base.Application, this);
		m_calcution = new CalculationOptionsImpl(base.Application, this);
		m_extFormats = new ExtendedFormatsCollection(base.Application, this);
		m_shapesData = new WorkbookShapeDataImpl(base.Application, this, GetWorksheetShapes);
		m_arrNames = new List<NameRecord>();
		m_rawFormats = new FormatsCollection(base.Application, this);
		m_arrBound = new List<BoundSheetRecord>();
		m_arrReparse = new List<IReparse>();
		m_arrExtFormatRecords = new List<ExtendedFormatRecord>();
		m_arrXFExtRecords = new List<ExtendedXFRecord>();
		m_arrStyleExtRecords = new List<StyleExtRecord>();
		m_headerFooterPictures = new WorkbookShapeDataImpl(base.Application, this, GetHeaderFooterShapes);
		m_sheetGroup = new WorksheetGroup(base.Application, this);
		WindowOne.SelectedTab = ushort.MaxValue;
	}

	internal void InsertDefaultValues()
	{
		m_rawFormats.InsertDefaultFormats();
		InsertDefaultExtFormats();
		InsertDefaultStyles();
	}

	protected void InsertDefaultExtFormats()
	{
		int count = m_extFormats.Count;
		ExtendedXFRecord defaultXFExt = GetDefaultXFExt();
		if (count <= 0)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(0);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 1)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(1);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 2)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(2);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 3)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(3);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 4)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(4);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 5)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(5);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 6)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(6);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 7)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(7);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 8)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(8);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 9)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(9);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 10)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(10);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 11)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(11);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 12)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(12);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 13)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(13);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 14)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(14);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 15)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(15);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 16)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(16);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 17)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(17);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 18)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(18);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 19)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(19);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
		if (count <= 20)
		{
			ExtendedFormatRecord defaultXF = GetDefaultXF(20);
			m_extFormats.ForceAdd(new ExtendedFormatImpl(base.Application, this, defaultXF, defaultXFExt));
		}
	}

	protected void InsertDefaultStyles()
	{
		InsertDefaultStyles(null);
	}

	protected void InsertDefaultStyles(List<StyleRecord> arrStyles)
	{
		StyleRecord styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		styleRecord.ExtendedFormatIndex = 0;
		styleRecord.BuildInOrNameLen = 0;
		styleRecord = FindStyle(arrStyles, styleRecord);
		StyleImpl styleImpl = base.AppImplementation.CreateStyle(this, styleRecord);
		if (!m_styles.ContainsName(styleImpl.Name))
		{
			m_styles.Add(styleImpl, bReplace: true);
		}
		styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		styleRecord.ExtendedFormatIndex = 16;
		styleRecord.BuildInOrNameLen = 3;
		styleRecord = FindStyle(arrStyles, styleRecord);
		StyleImpl stout = base.AppImplementation.CreateStyle(this, styleRecord);
		AddDefaultStyle(stout);
		styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		styleRecord.ExtendedFormatIndex = 17;
		styleRecord.BuildInOrNameLen = 6;
		styleRecord = FindStyle(arrStyles, styleRecord);
		stout = base.AppImplementation.CreateStyle(this, styleRecord);
		AddDefaultStyle(stout);
		styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		styleRecord.ExtendedFormatIndex = 18;
		styleRecord.BuildInOrNameLen = 4;
		styleRecord = FindStyle(arrStyles, styleRecord);
		stout = base.AppImplementation.CreateStyle(this, styleRecord);
		AddDefaultStyle(stout);
		styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		styleRecord.ExtendedFormatIndex = 19;
		styleRecord.BuildInOrNameLen = 7;
		styleRecord = FindStyle(arrStyles, styleRecord);
		stout = base.AppImplementation.CreateStyle(this, styleRecord);
		AddDefaultStyle(stout);
		styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		styleRecord = FindStyle(arrStyles, styleRecord);
		stout = base.AppImplementation.CreateStyle(this, styleRecord);
		AddDefaultStyle(stout);
		styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		styleRecord.ExtendedFormatIndex = 20;
		styleRecord.BuildInOrNameLen = 5;
		styleRecord = FindStyle(arrStyles, styleRecord);
		stout = base.AppImplementation.CreateStyle(this, styleRecord);
		AddDefaultStyle(stout);
		(Styles["Normal"].Font as FontWrapper).AfterChangeEvent += WorkbookImpl_AfterChangeEvent;
	}

	private void AddDefaultStyle(StyleImpl stout)
	{
		if (stout == null)
		{
			throw new ArgumentNullException("stout");
		}
		int xFormatIndex = stout.XFormatIndex;
		if (InnerExtFormats[xFormatIndex].HasParent)
		{
			ExtendedFormatImpl format = CreateExtFormatWithoutRegister(InnerExtFormats[0]);
			format = RegisterExtFormat(format, forceAdd: true);
			stout.SetFormatIndex(format.Index);
		}
		if (!m_styles.ContainsName(stout.Name))
		{
			m_styles.Add(stout);
		}
	}

	[CLSCompliant(false)]
	protected ExtendedFormatRecord GetDefaultXF(int index)
	{
		ExtendedFormatRecord extendedFormatRecord = null;
		switch (index)
		{
		case 0:
			extendedFormatRecord = (ExtendedFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedFormat);
			extendedFormatRecord.IsLocked = true;
			extendedFormatRecord.ParentIndex = (ushort)MaxXFCount;
			extendedFormatRecord.XFType = ExtendedFormatRecord.TXFType.XF_CELL;
			extendedFormatRecord.FillBackground = 65;
			extendedFormatRecord.FillForeground = 64;
			break;
		case 1:
		case 2:
			extendedFormatRecord = (ExtendedFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedFormat);
			extendedFormatRecord.FontIndex = 1;
			extendedFormatRecord.IsLocked = true;
			extendedFormatRecord.XFType = ExtendedFormatRecord.TXFType.XF_CELL;
			extendedFormatRecord.ParentIndex = (ushort)MaxXFCount;
			extendedFormatRecord.VAlignmentType = OfficeVAlign.VAlignBottom;
			extendedFormatRecord.IsNotParentFormat = true;
			extendedFormatRecord.IsNotParentAlignment = true;
			extendedFormatRecord.IsNotParentBorder = true;
			extendedFormatRecord.IsNotParentPattern = true;
			extendedFormatRecord.IsNotParentCellOptions = true;
			break;
		case 3:
		case 4:
			extendedFormatRecord = (ExtendedFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedFormat);
			if (InnerFonts.Count > 2)
			{
				extendedFormatRecord.FontIndex = 2;
			}
			extendedFormatRecord.IsLocked = true;
			extendedFormatRecord.XFType = ExtendedFormatRecord.TXFType.XF_CELL;
			extendedFormatRecord.ParentIndex = (ushort)MaxXFCount;
			extendedFormatRecord.VAlignmentType = OfficeVAlign.VAlignBottom;
			extendedFormatRecord.IsNotParentFormat = true;
			extendedFormatRecord.IsNotParentAlignment = true;
			extendedFormatRecord.IsNotParentBorder = true;
			extendedFormatRecord.IsNotParentPattern = true;
			extendedFormatRecord.IsNotParentCellOptions = true;
			break;
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
			extendedFormatRecord = (ExtendedFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedFormat);
			extendedFormatRecord.IsLocked = true;
			extendedFormatRecord.XFType = ExtendedFormatRecord.TXFType.XF_CELL;
			extendedFormatRecord.ParentIndex = (ushort)MaxXFCount;
			extendedFormatRecord.VAlignmentType = OfficeVAlign.VAlignBottom;
			extendedFormatRecord.IsNotParentFormat = true;
			extendedFormatRecord.IsNotParentAlignment = true;
			extendedFormatRecord.IsNotParentBorder = true;
			extendedFormatRecord.IsNotParentPattern = true;
			extendedFormatRecord.IsNotParentCellOptions = true;
			break;
		case 15:
			extendedFormatRecord = (ExtendedFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedFormat);
			extendedFormatRecord.IsLocked = true;
			extendedFormatRecord.HAlignmentType = OfficeHAlign.HAlignGeneral;
			extendedFormatRecord.VAlignmentType = OfficeVAlign.VAlignBottom;
			extendedFormatRecord.FillBackground = 65;
			extendedFormatRecord.FillForeground = 64;
			break;
		case 16:
			extendedFormatRecord = (ExtendedFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedFormat);
			extendedFormatRecord.IsNotParentFont = true;
			extendedFormatRecord.IsNotParentAlignment = true;
			extendedFormatRecord.IsNotParentBorder = true;
			extendedFormatRecord.IsNotParentPattern = true;
			extendedFormatRecord.IsNotParentCellOptions = true;
			extendedFormatRecord.FontIndex = 1;
			extendedFormatRecord.FormatIndex = 43;
			extendedFormatRecord.XFType = ExtendedFormatRecord.TXFType.XF_CELL;
			extendedFormatRecord.ParentIndex = (ushort)MaxXFCount;
			break;
		case 17:
			extendedFormatRecord = (ExtendedFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedFormat);
			extendedFormatRecord.IsNotParentFont = true;
			extendedFormatRecord.IsNotParentAlignment = true;
			extendedFormatRecord.IsNotParentBorder = true;
			extendedFormatRecord.IsNotParentPattern = true;
			extendedFormatRecord.IsNotParentCellOptions = true;
			extendedFormatRecord.FontIndex = 1;
			extendedFormatRecord.FormatIndex = 41;
			extendedFormatRecord.ParentIndex = (ushort)MaxXFCount;
			extendedFormatRecord.XFType = ExtendedFormatRecord.TXFType.XF_CELL;
			break;
		case 18:
			extendedFormatRecord = (ExtendedFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedFormat);
			extendedFormatRecord.IsLocked = true;
			extendedFormatRecord.ParentIndex = (ushort)MaxXFCount;
			extendedFormatRecord.XFType = ExtendedFormatRecord.TXFType.XF_CELL;
			extendedFormatRecord.IsNotParentFont = true;
			extendedFormatRecord.IsNotParentAlignment = true;
			extendedFormatRecord.IsNotParentBorder = true;
			extendedFormatRecord.IsNotParentPattern = true;
			extendedFormatRecord.IsNotParentCellOptions = true;
			extendedFormatRecord.FontIndex = 1;
			extendedFormatRecord.FormatIndex = 44;
			break;
		case 19:
			extendedFormatRecord = (ExtendedFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedFormat);
			extendedFormatRecord.IsLocked = true;
			extendedFormatRecord.ParentIndex = (ushort)MaxXFCount;
			extendedFormatRecord.XFType = ExtendedFormatRecord.TXFType.XF_CELL;
			extendedFormatRecord.IsNotParentFont = true;
			extendedFormatRecord.IsNotParentAlignment = true;
			extendedFormatRecord.IsNotParentBorder = true;
			extendedFormatRecord.IsNotParentPattern = true;
			extendedFormatRecord.IsNotParentCellOptions = true;
			extendedFormatRecord.FontIndex = 1;
			extendedFormatRecord.FormatIndex = 42;
			break;
		case 20:
			extendedFormatRecord = (ExtendedFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedFormat);
			extendedFormatRecord.IsLocked = true;
			extendedFormatRecord.ParentIndex = (ushort)MaxXFCount;
			extendedFormatRecord.XFType = ExtendedFormatRecord.TXFType.XF_CELL;
			extendedFormatRecord.IsNotParentFont = true;
			extendedFormatRecord.IsNotParentAlignment = true;
			extendedFormatRecord.IsNotParentBorder = true;
			extendedFormatRecord.IsNotParentPattern = true;
			extendedFormatRecord.IsNotParentCellOptions = true;
			extendedFormatRecord.FontIndex = 1;
			extendedFormatRecord.FormatIndex = 9;
			break;
		}
		return extendedFormatRecord;
	}

	internal string GetFilePath(string strUrl)
	{
		string empty = string.Empty;
		if (!string.IsNullOrEmpty(strUrl))
		{
			empty = strUrl.Split('/')[^1];
			empty = empty.Split('\\')[^1];
			return strUrl.Substring(0, strUrl.Length - empty.Length);
		}
		return empty;
	}

	internal string GetWorkbookName(WorkbookImpl workbook)
	{
		int num = (workbook.Application.Workbooks as WorkbooksCollection).IndexOf(workbook);
		int num2 = 0;
		for (int num3 = num - 1; num3 >= 0; num3--)
		{
			if (!(workbook.Application.Workbooks[num3] as WorkbookImpl).IsCreated)
			{
				num2++;
			}
		}
		return "Book" + (num - num2 + 1);
	}

	[CLSCompliant(false)]
	protected ExtendedXFRecord GetDefaultXFExt()
	{
		return (ExtendedXFRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedXFRecord);
	}

	private StyleRecord FindStyle(List<StyleRecord> arrStyles, StyleRecord style)
	{
		if (arrStyles == null)
		{
			return style;
		}
		int i = 0;
		for (int count = arrStyles.Count; i < count; i++)
		{
			StyleRecord styleRecord = arrStyles[i];
			if (CompareDefaultStyleRecords(styleRecord, style))
			{
				return styleRecord;
			}
		}
		return style;
	}

	private bool CompareDefaultStyleRecords(StyleRecord style1, StyleRecord style2)
	{
		if (style1.IsBuildInStyle && style2.IsBuildInStyle)
		{
			return style1.BuildInOrNameLen == style2.BuildInOrNameLen;
		}
		return false;
	}

	internal void InsertDefaultFonts()
	{
		m_fonts.InsertDefaultFonts();
	}

	internal void DisposeAll()
	{
		if (!m_IsDisposed)
		{
			m_arrObjects.DisposeInternalData();
			m_arrObjects.Clear();
			m_arrObjects = null;
			m_externBooks.Dispose();
			ClearAll();
			if (m_drawGroup != null)
			{
				m_drawGroup.m_data = null;
				m_drawGroup.Dispose();
			}
			if (m_shapesData != null)
			{
				m_shapesData.Dispose();
			}
			if (m_SSTDictionary != null)
			{
				m_SSTDictionary.Dispose();
				m_SSTDictionary = null;
			}
			base.Dispose();
			m_IsDisposed = true;
		}
	}

	protected void ClearAll()
	{
		if (m_bIsDisposed)
		{
			return;
		}
		if (m_ActiveSheet != null && m_ActiveSheet is WorksheetImpl)
		{
			m_ActiveSheet = null;
		}
		if (m_worksheets != null)
		{
			m_worksheets.Clear();
		}
		if (m_arrBound != null)
		{
			m_arrBound.Clear();
			m_arrBound = null;
		}
		if (m_arrNames != null)
		{
			m_arrNames.Clear();
			m_arrNames = null;
		}
		if (m_arrReparse != null)
		{
			m_arrReparse.Clear();
			m_arrReparse = null;
		}
		if (m_colors != null)
		{
			m_colors.Clear();
			m_colors = null;
		}
		if (m_extFormats != null)
		{
			m_extFormats.Dispose();
			m_extFormats.Clear();
			m_extFormats = null;
		}
		if (m_styles != null)
		{
			m_styles.Clear();
			m_styles = null;
		}
		if (m_fonts != null)
		{
			m_fonts.Dispose();
			m_fonts.Clear();
			m_fonts = null;
		}
		if (m_rawFormats != null)
		{
			m_rawFormats.Clear();
			m_rawFormats = null;
		}
		if (m_shapesData != null)
		{
			m_shapesData.Clear();
			m_shapesData = null;
		}
		if (m_SSTDictionary != null)
		{
			m_SSTDictionary.Clear();
			m_SSTDictionary.Dispose();
			m_SSTDictionary = null;
		}
		if (m_sstStream != null)
		{
			m_sstStream.Dispose();
			m_sstStream = null;
		}
		if (m_arrExtFormatRecords != null)
		{
			m_arrExtFormatRecords.Clear();
			m_arrExtFormatRecords = null;
		}
		if (m_arrXFExtRecords != null)
		{
			m_arrXFExtRecords.Clear();
			m_arrXFExtRecords = null;
		}
		if (m_styles != null)
		{
			m_styles.Clear();
			m_styles = null;
		}
		if (m_fonts != null)
		{
			m_fonts.Clear();
			m_fonts = null;
		}
		if (m_externBooks != null)
		{
			m_externBooks.Clear();
			m_externBooks = null;
		}
		if (m_majorFonts != null)
		{
			m_majorFonts.Clear();
			m_majorFonts = null;
		}
		if (m_minorFonts != null)
		{
			m_minorFonts.Clear();
			m_minorFonts = null;
		}
		if (m_fileDataHolder != null)
		{
			m_fileDataHolder.Dispose();
			m_fileDataHolder = null;
		}
		if (m_arrObjects != null)
		{
			m_arrObjects.Clear();
			m_arrObjects = null;
		}
		m_controlsStream = null;
		m_sstStream = null;
		if (m_names != null)
		{
			foreach (NameImpl name in m_names)
			{
				name.ClearAll();
			}
			m_names.Clear();
			m_names = null;
		}
		if (m_bookExt != null)
		{
			m_bookExt.ClearData();
			m_bookExt = null;
		}
		if (m_childElements != null)
		{
			m_childElements.Clear();
			m_childElements = null;
		}
		if (m_compatibility != null)
		{
			m_compatibility.ClearData();
			m_compatibility = null;
		}
		if (m_drawGroup != null)
		{
			m_drawGroup.Dispose();
			m_drawGroup = null;
		}
		if (m_externSheet != null)
		{
			m_externSheet.Dispose();
			m_externSheet = null;
		}
		if (m_formulaUtil != null)
		{
			m_formulaUtil.Dispose();
			m_formulaUtil = null;
		}
		if (m_headerFooterPictures != null)
		{
			m_headerFooterPictures.Clear();
			m_headerFooterPictures = null;
		}
		if (m_modifiedFormatRecord != null)
		{
			m_modifiedFormatRecord.Clear();
			m_modifiedFormatRecord = null;
		}
		if (m_password != null)
		{
			m_password.ClearData();
			m_password = null;
		}
		if (m_passwordRev4 != null)
		{
			m_passwordRev4.ClearData();
			m_passwordRev4 = null;
		}
		if (m_windowOne != null)
		{
			m_windowOne = null;
		}
		if (m_arrStyleExtRecords != null)
		{
			m_arrStyleExtRecords.Clear();
			m_arrStyleExtRecords = null;
		}
		if (m_xfCellCount != null)
		{
			m_xfCellCount.Clear();
			m_xfCellCount = null;
		}
		if (m_reCalcId != null)
		{
			m_reCalcId.ClearData();
			m_reCalcId = null;
		}
		if (SheetTypeToName != null)
		{
			SheetTypeToName.Clear();
			SheetTypeToName = null;
		}
		if (m_preservesPivotCache != null)
		{
			m_preservesPivotCache.Clear();
			m_preservesPivotCache = null;
		}
		if (m_worksheets != null)
		{
			m_worksheets.Clear();
			m_worksheets = null;
		}
		GC.SuppressFinalize(this);
		m_bIsDisposed = true;
	}

	internal void ClearExtendedFormats()
	{
		if (m_extFormats == null)
		{
			return;
		}
		foreach (ExtendedFormatImpl extFormat in m_extFormats)
		{
			extFormat.Clear();
		}
	}

	private void ParseExcel2007Stream(Stream stream, string password, bool parseOnDemand)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_rawFormats.InsertDefaultFormats();
		m_fileDataHolder.ParseDocument(ref m_themeColors, parseOnDemand);
	}

	private void ParseStream(Stream stream, string password, OfficeVersion version, OfficeParseOptions options)
	{
		m_bIsLoaded = true;
		if (version != 0)
		{
			if (version == OfficeVersion.Excel97to2003)
			{
				throw new ArgumentOutOfRangeException("version");
			}
			ParseExcel2007Stream(stream, password, options == OfficeParseOptions.ParseWorksheetsOnDemand);
			Activate();
		}
	}

	~WorkbookImpl()
	{
		Close();
	}

	private ExtendedFormatRecord RecheckExtendedFormatRecord(ExtendedFormatRecord xf)
	{
		if (xf == null)
		{
			throw new ArgumentNullException("ExtendedFormatRecord");
		}
		if (m_modifiedFormatRecord.TryGetValue(xf.FormatIndex, out var value))
		{
			xf.FormatIndex = Convert.ToUInt16(value);
		}
		return xf;
	}

	private FormatRecord RecheckFormatRecord(FormatRecord format, ref int m_newValue)
	{
		if (format == null)
		{
			throw new ArgumentNullException("FormatRecord");
		}
		int index = format.Index;
		if (index >= 50 && index <= 52)
		{
			int num = 163 + m_newValue++;
			m_modifiedFormatRecord.Add(index, num);
			format.Index = num;
		}
		return format;
	}

	private void NormalizeBorders(ExtendedFormatRecord xf)
	{
		if (xf.XFType == ExtendedFormatRecord.TXFType.XF_STYLE && !xf.IsNotParentBorder && xf.ParentIndex != MaxXFCount)
		{
			ExtendedFormatRecord extendedFormatRecord = m_arrExtFormatRecords[xf.ParentIndex];
			if (xf.BorderBottom != extendedFormatRecord.BorderBottom || xf.BorderLeft != extendedFormatRecord.BorderLeft || xf.BorderRight != extendedFormatRecord.BorderRight || xf.BorderTop != extendedFormatRecord.BorderTop)
			{
				xf.IsNotParentBorder = true;
			}
		}
	}

	private void ParseSSTRecord(SSTRecord sst, OfficeParseOptions options)
	{
		m_SSTDictionary.OriginalSST = sst;
	}

	internal void PrepareStyles(bool bIgnoreStyles, List<StyleRecord> arrStyles, Dictionary<int, int> hashNewXFormatIndexes)
	{
		PrepareExtendedFormats(bIgnoreStyles, arrStyles);
		m_rawFormats.InsertDefaultFormats();
		if (!bIgnoreStyles)
		{
			CreateAllStyles(arrStyles);
			InsertDefaultExtFormats();
			InsertDefaultStyles(arrStyles);
		}
		else
		{
			InsertDefaultFonts();
			InsertDefaultExtFormats();
			InsertDefaultStyles();
			CreateStyleForEachFormat(hashNewXFormatIndexes);
		}
	}

	private void PrepareExtendedFormats(bool bIgnoreStyle, List<StyleRecord> arrStyles)
	{
		if (bIgnoreStyle)
		{
			return;
		}
		int count = m_arrExtFormatRecords.Count;
		for (int i = 0; i < count; i++)
		{
			ExtendedFormatRecord extendedFormatRecord = m_arrExtFormatRecords[i];
			NormalizeBorders(extendedFormatRecord);
			ExtendedXFRecord xfext = (ExtendedXFRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedXFRecord);
			for (int j = 0; j < m_arrXFExtRecords.Count; j++)
			{
				if (m_arrXFExtRecords[j].XFIndex == i)
				{
					xfext = m_arrXFExtRecords[j];
					break;
				}
			}
			if (extendedFormatRecord.XFType == ExtendedFormatRecord.TXFType.XF_STYLE && extendedFormatRecord.ParentIndex >= MaxXFCount)
			{
				extendedFormatRecord.ParentIndex = 0;
			}
			int parentIndex = extendedFormatRecord.ParentIndex;
			if (parentIndex != MaxXFCount)
			{
				ExtendedFormatRecord extendedFormatRecord2 = m_arrExtFormatRecords[parentIndex];
				if (!extendedFormatRecord.IsNotParentFont && extendedFormatRecord.FontIndex != extendedFormatRecord2.FontIndex)
				{
					extendedFormatRecord.IsNotParentFont = true;
				}
				if (!extendedFormatRecord.IsNotParentAlignment && extendedFormatRecord.AlignmentOptions != extendedFormatRecord2.AlignmentOptions)
				{
					extendedFormatRecord.IsNotParentAlignment = true;
				}
				if (!extendedFormatRecord.IsNotParentFormat && extendedFormatRecord.FormatIndex != extendedFormatRecord2.FormatIndex)
				{
					extendedFormatRecord.IsNotParentFormat = true;
				}
			}
			ExtendedFormatImpl format = new ExtendedFormatImpl(base.Application, this, extendedFormatRecord, xfext, bInitializeColors: true);
			m_extFormats.ForceAdd(format);
		}
		for (int k = 0; k < count; k++)
		{
			m_extFormats[k].UpdateFromParent();
		}
	}

	private void ParseAutoFilters()
	{
		int i = 0;
		for (int count = m_worksheets.Count; i < count; i++)
		{
			((WorksheetImpl)m_worksheets[i]).ParseAutoFilters();
		}
	}

	private void PrepareNames()
	{
		int i = 0;
		for (int count = m_arrNames.Count; i < count; i++)
		{
			NameRecord nameRecord = m_arrNames[i];
			if (nameRecord.IndexOrGlobal == 0)
			{
				m_names.Add(nameRecord);
			}
			else
			{
				((WorksheetNamesCollection)((IWorksheet)m_arrObjects[nameRecord.IndexOrGlobal - 1]).Names).Add(nameRecord);
			}
		}
	}

	private void ParseNames()
	{
		m_names.ParseNames();
	}

	private void ParseWorksheets()
	{
		int i = 0;
		for (int count = m_arrObjects.Count; i < count; i++)
		{
			IParseable obj = (IParseable)m_arrObjects[i];
			obj.Parse();
			_ = (WorksheetBaseImpl)obj;
		}
		if (m_windowOne.SelectedTab != ushort.MaxValue)
		{
			m_ActiveSheet = (WorksheetBaseImpl)m_arrObjects[m_windowOne.SelectedTab];
		}
		else
		{
			((WorksheetBaseImpl)m_arrObjects[0]).Activate();
		}
	}

	internal void ParseWorksheetsOnDemand()
	{
		if (m_windowOne.SelectedTab != ushort.MaxValue)
		{
			m_ActiveSheet = (WorksheetBaseImpl)m_arrObjects[m_windowOne.SelectedTab];
		}
		else
		{
			((WorksheetBaseImpl)m_arrObjects[0]).Activate();
		}
	}

	private void Reparse()
	{
		if (!m_bWorkbookOpening)
		{
			int i = 0;
			for (int count = m_arrReparse.Count; i < count; i++)
			{
				m_arrReparse[i].Reparse();
			}
			m_arrReparse.Clear();
		}
	}

	private void CreateAllStyles(List<StyleRecord> arrStyles)
	{
		int i = 0;
		for (int count = arrStyles.Count; i < count; i++)
		{
			StyleRecord styleRecord = arrStyles[i];
			StyleExtRecord styleExtRecord = null;
			if (m_arrStyleExtRecords.Count > 0 && i < m_arrStyleExtRecords.Count)
			{
				styleExtRecord = m_arrStyleExtRecords[i];
			}
			int extendedFormatIndex = styleRecord.ExtendedFormatIndex;
			ExtendedFormatImpl extendedFormatImpl = InnerExtFormats[extendedFormatIndex];
			if (extendedFormatImpl.HasParent)
			{
				ExtendedFormatImpl extendedFormatImpl2 = (ExtendedFormatImpl)extendedFormatImpl.Clone();
				extendedFormatImpl2.ParentIndex = MaxXFCount;
				extendedFormatImpl2.Record.XFType = ExtendedFormatRecord.TXFType.XF_CELL;
				extendedFormatImpl2 = m_extFormats.ForceAdd(extendedFormatImpl2);
				extendedFormatImpl.ParentIndex = extendedFormatImpl2.Index;
				styleRecord.ExtendedFormatIndex = (ushort)extendedFormatImpl2.Index;
				extendedFormatImpl = extendedFormatImpl2;
			}
			if (styleRecord.IsBuildInStyle)
			{
				StyleImpl styleImpl = base.AppImplementation.CreateStyle(this, styleRecord);
				if (styleExtRecord != null)
				{
					styleImpl.StyleExt = styleExtRecord;
				}
				if (!m_styles.ContainsName(styleImpl.Name))
				{
					m_styles.Add(styleImpl, bReplace: true);
				}
			}
			else
			{
				if (styleRecord.Name == null || styleRecord.Name.Length == 0)
				{
					styleRecord.StyleName = CollectionBaseEx<WorksheetImpl>.GenerateDefaultName(arrStyles, "UNKNOWNSTYLE_");
				}
				StyleImpl styleImpl2 = m_styles.Add(styleRecord);
				if (styleExtRecord != null)
				{
					styleImpl2.StyleExt = styleExtRecord;
				}
			}
		}
	}

	private StyleRecord FindStyleRecord(List<StyleRecord> arrStyles, int formatIndex, out int iStyleIndex)
	{
		iStyleIndex = -1;
		int i = 0;
		for (int count = arrStyles.Count; i < count; i++)
		{
			StyleRecord styleRecord = arrStyles[i];
			if (styleRecord.ExtendedFormatIndex == formatIndex)
			{
				iStyleIndex = i;
				return styleRecord;
			}
		}
		StyleRecord[] defaultStyles = GetDefaultStyles();
		int j = 0;
		for (int num = defaultStyles.Length; j < num; j++)
		{
			StyleRecord styleRecord2 = defaultStyles[j];
			if (styleRecord2.ExtendedFormatIndex == formatIndex)
			{
				iStyleIndex = -j - 1;
				return styleRecord2;
			}
		}
		return null;
	}

	private StyleRecord[] GetDefaultStyles()
	{
		List<StyleRecord> list = new List<StyleRecord>(7);
		StyleRecord styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		styleRecord.ExtendedFormatIndex = 16;
		styleRecord.BuildInOrNameLen = 3;
		list.Add(styleRecord);
		styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		styleRecord.ExtendedFormatIndex = 17;
		styleRecord.BuildInOrNameLen = 6;
		list.Add(styleRecord);
		styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		styleRecord.ExtendedFormatIndex = 18;
		styleRecord.BuildInOrNameLen = 4;
		list.Add(styleRecord);
		styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		styleRecord.ExtendedFormatIndex = 19;
		styleRecord.BuildInOrNameLen = 7;
		list.Add(styleRecord);
		styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		list.Add(styleRecord);
		styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		styleRecord.ExtendedFormatIndex = 20;
		styleRecord.BuildInOrNameLen = 5;
		list.Add(styleRecord);
		return list.ToArray();
	}

	private void CreateStyleForEachFormat(Dictionary<int, int> hashNewXFormatIndexes)
	{
		foreach (KeyValuePair<int, FormatImpl> rawFormat in m_rawFormats)
		{
			int key = rawFormat.Key;
			string text = "Format_" + key;
			if (!m_styles.ContainsName(text))
			{
				StyleImpl obj = (StyleImpl)m_styles.Add(text, "Normal");
				obj.NumberFormat = rawFormat.Value.FormatString;
				int index = obj.Wrapped.CreateChildFormat().Index;
				hashNewXFormatIndexes.Add(key, index);
			}
		}
	}

	public void Activate()
	{
		((ApplicationImpl)base.Application).SetActiveWorkbook(this);
	}

	public void Close(string Filename)
	{
		Close(Filename != null && Filename.Length > 0, Filename);
	}

	public void Close(bool SaveChanges, string Filename)
	{
		if (base.Parent is IList)
		{
			IList list = (IList)base.Parent;
			int num = list.IndexOf(this);
			if (num >= 0)
			{
				list.RemoveAt(num);
			}
		}
		DisposeAll();
		if (base.AppImplementation != null && base.AppImplementation.Workbooks != null)
		{
			(base.AppImplementation.Workbooks as WorkbooksCollection).Remove(this);
		}
		GC.SuppressFinalize(this);
	}

	public void Close(bool saveChanges)
	{
		Close(saveChanges, null);
	}

	public void Close()
	{
		Close(saveChanges: false);
	}

	private IdReserver PrepareShapes(ShapesGetterMethod shapesGetter)
	{
		bool num = ReIndexShapeCollections(shapesGetter);
		bool bChanged;
		IdReserver idReserver = FillReserverFromShapes(shapesGetter, out bChanged);
		if (num)
		{
			if (m_shapesData != null && bChanged)
			{
				m_shapesData.ClearPreservedClusters();
			}
			RegisterNewShapes(idReserver, shapesGetter);
		}
		return idReserver;
	}

	private void RegisterNewShapes(IdReserver shapeIdReserver, ShapesGetterMethod shapesGetter)
	{
		if (shapeIdReserver == null)
		{
			throw new ArgumentNullException("shapeIdReserver");
		}
		UpdateAddedShapes(shapeIdReserver, shapesGetter);
		RegisterNewShapeCollections(shapeIdReserver, shapesGetter);
	}

	private void UpdateAddedShapes(IdReserver shapeIdReserver, ShapesGetterMethod shapesGetter)
	{
		foreach (ShapeCollectionBase item in EnumerateShapes(shapesGetter))
		{
			if (item.StartId == 0)
			{
				continue;
			}
			int shapesWithoutId = GetShapesWithoutId(item);
			if (shapesWithoutId > 0)
			{
				int shapesFreeIndexes = GetShapesFreeIndexes(shapeIdReserver, item);
				if (shapesWithoutId > shapesFreeIndexes)
				{
					shapeIdReserver.FreeSequence(item.CollectionIndex);
					AssignIndexes(shapeIdReserver, item);
				}
				else
				{
					AssignNewIndexes(shapeIdReserver, item);
				}
			}
		}
	}

	private void AssignNewIndexes(IdReserver shapeIdReserver, ShapeCollectionBase shapes)
	{
		int num = shapes.LastId;
		int i = 0;
		for (int count = shapes.Count; i < count; i++)
		{
			ShapeImpl shapeImpl = shapes[i] as ShapeImpl;
			if (shapeImpl.ShapeId == 0)
			{
				num = (shapeImpl.ShapeId = num + 1);
			}
		}
		shapes.LastId = num;
	}

	private int GetShapesFreeIndexes(IdReserver shapeIdReserver, ShapeCollectionBase shapes)
	{
		if (shapeIdReserver == null)
		{
			throw new ArgumentNullException("shapeIdReserver");
		}
		if (shapes == null)
		{
			throw new ArgumentNullException("shapes");
		}
		return shapeIdReserver.GetReservedCount(shapes.CollectionIndex) + shapes.StartId - shapes.LastId;
	}

	private int GetShapesWithoutId(ShapeCollectionBase shapes)
	{
		if (shapes == null)
		{
			throw new ArgumentNullException("shapes");
		}
		int num = 0;
		int i = 0;
		for (int count = shapes.Count; i < count; i++)
		{
			if ((shapes[i] as ShapeImpl).ShapeId <= 0)
			{
				num++;
			}
		}
		return num;
	}

	private void RegisterNewShapeCollections(IdReserver shapeIdReserver, ShapesGetterMethod shapesGetter)
	{
		if (shapeIdReserver == null)
		{
			throw new ArgumentNullException("shapeIdReserver");
		}
		foreach (ShapeCollectionBase item in EnumerateShapes(shapesGetter))
		{
			if (item.StartId == 0)
			{
				AssignIndexes(shapeIdReserver, item);
				shapeIdReserver.AddAdditionalShapes(item.CollectionIndex, item.Count);
			}
		}
	}

	private void AssignIndexes(IdReserver shapeIdReserver, ShapeCollectionBase shapes)
	{
		if (shapeIdReserver == null)
		{
			throw new ArgumentNullException("shapeIdReserver");
		}
		if (shapes == null)
		{
			throw new ArgumentNullException("shapes");
		}
		int count = shapes.Count;
		int num = shapeIdReserver.Allocate(count + 1, shapes.CollectionIndex);
		int lastId = num + shapes.Count;
		shapes.StartId = num;
		shapes.LastId = lastId;
		num++;
		for (int i = 0; i < count; i++)
		{
			if ((shapes[i] as ShapeImpl).ShapeId == 0)
			{
				(shapes[i] as ShapeImpl).ShapeId = num + i;
			}
		}
	}

	private IdReserver FillReserverFromShapes(ShapesGetterMethod shapesGetter, out bool bChanged)
	{
		IdReserver idReserver = new IdReserver();
		bChanged = false;
		foreach (ShapeCollectionBase item in EnumerateShapes(shapesGetter))
		{
			if (item == null)
			{
				continue;
			}
			int startId = item.StartId;
			_ = item.LastId;
			int collectionIndex = item.CollectionIndex;
			if (startId > 0)
			{
				int i = 0;
				for (int count = item.Count; i < count; i++)
				{
					int shapeId = (item[i] as ShapeImpl).ShapeId;
					if (shapeId > 0 && !idReserver.TryReserve(shapeId, shapeId, collectionIndex))
					{
						item.StartId = 0;
						item.LastId = 0;
					}
					else if (shapeId <= 0 && item[i].ShapeType != 0)
					{
						bChanged = true;
					}
				}
			}
			if (item != null && item.Count > 0)
			{
				_ = item.Worksheet;
				idReserver.AddAdditionalShapes(collectionIndex, item.Count);
			}
		}
		return idReserver;
	}

	private bool ReIndexShapeCollections(ShapesGetterMethod shapesGetter)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = -1;
		num = GetMaxCollectionIndex(shapesGetter);
		foreach (ShapeCollectionBase item in EnumerateShapes(shapesGetter))
		{
			if (item != null && item.Count > 0)
			{
				int num2 = item.CollectionIndex;
				if (dictionary.ContainsKey(num2))
				{
					num2 = (item.CollectionIndex = ++num);
				}
				dictionary.Add(num2, num2);
			}
		}
		return num >= 0;
	}

	private int GetMaxCollectionIndex(ShapesGetterMethod shapesGetter)
	{
		int num = -1;
		foreach (ShapeCollectionBase item in EnumerateShapes(shapesGetter))
		{
			int num2 = item?.CollectionIndex ?? (-1);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	internal IEnumerable<ShapeCollectionBase> EnumerateShapes(ShapesGetterMethod shapesGetter)
	{
		int i = 0;
		for (int len = m_arrObjects.Count; i < len; i++)
		{
			ITabSheet tabSheet = m_arrObjects[i] as ITabSheet;
			ShapeCollectionBase shapes = shapesGetter(tabSheet);
			if (shapes != null && shapes.Count > 0)
			{
				yield return shapes;
			}
			shapes = tabSheet.Shapes as ShapeCollectionBase;
			int j = 0;
            /*
			 for (int lenJ = shapes.Count; j < lenJ; j++)
			{
				if (shapes[j] is ITabSheet tabSheet)
				{
					ShapeCollectionBase shapeCollectionBase = shapesGetter(tabSheet);
					if (shapeCollectionBase != null && shapeCollectionBase.Count > 0)
					{
						yield return shapeCollectionBase;
					}
				}
			}
			 */
        }
    }

	private ShapeCollectionBase GetWorksheetShapes(ITabSheet sheet)
	{
		return sheet.Shapes as ShapeCollectionBase;
	}

	private ShapeCollectionBase GetHeaderFooterShapes(ITabSheet sheet)
	{
		if (sheet is ChartShapeImpl chartShapeImpl)
		{
			sheet = (WorksheetBaseImpl)chartShapeImpl;
		}
		return ((WorksheetBaseImpl)sheet).HeaderFooterShapes;
	}

	private void PrepareShapes()
	{
	}

	public void SaveAs(Stream stream, string separator)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (separator == null || separator.Length == 0)
		{
			throw new ArgumentNullException("separator");
		}
		SaveAsInternal(stream, separator);
	}

	private void SaveAsInternal(Stream stream, string separator)
	{
		if (ActiveSheet is WorksheetImpl worksheetImpl)
		{
			worksheetImpl.SaveAs(stream, separator);
		}
	}

	public void SaveAsXmlInternal(XmlWriter writer, OfficeXmlSaveType saveType)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		m_bSaving = true;
		XmlSerializatorFactory.GetSerializator(saveType).Serialize(writer, this);
		m_bSaving = false;
	}

	public void SaveAsXml(XmlWriter writer, OfficeXmlSaveType saveType)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		SaveAsXmlInternal(writer, saveType);
	}

	private string GetContentTypeString(OfficeHttpContentType contentType)
	{
		switch (contentType)
		{
		case OfficeHttpContentType.Excel97:
			return "Application/x-msexcel";
		case OfficeHttpContentType.Excel2000:
			return "Application/vnd.ms-excel";
		case OfficeHttpContentType.Excel2007:
		case OfficeHttpContentType.Excel2010:
		case OfficeHttpContentType.Excel2013:
			return "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
		case OfficeHttpContentType.CSV:
			return "text/csv";
		default:
			throw new ArgumentOutOfRangeException("contentType");
		}
	}

	public void SaveAs(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SaveAs(stream, OfficeSaveType.SaveAsXLS);
	}

	public void SaveAs(Stream stream, OfficeSaveType saveType)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SaveAsInternal(stream, saveType);
	}

	private void SaveAsInternal(Stream stream, OfficeSaveType saveType)
	{
		m_bSaving = true;
		IdReserver shapeIds = PrepareShapes(GetWorksheetShapes);
		CreateSerializator(m_version, shapeIds);
		stream.Flush();
		m_bSaving = false;
		m_bSaved = true;
		RaiseSavedEvent();
	}

	private string[] GetDocParts()
	{
		string[] array = new string[Worksheets.Count];
		IWorksheets worksheets = Worksheets;
		int i = 0;
		for (int count = worksheets.Count; i < count; i++)
		{
			array[i] = worksheets[i].Name;
		}
		return array;
	}

	private object[] GetHeadingPairs()
	{
		List<object> list = new List<object>();
		foreach (KeyValuePair<OfficeSheetType, int> hashHeadingPair in GetHashHeadingPairs())
		{
			OfficeSheetType key = hashHeadingPair.Key;
			int value = hashHeadingPair.Value;
			if (SheetTypeToName.ContainsKey(key))
			{
				list.Add(SheetTypeToName[key]);
				list.Add(value);
			}
		}
		return list.ToArray();
	}

	private Dictionary<OfficeSheetType, int> GetHashHeadingPairs()
	{
		Dictionary<OfficeSheetType, int> dictionary = new Dictionary<OfficeSheetType, int>();
		IWorksheets worksheets = Worksheets;
		int i = 0;
		for (int count = worksheets.Count; i < count; i++)
		{
			OfficeSheetType type = (worksheets[i] as WorksheetImpl).Type;
			if (dictionary.ContainsKey(type))
			{
				int num = dictionary[type];
				num++;
				dictionary[type] = num;
			}
			else
			{
				dictionary.Add(type, 1);
			}
		}
		return dictionary;
	}

	public void SetPaletteColor(int index, Color color)
	{
		if ((!m_bWorkbookOpening && index < 8) || index >= m_colors.Count)
		{
			throw new ArgumentOutOfRangeException("index", "Index cannot be less than 0 and larger than Palette colors array size.");
		}
		if (m_colors[index] != color)
		{
			m_bOwnPalette = true;
			m_colors[index] = Color.FromArgb(color.A, color.R, color.G, color.B);
		}
	}

	public void CopyPaletteColorTo(WorkbookImpl destinationWorkbook)
	{
		destinationWorkbook.InnerPalette.Clear();
		destinationWorkbook.InnerPalette.AddRange(InnerPalette);
		destinationWorkbook.m_bOwnPalette = m_bOwnPalette;
	}

	public void ResetPalette()
	{
		m_bOwnPalette = false;
		m_colors = new List<Color>(DEF_PALETTE);
	}

	public Color GetPaletteColor(OfficeKnownColors color)
	{
		int num = (int)color;
		Color black = Color.Black;
		if (num == 32767 && m_colors.Count > 0)
		{
			black = m_colors[0];
		}
		if (num >= 77 && num <= 79)
		{
			return m_chartColors[num - 77];
		}
		if (num == 80)
		{
			return ShapeFillImpl.DEF_COMENT_PARSE_COLOR;
		}
		num %= m_colors.Count;
		return m_colors[num];
	}

	public OfficeKnownColors GetNearestColor(Color color)
	{
		return GetNearestColor(color, 0);
	}

	public OfficeKnownColors GetNearestColor(Color color, int iStartIndex)
	{
		if (iStartIndex < 0 || iStartIndex > m_colors.Count)
		{
			throw new ArgumentOutOfRangeException("iStartIndex");
		}
		int result = iStartIndex;
		double num = ColorDistance(m_colors[iStartIndex], color);
		for (int i = iStartIndex + 1; i < m_colors.Count; i++)
		{
			double num2 = ColorDistance(m_colors[i], color);
			if (num2 < num)
			{
				num = num2;
				result = i;
				if (num2 == 0.0)
				{
					break;
				}
			}
		}
		return (OfficeKnownColors)result;
	}

	public OfficeKnownColors GetNearestColor(int r, int g, int b)
	{
		Color color = Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
		int result = 0;
		double num = ColorDistance(m_colors[0], color);
		for (int i = 1; i < m_colors.Count; i++)
		{
			double num2 = ColorDistance(m_colors[i], color);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return (OfficeKnownColors)result;
	}

	public OfficeKnownColors SetColorOrGetNearest(Color color)
	{
		OfficeKnownColors nearestColor = GetNearestColor(color);
		if (!(isEqualColor = CompareColors(m_colors[(int)nearestColor], color)) && m_iFirstUnusedColor < m_colors.Count)
		{
			SetPaletteColor(m_iFirstUnusedColor, color);
			int iFirstUnusedColor = m_iFirstUnusedColor;
			m_iFirstUnusedColor++;
			return (OfficeKnownColors)iFirstUnusedColor;
		}
		return nearestColor;
	}

	public OfficeKnownColors SetColorOrGetNearest(int r, int g, int b)
	{
		Color colorOrGetNearest = Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
		return SetColorOrGetNearest(colorOrGetNearest);
	}

	public void Replace(string oldValue, string newValue)
	{
		int i = 0;
		for (int count = m_worksheets.Count; i < count; i++)
		{
			m_worksheets[i].Replace(oldValue, newValue);
		}
	}

	public void Replace(string oldValue, DateTime newValue)
	{
		int i = 0;
		for (int count = m_worksheets.Count; i < count; i++)
		{
			m_worksheets[i].Replace(oldValue, newValue);
		}
	}

	public void Replace(string oldValue, double newValue)
	{
		int i = 0;
		for (int count = m_worksheets.Count; i < count; i++)
		{
			m_worksheets[i].Replace(oldValue, newValue);
		}
	}

	public void Replace(string oldValue, string[] newValues, bool isVertical)
	{
		int i = 0;
		for (int count = m_worksheets.Count; i < count; i++)
		{
			m_worksheets[i].Replace(oldValue, newValues, isVertical);
		}
	}

	public void Replace(string oldValue, int[] newValues, bool isVertical)
	{
		int i = 0;
		for (int count = m_worksheets.Count; i < count; i++)
		{
			m_worksheets[i].Replace(oldValue, newValues, isVertical);
		}
	}

	public void Replace(string oldValue, double[] newValues, bool isVertical)
	{
		int i = 0;
		for (int count = m_worksheets.Count; i < count; i++)
		{
			m_worksheets[i].Replace(oldValue, newValues, isVertical);
		}
	}

	public IOfficeFont CreateFont()
	{
		return new FontWrapper(base.AppImplementation.CreateFont(this), bReadOnly: false, bRaiseEvents: false);
	}

	public IOfficeFont AddFont(IOfficeFont fontToAdd)
	{
		bool num = fontToAdd is FontWrapper;
		FontWrapper fontWrapper = null;
		FontImpl font;
		if (num)
		{
			fontWrapper = fontToAdd as FontWrapper;
			if (fontWrapper == null)
			{
				throw new ArgumentNullException("fontToAdd");
			}
			font = fontWrapper.Wrapped;
		}
		else
		{
			font = fontToAdd as FontImpl;
		}
		font = m_fonts.Add(font) as FontImpl;
		if (num)
		{
			fontWrapper.Wrapped = font;
			fontWrapper.IsReadOnly = true;
			return fontWrapper;
		}
		return font;
	}

	public IOfficeFont CreateFont(IOfficeFont baseFont)
	{
		return CreateFont(baseFont, bAddToCollection: true);
	}

	public IOfficeFont CreateFont(IOfficeFont baseFont, bool bAddToCollection)
	{
		IOfficeFont officeFont = ((baseFont != null) ? base.AppImplementation.CreateFont(baseFont) : base.AppImplementation.CreateFont(this));
		if (baseFont != null && baseFont is FontImpl)
		{
			(officeFont as FontImpl).ParaAlign = (baseFont as FontImpl).ParaAlign;
			officeFont.RGBColor = baseFont.RGBColor;
		}
		if (bAddToCollection)
		{
			((FontImpl)officeFont).Index = m_fonts.Count;
			m_fonts.Add(officeFont);
		}
		return officeFont;
	}

	public IRange FindFirst(string findValue, OfficeFindType flags)
	{
		return FindFirst(findValue, flags, OfficeFindOptions.None);
	}

	public IRange FindStringStartsWith(string findValue, OfficeFindType flags)
	{
		return FindStringStartsWith(findValue, flags, ignoreCase: false);
	}

	public IRange FindStringStartsWith(string findValue, OfficeFindType flags, bool ignoreCase)
	{
		m_isStartsOrEndsWith = true;
		OfficeFindOptions findOptions = ((!ignoreCase) ? OfficeFindOptions.MatchCase : OfficeFindOptions.None);
		return FindFirst(findValue, flags, findOptions);
	}

	public IRange FindStringEndsWith(string findValue, OfficeFindType flags)
	{
		return FindStringEndsWith(findValue, flags, ignoreCase: false);
	}

	public IRange FindStringEndsWith(string findValue, OfficeFindType flags, bool ignoreCase)
	{
		m_isStartsOrEndsWith = false;
		OfficeFindOptions findOptions = ((!ignoreCase) ? OfficeFindOptions.MatchCase : OfficeFindOptions.None);
		return FindFirst(findValue, flags, findOptions);
	}

	public IRange FindFirst(string findValue, OfficeFindType flags, OfficeFindOptions findOptions)
	{
		if (findValue == null)
		{
			return null;
		}
		bool num = (flags & OfficeFindType.Formula) == OfficeFindType.Formula;
		bool flag = (flags & OfficeFindType.Text) == OfficeFindType.Text;
		bool flag2 = (flags & OfficeFindType.FormulaStringValue) == OfficeFindType.FormulaStringValue;
		bool flag3 = (flags & OfficeFindType.Error) == OfficeFindType.Error;
		if (!(num || flag || flag2 || flag3))
		{
			throw new ArgumentException("Parameter flags is not valid.", "flags");
		}
		return Worksheets.FindFirst(findValue, flags, findOptions);
	}

	public IRange FindFirst(double findValue, OfficeFindType flags)
	{
		bool num = (flags & OfficeFindType.FormulaValue) == OfficeFindType.FormulaValue;
		bool flag = (flags & OfficeFindType.Number) == OfficeFindType.Number;
		if (!(num || flag))
		{
			throw new ArgumentException("Parameter flags is not valid.", "flags");
		}
		return Worksheets.FindFirst(findValue, flags);
	}

	public IRange FindFirst(bool findValue)
	{
		return Worksheets.FindFirst(findValue);
	}

	public IRange FindFirst(DateTime findValue)
	{
		return Worksheets.FindFirst(findValue);
	}

	public IRange FindFirst(TimeSpan findValue)
	{
		return Worksheets.FindFirst(findValue);
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags)
	{
		return FindAll(findValue, flags, OfficeFindOptions.None);
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags, OfficeFindOptions findOptions)
	{
		if (findValue == null)
		{
			return null;
		}
		bool num = (flags & OfficeFindType.Formula) == OfficeFindType.Formula;
		bool flag = (flags & OfficeFindType.Text) == OfficeFindType.Text;
		bool flag2 = (flags & OfficeFindType.FormulaStringValue) == OfficeFindType.FormulaStringValue;
		bool flag3 = (flags & OfficeFindType.Error) == OfficeFindType.Error;
		if (!(num || flag || flag2 || flag3))
		{
			throw new ArgumentException("Parameter flags is not valid.", "flags");
		}
		return Worksheets.FindAll(findValue, flags, findOptions);
	}

	public IRange[] FindAll(double findValue, OfficeFindType flags)
	{
		bool num = (flags & OfficeFindType.FormulaValue) == OfficeFindType.FormulaValue;
		bool flag = (flags & OfficeFindType.Number) == OfficeFindType.Number;
		if (!(num || flag))
		{
			throw new ArgumentException("Parameter flags is not valid.", "flags");
		}
		return Worksheets.FindAll(findValue, flags);
	}

	public IRange[] FindAll(bool findValue)
	{
		return Worksheets.FindAll(findValue);
	}

	public IRange[] FindAll(DateTime findValue)
	{
		return Worksheets.FindAll(findValue);
	}

	public IRange[] FindAll(TimeSpan findValue)
	{
		return Worksheets.FindAll(findValue);
	}

	public void SetSeparators(char argumentsSeparator, char arrayRowsSeparator)
	{
		FormulaUtil.SetSeparators(argumentsSeparator, arrayRowsSeparator);
	}

	public void Protect(bool bIsProtectWindow, bool bIsProtectContent)
	{
		Protect(bIsProtectWindow, bIsProtectContent, null);
	}

	public void Protect(bool bIsProtectWindow, bool bIsProtectContent, string password)
	{
		if (!bIsProtectWindow && !bIsProtectContent)
		{
			throw new ArgumentOutOfRangeException("One of params must be TRUE.");
		}
		if (m_bCellProtect || m_bWindowProtect)
		{
			throw new NotSupportedException("Workbook is already protected. Use Unprotect before calling method.");
		}
		m_bCellProtect = bIsProtectContent;
		m_bWindowProtect = bIsProtectWindow;
		m_encryptionType = ExcelEncryptionType.Standard;
		if (password != null)
		{
			Password.IsPassword = (ushort)((password.Length > 0) ? WorksheetBaseImpl.GetPasswordHash(password) : 0);
		}
	}

	public void Unprotect()
	{
		Unprotect(null);
	}

	public void Unprotect(string password)
	{
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
	}

	public IWorkbook Clone()
	{
		WorkbookImpl workbookImpl = (WorkbookImpl)MemberwiseClone();
		if (m_fileDataHolder != null)
		{
			workbookImpl.m_fileDataHolder = m_fileDataHolder.Clone(workbookImpl);
		}
		workbookImpl.m_ptrHeapHandle = IntPtr.Zero;
		workbookImpl.m_fonts = m_fonts.Clone(workbookImpl);
		workbookImpl.m_rawFormats = m_rawFormats.Clone(workbookImpl);
		workbookImpl.m_extFormats = (ExtendedFormatsCollection)m_extFormats.Clone(workbookImpl);
		workbookImpl.m_styles = (StylesCollection)m_styles.Clone(workbookImpl);
		workbookImpl.m_colors = ClonePalette();
		if (m_CustomTableStylesStream != null)
		{
			m_CustomTableStylesStream.Position = 0L;
			byte[] array = new byte[m_CustomTableStylesStream.Length];
			m_CustomTableStylesStream.Read(array, 0, array.Length);
			m_CustomTableStylesStream.Position = 0L;
			workbookImpl.m_CustomTableStylesStream = new MemoryStream(array);
		}
		if (m_controlsStream != null)
		{
			m_controlsStream.Position = 0L;
			byte[] array2 = new byte[m_controlsStream.Length];
			m_controlsStream.Read(array2, 0, array2.Length);
			m_controlsStream.Position = 0L;
			workbookImpl.m_controlsStream = new MemoryStream(array2);
		}
		workbookImpl.m_worksheets = new WorksheetsCollection(base.Application, workbookImpl);
		workbookImpl.m_externSheet = (ExternSheetRecord)CloneUtils.CloneCloneable(m_externSheet);
		workbookImpl.m_externBooks = (ExternBookCollection)m_externBooks.Clone(workbookImpl);
		workbookImpl.m_arrObjects = (WorkbookObjectsCollection)m_arrObjects.Clone(workbookImpl);
		workbookImpl.m_SSTDictionary = (SSTDictionary)m_SSTDictionary.Clone(workbookImpl);
		workbookImpl.m_calcution = (CalculationOptionsImpl)m_calcution.Clone(workbookImpl);
		workbookImpl.m_names = (WorkbookNamesCollection)m_names.Clone(workbookImpl);
		workbookImpl.m_shapesData = (WorkbookShapeDataImpl)m_shapesData.Clone(workbookImpl);
		workbookImpl.m_headerFooterPictures = (WorkbookShapeDataImpl)m_headerFooterPictures.Clone(workbookImpl);
		workbookImpl.m_sheetGroup = (WorksheetGroup)m_sheetGroup.Clone(workbookImpl);
		workbookImpl.m_externSheet = (ExternSheetRecord)CloneUtils.CloneCloneable(m_externSheet);
		workbookImpl.m_windowOne = (WindowOneRecord)CloneUtils.CloneCloneable(m_windowOne);
		workbookImpl.m_password = (PasswordRecord)CloneUtils.CloneCloneable(m_password);
		workbookImpl.m_passwordRev4 = (PasswordRev4Record)CloneUtils.CloneCloneable(m_passwordRev4);
		workbookImpl.m_protectionRev4 = (ProtectionRev4Record)CloneUtils.CloneCloneable(m_protectionRev4);
		workbookImpl.m_fileSharing = (FileSharingRecord)CloneUtils.CloneCloneable(m_fileSharing);
		workbookImpl.m_arrNames = CloneUtils.CloneCloneable(m_arrNames);
		workbookImpl.m_arrBound = CloneUtils.CloneCloneable(m_arrBound);
		workbookImpl.m_arrReparse = new List<IReparse>();
		workbookImpl.m_arrExtFormatRecords = CloneUtils.CloneCloneable(m_arrExtFormatRecords);
		workbookImpl.m_arrXFExtRecords = CloneUtils.CloneCloneable(m_arrXFExtRecords);
		workbookImpl.m_ActiveSheet = null;
		workbookImpl.ActiveSheetIndex = ActiveSheetIndex;
		workbookImpl.m_formulaUtil = null;
		workbookImpl.m_xfCellCount = CloneUtils.CloneHash(m_xfCellCount);
		return workbookImpl;
	}

	public void SetWriteProtectionPassword(string password)
	{
		if (password == null || password.Length == 0)
		{
			if (m_fileSharing != null)
			{
				m_fileSharing.HashPassword = 0;
				m_fileSharing.CreatorName = null;
			}
			return;
		}
		if (m_fileSharing == null)
		{
			m_fileSharing = (FileSharingRecord)BiffRecordFactory.GetRecord(TBIFFRecord.FileSharing);
		}
		m_fileSharing.HashPassword = WorksheetBaseImpl.GetPasswordHash(password);
		m_fileSharing.CreatorName = Author;
	}

	private List<Color> ClonePalette()
	{
		return new List<Color>(m_colors);
	}

	private void SaveInExcel2007(Stream stream, OfficeSaveType saveType)
	{
		if (m_fileDataHolder == null)
		{
			m_fileDataHolder = new FileDataHolder(this);
		}
	}

	private IWorkbookSerializator CreateSerializator(OfficeVersion version, IdReserver shapeIds)
	{
		CheckLicensingSheet();
		AddLicenseWorksheet();
		if (m_externSheet.RefCount > 1370)
		{
			OptimizeReferences();
		}
		switch (version)
		{
		case OfficeVersion.Excel97to2003:
			return null;
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
			if (m_fileDataHolder == null)
			{
				m_fileDataHolder = new FileDataHolder(this);
			}
			return m_fileDataHolder;
		default:
			throw new ArgumentOutOfRangeException("version");
		}
	}

	private void ChangeStylesTo97()
	{
		int[] array = null;
		if (CheckIfStyleChangeNeeded())
		{
			List<int> defaultStyleIndexes = PredefidedStylesPositions();
			array = FixStyles97(defaultStyleIndexes);
			UpdateStyleIndexes(array);
		}
	}

	private void UpdateStyleIndexes(int[] styleIndexes)
	{
		if (styleIndexes == null)
		{
			throw new ArgumentNullException("styleIndexes");
		}
		int i = 0;
		for (int count = m_arrObjects.Count; i < count; i++)
		{
			((WorksheetBaseImpl)m_arrObjects[i]).UpdateStyleIndexes(styleIndexes);
		}
	}

	private List<int> PredefidedStylesPositions()
	{
		List<int> list = new List<int>();
		int i = 0;
		for (int num = PredefinedStyleOutlines.Length; i < num; i++)
		{
			int num2 = PredefinedStyleOutlines[i];
			string name = base.AppImplementation.DefaultStyleNames[num2];
			int item = (m_styles.Contains(name) ? ((StyleImpl)m_styles[name]) : null)?.XFormatIndex ?? (-1);
			list.Add(item);
		}
		return list;
	}

	private bool CheckIfStyleChangeNeeded()
	{
		bool flag = false;
		if (!flag)
		{
			flag = DefaultXFIndex != 15;
		}
		return flag;
	}

	private int[] FixStyles97(List<int> defaultStyleIndexes)
	{
		if (defaultStyleIndexes == null)
		{
			throw new ArgumentNullException("defaultStyleIndexes");
		}
		int count = m_extFormats.Count;
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = -1;
		}
		List<int> fontIndexes = (ArrayFontIndex = ConvertFonts());
		ExtendedFormatsCollection extFormats = m_extFormats;
		m_extFormats = new ExtendedFormatsCollection(base.Application, this);
		InsertDefaultExtFormats();
		int j = 0;
		for (int count2 = defaultStyleIndexes.Count; j < count2; j++)
		{
			int num = defaultStyleIndexes[j];
			if (num >= 0)
			{
				int iXFIndex = (array[num] = PredefinedXFs[j]);
				m_extFormats.SetXF(iXFIndex, extFormats[num]);
			}
		}
		int k = 1;
		for (int count3 = extFormats.Count; k < count3; k++)
		{
			ExtendedFormatImpl extendedFormatImpl = extFormats[k];
			ConvertColors(extendedFormatImpl, fontIndexes);
			extendedFormatImpl.IndentLevel = Math.Min(extendedFormatImpl.IndentLevel, m_iMaxIndent);
			if (extendedFormatImpl.FillPattern == OfficePattern.Gradient)
			{
				extendedFormatImpl.FillPattern = OfficePattern.Solid;
				OfficeKnownColors indexed = extendedFormatImpl.Gradient.BackColorObject.GetIndexed(this);
				extendedFormatImpl.ColorObject.SetIndexed(indexed);
			}
			if (!extendedFormatImpl.HasParent && array[k] < 0)
			{
				extendedFormatImpl = m_extFormats.ForceAdd(extendedFormatImpl);
				array[k] = extendedFormatImpl.Index;
			}
		}
		int l = 1;
		for (int count4 = extFormats.Count; l < count4; l++)
		{
			ExtendedFormatImpl extendedFormatImpl2 = extFormats[l];
			if (extendedFormatImpl2.HasParent && extendedFormatImpl2.Index != m_iDefaultXFIndex)
			{
				extendedFormatImpl2.ParentIndex = array[extendedFormatImpl2.ParentIndex];
				extendedFormatImpl2 = m_extFormats.Add(extendedFormatImpl2);
				array[l] = extendedFormatImpl2.Index;
			}
		}
		m_extFormats.SetXF(15, extFormats[DefaultXFIndex]);
		array[m_iDefaultXFIndex] = 15;
		m_iDefaultXFIndex = 15;
		return array;
	}

	private void ConvertColors(ExtendedFormatImpl format, List<int> fontIndexes)
	{
		format.ColorObject.ConvertToIndexed(this);
		format.PatternColorObject.ConvertToIndexed(this);
		format.TopBorderColor.ConvertToIndexed(this);
		format.BottomBorderColor.ConvertToIndexed(this);
		format.LeftBorderColor.ConvertToIndexed(this);
		format.RightBorderColor.ConvertToIndexed(this);
		format.DiagonalBorderColor.ConvertToIndexed(this);
		int fontIndex = format.FontIndex;
		format.FontIndex = fontIndexes[fontIndex];
	}

	private List<int> ConvertFonts()
	{
		int count = m_fonts.Count;
		FontsCollection fontsCollection = new FontsCollection(base.Application, this);
		List<int> list = new List<int>(count);
		for (int i = 0; i < count; i++)
		{
			FontImpl fontImpl = (FontImpl)m_fonts[i];
			fontImpl.ColorObject.ConvertToIndexed(this);
			fontImpl = (FontImpl)fontsCollection.Add(fontImpl);
			list.Add(fontImpl.Index);
		}
		count = fontsCollection.Count;
		if (count < 5)
		{
			FontImpl fontImpl2 = (FontImpl)fontsCollection[0];
			for (int j = count; j <= 5; j++)
			{
				fontImpl2 = (FontImpl)fontImpl2.Clone();
				fontsCollection.ForceAdd(fontImpl2);
			}
		}
		m_fonts = fontsCollection;
		return list;
	}

	private bool IsValidDocument(Stream stream, Encoding encoding, string separator)
	{
		string text = new StreamReader(stream, encoding).ReadToEnd();
		int num = 0;
		int num2 = 0;
		int num3 = 1;
		bool flag = true;
		int length = separator.Length;
		double num4 = text.Length;
		while (flag && num3 != 0 && (double)num < num4)
		{
			num = text.IndexOf('"', num);
			num++;
			num3 = num;
			num2++;
			if ((double)(num + length) <= num4 && text.Substring(num, length) == separator && num2 % 2 != 0)
			{
				flag = false;
			}
		}
		stream.Position = 0L;
		return flag;
	}

	[CLSCompliant(false)]
	protected internal void SerializeForClipboard(OffsetArrayList records, WorksheetImpl sheet)
	{
	}

	public void SetActiveWorksheet(WorksheetBaseImpl sheet)
	{
		m_ActiveSheet = sheet;
		int realIndex = sheet.RealIndex;
		WindowOneRecord windowOne = WindowOne;
		windowOne.SelectedTab = (ushort)realIndex;
		if (windowOne.DisplayedTab > (ushort)realIndex)
		{
			windowOne.DisplayedTab = (ushort)realIndex;
		}
	}

	public bool ContainsFont(FontImpl font)
	{
		return m_fonts.Contains(font);
	}

	public void UpdateNamedRangeIndexes(int[] arrNewIndex)
	{
		if (arrNewIndex == null)
		{
			throw new ArgumentNullException("arrNewIndex");
		}
		int i = 0;
		for (int count = m_worksheets.Count; i < count; i++)
		{
			((WorksheetImpl)m_worksheets[i]).UpdateNamedRangeIndexes(arrNewIndex);
		}
	}

	public void UpdateNamedRangeIndexes(IDictionary<int, int> dicNewIndex)
	{
		if (dicNewIndex == null)
		{
			throw new ArgumentNullException("dicNewIndex");
		}
		int i = 0;
		for (int count = m_worksheets.Count; i < count; i++)
		{
			((WorksheetImpl)m_worksheets[i]).UpdateNamedRangeIndexes(dicNewIndex);
		}
	}

	public void SetChanged()
	{
		Saved = false;
	}

	public void UpdateStringIndexes(List<int> arrNewIndexes)
	{
		if (arrNewIndexes == null)
		{
			throw new ArgumentNullException("arrNewIndexes");
		}
		m_worksheets.UpdateStringIndexes(arrNewIndexes);
	}

	[CLSCompliant(false)]
	public Dictionary<int, int> CopyExternSheets(ExternSheetRecord externSheet, Dictionary<int, int> hashSubBooks)
	{
		if (externSheet == null)
		{
			throw new ArgumentNullException("externSheet");
		}
		if (hashSubBooks == null)
		{
			throw new ArgumentNullException("hashSubBooks");
		}
		ExternSheetRecord externSheet2 = ExternSheet;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int i = 0;
		for (int refCount = externSheet.RefCount; i < refCount; i++)
		{
			ExternSheetRecord.TREF tREF = externSheet.Refs[i];
			int num = tREF.SupBookIndex;
			if (hashSubBooks.ContainsKey(num))
			{
				num = hashSubBooks[num];
			}
			int value = externSheet2.AddReference(num, tREF.FirstSheet, tREF.LastSheet);
			dictionary.Add(i, value);
		}
		return dictionary;
	}

	public void ReAddAllStrings()
	{
		int i = 0;
		for (int count = m_worksheets.Count; i < count; i++)
		{
			((WorksheetImpl)m_worksheets[i]).ReAddAllStrings();
		}
	}

	public void UpdateXFIndexes(int maxCount)
	{
		if (maxCount <= 0)
		{
			throw new ArgumentOutOfRangeException("maxCount");
		}
		int i = 0;
		for (int count = m_worksheets.Count; i < count; i++)
		{
			((WorksheetImpl)m_worksheets[i]).UpdateExtendedFormatIndex(maxCount);
		}
	}

	public bool IsFormatted(int xfIndex)
	{
		return xfIndex != DefaultXFIndex;
	}

	public double GetMaxDigitWidth()
	{
		double result = 0.0;
		using (new AutoFitManager())
		{
			_ = ((IInternalFont)m_styles["Normal"].Font).Font;
			for (char c = '0'; c <= '9'; c = (char)(c + 1))
			{
			}
			return result;
		}
	}

	public double GetMaxDigitHeight()
	{
		FontImpl font = ((IInternalFont)m_styles["Normal"].Font).Font;
		double num = 0.0;
		for (char c = '0'; c <= '9'; c = (char)(c + 1))
		{
			SizeF sizeF = font.MeasureString(new string(c, 1));
			if ((double)sizeF.Height > num)
			{
				num = sizeF.Height;
			}
		}
		return num;
	}

	public double WidthToFileWidth(double width)
	{
		double maxDigitWidth = MaxDigitWidth;
		if (!(width > 1.0))
		{
			return width * (maxDigitWidth + 5.0) / maxDigitWidth * 256.0 / 256.0;
		}
		return (width * maxDigitWidth + 5.0) / maxDigitWidth * 256.0 / 256.0;
	}

	public double FileWidthToPixels(double fileWidth)
	{
		double maxDigitWidth = MaxDigitWidth;
		return MathGeneral.Truncate((256.0 * fileWidth + MathGeneral.Truncate(128.0 / maxDigitWidth)) / 256.0 * maxDigitWidth);
	}

	private static double Truncate(double d)
	{
		if (!(d > 0.0))
		{
			return 0.0 - Math.Floor(0.0 - d);
		}
		return Math.Floor(d);
	}

	public double PixelsToWidth(double pixels)
	{
		double maxDigitWidth = MaxDigitWidth;
		if (!(pixels > maxDigitWidth + 5.0))
		{
			return pixels / (maxDigitWidth + 5.0);
		}
		return Truncate((pixels - 5.0) / maxDigitWidth * 100.0 + 0.5) / 100.0;
	}

	private void WorkbookImpl_AfterChangeEvent(object sender, EventArgs e)
	{
		m_iFirstCharSize = -1;
		m_iSecondCharSize = -1;
		m_dMaxDigitWidth = GetMaxDigitWidth();
		StandardRowHeightInPixels = (int)GetMaxDigitHeight();
	}

	internal void ExtractControlProperties()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	internal uint CalculateCRC(uint crcValue, byte[] arrData, uint[] crcCache)
	{
		uint num = 0u;
		foreach (byte b in arrData)
		{
			num = crcValue;
			num >>= 24;
			num ^= b;
			crcValue <<= 8;
			crcValue ^= crcCache[num];
		}
		return crcValue;
	}

	internal static uint[] InitCRC()
	{
		uint[] array = new uint[256];
		uint num = 0u;
		uint num2 = 2147483648u;
		for (uint num3 = 0u; num3 <= 255; num3++)
		{
			num = num3;
			num <<= 24;
			for (int i = 0; i <= 7; i++)
			{
				if ((num & num2) == num2)
				{
					num <<= 1;
					num ^= 0xAFu;
				}
				else
				{
					num <<= 1;
				}
			}
			num &= 0xFFFFu;
			array[num3] = num;
		}
		return array;
	}

	internal ExtendedFormatImpl AddExtendedProperties(ExtendedFormatImpl m_xFormat)
	{
		if (m_xFormat.Index != DefaultXFIndex)
		{
			if (m_xFormat.Properties.Count > 0)
			{
				m_xFormat.Properties.Clear();
			}
			if (m_xFormat.FillPattern == OfficePattern.Solid)
			{
				if (m_xFormat.ColorObject.ColorType == ColorType.RGB || m_xFormat.ColorObject.ColorType == ColorType.Theme)
				{
					AddExtendedProperty(CellPropertyExtensionType.ForeColor, m_xFormat.Color, m_xFormat);
				}
				if (m_xFormat.PatternColorObject.ColorType == ColorType.RGB || m_xFormat.PatternColorObject.ColorType == ColorType.Theme)
				{
					AddExtendedProperty(CellPropertyExtensionType.BackColor, m_xFormat.PatternColor, m_xFormat);
				}
			}
			else
			{
				if (m_xFormat.ColorObject.ColorType == ColorType.RGB || m_xFormat.ColorObject.ColorType == ColorType.Theme)
				{
					AddExtendedProperty(CellPropertyExtensionType.BackColor, m_xFormat.Color, m_xFormat);
				}
				if (m_xFormat.PatternColorObject.ColorType == ColorType.RGB || m_xFormat.PatternColorObject.ColorType == ColorType.Theme)
				{
					AddExtendedProperty(CellPropertyExtensionType.ForeColor, m_xFormat.PatternColor, m_xFormat);
				}
			}
			if ((m_xFormat.TopBorderColor.ColorType == ColorType.RGB || m_xFormat.TopBorderColor.ColorType == ColorType.Theme) && m_xFormat.TopBorderLineStyle != 0)
			{
				AddExtendedProperty(CellPropertyExtensionType.TopBorderColor, m_xFormat.Borders[OfficeBordersIndex.EdgeTop].ColorRGB, m_xFormat);
			}
			if ((m_xFormat.BottomBorderColor.ColorType == ColorType.RGB || m_xFormat.BottomBorderColor.ColorType == ColorType.Theme) && m_xFormat.BottomBorderLineStyle != 0)
			{
				AddExtendedProperty(CellPropertyExtensionType.BottomBorderColor, m_xFormat.Borders[OfficeBordersIndex.EdgeBottom].ColorRGB, m_xFormat);
			}
			if ((m_xFormat.LeftBorderColor.ColorType == ColorType.RGB || m_xFormat.LeftBorderColor.ColorType == ColorType.Theme) && m_xFormat.LeftBorderLineStyle != 0)
			{
				AddExtendedProperty(CellPropertyExtensionType.LeftBorderColor, m_xFormat.Borders[OfficeBordersIndex.EdgeLeft].ColorRGB, m_xFormat);
			}
			if ((m_xFormat.RightBorderColor.ColorType == ColorType.RGB || m_xFormat.RightBorderColor.ColorType == ColorType.Theme) && m_xFormat.RightBorderLineStyle != 0)
			{
				AddExtendedProperty(CellPropertyExtensionType.RightBorderColor, m_xFormat.Borders[OfficeBordersIndex.EdgeRight].ColorRGB, m_xFormat);
			}
			if ((m_xFormat.DiagonalBorderColor.ColorType == ColorType.RGB || m_xFormat.DiagonalBorderColor.ColorType == ColorType.Theme) && m_xFormat.DiagonalDownBorderLineStyle != 0)
			{
				AddExtendedProperty(CellPropertyExtensionType.DiagonalCellBorder, m_xFormat.Borders[OfficeBordersIndex.DiagonalDown].ColorRGB, m_xFormat);
			}
			if (m_xFormat.IndentLevel > 15)
			{
				ExtendedProperty extendedProperty = new ExtendedProperty();
				extendedProperty.Type = CellPropertyExtensionType.TextIndentationLevel;
				extendedProperty.Size = 6;
				extendedProperty.Indent = (ushort)m_xFormat.IndentLevel;
				m_xFormat.Properties.Add(extendedProperty);
			}
			FontImpl fontImpl = InnerFonts[m_xFormat.FontIndex] as FontImpl;
			if (fontImpl.ColorObject.ColorType == ColorType.RGB || fontImpl.ColorObject.ColorType == ColorType.Theme)
			{
				AddExtendedProperty(CellPropertyExtensionType.TextColor, m_xFormat.Font.RGBColor, m_xFormat);
			}
		}
		return m_xFormat;
	}

	internal void AddExtendedProperty(CellPropertyExtensionType type, Color ColorValue, ExtendedFormatImpl m_xFormat)
	{
		ColorValue = ConvertARGBToRGBA(ColorValue);
		ExtendedProperty extendedProperty = new ExtendedProperty();
		extendedProperty.Type = GetPropertyType(type);
		extendedProperty.Size = 20;
		extendedProperty.ColorValue = ColorToUInt(ColorValue);
		m_xFormat.Properties.Add(extendedProperty);
	}

	internal CellPropertyExtensionType GetPropertyType(CellPropertyExtensionType type)
	{
		switch (type)
		{
		case CellPropertyExtensionType.ForeColor:
			type = CellPropertyExtensionType.ForeColor;
			break;
		case CellPropertyExtensionType.BackColor:
			type = CellPropertyExtensionType.BackColor;
			break;
		case CellPropertyExtensionType.GradientFill:
			type = CellPropertyExtensionType.GradientFill;
			break;
		case CellPropertyExtensionType.TopBorderColor:
			type = CellPropertyExtensionType.TopBorderColor;
			break;
		case CellPropertyExtensionType.BottomBorderColor:
			type = CellPropertyExtensionType.BottomBorderColor;
			break;
		case CellPropertyExtensionType.LeftBorderColor:
			type = CellPropertyExtensionType.LeftBorderColor;
			break;
		case CellPropertyExtensionType.RightBorderColor:
			type = CellPropertyExtensionType.RightBorderColor;
			break;
		case CellPropertyExtensionType.DiagonalCellBorder:
			type = CellPropertyExtensionType.DiagonalCellBorder;
			break;
		case CellPropertyExtensionType.TextColor:
			type = CellPropertyExtensionType.TextColor;
			break;
		case CellPropertyExtensionType.FontScheme:
			type = CellPropertyExtensionType.FontScheme;
			break;
		case CellPropertyExtensionType.TextIndentationLevel:
			type = CellPropertyExtensionType.TextIndentationLevel;
			break;
		}
		return type;
	}

	internal Color ConvertARGBToRGBA(Color colorValue)
	{
		byte b = colorValue.B;
		byte g = colorValue.G;
		byte r = colorValue.R;
		colorValue = Color.FromArgb(colorValue.A, b, g, r);
		return colorValue;
	}

	internal Color ConvertRGBAToARGB(Color colorValue)
	{
		byte a = colorValue.A;
		byte b = colorValue.B;
		byte g = colorValue.G;
		byte r = colorValue.R;
		colorValue = Color.FromArgb(a, b, g, r);
		return colorValue;
	}

	internal uint ColorToUInt(Color color)
	{
		return (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);
	}

	internal Color UIntToColor(uint color)
	{
		byte alpha = (byte)(color >> 24);
		byte red = (byte)(color >> 16);
		byte green = (byte)(color >> 8);
		byte blue = (byte)color;
		return Color.FromArgb(alpha, red, green, blue);
	}

	private bool IsLegalXmlChar(int character)
	{
		if (character != 9 && character != 10 && character != 13 && (character < 32 || character > 55295) && (character < 57344 || character > 65533))
		{
			if (character >= 65536)
			{
				return character <= 1114111;
			}
			return false;
		}
		return true;
	}

	internal string RemoveInvalidXmlCharacters(string nameValue)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(nameValue);
		bool flag = false;
		for (int i = 0; i < bytes.Length; i++)
		{
			if (!IsLegalXmlChar(bytes[i]) && bytes[i] != 0)
			{
				bytes[i] = 0;
				flag = true;
			}
		}
		string @string = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		@string = @string.Replace("\0", string.Empty);
		if (flag)
		{
			@string = "_" + XmlInvalidCharCount + @string;
			XmlInvalidCharCount++;
		}
		return @string;
	}

	internal List<string> GetDrawString(string cellText, RichTextString RTF, out List<IOfficeFont> richTextFonts, IOfficeFont excelFont)
	{
		List<IOfficeFont> list = new List<IOfficeFont>();
		IList<int> values = RTF.TextObject.FormattingRuns.Values;
		int formattingRunsCount = RTF.TextObject.FormattingRunsCount;
		IList<int> keys = RTF.TextObject.FormattingRuns.Keys;
		List<string> list2 = new List<string>();
		string empty = string.Empty;
		int num = 0;
		if (formattingRunsCount == 0)
		{
			list2.Add(RTF.Text);
			richTextFonts = new List<IOfficeFont>();
			richTextFonts.Add(excelFont);
			return list2;
		}
		if (keys[0] != 0)
		{
			num = 1;
			empty = cellText.Substring(0, keys[0]);
			UpdateRTFText(excelFont, list, list2, empty);
		}
		IOfficeFont excelFont2;
		for (int i = 0; i < formattingRunsCount - 1; i++)
		{
			empty = cellText.Substring(keys[i], keys[i + 1] - keys[i]);
			excelFont2 = InnerFonts[values[i]];
			UpdateRTFText(excelFont2, list, list2, empty);
			num++;
		}
		empty = cellText.Substring(keys[formattingRunsCount - 1], cellText.Length - keys[formattingRunsCount - 1]);
		excelFont2 = RTF.GetFontByIndex(values[formattingRunsCount - 1]);
		UpdateRTFText(excelFont2, list, list2, empty);
		richTextFonts = list;
		return list2;
	}

	private void UpdateRTFText(IOfficeFont excelFont, List<IOfficeFont> pdfFonts, List<string> drawString, string rtfText)
	{
		if (rtfText.Contains("\n"))
		{
			char[] array = rtfText.ToCharArray();
			string text = string.Empty;
			char[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				char c = array2[i];
				if (c.ToString().Equals("\n"))
				{
					if (text != string.Empty)
					{
						drawString.Add(text);
						pdfFonts.Add(excelFont);
						text = string.Empty;
					}
					drawString.Add(c.ToString());
					pdfFonts.Add(excelFont);
				}
				else
				{
					text += c;
				}
			}
			if (text != string.Empty)
			{
				drawString.Add(text);
				pdfFonts.Add(excelFont);
				text = string.Empty;
			}
		}
		else
		{
			drawString.Add(rtfText);
			pdfFonts.Add(excelFont);
		}
	}

	internal bool IsNullOrWhiteSpace(string text)
	{
		if (text == null)
		{
			return true;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (!char.IsWhiteSpace(text[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal double GetScaledHeight(string fontName, double fontSize, IWorksheet sheet)
	{
		if (fontName == "Calibri" && fontSize == 10.0)
		{
			return 1.0276243093922652;
		}
		if (fontName == "Calibri" && fontSize == 11.0)
		{
			return 0.8954000466666667;
		}
		if (fontName == "Arial" && fontSize == 8.0)
		{
			return 0.81777777777;
		}
		if (fontName == "Arial" && fontSize == 9.0)
		{
			return 310.0 / 333.0;
		}
		if (fontName == "Arial" && fontSize == 10.0)
		{
			return 0.90196078431;
		}
		if (fontName == "Arial" && fontSize == 11.0)
		{
			return 0.8877193403508772;
		}
		if (fontName == "Arial" && fontSize == 12.0)
		{
			return 0.9201500415802002;
		}
		if (fontName == "Verdana" && fontSize == 10.0)
		{
			return 0.9532016679352405;
		}
		return GetCellScaledWidthHeight(sheet)[1];
	}

	internal double[] GetCellScaledWidthHeight(IWorksheet sheet)
	{
		string standardFont = StandardFont;
		double standardFontSize = StandardFontSize;
		double[] array = new double[2] { 1.0, 1.0 };
		if (standardFont == "Calibri" && standardFontSize == 10.0)
		{
			array[0] = 649.0 / 675.0;
			array[1] = 1.0276243093922652;
		}
		else if (standardFont == "Calibri" && standardFontSize == 11.0)
		{
			array[0] = 1.076285240464345;
			array[1] = 0.9697601668404588;
		}
		else if (standardFont == "Arial" && standardFontSize == 9.0)
		{
			array[0] = 649.0 / 675.0;
			array[1] = 310.0 / 333.0;
		}
		else if (standardFont == "Arial" && standardFontSize == 10.0)
		{
			if (sheet.PageSetup.Orientation == OfficePageOrientation.Portrait)
			{
				array[0] = 1.05504950495;
			}
			else
			{
				array[0] = 1.0521920668058455;
			}
			array[1] = 0.97064110245;
		}
		else if (standardFont == "Arial" && standardFontSize == 11.0)
		{
			array[0] = 1.020440251572327;
			array[1] = 0.9617373319544984;
		}
		return array;
	}

	internal Font GetFont(IOfficeFont font)
	{
		FontStyle fontStyle = GetFontStyle(font);
		return new Font(font.FontName, (float)font.Size, fontStyle);
	}

	internal Font GetSystemFont(IOfficeFont font, string fontName)
	{
		FontStyle fontStyle = GetFontStyle(font);
		return new Font(fontName, GetFontSize(font), fontStyle);
	}

	internal FontStyle GetFontStyle(IOfficeFont font)
	{
		FontStyle fontStyle = FontStyle.Regular;
		if (font.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (font.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (font.Strikethrough)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		if (font.Underline != 0)
		{
			fontStyle |= FontStyle.Underline;
		}
		return fontStyle;
	}

	internal float GetFontSize(IOfficeFont font)
	{
		float num = (float)font.Size;
		float num2 = ((num == 0f) ? 0.5f : num);
		if (font.Superscript || font.Subscript)
		{
			num2 /= 1.5f;
		}
		return num2;
	}
}
