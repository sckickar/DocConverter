using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using DocGen.Compression;
using DocGen.Compression.Zip;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class ApplicationImpl : IApplication, IParentApplication
{
	private const double DEF_ZERO_CHAR_WIDTH = 8.0;

	private const int DEF_BUILD_NUMBER = 0;

	private const double DEF_STANDARD_FONT_SIZE = 10.0;

	private const int DEF_FIXED_DECIMAL_PLACES = 4;

	private const int DEF_SHEETS_IN_NEW_WORKBOOK = 3;

	private const string DEF_DEFAULT_FONT = "Arial";

	private const string DEF_VALUE = "Microsoft Excel";

	private const string DEF_SWITCH_NAME = "DocGen.OfficeChart.DebugInfo";

	private const string DEF_SWITCH_DESCRIPTION = "Indicates wether to show library debug messages.";

	public const char DEF_ARGUMENT_SEPARATOR = ',';

	public const char DEF_ROW_SEPARATOR = ';';

	private static readonly byte[] DEF_XLS_FILE_HEADER;

	private const byte DEF_BIFF_HEADER_SIZE = 8;

	private const string DEF_XML_HEADER = "<?xml";

	private const string DEF_HTML_HEADER = "<html";

	private const int DEF_BUFFER_SIZE = 512;

	private static readonly double[] s_arrProportions;

	internal static readonly SizeF MinCellSize;

	private static readonly bool m_bDebugMessage;

	internal static Type[] AssemblyTypes;

	private object m_parent;

	private static bool m_bIsDebugInfoEnabled;

	private StringEnumerations m_stringEnum = new StringEnumerations();

	private static OfficeDataProviderType m_dataType;

	private bool m_isChangeSeparator;

	private string[] m_defaultStyleNames = new string[54]
	{
		"Normal", "RowLevel_", "ColLevel_", "Comma", "Currency", "Percent", "Comma [0]", "Currency [0]", "Hyperlink", "Followed Hyperlink",
		"Note", "Warning Text", "Emphasis 1", "Emphasis 2", "", "Title", "Heading 1", "Heading 2", "Heading 3", "Heading 4",
		"Input", "Output", "Calculation", "Check Cell", "Linked Cell", "Total", "Good", "Bad", "Neutral", "Accent1",
		"20% - Accent1", "40% - Accent1", "60% - Accent1", "Accent2", "20% - Accent2", "40% - Accent2", "60% - Accent2", "Accent3", "20% - Accent3", "40% - Accent3",
		"60% - Accent3", "Accent4", "20% - Accent4", "40% - Accent4", "60% - Accent4", "Accent5", "20% - Accent5", "40% - Accent5", "60% - Accent5", "Accent6",
		"20% - Accent6", "40% - Accent6", "60% - Accent6", "Explanatory Text"
	};

	private Dictionary<int, PageSetupBaseImpl.PaperSizeEntry> m_dicPaperSizeTable;

	private IRange m_ActiveCell;

	private WorksheetBaseImpl m_ActiveSheet;

	private IWorkbook m_ActiveBook;

	private WorkbooksCollection m_workbooks;

	private bool m_bFixedDecimal;

	private bool m_bUseSystemSep;

	private double m_dbStandardFontSize;

	private int m_iFixedDecimalPlaces;

	private int m_iSheetsInNewWorkbook;

	private string m_strDecimalSeparator;

	private string m_strStandardFont;

	private string m_strThousandsSeparator;

	private string m_strUserName;

	private bool m_bChangeStyle;

	private OfficeSkipExtRecords m_enSkipExtRecords;

	private int m_iStandardRowHeight = 255;

	private bool m_bStandartRowHeightFlag;

	private double m_dStandardColWidth = 8.43;

	private bool m_bOptimizeFonts;

	private bool m_bOptimizeImport;

	private char m_chRowSeparator = ';';

	private char m_chArgumentSeparator = ',';

	private string m_strCSVSeparator = ",";

	private bool m_bUseFastRecordParsing;

	private int m_iRowStorageBlock = 128;

	private bool m_bDeleteDestinationFile = true;

	private CultureInfo m_standredCulture;

	private CultureInfo m_currentCulture;

	private OfficeVersion m_defaultVersion = OfficeVersion.Excel2013;

	private bool m_bNetStorage = true;

	private bool m_bEvalExpired;

	private bool m_bIsDefaultFontChanged;

	private CompressionLevel? m_compressionLevel;

	private bool m_preserveTypes;

	private bool m_isFormulaparsed = true;

	private StyleImpl.StyleSettings[] m_builtInStyleInfo;

	private const int DefaultRowHeightXlsx = 300;

	private IOfficeChartToImageConverter m_chartToImageConverter;

	private bool m_isExternBookParsing;

	private int dpiX = 96;

	private int dpiY = 96;

	public string[] DefaultStyleNames => m_defaultStyleNames;

	internal StyleImpl.StyleSettings[] BuiltInStyleInfo => m_builtInStyleInfo;

	internal Dictionary<int, PageSetupBaseImpl.PaperSizeEntry> DicPaperSizeTable => m_dicPaperSizeTable;

	public static bool IsDebugInfoEnabled
	{
		get
		{
			return m_bIsDebugInfoEnabled;
		}
		set
		{
			m_bIsDebugInfoEnabled = value;
		}
	}

	[Obsolete]
	public static bool UseUnsafeCodeStatic
	{
		get
		{
			return m_dataType == OfficeDataProviderType.Unsafe;
		}
		set
		{
			if (value)
			{
				m_dataType = OfficeDataProviderType.Unsafe;
			}
			else
			{
				m_dataType = OfficeDataProviderType.Native;
			}
		}
	}

	public static OfficeDataProviderType DataProviderTypeStatic
	{
		get
		{
			return m_dataType;
		}
		set
		{
			m_dataType = value;
		}
	}

	public bool IsSaved
	{
		get
		{
			IWorkbooks workbooks = Workbooks;
			int i = 0;
			for (int count = workbooks.Count; i < count; i++)
			{
				if (!workbooks[i].Saved)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool IsFormulaParsed
	{
		get
		{
			return m_isFormulaparsed;
		}
		set
		{
			m_isFormulaparsed = value;
		}
	}

	public int StandardHeightInRowUnits => m_iStandardRowHeight;

	internal bool EvalExpired
	{
		get
		{
			return m_bEvalExpired;
		}
		set
		{
			m_bEvalExpired = value;
		}
	}

	public IRange ActiveCell => m_ActiveCell;

	public IWorksheet ActiveSheet => m_ActiveSheet as IWorksheet;

	public IWorkbook ActiveWorkbook => m_ActiveBook;

	public IApplication Application => this;

	public IWorkbooks Workbooks
	{
		[DebuggerStepThrough]
		get
		{
			return m_workbooks;
		}
	}

	public IWorksheets Worksheets
	{
		get
		{
			if (ActiveWorkbook != null)
			{
				return ActiveWorkbook.Worksheets;
			}
			return null;
		}
	}

	public object Parent
	{
		[DebuggerStepThrough]
		get
		{
			return null;
		}
	}

	public IRange Range => CreateRange(this);

	public bool FixedDecimal
	{
		get
		{
			return m_bFixedDecimal;
		}
		set
		{
			m_bFixedDecimal = value;
		}
	}

	public bool UseSystemSeparators
	{
		get
		{
			return m_bUseSystemSep;
		}
		set
		{
			m_bUseSystemSep = value;
		}
	}

	public double StandardFontSize
	{
		get
		{
			return m_dbStandardFontSize;
		}
		set
		{
			m_dbStandardFontSize = value;
			m_bIsDefaultFontChanged = true;
		}
	}

	public int Build
	{
		[DebuggerStepThrough]
		get
		{
			return 0;
		}
	}

	public int FixedDecimalPlaces
	{
		get
		{
			return m_iFixedDecimalPlaces;
		}
		set
		{
			m_iFixedDecimalPlaces = value;
		}
	}

	public int SheetsInNewWorkbook
	{
		get
		{
			return m_iSheetsInNewWorkbook;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("Sheets in workbook cannot be less then 1");
			}
			m_iSheetsInNewWorkbook = value;
		}
	}

	public string DecimalSeparator
	{
		get
		{
			return m_strDecimalSeparator;
		}
		set
		{
			m_strDecimalSeparator = value;
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			if (m_currentCulture.Name == currentCulture.Name)
			{
				SetNumberDecimalSeparator(m_currentCulture.NumberFormat, m_strDecimalSeparator);
				m_isChangeSeparator = true;
			}
			else
			{
				SetNumberDecimalSeparator(currentCulture.NumberFormat, m_strDecimalSeparator);
			}
		}
	}

	public string StandardFont
	{
		get
		{
			return m_strStandardFont;
		}
		set
		{
			m_strStandardFont = value;
			m_bIsDefaultFontChanged = true;
		}
	}

	public string ThousandsSeparator
	{
		get
		{
			return m_strThousandsSeparator;
		}
		set
		{
			m_strThousandsSeparator = value;
			m_currentCulture.NumberFormat.NumberGroupSeparator = m_strThousandsSeparator;
			m_currentCulture.NumberFormat.PercentGroupSeparator = m_strThousandsSeparator;
			m_currentCulture.NumberFormat.CurrencyGroupSeparator = m_strThousandsSeparator;
		}
	}

	public string UserName
	{
		get
		{
			return m_strUserName;
		}
		set
		{
			m_strUserName = value;
		}
	}

	public string Value
	{
		[DebuggerStepThrough]
		get
		{
			return "Microsoft Excel";
		}
	}

	public bool ChangeStyleOnCellEdit
	{
		get
		{
			return m_bChangeStyle;
		}
		set
		{
			if (value != m_bChangeStyle)
			{
				if (m_workbooks.Count > 0)
				{
					throw new ArgumentException("ChangeStyleOnCellEdit property can be changed only when Application does not contains any workbook");
				}
				m_bChangeStyle = value;
			}
		}
	}

	public OfficeSkipExtRecords SkipOnSave
	{
		get
		{
			return m_enSkipExtRecords;
		}
		set
		{
			m_enSkipExtRecords = value;
		}
	}

	public double StandardHeight
	{
		get
		{
			return (double)m_iStandardRowHeight / 20.0;
		}
		set
		{
			if (value < 0.0)
			{
				throw new ArgumentOutOfRangeException("StandardHeight");
			}
			m_iStandardRowHeight = (int)(value * 20.0);
			m_bStandartRowHeightFlag = true;
		}
	}

	public bool StandardHeightFlag
	{
		get
		{
			return m_bStandartRowHeightFlag;
		}
		set
		{
			m_bStandartRowHeightFlag = value;
		}
	}

	public double StandardWidth
	{
		get
		{
			return m_dStandardColWidth;
		}
		set
		{
			if (m_dStandardColWidth != value)
			{
				m_dStandardColWidth = value;
			}
		}
	}

	public bool OptimizeFonts
	{
		get
		{
			return m_bOptimizeFonts;
		}
		set
		{
			m_bOptimizeFonts = value;
		}
	}

	public bool OptimizeImport
	{
		get
		{
			return m_bOptimizeImport;
		}
		set
		{
			m_bOptimizeImport = value;
		}
	}

	public char RowSeparator
	{
		get
		{
			return m_chRowSeparator;
		}
		set
		{
			m_chRowSeparator = value;
		}
	}

	public char ArgumentsSeparator
	{
		get
		{
			return m_chArgumentSeparator;
		}
		set
		{
			m_chArgumentSeparator = value;
			m_currentCulture.TextInfo.ListSeparator = Convert.ToString(m_chArgumentSeparator);
		}
	}

	public string CSVSeparator
	{
		get
		{
			return m_strCSVSeparator;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("value - string cannot be empty");
			}
			m_strCSVSeparator = value;
		}
	}

	[Obsolete]
	public bool UseNativeOptimization
	{
		get
		{
			return UseUnsafeCodeStatic;
		}
		set
		{
			UseUnsafeCodeStatic = value;
		}
	}

	public bool UseFastRecordParsing
	{
		get
		{
			return m_bUseFastRecordParsing;
		}
		set
		{
			m_bUseFastRecordParsing = value;
		}
	}

	public int RowStorageAllocationBlockSize
	{
		get
		{
			return m_iRowStorageBlock;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("RowStorageAllocationBlock", "Property must be larger than zero.");
			}
			m_iRowStorageBlock = value;
		}
	}

	public bool DeleteDestinationFile
	{
		get
		{
			return m_bDeleteDestinationFile;
		}
		set
		{
			m_bDeleteDestinationFile = value;
		}
	}

	public OfficeVersion DefaultVersion
	{
		get
		{
			return m_defaultVersion;
		}
		set
		{
			m_defaultVersion = value;
			if (!m_bIsDefaultFontChanged)
			{
				CheckDefaultFont();
			}
			if (value != 0)
			{
				m_iStandardRowHeight = 300;
				m_bStandartRowHeightFlag = true;
			}
		}
	}

	public bool UseNativeStorage
	{
		get
		{
			return !m_bNetStorage;
		}
		set
		{
			m_bNetStorage = !value;
		}
	}

	public OfficeDataProviderType DataProviderType
	{
		get
		{
			return m_dataType;
		}
		set
		{
			m_dataType = value;
		}
	}

	public CompressionLevel? CompressionLevel
	{
		get
		{
			return m_compressionLevel;
		}
		set
		{
			m_compressionLevel = value;
		}
	}

	public bool PreserveCSVDataTypes
	{
		get
		{
			return m_preserveTypes;
		}
		set
		{
			m_preserveTypes = value;
		}
	}

	internal StringEnumerations StringEnum => m_stringEnum;

	internal bool IsExternBookParsing
	{
		get
		{
			return m_isExternBookParsing;
		}
		set
		{
			m_isExternBookParsing = value;
		}
	}

	public IOfficeChartToImageConverter ChartToImageConverter
	{
		get
		{
			return m_chartToImageConverter;
		}
		set
		{
			m_chartToImageConverter = value;
		}
	}

	public event ProgressEventHandler ProgressEvent;

	public event PasswordRequiredEventHandler OnPasswordRequired;

	public event PasswordRequiredEventHandler OnWrongPassword;

	internal void SetNumberDecimalSeparator(NumberFormatInfo numberFormat, string numberDecimalSeparator)
	{
		numberFormat.NumberDecimalSeparator = numberDecimalSeparator;
		numberFormat.PercentDecimalSeparator = numberDecimalSeparator;
		numberFormat.CurrencyDecimalSeparator = numberDecimalSeparator;
	}

	internal static void InitAssemblyTypes()
	{
		TypeInfo[] array = typeof(ApplicationImpl).GetTypeInfo().Assembly.DefinedTypes.ToArray();
		AssemblyTypes = new Type[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			AssemblyTypes[i] = array[i].AsType();
		}
	}

	static ApplicationImpl()
	{
		DEF_XLS_FILE_HEADER = new byte[8] { 208, 207, 17, 224, 161, 177, 26, 225 };
		MinCellSize = new SizeF(8.43f, 12.75f);
		m_bDebugMessage = false;
		m_bIsDebugInfoEnabled = false;
		m_dataType = OfficeDataProviderType.ByteArray;
		s_arrProportions = new double[8] { 1.28, 0.32, 96.0, 3.7795275590551185, 37.79527559055118, 1.0, 1.3333333333333333, 0.00010498687664041994 };
		MinCellSize.Height = (float)ConvertToPixels(MinCellSize.Height, MeasureUnits.Point);
		try
		{
			AssemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			if (ex.Types == null)
			{
				return;
			}
			List<Type> list = new List<Type>(ex.Types.Length);
			Type[] types = ex.Types;
			foreach (Type type in types)
			{
				if (type != null)
				{
					list.Add(type);
				}
			}
			AssemblyTypes = list.ToArray();
		}
	}

	public ApplicationImpl(ExcelEngine excelEngine)
	{
		m_parent = excelEngine;
		m_builtInStyleInfo = new StyleImpl.StyleSettings[m_defaultStyleNames.Length];
		m_dbStandardFontSize = 10.0;
		m_iFixedDecimalPlaces = 4;
		m_iSheetsInNewWorkbook = 3;
		m_standredCulture = CultureInfo.InvariantCulture;
		m_currentCulture = new CultureInfo(CultureInfo.CurrentCulture.Name);
		m_strDecimalSeparator = m_currentCulture.NumberFormat.NumberDecimalSeparator;
		m_strStandardFont = "Tahoma";
		m_strThousandsSeparator = m_currentCulture.NumberFormat.NumberGroupSeparator;
		m_chArgumentSeparator = Convert.ToChar(m_currentCulture.TextInfo.ListSeparator);
		InitializeCollection();
		InitializeStyleCollections();
		InitializePageSetup();
	}

	private void InitializePageSetup()
	{
		m_dicPaperSizeTable = new Dictionary<int, PageSetupBaseImpl.PaperSizeEntry>();
		m_dicPaperSizeTable.Add(1, new PageSetupBaseImpl.PaperSizeEntry(8.5, 11.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(2, new PageSetupBaseImpl.PaperSizeEntry(8.5, 11.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(3, new PageSetupBaseImpl.PaperSizeEntry(11.0, 17.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(4, new PageSetupBaseImpl.PaperSizeEntry(17.0, 11.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(5, new PageSetupBaseImpl.PaperSizeEntry(8.5, 14.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(6, new PageSetupBaseImpl.PaperSizeEntry(5.5, 8.5, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(7, new PageSetupBaseImpl.PaperSizeEntry(7.25, 10.5, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(8, new PageSetupBaseImpl.PaperSizeEntry(297.0, 420.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(9, new PageSetupBaseImpl.PaperSizeEntry(210.0, 297.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(10, new PageSetupBaseImpl.PaperSizeEntry(210.0, 297.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(11, new PageSetupBaseImpl.PaperSizeEntry(148.0, 210.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(12, new PageSetupBaseImpl.PaperSizeEntry(257.0, 368.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(13, new PageSetupBaseImpl.PaperSizeEntry(182.0, 257.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(14, new PageSetupBaseImpl.PaperSizeEntry(8.5, 13.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(15, new PageSetupBaseImpl.PaperSizeEntry(215.0, 275.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(16, new PageSetupBaseImpl.PaperSizeEntry(10.0, 14.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(17, new PageSetupBaseImpl.PaperSizeEntry(11.0, 17.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(18, new PageSetupBaseImpl.PaperSizeEntry(8.5, 11.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(19, new PageSetupBaseImpl.PaperSizeEntry(3.875, 8.875, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(20, new PageSetupBaseImpl.PaperSizeEntry(4.125, 9.5, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(21, new PageSetupBaseImpl.PaperSizeEntry(4.5, 10.375, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(22, new PageSetupBaseImpl.PaperSizeEntry(4.75, 11.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(23, new PageSetupBaseImpl.PaperSizeEntry(5.0, 11.5, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(24, new PageSetupBaseImpl.PaperSizeEntry(17.0, 22.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(25, new PageSetupBaseImpl.PaperSizeEntry(22.0, 34.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(26, new PageSetupBaseImpl.PaperSizeEntry(34.0, 44.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(27, new PageSetupBaseImpl.PaperSizeEntry(110.0, 220.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(28, new PageSetupBaseImpl.PaperSizeEntry(162.0, 229.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(29, new PageSetupBaseImpl.PaperSizeEntry(324.0, 458.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(30, new PageSetupBaseImpl.PaperSizeEntry(229.0, 324.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(31, new PageSetupBaseImpl.PaperSizeEntry(114.0, 162.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(32, new PageSetupBaseImpl.PaperSizeEntry(114.0, 229.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(33, new PageSetupBaseImpl.PaperSizeEntry(250.0, 353.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(34, new PageSetupBaseImpl.PaperSizeEntry(176.0, 250.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(35, new PageSetupBaseImpl.PaperSizeEntry(125.0, 176.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(36, new PageSetupBaseImpl.PaperSizeEntry(110.0, 230.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(37, new PageSetupBaseImpl.PaperSizeEntry(3.875, 7.5, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(38, new PageSetupBaseImpl.PaperSizeEntry(3.625, 6.5, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(39, new PageSetupBaseImpl.PaperSizeEntry(14.875, 11.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(40, new PageSetupBaseImpl.PaperSizeEntry(8.5, 12.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(41, new PageSetupBaseImpl.PaperSizeEntry(8.5, 13.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(42, new PageSetupBaseImpl.PaperSizeEntry(250.0, 353.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(43, new PageSetupBaseImpl.PaperSizeEntry(100.0, 148.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(44, new PageSetupBaseImpl.PaperSizeEntry(9.0, 11.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(45, new PageSetupBaseImpl.PaperSizeEntry(10.0, 11.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(46, new PageSetupBaseImpl.PaperSizeEntry(15.0, 11.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(47, new PageSetupBaseImpl.PaperSizeEntry(220.0, 220.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(50, new PageSetupBaseImpl.PaperSizeEntry(9.5, 12.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(51, new PageSetupBaseImpl.PaperSizeEntry(9.5, 15.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(52, new PageSetupBaseImpl.PaperSizeEntry(11.6875, 18.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(53, new PageSetupBaseImpl.PaperSizeEntry(235.0, 322.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(54, new PageSetupBaseImpl.PaperSizeEntry(8.5, 11.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(55, new PageSetupBaseImpl.PaperSizeEntry(210.0, 297.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(56, new PageSetupBaseImpl.PaperSizeEntry(9.5, 12.0, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(57, new PageSetupBaseImpl.PaperSizeEntry(227.0, 356.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(58, new PageSetupBaseImpl.PaperSizeEntry(305.0, 487.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(59, new PageSetupBaseImpl.PaperSizeEntry(8.5, 12.6875, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(60, new PageSetupBaseImpl.PaperSizeEntry(210.0, 330.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(61, new PageSetupBaseImpl.PaperSizeEntry(148.0, 210.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(62, new PageSetupBaseImpl.PaperSizeEntry(182.0, 257.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(63, new PageSetupBaseImpl.PaperSizeEntry(322.0, 445.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(64, new PageSetupBaseImpl.PaperSizeEntry(174.0, 235.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(65, new PageSetupBaseImpl.PaperSizeEntry(201.0, 276.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(66, new PageSetupBaseImpl.PaperSizeEntry(420.0, 594.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(67, new PageSetupBaseImpl.PaperSizeEntry(297.0, 420.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(68, new PageSetupBaseImpl.PaperSizeEntry(322.0, 445.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(69, new PageSetupBaseImpl.PaperSizeEntry(200.0, 148.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(70, new PageSetupBaseImpl.PaperSizeEntry(105.0, 148.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(75, new PageSetupBaseImpl.PaperSizeEntry(11.0, 8.5, MeasureUnits.Inch));
		m_dicPaperSizeTable.Add(76, new PageSetupBaseImpl.PaperSizeEntry(420.0, 297.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(77, new PageSetupBaseImpl.PaperSizeEntry(297.0, 210.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(78, new PageSetupBaseImpl.PaperSizeEntry(210.0, 148.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(79, new PageSetupBaseImpl.PaperSizeEntry(364.0, 257.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(80, new PageSetupBaseImpl.PaperSizeEntry(257.0, 182.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(81, new PageSetupBaseImpl.PaperSizeEntry(148.0, 100.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(82, new PageSetupBaseImpl.PaperSizeEntry(148.0, 200.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(83, new PageSetupBaseImpl.PaperSizeEntry(148.0, 105.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(88, new PageSetupBaseImpl.PaperSizeEntry(128.0, 182.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(89, new PageSetupBaseImpl.PaperSizeEntry(182.0, 128.0, MeasureUnits.Millimeter));
		m_dicPaperSizeTable.Add(90, new PageSetupBaseImpl.PaperSizeEntry(12.0, 11.0, MeasureUnits.Inch));
	}

	private void InitializeStyleCollections()
	{
		int num = 0;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		FillImpl fill = new FillImpl(OfficePattern.Solid, Color.FromArgb(255, 255, 255, 204), ColorExtension.Empty);
		StyleImpl.FontSettings font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		StyleImpl.BorderSettings borders = new StyleImpl.BorderSettings(Color.FromArgb(255, 178, 178, 178), OfficeLineStyle.Thin);
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font, borders);
		num++;
		font = new StyleImpl.FontSettings(Color.FromArgb(255, 255, 0, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, font);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, null);
		num++;
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 3), 18, FontStyle.Bold, "Cambria");
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, font);
		num++;
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 3), 15, FontStyle.Bold);
		borders = new StyleImpl.BorderSettings(new ChartColor(ColorType.Theme, 4), OfficeLineStyle.None, OfficeLineStyle.None, OfficeLineStyle.None, OfficeLineStyle.Thick);
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, font, borders);
		num++;
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 3), 13, FontStyle.Bold);
		borders = new StyleImpl.BorderSettings(new ChartColor(ColorType.Theme, 4, 0.499984740745262), OfficeLineStyle.None, OfficeLineStyle.None, OfficeLineStyle.None, OfficeLineStyle.Thick);
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, font, borders);
		num++;
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 3), FontStyle.Bold), new StyleImpl.BorderSettings(new ChartColor(ColorType.Theme, 4, 0.3999755851924192), OfficeLineStyle.None, OfficeLineStyle.None, OfficeLineStyle.None, OfficeLineStyle.Medium));
		num++;
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 3), FontStyle.Bold);
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, Color.FromArgb(255, 255, 204, 153), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(Color.FromArgb(255, 63, 63, 118));
		borders = new StyleImpl.BorderSettings(Color.FromArgb(255, 127, 127, 127), OfficeLineStyle.Thin);
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font, borders);
		num++;
		fill = new FillImpl(OfficePattern.Solid, Color.FromArgb(255, 242, 242, 242), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(Color.FromArgb(255, 63, 63, 63), FontStyle.Bold);
		borders = new StyleImpl.BorderSettings(Color.FromArgb(255, 63, 63, 63), OfficeLineStyle.Thin);
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font, borders);
		num++;
		fill = new FillImpl(OfficePattern.Solid, Color.FromArgb(255, 242, 242, 242), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(Color.FromArgb(255, 250, 125, 0), FontStyle.Bold);
		borders = new StyleImpl.BorderSettings(Color.FromArgb(255, 127, 127, 127), OfficeLineStyle.Thin);
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font, borders);
		num++;
		fill = new FillImpl(OfficePattern.Solid, Color.FromArgb(255, 165, 165, 165), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0), FontStyle.Bold);
		borders = new StyleImpl.BorderSettings(Color.FromArgb(255, 63, 63, 63), OfficeLineStyle.Double);
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font, borders);
		num++;
		font = new StyleImpl.FontSettings(Color.FromArgb(255, 250, 125, 0));
		borders = new StyleImpl.BorderSettings(Color.FromArgb(255, 255, 128, 1), OfficeLineStyle.None, OfficeLineStyle.None, OfficeLineStyle.None, OfficeLineStyle.Double);
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, font, borders);
		num++;
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1), FontStyle.Bold);
		borders = new StyleImpl.BorderSettings(new ChartColor(ColorType.Theme, 4), OfficeLineStyle.None, OfficeLineStyle.None, OfficeLineStyle.Thin, OfficeLineStyle.Double);
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, font, borders);
		num++;
		fill = new FillImpl(OfficePattern.Solid, Color.FromArgb(255, 198, 239, 206), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(Color.FromArgb(255, 0, 97, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, Color.FromArgb(255, 255, 199, 206), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(Color.FromArgb(255, 156, 0, 6));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, Color.FromArgb(255, 255, 235, 156), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(Color.FromArgb(255, 156, 101, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 4), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 4, 0.7999816888943144), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 4, 0.5999938962981048), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 4, 0.3999755851924192), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 5), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 5, 0.7999816888943144), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 5, 0.5999938962981048), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 5, 0.3999755851924192), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 6), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 6, 0.7999816888943144), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 6, 0.5999938962981048), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 6, 0.3999755851924192), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 7), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 7, 0.7999816888943144), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 7, 0.5999938962981048), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 7, 0.3999755851924192), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 8), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 8, 0.7999816888943144), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 8, 0.5999938962981048), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 8, 0.3999755851924192), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 9), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 9, 0.7999816888943144), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 9, 0.5999938962981048), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 1));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		fill = new FillImpl(OfficePattern.Solid, new ChartColor(ColorType.Theme, 9, 0.3999755851924192), ColorExtension.Empty);
		font = new StyleImpl.FontSettings(new ChartColor(ColorType.Theme, 0));
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(fill, font);
		num++;
		font = new StyleImpl.FontSettings(Color.FromArgb(255, 127, 127, 127), FontStyle.Italic);
		BuiltInStyleInfo[num] = new StyleImpl.StyleSettings(null, font);
		num++;
	}

	protected void InitializeCollection()
	{
		m_workbooks = new WorkbooksCollection(Application, this);
	}

	public double CentimetersToPoints(double Centimeters)
	{
		return ConvertUnits((float)Centimeters, MeasureUnits.Centimeter, MeasureUnits.Point);
	}

	public bool IsSupported(Stream stream)
	{
		return true;
	}

	public OfficeOpenType DetectFileFromStream(Stream stream)
	{
		OfficeOpenType result = OfficeOpenType.Automatic;
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		long position = stream.Position;
		if (ZipArchive.ReadInt32(stream) == 67324752)
		{
			stream.Position = position;
			result = OfficeOpenType.SpreadsheetML2007;
		}
		else
		{
			stream.Position = position;
			byte[] array = new byte[512];
			int num = stream.Read(array, 0, 512);
			if (num != 0)
			{
				if (num >= 8)
				{
					for (int i = 0; i < 8 && DEF_XLS_FILE_HEADER[i] == array[i]; i++)
					{
					}
				}
				stream.Position = position;
			}
		}
		return result;
	}

	private OfficeOpenType DetectIsCSVOrXML(Stream stream, MemoryStream memoryStream, long lPosition)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (memoryStream == null)
		{
			throw new ArgumentNullException("memoryStream");
		}
		string cSVSeparator = Application.CSVSeparator;
		bool bIsCompare = IsContainSurrogate(cSVSeparator, "", bIsCompare: false);
		bool flag = false;
		StreamReader streamReader = new StreamReader(memoryStream, detectEncodingFromByteOrderMarks: true);
		string text = streamReader.ReadLine();
		Encoding currentEncoding = streamReader.CurrentEncoding;
		OfficeOpenType officeOpenType = OfficeOpenType.Automatic;
		while (text != null)
		{
			text = text.ToLower();
			int num = text.IndexOf("<?xml");
			if (num != -1)
			{
				stream.Position = lPosition + num;
				officeOpenType = OfficeOpenType.SpreadsheetML;
				break;
			}
			if (text.IndexOf("<html") != -1)
			{
				break;
			}
			if (!flag)
			{
				flag = IsContainSurrogate(text, cSVSeparator, bIsCompare);
			}
			lPosition += currentEncoding.GetByteCount(text);
			lPosition += currentEncoding.GetByteCount("\n") * 2;
			text = streamReader.ReadLine();
		}
		if (officeOpenType == OfficeOpenType.Automatic && !flag)
		{
			officeOpenType = OfficeOpenType.CSV;
		}
		if (!flag)
		{
			return officeOpenType;
		}
		return OfficeOpenType.Automatic;
	}

	private bool IsContainSurrogate(string strValue, string strSeparator, bool bIsCompare)
	{
		if (strValue == null)
		{
			throw new ArgumentNullException("strValue");
		}
		if (strSeparator == null)
		{
			throw new ArgumentNullException("strSeparator");
		}
		int i = 0;
		for (int length = strValue.Length; i < length; i++)
		{
			char c = strValue[i];
			if (!char.IsLetterOrDigit(c) && !char.IsPunctuation(c) && !char.IsSeparator(c) && !char.IsSymbol(c) && !char.IsWhiteSpace(c) && (!bIsCompare || strSeparator.IndexOf(c) == -1))
			{
				return true;
			}
		}
		return false;
	}

	public double InchesToPoints(double Inches)
	{
		return ConvertUnits((float)Inches, MeasureUnits.Inch, MeasureUnits.Point);
	}

	public virtual WorkbookImpl CreateWorkbook(object parent, OfficeVersion version)
	{
		return new WorkbookImpl(this, parent, version);
	}

	public virtual WorkbookImpl CreateWorkbook(object parent, Stream stream, string separator, int row, int column, OfficeVersion version, string fileName, Encoding encoding)
	{
		return new WorkbookImpl(this, parent, stream, separator, row, column, version, fileName, encoding);
	}

	public virtual WorkbookImpl CreateWorkbook(object parent, Stream stream, OfficeVersion version, OfficeParseOptions options)
	{
		return new WorkbookImpl(this, parent, stream, options, version);
	}

	public virtual WorkbookImpl CreateWorkbook(object parent, Stream stream, OfficeParseOptions options, OfficeVersion version)
	{
		return new WorkbookImpl(this, parent, stream, options, version);
	}

	public virtual WorkbookImpl CreateWorkbook(object parent, Stream stream, OfficeParseOptions options, bool bReadOnly, string password, OfficeVersion version)
	{
		return new WorkbookImpl(this, parent, stream, options, bReadOnly, password, version);
	}

	public virtual WorkbookImpl CreateWorkbook(object parent, int sheetsQuantity, OfficeVersion version)
	{
		return new WorkbookImpl(this, parent, sheetsQuantity, version);
	}

	public virtual WorksheetImpl CreateWorksheet(object parent)
	{
		return new WorksheetImpl(this, parent);
	}

	public virtual RangeImpl CreateRange(object parent)
	{
		return new RangeImpl(this, parent);
	}

	public virtual RangeImpl CreateRange(object parent, int col, int row)
	{
		return new RangeImpl(this, parent, col, row);
	}

	[CLSCompliant(false)]
	public virtual RangeImpl CreateRange(object parent, BiffRecordRaw[] data, ref int i)
	{
		return new RangeImpl(this, parent, data, i);
	}

	[CLSCompliant(false)]
	public virtual RangeImpl CreateRange(object parent, BiffRecordRaw[] data, ref int i, bool ignoreStyles)
	{
		return new RangeImpl(this, parent, data, ref i, ignoreStyles);
	}

	public virtual RangeImpl CreateRange(object parent, List<BiffRecordRaw> data, ref int i, bool ignoreStyles)
	{
		return new RangeImpl(this, parent, data, ref i, ignoreStyles);
	}

	public virtual RangeImpl CreateRange(object parent, int firstCol, int firstRow, int lastCol, int lastRow)
	{
		return new RangeImpl(this, parent, firstCol, firstRow, lastCol, lastRow);
	}

	[CLSCompliant(false)]
	public virtual RangeImpl CreateRange(object parent, BiffRecordRaw record, bool bIgnoreStyles)
	{
		return new RangeImpl(this, parent, record, bIgnoreStyles);
	}

	public virtual StyleImpl CreateStyle(WorkbookImpl parent, string name)
	{
		return new StyleImpl(parent, name);
	}

	public virtual StyleImpl CreateStyle(WorkbookImpl parent, string name, StyleImpl basedOn)
	{
		return new StyleImpl(parent, name, basedOn);
	}

	[CLSCompliant(false)]
	public virtual StyleImpl CreateStyle(WorkbookImpl parent, StyleRecord style)
	{
		return new StyleImpl(parent, style);
	}

	public virtual StyleImpl CreateStyle(WorkbookImpl parent, string name, bool bIsBuildIn)
	{
		return new StyleImpl(parent, name, null, bIsBuildIn);
	}

	public virtual FontImpl CreateFont(object parent)
	{
		return new FontImpl(this, parent);
	}

	public virtual FontImpl CreateFont(IOfficeFont basedOn)
	{
		return new FontImpl(basedOn);
	}

	[CLSCompliant(false)]
	public virtual FontImpl CreateFont(object parent, FontRecord font)
	{
		return new FontImpl(this, parent, font);
	}

	[CLSCompliant(false)]
	public virtual FontImpl CreateFont(object parent, FontImpl font)
	{
		return new FontImpl(this, parent, font);
	}

	public void CheckDefaultFont()
	{
		StandardFont = "Calibri";
		StandardFontSize = 11.0;
	}

	public virtual ChartImpl CreateChart(object parent)
	{
		return new ChartImpl(this, parent);
	}

	public virtual ChartSerieImpl CreateSerie(object parent)
	{
		return new ChartSerieImpl(this, parent);
	}

	public RangesCollection CreateRangesCollection(object parent)
	{
		return new RangesCollection(this, parent);
	}

	public static DataProvider CreateDataProvider(nint heapHandle)
	{
		return new ByteArrayDataProvider();
	}

	internal static DataProvider CreateDataProvider()
	{
		return new ByteArrayDataProvider();
	}

	public static Image CreateImage(Stream stream)
	{
		return Image.FromStream(stream, p: true, p3: true);
	}

	internal TextBoxShapeImpl CreateTextBoxShapeImpl(ShapesCollection shapesCollection, WorksheetImpl sheet)
	{
		return new TextBoxShapeImpl(this, shapesCollection, sheet);
	}

	public virtual Stream CreateCompressor(Stream outputStream)
	{
		return new NetCompressor(m_compressionLevel.HasValue ? m_compressionLevel.Value : DocGen.Compression.CompressionLevel.Normal, outputStream);
	}

	internal CultureInfo CheckAndApplySeperators()
	{
		if (!IsFormulaParsed)
		{
			return m_standredCulture;
		}
		if (!(m_currentCulture.Name == CultureInfo.CurrentCulture.Name))
		{
			return CultureInfo.CurrentCulture;
		}
		return GetCultureInfo(m_currentCulture, CultureInfo.CurrentCulture);
	}

	internal CultureInfo GetCultureInfo(CultureInfo oldCulture, CultureInfo newCulture)
	{
		if (m_isChangeSeparator || (oldCulture.NumberFormat.NumberDecimalSeparator == newCulture.NumberFormat.NumberDecimalSeparator && oldCulture.NumberFormat.PercentDecimalSeparator == newCulture.NumberFormat.PercentDecimalSeparator && oldCulture.NumberFormat.CurrencyDecimalSeparator == newCulture.NumberFormat.CurrencyDecimalSeparator && oldCulture.NumberFormat.NumberGroupSeparator == newCulture.NumberFormat.NumberGroupSeparator && oldCulture.NumberFormat.PercentGroupSeparator == newCulture.NumberFormat.PercentGroupSeparator && oldCulture.NumberFormat.CurrencyGroupSeparator == newCulture.NumberFormat.CurrencyGroupSeparator && oldCulture.TextInfo.ListSeparator == newCulture.TextInfo.ListSeparator))
		{
			return oldCulture;
		}
		return newCulture;
	}

	public void SetActiveWorkbook(IWorkbook book)
	{
		m_ActiveBook = book;
	}

	public void SetActiveWorksheet(WorksheetBaseImpl sheet)
	{
		m_ActiveSheet = sheet;
	}

	public void SetActiveCell(IRange cell)
	{
		m_ActiveCell = cell;
	}

	internal static double ConvertToPixels(double value, MeasureUnits from)
	{
		return value * s_arrProportions[(int)from];
	}

	internal static double ConvertFromPixel(double value, MeasureUnits to)
	{
		return value / s_arrProportions[(int)to];
	}

	public static double ConvertUnitsStatic(double value, MeasureUnits from, MeasureUnits to)
	{
		return value * s_arrProportions[(int)from] / s_arrProportions[(int)to];
	}

	public double ConvertUnits(double value, MeasureUnits from, MeasureUnits to)
	{
		if (from != to)
		{
			return value * s_arrProportions[(int)from] / s_arrProportions[(int)to];
		}
		return value;
	}

	public void RaiseProgressEvent(long curPos, long fullSize)
	{
		if (this.ProgressEvent != null)
		{
			this.ProgressEvent(this, new ProgressEventArgs(curPos, fullSize));
		}
	}

	public SizeF MeasureString(string strToMeasure, FontImpl font, SizeF rectSize)
	{
		return SizeF.Empty;
	}

	internal bool RaiseOnPasswordRequired(object sender, PasswordRequiredEventArgs e)
	{
		bool result = false;
		if (this.OnPasswordRequired != null)
		{
			this.OnPasswordRequired(sender, e);
			result = true;
		}
		return result;
	}

	internal bool RaiseOnWrongPassword(object sender, PasswordRequiredEventArgs e)
	{
		bool result = false;
		if (this.OnWrongPassword != null)
		{
			this.OnWrongPassword(sender, e);
			result = true;
		}
		return result;
	}

	internal void Dispose()
	{
		if (ActiveSheet is WorksheetImpl { Parent: not null } worksheetImpl)
		{
			_ = ((WorksheetsCollection)worksheetImpl.Parent).InnerList;
			if (worksheetImpl != null && worksheetImpl.ParentWorkbook != null)
			{
				worksheetImpl.ParentWorkbook.DisposeAll();
			}
		}
		m_defaultStyleNames = null;
		m_workbooks = null;
		RemoveStylesCollection();
		RemovePageSetupCollection();
	}

	internal void RemoveStylesCollection()
	{
		int num = m_builtInStyleInfo.Length;
		for (int i = 0; i < num; i++)
		{
			m_builtInStyleInfo[i].Clear();
			m_builtInStyleInfo[i] = null;
		}
		m_builtInStyleInfo = null;
	}

	internal void RemovePageSetupCollection()
	{
		m_dicPaperSizeTable.Clear();
		m_dicPaperSizeTable = null;
	}

	internal int GetFontCalc1()
	{
		return 182;
	}

	internal int GetFontCalc2()
	{
		return 7;
	}

	internal int GetFontCalc3()
	{
		return 5;
	}

	internal int GetdpiX()
	{
		return dpiX;
	}

	internal int GetdpiY()
	{
		return dpiY;
	}
}
