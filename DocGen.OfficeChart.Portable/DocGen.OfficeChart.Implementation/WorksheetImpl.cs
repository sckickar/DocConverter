using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.OfficeChart.Calculate;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Implementation.XmlSerialization;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation;

internal class WorksheetImpl : WorksheetBaseImpl, IWorksheet, ITabSheet, IParentApplication, ICalcData, ISerializableNamedObject, INamedObject, IParseable, ICloneParent, IInternalWorksheet, ISheetData
{
	private enum RangeProperty
	{
		Value2,
		Text,
		DateTime,
		TimeSpan,
		Double,
		Int
	}

	[Flags]
	public enum TRangeValueType
	{
		Blank = 0,
		Error = 1,
		Boolean = 2,
		Number = 4,
		Formula = 8,
		String = 0x10
	}

	private delegate IOutline OutlineDelegate(int iIndex);

	internal bool m_hasSheetCalculation;

	private bool m_hasAlternateContent;

	internal int unknown_formula_name = 9;

	private CalcEngine m_calcEngine;

	private bool m_copyshapesForSorting;

	internal new PicturesCollection m_pictures;

	internal const char DEF_STANDARD_CHAR = '0';

	private const float DEF_AXE_IN_RADIANS = (float)Math.PI / 180f;

	private const int DEF_MAX_COLUMN_WIDTH = 255;

	private const double DEF_ZERO_CHAR_WIDTH = 8.0;

	private const int DEF_ARRAY_SIZE = 100;

	private const int DEF_AUTO_FILTER_WIDTH = 16;

	private const int DEF_INDENT_WIDTH = 12;

	private const double DEF_OLE_DOUBLE = 2958465.9999999884;

	private const double DEF_MAX_DOUBLE = 2958466.0;

	private const char CarriageReturn = '\r';

	private const char NewLine = '\n';

	private const string MSExcel = "Microsoft Excel";

	private const int DEFAULT_DATE_NUMBER_FORMAT_INDEX = 14;

	private static readonly TBIFFRecord[] s_arrAutofilterRecord;

	private bool m_bIsUnsupportedFormula;

	private Dictionary<int, int> m_indexAndLevels;

	private bool m_bParseDataOnDemand;

	private RangeImpl m_rngUsed;

	private CellRecordCollection m_dicRecordsCells;

	private ColumnInfoRecord[] m_arrColumnInfo;

	private bool m_bDisplayPageBreaks;

	private PageSetupImpl m_pageSetup;

	private double m_dStandardColWidth = 8.43;

	private MergeCellsImpl m_mergedCells;

	private List<SelectionRecord> m_arrSelections;

	private PaneRecord m_pane;

	private WorksheetNamesCollection m_names;

	private OfficeSheetType m_sheetType;

	private bool m_bStringsPreserved;

	private List<BiffRecordRaw> m_arrAutoFilter;

	private SortedList<int, NoteRecord> m_arrNotes;

	private SortedList<long, NoteRecord> m_arrNotesByCellIndex;

	private NameImpl.NameIndexChangedEventHandler m_nameIndexChanged;

	private List<BiffRecordRaw> m_arrSortRecords;

	private int m_iPivotStartIndex = -1;

	private int m_iHyperlinksStartIndex = -1;

	private int m_iCondFmtPos = -1;

	private int m_iDValPos = -1;

	private int m_iCustomPropertyStartIndex = -1;

	private List<BiffRecordRaw> m_arrDConRecords;

	private IMigrantRange m_migrantRange;

	private IndexRecord m_index;

	private bool m_bUsedRangeIncludesFormatting = true;

	private bool m_busedRangeIncludesCF;

	private RangeTrueFalse m_stringPreservedRanges = new RangeTrueFalse();

	private ItemSizeHelper m_rowHeightHelper;

	private List<BiffRecordRaw> m_tableRecords;

	private bool m_isRowHeightSet;

	private bool m_isZeroHeight;

	private int m_baseColumnWidth;

	private bool m_isThickBottom;

	private bool m_isThickTop;

	private byte m_outlineLevelColumn;

	private double m_defaultColWidth = 8.43;

	private byte m_outlineLevelRow;

	private ColumnInfoRecord m_rawColRecord;

	private bool m_bOptimizeImport;

	private OfficeSheetView m_view;

	internal List<Stream> preservedStreams;

	internal Dictionary<int, CondFMTRecord> m_dictCondFMT = new Dictionary<int, CondFMTRecord>();

	internal Dictionary<int, CFExRecord> m_dictCFExRecords = new Dictionary<int, CFExRecord>();

	private AutoFitManager m_autoFitManager;

	internal List<IOutlineWrapper> m_outlineWrappers;

	private Dictionary<int, List<Point>> m_columnOutlineLevels;

	private Dictionary<int, List<Point>> m_rowOutlineLevels;

	private Dictionary<string, string> m_inlineStrings;

	private List<BiffRecordRaw> m_preserveExternalConnection;

	private List<Stream> m_preservePivotTables;

	internal Stream m_worksheetSlicer;

	internal string m_formulaString;

	private bool m_bIsExportDataTable;

	private ColumnCollection columnCollection;

	private bool m_bIsImporting;

	private ExtendedFormatImpl format;

	private int dateTimeStyleIndex;

	internal bool m_parseCondtionalFormats = true;

	internal bool m_parseCF = true;

	private string m_archiveItemName;

	internal CalcEngine CalcEngine
	{
		get
		{
			return m_calcEngine;
		}
		set
		{
			m_calcEngine = value;
		}
	}

	internal bool CopyShapesForSorting
	{
		get
		{
			return m_copyshapesForSorting;
		}
		set
		{
			m_copyshapesForSorting = value;
		}
	}

	internal bool HasSheetCalculation => m_hasSheetCalculation;

	internal bool HasAlternateContent
	{
		get
		{
			return m_hasAlternateContent;
		}
		set
		{
			m_hasAlternateContent = value;
		}
	}

	internal Dictionary<int, int> IndexAndLevels
	{
		get
		{
			if (m_indexAndLevels == null)
			{
				m_indexAndLevels = new Dictionary<int, int>();
			}
			return m_indexAndLevels;
		}
		set
		{
			m_indexAndLevels = value;
		}
	}

	internal Stream WorksheetSlicerStream
	{
		get
		{
			return m_worksheetSlicer;
		}
		set
		{
			m_worksheetSlicer = value;
		}
	}

	internal double DefaultColumnWidth
	{
		get
		{
			return m_defaultColWidth;
		}
		set
		{
			m_defaultColWidth = value;
		}
	}

	public MergeCellsImpl MergeCells
	{
		get
		{
			ParseData();
			if (m_mergedCells == null)
			{
				m_mergedCells = new MergeCellsImpl(base.Application, this);
			}
			return m_mergedCells;
		}
	}

	[CLSCompliant(false)]
	public ColumnInfoRecord[] ColumnInformation
	{
		get
		{
			ParseData();
			return m_arrColumnInfo;
		}
	}

	public int VerticalSplit
	{
		get
		{
			ParseData();
			if (m_pane == null)
			{
				return 0;
			}
			return m_pane.VerticalSplit;
		}
		set
		{
			ParseData();
			if (m_pane == null)
			{
				CreateEmptyPane();
			}
			m_pane.VerticalSplit = (ushort)value;
		}
	}

	public int HorizontalSplit
	{
		get
		{
			ParseData();
			if (m_pane == null)
			{
				return 0;
			}
			return m_pane.HorizontalSplit;
		}
		set
		{
			ParseData();
			if (m_pane == null)
			{
				CreateEmptyPane();
			}
			m_pane.HorizontalSplit = (ushort)value;
		}
	}

	public int FirstVisibleRow
	{
		get
		{
			ParseData();
			if (m_pane == null)
			{
				return 0;
			}
			return m_pane.FirstRow;
		}
		set
		{
			ParseData();
			if (m_pane == null)
			{
				CreateEmptyPane();
			}
			m_pane.FirstRow = (ushort)value;
		}
	}

	internal int MaxColumnWidth => 255;

	public int FirstVisibleColumn
	{
		get
		{
			ParseData();
			if (m_pane == null)
			{
				return 0;
			}
			return m_pane.FirstColumn;
		}
		set
		{
			ParseData();
			if (m_pane == null)
			{
				CreateEmptyPane();
			}
			m_pane.FirstColumn = (ushort)value;
		}
	}

	public IRange PrintArea => UsedRange;

	public int SelectionCount
	{
		get
		{
			ParseData();
			int num = 1;
			if (m_pane != null)
			{
				if (m_pane.VerticalSplit != 0)
				{
					num *= 2;
				}
				if (m_pane.HorizontalSplit != 0)
				{
					num *= 2;
				}
			}
			return num;
		}
	}

	public OfficeSheetView View
	{
		get
		{
			return m_view;
		}
		set
		{
			m_view = value;
		}
	}

	public int DefaultRowHeight
	{
		get
		{
			ParseData();
			return m_pageSetup.DefaultRowHeight;
		}
		set
		{
			ParseData();
			if (m_pageSetup.DefaultRowHeight == value)
			{
				return;
			}
			if (StandardHeight != m_book.StandardRowHeight)
			{
				m_pageSetup.DefaultRowHeightFlag = true;
			}
			int defaultRowHeight = m_pageSetup.DefaultRowHeight;
			if (m_iFirstRow >= 0 && m_iLastRow >= 0)
			{
				for (int i = m_iFirstRow; i <= m_iLastRow; i++)
				{
					RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, i, bCreate: false);
					if (orCreateRow != null && !orCreateRow.IsBadFontHeight && orCreateRow.Height == defaultRowHeight)
					{
						orCreateRow.Height = (ushort)value;
						orCreateRow.IsBadFontHeight = true;
					}
				}
			}
			m_pageSetup.DefaultRowHeight = value;
		}
	}

	public WorksheetNamesCollection InnerNames => m_names;

	public CellRecordCollection CellRecords
	{
		[DebuggerStepThrough]
		get
		{
			ParseData();
			return m_dicRecordsCells;
		}
	}

	public override PageSetupBaseImpl PageSetupBase
	{
		get
		{
			ParseData();
			return m_pageSetup;
		}
	}

	[CLSCompliant(false)]
	public PaneRecord Pane
	{
		get
		{
			ParseData();
			if (m_pane == null)
			{
				m_pane = (PaneRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Pane);
			}
			return m_pane;
		}
	}

	[CLSCompliant(false)]
	public List<SelectionRecord> Selections
	{
		get
		{
			ParseData();
			return m_arrSelections;
		}
	}

	public bool UseRangesCache
	{
		get
		{
			ParseData();
			return m_dicRecordsCells.UseCache;
		}
		set
		{
			ParseData();
			m_dicRecordsCells.UseCache = value;
		}
	}

	private List<BiffRecordRaw> AutoFilterRecords
	{
		get
		{
			ParseData();
			if (m_arrAutoFilter == null)
			{
				m_arrAutoFilter = new List<BiffRecordRaw>();
			}
			return m_arrAutoFilter;
		}
	}

	private List<BiffRecordRaw> DConRecords
	{
		get
		{
			ParseData();
			if (m_arrDConRecords == null)
			{
				m_arrDConRecords = new List<BiffRecordRaw>();
			}
			return m_arrDConRecords;
		}
	}

	private List<BiffRecordRaw> SortRecords
	{
		get
		{
			ParseData();
			if (m_arrSortRecords == null)
			{
				m_arrSortRecords = new List<BiffRecordRaw>();
			}
			return m_arrSortRecords;
		}
	}

	public string QuotedName
	{
		get
		{
			ParseData();
			return "'" + base.Name.Replace("'", "''") + "'";
		}
	}

	public OfficeVersion Version
	{
		get
		{
			return m_book.Version;
		}
		set
		{
			if (m_iLastRow != -1)
			{
				m_iLastRow = Math.Min(m_iLastRow, m_book.MaxRowCount);
			}
			if (m_iFirstRow != -1)
			{
				m_iFirstRow = Math.Min(m_iFirstRow, m_book.MaxRowCount);
			}
			if (m_iFirstColumn != int.MaxValue)
			{
				m_iFirstColumn = Math.Min(m_iFirstColumn, m_book.MaxColumnCount);
			}
			if (m_iLastColumn != int.MaxValue)
			{
				m_iLastColumn = Math.Min(m_iLastColumn, m_book.MaxColumnCount);
			}
			ColumnInfoRecord[] arrColumnInfo = m_arrColumnInfo;
			m_arrColumnInfo = new ColumnInfoRecord[m_book.MaxColumnCount + 2];
			Array.Copy(arrColumnInfo, 0, m_arrColumnInfo, 0, Math.Min(arrColumnInfo.Length, m_arrColumnInfo.Length));
			if (m_book.IsConverted && arrColumnInfo[^1] != null && m_rawColRecord != null)
			{
				ColumnInfoRecord columnInfoRecord = null;
				for (int i = arrColumnInfo.Length; i < m_arrColumnInfo.Length; i++)
				{
					columnInfoRecord = m_rawColRecord.Clone() as ColumnInfoRecord;
					columnInfoRecord.FirstColumn = (ushort)i;
					columnInfoRecord.LastColumn = (ushort)i;
					m_arrColumnInfo[i] = columnInfoRecord;
				}
			}
			m_dicRecordsCells.Version = value;
			if (value == OfficeVersion.Excel97to2003 && m_mergedCells != null)
			{
				m_mergedCells.SetNewDimensions(m_book.MaxRowCount, m_book.MaxColumnCount);
			}
			_ = ((WorkbookImpl)base.Workbook).DataHolder;
			InnerNames?.ConvertFullRowColumnNames(value);
			if (m_pane != null && (m_pane.FirstRow > m_book.MaxRowCount - 1 || m_pane.FirstColumn > m_book.MaxColumnCount - 1))
			{
				m_pane = null;
			}
			base.InnerShapes?.SetVersion(value);
		}
	}

	public RecordExtractor RecordExtractor => CellRecords.RecordExtractor;

	internal ItemSizeHelper RowHeightHelper
	{
		get
		{
			if (m_rowHeightHelper == null)
			{
				m_rowHeightHelper = new ItemSizeHelper(GetRowHeightInPixels);
			}
			return m_rowHeightHelper;
		}
	}

	internal bool IsVisible
	{
		get
		{
			return m_isRowHeightSet;
		}
		set
		{
			m_isRowHeightSet = value;
		}
	}

	internal bool IsZeroHeight
	{
		get
		{
			return m_isZeroHeight;
		}
		set
		{
			m_isZeroHeight = value;
		}
	}

	internal int BaseColumnWidth
	{
		get
		{
			return m_baseColumnWidth;
		}
		set
		{
			m_baseColumnWidth = value;
		}
	}

	internal bool IsThickBottom
	{
		get
		{
			return m_isThickBottom;
		}
		set
		{
			m_isThickBottom = value;
		}
	}

	internal bool IsThickTop
	{
		get
		{
			return m_isThickTop;
		}
		set
		{
			m_isThickTop = value;
		}
	}

	internal byte OutlineLevelColumn
	{
		get
		{
			return m_outlineLevelColumn;
		}
		set
		{
			m_outlineLevelColumn = value;
		}
	}

	internal byte OutlineLevelRow
	{
		get
		{
			return m_outlineLevelRow;
		}
		set
		{
			m_outlineLevelRow = value;
		}
	}

	internal bool CustomHeight
	{
		get
		{
			return m_isCustomHeight;
		}
		set
		{
			m_isCustomHeight = value;
		}
	}

	public int RowsOutlineLevel => OutlineLevelRow;

	public int ColumnsOutlineLevel => OutlineLevelColumn;

	public List<IOutlineWrapper> OutlineWrappers
	{
		get
		{
			if (m_outlineWrappers == null)
			{
				OutlineWrappers = new List<IOutlineWrapper>();
			}
			return m_outlineWrappers;
		}
		set
		{
			m_outlineWrappers = value;
		}
	}

	public bool HasMergedCells
	{
		get
		{
			if (m_mergedCells != null)
			{
				return m_mergedCells.MergeCount > 0;
			}
			return false;
		}
	}

	protected override OfficeSheetProtection DefaultProtectionOptions => OfficeSheetProtection.LockedCells | OfficeSheetProtection.UnLockedCells;

	protected override OfficeSheetProtection UnprotectedOptions => OfficeSheetProtection.Content;

	internal Dictionary<string, string> InlineStrings
	{
		get
		{
			if (m_inlineStrings == null)
			{
				m_inlineStrings = new Dictionary<string, string>();
			}
			return m_inlineStrings;
		}
	}

	internal List<BiffRecordRaw> PreserveExternalConnection
	{
		get
		{
			if (m_preserveExternalConnection == null)
			{
				m_preserveExternalConnection = new List<BiffRecordRaw>();
			}
			return m_preserveExternalConnection;
		}
	}

	internal List<Stream> PreservePivotTables
	{
		get
		{
			if (m_preservePivotTables == null)
			{
				m_preservePivotTables = new List<Stream>();
			}
			return m_preservePivotTables;
		}
	}

	internal override bool ParseDataOnDemand
	{
		get
		{
			return m_bParseDataOnDemand;
		}
		set
		{
			m_bParseDataOnDemand = value;
		}
	}

	internal Dictionary<int, List<Point>> ColumnOutlineLevels
	{
		get
		{
			if (m_columnOutlineLevels == null)
			{
				m_columnOutlineLevels = new Dictionary<int, List<Point>>();
			}
			return m_columnOutlineLevels;
		}
		set
		{
			m_columnOutlineLevels = value;
		}
	}

	internal Dictionary<int, List<Point>> RowOutlineLevels
	{
		get
		{
			if (m_rowOutlineLevels == null)
			{
				m_rowOutlineLevels = new Dictionary<int, List<Point>>();
			}
			return m_rowOutlineLevels;
		}
		set
		{
			m_rowOutlineLevels = value;
		}
	}

	public IRange this[int row, int column] => Range[row, column];

	public IRange this[int row, int column, int lastRow, int lastColumn] => Range[row, column, lastRow, lastColumn];

	public IRange this[string name] => this[name, false];

	public IRange this[string name, bool IsR1C1Notation] => Range[name, IsR1C1Notation];

	public int ActivePane
	{
		get
		{
			ParseData();
			if (m_pane == null)
			{
				return int.MinValue;
			}
			return m_pane.ActivePane;
		}
		set
		{
			ParseData();
			if (m_pane == null)
			{
				CreateEmptyPane();
			}
			m_pane.ActivePane = (ushort)value;
		}
	}

	public IRange[] Cells => UsedRange.Cells;

	internal ColumnCollection Columnss
	{
		get
		{
			if (columnCollection != null)
			{
				return columnCollection;
			}
			int num = GetAppImpl().GetFontCalc2() * 8 + GetAppImpl().GetFontCalc3();
			int num2 = (num / 8 + 1) * 8;
			double defaultWidth = 8.0 + (double)(num2 - num) * 1.0 / (double)GetAppImpl().GetFontCalc2();
			columnCollection = new ColumnCollection(this, defaultWidth);
			return columnCollection;
		}
	}

	public IRange[] Columns => UsedRange.Columns;

	public bool DisplayPageBreaks
	{
		get
		{
			ParseData();
			return m_bDisplayPageBreaks;
		}
		set
		{
			ParseData();
			if (m_bDisplayPageBreaks != value)
			{
				SetChanged();
				m_bDisplayPageBreaks = value;
			}
		}
	}

	internal AutoFitManager AutoFitManagerImpl
	{
		get
		{
			if (m_autoFitManager == null)
			{
				m_autoFitManager = new AutoFitManager();
			}
			return m_autoFitManager;
		}
		set
		{
			m_autoFitManager = value;
		}
	}

	public bool IsDisplayZeros
	{
		get
		{
			ParseData();
			return base.WindowTwo.IsDisplayZeros;
		}
		set
		{
			ParseData();
			base.WindowTwo.IsDisplayZeros = value;
		}
	}

	public bool IsGridLinesVisible
	{
		get
		{
			ParseData();
			return base.WindowTwo.IsDisplayGridlines;
		}
		set
		{
			ParseData();
			base.WindowTwo.IsDisplayGridlines = value;
		}
	}

	public bool IsRowColumnHeadersVisible
	{
		get
		{
			ParseData();
			return base.WindowTwo.IsDisplayRowColHeadings;
		}
		set
		{
			ParseData();
			base.WindowTwo.IsDisplayRowColHeadings = value;
		}
	}

	public bool IsStringsPreserved
	{
		get
		{
			ParseData();
			return m_bStringsPreserved;
		}
		set
		{
			ParseData();
			m_stringPreservedRanges.Clear();
			m_bStringsPreserved = value;
		}
	}

	public IRange[] MergedCells
	{
		get
		{
			ParseData();
			int num = ((m_mergedCells != null) ? m_mergedCells.MergeCount : 0);
			IRange[] array = ((num > 0) ? new IRange[num] : null);
			if (array != null)
			{
				List<Rectangle> mergedRegions = m_mergedCells.MergedRegions;
				for (int i = 0; i < num; i++)
				{
					Rectangle rectangle = mergedRegions[i];
					RangeImpl rangeImpl = base.AppImplementation.CreateRange(this, rectangle.X + 1, rectangle.Y + 1, rectangle.Right + 1, rectangle.Bottom + 1);
					array[i] = rangeImpl;
				}
			}
			return array;
		}
	}

	public INames Names => m_names;

	public IPageSetup PageSetup
	{
		get
		{
			ParseData();
			return m_pageSetup;
		}
	}

	public IRange PaneFirstVisible
	{
		get
		{
			ParseData();
			return base.AppImplementation.CreateRange(this, FirstVisibleColumn + 1, FirstVisibleRow + 1);
		}
		set
		{
			ParseData();
			FirstVisibleRow = value.Row - 1;
			FirstVisibleColumn = value.Column - 1;
		}
	}

	public IRange Range
	{
		[DebuggerStepThrough]
		get
		{
			return UsedRange;
		}
	}

	public IRange[] Rows => UsedRange.Rows;

	public bool IsFreezePanes => base.WindowTwo.IsFreezePanes;

	public IRange SplitCell
	{
		get
		{
			ParseData();
			return base.AppImplementation.CreateRange(this, VerticalSplit + 1, HorizontalSplit + 1);
		}
		set
		{
			ParseData();
			VerticalSplit = value.Column - 1;
			HorizontalSplit = value.Row - 1;
			base.WindowTwo.IsFreezePanes = true;
			base.WindowTwo.IsFreezePanesNoSplit = true;
		}
	}

	public double StandardHeight
	{
		get
		{
			return (double)DefaultRowHeight / 20.0;
		}
		set
		{
			if (value < 0.0)
			{
				throw new ArgumentOutOfRangeException("Standard Row Height");
			}
			DefaultRowHeight = (int)(value * 20.0);
		}
	}

	public bool StandardHeightFlag
	{
		get
		{
			ParseData();
			return m_pageSetup.DefaultRowHeightFlag;
		}
		set
		{
			ParseData();
			m_pageSetup.DefaultRowHeightFlag = value;
		}
	}

	public double StandardWidth
	{
		get
		{
			ParseData();
			return m_dStandardColWidth;
		}
		set
		{
			ParseData();
			m_dStandardColWidth = value;
		}
	}

	public OfficeSheetType Type
	{
		get
		{
			return m_sheetType;
		}
		set
		{
			m_sheetType = value;
			if (!base.IsSupported && m_sheetType == OfficeSheetType.Worksheet)
			{
				base.IsSupported = true;
			}
		}
	}

	public IRange UsedRange
	{
		get
		{
			ParseData();
			if (m_busedRangeIncludesCF && m_parseCF && !(base.Workbook as WorkbookImpl).IsWorkbookOpening && !(base.Workbook as WorkbookImpl).Saving)
			{
				ParseSheetCF();
			}
			if ((m_iFirstColumn == m_iLastColumn && m_iFirstColumn == int.MaxValue) || (m_iFirstRow == m_iLastRow && m_iFirstRow < 0))
			{
				if (m_rngUsed != null)
				{
					m_rngUsed.Dispose();
				}
				m_rngUsed = base.AppImplementation.CreateRange(this);
			}
			else
			{
				int firstRow = m_iFirstRow;
				int firstColumn = m_iFirstColumn;
				int lastRow = m_iLastRow;
				int lastColumn = m_iLastColumn;
				GetRangeCoordinates(ref firstRow, ref firstColumn, ref lastRow, ref lastColumn);
				CreateUsedRange(firstRow, firstColumn, lastRow, lastColumn);
			}
			return m_rngUsed;
		}
	}

	public IRange[] UsedCells
	{
		get
		{
			ParseData();
			List<IRange> list = new List<IRange>();
			int num = 0;
			foreach (DictionaryEntry dicRecordsCell in m_dicRecordsCells)
			{
				if (dicRecordsCell.Value != null)
				{
					ICellPositionFormat cellPositionFormat = dicRecordsCell.Value as ICellPositionFormat;
					list.Add(InnerGetCell(cellPositionFormat.Column + 1, cellPositionFormat.Row + 1));
					num++;
				}
			}
			return list.ToArray();
		}
	}

	public bool IsEmpty
	{
		get
		{
			ParseData();
			return m_iFirstRow == -1;
		}
	}

	public IMigrantRange MigrantRange
	{
		get
		{
			ParseData();
			if (m_migrantRange == null)
			{
				CreateMigrantRange();
			}
			return m_migrantRange;
		}
	}

	public bool UsedRangeIncludesFormatting
	{
		get
		{
			return m_bUsedRangeIncludesFormatting;
		}
		set
		{
			m_bUsedRangeIncludesFormatting = value;
		}
	}

	internal bool UsedRangeIncludesCF
	{
		get
		{
			return m_busedRangeIncludesCF;
		}
		set
		{
			m_busedRangeIncludesCF = value;
		}
	}

	public override bool ProtectContents
	{
		get
		{
			return (InnerProtection & OfficeSheetProtection.Content) == 0;
		}
		internal set
		{
			if (!value)
			{
				InnerProtection |= OfficeSheetProtection.Content;
			}
			else
			{
				InnerProtection &= ~OfficeSheetProtection.Content;
			}
		}
	}

	internal bool IsImporting
	{
		get
		{
			return m_bIsImporting;
		}
		set
		{
			m_bIsImporting = value;
		}
	}

	public IRange TopLeftCell
	{
		get
		{
			int topVisibleRow = base.TopVisibleRow;
			int leftVisibleColumn = base.LeftVisibleColumn;
			return this[topVisibleRow, leftVisibleColumn];
		}
		set
		{
			if (IsFreezePanes)
			{
				if (value.Row > PaneFirstVisible.Row && value.Column > PaneFirstVisible.Column)
				{
					FirstVisibleRow = value.Row - 1;
					FirstVisibleColumn = value.Column - 1;
				}
			}
			else
			{
				base.TopVisibleRow = value.Row;
				base.LeftVisibleColumn = value.Column;
			}
		}
	}

	internal override bool ContainsProtection => base.ContainsProtection;

	internal string ArchiveItemName
	{
		get
		{
			return m_archiveItemName;
		}
		set
		{
			m_archiveItemName = value;
		}
	}

	public event MissingFunctionEventHandler MissingFunction;

	public event RangeImpl.CellValueChangedEventHandler CellValueChanged;

	public event DocGen.OfficeChart.Calculate.ValueChangedEventHandler ValueChanged;

	public event ValueChangedEventHandler ColumnWidthChanged;

	public event ValueChangedEventHandler RowHeightChanged;

	internal void EnableSheetCalculations()
	{
		m_book.EnabledCalcEngine = true;
		if (CalcEngine != null)
		{
			return;
		}
		CalcEngine.ParseArgumentSeparator = base.AppImplementation.ArgumentsSeparator;
		CalcEngine.ParseDecimalSeparator = Convert.ToChar(base.AppImplementation.DecimalSeparator);
		CalcEngine = new CalcEngine(this);
		CalcEngine.UseDatesInCalculations = true;
		CalcEngine.UseNoAmpersandQuotes = true;
		CalcEngine.ArrayParser.GetArrayRecordPosition = GetArrayRecordPosition;
		lock (CalcEngine)
		{
			int sheetFamilyID = CalcEngine.CreateSheetFamilyID();
			string text = "!";
			foreach (IWorksheet worksheet in base.ParentWorkbook.Worksheets)
			{
				if ((worksheet as WorksheetImpl).CalcEngine == null)
				{
					(worksheet as WorksheetImpl).CalcEngine = new CalcEngine(worksheet);
					(worksheet as WorksheetImpl).CalcEngine.UseDatesInCalculations = true;
					(worksheet as WorksheetImpl).CalcEngine.UseNoAmpersandQuotes = true;
					(worksheet as WorksheetImpl).CalcEngine.ExcelLikeComputations = true;
				}
				if (CalcEngine.modelToSheetID != null && worksheet is WorksheetImpl && CalcEngine.modelToSheetID.ContainsKey(worksheet as WorksheetImpl))
				{
					CalcEngine.modelToSheetID.Remove(worksheet);
				}
				(worksheet as WorksheetImpl).CalcEngine.RegisterGridAsSheet(worksheet.Name, worksheet, sheetFamilyID);
				(worksheet as WorksheetImpl).CalcEngine.UnknownFunction += CalcEngine_UnknownFunction;
				(worksheet as WorksheetImpl).CalcEngine.UpdateNamedRange += UpdateNamedRange;
				(worksheet as WorksheetImpl).CalcEngine.UpdateExternalFormula += UpdateExternalFormula;
				(worksheet as WorksheetImpl).CalcEngine.QueryExternalWorksheet += GetExternWorksheet;
				(worksheet as WorksheetImpl).CalcEngine.ArrayParser.GetArrayRecordPosition = GetArrayRecordPosition;
				(worksheet as WorksheetImpl).m_hasSheetCalculation = true;
				text = text + worksheet.Name + "!";
				if (base.ParentWorkbook.Date1904)
				{
					(worksheet as WorksheetImpl).CalcEngine.UseDate1904 = true;
				}
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (IName name in base.ParentWorkbook.Names)
			{
				if (name.Scope.Length > 0 && text.IndexOf("!" + name.Scope + "!") > -1 && name.Value != null)
				{
					if (dictionary.ContainsKey((name.Scope + "!" + name.Name).ToUpper()))
					{
						dictionary.Remove((name.Scope + "!" + name.Name).ToUpper());
					}
					dictionary.Add((name.Scope + "!" + name.Name).ToUpper(), name.Value.Replace("'", ""));
				}
				else if (name.Name != null && name.Value != null)
				{
					if (dictionary.ContainsKey(name.Name.ToUpper()))
					{
						dictionary.Remove(name.Name.ToUpper());
					}
					dictionary.Add(name.Name.ToUpper(), name.Value.Replace("'", ""));
				}
			}
			Dictionary<object, object> dictionary2 = new Dictionary<object, object>();
			if (dictionary != null)
			{
				foreach (string key in dictionary.Keys)
				{
					dictionary2.Add(key.ToUpper(CultureInfo.InvariantCulture), dictionary[key]);
				}
			}
			foreach (IWorksheet worksheet2 in base.ParentWorkbook.Worksheets)
			{
				(worksheet2 as WorksheetImpl).CalcEngine.NamedRanges = dictionary2;
			}
		}
	}

	private void CalcEngine_UnknownFunction(object sender, UnknownFunctionEventArgs args)
	{
		m_bIsUnsupportedFormula = true;
		if (this.MissingFunction != null && CalcEngine != null)
		{
			MissingFunctionEventArgs missingFunctionEventArgs = new MissingFunctionEventArgs();
			missingFunctionEventArgs.MissingFunctionName = args.MissingFunctionName;
			missingFunctionEventArgs.CellLocation = args.CellLocation;
			this.MissingFunction(this, missingFunctionEventArgs);
		}
	}

	private void UpdateNamedRange(object sender, UpdateNamedRangeEventArgs args)
	{
		IRange intersect = null;
		string text = args.Name;
		if (TryGetIntersectRange(text, out intersect) && intersect != null)
		{
			args.Address = intersect.AddressGlobal;
			args.IsFormulaUpdated = true;
			return;
		}
		if (TryGetExternalIntersectRange(text, out var intersect2) && intersect2 != null)
		{
			args.Address = intersect2;
			args.IsFormulaUpdated = true;
			return;
		}
		int num = text.LastIndexOf('!');
		if (num > 0)
		{
			string text2 = text.Substring(0, num);
			string text3 = text.Substring(num + 1);
			if (text2[0] == '\'' && text2[text2.Length - 1] == '\'')
			{
				text2 = text2.Substring(1, text2.Length - 2);
			}
			WorkbookImpl workbookImpl = base.Workbook as WorkbookImpl;
			string text4 = (workbookImpl.IsCreated ? workbookImpl.GetWorkbookName(workbookImpl) : workbookImpl.FullFileName);
			if (text4 == text2 || (workbookImpl.FullFileName != null && text4 == workbookImpl.GetFilePath(workbookImpl.FullFileName) + text2))
			{
				text = text3;
			}
		}
		if ((Names as WorksheetNamesCollection).Contains(text, isFormulaNamedrange: true) && Names[text].RefersToRange != null)
		{
			args.Address = Names[text].RefersToRange.AddressGlobal;
			args.IsFormulaUpdated = true;
		}
		else if ((base.Workbook.Names as WorkbookNamesCollection).Contains(text, isFormulaNamedrange: true) && base.Workbook.Names[text].RefersToRange != null)
		{
			args.Address = base.Workbook.Names[text].RefersToRange.AddressGlobal;
			args.IsFormulaUpdated = true;
		}
		else
		{
			args.Address = text;
		}
	}

	private void UpdateExternalFormula(object sender, UpdateExternalFormulaEventArgs args)
	{
		WorkbookImpl workbookImpl = base.Workbook as WorkbookImpl;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string text = string.Empty;
		for (int i = 0; i < workbookImpl.ExternWorkbooks.Count; i++)
		{
			ExternWorkbookImpl externWorkbookImpl = workbookImpl.ExternWorkbooks[i];
			if (!externWorkbookImpl.IsInternalReference)
			{
				string key = externWorkbookImpl.URL.Replace("\\", "/");
				dictionary.Add(key, "[" + externWorkbookImpl.Index + "]");
			}
		}
		string formula = args.formula;
		formula = formula.Replace("[", "");
		formula = formula.Replace("]", "");
		formula = formula.Replace("\\", "/");
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			if (formula.IndexOf(item.Key, StringComparison.OrdinalIgnoreCase) > -1)
			{
				formula = (args.formula = Regex.Replace(formula, Regex.Escape(item.Key), item.Value, RegexOptions.IgnoreCase));
				args.parsedFormula = args.formula;
				args.IsFormulaUpdated = true;
			}
		}
		formula = args.formula;
		int num = formula.IndexOf('!');
		int num2 = -1;
		int num3 = -1;
		string text2 = string.Empty;
		int result = -1;
		string identifier = formula.Substring(num + 1);
		bool flag = IsCellRange(workbookImpl, identifier);
		ExternWorkbookImpl externWorkbookImpl2 = null;
		if ((formula.Contains("[") || formula.Contains("'[")) && formula.Contains("]"))
		{
			num2 = formula.IndexOf("[");
			if (num2 > 0 && formula[num2 - 1] == '\'')
			{
				text = formula.Substring(0, num2 - 1);
			}
			for (num2++; formula[num2] != ']'; num2++)
			{
				text2 += formula[num2];
			}
			if (int.TryParse(text2, out result) && result < workbookImpl.ExternWorkbooks.Count)
			{
				externWorkbookImpl2 = workbookImpl.ExternWorkbooks[result];
			}
			num3 = num2;
		}
		if (externWorkbookImpl2 != null)
		{
			string text3 = string.Empty;
			int num4 = -1;
			if (num3 != -1 && num > num3)
			{
				text3 = formula.Substring(num3 + 1, num - num3 - 1);
				if (text3.EndsWith("'"))
				{
					text3 = text3.Substring(0, text3.Length - 1);
				}
			}
			num4 = GetExternSheetIndex(text3, externWorkbookImpl2);
			if ((TryGetIdentifier(externWorkbookImpl2, ref identifier, ref num4) || flag) && num4 != -1)
			{
				args.parsedFormula = text + "[" + externWorkbookImpl2.Index + "]!" + num4 + "!" + identifier;
				args.IsFormulaUpdated = true;
			}
		}
		else
		{
			args.parsedFormula = args.formula;
		}
	}

	internal int GetArrayRecordPosition(int row, int col, ref int height, ref int width, ICalcData calcData)
	{
		if (!(calcData is WorksheetImpl worksheetImpl))
		{
			return -1;
		}
		ArrayRecord arrayRecord = worksheetImpl.CellRecords.GetArrayRecord(row, col);
		int num = height;
		int num2 = width;
		if (row - (arrayRecord.FirstRow + 1) >= num || col - (arrayRecord.FirstColumn + 1) >= num2)
		{
			return -1;
		}
		int num3 = 0;
		if (height > arrayRecord.LastRow - arrayRecord.FirstRow + 1)
		{
			height = arrayRecord.LastRow - arrayRecord.FirstRow + 1;
		}
		if (width > arrayRecord.LastColumn - arrayRecord.FirstColumn + 1)
		{
			width = arrayRecord.LastColumn - arrayRecord.FirstColumn + 1;
		}
		for (int i = arrayRecord.FirstRow + 1; i <= arrayRecord.LastRow + 1; i++)
		{
			for (int j = arrayRecord.FirstColumn + 1; j <= arrayRecord.LastColumn + 1; j++)
			{
				if (i - (arrayRecord.FirstRow + 1) < num && j - (arrayRecord.FirstColumn + 1) < num2)
				{
					if (i == row && j == col)
					{
						return num3;
					}
					num3++;
				}
			}
		}
		return num3;
	}

	private void GetExternWorksheet(object sender, QueryExternalWorksheetEventArgs args)
	{
		WorkbookImpl workbookImpl = base.Workbook as WorkbookImpl;
		string text = string.Empty;
		string text2 = string.Empty;
		int result = 0;
		int result2 = 0;
		string formula = args.formula;
		int i = 0;
		if (formula[i] == '[')
		{
			for (i++; i < formula.Length && formula[i] != ']'; i++)
			{
				text2 += formula[i];
			}
			if (i < formula.Length)
			{
				i++;
			}
		}
		if (formula[i] == '!')
		{
			for (i++; i < formula.Length && formula[i] != '!'; i++)
			{
				text += formula[i];
			}
			if (i < formula.Length)
			{
				i++;
			}
		}
		if (text2 != string.Empty && int.TryParse(text2, out result) && result >= 0 && result < workbookImpl.ExternWorkbooks.Count)
		{
			ExternWorkbookImpl externWorkbookImpl = workbookImpl.ExternWorkbooks[result];
			if (text != string.Empty && int.TryParse(text, out result2) && result2 >= 0 && result2 < externWorkbookImpl.Worksheets.Count)
			{
				args.worksheet = externWorkbookImpl.Worksheets[result2];
				args.worksheetName = externWorkbookImpl.Worksheets[result2].Name;
				args.IsWorksheetUpdated = true;
			}
		}
	}

	private bool TryGetExternRangeAddress(WorkbookImpl workbook, ref string formula)
	{
		string text = string.Empty;
		string text2 = formula;
		int num = text2.IndexOf('!');
		int num2 = -1;
		int num3 = -1;
		string text3 = string.Empty;
		int result = -1;
		string identifier = text2.Substring(num + 1);
		bool flag = IsCellRange(workbook, identifier);
		ExternWorkbookImpl externWorkbookImpl = null;
		if (text2.Contains("'[") && text2.Contains("]"))
		{
			num2 = text2.IndexOf("[");
			if (num2 > 0 && text2[num2 - 1] == '\'')
			{
				text = text2.Substring(0, num2 - 1);
			}
			for (num2++; text2[num2] != ']'; num2++)
			{
				text3 += text2[num2];
			}
			if (int.TryParse(text3, out result) && result < workbook.ExternWorkbooks.Count)
			{
				externWorkbookImpl = workbook.ExternWorkbooks[result];
			}
			num3 = num2;
		}
		bool result2 = false;
		if (externWorkbookImpl != null)
		{
			string text4 = string.Empty;
			int num4 = -1;
			if (num3 != -1 && num > num3)
			{
				text4 = text2.Substring(num3 + 1, num - num3 - 1);
				if (text4.EndsWith("'") && text2.LastIndexOf('\'') + 1 == num)
				{
					text4 = text4.Substring(0, text4.Length - 1);
				}
			}
			num4 = GetExternSheetIndex(text4, externWorkbookImpl);
			if ((TryGetIdentifier(externWorkbookImpl, ref identifier, ref num4) || flag) && num4 != -1)
			{
				text2 = text + "[" + externWorkbookImpl.Index + "]!" + num4 + "!" + identifier;
				formula = text2;
				result2 = true;
			}
		}
		return result2;
	}

	private bool TryGetIdentifier(ExternWorkbookImpl externBook, ref string identifier, ref int sheetIndex)
	{
		bool flag = false;
		if (identifier != string.Empty)
		{
			foreach (ExternNameImpl externName in externBook.ExternNames)
			{
				if (string.Equals(externName.Name.Replace("[", "").Replace("]", "").Replace(" ", ""), identifier, StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
					string text = externName.RefersTo;
					if (text[0] == '=')
					{
						text = text.Substring(1);
					}
					int num = text.LastIndexOf('!');
					identifier = text.Substring(num + 1);
					if (identifier.Contains("$"))
					{
						identifier = identifier.Replace("$", "");
					}
					string text2 = text.Substring(0, num);
					if (text2[0] == '\'' && text2[text2.Length - 1] == '\'')
					{
						text2 = text2.Substring(1, text2.Length - 2);
					}
					sheetIndex = GetExternSheetIndex(text2, externBook);
					break;
				}
			}
		}
		return sheetIndex != -1 && flag;
	}

	private int GetExternSheetIndex(string sheetName, ExternWorkbookImpl externBook)
	{
		int result = -1;
		if (sheetName != string.Empty)
		{
			foreach (KeyValuePair<int, ExternWorksheetImpl> worksheet in externBook.Worksheets)
			{
				if (string.Equals(worksheet.Value.Name, sheetName, StringComparison.OrdinalIgnoreCase))
				{
					result = worksheet.Value.Index;
					break;
				}
			}
		}
		return result;
	}

	private bool IsCellRange(WorkbookImpl workbook, string idendifier)
	{
		string strColumn = string.Empty;
		if (!FormulaUtil.IsCell(idendifier, bR1C1: false, out var strRow, out var strColumn2) && !FormulaUtil.IsCell3D(idendifier, bR1C1: false, out var strSheetName, out strRow, out strColumn2) && !workbook.FormulaUtil.IsCellRange(idendifier, bR1C1: false, out strRow, out strColumn2, out var strRow2, out strColumn))
		{
			return workbook.FormulaUtil.IsCellRange3D(idendifier, bR1C1: false, out strSheetName, out strRow, out strColumn2, out strRow2, out strColumn);
		}
		return true;
	}

	internal void DisableSheetCalculations()
	{
		m_book.EnabledCalcEngine = false;
		if (CalcEngine == null || base.ParentWorkbook == null || base.ParentWorkbook.Worksheets == null)
		{
			return;
		}
		foreach (IWorksheet worksheet in base.ParentWorkbook.Worksheets)
		{
			if ((worksheet as WorksheetImpl).CalcEngine != null)
			{
				(worksheet as WorksheetImpl).CalcEngine.UnknownFunction -= CalcEngine_UnknownFunction;
				(worksheet as WorksheetImpl).CalcEngine.UpdateNamedRange -= UpdateNamedRange;
				(worksheet as WorksheetImpl).CalcEngine.UpdateExternalFormula -= UpdateExternalFormula;
				(worksheet as WorksheetImpl).CalcEngine.QueryExternalWorksheet -= GetExternWorksheet;
				ArrayParser arrayParser = (worksheet as WorksheetImpl).CalcEngine.ArrayParser;
				arrayParser.GetArrayRecordPosition = (ArrayParser.ArrayDelegate)Delegate.Remove(arrayParser.GetArrayRecordPosition, new ArrayParser.ArrayDelegate(GetArrayRecordPosition));
				(worksheet as WorksheetImpl).CalcEngine.Dispose();
			}
			(worksheet as WorksheetImpl).CalcEngine = null;
			(worksheet as WorksheetImpl).m_hasSheetCalculation = false;
		}
		if (CalcEngine.modelToSheetID == null || CalcEngine.modelToSheetID.Count <= 0)
		{
			return;
		}
		List<object> list = new List<object>();
		foreach (object key in CalcEngine.modelToSheetID.Keys)
		{
			list.Add(key);
		}
		for (int i = 0; i < list.Count; i++)
		{
			object obj = list[i];
			if (obj is ExternWorksheetImpl)
			{
				int num = (int)CalcEngine.modelToSheetID[obj];
				if (CalcEngine.sheetFamiliesList != null && CalcEngine.sheetFamiliesList.Count > 0)
				{
					CalcEngine.sheetFamiliesList[num] = null;
					CalcEngine.sheetFamiliesList.Remove(num);
				}
				CalcEngine.modelToSheetID.Remove(obj);
			}
		}
	}

	internal void OnValueChanged(int row, int col, string value)
	{
		if (this.ValueChanged != null)
		{
			DocGen.OfficeChart.Calculate.ValueChangedEventArgs e = new DocGen.OfficeChart.Calculate.ValueChangedEventArgs(row, col, value);
			this.ValueChanged(this, e);
		}
	}

	public object GetValueRowCol(int row, int col)
	{
		IRange range = this[row, col];
		if (range.HasFormula)
		{
			return range.Formula;
		}
		return range.Value;
	}

	public void SetValueRowCol(object value, int row, int col)
	{
		if (value != null)
		{
			SetValue(row, col, value.ToString());
		}
	}

	public void WireParentObject()
	{
	}

	static WorksheetImpl()
	{
		s_arrAutofilterRecord = new TBIFFRecord[3]
		{
			TBIFFRecord.AutoFilter,
			TBIFFRecord.AutoFilterInfo,
			TBIFFRecord.FilterMode
		};
	}

	public WorksheetImpl(IApplication application, object parent)
		: base(application, parent)
	{
	}

	protected override void InitializeCollections()
	{
		base.InitializeCollections();
		m_nameIndexChanged = OnNameIndexChanged;
		m_dicRecordsCells = new CellRecordCollection(base.Application, this);
		m_pageSetup = new PageSetupImpl(base.Application, this);
		if (base.Application.DefaultVersion != 0)
		{
			m_pageSetup.DefaultRowHeight = (int)base.Application.StandardHeight * 20;
		}
		m_names = new WorksheetNamesCollection(base.Application, this);
		m_arrColumnInfo = new ColumnInfoRecord[m_book.MaxColumnCount + 2];
		m_bOptimizeImport = base.Application.OptimizeImport;
		base.Index = m_book.Worksheets.Count;
		m_arrSelections = new List<SelectionRecord>();
		StandardWidth = base.Application.StandardWidth;
		StandardHeight = base.Application.StandardHeight;
		StandardHeightFlag = base.Application.StandardHeightFlag;
		AttachEvents();
	}

	protected void ClearAll()
	{
		ClearAll(OfficeWorksheetCopyFlags.CopyAll);
	}

	protected override void ClearAll(OfficeWorksheetCopyFlags flags)
	{
		m_dicRecordsCells.Clear();
		m_arrSelections.Clear();
		if ((flags & OfficeWorksheetCopyFlags.CopyNames) != 0)
		{
			m_names.Clear();
		}
		base.ClearAll(flags);
	}

	protected void CopyNames(WorksheetImpl basedOn, Dictionary<string, string> hashNewSheetNames, Dictionary<int, int> hashNewNameIndexes, Dictionary<int, int> hashExternSheetIndexes)
	{
		if (basedOn == null)
		{
			throw new ArgumentNullException("basedOn");
		}
		for (int num = m_names.Count - 1; num >= 0; num--)
		{
			m_names[num].Delete();
		}
		if (hashNewSheetNames == null)
		{
			hashNewSheetNames = new Dictionary<string, string>();
			hashNewSheetNames.Add(base.Name, basedOn.Name);
		}
		m_names.FillFrom(basedOn.m_names, hashNewSheetNames, hashNewNameIndexes, ExcelNamesMergeOptions.MakeLocal, hashExternSheetIndexes);
		WorkbookNamesCollection workbookNamesCollection = basedOn.Workbook.Names as WorkbookNamesCollection;
		WorkbookNamesCollection workbookNamesCollection2 = base.Workbook.Names as WorkbookNamesCollection;
		int i = 0;
		for (int count = workbookNamesCollection.Count; i < count; i++)
		{
			NameImpl nameImpl = (NameImpl)workbookNamesCollection.InnerList[i];
			if (nameImpl.IsLocal)
			{
				continue;
			}
			IRange refersToRange = nameImpl.RefersToRange;
			int index = nameImpl.Index;
			if (refersToRange == null)
			{
				if (!workbookNamesCollection2.Contains(nameImpl.Name))
				{
					NameRecord name = (NameRecord)nameImpl.Record.Clone();
					WorksheetNamesCollection.UpdateReferenceIndexes(name, nameImpl.Workbook, hashNewSheetNames, hashExternSheetIndexes, m_book);
					IName name2 = workbookNamesCollection2.Add(name);
					hashNewNameIndexes[index] = (name2 as NameImpl).Index;
				}
			}
			else
			{
				if (refersToRange.Worksheet != basedOn)
				{
					continue;
				}
				try
				{
					IName name2 = workbookNamesCollection2.AddCopy(nameImpl, this, hashExternSheetIndexes, hashNewSheetNames);
					hashNewNameIndexes[index] = (name2 as NameImpl).Index;
				}
				catch (Exception)
				{
					if (!workbookNamesCollection2.Contains(nameImpl.Name))
					{
						NameRecord name3 = (NameRecord)nameImpl.Record.Clone();
						WorksheetNamesCollection.UpdateReferenceIndexes(name3, nameImpl.Workbook, hashNewSheetNames, hashExternSheetIndexes, m_book);
						IName name2 = workbookNamesCollection2.Add(name3);
						hashNewNameIndexes[index] = (name2 as NameImpl).Index;
					}
				}
			}
		}
		foreach (int key in basedOn.GetUsedNames().Keys)
		{
			if (!hashNewNameIndexes.ContainsKey(key))
			{
				NameImpl nameImpl = (NameImpl)workbookNamesCollection[key];
				IName name2 = workbookNamesCollection2.AddCopy(nameImpl, this, hashExternSheetIndexes, hashNewSheetNames);
				hashNewNameIndexes[key] = (name2 as NameImpl).Index;
			}
		}
	}

	private Dictionary<int, object> GetUsedNames()
	{
		ArrayListEx rows = m_dicRecordsCells.Table.Rows;
		Dictionary<int, object> result = new Dictionary<int, object>();
		for (int i = m_iFirstRow; i <= m_iLastRow; i++)
		{
			rows[i]?.GetUsedNames(result);
		}
		return result;
	}

	protected void CopyRowHeight(WorksheetImpl sourceSheet, Dictionary<int, int> hashExtFormatIndexes)
	{
		if (sourceSheet == null)
		{
			throw new ArgumentNullException("sourceSheet");
		}
	}

	protected void CopyConditionalFormats(WorksheetImpl sourceSheet)
	{
		if (sourceSheet == null)
		{
			throw new ArgumentNullException("sourceSheet");
		}
		sourceSheet.ParseSheetCF();
		ParseSheetCF();
	}

	protected void CopyAutoFilters(WorksheetImpl sourceSheet)
	{
		if (sourceSheet == null)
		{
			throw new ArgumentNullException("sourceSheet");
		}
		List<BiffRecordRaw> arrAutoFilter = sourceSheet.m_arrAutoFilter;
		if (arrAutoFilter != null)
		{
			List<BiffRecordRaw> autoFilterRecords = AutoFilterRecords;
			int i = 0;
			for (int count = arrAutoFilter.Count; i < count; i++)
			{
				autoFilterRecords.Add((BiffRecordRaw)CloneUtils.CloneCloneable(arrAutoFilter[i]));
			}
		}
	}

	protected void CopyColumnWidth(WorksheetImpl sourceSheet, Dictionary<int, int> hashExtFormatIndexes)
	{
		if (sourceSheet == null)
		{
			throw new ArgumentNullException("sourceSheet");
		}
		ColumnInfoRecord[] array = CloneUtils.CloneArray(sourceSheet.m_arrColumnInfo);
		Array.Copy(length: Math.Min(array.Length, m_arrColumnInfo.Length), sourceArray: array, destinationArray: m_arrColumnInfo);
		UpdateIndexes(m_arrColumnInfo, sourceSheet, hashExtFormatIndexes);
		if (hashExtFormatIndexes == null)
		{
			return;
		}
		int defaultXFIndex = sourceSheet.ParentWorkbook.DefaultXFIndex;
		if (!hashExtFormatIndexes.TryGetValue(defaultXFIndex, out var value))
		{
			return;
		}
		List<int> arrIsDefaultColumnWidth = null;
		if (value != defaultXFIndex)
		{
			arrIsDefaultColumnWidth = CreateColumnsOnUpdate(m_arrColumnInfo, value);
		}
		double num = double.NaN;
		int num2 = -1;
		int startIndex = 0;
		int i = 1;
		for (int num3 = m_arrColumnInfo.Length; i < num3; i++)
		{
			ColumnInfoRecord columnInfoRecord = m_arrColumnInfo[i];
			if (columnInfoRecord == null)
			{
				continue;
			}
			if (IsDefaultColumnWidth(arrIsDefaultColumnWidth, ref startIndex, i))
			{
				if (num2 < 0)
				{
					_ = columnInfoRecord.ColumnWidth;
					int columnWidthInPixels = sourceSheet.GetColumnWidthInPixels(i);
					SetColumnWidthInPixels(i, columnWidthInPixels);
					num2 = columnInfoRecord.ColumnWidth;
				}
				else
				{
					columnInfoRecord.ColumnWidth = (ushort)num2;
				}
			}
			else if (double.IsNaN(num))
			{
				int columnWidth = columnInfoRecord.ColumnWidth;
				int columnWidthInPixels2 = sourceSheet.GetColumnWidthInPixels(i);
				SetColumnWidthInPixels(i, columnWidthInPixels2);
				num = (double)(int)columnInfoRecord.ColumnWidth / (double)columnWidth;
			}
			else
			{
				columnInfoRecord.ColumnWidth = (ushort)((double)(int)columnInfoRecord.ColumnWidth * num);
			}
		}
	}

	private bool IsDefaultColumnWidth(List<int> arrIsDefaultColumnWidth, ref int startIndex, int columnIndex)
	{
		if (arrIsDefaultColumnWidth == null)
		{
			return false;
		}
		int count = arrIsDefaultColumnWidth.Count;
		if (count == 0)
		{
			return false;
		}
		if (startIndex >= count)
		{
			return false;
		}
		int num;
		for (num = arrIsDefaultColumnWidth[startIndex]; num < columnIndex; num = arrIsDefaultColumnWidth[startIndex])
		{
			startIndex++;
			if (startIndex >= count)
			{
				return false;
			}
		}
		if (num == columnIndex)
		{
			return true;
		}
		return false;
	}

	private void UpdateIndexes(ICollection collection, WorksheetImpl sourceSheet, Dictionary<int, int> hashExtFormatIndexes)
	{
		UpdateIndexes(collection, sourceSheet, hashExtFormatIndexes, bUpdateDefault: true);
	}

	private void UpdateIndexes(ICollection collection, WorksheetImpl sourceSheet, Dictionary<int, int> hashExtFormatIndexes, bool bUpdateDefault)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (sourceSheet.Workbook == base.Workbook)
		{
			return;
		}
		_ = sourceSheet.ParentWorkbook;
		int defaultXFIndex = m_book.DefaultXFIndex;
		foreach (IOutline item in collection)
		{
			if (item == null)
			{
				continue;
			}
			int extendedFormatIndex = item.ExtendedFormatIndex;
			if (hashExtFormatIndexes.ContainsKey(extendedFormatIndex))
			{
				extendedFormatIndex = hashExtFormatIndexes[extendedFormatIndex];
				if (bUpdateDefault || extendedFormatIndex == defaultXFIndex)
				{
					item.ExtendedFormatIndex = (ushort)extendedFormatIndex;
				}
			}
		}
	}

	private void UpdateOutlineIndexes(ICollection collection, int[] extFormatIndexes)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		foreach (IOutline item in collection)
		{
			if (item != null)
			{
				int extendedFormatIndex = item.ExtendedFormatIndex;
				extendedFormatIndex = extFormatIndexes[extendedFormatIndex];
				item.ExtendedFormatIndex = (ushort)extendedFormatIndex;
			}
		}
	}

	private List<int> CreateColumnsOnUpdate(ColumnInfoRecord[] columns, int iXFIndex)
	{
		if (columns == null)
		{
			throw new ArgumentNullException("columns");
		}
		List<int> list = new List<int>();
		for (int i = 1; i <= m_book.MaxColumnCount; i++)
		{
			if (columns[i] == null)
			{
				ColumnInfoRecord columnInfoRecord = (ColumnInfoRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ColumnInfo);
				ushort firstColumn = (columnInfoRecord.LastColumn = (ushort)(i - 1));
				columnInfoRecord.FirstColumn = firstColumn;
				columnInfoRecord.ExtendedFormatIndex = (ushort)iXFIndex;
				columns[i] = columnInfoRecord;
				list.Add(i);
			}
		}
		return list;
	}

	protected void CopyMerges(WorksheetImpl sourceSheet)
	{
		if (sourceSheet == null)
		{
			throw new ArgumentNullException("sourceSheet");
		}
		MergeCellsImpl mergeCells = sourceSheet.MergeCells;
		if (mergeCells != null)
		{
			m_mergedCells = (MergeCellsImpl)CloneUtils.CloneCloneable(mergeCells, this);
		}
	}

	protected void AttachEvents()
	{
		if (m_book.Styles.Contains("Normal") || !m_book.IsWorkbookOpening)
		{
			(m_book.Styles["Normal"].Font as FontWrapper).AfterChangeEvent += NormalFont_OnAfterChange;
		}
	}

	protected void DetachEvents()
	{
		if (m_book != null && m_book.Styles != null && m_book.Styles.Contains("Normal"))
		{
			(m_book.Styles["Normal"].Font as FontWrapper).AfterChangeEvent -= NormalFont_OnAfterChange;
		}
	}

	protected override void OnDispose()
	{
		if (!m_bIsDisposed)
		{
			DetachEvents();
			m_arrColumnInfo = null;
			m_arrDConRecords = null;
			m_arrNotes = null;
			m_arrNotesByCellIndex = null;
			m_arrRecords = null;
			m_arrSortRecords = null;
			if (columnCollection != null)
			{
				columnCollection.Clear();
				columnCollection = null;
			}
			if (m_arrAutoFilter != null)
			{
				m_arrAutoFilter.Clear();
				m_arrAutoFilter = null;
			}
			if (m_arrSelections != null)
			{
				m_arrSelections.Clear();
				m_arrSelections = null;
			}
			if (m_autoFitManager != null)
			{
				m_autoFitManager.Dispose();
			}
			m_bof = null;
			if (m_dataHolder != null && m_book == null)
			{
				m_dataHolder.Dispose();
				m_dataHolder = null;
			}
			if (m_dicRecordsCells != null)
			{
				m_dicRecordsCells.Dispose();
				m_dicRecordsCells = null;
			}
			m_dictCFExRecords = null;
			m_dictCondFMT = null;
			if (m_inlineStrings != null)
			{
				m_inlineStrings.Clear();
				m_inlineStrings = null;
			}
			if (m_mergedCells != null)
			{
				m_mergedCells.Dispose();
				m_mergedCells = null;
			}
			m_migrantRange = null;
			m_nameIndexChanged = null;
			m_pane = null;
			if (m_preservePivotTables != null)
			{
				m_preservePivotTables.Clear();
			}
			m_rawColRecord = null;
			m_rowHeightHelper = null;
			if (m_tableRecords != null)
			{
				m_tableRecords.Clear();
			}
			m_worksheetSlicer = null;
			if (m_book != null)
			{
				m_book.EnabledCalcEngine = false;
			}
			if (m_dicRecordsCells != null)
			{
				m_dicRecordsCells.Dispose();
				m_dicRecordsCells = null;
			}
			if (m_pageSetup != null)
			{
				m_pageSetup.Dispose();
			}
			this.RowHeightChanged = null;
			this.ColumnWidthChanged = null;
			GC.SuppressFinalize(this);
		}
	}

	protected void CopyPageSetup(WorksheetImpl sourceSheet)
	{
		if (sourceSheet == null)
		{
			throw new ArgumentNullException("sourceSheet");
		}
		m_pageSetup = sourceSheet.m_pageSetup.Clone(this);
	}

	protected int ImportExtendedFormat(int iXFIndex, WorkbookImpl basedOn, Dictionary<int, int> hashExtFormatIndexes)
	{
		return m_book.InnerExtFormats.Import(basedOn.InnerExtFormats[iXFIndex], hashExtFormatIndexes);
	}

	protected internal override void UpdateStyleIndexes(int[] styleIndexes)
	{
		UpdateOutlineIndexes(m_arrColumnInfo, styleIndexes);
		m_dicRecordsCells.UpdateExtendedFormatIndex(styleIndexes);
	}

	public IInternalWorksheet GetClonedObject(Dictionary<string, string> hashNewNames, WorkbookImpl book)
	{
		string text = base.Name;
		if (hashNewNames != null && hashNewNames.TryGetValue(text, out var value))
		{
			text = value;
		}
		return book.Worksheets[text] as IInternalWorksheet;
	}

	public void ParseCFFromExcel2007(FileDataHolder dataHolder)
	{
		if (dataHolder != null)
		{
			List<DxfImpl> list = dataHolder.ParseDxfsCollection();
			WorksheetDataHolder worksheetDataHolder = null;
			if (list != null)
			{
				worksheetDataHolder = base.DataHolder;
			}
			worksheetDataHolder?.ParseConditionalFormatting(list, this);
		}
	}

	public void ParseSheetCF()
	{
		if (Version != 0)
		{
			m_book.AppImplementation.IsFormulaParsed = false;
			FileDataHolder dataHolder = m_book.DataHolder;
			ParseCFFromExcel2007(dataHolder);
			m_book.AppImplementation.IsFormulaParsed = true;
		}
	}

	public override void UpdateExtendedFormatIndex(Dictionary<int, int> dictFormats)
	{
		ParseData();
		base.UpdateExtendedFormatIndex(dictFormats);
		m_dicRecordsCells.UpdateExtendedFormatIndex(dictFormats);
		UpdateOutlineAfterXFRemove(m_arrColumnInfo, dictFormats);
	}

	public void UpdateExtendedFormatIndex(int maxCount)
	{
		ParseData();
		if (maxCount <= 0)
		{
			throw new ArgumentOutOfRangeException("maxCount");
		}
		m_dicRecordsCells.UpdateExtendedFormatIndex(maxCount);
		int defaultXFIndex = m_book.DefaultXFIndex;
		int i = 0;
		for (int num = m_arrColumnInfo.Length; i < num; i++)
		{
			ColumnInfoRecord columnInfoRecord = m_arrColumnInfo[i];
			if (columnInfoRecord != null && columnInfoRecord.ExtendedFormatIndex >= maxCount)
			{
				columnInfoRecord.ExtendedFormatIndex = (ushort)defaultXFIndex;
			}
		}
	}

	public RangeRichTextString CreateLabelSSTRTFString(long cellIndex)
	{
		ParseData();
		RangeRichTextString rangeRichTextString = null;
		IRange range = m_dicRecordsCells.GetRange(cellIndex);
		if (range != null)
		{
			return (RangeRichTextString)range.RichText;
		}
		return new RangeRichTextString(base.Application, this, cellIndex);
	}

	public IRange[] Find(IRange range, byte findValue, bool bIsError, bool bIsFindFirst)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		ParseData();
		List<long> arrIndexes = m_dicRecordsCells.Find(range, findValue, bIsError, bIsFindFirst);
		return ConvertCellListIntoRange(arrIndexes);
	}

	public IRange[] Find(IRange range, double findValue, OfficeFindType flags, bool bIsFindFirst)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		ParseData();
		List<long> arrIndexes = m_dicRecordsCells.Find(range, findValue, flags, bIsFindFirst);
		return ConvertCellListIntoRange(arrIndexes);
	}

	public IRange[] Find(IRange range, string findValue, OfficeFindType flags, bool bIsFindFirst)
	{
		return Find(range, findValue, flags, OfficeFindOptions.None, bIsFindFirst);
	}

	public IRange[] Find(IRange range, string findValue, OfficeFindType flags, OfficeFindOptions findOptions, bool bIsFindFirst)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		if (findValue == null || findValue.Length == 0)
		{
			return null;
		}
		ParseData();
		List<long> arrIndexes = m_dicRecordsCells.Find(range, findValue, flags, findOptions, bIsFindFirst);
		return ConvertCellListIntoRange(arrIndexes);
	}

	public void MoveRange(IRange destination, IRange source, OfficeCopyRangeOptions options, bool bUpdateRowRecords)
	{
		MoveRange(destination, source, options, bUpdateRowRecords, null);
	}

	private void MoveRange(IRange destination, IRange source, OfficeCopyRangeOptions options, bool bUpdateRowRecords, IOperation beforeMove)
	{
		ParseData();
		if (destination != source)
		{
			if (!CanMove(ref destination, source))
			{
				throw new InvalidRangeException();
			}
			WorksheetImpl worksheetImpl = (WorksheetImpl)destination.Worksheet;
			WorksheetImpl worksheetImpl2 = (WorksheetImpl)source.Worksheet;
			beforeMove?.Do();
			int iSourceIndex = m_book.AddSheetReference(worksheetImpl2);
			int iDestIndex = m_book.AddSheetReference(worksheetImpl);
			m_book.AddSheetReference(worksheetImpl2);
			Rectangle rectangle = Rectangle.FromLTRB(source.Column - 1, source.Row - 1, source.LastColumn - 1, source.LastRow - 1);
			Rectangle rectangle2 = Rectangle.FromLTRB(destination.Column - 1, destination.Row - 1, destination.LastColumn - 1, destination.LastRow - 1);
			_ = destination.Row;
			_ = source.Row;
			_ = destination.Column;
			_ = source.Column;
			bool num = (options & OfficeCopyRangeOptions.UpdateFormulas) != 0;
			RangeImpl rangeImpl = (RangeImpl)destination;
			int iMaxRow = 0;
			int iMaxColumn = 0;
			RecordTable recordTable = CacheAndRemoveFromParent(source, destination, ref iMaxRow, ref iMaxColumn, worksheetImpl2.m_dicRecordsCells);
			if ((options & OfficeCopyRangeOptions.UpdateMerges) != 0)
			{
				CopyRangeMerges(destination, source, bDeleteSource: true);
			}
			worksheetImpl2.PartialClearRange(rectangle);
			worksheetImpl.CellRecords.ClearRange(rectangle2);
			CopyCacheInto(recordTable, worksheetImpl.m_dicRecordsCells.Table, bUpdateRowRecords);
			recordTable?.Dispose();
			WorksheetHelper.AccessRow(worksheetImpl, rangeImpl.FirstRow);
			WorksheetHelper.AccessColumn(worksheetImpl, rangeImpl.FirstColumn);
			if (iMaxColumn > rangeImpl.FirstColumn)
			{
				WorksheetHelper.AccessColumn(worksheetImpl, iMaxColumn);
			}
			if (iMaxRow > rangeImpl.FirstRow)
			{
				WorksheetHelper.AccessRow(worksheetImpl, iMaxRow);
			}
			if (num)
			{
				m_book.UpdateFormula(iSourceIndex, rectangle, iDestIndex, rectangle2);
			}
			if ((options & OfficeCopyRangeOptions.CopyShapes) != 0)
			{
				rectangle.X++;
				rectangle.Y++;
				rectangle2.X++;
				rectangle2.Y++;
				((ShapesCollection)base.Shapes).CopyMoveShapeOnRangeCopy(worksheetImpl, rectangle, rectangle2, bIsCopy: false);
			}
		}
	}

	public IRange CopyRange(IRange destination, IRange source)
	{
		return CopyRange(destination, source, OfficeCopyRangeOptions.UpdateMerges);
	}

	public IRange CopyRange(IRange destination, IRange source, OfficeCopyRangeOptions options)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		ParseData();
		if (source.Worksheet != this)
		{
			return ((WorksheetImpl)source.Worksheet).CopyRange(destination, source);
		}
		RangeImpl rangeImpl = (RangeImpl)source;
		RangeImpl rangeImpl2 = (RangeImpl)destination;
		if (rangeImpl.IsEntireRow)
		{
			if (source.RowHeight < 0.0)
			{
				rangeImpl.SetDifferedRowHeight(rangeImpl, rangeImpl2);
			}
			else
			{
				destination.RowHeight = source.RowHeight;
			}
		}
		if (rangeImpl.IsEntireColumn)
		{
			if (source.ColumnWidth < 0.0)
			{
				rangeImpl.SetDifferedColumnWidth(rangeImpl, rangeImpl2);
			}
			else
			{
				destination.ColumnWidth = source.ColumnWidth;
			}
		}
		int num = destination.Row;
		int column = destination.Column;
		if (rangeImpl2.IsSingleCell && !rangeImpl.IsSingleCell)
		{
			int lastRow = num + rangeImpl.LastRow - rangeImpl.Row;
			int lastColumn = column + rangeImpl.LastColumn - rangeImpl.Column;
			rangeImpl2 = (RangeImpl)rangeImpl2[num, column, lastRow, lastColumn];
			destination = rangeImpl2;
		}
		int num2 = destination.LastRow - num + 1;
		int num3 = destination.LastColumn - column + 1;
		int num4 = source.LastRow - source.Row + 1;
		int num5 = source.LastColumn - source.Column + 1;
		int num6 = 1;
		int num7 = 1;
		if (num2 % num4 == 0 && num3 % num5 == 0)
		{
			num6 = num2 / num4;
			num7 = num3 / num5;
		}
		int num8 = 0;
		while (num8 < num6)
		{
			int num9 = 0;
			int num10 = column;
			while (num9 < num7)
			{
				rangeImpl2 = (RangeImpl)destination[num, num10, num + num4 - 1, num10 + num5 - 1];
				if (!rangeImpl2.AreFormulaArraysNotSeparated)
				{
					throw new InvalidRangeException();
				}
				CopyRangeWithoutCheck(rangeImpl, rangeImpl2, options);
				num9++;
				num10 += num5;
			}
			num8++;
			num += num4;
		}
		return destination;
	}

	public void CopyRangeWithoutCheck(RangeImpl source, RangeImpl destination, OfficeCopyRangeOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (!destination.AreFormulaArraysNotSeparated)
		{
			throw new InvalidRangeException("Can't copy to destination range.");
		}
		ParseData();
		int num = source.LastRow - source.Row + 1;
		int num2 = source.LastColumn - source.Column + 1;
		Rectangle rectIntersection;
		RecordTable recordTable = m_dicRecordsCells.CacheIntersection(destination, source, out rectIntersection);
		Dictionary<ArrayRecord, object> formulaArrays = destination.FormulaArrays;
		WorksheetImpl worksheetImpl = (WorksheetImpl)destination.Worksheet;
		if ((options & OfficeCopyRangeOptions.UpdateMerges) != 0)
		{
			CopyRangeMerges(destination, source);
		}
		if (formulaArrays != null && formulaArrays.Count > 0)
		{
			worksheetImpl.RemoveArrayFormulas(formulaArrays.Keys, bClearRange: true);
		}
		int column = destination.Column;
		int row = destination.Row;
		int row2 = source.Row;
		int column2 = source.Column;
		if ((options & OfficeCopyRangeOptions.CopyShapes) != 0)
		{
			Rectangle rectangle = new Rectangle(column2, row2, num2 - 1, num - 1);
			Rectangle recDest = rectangle;
			recDest.X = column;
			recDest.Y = row;
			((ShapesCollection)base.Shapes).CopyMoveShapeOnRangeCopy(worksheetImpl, rectangle, recDest, bIsCopy: true);
		}
		destination.Clear();
		CopyRange(row2, column2, num, num2, row, column, worksheetImpl, recordTable, rectIntersection, options);
		recordTable?.Dispose();
	}

	[CLSCompliant(false)]
	public void CopyCell(ICellPositionFormat cell, string strFormulaValue, IDictionary dicXFIndexes, long lNewIndex, WorkbookImpl book, Dictionary<int, int> dicFontIndexes, OfficeCopyRangeOptions options)
	{
		ParseData();
		_ = cell.Row;
		_ = cell.Column;
		m_dicRecordsCells.GetRange(lNewIndex)?.UpdateRecord();
	}

	public void CopyRange(int iSourceRow, int iSourceColumn, int iRowCount, int iColumnCount, int iDestRow, int iDestColumn, WorksheetImpl destSheet, RecordTable intersection, Rectangle rectIntersection, OfficeCopyRangeOptions options)
	{
		ParseData();
		Dictionary<int, int> dicFontIndexes = null;
		Dictionary<int, int> dicXFIndexes = null;
		if ((options & OfficeCopyRangeOptions.CopyStyles) != 0)
		{
			dicXFIndexes = GetUpdatedXFIndexes(iSourceRow, iSourceColumn, iRowCount, iColumnCount, destSheet, out dicFontIndexes);
		}
		CellRecordCollection cellRecords = destSheet.CellRecords;
		int num = iDestColumn - iSourceColumn;
		int num2 = iDestRow - iSourceRow;
		Dictionary<long, long> dictionary = new Dictionary<long, long>();
		RecordTable table = destSheet.CellRecords.Table;
		int rowStorageAllocationBlockSize = base.Application.RowStorageAllocationBlockSize;
		for (int i = 0; i < iRowCount; i++)
		{
			int num3 = iDestRow + i;
			int num4 = iSourceRow + i;
			for (int j = 0; j < iColumnCount; j++)
			{
				int num5 = iDestColumn + j;
				int num6 = iSourceColumn + j;
				long cellIndex = RangeImpl.GetCellIndex(num5, num3);
				RecordTable recordTable = GetRecordTable(num4, num6, rectIntersection, intersection, m_dicRecordsCells.Table);
				RowStorage rowStorage = recordTable.Rows[num4 - 1];
				ICellPositionFormat cellPositionFormat = rowStorage?.GetRecord(num6 - 1, rowStorageAllocationBlockSize);
				if (cellPositionFormat != null)
				{
					string formulaStringValue = rowStorage.GetFormulaStringValue(num6 - 1);
					destSheet.CopyCell(cellPositionFormat, formulaStringValue, dicXFIndexes, cellIndex, m_book, dicFontIndexes, options);
					if (cellPositionFormat.TypeCode != TBIFFRecord.Formula)
					{
						continue;
					}
					ArrayRecord arrayRecord = recordTable.GetArrayRecord(cellPositionFormat);
					if (arrayRecord == null)
					{
						continue;
					}
					long cellIndex2 = RangeImpl.GetCellIndex(arrayRecord.FirstColumn, arrayRecord.FirstRow);
					int num7 = 0;
					int num8 = 0;
					if (dictionary.ContainsKey(cellIndex2))
					{
						long index = dictionary[cellIndex2];
						num7 = RangeImpl.GetRowFromCellIndex(index);
						num8 = RangeImpl.GetColumnFromCellIndex(index);
					}
					else
					{
						num7 = num3 - 1;
						num8 = num5 - 1;
						long cellIndex3 = RangeImpl.GetCellIndex(num8, num7);
						dictionary[cellIndex2] = cellIndex3;
						arrayRecord.FirstColumn = Math.Max(arrayRecord.FirstColumn, iSourceColumn - 1) + num;
						arrayRecord.FirstRow = Math.Max(arrayRecord.FirstRow, iSourceRow - 1) + num2;
						arrayRecord.LastColumn = Math.Min(arrayRecord.LastColumn, iSourceColumn + iColumnCount - 2);
						arrayRecord.LastColumn += num;
						arrayRecord.LastRow = Math.Min(arrayRecord.LastRow, iSourceRow + iRowCount - 2);
						arrayRecord.LastRow += num2;
						if ((options & OfficeCopyRangeOptions.UpdateFormulas) != 0)
						{
							UpdateArrayFormula(arrayRecord, destSheet, num2, num);
						}
						table.Rows[num3 - 1].SetArrayRecord(num5 - 1, arrayRecord, base.Application.RowStorageAllocationBlockSize);
					}
					table.Rows[num3 - 1]?.SetArrayFormulaIndex(num5 - 1, num7, num8, base.Application.RowStorageAllocationBlockSize);
				}
				else
				{
					cellRecords.Remove(cellIndex);
				}
			}
		}
	}

	public void RemoveArrayFormulas(ICollection<ArrayRecord> colRemove, bool bClearRange)
	{
		if (colRemove == null)
		{
			throw new ArgumentNullException("colRemove");
		}
		ParseData();
		foreach (ArrayRecord item in colRemove)
		{
			RemoveArrayFormula(item, bClearRange);
		}
	}

	public Ptg[] UpdateFormula(Ptg[] arrFormula, int iRowOffset, int iColOffset)
	{
		if (arrFormula == null)
		{
			throw new ArgumentNullException("arrFormula");
		}
		ParseData();
		bool flag = iRowOffset != 0 || iColOffset != 0;
		Ptg[] array = new Ptg[arrFormula.Length];
		int i = 0;
		for (int num = arrFormula.Length; i < num; i++)
		{
			if (flag)
			{
				array[i] = arrFormula[i].Offset(iRowOffset, iColOffset, m_book);
			}
			else
			{
				array[i] = (Ptg)arrFormula[i].Clone();
			}
		}
		return array;
	}

	public override void UpdateFormula(int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect)
	{
		ParseData();
		m_dicRecordsCells.UpdateFormula(iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect);
		base.UpdateFormula(iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect);
	}

	public void AutofitRow(int rowIndex)
	{
		int column = UsedRange.Column;
		int lastColumn = UsedRange.LastColumn;
		AutofitRow(rowIndex, column, lastColumn, bRaiseEvents: true);
	}

	public void AutofitColumn(int colIndex)
	{
		(this[UsedRange.Row, UsedRange.Column, UsedRange.LastRow, UsedRange.LastColumn] as RangeImpl).AutoFitToColumn(colIndex, colIndex);
	}

	public void AutofitColumn(int colIndex, int firstRow, int lastRow)
	{
		(this[firstRow, colIndex, lastRow, colIndex] as RangeImpl).AutoFitToColumn(colIndex, colIndex);
	}

	public void CopyFrom(WorksheetImpl worksheet, Dictionary<string, string> hashStyleNames, Dictionary<string, string> hashWorksheetNames, Dictionary<int, int> dicFontIndexes, OfficeWorksheetCopyFlags flags)
	{
		Dictionary<int, int> hashExtFormatIndexes = new Dictionary<int, int>();
		Dictionary<int, int> hashNameIndexes = new Dictionary<int, int>();
		Dictionary<int, int> hashExternSheets = new Dictionary<int, int>();
		CopyFrom(worksheet, hashStyleNames, hashWorksheetNames, dicFontIndexes, flags, hashExtFormatIndexes, hashNameIndexes, hashExternSheets);
	}

	public void CopyFrom(WorksheetImpl worksheet, Dictionary<string, string> hashStyleNames, Dictionary<string, string> hashWorksheetNames, Dictionary<int, int> dicFontIndexes, OfficeWorksheetCopyFlags flags, Dictionary<int, int> hashExtFormatIndexes, Dictionary<int, int> hashNameIndexes)
	{
		CopyFrom(worksheet, hashStyleNames, hashWorksheetNames, dicFontIndexes, flags, hashExtFormatIndexes, hashNameIndexes, new Dictionary<int, int>(0));
	}

	public void CopyFrom(WorksheetImpl worksheet, Dictionary<string, string> hashStyleNames, Dictionary<string, string> hashWorksheetNames, Dictionary<int, int> dicFontIndexes, OfficeWorksheetCopyFlags flags, Dictionary<int, int> hashExtFormatIndexes, Dictionary<int, int> hashNameIndexes, Dictionary<int, int> hashExternSheets)
	{
		ParseData();
		if (worksheet.ParseDataOnDemand || worksheet.ParseOnDemand)
		{
			if (worksheet.m_dataHolder != null && worksheet.ParseDataOnDemand)
			{
				worksheet.m_dataHolder.ParseWorksheetData(worksheet, null, worksheet.ParseDataOnDemand);
			}
			else if (worksheet.m_dataHolder == null && worksheet.ParseOnDemand && !worksheet.IsParsed && worksheet.Parent is WorksheetsCollection)
			{
				worksheet.ParseData(null);
			}
			if (base.Parent is WorksheetsCollection)
			{
				foreach (WorksheetImpl item in base.Parent as WorksheetsCollection)
				{
					if (item.m_dataHolder != null && item.ParseDataOnDemand)
					{
						item.m_dataHolder.ParseWorksheetData(item, null, item.ParseDataOnDemand);
					}
					else if (item.m_dataHolder == null && item.ParseOnDemand && !item.IsParsed && item.Parent is WorksheetsCollection)
					{
						item.ParseData(null);
					}
				}
			}
		}
		if ((flags & OfficeWorksheetCopyFlags.ClearBefore) != 0)
		{
			ClearAll();
			flags &= ~OfficeWorksheetCopyFlags.ClearBefore;
		}
		if ((flags & OfficeWorksheetCopyFlags.CopyColumnHeight) != 0)
		{
			CopyColumnWidth(worksheet, hashExtFormatIndexes);
			flags &= ~OfficeWorksheetCopyFlags.CopyColumnHeight;
		}
		if ((flags & OfficeWorksheetCopyFlags.CopyRowHeight) != 0)
		{
			CopyRowHeight(worksheet, hashExtFormatIndexes);
			flags &= ~OfficeWorksheetCopyFlags.CopyRowHeight;
		}
		CustomHeight = worksheet.CustomHeight;
		if ((flags & OfficeWorksheetCopyFlags.CopyNames) != 0)
		{
			CopyNames(worksheet, hashWorksheetNames, hashNameIndexes, hashExternSheets);
			flags &= ~OfficeWorksheetCopyFlags.CopyNames;
		}
		CopyFrom(worksheet, hashStyleNames, hashWorksheetNames, dicFontIndexes, flags, hashExtFormatIndexes);
		if ((flags & OfficeWorksheetCopyFlags.CopyCells) != 0)
		{
			m_iFirstRow = worksheet.m_iFirstRow;
			m_iLastRow = worksheet.m_iLastRow;
			m_iFirstColumn = worksheet.m_iFirstColumn;
			base.Zoom = worksheet.Zoom;
			m_iLastColumn = worksheet.m_iLastColumn;
			m_dicRecordsCells.CopyCells(worksheet.m_dicRecordsCells, hashStyleNames, hashWorksheetNames, hashExtFormatIndexes, hashNameIndexes, dicFontIndexes, hashExternSheets);
		}
		if ((flags & OfficeWorksheetCopyFlags.CopyMerges) != 0)
		{
			CopyMerges(worksheet);
		}
		if ((flags & OfficeWorksheetCopyFlags.CopyConditionlFormats) != 0)
		{
			CopyConditionalFormats(worksheet);
		}
		if ((flags & OfficeWorksheetCopyFlags.CopyAutoFilters) != 0)
		{
			CopyAutoFilters(worksheet);
		}
		if ((flags & OfficeWorksheetCopyFlags.CopyPageSetup) != 0)
		{
			CopyPageSetup(worksheet);
		}
		if ((flags & OfficeWorksheetCopyFlags.CopyTables) != 0)
		{
			CopyTables(worksheet, hashWorksheetNames);
		}
	}

	private void CopyTables(WorksheetImpl worksheet, Dictionary<string, string> hashWorksheetNames)
	{
	}

	public bool CanMove(ref IRange destination, IRange source)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		ParseData();
		RangeImpl rangeImpl = (RangeImpl)destination;
		RangeImpl rangeImpl2 = (RangeImpl)source;
		int lastRow = rangeImpl.FirstRow + rangeImpl2.LastRow - rangeImpl2.FirstRow;
		int lastColumn = rangeImpl.FirstColumn + rangeImpl2.LastColumn - rangeImpl2.FirstColumn;
		rangeImpl = (RangeImpl)(destination = (RangeImpl)rangeImpl.InnerWorksheet.Range[rangeImpl.Row, rangeImpl.Column, lastRow, lastColumn]);
		if (rangeImpl == rangeImpl2)
		{
			return true;
		}
		Dictionary<ArrayRecord, object> dictionary = new Dictionary<ArrayRecord, object>();
		bool areArrayFormulasNotSeparated = rangeImpl2.GetAreArrayFormulasNotSeparated(dictionary);
		if (areArrayFormulasNotSeparated)
		{
			if (rangeImpl.Worksheet != rangeImpl2.Worksheet)
			{
				dictionary.Clear();
			}
			areArrayFormulasNotSeparated = rangeImpl.GetAreArrayFormulasNotSeparated(dictionary);
		}
		return areArrayFormulasNotSeparated;
	}

	public bool CanInsertRow(int iRowIndex, int iRowCount, OfficeInsertOptions options)
	{
		ParseData();
		if (iRowIndex < 1 || iRowIndex > m_book.MaxRowCount)
		{
			return false;
		}
		if (iRowCount <= 0)
		{
			return false;
		}
		if (m_iLastRow <= iRowIndex)
		{
			return true;
		}
		if (iRowIndex >= m_iFirstRow && m_iLastColumn <= m_book.MaxColumnCount && !((RangeImpl)Range[iRowIndex, m_iFirstColumn, m_iLastRow, m_iLastColumn]).AreFormulaArraysNotSeparated)
		{
			return false;
		}
		int num = m_iLastRow + iRowCount - m_book.MaxRowCount;
		if (num > 0)
		{
			int num2 = Math.Max(m_iLastRow - num, m_iFirstRow);
			for (int num3 = m_iLastRow; num3 >= num2; num3--)
			{
				if (!IsRowEmpty(num3))
				{
					return false;
				}
				m_iLastRow--;
			}
			if (m_iFirstRow > m_iLastRow)
			{
				m_iLastRow = (m_iFirstRow = -1);
				m_iLastColumn = (m_iFirstColumn = int.MaxValue);
			}
		}
		return true;
	}

	public bool CanInsertColumn(int iColumnIndex, int iColumnCount, OfficeInsertOptions options)
	{
		ParseData();
		if (iColumnIndex < 1 || iColumnIndex > m_book.MaxColumnCount)
		{
			return false;
		}
		if (m_iLastColumn < iColumnIndex || m_iFirstColumn == int.MaxValue)
		{
			return true;
		}
		if (iColumnIndex >= m_iFirstColumn && !((RangeImpl)Range[m_iFirstRow, iColumnIndex, m_iLastRow, m_iLastColumn]).AreFormulaArraysNotSeparated)
		{
			return false;
		}
		int num = m_iLastColumn + iColumnCount - m_book.MaxColumnCount;
		if (num > 0)
		{
			int num2 = Math.Max(m_iLastColumn - num, m_iFirstColumn);
			for (int num3 = m_iLastColumn; num3 >= num2; num3--)
			{
				if (!IsColumnEmpty(num3))
				{
					return false;
				}
				m_iLastColumn--;
			}
			if (m_iFirstColumn > m_iLastColumn)
			{
				m_iLastRow = (m_iFirstRow = -1);
				m_iLastColumn = (m_iFirstColumn = int.MaxValue);
			}
		}
		return true;
	}

	public IRange GetRangeByString(string strRangeValue, bool hasFormula)
	{
		if (strRangeValue == null || strRangeValue.Length == 0)
		{
			return null;
		}
		Ptg[] array = (m_book.IsWorkbookOpening ? new FormulaUtil(base.Application, m_book, NumberFormatInfo.InvariantInfo, base.Application.ArgumentsSeparator, base.Application.RowSeparator) : m_book.FormulaUtil).ParseString(strRangeValue);
		Stack<object> stack = new Stack<object>();
		List<IRange> list = new List<IRange>();
		int num = 0;
		int i = 0;
		for (int num2 = array.Length; i < num2; i++)
		{
			if (array[i] is IRangeGetter)
			{
				List<IRange> list2 = ((list != null) ? list : new List<IRange>());
				IRange range = ((IRangeGetter)array[i]).GetRange(base.Workbook, this);
				list2.Add(range);
				stack.Push(list2);
				num++;
				list = null;
			}
			else if (array[i].TokenCode == FormulaToken.tCellRangeList && num > 0)
			{
				list = (List<IRange>)stack.Pop();
				if (stack.Count > 0)
				{
					((List<IRange>)stack.Peek()).AddRange(list);
				}
				list.Clear();
				num--;
			}
		}
		list = (List<IRange>)stack.Pop();
		int count = list.Count;
		if (count == 1)
		{
			return list[0];
		}
		IRanges ranges = list[0].Worksheet.CreateRangesCollection();
		for (int j = 0; j < count; j++)
		{
			ranges.Add(list[j]);
		}
		return ranges;
	}

	public void UpdateNamedRangeIndexes(int[] arrNewIndex)
	{
		if (arrNewIndex == null)
		{
			throw new ArgumentNullException("arrNewIndex");
		}
		ParseData();
		m_dicRecordsCells.UpdateNameIndexes(m_book, arrNewIndex);
		base.InnerShapes?.UpdateNamedRangeIndexes(arrNewIndex);
	}

	public void UpdateNamedRangeIndexes(IDictionary<int, int> dicNewIndex)
	{
		if (dicNewIndex == null)
		{
			throw new ArgumentNullException("dicNewIndex");
		}
		ParseData();
		if (m_dicRecordsCells != null)
		{
			m_dicRecordsCells.UpdateNameIndexes(m_book, dicNewIndex);
		}
		base.InnerShapes?.UpdateNamedRangeIndexes(dicNewIndex);
	}

	public int GetStringIndex(long cellIndex)
	{
		ParseData();
		if (!(m_dicRecordsCells.GetCellRecord(cellIndex) is LabelSSTRecord labelSSTRecord))
		{
			return -1;
		}
		return labelSSTRecord.SSTIndex;
	}

	public TextWithFormat GetTextWithFormat(long cellIndex)
	{
		ParseData();
		ICellPositionFormat cellRecord = m_dicRecordsCells.GetCellRecord(cellIndex);
		if (cellRecord is LabelRecord)
		{
			LabelRecord labelRecord = cellRecord as LabelRecord;
			SetString(labelRecord.Row + 1, labelRecord.Column + 1, labelRecord.Label);
		}
		if (!(m_dicRecordsCells.GetCellRecord(cellIndex) is LabelSSTRecord { SSTIndex: var sSTIndex }))
		{
			return null;
		}
		return m_book.InnerSST[sSTIndex];
	}

	public object GetTextObject(long cellIndex)
	{
		ParseData();
		if (!(m_dicRecordsCells.GetCellRecord(cellIndex) is LabelSSTRecord { SSTIndex: var sSTIndex }))
		{
			return null;
		}
		return m_book.InnerSST[sSTIndex];
	}

	public ExtendedFormatImpl GetExtendedFormat(long cellIndex)
	{
		ParseData();
		ICellPositionFormat cellRecord = m_dicRecordsCells.GetCellRecord(cellIndex);
		if (cellRecord == null)
		{
			return null;
		}
		int extendedFormatIndex = cellRecord.ExtendedFormatIndex;
		return m_book.InnerExtFormats[extendedFormatIndex];
	}

	public void SetLabelSSTIndex(long cellIndex, int iSSTIndex)
	{
		ParseData();
		int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(cellIndex);
		int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(cellIndex);
		ICellPositionFormat cellPositionFormat = m_dicRecordsCells.GetCellRecord(rowFromCellIndex, columnFromCellIndex);
		if (iSSTIndex == -1)
		{
			if (cellPositionFormat == null || cellPositionFormat.TypeCode != TBIFFRecord.Blank)
			{
				m_dicRecordsCells.SetCellRecord(rowFromCellIndex, columnFromCellIndex, (ICellPositionFormat)GetRecord(TBIFFRecord.Blank, rowFromCellIndex, columnFromCellIndex));
			}
			return;
		}
		if (iSSTIndex < 0 || iSSTIndex >= m_book.InnerSST.Count)
		{
			throw new ArgumentOutOfRangeException("iSSTIndex");
		}
		if (cellPositionFormat == null || cellPositionFormat.TypeCode != TBIFFRecord.LabelSST)
		{
			cellPositionFormat = (ICellPositionFormat)GetRecord(TBIFFRecord.LabelSST, rowFromCellIndex, columnFromCellIndex);
		}
		((LabelSSTRecord)cellPositionFormat).SSTIndex = iSSTIndex;
		if (rowFromCellIndex != 0 || columnFromCellIndex != 0)
		{
			m_dicRecordsCells.SetCellRecord(rowFromCellIndex, columnFromCellIndex, cellPositionFormat);
		}
	}

	public void UpdateStringIndexes(List<int> arrNewIndexes)
	{
		if (arrNewIndexes == null)
		{
			throw new ArgumentNullException("arrNewIndexes");
		}
		ParseData();
		m_dicRecordsCells.UpdateStringIndexes(arrNewIndexes);
	}

	public void RemoveMergedCells(IRange range)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		ParseData();
		int row = range.Row;
		int column = range.Column;
		int lastRow = range.LastRow;
		int lastColumn = range.LastColumn;
		for (int i = row; i <= lastRow; i++)
		{
			for (int j = column; j <= lastColumn; j++)
			{
				if (i != row || j != column)
				{
					long cellIndex = RangeImpl.GetCellIndex(j, i);
					m_dicRecordsCells.Remove(cellIndex);
				}
			}
		}
	}

	public void SetActiveCell(IRange range)
	{
		SetActiveCell(range, updateApplication: true);
	}

	public void SetActiveCell(IRange range, bool updateApplication)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		ParseData();
		if (updateApplication)
		{
			base.AppImplementation.SetActiveCell(range);
		}
		CreateAllSelections();
		ActivatePane(range);
		SelectionRecord activeSelection = GetActiveSelection();
		int num = range.Column - 1;
		int num2 = range.Row - 1;
		activeSelection.ColumnActiveCell = (ushort)num;
		activeSelection.RowActiveCell = (ushort)num2;
		SelectionRecord.TAddr addr = new SelectionRecord.TAddr((ushort)num2, (ushort)num2, (byte)num, (byte)num);
		activeSelection.SetSelection(0, addr);
	}

	private void ActivatePane(IRange range)
	{
		if (base.WindowTwo.IsFreezePanes)
		{
			IRange splitCell = SplitCell;
			int num = ((TopLeftCell.Row < splitCell.Row) ? ((TopLeftCell.Column >= splitCell.Column) ? 2 : 0) : ((TopLeftCell.Column < splitCell.Column) ? 1 : 3));
			m_pane.ActivePane = (ushort)num;
		}
	}

	private SelectionRecord GetActiveSelection()
	{
		SelectionRecord selectionRecord = null;
		int num = ((m_pane != null) ? m_pane.ActivePane : 0);
		int i = 0;
		for (int count = m_arrSelections.Count; i < count; i++)
		{
			SelectionRecord selectionRecord2 = m_arrSelections[i];
			if (selectionRecord2.Pane == num)
			{
				selectionRecord = selectionRecord2;
				break;
			}
		}
		if (selectionRecord == null && m_arrSelections.Count == 1)
		{
			selectionRecord = m_arrSelections[0];
		}
		return selectionRecord;
	}

	public IRange GetActiveCell()
	{
		ParseData();
		SelectionRecord activeSelection = GetActiveSelection();
		int num = 0;
		int num2 = 0;
		if (activeSelection != null)
		{
			num = activeSelection.RowActiveCell;
			num2 = activeSelection.ColumnActiveCell;
		}
		return this[num + 1, num2 + 1];
	}

	[CLSCompliant(false)]
	public bool IsArrayFormula(FormulaRecord formula)
	{
		if (formula == null || formula.ParsedExpression == null || formula.ParsedExpression.Length == 0)
		{
			return false;
		}
		if (formula.ParsedExpression[0].TokenCode == FormulaToken.tExp)
		{
			return CellRecords.GetArrayRecord(formula.Row + 1, formula.Column + 1) != null;
		}
		return false;
	}

	public bool IsArrayFormula(long cellIndex)
	{
		ParseData();
		if (m_dicRecordsCells.GetCellRecord(cellIndex) is FormulaRecord formula)
		{
			return IsArrayFormula(formula);
		}
		return false;
	}

	public double InnerGetRowHeight(int iRow, bool bRaiseEvents)
	{
		if (iRow < 1 || iRow > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("Value cannot be less 1 and greater than max row index.");
		}
		RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, iRow - 1, bCreate: false);
		bool flag = orCreateRow != null;
		if (orCreateRow != null)
		{
			if (orCreateRow.IsHidden)
			{
				return 0.0;
			}
			if (m_bIsExportDataTable || orCreateRow.IsBadFontHeight || (CustomHeight && StandardHeight == (double)(int)orCreateRow.Height / 20.0 && Range[iRow, FirstColumn, iRow, LastColumn].CellStyle.Rotation <= 0) || (orCreateRow.IsWrapText && !Range[iRow, FirstColumn, iRow, LastColumn].IsMerged))
			{
				return (double)(int)orCreateRow.Height / 20.0;
			}
		}
		if (flag)
		{
			return (double)(int)orCreateRow.Height / 20.0;
		}
		return StandardHeight;
	}

	public override object Clone(object parent, bool cloneShapes)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		WorksheetImpl worksheetImpl = (WorksheetImpl)base.Clone(parent, cloneShapes);
		worksheetImpl.m_rngUsed = null;
		worksheetImpl.m_migrantRange = null;
		worksheetImpl.m_pane = (PaneRecord)CloneUtils.CloneCloneable(m_pane);
		worksheetImpl.m_arrSortRecords = CloneUtils.CloneCloneable(m_arrSortRecords);
		worksheetImpl.m_arrDConRecords = CloneUtils.CloneCloneable(m_arrDConRecords);
		worksheetImpl.m_arrAutoFilter = CloneUtils.CloneCloneable(m_arrAutoFilter);
		worksheetImpl.m_arrSelections = CloneUtils.CloneCloneable(m_arrSelections);
		if (m_arrNotes != null)
		{
			worksheetImpl.m_arrNotes = CloneUtils.CloneCloneable(m_arrNotes);
			worksheetImpl.m_arrNotesByCellIndex = CloneUtils.CloneCloneable(m_arrNotesByCellIndex);
		}
		worksheetImpl.m_arrColumnInfo = CloneUtils.CloneArray(m_arrColumnInfo);
		worksheetImpl.m_names = new WorksheetNamesCollection(base.Application, this);
		worksheetImpl.m_pageSetup = m_pageSetup.Clone(worksheetImpl);
		worksheetImpl.m_mergedCells = (MergeCellsImpl)CloneUtils.CloneCloneable(m_mergedCells, worksheetImpl);
		worksheetImpl.m_dicRecordsCells = m_dicRecordsCells.Clone(worksheetImpl);
		worksheetImpl.m_book.InnerWorksheets.InnerAdd(worksheetImpl);
		return worksheetImpl;
	}

	public void ReAddAllStrings()
	{
		ParseData();
		m_dicRecordsCells.ReAddAllStrings();
	}

	public bool? GetStringPreservedValue(ICombinedRange range)
	{
		return m_stringPreservedRanges.GetRangeValue(range);
	}

	public void SetStringPreservedValue(ICombinedRange range, bool? value)
	{
		m_stringPreservedRanges.SetRange(range, value);
	}

	public override void MarkUsedReferences(bool[] usedItems)
	{
		m_dicRecordsCells.MarkUsedReferences(usedItems);
		IOfficeChartShapes charts = base.Charts;
		int i = 0;
		for (int count = charts.Count; i < count; i++)
		{
			(charts[i] as ChartImpl).MarkUsedReferences(usedItems);
		}
	}

	public override void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		m_dicRecordsCells.UpdateReferenceIndexes(arrUpdatedIndexes);
		IOfficeChartShapes charts = base.Charts;
		int i = 0;
		for (int count = charts.Count; i < count; i++)
		{
			(charts[i] as ChartImpl).UpdateReferenceIndexes(arrUpdatedIndexes);
		}
	}

	protected void CreateEmptyPane()
	{
		m_pane = (PaneRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Pane);
	}

	protected void CopyCell(IRange destCell, IRange sourceCell)
	{
		CopyCell(destCell, sourceCell, OfficeCopyRangeOptions.None);
	}

	protected void CopyCell(IRange destCell, IRange sourceCell, OfficeCopyRangeOptions options)
	{
		if (destCell == null)
		{
			throw new ArgumentNullException("destCell");
		}
		if (sourceCell == null)
		{
			throw new ArgumentNullException("sourceCell");
		}
		RangeImpl rangeImpl = (RangeImpl)destCell;
		RangeImpl rangeImpl2 = (RangeImpl)sourceCell;
		if (!rangeImpl.IsSingleCell || !rangeImpl2.IsSingleCell)
		{
			throw new ArgumentException("Each range argument should contain a single cell");
		}
		rangeImpl.ExtendedFormatIndex = rangeImpl2.ExtendedFormatIndex;
		if (rangeImpl2.Record != null && rangeImpl2.Record is FormulaRecord)
		{
			if (sourceCell.HasFormulaArray)
			{
				return;
			}
			FormulaRecord formulaRecord = (FormulaRecord)rangeImpl2.Record;
			FormulaRecord formulaRecord2 = (FormulaRecord)formulaRecord.Clone();
			formulaRecord2.Row = (ushort)(destCell.Row - 1);
			formulaRecord2.Column = (ushort)(destCell.Column - 1);
			bool num = (options & OfficeCopyRangeOptions.UpdateFormulas) != 0;
			int iRowOffset = (num ? (destCell.Row - sourceCell.Row) : 0);
			int iColOffset = (num ? (destCell.Column - sourceCell.Column) : 0);
			formulaRecord2.ParsedExpression = UpdateFormula(formulaRecord.ParsedExpression, iRowOffset, iColOffset);
			rangeImpl.SetFormula(formulaRecord2);
		}
		else
		{
			destCell.Value = sourceCell.Value;
		}
		CopyComment(rangeImpl2, rangeImpl);
	}

	private int GetColumnCount(RangeImpl source, RangeImpl dest)
	{
		int result = 0;
		if (source.Row != dest.Row)
		{
			result = dest.Column - source.Column;
		}
		return result;
	}

	private int GetRowCount(RangeImpl source, RangeImpl dest)
	{
		int result = 0;
		if (source.Row != dest.Row)
		{
			result = dest.Row - source.Row;
		}
		return result;
	}

	private void CopyComment(RangeImpl source, RangeImpl dest)
	{
	}

	private void RemoveLastRow(bool bUpdateFormula)
	{
		RemoveLastRow(bUpdateFormula, 1);
	}

	private void RemoveLastRow(bool bUpdateFormula, int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (count == 0)
		{
			return;
		}
		ParseData();
		int num = m_dicRecordsCells.Table.LastRow + 1;
		int num2 = 0;
		int num3 = num;
		while (num2 < count)
		{
			if (num3 < 0)
			{
				count = num2;
				break;
			}
			m_dicRecordsCells.RemoveRow(num3);
			num2++;
			num3--;
		}
		m_iLastRow = m_dicRecordsCells.Table.LastRow + 1;
		if (m_iLastRow == 0)
		{
			m_iFirstRow = -1;
			m_iLastRow = -1;
		}
	}

	private void RemoveLastColumn(bool bUpdateFormula)
	{
		ParseData();
		int num = m_iLastColumn--;
		m_dicRecordsCells.RemoveLastColumn(num);
		if (m_iFirstColumn > m_iLastColumn)
		{
			m_iLastColumn = (m_iFirstColumn = int.MaxValue);
		}
		if (bUpdateFormula)
		{
			Rectangle rectSource = Rectangle.FromLTRB(num, 0, m_book.MaxColumnCount - 1, m_book.MaxRowCount - 1);
			Rectangle rectDest = Rectangle.FromLTRB(num - 1, 0, m_book.MaxColumnCount - 1, m_book.MaxRowCount - 1);
			int num2 = m_book.AddSheetReference(this);
			m_book.UpdateFormula(num2, rectSource, num2, rectDest);
		}
	}

	private void RemoveLastColumn(bool bUpdateFormula, int count)
	{
		ParseData();
		int num = m_iLastColumn--;
		int num2 = 0;
		while (num2 < count && m_iLastColumn >= 0)
		{
			m_dicRecordsCells.RemoveLastColumn(m_iLastColumn + 1);
			num2++;
			m_iLastColumn--;
		}
		m_iLastColumn = m_dicRecordsCells.LastColumn + 1;
		if (m_iFirstColumn > m_iLastColumn)
		{
			m_iLastColumn = m_iFirstColumn;
		}
		if (bUpdateFormula)
		{
			Rectangle rectSource = Rectangle.FromLTRB(num + count - 1, 0, m_book.MaxColumnCount - 1, m_book.MaxRowCount - 1);
			Rectangle rectDest = Rectangle.FromLTRB(num - 1, 0, m_book.MaxColumnCount - 1, m_book.MaxRowCount - 1);
			int num3 = m_book.AddSheetReference(this);
			m_book.UpdateFormula(num3, rectSource, num3, rectDest);
		}
	}

	private void PartialClearRange(Rectangle rect)
	{
		ParseData();
		if (!m_dicRecordsCells.UseCache)
		{
			return;
		}
		int num = rect.Top + 1;
		int num2 = rect.Left + 1;
		int num3 = rect.Bottom + 1;
		int num4 = rect.Right + 1;
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				m_dicRecordsCells.GetRange(i, j)?.PartialClear();
			}
		}
	}

	private RecordTable CacheAndRemoveFromParent(IRange source, IRange destination, ref int iMaxRow, ref int iMaxColumn, CellRecordCollection tableSource)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (tableSource == null)
		{
			throw new ArgumentNullException("tableSource");
		}
		_ = (WorksheetImpl)destination.Parent;
		_ = (WorksheetImpl)source.Worksheet;
		int iDeltaRow = destination.Row - source.Row;
		int iDeltaColumn = destination.Column - source.Column;
		_ = source.LastColumn;
		_ = source.Column;
		return tableSource.CacheAndRemove((RangeImpl)source, iDeltaRow, iDeltaColumn, ref iMaxRow, ref iMaxColumn);
	}

	private void CopyCacheInto(RecordTable source, RecordTable destination, bool bUpdateRowRecords)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (source.FirstRow < 0)
		{
			return;
		}
		int i = source.FirstRow;
		for (int lastRow = source.LastRow; i <= lastRow; i++)
		{
			RowStorage rowStorage = source.Rows[i];
			RowStorage rowStorage2 = destination.Rows[i];
			if (rowStorage == null)
			{
				if (rowStorage2 != null && rowStorage2.UsedSize == 0 && bUpdateRowRecords)
				{
					destination.RemoveRow(i);
				}
				continue;
			}
			rowStorage2 = destination.Rows[i];
			WorksheetHelper.AccessRow(this, i + 1);
			if (rowStorage2 == null)
			{
				rowStorage2 = new RowStorage(i, base.AppImplementation.StandardHeightInRowUnits, destination.Workbook.DefaultXFIndex);
				rowStorage2.IsFormatted = false;
				destination.SetRow(i, rowStorage2);
			}
			if (bUpdateRowRecords)
			{
				rowStorage2.CopyRowRecordFrom(rowStorage);
			}
			if (rowStorage.UsedSize > 0)
			{
				WorksheetHelper.AccessColumn(this, rowStorage.FirstColumn + 1);
				WorksheetHelper.AccessColumn(this, rowStorage.LastColumn + 1);
				rowStorage2.InsertRowData(rowStorage, base.Application.RowStorageAllocationBlockSize);
			}
		}
	}

	private static void ClearRange(IDictionary dictionary, Rectangle rect)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		int num = rect.Top + 1;
		int num2 = rect.Left + 1;
		int num3 = rect.Bottom + 1;
		int num4 = rect.Right + 1;
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				long cellIndex = RangeImpl.GetCellIndex(j, i);
				dictionary.Remove(cellIndex);
			}
		}
	}

	private void UpdateArrayFormula(ArrayRecord array, IWorksheet destSheet, int iDeltaRow, int iDeltaColumn)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (destSheet == null)
		{
			throw new ArgumentNullException("destSheet");
		}
		WorkbookImpl workbookImpl = (WorkbookImpl)destSheet.Workbook;
		array.Formula = workbookImpl.FormulaUtil.UpdateFormula(array.Formula, iDeltaRow, iDeltaColumn);
	}

	private RecordTable GetRecordTable(int iRow, int iColumn, Rectangle rectIntersection, RecordTable intersection, RecordTable rectSource)
	{
		if (!UtilityMethods.Contains(rectIntersection, iColumn, iRow))
		{
			return rectSource;
		}
		return intersection;
	}

	private Dictionary<int, int> GetUpdatedXFIndexes(int iRow, int iColumn, int iRowCount, int iColCount, WorksheetImpl destSheet, out Dictionary<int, int> dicFontIndexes)
	{
		if (destSheet == null)
		{
			throw new ArgumentNullException("destSheet");
		}
		dicFontIndexes = null;
		if (m_book == destSheet.Workbook)
		{
			return null;
		}
		ParseData();
		dicFontIndexes = new Dictionary<int, int>();
		Dictionary<int, object> hashToAdd = new Dictionary<int, object>();
		IList<ExtendedFormatImpl> arrXFormats = new List<ExtendedFormatImpl>();
		ExtendedFormatsCollection innerExtFormats = m_book.InnerExtFormats;
		int i = iRow;
		for (int num = iRow + iRowCount; i < num; i++)
		{
			int j = iColumn;
			for (int num2 = iColumn + iColCount; j < num2; j++)
			{
				long cellIndex = RangeImpl.GetCellIndex(j, i);
				ICellPositionFormat cellRecord = m_dicRecordsCells.GetCellRecord(cellIndex);
				if (cellRecord != null)
				{
					int extendedFormatIndex = cellRecord.ExtendedFormatIndex;
					innerExtFormats.AddIndex(hashToAdd, arrXFormats, extendedFormatIndex);
				}
			}
		}
		return destSheet.ParentWorkbook.InnerExtFormats.Merge(arrXFormats, out dicFontIndexes);
	}

	private void ClearCell(long cellIndex)
	{
		ParseData();
		m_dicRecordsCells.Remove(cellIndex);
		m_dicRecordsCells.GetRange(cellIndex)?.Clear();
	}

	private void SetArrayFormulaRanges(ArrayRecord array)
	{
		ParseData();
		Ptg ptg = FormulaUtil.CreatePtg(FormulaToken.tExp, array.FirstRow, array.FirstColumn);
		int lastRow = array.LastRow;
		int lastColumn = array.LastColumn;
		int firstRow = array.FirstRow;
		int firstColumn = array.FirstColumn;
		for (int i = firstRow; i <= lastRow; i++)
		{
			for (int j = firstColumn; j <= lastColumn; j++)
			{
				long cellIndex = RangeImpl.GetCellIndex(j + 1, i + 1);
				FormulaRecord formulaRecord = (FormulaRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Formula);
				formulaRecord.Row = i;
				formulaRecord.Column = j;
				formulaRecord.ParsedExpression = new Ptg[1] { (Ptg)ptg.Clone() };
				m_dicRecordsCells.SetCellRecord(cellIndex, formulaRecord);
				((RangeImpl)Range[i + 1, j + 1]).UpdateRecord();
			}
		}
		UpdateFirstLast(firstRow + 1, firstColumn + 1);
		UpdateFirstLast(lastRow + 1, lastColumn + 1);
	}

	[CLSCompliant(false)]
	protected void RemoveArrayFormula(ArrayRecord record, bool bClearRange)
	{
		ParseData();
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		int num = record.FirstRow + 1;
		int num2 = record.FirstColumn + 1;
		int num3 = record.LastRow + 1;
		int num4 = record.LastColumn + 1;
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				m_dicRecordsCells.SetCellRecord(i, j, null);
			}
		}
	}

	private ArrayRecord CreateArrayFormula(ArrayRecord arraySource, IRange destination, IRange source, int iRow, int iColumn, bool bUpdateFormula)
	{
		if (arraySource == null)
		{
			throw new ArgumentNullException("arraySource");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		ParseData();
		Rectangle a = Rectangle.FromLTRB(arraySource.FirstRow, arraySource.FirstColumn, arraySource.LastRow, arraySource.LastColumn);
		Rectangle b = Rectangle.FromLTRB(source.Row - 1, source.Column - 1, source.LastRow - 1, source.LastColumn - 1);
		Rectangle rectangle = Rectangle.Intersect(a, b);
		if (rectangle.IsEmpty)
		{
			throw new ArgumentNullException("Intersection is empty");
		}
		rectangle.Offset(destination.Row - source.Row, destination.Column - source.Column);
		if (rectangle.Left < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (rectangle.Top < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		ArrayRecord arrayRecord = (ArrayRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Array);
		arrayRecord.FirstRow = iRow + destination.Row - source.Row - 1;
		arrayRecord.FirstColumn = iColumn + destination.Column - source.Column - 1;
		arrayRecord.LastRow = arrayRecord.FirstRow - rectangle.Left + rectangle.Right;
		arrayRecord.LastColumn = arrayRecord.FirstColumn - rectangle.Top + rectangle.Bottom;
		arrayRecord.IsRecalculateAlways = true;
		arrayRecord.IsRecalculateOnOpen = true;
		int iRowOffset = (bUpdateFormula ? (arrayRecord.FirstRow - arraySource.FirstRow) : 0);
		int iColOffset = (bUpdateFormula ? (arrayRecord.FirstColumn - arraySource.FirstColumn) : 0);
		arrayRecord.Formula = UpdateFormula(arraySource.Formula, iRowOffset, iColOffset);
		return arrayRecord;
	}

	protected void CheckRangesSizes(IRange destination, IRange source)
	{
		if (destination.LastRow - destination.Row != source.LastRow - source.Row || destination.LastColumn - destination.Column != source.LastColumn - source.Column)
		{
			throw new ArgumentException("Ranges do not fit each other");
		}
	}

	private void CopyRangeMerges(IRange destination, IRange source)
	{
		CopyRangeMerges(destination, source, bDeleteSource: false);
	}

	private static void CopyRangeMerges(IRange destination, IRange source, bool bDeleteSource)
	{
		RangeImpl rangeImpl = (RangeImpl)source;
		RangeImpl rangeImpl2 = (RangeImpl)destination;
		MergeCellsImpl mergeCells = rangeImpl.InnerWorksheet.MergeCells;
		MergeCellsImpl mergeCells2 = rangeImpl2.InnerWorksheet.MergeCells;
		if (mergeCells != null)
		{
			if (mergeCells == mergeCells2)
			{
				mergeCells.CopyMoveMerges(rangeImpl2, rangeImpl, bDeleteSource);
				return;
			}
			int iRowDelta = destination.Row - source.Row;
			int iColDelta = destination.Column - source.Column;
			List<MergeCellsRecord.MergedRegion> lstRegions = mergeCells.FindMergesToCopyMove(rangeImpl, bDeleteSource);
			Rectangle range = Rectangle.FromLTRB(destination.Column - 1, destination.Row - 1, destination.LastColumn - 1, destination.LastRow - 1);
			mergeCells2.DeleteMerge(range);
			mergeCells2.AddCache(lstRegions, iRowDelta, iColDelta);
		}
	}

	[CLSCompliant(false)]
	protected internal NoteRecord GetNoteByObjectIndex(int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index < 0");
		}
		ParseData();
		NoteRecord value = null;
		if (m_arrNotes != null)
		{
			m_arrNotes.TryGetValue(index, out value);
		}
		return value;
	}

	[CLSCompliant(false)]
	protected internal void AddNote(NoteRecord note)
	{
		ParseData();
		int objId = note.ObjId;
		bool flag = m_arrNotes == null;
		long cellIndex;
		if (!flag && m_arrNotes.ContainsKey(objId))
		{
			NoteRecord noteRecord = m_arrNotes[objId];
			cellIndex = RangeImpl.GetCellIndex(noteRecord.Column, noteRecord.Row);
			m_arrNotesByCellIndex.Remove(cellIndex);
		}
		else if (flag)
		{
			m_arrNotes = new SortedList<int, NoteRecord>();
			m_arrNotesByCellIndex = new SortedList<long, NoteRecord>();
		}
		m_arrNotes[objId] = note;
		cellIndex = RangeImpl.GetCellIndex(note.Column, note.Row);
		m_arrNotesByCellIndex[cellIndex] = note;
	}

	public void AutofitRow(int rowIndex, int firstColumn, int lastColumn, bool bRaiseEvents)
	{
		ParseData();
		RichTextString richText = new RichTextString(base.Application, this, isReadOnly: false, bCreateText: true);
		if (firstColumn == 0 || lastColumn == 0 || firstColumn > lastColumn)
		{
			return;
		}
		SizeF sizeF = new SizeF(0f, 0f);
		bool bIsMergedAndWrapped = false;
		for (int i = firstColumn; i <= lastColumn; i++)
		{
			long cellIndex = RangeImpl.GetCellIndex(i, rowIndex);
			if (m_dicRecordsCells.Contains(cellIndex))
			{
				SizeF sizeF2 = MeasureCell(cellIndex, bAutoFitRows: true, richText, ignoreRotation: false, out bIsMergedAndWrapped);
				if (sizeF.Height < sizeF2.Height)
				{
					sizeF.Height = sizeF2.Height;
				}
				if (Range[rowIndex, i].CellStyle.Rotation > 0 && sizeF.Height < sizeF2.Width)
				{
					sizeF.Height = sizeF2.Width;
				}
			}
		}
		if (sizeF.Height == 0f)
		{
			sizeF.Height = (m_book.Styles["Normal"].Font as FontWrapper).Wrapped.MeasureString('0'.ToString()).Height;
		}
		double num = ApplicationImpl.ConvertFromPixel(sizeF.Height, MeasureUnits.Point);
		if (num > 409.5)
		{
			num = 409.5;
		}
		(UsedRange[rowIndex, firstColumn] as RangeImpl).SetRowHeight(num, bIsMergedAndWrapped);
	}

	internal void InnerSetRowHeight(int iRowIndex, double value, bool bIsBadFontHeight, MeasureUnits units, bool bRaiseEvents)
	{
		value = base.Application.ConvertUnits(value, units, MeasureUnits.Point);
		RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, iRowIndex - 1, bCreate: true);
		if (value == 0.0)
		{
			orCreateRow.IsHidden = true;
			return;
		}
		ushort num = (ushort)Math.Round(value * 20.0);
		if (orCreateRow.Height != num)
		{
			orCreateRow.Height = num;
			orCreateRow.IsBadFontHeight = bIsBadFontHeight;
			WorksheetHelper.AccessRow(this, iRowIndex);
			SetChanged();
		}
		if (bRaiseEvents)
		{
			RaiseRowHeightChangedEvent(iRowIndex, value);
		}
	}

	private bool IsRowEmpty(int iRowIndex)
	{
		return IsRowEmpty(iRowIndex, bCheckStyle: true);
	}

	private bool IsRowEmpty(int iRowIndex, bool bCheckStyle)
	{
		ParseData();
		if (iRowIndex < m_iFirstRow || iRowIndex > m_iLastRow)
		{
			return true;
		}
		int defaultXFIndex = m_book.DefaultXFIndex;
		for (int i = m_iFirstColumn; i <= m_iLastColumn; i++)
		{
			long cellIndex = RangeImpl.GetCellIndex(i, iRowIndex);
			if (!m_dicRecordsCells.Contains(cellIndex))
			{
				continue;
			}
			bool flag = true;
			ICellPositionFormat cellRecord = m_dicRecordsCells.GetCellRecord(cellIndex);
			if (bCheckStyle && cellRecord.TypeCode == TBIFFRecord.Blank)
			{
				int extendedFormatIndex = cellRecord.ExtendedFormatIndex;
				if (extendedFormatIndex == defaultXFIndex || extendedFormatIndex == 0)
				{
					flag = false;
				}
			}
			if (flag)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsColumnEmpty(int iColumnIndex)
	{
		return IsColumnEmpty(iColumnIndex, bIgnoreStyles: true);
	}

	private bool IsColumnEmpty(int iColumnIndex, bool bIgnoreStyles)
	{
		ParseData();
		if (iColumnIndex < 1 || iColumnIndex > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("iColumnIndex", "Value cannot be less 1 and greater than max column index.");
		}
		if (iColumnIndex < m_iFirstColumn || iColumnIndex > m_iLastColumn)
		{
			return true;
		}
		int defaultXFIndex = m_book.DefaultXFIndex;
		for (int i = m_iFirstRow; i <= m_iLastRow; i++)
		{
			long cellIndex = RangeImpl.GetCellIndex(iColumnIndex, i);
			if (!m_dicRecordsCells.Contains(cellIndex))
			{
				continue;
			}
			bool flag = true;
			ICellPositionFormat cellRecord = m_dicRecordsCells.GetCellRecord(cellIndex);
			if (bIgnoreStyles && cellRecord.TypeCode == TBIFFRecord.Blank)
			{
				int extendedFormatIndex = cellRecord.ExtendedFormatIndex;
				if (extendedFormatIndex == defaultXFIndex || extendedFormatIndex == 0)
				{
					flag = false;
				}
			}
			if (flag)
			{
				return false;
			}
		}
		return true;
	}

	private int ParseRange(IRange range, string strRowString, string separator, int i)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		if (strRowString == null)
		{
			throw new ArgumentNullException("strRowString");
		}
		if (separator == null)
		{
			throw new ArgumentNullException("separator");
		}
		int length = strRowString.Length;
		int length2 = separator.Length;
		bool flag = true;
		int num = i;
		while (flag && num < length)
		{
			if (strRowString[num] == '"' && num + 1 < length)
			{
				int num2 = strRowString.IndexOf('"', num + 1);
				num = ((num2 == -1) ? (num + 1) : (num2 + 1));
			}
			else if (string.CompareOrdinal(strRowString, num, separator, 0, length2) == 0)
			{
				flag = false;
			}
			else
			{
				num++;
			}
		}
		int num3 = num - i;
		string text = strRowString.Substring(i, num3);
		if (num3 > 1 && text[0] == '"' && text[num3 - 1] == '"')
		{
			text = text.Substring(1, num3 - 2);
		}
		text = text.Replace("\"\"", "\"");
		if (text.IndexOf('\n') >= 0)
		{
			range.WrapText = true;
		}
		if (text != null && text.Length > 1 && text[0] == '=' && (char.IsLetter(text, 1) || text[1] == '%' || text[1] == '('))
		{
			range.Value = text;
		}
		else
		{
			range.Text = text;
		}
		if (base.Application.PreserveCSVDataTypes)
		{
			if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
			{
				range.DateTime = result;
			}
			if (double.TryParse(text, out var result2))
			{
				range.Number = result2;
			}
		}
		return Math.Min(length, num + length2 - 1);
	}

	internal SizeF MeasureCell(IRange cell, bool bAutoFitRows, bool ignoreRotation)
	{
		long cellIndex = RangeImpl.GetCellIndex(cell.Column, cell.Row);
		return MeasureCell(cellIndex, bAutoFitRows, ignoreRotation);
	}

	internal SizeF MeasureCell(long cellIndex, bool bAutoFitRows, bool ignoreRotation)
	{
		RichTextString richText = new RichTextString(base.Application, this, isReadOnly: false, bCreateText: true);
		bool bIsMergedAndWrapped = false;
		return MeasureCell(cellIndex, bAutoFitRows, richText, ignoreRotation, out bIsMergedAndWrapped);
	}

	private SizeF MeasureCell(long cellIndex, bool bAutoFitRows, RichTextString richText, bool ignoreRotation, out bool bIsMergedAndWrapped)
	{
		ParseData();
		m_dicRecordsCells.FillRTFString(cellIndex, bAutoFitRows, richText);
		int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(cellIndex);
		int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(cellIndex);
		bool flag = false;
		string text = richText.Text;
		if (text == null || text.Length == 0)
		{
			bIsMergedAndWrapped = false;
			return new SizeF(0f, 0f);
		}
		if (m_mergedCells != null)
		{
			Rectangle rect = Rectangle.FromLTRB(columnFromCellIndex - 1, rowFromCellIndex - 1, columnFromCellIndex - 1, rowFromCellIndex - 1);
			MergeCellsRecord.MergedRegion mergedRegion = m_mergedCells[rect];
			if (mergedRegion != null && ((bAutoFitRows && (mergedRegion.RowFrom <= rowFromCellIndex - 1 || mergedRegion.RowTo >= rowFromCellIndex - 1)) || (!bAutoFitRows && (mergedRegion.ColumnFrom <= columnFromCellIndex - 1 || mergedRegion.ColumnTo >= columnFromCellIndex - 1))))
			{
				flag = true;
			}
		}
		_ = (m_book.Styles["Normal"].Font as FontWrapper).Wrapped;
		ExtendedFormatImpl extendedFormat = GetExtendedFormat(cellIndex);
		int rotation = extendedFormat.Rotation;
		SizeF sizeF = richText.StringSize;
		_ = extendedFormat.HorizontalAlignment;
		if (bAutoFitRows)
		{
			if (!flag && extendedFormat.WrapText)
			{
				int num = AutoFitManagerImpl.CalculateWrappedCell(extendedFormat, text, GetColumnWidthInPixels(columnFromCellIndex), base.AppImplementation);
				sizeF.Height = num;
			}
		}
		else
		{
			sizeF = UpdateAutofitByIndent(sizeF, extendedFormat);
			if (!ignoreRotation)
			{
				sizeF.Width = UpdateTextWidthOrHeightByRotation(sizeF, rotation, bUpdateHeight: false);
			}
		}
		bIsMergedAndWrapped = flag && extendedFormat.WrapText;
		return sizeF;
	}

	private Size WrapLine(IWorksheet sheet, IRichTextString rtf, int columnIndex)
	{
		RichTextString richTextString = rtf as RichTextString;
		string[] array = richTextString.Text.Split('\n');
		int num = 0;
		Size empty = Size.Empty;
		int columnWidthInPixels = sheet.GetColumnWidthInPixels(columnIndex);
		string[] array2 = array;
		foreach (string text in array2)
		{
			RichTextString richTextString2 = richTextString.Clone(richTextString.Parent) as RichTextString;
			string text2 = text.TrimEnd('\r');
			richTextString2.Substring(num, text2.Length);
			Size size = WrapSingleLine(text, columnWidthInPixels, richTextString2);
			empty.Height += size.Height;
			empty.Width = Math.Max(size.Width, empty.Width);
			num += text.Length + 1;
		}
		return empty;
	}

	private Size WrapSingleLine(string line, int availableWidth, RichTextString stringPart)
	{
		Size empty = Size.Empty;
		SizeF sizeF = stringPart.StringSize;
		if ((int)sizeF.Width > availableWidth)
		{
			sizeF = FitByWords(stringPart, availableWidth);
		}
		empty.Height += (int)sizeF.Height;
		empty.Width = Math.Max((int)sizeF.Width, empty.Width);
		return empty;
	}

	private Size FitByWords(RichTextString stringPart, int availableWidth)
	{
		RichTextString richTextString = (RichTextString)stringPart.Clone(stringPart.Parent);
		int currentIndex = 0;
		int num = 0;
		int length = stringPart.Text.Length;
		Size empty = Size.Empty;
		while (num < length)
		{
			stringPart = AddNextWord(richTextString, currentIndex, ref currentIndex);
			RichTextString richTextString2 = null;
			int num2 = -1;
			SizeF stringSize = stringPart.StringSize;
			while (stringSize.Width < (float)availableWidth && currentIndex < richTextString.Text.Length)
			{
				richTextString2 = stringPart;
				num2 = currentIndex;
				stringPart = AddNextWord(richTextString, num, ref currentIndex);
				stringSize = stringPart.StringSize;
			}
			if (stringSize.Width > (float)availableWidth)
			{
				stringPart = richTextString2;
				currentIndex = num2;
			}
			SizeF sizeF;
			if (stringPart != null)
			{
				sizeF = stringPart.StringSize;
			}
			else
			{
				currentIndex = num;
				sizeF = SplitByChars(richTextString, num, ref currentIndex, availableWidth);
			}
			empty.Width = Math.Max((int)sizeF.Width, empty.Width);
			empty.Height += (int)sizeF.Height;
			num = currentIndex;
			if (currentIndex == 0)
			{
				num++;
			}
		}
		return empty;
	}

	private SizeF SplitByChars(RichTextString originalString, int startIndex, ref int currentIndex, int availableWidth)
	{
		int length = originalString.Text.Length;
		Size size = Size.Empty;
		Size empty = Size.Empty;
		while (currentIndex < length && size.Width < availableWidth)
		{
			RichTextString obj = (RichTextString)originalString.Clone(originalString.Parent);
			obj.Substring(startIndex, currentIndex - startIndex + 1);
			empty = size;
			size = obj.StringSize.ToSize();
			if (size.Width > availableWidth)
			{
				size = empty;
				break;
			}
			currentIndex++;
		}
		return size;
	}

	private RichTextString AddNextWord(RichTextString originalString, int startIndex, ref int currentIndex)
	{
		RichTextString richTextString = (RichTextString)originalString.Clone(originalString.Parent);
		int num = richTextString.Text.IndexOfAny(new char[2] { '-', ' ' }, currentIndex);
		if (num < 0)
		{
			num = richTextString.Text.Length - 1;
		}
		richTextString.Substring(startIndex, num - startIndex + 1);
		currentIndex = num + 1;
		return richTextString;
	}

	private SizeF UpdateAutofitByIndent(SizeF curSize, ExtendedFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.HorizontalAlignment != OfficeHAlign.HAlignLeft && format.HorizontalAlignment != OfficeHAlign.HAlignRight && format.Rotation != 0 && format.IndentLevel == 0)
		{
			return curSize;
		}
		curSize.Width += format.IndentLevel * 12;
		return curSize;
	}

	private float UpdateTextWidthOrHeightByRotation(SizeF size, int rotation, bool bUpdateHeight)
	{
		if (rotation == 0)
		{
			if (!bUpdateHeight)
			{
				return size.Width;
			}
			return size.Height;
		}
		if (rotation == 90 || rotation == 180)
		{
			if (!bUpdateHeight)
			{
				return size.Height;
			}
			return size.Width;
		}
		if (rotation > 90)
		{
			rotation -= 90;
		}
		if (bUpdateHeight)
		{
			rotation = 90 - rotation;
		}
		float num = (float)Math.Sin((float)Math.PI / 180f * (float)rotation) * size.Height;
		return (float)Math.Cos((float)Math.PI / 180f * (float)rotation) * size.Width + num;
	}

	private FontImpl GetFontByExtendedFormatIndex(ICellPositionFormat cellFormat, out int rotation)
	{
		ExtendedFormatsCollection innerExtFormats = m_book.InnerExtFormats;
		if (innerExtFormats.Count <= cellFormat.ExtendedFormatIndex)
		{
			throw new ArgumentException("cellFormat");
		}
		ExtendedFormatImpl extendedFormatImpl = innerExtFormats[cellFormat.ExtendedFormatIndex];
		rotation = extendedFormatImpl.Rotation;
		return (FontImpl)m_book.InnerFonts[extendedFormatImpl.FontIndex];
	}

	protected override void CopyOptions(WorksheetBaseImpl sourceSheet)
	{
		base.CopyOptions(sourceSheet);
		WorksheetImpl worksheetImpl = (WorksheetImpl)sourceSheet;
		IsRowColumnHeadersVisible = worksheetImpl.IsRowColumnHeadersVisible;
		IsStringsPreserved = worksheetImpl.IsStringsPreserved;
		IsGridLinesVisible = worksheetImpl.IsGridLinesVisible;
		m_pane = (PaneRecord)CloneUtils.CloneCloneable(worksheetImpl.m_pane);
	}

	protected override void OnRealIndexChanged(int iOldIndex)
	{
		if (m_names != null)
		{
			m_names.SetSheetIndex(base.RealIndex);
		}
	}

	private void OnInsertRowColumnComplete(int iRowIndex, int iRowCount, bool bRow)
	{
	}

	private SizeF UpdateAutoFitByAutoFilter(SizeF size, ExtendedFormatImpl format, CellRecordCollection col, long cellIndex)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (col == null)
		{
			throw new ArgumentNullException("col");
		}
		OfficeHAlign horizontalAlignment = format.HorizontalAlignment;
		int rotation = format.Rotation;
		switch (horizontalAlignment)
		{
		case OfficeHAlign.HAlignRight:
		case OfficeHAlign.HAlignFill:
		case OfficeHAlign.HAlignCenterAcrossSelection:
			return size;
		case OfficeHAlign.HAlignJustify:
		case OfficeHAlign.HAlignDistributed:
			if (rotation > 0 && rotation < 90)
			{
				size.Width += 16f;
			}
			return size;
		case OfficeHAlign.HAlignLeft:
		case OfficeHAlign.HAlignCenter:
			size.Width += ((horizontalAlignment == OfficeHAlign.HAlignLeft) ? 16 : 32);
			return size;
		default:
			return UpdateAutoFilterForGeneralAllignment(size, rotation, col, cellIndex);
		}
	}

	private SizeF UpdateAutoFilterForGeneralAllignment(SizeF size, int iRot, CellRecordCollection col, long cellIndex)
	{
		if (col == null)
		{
			throw new ArgumentNullException("col");
		}
		if (m_dicRecordsCells.ContainFormulaBoolOrError(cellIndex) || m_dicRecordsCells.ContainBoolOrError(cellIndex))
		{
			size.Width += 32f;
			return size;
		}
		if ((iRot > 0 && iRot < 90) || iRot >= 180)
		{
			size.Width += 16f;
			return size;
		}
		if (!m_dicRecordsCells.ContainFormulaNumber(cellIndex) && !m_dicRecordsCells.ContainNumber(cellIndex) && iRot == 0)
		{
			size.Width += 16f;
		}
		return size;
	}

	private void CreateMigrantRange()
	{
		m_migrantRange = new MigrantRangeImpl(base.Application, this);
	}

	private IStyle GetDefaultOutlineStyle(IDictionary dicOutlines, int iIndex)
	{
		if (dicOutlines == null)
		{
			throw new ArgumentNullException("dicOutlines");
		}
		int iXFIndex = ((int?)((IOutline)dicOutlines[iIndex])?.ExtendedFormatIndex) ?? m_book.DefaultXFIndex;
		return new ExtendedFormatWrapper(m_book, iXFIndex);
	}

	private int SetDefaultRowColumnStyle(int iIndex, int iEndIndex, IStyle defaultStyle, IDictionary dicOutlines, OutlineDelegate createOutline, bool bIsRow)
	{
		ParseData();
		int num = ConvertStyleToCorrectIndex(defaultStyle);
		for (int i = iIndex; i <= iEndIndex; i++)
		{
			(dicOutlines.Contains(i) ? ((IOutline)dicOutlines[i]) : createOutline(i)).ExtendedFormatIndex = (ushort)num;
		}
		return num;
	}

	private int SetDefaultRowColumnStyle(int iIndex, int iEndIndex, IStyle defaultStyle, IList outlines, OutlineDelegate createOutline, bool bIsRow)
	{
		ParseData();
		int num = ConvertStyleToCorrectIndex(defaultStyle);
		for (int i = iIndex; i <= iEndIndex; i++)
		{
			((outlines[i] != null) ? ((IOutline)outlines[i]) : createOutline(i)).ExtendedFormatIndex = (ushort)num;
			SetCellStyle(i, (ushort)num);
		}
		return num;
	}

	private int ConvertStyleToCorrectIndex(IStyle style)
	{
		if (style == null)
		{
			throw new ArgumentNullException("defaultStyle");
		}
		int xFormatIndex = ((IXFIndex)style).XFormatIndex;
		if (xFormatIndex == int.MinValue)
		{
			throw new ArgumentException("defaultStyle");
		}
		return m_book.InnerExtFormats[xFormatIndex].CreateChildFormat().Index;
	}

	private IOutline CreateColumnOutline(int iColumnIndex)
	{
		ParseData();
		if (iColumnIndex < 1 || iColumnIndex > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("iColumnIndex", "Column index is out of range.");
		}
		ColumnInfoRecord columnInfoRecord = BiffRecordFactory.GetRecord(TBIFFRecord.ColumnInfo) as ColumnInfoRecord;
		columnInfoRecord.FirstColumn = (ushort)(iColumnIndex - 1);
		columnInfoRecord.LastColumn = (ushort)(iColumnIndex - 1);
		columnInfoRecord.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
		WorksheetHelper.AccessColumn(this, iColumnIndex);
		m_arrColumnInfo[iColumnIndex] = columnInfoRecord;
		return columnInfoRecord;
	}

	private void CopyStylesAfterInsert(int iIndex, int iCount, OfficeInsertOptions options, bool bRow)
	{
		int indexForStyleCopy = GetIndexForStyleCopy(iIndex, iCount, options);
		int num;
		int num2;
		if (!bRow)
		{
			num = m_iFirstRow;
			num2 = m_iLastRow;
		}
		else if (m_iFirstColumn == int.MaxValue)
		{
			num = -1;
			num2 = -1;
		}
		else
		{
			num = m_iFirstColumn;
			num2 = m_iLastColumn;
		}
		RowStorage rowStorage = null;
		ColumnInfoRecord sourceColumn = null;
		if (indexForStyleCopy != -1)
		{
			if (bRow)
			{
				rowStorage = WorksheetHelper.GetOrCreateRow(this, indexForStyleCopy - 1, bCreate: false);
			}
			else
			{
				sourceColumn = m_arrColumnInfo[indexForStyleCopy];
			}
			if (num > 0)
			{
				for (int i = num; i <= num2; i++)
				{
					long key = (bRow ? RangeImpl.GetCellIndex(i, indexForStyleCopy) : RangeImpl.GetCellIndex(indexForStyleCopy, i));
					ICellPositionFormat cellRecord = m_dicRecordsCells.GetCellRecord(key);
					if (cellRecord != null && cellRecord.ExtendedFormatIndex != m_book.DefaultXFIndex && (rowStorage == null || cellRecord.ExtendedFormatIndex != rowStorage.ExtendedFormatIndex))
					{
						int j = iIndex;
						for (int num3 = iIndex + iCount; j < num3; j++)
						{
							key = (bRow ? RangeImpl.GetCellIndex(i, j) : RangeImpl.GetCellIndex(j, i));
							BlankRecord blankRecord = (BlankRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Blank);
							blankRecord.Row = (bRow ? j : i) - 1;
							blankRecord.Column = (bRow ? i : j) - 1;
							blankRecord.ExtendedFormatIndex = cellRecord.ExtendedFormatIndex;
							m_dicRecordsCells.SetCellRecord(key, blankRecord);
						}
					}
				}
			}
		}
		if (bRow)
		{
			int k = iIndex;
			for (int num4 = iIndex + iCount; k < num4; k++)
			{
				CopyRowColumnSettings(rowStorage, sourceColumn, bRow, indexForStyleCopy, k, options);
			}
		}
	}

	private void CopyRowColumnSettings(RowStorage sourceRow, ColumnInfoRecord sourceColumn, bool bRow, int iSourceIndex, int iCurIndex, OfficeInsertOptions options)
	{
		if (options == OfficeInsertOptions.FormatDefault)
		{
			if (bRow)
			{
				RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, iCurIndex - 1, bCreate: false);
				if (orCreateRow != null)
				{
					orCreateRow.SetDefaultRowOptions();
					orCreateRow.Height = (ushort)base.AppImplementation.StandardHeightInRowUnits;
					orCreateRow.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
				}
				return;
			}
			ColumnInfoRecord columnInfoRecord = m_arrColumnInfo[iCurIndex];
			if (columnInfoRecord != null)
			{
				ushort firstColumn = (columnInfoRecord.LastColumn = (ushort)(iCurIndex - 1));
				columnInfoRecord.FirstColumn = firstColumn;
				columnInfoRecord.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
				columnInfoRecord.SetDefaultOptions();
			}
		}
		else
		{
			if (iSourceIndex == -1)
			{
				return;
			}
			if (bRow)
			{
				RowStorage orCreateRow2 = WorksheetHelper.GetOrCreateRow(this, iCurIndex - 1, sourceRow != null);
				if (sourceRow == null && orCreateRow2 != null)
				{
					orCreateRow2.Height = (ushort)base.AppImplementation.StandardHeightInRowUnits;
					orCreateRow2.SetDefaultRowOptions();
				}
				else if (sourceRow != null)
				{
					orCreateRow2.CopyRowRecordFrom(sourceRow);
				}
			}
			else
			{
				ColumnInfoRecord columnInfoRecord2 = (ColumnInfoRecord)CloneUtils.CloneCloneable(sourceColumn);
				m_arrColumnInfo[iCurIndex] = columnInfoRecord2;
				if (columnInfoRecord2 != null)
				{
					ushort firstColumn = (columnInfoRecord2.LastColumn = (ushort)(iCurIndex - 1));
					columnInfoRecord2.FirstColumn = firstColumn;
				}
			}
		}
	}

	private int GetIndexForStyleCopy(int iIndex, int iCount, OfficeInsertOptions options)
	{
		iIndex = options switch
		{
			OfficeInsertOptions.FormatAsBefore => iIndex - 1, 
			OfficeInsertOptions.FormatAsAfter => iIndex + iCount, 
			_ => -1, 
		};
		return iIndex;
	}

	private OfficeFormatType GetFormatType(int iRow, int iColumn, bool bUseDefaultStyle)
	{
		int index;
		if (bUseDefaultStyle)
		{
			ColumnInfoRecord columnInfoRecord = m_arrColumnInfo[iColumn];
			if (columnInfoRecord == null)
			{
				return OfficeFormatType.General;
			}
			index = columnInfoRecord.ExtendedFormatIndex;
		}
		else
		{
			index = ((int?)m_dicRecordsCells.GetCellRecord(iRow, iColumn)?.ExtendedFormatIndex) ?? m_book.DefaultXFIndex;
		}
		int numberFormatIndex = m_book.InnerExtFormats[index].NumberFormatIndex;
		return m_book.InnerFormats[numberFormatIndex].GetFormatType(1.0);
	}

	private Type GetType(ExcelExportType exportType, bool preserveOLEDate)
	{
		switch (exportType)
		{
		case ExcelExportType.DateTime:
			if (!preserveOLEDate)
			{
				return typeof(DateTime);
			}
			return typeof(double);
		case ExcelExportType.Bool:
			return typeof(bool);
		case ExcelExportType.Number:
			return typeof(double);
		case ExcelExportType.Text:
		case ExcelExportType.Error:
		case ExcelExportType.Formula:
			return typeof(string);
		default:
			throw new ArgumentOutOfRangeException("exportType");
		}
	}

	internal string GetValue(ICellPositionFormat cell, bool preserveOLEDate)
	{
		if (cell == null)
		{
			return string.Empty;
		}
		object obj;
		switch (cell.TypeCode)
		{
		case TBIFFRecord.Blank:
			obj = string.Empty;
			break;
		case TBIFFRecord.BoolErr:
		{
			BoolErrRecord boolErrRecord = (BoolErrRecord)cell;
			int boolOrError = boolErrRecord.BoolOrError;
			obj = (boolErrRecord.IsErrorCode ? FormulaUtil.ErrorCodeToName[boolOrError] : (boolErrRecord.BoolOrError != 0).ToString().ToUpper());
			break;
		}
		case TBIFFRecord.Formula:
		{
			FormulaRecord formulaRecord = (FormulaRecord)cell;
			Ptg[] parsedExpression = formulaRecord.ParsedExpression;
			obj = ((!HasArrayFormula(parsedExpression)) ? GetFormula(cell.Row, cell.Column, parsedExpression, bR1C1: false, m_book.FormulaUtil, isForSerialization: false) : GetFormulaArray(formulaRecord));
			break;
		}
		case TBIFFRecord.Number:
		case TBIFFRecord.RK:
		{
			double doubleValue = ((IDoubleValue)cell).DoubleValue;
			int extendedFormatIndex = cell.ExtendedFormatIndex;
			int numberFormatIndex = m_book.InnerExtFormats[extendedFormatIndex].NumberFormatIndex;
			FormatImpl formatImpl = m_book.InnerFormats[numberFormatIndex];
			obj = ((formatImpl.FormatType != OfficeFormatType.DateTime) ? ((object)doubleValue) : (preserveOLEDate ? ((object)CalcEngineHelper.FromOADate(doubleValue).ToOADate()) : (formatImpl.IsTimeFormat(doubleValue) ? CalcEngineHelper.FromOADate(doubleValue).TimeOfDay.ToString() : (formatImpl.IsDateFormat(doubleValue) ? CalcEngineHelper.FromOADate(doubleValue).Date.ToString() : ((object)CalcEngineHelper.FromOADate(doubleValue))))));
			break;
		}
		case TBIFFRecord.LabelSST:
		{
			LabelSSTRecord labelSSTRecord = (LabelSSTRecord)cell;
			object sSTContentByIndex = m_book.InnerSST.GetSSTContentByIndex(labelSSTRecord.SSTIndex);
			obj = ((sSTContentByIndex is TextWithFormat textWithFormat) ? textWithFormat.Text : sSTContentByIndex);
			break;
		}
		case TBIFFRecord.String:
			obj = ((StringRecord)cell).Value;
			break;
		case TBIFFRecord.Label:
			obj = ((LabelRecord)cell).Label;
			break;
		default:
			throw new ArgumentException("Cannot recognize cell type.");
		}
		return obj.ToString();
	}

	private void UpdateOutlineAfterXFRemove(ICollection dictOutline, IDictionary dictFormats)
	{
		foreach (IOutline item in dictOutline)
		{
			if (item != null)
			{
				int extendedFormatIndex = item.ExtendedFormatIndex;
				if (dictFormats.Contains(extendedFormatIndex))
				{
					extendedFormatIndex = (int)dictFormats[extendedFormatIndex];
					item.ExtendedFormatIndex = (ushort)extendedFormatIndex;
				}
			}
		}
	}

	private IRange[] ConvertCellListIntoRange(List<long> arrIndexes)
	{
		if (arrIndexes == null || arrIndexes.Count == 0)
		{
			return null;
		}
		int count = arrIndexes.Count;
		IRange[] array = new IRange[count];
		for (int i = 0; i < count; i++)
		{
			long index = arrIndexes[i];
			int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(index);
			int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(index);
			array[i] = this[rowFromCellIndex, columnFromCellIndex];
		}
		return array;
	}

	private IRange FindValueForNumber(BiffRecordRaw record, double findValue, bool bIsNumber, bool bIsFormulaValue)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		double num = double.MinValue;
		ICellPositionFormat cellPositionFormat = (ICellPositionFormat)record;
		if (bIsNumber)
		{
			if (record is NumberRecord)
			{
				num = ((NumberRecord)record).Value;
			}
			if (record is RKRecord)
			{
				num = ((RKRecord)record).RKNumber;
			}
		}
		if (bIsFormulaValue && record is FormulaRecord)
		{
			num = ((FormulaRecord)record).Value;
		}
		if (num != findValue)
		{
			return null;
		}
		return Range[cellPositionFormat.Row + 1, cellPositionFormat.Column + 1];
	}

	private IRange FindValueForByteOrError(BoolErrRecord boolError, byte findValue, bool bIsError)
	{
		if (boolError == null)
		{
			throw new ArgumentNullException("boolError");
		}
		if (bIsError == boolError.IsErrorCode && boolError.BoolOrError == findValue)
		{
			return Range[boolError.Row + 1, boolError.Column + 1];
		}
		return null;
	}

	protected internal IRange InnerGetCell(int column, int row)
	{
		return InnerGetCell(column, row, GetXFIndex(row, column));
	}

	protected internal IRange InnerGetCell(int column, int row, int iXFIndex)
	{
		ParseData();
		IRange range = m_dicRecordsCells.GetRange(row, column);
		if (range == null)
		{
			if (!(m_dicRecordsCells.GetCellRecord(row, column) is BiffRecordRaw record))
			{
				RangeImpl rangeImpl = base.AppImplementation.CreateRange(this, column, row, column, row);
				if (rangeImpl.ExtendedFormatIndex != iXFIndex)
				{
					rangeImpl.ExtendedFormatIndex = (ushort)iXFIndex;
				}
				m_dicRecordsCells.SetRange(row, column, rangeImpl);
				range = rangeImpl;
			}
			else
			{
				range = ConvertRecordToRange(record);
			}
		}
		return range;
	}

	protected internal IStyle InnerGetCellStyle(int column, int row, int iXFIndex, RangeImpl rangeImpl)
	{
		return RangeImpl.CreateTempStyleWrapperWithoutRange(rangeImpl, iXFIndex);
	}

	private IRange ConvertRecordToRange(BiffRecordRaw record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		ParseData();
		RangeImpl rangeImpl = base.AppImplementation.CreateRange(this, record, bIgnoreStyles: false);
		long cellIndex = rangeImpl.CellIndex;
		m_dicRecordsCells.SetRange(cellIndex, rangeImpl);
		return rangeImpl;
	}

	protected void UpdateFirstLast(int iRowIndex, int iColumnIndex)
	{
		ParseData();
		m_iFirstColumn = ((m_iFirstColumn > iColumnIndex || m_iFirstColumn == int.MaxValue) ? ((ushort)iColumnIndex) : m_iFirstColumn);
		m_iLastColumn = ((m_iLastColumn < iColumnIndex || m_iLastColumn == int.MaxValue) ? ((ushort)iColumnIndex) : m_iLastColumn);
		m_iFirstRow = ((m_iFirstRow > iRowIndex || m_iFirstRow < 0) ? iRowIndex : m_iFirstRow);
		m_iLastRow = ((m_iLastRow < iRowIndex || m_iLastRow < 0) ? iRowIndex : m_iLastRow);
	}

	protected internal void InnerSetCell(int column, int row, RangeImpl range)
	{
		if (!range.IsSingleCell)
		{
			throw new ArgumentException("Range must represent single cell");
		}
		ParseData();
		m_dicRecordsCells.SetRange(row, column, range);
	}

	[CLSCompliant(false)]
	protected internal void InnerSetCell(long cellIndex, BiffRecordRaw record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		ParseData();
		ICellPositionFormat cellPositionFormat = (ICellPositionFormat)record;
		WorksheetHelper.AccessColumn(this, cellPositionFormat.Column + 1);
		WorksheetHelper.AccessRow(this, cellPositionFormat.Row + 1);
		m_dicRecordsCells.SetCellRecord(cellIndex, cellPositionFormat);
	}

	[CLSCompliant(false)]
	protected internal void InnerSetCell(int iColumn, int iRow, BiffRecordRaw record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		ParseData();
		m_dicRecordsCells.SetCellRecord(iRow, iColumn, record as ICellPositionFormat);
	}

	protected internal void InnerGetDimensions(out int left, out int top, out int right, out int bottom)
	{
		ParseData();
		left = m_iFirstColumn;
		right = m_iLastColumn;
		top = m_iFirstRow;
		bottom = m_iLastRow;
	}

	protected internal void InnerGetColumnDimensions(int column, out int top, out int bottom)
	{
		ParseData();
		int num = -1;
		int num2 = -1;
		int i = FirstRow;
		for (int lastRow = LastRow; i <= lastRow; i++)
		{
			long cellIndex = RangeImpl.GetCellIndex(column, i);
			if (m_dicRecordsCells.Contains(cellIndex))
			{
				if (num < i)
				{
					num = i;
				}
				if (num2 == -1)
				{
					num2 = i;
				}
			}
		}
		top = num2;
		bottom = num;
	}

	internal void UpdateLabelSSTIndexes(Dictionary<int, int> dictUpdatedIndexes, IncreaseIndex method)
	{
		ParseData();
		m_dicRecordsCells.UpdateLabelSSTIndexes(dictUpdatedIndexes, method);
	}

	private void InsertIntoDefaultColumns(int iColumnIndex, int iColumnCount, OfficeInsertOptions insertOptions)
	{
		ParseData();
		ColumnInfoRecord columnInfoRecord = null;
		for (int num = m_book.MaxColumnCount; num > iColumnIndex + iColumnCount - 1; num--)
		{
			int num2 = num - iColumnCount;
			columnInfoRecord = m_arrColumnInfo[num2];
			if (columnInfoRecord != null)
			{
				ColumnInfoRecord columnInfoRecord2 = columnInfoRecord;
				ushort firstColumn = (columnInfoRecord.LastColumn = (ushort)(num - 1));
				columnInfoRecord2.FirstColumn = firstColumn;
			}
			m_arrColumnInfo[num] = columnInfoRecord;
			m_arrColumnInfo[num2] = null;
		}
		columnInfoRecord = null;
		switch (insertOptions)
		{
		case OfficeInsertOptions.FormatAsBefore:
			columnInfoRecord = m_arrColumnInfo[iColumnIndex - 1];
			break;
		case OfficeInsertOptions.FormatAsAfter:
			columnInfoRecord = m_arrColumnInfo[iColumnIndex + iColumnCount];
			break;
		}
		if (columnInfoRecord != null)
		{
			int i = iColumnIndex;
			for (int num4 = iColumnIndex + iColumnCount; i < num4; i++)
			{
				columnInfoRecord = (ColumnInfoRecord)columnInfoRecord.Clone();
				ColumnInfoRecord columnInfoRecord3 = columnInfoRecord;
				ushort firstColumn = (columnInfoRecord.LastColumn = (ushort)(i - 1));
				columnInfoRecord3.FirstColumn = firstColumn;
				m_arrColumnInfo[i] = columnInfoRecord;
			}
		}
	}

	private void RemoveFromDefaultColumns(int iColumnIndex, int iColumnCount, OfficeInsertOptions insertOptions)
	{
		ParseData();
		ColumnInfoRecord columnInfoRecord = null;
		for (int i = iColumnIndex; i <= m_book.MaxColumnCount - iColumnCount; i++)
		{
			int num = i + iColumnCount;
			columnInfoRecord = m_arrColumnInfo[num];
			if (columnInfoRecord != null)
			{
				ColumnInfoRecord columnInfoRecord2 = columnInfoRecord;
				ushort firstColumn = (columnInfoRecord.LastColumn = (ushort)(i - 1));
				columnInfoRecord2.FirstColumn = firstColumn;
			}
			m_arrColumnInfo[i] = columnInfoRecord;
		}
		int maxColumnCount = m_book.MaxColumnCount;
		columnInfoRecord = (ColumnInfoRecord)CloneUtils.CloneCloneable(m_arrColumnInfo[maxColumnCount - 1]);
		m_arrColumnInfo[maxColumnCount] = columnInfoRecord;
		if (columnInfoRecord != null)
		{
			ColumnInfoRecord columnInfoRecord3 = columnInfoRecord;
			ushort firstColumn = (columnInfoRecord.LastColumn = (ushort)(maxColumnCount - 1));
			columnInfoRecord3.FirstColumn = firstColumn;
		}
	}

	private void GetRangeCoordinates(ref int firstRow, ref int firstColumn, ref int lastRow, ref int lastColumn)
	{
		if (!m_bUsedRangeIncludesFormatting)
		{
			while (firstRow <= lastRow && IsRowBlankOnly(firstRow))
			{
				firstRow++;
			}
			while (lastRow >= firstRow && IsRowBlankOnly(lastRow))
			{
				lastRow--;
			}
			while (firstColumn <= lastColumn && IsColumnBlankOnly(firstColumn))
			{
				firstColumn++;
			}
			while (lastColumn >= firstColumn && IsColumnBlankOnly(lastColumn))
			{
				lastColumn--;
			}
		}
	}

	private bool IsRowBlankOnly(int rowIndex)
	{
		bool result = true;
		for (int i = m_iFirstColumn; i <= m_iLastColumn; i++)
		{
			if (GetCellType(rowIndex, i, bNeedFormulaSubType: false) != 0)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private bool IsColumnBlankOnly(int columnIndex)
	{
		bool result = true;
		for (int i = m_iFirstRow; i <= m_iLastRow; i++)
		{
			if (GetCellType(i, columnIndex, bNeedFormulaSubType: false) != 0)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void CreateUsedRange(int firstRow, int firstColumn, int lastRow, int lastColumn)
	{
		if (m_rngUsed != null && m_rngUsed.FirstColumn == firstColumn && m_rngUsed.FirstRow == firstRow && m_rngUsed.LastColumn == lastColumn && m_rngUsed.LastRow == lastRow)
		{
			m_rngUsed.ResetCells();
			return;
		}
		if (m_rngUsed != null)
		{
			m_rngUsed.Dispose();
		}
		m_rngUsed = base.AppImplementation.CreateRange(this, firstColumn, firstRow, lastColumn, lastRow);
	}

	internal bool ClearExceptFirstCell(RangeImpl rangeImpl, bool isClearCells)
	{
		bool flag = false;
		bool flag2 = false;
		int num = rangeImpl.Column - 1;
		int num2 = rangeImpl.LastColumn - 1;
		int i = rangeImpl.Row - 1;
		for (int lastRow = rangeImpl.LastRow; i < lastRow; i++)
		{
			RowStorage rowStorage = m_dicRecordsCells.Table.Rows[i];
			if (rowStorage == null)
			{
				continue;
			}
			if (!flag)
			{
				int num3 = rowStorage.FindFirstCell(num, num2);
				if (num3 > num2)
				{
					continue;
				}
				flag = true;
				if (isClearCells)
				{
					rowStorage.Remove(num3 + 1, num2, base.AppImplementation.RowStorageAllocationBlockSize);
				}
				num3 = Math.Max(rowStorage.FirstColumn, num3 + 1);
				if (!flag2 && rangeImpl[rangeImpl.Row, rangeImpl.Column].Value != rangeImpl[i + 1, num3].Value)
				{
					rangeImpl[rangeImpl.Row, rangeImpl.Column].Value = rangeImpl[i + 1, num3].Value;
					rangeImpl[rangeImpl.Row, rangeImpl.Column].CellStyle = rangeImpl[i + 1, num3].CellStyle;
					rangeImpl[i + 1, num3].Value = null;
					flag2 = true;
					if (!isClearCells)
					{
						return true;
					}
				}
			}
			else if (isClearCells)
			{
				rowStorage.Remove(num, num2, base.AppImplementation.RowStorageAllocationBlockSize);
			}
		}
		return flag;
	}

	protected override OfficeSheetProtection PrepareProtectionOptions(OfficeSheetProtection options)
	{
		return options &= ~OfficeSheetProtection.Content;
	}

	protected override void PrepareVariables(OfficeParseOptions options, bool bSkipParsing)
	{
		base.PrepareVariables(options, bSkipParsing);
		if (m_arrAutoFilter != null)
		{
			m_arrAutoFilter.Clear();
		}
		if (m_arrDConRecords != null)
		{
			m_arrDConRecords.Clear();
		}
		m_iDValPos = -1;
		m_iCondFmtPos = -1;
		m_iPivotStartIndex = -1;
		m_iHyperlinksStartIndex = -1;
	}

	[CLSCompliant(false)]
	protected override void ParseRecord(BiffRecordRaw raw, bool bIgnoreStyles, Dictionary<int, int> hashNewXFormatIndexes)
	{
		if (m_book.HasDuplicatedNames && raw.TypeCode == TBIFFRecord.Formula)
		{
			FormulaRecord formula = (FormulaRecord)raw;
			UpdateDuplicatedNameIndexes(formula);
		}
		if (base.IsSkipParsing)
		{
			return;
		}
		if (UtilityMethods.IndexOf(s_arrAutofilterRecord, raw.TypeCode) >= 0)
		{
			AutoFilterRecords.Add(raw);
		}
		ICellPositionFormat cellPositionFormat = raw as ICellPositionFormat;
		if (cellPositionFormat != null && bIgnoreStyles)
		{
			cellPositionFormat.ExtendedFormatIndex = (ushort)GetNewXFormatIndex(cellPositionFormat.ExtendedFormatIndex, hashNewXFormatIndexes);
		}
		switch (raw.TypeCode)
		{
		case TBIFFRecord.Index:
			m_index = (IndexRecord)raw;
			break;
		case TBIFFRecord.ColumnInfo:
			ParseColumnInfo((ColumnInfoRecord)raw, bIgnoreStyles);
			break;
		case TBIFFRecord.Row:
			ParseRowRecord((RowRecord)raw, bIgnoreStyles);
			break;
		case TBIFFRecord.DefaultColWidth:
		{
			DefaultColWidthRecord defaultColWidthRecord = (DefaultColWidthRecord)raw;
			if (defaultColWidthRecord.Width != 8)
			{
				m_dStandardColWidth = m_book.WidthToFileWidth((int)defaultColWidthRecord.Width);
			}
			break;
		}
		case TBIFFRecord.MergeCells:
			MergeCells.AddMerge((MergeCellsRecord)raw);
			break;
		case TBIFFRecord.Pane:
			m_pane = (PaneRecord)raw;
			break;
		case TBIFFRecord.Selection:
			m_arrSelections.Add((SelectionRecord)raw);
			break;
		case TBIFFRecord.CondFMT:
			if (!KeepRecord)
			{
				KeepRecord = true;
				m_arrRecords.Add(raw);
			}
			if (m_iCondFmtPos < 0)
			{
				m_iCondFmtPos = m_arrRecords.Count - 1;
			}
			break;
		case TBIFFRecord.CondFMT12:
			if (!KeepRecord)
			{
				KeepRecord = true;
				m_arrRecords.Add(raw);
			}
			if (m_iCondFmtPos < 0)
			{
				m_iCondFmtPos = m_arrRecords.Count - 1;
			}
			break;
		case TBIFFRecord.DVal:
			if (!KeepRecord)
			{
				KeepRecord = true;
				m_arrRecords.Add(raw);
			}
			((DValRecord)raw).IsDataCached = false;
			if (m_iDValPos < 0)
			{
				m_iDValPos = m_arrRecords.Count - 1;
			}
			break;
		case TBIFFRecord.HLink:
			if (!KeepRecord)
			{
				KeepRecord = true;
				m_arrRecords.Add(raw);
			}
			if (m_iHyperlinksStartIndex < 0)
			{
				m_iHyperlinksStartIndex = m_arrRecords.Count - 1;
			}
			break;
		case TBIFFRecord.Sort:
			SortRecords.Add(raw);
			break;
		case TBIFFRecord.Note:
			AddNote(raw as NoteRecord);
			break;
		case TBIFFRecord.DCON:
			DConRecords.Add(raw);
			break;
		case TBIFFRecord.CustomProperty:
			if (!KeepRecord)
			{
				KeepRecord = true;
				m_arrRecords.Add(raw);
			}
			if (m_iCustomPropertyStartIndex < 0)
			{
				m_iCustomPropertyStartIndex = m_arrRecords.Count - 1;
			}
			break;
		case TBIFFRecord.PivotString:
		case TBIFFRecord.ExternalSourceInfo:
		case TBIFFRecord.Qsi:
		case TBIFFRecord.QsiSXTag:
		case TBIFFRecord.DBQueryExt:
		case TBIFFRecord.ExtString:
		case TBIFFRecord.TextQuery:
		case TBIFFRecord.Qsir:
		case TBIFFRecord.Qsif:
		case TBIFFRecord.OleDbConn:
		case TBIFFRecord.PivotViewAdditionalInfo:
		case TBIFFRecord.Feature12:
			PreserveExternalConnection.Add(raw);
			break;
		case (TBIFFRecord)2161:
		case (TBIFFRecord)2162:
		case (TBIFFRecord)2167:
			if (m_tableRecords == null)
			{
				m_tableRecords = new List<BiffRecordRaw>();
			}
			m_tableRecords.Add(raw);
			break;
		}
	}

	private void UpdateDuplicatedNameIndexes(FormulaRecord formula)
	{
		if (formula == null)
		{
			throw new ArgumentNullException("formula");
		}
		Ptg[] parsedExpression = formula.ParsedExpression;
		int i = 0;
		for (int num = parsedExpression.Length; i < num; i++)
		{
			Ptg ptg = parsedExpression[i];
			if (FormulaUtil.IndexOf(FormulaUtil.NameXCodes, ptg.TokenCode) != -1)
			{
				NameXPtg nameXPtg = (NameXPtg)ptg;
				int refIndex = nameXPtg.RefIndex;
				int nameIndex = nameXPtg.NameIndex;
				if (!m_book.IsLocalReference(refIndex))
				{
					ExternWorkbookImpl externWorkbookImpl = m_book.ExternWorkbooks[refIndex];
					nameXPtg.NameIndex = (ushort)(externWorkbookImpl.GetNewIndex(nameIndex - 1) + 1);
				}
			}
		}
	}

	private int GetNewXFormatIndex(int iXFIndex, Dictionary<int, int> hashNewXFormatIndexes)
	{
		if (hashNewXFormatIndexes == null)
		{
			throw new ArgumentNullException("hashNewXFormatIndexes");
		}
		int formatIndex = m_book.InnerExtFormatRecords[iXFIndex].FormatIndex;
		return hashNewXFormatIndexes[formatIndex];
	}

	public void Parse(TextReader streamToRead, string separator, int row, int column, bool isValid)
	{
		if (streamToRead == null)
		{
			throw new ArgumentNullException("streamToRead");
		}
		if (separator == null)
		{
			throw new ArgumentNullException("separator");
		}
		if (separator.Length == 0)
		{
			throw new ArgumentException("separator");
		}
		int num = row;
		StringBuilder builder = new StringBuilder();
		int num2 = column;
		CustomHeight = false;
		while (streamToRead.Peek() >= 0)
		{
			string text = ReadCellValue(streamToRead, separator, builder, isValid);
			bool num3 = text.EndsWith("\n");
			if (num3)
			{
				text = text.Remove(text.Length - 1);
			}
			if (text.Length > 0)
			{
				ParseRange(Range[num, num2], text, separator, 0);
			}
			if (num3)
			{
				num++;
				num2 = column;
			}
			else
			{
				num2++;
			}
		}
	}

	private static string ReadCellValue(TextReader reader, string separator, StringBuilder builder, bool isValid)
	{
		builder.Length = 0;
		while (true)
		{
			int num = reader.Read();
			if (num < 0)
			{
				break;
			}
			char c = (char)num;
			switch (c)
			{
			case '\r':
				continue;
			case '"':
				builder.Append(c);
				ReadToChar(reader, c, builder, separator, isValid);
				continue;
			case '\n':
				builder.Append(c);
				break;
			default:
				builder.Append(c);
				if (!EndsWith(builder, separator))
				{
					continue;
				}
				break;
			}
			break;
		}
		return builder.ToString();
	}

	private static bool EndsWith(StringBuilder builder, string separator)
	{
		if (string.IsNullOrEmpty(separator))
		{
			throw new ArgumentException("separator");
		}
		int length = builder.Length;
		int length2 = separator.Length;
		bool result = false;
		if (length >= length2)
		{
			result = true;
			int num = length - 1;
			for (int num2 = length2 - 1; num2 >= 0; num2--)
			{
				if (builder[num] != separator[num2])
				{
					result = false;
					break;
				}
				num--;
			}
		}
		return result;
	}

	private static void ReadToChar(TextReader reader, char endChar, StringBuilder builder, string separator, bool isValid)
	{
		if (isValid)
		{
			ReadToChar(reader, endChar, builder);
		}
		else
		{
			RemoveJunkChar(reader, endChar, builder, separator);
		}
	}

	private static void RemoveJunkChar(TextReader reader, char endChar, StringBuilder builder, string separator)
	{
		char c = ' ';
		bool flag = true;
		bool flag2 = true;
		int num;
		do
		{
			char c2 = c;
			num = reader.Read();
			c = (char)num;
			if (c == endChar)
			{
				flag2 = ((!flag2 || c2 == '\ufffd') ? true : false);
				char c3 = (char)reader.Peek();
				if ((c3 == Convert.ToChar(separator) || c3 == '\r' || c3 == '\n') && !flag2)
				{
					flag = false;
					builder.Append(c);
				}
				else if (c2 != '\ufffd')
				{
					builder.Append(c);
				}
			}
			else if (num > 0)
			{
				builder.Append(c);
			}
		}
		while (flag && num > 0);
	}

	private static void ReadToChar(TextReader reader, char endChar, StringBuilder builder)
	{
		int num;
		char c;
		do
		{
			num = reader.Read();
			c = (char)num;
			builder.Append(c);
		}
		while (c != endChar && num > 0 && c != '\r');
	}

	private static int CharCount(string value, char ch)
	{
		int num = 0;
		for (int num2 = value.Length - 1; num2 >= 0; num2--)
		{
			if (value[num2] == ch)
			{
				num++;
			}
		}
		return num;
	}

	protected internal override void ParseData(Dictionary<int, int> dictUpdatedSSTIndexes)
	{
		if ((base.IsParsed || base.IsParsing) && !ParseDataOnDemand)
		{
			return;
		}
		base.IsParsing = true;
		if (m_dataHolder == null)
		{
			if (!base.IsSkipParsing)
			{
				if (base.ParseOnDemand)
				{
					Stream stream = new MemoryStream();
					BinaryWriter binaryWriter = new BinaryWriter(stream);
					foreach (BiffRecordRaw arrRecord in m_arrRecords)
					{
						int recordCode = arrRecord.RecordCode;
						int num = 0;
						byte[] data = arrRecord.Data;
						if (data != null)
						{
							num = data.Length;
						}
						binaryWriter.Write((short)recordCode);
						binaryWriter.Write((short)num);
						if (data != null)
						{
							binaryWriter.Write(data, 0, num);
						}
					}
					binaryWriter.Flush();
					if (stream.Length > 0)
					{
						m_arrRecords.Clear();
						base.ParseOnDemand = false;
						m_book.IsWorkbookOpening = true;
						stream.Position = 0L;
						new BiffReader(stream);
						m_book.ParseWorksheetsOnDemand();
						m_book.IsWorkbookOpening = false;
					}
				}
				int iStartIndex = ExtractCalculationOptions();
				ReplaceSharedFormula();
				ExtractPageSetup(iStartIndex);
				if (m_iCondFmtPos >= 0)
				{
					ExtractConditionalFormats(m_iCondFmtPos);
				}
				if (m_iDValPos >= 0)
				{
					ExtractDataValidation(m_iDValPos);
				}
				if (m_iCustomPropertyStartIndex >= 0)
				{
					ExtractCustomProperties(m_iCustomPropertyStartIndex);
				}
			}
		}
		else
		{
			if (base.AppImplementation.IsFormulaParsed)
			{
				base.AppImplementation.IsFormulaParsed = false;
			}
			AttachEvents();
			m_dataHolder.ParseWorksheetData(this, dictUpdatedSSTIndexes, ParseDataOnDemand);
			base.AppImplementation.IsFormulaParsed = true;
		}
		if (!base.IsParsed)
		{
			base.IsSaved = true;
		}
		base.IsParsed = true;
		base.IsParsing = false;
	}

	private void ReplaceSharedFormula()
	{
		m_dicRecordsCells.ReplaceSharedFormula();
	}

	internal void ParseColumnInfo(ColumnInfoRecord columnInfo, bool bIgnoreStyles)
	{
		if (columnInfo == null)
		{
			throw new ArgumentNullException("columnInfo");
		}
		if (bIgnoreStyles)
		{
			columnInfo.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
		}
		columnInfo.ColumnWidth = columnInfo.ColumnWidth;
		_ = columnInfo.ExtendedFormatIndex;
		if (columnInfo.FirstColumn != columnInfo.LastColumn)
		{
			if (columnInfo.LastColumn == m_book.MaxColumnCount)
			{
				m_rawColRecord = columnInfo.Clone() as ColumnInfoRecord;
			}
			for (int i = columnInfo.FirstColumn; i <= columnInfo.LastColumn; i++)
			{
				int num = i + 1;
				ColumnInfoRecord columnInfoRecord = (ColumnInfoRecord)columnInfo.Clone();
				columnInfoRecord.FirstColumn = (ushort)i;
				columnInfoRecord.LastColumn = (ushort)i;
				m_arrColumnInfo[num] = columnInfoRecord;
			}
		}
		else
		{
			m_arrColumnInfo[columnInfo.FirstColumn + 1] = columnInfo;
		}
	}

	internal void ParseRowRecord(RowRecord row, bool bIgnoreStyles)
	{
		if (row == null)
		{
			throw new ArgumentNullException("row");
		}
		if (bIgnoreStyles)
		{
			row.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
			row.IsFormatted = false;
		}
		RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, row.RowNumber, bCreate: true);
		orCreateRow.UpdateRowInfo(row, base.AppImplementation.UseFastRecordParsing);
		int num = row.RowNumber + 1;
		if (num < FirstRow)
		{
			FirstRow = num;
		}
		if (num > LastRow)
		{
			LastRow = num;
		}
		if (FirstColumn == int.MaxValue)
		{
			FirstColumn = 0;
		}
		if (LastColumn == int.MaxValue)
		{
			LastColumn = 1;
		}
		int extendedFormatIndex = orCreateRow.ExtendedFormatIndex;
		if (extendedFormatIndex > m_book.InnerExtFormats.Count)
		{
			orCreateRow.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
			return;
		}
		ExtendedFormatImpl extendedFormatImpl = m_book.InnerExtFormats[extendedFormatIndex];
		if (!extendedFormatImpl.HasParent)
		{
			extendedFormatImpl = (ExtendedFormatImpl)extendedFormatImpl.Clone();
			extendedFormatImpl.ParentIndex = extendedFormatIndex;
			extendedFormatImpl.Record.XFType = ExtendedFormatRecord.TXFType.XF_STYLE;
			extendedFormatImpl = m_book.InnerExtFormats.Add(extendedFormatImpl);
			orCreateRow.ExtendedFormatIndex = (ushort)extendedFormatImpl.Index;
		}
	}

	protected int ExtractCalculationOptions()
	{
		int i = 0;
		for (int count = m_arrRecords.Count; i < count; i++)
		{
			BiffRecordRaw biffRecordRaw = m_arrRecords[i];
			if (Array.IndexOf(CalculationOptionsImpl.DEF_CORRECT_CODES, biffRecordRaw.TypeCode) != -1)
			{
				return m_book.InnerCalculation.Parse(m_arrRecords, i);
			}
		}
		return 0;
	}

	protected void ExtractPageSetup(int iStartIndex)
	{
		if (iStartIndex < 0)
		{
			throw new ArgumentOutOfRangeException("iStartIndex");
		}
		int i = iStartIndex;
		for (int count = m_arrRecords.Count; i < count; i++)
		{
			TBIFFRecord typeCode = m_arrRecords[i].TypeCode;
			if (typeCode == TBIFFRecord.PrintHeaders || typeCode == TBIFFRecord.DefaultRowHeight)
			{
				m_pageSetup = new PageSetupImpl(base.Application, this, m_arrRecords, i);
				break;
			}
		}
	}

	protected void ExtractConditionalFormats(int iCondFmtPos)
	{
		if (iCondFmtPos < 0)
		{
			throw new ArgumentOutOfRangeException("iCondFmtPos");
		}
		bool flag = true;
		int num = 0;
		CondFMTRecord condFMTRecord = null;
		List<CFRecord> list = new List<CFRecord>();
		CondFmt12Record condFmt12Record = null;
		List<CF12Record> list2 = new List<CF12Record>();
		List<CFExRecord> cFExRecords = new List<CFExRecord>();
		while (flag)
		{
			BiffRecordRaw biffRecordRaw = m_arrRecords[iCondFmtPos];
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.CondFMT:
				if (condFMTRecord != null)
				{
					CreateFormatsCollection(condFMTRecord, list, cFExRecords, isFutureRecord: false);
					condFMTRecord = null;
					list.Clear();
				}
				if (condFmt12Record != null)
				{
					CreateCF12RecordCollection(condFmt12Record, list2);
					list2.Clear();
					condFmt12Record = null;
				}
				condFMTRecord = (CondFMTRecord)biffRecordRaw;
				num++;
				if (condFMTRecord.Index == 0)
				{
					condFMTRecord.Index = (ushort)num;
				}
				if (!m_dictCondFMT.ContainsKey(condFMTRecord.Index))
				{
					m_dictCondFMT.Add(condFMTRecord.Index, condFMTRecord);
					break;
				}
				num++;
				condFMTRecord.Index = (ushort)num;
				m_dictCondFMT.Add(condFMTRecord.Index, condFMTRecord);
				break;
			case TBIFFRecord.CF:
				list.Add((CFRecord)biffRecordRaw);
				break;
			case TBIFFRecord.CondFMT12:
				if (condFmt12Record != null)
				{
					CreateCF12RecordCollection(condFmt12Record, list2);
					condFmt12Record = null;
					list2.Clear();
				}
				if (condFMTRecord != null)
				{
					CreateFormatsCollection(condFMTRecord, list, cFExRecords, isFutureRecord: false);
					list.Clear();
					condFMTRecord = null;
				}
				condFmt12Record = (CondFmt12Record)biffRecordRaw;
				break;
			case TBIFFRecord.CF12:
				if (m_dictCFExRecords.Count > 0)
				{
					CFExRecord cFExRecord = m_dictCFExRecords[m_dictCFExRecords.Count - 1];
					if (cFExRecord.IsCF12Extends == 1)
					{
						cFExRecord.CF12RecordIfExtends = (CF12Record)biffRecordRaw;
					}
					else
					{
						list2.Add((CF12Record)biffRecordRaw);
					}
				}
				else
				{
					list2.Add((CF12Record)biffRecordRaw);
				}
				break;
			case TBIFFRecord.CFEx:
				if (condFMTRecord != null)
				{
					CreateFormatsCollection(condFMTRecord, list, cFExRecords, isFutureRecord: false);
					list.Clear();
					condFMTRecord = null;
				}
				if (condFmt12Record != null)
				{
					CreateCF12RecordCollection(condFmt12Record, list2);
					list2.Clear();
					condFmt12Record = null;
				}
				_ = (CFExRecord)biffRecordRaw;
				break;
			default:
				flag = false;
				break;
			}
			iCondFmtPos++;
		}
		if (condFMTRecord != null)
		{
			CreateFormatsCollection(condFMTRecord, list, cFExRecords, isFutureRecord: false);
			list.Clear();
		}
		if (condFmt12Record != null)
		{
			CreateCF12RecordCollection(condFmt12Record, list2);
			list2.Clear();
		}
		m_dictCondFMT.Clear();
		m_dictCFExRecords.Clear();
	}

	protected void ExtractDataValidation(int iDValPos)
	{
		if (iDValPos < 0)
		{
			throw new ArgumentOutOfRangeException("iDValPos");
		}
	}

	protected void ExtractCustomProperties(int iCustomPropertyPos)
	{
		if (iCustomPropertyPos < 0)
		{
			throw new ArgumentOutOfRangeException("iCustomPropertyPos");
		}
	}

	private void CreateFormatsCollection(CondFMTRecord format, IList lstConditions, IList CFExRecords, bool isFutureRecord)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (lstConditions == null && CFExRecords == null)
		{
			throw new ArgumentNullException("Conditions");
		}
	}

	private void CreateCF12RecordCollection(CondFmt12Record format, IList conditions)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (conditions == null)
		{
			throw new ArgumentNullException("conditions");
		}
	}

	public double InnerGetColumnWidth(int iColumn)
	{
		if (iColumn < 1)
		{
			throw new ArgumentOutOfRangeException("iColumn can't be less then 1");
		}
		ParseData();
		ColumnInfoRecord columnInfoRecord = m_arrColumnInfo[iColumn];
		if (columnInfoRecord == null)
		{
			return StandardWidth;
		}
		return columnInfoRecord.IsHidden ? 0.0 : ((double)(int)columnInfoRecord.ColumnWidth / 256.0);
	}

	public int ColumnWidthToPixels(double widthInChars)
	{
		double fileWidth = m_book.WidthToFileWidth(widthInChars);
		return (int)m_book.FileWidthToPixels(fileWidth);
	}

	public double PixelsToColumnWidth(int pixels)
	{
		return m_book.PixelsToWidth(pixels);
	}

	internal int EvaluateRealColumnWidth(int fileWidth)
	{
		double pixels = m_book.FileWidthToPixels((double)fileWidth / 256.0);
		return (int)(m_book.PixelsToWidth(pixels) * 256.0);
	}

	internal int EvaluateFileColumnWidth(int realWidth)
	{
		return (int)(m_book.WidthToFileWidth((double)realWidth / 256.0) * 256.0);
	}

	private void OnNameIndexChanged(object sender, NameIndexChangedEventArgs args)
	{
		throw new NotImplementedException();
	}

	internal void AttachNameIndexChangedEvent()
	{
		AttachNameIndexChangedEvent(0);
	}

	internal void AttachNameIndexChangedEvent(int iStartIndex)
	{
		throw new NotImplementedException();
	}

	public void ParseAutoFilters()
	{
	}

	[CLSCompliant(false)]
	protected internal ICellPositionFormat GetRecord(long cellIndex)
	{
		return m_dicRecordsCells.GetCellRecord(cellIndex);
	}

	[CLSCompliant(false)]
	protected internal ICellPositionFormat GetRecord(int iRow, int iColumn)
	{
		return m_dicRecordsCells.GetCellRecord(iRow, iColumn);
	}

	[CLSCompliant(false)]
	protected override void ParseDimensions(DimensionsRecord dimensions)
	{
		base.ParseDimensions(dimensions);
		m_dicRecordsCells.Table.EnsureSize(m_iLastRow);
	}

	public void SetPaneCell(IRange range)
	{
		if (range.Row != range.LastRow || range.Column != range.LastColumn)
		{
			throw new ArgumentOutOfRangeException("range");
		}
		SplitCell = range;
		PaneFirstVisible = range;
		CreateAllSelections();
	}

	private void CreateAllSelections()
	{
		int selectionCount = SelectionCount;
		Dictionary<int, object> dictionary = new Dictionary<int, object>();
		for (int num = m_arrSelections.Count - 1; num >= 0; num--)
		{
			dictionary[m_arrSelections[num].Pane] = null;
		}
		int currentIndex = 0;
		for (int i = m_arrSelections.Count; i < selectionCount; i++)
		{
			SelectionRecord selectionRecord = (SelectionRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Selection);
			byte b2 = (selectionRecord.Pane = (byte)GetFreeIndex(currentIndex, dictionary));
			currentIndex = b2;
			m_arrSelections.Add(selectionRecord);
		}
		int num2 = m_arrSelections.Count - selectionCount;
		if (num2 > 0)
		{
			m_arrSelections.RemoveRange(selectionCount, num2);
		}
		ReIndexSelections(dictionary);
	}

	private void ReIndexSelections(Dictionary<int, object> usedIndexes)
	{
		int num = 0;
		int num2 = 0;
		if (m_pane != null)
		{
			num = m_pane.VerticalSplit;
			num2 = m_pane.HorizontalSplit;
		}
		List<int> list = new List<int>();
		Dictionary<int, object> dictionary = new Dictionary<int, object>();
		if (num != 0 && num2 != 0)
		{
			TryAdd(dictionary, list, usedIndexes, 0);
			TryAdd(dictionary, list, usedIndexes, 1);
			TryAdd(dictionary, list, usedIndexes, 2);
			TryAdd(dictionary, list, usedIndexes, 3);
		}
		else if (num != 0)
		{
			TryAdd(dictionary, list, usedIndexes, 3);
			TryAdd(dictionary, list, usedIndexes, 1);
		}
		else if (num2 != 0)
		{
			TryAdd(dictionary, list, usedIndexes, 3);
			TryAdd(dictionary, list, usedIndexes, 2);
		}
		else
		{
			TryAdd(dictionary, list, usedIndexes, 3);
		}
		int i = 0;
		int num3 = 0;
		int count = m_arrSelections.Count;
		int count2 = list.Count;
		for (; i < count; i++)
		{
			if (num3 >= count2)
			{
				break;
			}
			SelectionRecord selectionRecord = m_arrSelections[i];
			int pane = selectionRecord.Pane;
			if (!dictionary.ContainsKey(pane))
			{
				selectionRecord.Pane = (byte)list[num3];
				num3++;
			}
		}
		if (m_pane != null && !dictionary.ContainsKey(m_pane.ActivePane))
		{
			m_pane.ActivePane = 3;
		}
	}

	private void TryAdd(Dictionary<int, object> mustPresent, List<int> panes, Dictionary<int, object> usedIndexes, int paneIndex)
	{
		mustPresent.Add(paneIndex, null);
		if (!usedIndexes.ContainsKey(paneIndex))
		{
			panes.Add(paneIndex);
		}
	}

	private int GetFreeIndex(int currentIndex, Dictionary<int, object> usedIndexes)
	{
		while (usedIndexes.ContainsKey(currentIndex))
		{
			currentIndex++;
		}
		usedIndexes[currentIndex] = null;
		return currentIndex;
	}

	public void Clear()
	{
		base.ClearAll(OfficeWorksheetCopyFlags.CopyAll);
		ClearData();
		if (m_dicRecordsCells != null)
		{
			m_dicRecordsCells.Clear();
		}
		m_rngUsed = null;
		m_iFirstColumn = int.MaxValue;
		m_iLastColumn = int.MaxValue;
		m_iFirstRow = -1;
		m_iLastRow = -1;
		if (m_mergedCells != null)
		{
			m_mergedCells.Clear();
		}
	}

	public void ClearData()
	{
		if (m_dicRecordsCells != null)
		{
			m_dicRecordsCells.ClearData();
		}
		if (m_arrColumnInfo != null)
		{
			m_arrColumnInfo = null;
		}
	}

	public bool Contains(int iRow, int iColumn)
	{
		ParseData();
		long cellIndex = RangeImpl.GetCellIndex(iColumn, iRow);
		return m_dicRecordsCells.Contains(cellIndex);
	}

	public IRanges CreateRangesCollection()
	{
		return base.AppImplementation.CreateRangesCollection(this);
	}

	public void CreateNamedRanges(string namedRange, string referRange, bool vertical)
	{
		IRanges ranges = ((IWorksheet)this).CreateRangesCollection();
		if (!vertical)
		{
			for (int i = ((IWorksheet)this)[referRange].Row; i < ((IWorksheet)this)[referRange].LastRow + 1; i++)
			{
				ranges.Add(((IWorksheet)this)[i, ((IWorksheet)this)[referRange].Column, i, ((IWorksheet)this)[referRange].LastColumn]);
			}
		}
		else
		{
			for (int j = ((IWorksheet)this)[referRange].Column; j < ((IWorksheet)this)[referRange].LastColumn + 1; j++)
			{
				ranges.Add(((IWorksheet)this)[((IWorksheet)this)[referRange].Row, j, ((IWorksheet)this)[referRange].LastRow, j]);
			}
		}
		int num = 0;
		INames names = ((IWorksheet)this).Names;
		try
		{
			foreach (IRange item in ((IWorksheet)this)[namedRange])
			{
				names.Add(item.Text).RefersToRange = ranges[num];
				num++;
			}
		}
		catch (Exception)
		{
			throw new InvalidRangeException("NamedRange and data count mismatch");
		}
	}

	public void ShowColumn(int columnIndex, bool isVisible)
	{
		ParseData();
		if (columnIndex < 0 || columnIndex > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("columnIndex", "Value cannot be less than 0 and greater than 255");
		}
		ColumnInfoRecord columnInfoRecord = m_arrColumnInfo[columnIndex];
		if (columnInfoRecord == null)
		{
			columnInfoRecord = (ColumnInfoRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ColumnInfo);
			columnInfoRecord.FirstColumn = (ushort)(columnIndex - 1);
			columnInfoRecord.LastColumn = (ushort)(columnIndex - 1);
			columnInfoRecord.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
			m_arrColumnInfo[columnIndex] = columnInfoRecord;
		}
		else if (isVisible && columnInfoRecord.ColumnWidth == 0)
		{
			SetColumnWidth(columnIndex, StandardWidth);
		}
		columnInfoRecord.IsHidden = !isVisible;
		UpdateShapes();
	}

	public void HideColumn(int columnIndex)
	{
		ShowColumn(columnIndex, isVisible: false);
	}

	public void HideRow(int rowIndex)
	{
		ShowRow(rowIndex, isVisible: false);
	}

	public void ShowRow(int rowIndex, bool isVisible)
	{
		if (rowIndex < 1 || rowIndex > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("rowIndex");
		}
		WorksheetHelper.GetOrCreateRow(this, rowIndex - 1, bCreate: true).IsHidden = !isVisible;
		UpdateShapes();
	}

	private void UpdateShapes()
	{
		if (base.Shapes.Count == 0)
		{
			return;
		}
		for (int i = 0; i < base.Shapes.Count; i++)
		{
			if (!base.Shapes[i].IsSizeWithCell)
			{
				((ShapeImpl)base.Shapes[i]).UpdateAnchorPoints();
			}
		}
	}

	public void ShowRange(IRange range, bool isVisible)
	{
		bool flag = false;
		bool flag2 = false;
		if (range.Row < 1 || range.Row > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("Row");
		}
		int num = range.Row;
		int num2 = range.LastRow;
		if (range.LastRow - range.Row > m_book.MaxRowCount - (range.LastRow - range.Row) && range.LastRow == m_book.MaxRowCount && !isVisible)
		{
			flag = true;
			num = 1;
			num2 = range.Row - 1;
			if (num2 < UsedRange.LastRow)
			{
				num = range.Row;
				num2 = UsedRange.LastRow;
				flag2 = true;
			}
			IsZeroHeight = true;
			isVisible = true;
		}
		int i = num;
		for (int num3 = num2; i <= num3; i++)
		{
			WorksheetHelper.GetOrCreateRow(this, i - 1, bCreate: true).IsHidden = (flag2 ? isVisible : (!isVisible));
			ParseData();
		}
		if (range.Column < 0 || range.Column > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("Column", "Value cannot be less than 0 and greater than 255");
		}
		int j = range.Column;
		for (int lastColumn = range.LastColumn; j <= lastColumn; j++)
		{
			ColumnInfoRecord columnInfoRecord = m_arrColumnInfo[j];
			if (columnInfoRecord == null)
			{
				columnInfoRecord = (ColumnInfoRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ColumnInfo);
				columnInfoRecord.FirstColumn = (ushort)(j - 1);
				columnInfoRecord.LastColumn = (ushort)(lastColumn - 1);
				columnInfoRecord.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
				m_arrColumnInfo[j] = columnInfoRecord;
			}
			else if (isVisible && columnInfoRecord.ColumnWidth == 0)
			{
				SetColumnWidth(j, StandardWidth);
			}
			columnInfoRecord.IsHidden = (flag ? isVisible : (!isVisible));
		}
		UpdateShapes();
	}

	public void ShowRange(RangesCollection ranges, bool isVisible)
	{
		if (ranges.Count == 0)
		{
			return;
		}
		foreach (IRange range in ranges)
		{
			ShowRange(range, isVisible);
		}
	}

	public void ShowRange(IRange[] ranges, bool isVisible)
	{
		if (ranges.Length != 0)
		{
			RangesCollection rangesCollection = new RangesCollection(base.Application, this);
			foreach (IRange range in ranges)
			{
				rangesCollection.Add(range);
			}
			ShowRange(rangesCollection, isVisible);
		}
	}

	public bool IsColumnVisible(int columnIndex)
	{
		if (columnIndex < 1 || columnIndex > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("columnIndex", "Value cannot be less than 0 and greater than 255");
		}
		ParseData();
		ColumnInfoRecord columnInfoRecord = m_arrColumnInfo[columnIndex];
		if (columnInfoRecord == null)
		{
			return true;
		}
		return !columnInfoRecord.IsHidden;
	}

	public bool IsRowVisible(int rowIndex)
	{
		if (rowIndex < 1 || rowIndex > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("rowIndex");
		}
		RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, rowIndex - 1, bCreate: false);
		if (orCreateRow == null)
		{
			return true;
		}
		return !orCreateRow.IsHidden;
	}

	public void InsertRow(int iRowIndex)
	{
		InsertRow(iRowIndex, 1, OfficeInsertOptions.FormatDefault);
	}

	public void InsertRow(int iRowIndex, int iRowCount)
	{
		InsertRow(iRowIndex, iRowCount, OfficeInsertOptions.FormatDefault);
	}

	public void InsertRow(int iRowIndex, int iRowCount, OfficeInsertOptions insertOptions)
	{
		ParseData();
		if (iRowIndex < 1 || iRowIndex > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("iRowIndex");
		}
		if (!CanInsertRow(iRowIndex, iRowCount, insertOptions) || !base.InnerShapes.CanInsertRowColumn(iRowIndex, iRowCount, bRow: true, m_book.MaxRowCount))
		{
			throw new ArgumentException("Can't insert row");
		}
		if (m_mergedCells != null)
		{
			m_mergedCells.InsertRow(iRowIndex, iRowCount);
		}
		m_book.InnerNamesColection.InsertRow(iRowIndex, iRowCount, base.Name);
		bool flag = iRowIndex <= m_iLastRow;
		if (iRowIndex + iRowCount < m_book.MaxRowCount)
		{
			if (!flag)
			{
				m_iLastRow = iRowIndex;
			}
			if (m_iFirstColumn < m_book.MaxColumnCount)
			{
				IRange source = Range[iRowIndex, m_iFirstColumn, m_iLastRow, m_iLastColumn];
				OfficeCopyRangeOptions options = OfficeCopyRangeOptions.UpdateFormulas | OfficeCopyRangeOptions.CopyErrorIndicators | OfficeCopyRangeOptions.CopyConditionalFormats;
				IRange destination = Range[iRowIndex + iRowCount, m_iFirstColumn, m_iLastRow + iRowCount, m_iLastColumn];
				MoveRange(destination, source, options, bUpdateRowRecords: true);
			}
			else
			{
				m_iLastRow += iRowCount;
				m_dicRecordsCells.Table.InsertIntoDefaultRows(iRowIndex - 1, iRowCount);
			}
		}
		if (flag)
		{
			CopyStylesAfterInsert(iRowIndex, iRowCount, insertOptions, bRow: true);
		}
		else if (insertOptions != OfficeInsertOptions.FormatDefault)
		{
			CopyStylesAfterInsert(iRowIndex, iRowCount, insertOptions, bRow: true);
		}
		base.InnerShapes.InsertRemoveRowColumn(iRowIndex, iRowCount, bRow: true, bRemove: false);
	}

	public void InsertColumn(int iColumnIndex)
	{
		InsertColumn(iColumnIndex, 1, OfficeInsertOptions.FormatDefault);
	}

	public void InsertColumn(int iColumnIndex, int iColumnCount)
	{
		InsertColumn(iColumnIndex, iColumnCount, OfficeInsertOptions.FormatDefault);
	}

	public void InsertColumn(int iColumnIndex, int iColumnCount, OfficeInsertOptions insertOptions)
	{
		ParseData();
		if (iColumnIndex < 1 || iColumnIndex > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("Value cannot be less 1 and greater than max column index.");
		}
		if (!CanInsertColumn(iColumnIndex, iColumnCount, insertOptions) || !base.InnerShapes.CanInsertRowColumn(iColumnIndex, iColumnCount, bRow: false, m_book.MaxColumnCount))
		{
			throw new ArgumentException("Can't insert column");
		}
		if (iColumnCount < 1 || iColumnCount > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("iColumnCount", "Value cannot be less 1 and greater than max column index");
		}
		int num = m_iFirstColumn;
		if (m_mergedCells != null)
		{
			m_mergedCells.InsertColumn(iColumnIndex, iColumnCount);
		}
		m_book.InnerNamesColection.InsertColumn(iColumnIndex, iColumnCount, base.Name);
		if (iColumnIndex <= m_iLastColumn && m_iFirstRow > 0 && m_iFirstRow <= m_book.MaxRowCount)
		{
			if (iColumnIndex >= num)
			{
				num = iColumnIndex;
			}
			IRange source = Range[m_iFirstRow, num, m_iLastRow, m_iLastColumn];
			OfficeCopyRangeOptions options = OfficeCopyRangeOptions.UpdateFormulas | OfficeCopyRangeOptions.CopyErrorIndicators | OfficeCopyRangeOptions.CopyConditionalFormats;
			MoveRange(Range[m_iFirstRow, num + iColumnCount], source, options, bUpdateRowRecords: false);
			InsertIntoDefaultColumns(iColumnIndex, iColumnCount, insertOptions);
		}
		CopyStylesAfterInsert(iColumnIndex, iColumnCount, insertOptions, bRow: false);
		base.InnerShapes.InsertRemoveRowColumn(iColumnIndex, iColumnCount, bRow: false, bRemove: false);
	}

	public void DeleteRow(int index)
	{
		DeleteRow(index, 1);
	}

	public void DeleteRow(int index, int count)
	{
		ParseData();
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (index < 1 || index > m_book.MaxRowCount - count + 1)
		{
			throw new ArgumentOutOfRangeException("row index");
		}
		RecordTable table = m_dicRecordsCells.Table;
		int num = table.FirstRow + 1;
		int num2 = table.LastRow + 1;
		int iFirstColumn = m_iFirstColumn;
		int num3 = ((iFirstColumn <= 0) ? 1 : iFirstColumn);
		iFirstColumn = m_iLastColumn;
		int num4 = ((iFirstColumn <= 0) ? 1 : iFirstColumn);
		if (num > 0 && (num3 != num4 || (num3 == 1 && num4 == 1)))
		{
			_ = (RangeImpl)Range[index, num3, index + count - 1, num4];
			num = index + count;
			Rectangle.FromLTRB(FirstColumn - 1, index - 1, LastColumn - 1, index + count - 2);
			if (num <= num2)
			{
				IRange source = Range[num, num3, num2, num4];
				IRange destination = Range[index, num3];
				OfficeCopyRangeOptions options = OfficeCopyRangeOptions.UpdateFormulas | OfficeCopyRangeOptions.CopyConditionalFormats;
				IOperation beforeMove = new RowsClearer(this, index, count);
				MoveRange(destination, source, options, bUpdateRowRecords: true, beforeMove);
			}
			if (m_mergedCells != null)
			{
				m_mergedCells.RemoveRow(index, count);
			}
		}
		m_book.InnerNamesColection.RemoveRow(index, base.Name, count);
		base.InnerShapes.InsertRemoveRowColumn(index, count, bRow: true, bRemove: true);
		int num5 = Math.Min(num2 - index + 1, count);
		if (num5 > 0)
		{
			RemoveLastRow(bUpdateFormula: true, num5);
		}
		if (!HasSheetCalculation)
		{
			return;
		}
		string text = "!";
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (IName name in base.ParentWorkbook.Names)
		{
			if (name.Scope.Length > 0 && text.IndexOf("!" + name.Scope + "!") > -1 && name.Value != null)
			{
				dictionary.Add((name.Scope + "!" + name.Name).ToUpper(), name.Value.Replace("'", ""));
			}
			else if (name.Name != null && name.Value != null && !dictionary.ContainsKey(name.Name.ToUpper()))
			{
				dictionary.Add(name.Name.ToUpper(), name.Value.Replace("'", ""));
			}
		}
		Dictionary<object, object> dictionary2 = new Dictionary<object, object>();
		if (dictionary == null)
		{
			return;
		}
		foreach (string key in dictionary.Keys)
		{
			dictionary2.Add(key.ToUpper(CultureInfo.InvariantCulture), dictionary[key]);
		}
	}

	private void CopyRowRecord(int iDestRowIndex, int iSourceRowIndex)
	{
		RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, iDestRowIndex, bCreate: true);
		RowStorage orCreateRow2 = WorksheetHelper.GetOrCreateRow(this, iSourceRowIndex, bCreate: true);
		orCreateRow.CopyRowRecordFrom(orCreateRow2);
	}

	public void DeleteColumn(int index)
	{
		DeleteColumn(index, 1);
	}

	public void DeleteColumn(int index, int count)
	{
		ParseData();
		if (index < 1 || index > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("column index");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (index + count > m_book.MaxColumnCount)
		{
			count = m_book.MaxColumnCount - index;
		}
		if (count == 0)
		{
			return;
		}
		int iFirstRow = m_iFirstRow;
		int iLastRow = m_iLastRow;
		int iFirstColumn = m_iFirstColumn;
		int iLastColumn = m_iLastColumn;
		if (iFirstRow > 0 && (iFirstColumn != iLastColumn || (iFirstColumn == 1 && iLastColumn == 1)))
		{
			if (!((RangeImpl)Range[iFirstRow, index, iLastRow, index + count - 1]).AreFormulaArraysNotSeparated)
			{
				throw new InvalidRangeException();
			}
			Rectangle rectangle = Rectangle.FromLTRB(index - 1, FirstRow - 1, index + count - 2, LastRow - 1);
			(new Rectangle[1])[0] = rectangle;
			if (index < iLastColumn)
			{
				iFirstColumn = index + count;
				if (iFirstColumn <= iLastColumn)
				{
					IRange source = Range[iFirstRow, iFirstColumn, iLastRow, iLastColumn];
					IRange destination = Range[iFirstRow, index];
					OfficeCopyRangeOptions options = OfficeCopyRangeOptions.UpdateFormulas | OfficeCopyRangeOptions.CopyConditionalFormats | OfficeCopyRangeOptions.CopyDataValidations;
					MoveRange(destination, source, options, bUpdateRowRecords: false);
				}
			}
			if (m_mergedCells != null)
			{
				m_mergedCells.RemoveColumn(index, count);
			}
		}
		m_book.InnerNamesColection.RemoveColumn(index, base.Name, count);
		RemoveFromDefaultColumns(index, count, OfficeInsertOptions.FormatDefault);
		base.InnerShapes.InsertRemoveRowColumn(index, count, bRow: false, bRemove: true);
		if (UsedRange.LastColumn >= index)
		{
			count = Math.Min(count, iLastColumn - index + 1);
			RemoveLastColumn(bUpdateFormula: true, count);
		}
	}

	public double GetColumnWidth(int iColumnIndex)
	{
		if (iColumnIndex < 1 || iColumnIndex > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("Value cannot be less 1 and greater than max column index.");
		}
		return InnerGetColumnWidth(iColumnIndex);
	}

	public int GetColumnWidthInPixels(int iColumnIndex)
	{
		if (iColumnIndex > m_book.MaxColumnCount)
		{
			iColumnIndex = m_book.MaxColumnCount;
		}
		if (iColumnIndex < 1 || iColumnIndex > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("Value cannot be less 1 and greater than max column index.");
		}
		double widthInChars = InnerGetColumnWidth(iColumnIndex);
		return ColumnWidthToPixels(widthInChars);
	}

	public double GetRowHeight(int iRow)
	{
		return InnerGetRowHeight(iRow, bRaiseEvents: true);
	}

	internal double GetInnerRowHeight(int iRow)
	{
		return InnerGetRowHeight(iRow + 1, bRaiseEvents: true);
	}

	public int GetRowHeightInPixels(int iRowIndex)
	{
		return (int)ApplicationImpl.ConvertToPixels((float)GetRowHeight(iRowIndex), MeasureUnits.Point);
	}

	internal int GetInnerRowHeightInPixels(int iRowIndex)
	{
		return (int)ApplicationImpl.ConvertToPixels((float)GetInnerRowHeight(iRowIndex), MeasureUnits.Point);
	}

	private int ImportArray<T>(T[] arrObject, int firstRow, int firstColumn, bool isVertical)
	{
		if (arrObject == null)
		{
			throw new ArgumentNullException("arrObject");
		}
		if (firstRow < 1 || firstRow > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("firstRow");
		}
		if (firstColumn < 1 || firstColumn > m_book.MaxColumnCount)
		{
			throw new ArgumentNullException("firstColumn");
		}
		ParseData();
		bool isStringsPreserved = false;
		int num = 0;
		int num2 = ((!isVertical) ? (Math.Min(firstColumn + arrObject.Length - 1, m_book.MaxColumnCount) - firstColumn + 1) : (Math.Min(firstRow + arrObject.Length - 1, m_book.MaxRowCount) - firstRow + 1));
		int iXFIndex = m_book.DefaultXFIndex;
		if (num2 > 0)
		{
			IRange range = InnerGetCell(firstColumn, firstRow);
			if (arrObject[num] == null)
			{
				range.Value2 = null;
			}
			else
			{
				isStringsPreserved = IsStringsPreserved;
				if (arrObject[num].GetType() == typeof(string) && !CheckIsFormula(arrObject[num]) && IsStringsPreserved)
				{
					IsStringsPreserved = true;
				}
				else
				{
					IsStringsPreserved = false;
				}
				range.Value2 = arrObject[num];
				IsStringsPreserved = isStringsPreserved;
			}
			iXFIndex = ((RangeImpl)range).ExtendedFormatIndex;
		}
		for (num = 1; num < num2; num++)
		{
			IRange range = (isVertical ? InnerGetCell(firstColumn, firstRow + num, iXFIndex) : InnerGetCell(firstColumn + num, firstRow, iXFIndex));
			if (arrObject[num] != null)
			{
				isStringsPreserved = IsStringsPreserved;
				if (arrObject[num].GetType() == typeof(string) && !CheckIsFormula(arrObject[num]) && IsStringsPreserved)
				{
					IsStringsPreserved = true;
				}
				else
				{
					IsStringsPreserved = false;
				}
			}
			range.Value2 = arrObject[num];
			IsStringsPreserved = isStringsPreserved;
		}
		return num;
	}

	private bool CheckIsFormula(object value)
	{
		if (value.ToString().StartsWith("="))
		{
			return true;
		}
		return false;
	}

	internal bool checkIsNumber(string value, CultureInfo cultureInfo)
	{
		bool result = true;
		if (value.Contains(cultureInfo.NumberFormat.NumberDecimalSeparator))
		{
			if (new Regex("[" + cultureInfo.NumberFormat.NumberDecimalSeparator + "]").Matches(value).Count > 1)
			{
				return false;
			}
			if (value.Contains(cultureInfo.NumberFormat.NumberGroupSeparator))
			{
				int num = value.IndexOf(cultureInfo.NumberFormat.NumberDecimalSeparator);
				string value2 = value.Substring(0, num);
				if (value.Substring(num + 1, value.Length - 1 - num).Contains(cultureInfo.NumberFormat.NumberGroupSeparator))
				{
					return false;
				}
				result = checkGroupSeparatorPosition(value2, cultureInfo);
			}
		}
		else
		{
			result = checkGroupSeparatorPosition(value, cultureInfo);
		}
		return result;
	}

	private bool checkGroupSeparatorPosition(string value, CultureInfo cultureInfo)
	{
		string text = "";
		for (int num = value.Length - 1; num >= 0; num--)
		{
			text += value[num];
		}
		MatchCollection matchCollection = new Regex("[" + cultureInfo.NumberFormat.NumberGroupSeparator + "]").Matches(text);
		for (int i = 0; i < matchCollection.Count; i++)
		{
			if ((matchCollection[i].Index - i) % 3 != 0)
			{
				return false;
			}
		}
		return true;
	}

	public int ImportArray(object[] arrObject, int firstRow, int firstColumn, bool isVertical)
	{
		return ImportArray<object>(arrObject, firstRow, firstColumn, isVertical);
	}

	public int ImportArray(string[] arrString, int firstRow, int firstColumn, bool isVertical)
	{
		return ImportArray<string>(arrString, firstRow, firstColumn, isVertical);
	}

	public int ImportArray(int[] arrInt, int firstRow, int firstColumn, bool isVertical)
	{
		return ImportArray<int>(arrInt, firstRow, firstColumn, isVertical);
	}

	public int ImportArray(double[] arrDouble, int firstRow, int firstColumn, bool isVertical)
	{
		return ImportArray<double>(arrDouble, firstRow, firstColumn, isVertical);
	}

	public int ImportArray(DateTime[] arrDateTime, int firstRow, int firstColumn, bool isVertical)
	{
		if (arrDateTime == null)
		{
			throw new ArgumentNullException("arrObject");
		}
		if (firstRow < 1 || firstRow > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("firstRow");
		}
		if (firstColumn < 1 || firstColumn > m_book.MaxColumnCount)
		{
			throw new ArgumentNullException("firstColumn");
		}
		ParseData();
		IsStringsPreserved = false;
		int num = 0;
		int num2 = ((!isVertical) ? (Math.Min(firstColumn + arrDateTime.Length - 1, m_book.MaxColumnCount) - firstColumn + 1) : (Math.Min(firstRow + arrDateTime.Length - 1, m_book.MaxRowCount) - firstRow + 1));
		int iXFIndex = m_book.DefaultXFIndex;
		if (num2 > 0)
		{
			IRange range = (isVertical ? InnerGetCell(firstColumn, firstRow) : InnerGetCell(firstColumn, firstRow));
			range.DateTime = arrDateTime[num];
			iXFIndex = ((RangeImpl)range).ExtendedFormatIndex;
		}
		for (num = 1; num < num2; num++)
		{
			IRange range = (isVertical ? InnerGetCell(firstColumn, firstRow + num, iXFIndex) : InnerGetCell(firstColumn + num, firstRow, iXFIndex));
			range.DateTime = arrDateTime[num];
		}
		return num;
	}

	public int ImportArray(object[,] arrObject, int firstRow, int firstColumn)
	{
		if (arrObject == null)
		{
			throw new ArgumentNullException("arrObject");
		}
		if (firstRow < 1 || firstRow > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("firstRow");
		}
		if (firstColumn < 1 || firstColumn > m_book.MaxColumnCount)
		{
			throw new ArgumentNullException("firstColumn");
		}
		ParseData();
		int num = Math.Min(firstRow + arrObject.GetLength(0) - 1, m_book.MaxRowCount) - firstRow + 1;
		int num2 = Math.Min(firstColumn + arrObject.GetLength(1) - 1, m_book.MaxColumnCount) - firstColumn + 1;
		int[] array = new int[num2];
		if (num2 > 0 && num > 0)
		{
			for (int i = 0; i < num2; i++)
			{
				IRange range = InnerGetCell(i + firstColumn, firstRow);
				if (arrObject[0, i] == null)
				{
					range.Value2 = null;
				}
				else
				{
					if (arrObject[0, i].GetType() == typeof(string) && !CheckIsFormula(arrObject[0, i]) && IsStringsPreserved)
					{
						IsStringsPreserved = true;
					}
					else
					{
						IsStringsPreserved = false;
					}
					range.Value2 = arrObject[0, i];
				}
				RangeImpl rangeImpl = (RangeImpl)range;
				array[i] = rangeImpl.ExtendedFormatIndex;
			}
			int j;
			for (j = 1; j < num; j++)
			{
				for (int k = 0; k < num2; k++)
				{
					IRange range = InnerGetCell(firstColumn + k, j + firstRow, array[k]);
					if (arrObject[j, k] == null)
					{
						range.Value2 = null;
						continue;
					}
					if (arrObject[j, k].GetType() == typeof(string) && !CheckIsFormula(arrObject[j, k]) && IsStringsPreserved)
					{
						IsStringsPreserved = true;
					}
					else
					{
						IsStringsPreserved = false;
					}
					range.Value2 = arrObject[j, k];
				}
			}
			return j;
		}
		return 0;
	}

	public int ImportData(IEnumerable arrObject, int firstRow, int firstColumn, bool includeHeader)
	{
		if (arrObject == null)
		{
			throw new ArgumentNullException("arrObject");
		}
		if (firstRow < 1 || firstRow > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("firstRow");
		}
		if (firstColumn < 1 || firstColumn > m_book.MaxColumnCount)
		{
			throw new ArgumentNullException("firstColumn");
		}
		IEnumerator enumerator = arrObject.GetEnumerator();
		if (enumerator == null)
		{
			return 0;
		}
		bool flag = false;
		List<PropertyInfo> propertyInfo = null;
		List<TypeCode> list = null;
		int num = 0;
		enumerator.MoveNext();
		object current = enumerator.Current;
		if (current == null)
		{
			return 0;
		}
		Type type = current.GetType();
		if (type.Namespace == null || (type.Namespace != null && !type.Namespace.Contains("System")))
		{
			flag = true;
		}
		if (!flag)
		{
			return 0;
		}
		list = GetObjectMembersInfo(current, out propertyInfo);
		if (includeHeader)
		{
			for (int i = 0; i < propertyInfo.Count; i++)
			{
				SetText(firstRow, firstColumn + i, propertyInfo[i].Name);
			}
			firstRow++;
		}
		IMigrantRange migrantRange = MigrantRange;
		do
		{
			current = enumerator.Current;
			if (current == null)
			{
				continue;
			}
			for (int j = 0; j < propertyInfo.Count; j++)
			{
				PropertyInfo strProperty = propertyInfo[j];
				migrantRange.ResetRowColumn(firstRow + num, firstColumn + j);
				object valueFromProperty = GetValueFromProperty(current, strProperty);
				if (valueFromProperty == null)
				{
					continue;
				}
				switch (list[j])
				{
				case TypeCode.String:
				{
					string text = (string)GetValueFromProperty(current, strProperty);
					if (text != null && text.Length != 0)
					{
						migrantRange.SetValue(text);
					}
					break;
				}
				case TypeCode.Int32:
					migrantRange.SetValue((int)valueFromProperty);
					break;
				case TypeCode.Int16:
					migrantRange.SetValue(Convert.ToInt16(valueFromProperty));
					break;
				case TypeCode.Double:
					migrantRange.SetValue((double)valueFromProperty);
					break;
				case TypeCode.Int64:
				case TypeCode.Decimal:
					migrantRange.SetValue(Convert.ToDouble(valueFromProperty));
					break;
				case TypeCode.Boolean:
					migrantRange.SetValue((bool)valueFromProperty);
					break;
				case TypeCode.DateTime:
					migrantRange.SetValue((DateTime)valueFromProperty);
					break;
				default:
					migrantRange.SetValue(valueFromProperty.ToString());
					break;
				}
			}
			num++;
		}
		while (enumerator.MoveNext());
		return num;
	}

	private List<TypeCode> GetObjectMembersInfo(object obj, out List<PropertyInfo> propertyInfo)
	{
		Type type = obj.GetType();
		List<TypeCode> list = new List<TypeCode>();
		propertyInfo = new List<PropertyInfo>();
		PropertyInfo[] array = type.GetRuntimeProperties().ToArray();
		foreach (PropertyInfo propertyInfo2 in array)
		{
			propertyInfo.Add(propertyInfo2);
			list.Add(TypeExtension.GetTypeCode(propertyInfo2.PropertyType));
		}
		return list;
	}

	private object GetValueFromProperty(object value, PropertyInfo strProperty)
	{
		if (strProperty == null)
		{
			throw new ArgumentOutOfRangeException("Can't find property");
		}
		value = strProperty.GetValue(value, null);
		return value;
	}

	public void RemovePanes()
	{
		ParseData();
		base.WindowTwo.IsFreezePanes = false;
		base.WindowTwo.IsFreezePanesNoSplit = false;
		m_pane = null;
	}

	public IRange IntersectRanges(IRange range1, IRange range2)
	{
		if (range1 == null)
		{
			throw new ArgumentNullException("range1");
		}
		if (range1 == null)
		{
			throw new ArgumentNullException("range2");
		}
		if (range1.Parent != range2.Parent)
		{
			return null;
		}
		Rectangle a = Rectangle.FromLTRB(range1.Column, range1.Row, range1.LastColumn, range1.LastRow);
		Rectangle b = Rectangle.FromLTRB(range2.Column, range2.Row, range2.LastColumn, range2.LastRow);
		Rectangle rectangle = Rectangle.Intersect(a, b);
		if (rectangle == Rectangle.Empty)
		{
			return null;
		}
		return range1[rectangle.Top, rectangle.Left, rectangle.Bottom, rectangle.Right];
	}

	public IRange MergeRanges(IRange range1, IRange range2)
	{
		if (range1 == null)
		{
			throw new ArgumentNullException("range1");
		}
		if (range2 == null)
		{
			throw new ArgumentNullException("range2");
		}
		if (range1.Parent != range2.Parent)
		{
			return null;
		}
		int num = range1.LastColumn - range1.Column + 1;
		int num2 = range2.LastColumn - range2.Column + 1;
		int num3 = range1.LastRow - range1.Row + 1;
		int num4 = range2.LastRow - range2.Row + 1;
		if (num != num2 && num3 != num4)
		{
			return null;
		}
		if (num == num2 && range1.Column == range2.Column)
		{
			if (range2.Row < range1.Row)
			{
				IRange range3 = range1;
				range1 = range2;
				range2 = range3;
			}
			if (range2.Row >= range1.Row && range2.Row <= range1.LastRow + 1)
			{
				return range1[range1.Row, range1.Column, Math.Max(range1.LastRow, range2.LastRow), range1.LastColumn];
			}
		}
		if (num3 == num4 && range1.Row == range2.Row)
		{
			if (range2.Column < range1.Column)
			{
				IRange range4 = range1;
				range1 = range2;
				range2 = range4;
			}
			if (range2.Column >= range1.Column && range2.Column <= range1.LastColumn + 1)
			{
				return range1[range1.Row, range1.Column, range1.LastRow, Math.Max(range1.LastColumn, range2.LastColumn)];
			}
		}
		return null;
	}

	private IRange[] Find(string value)
	{
		ParseData();
		Dictionary<int, object> stringIndexes = m_book.InnerSST.GetStringIndexes(value);
		return ConvertCellListIntoRange(m_dicRecordsCells.Find(stringIndexes));
	}

	public void Replace(string oldValue, string newValue)
	{
		IRange[] array = Find(oldValue);
		int num = ((array != null) ? array.Length : 0);
		for (int i = 0; i < num; i++)
		{
			IRange obj = array[i];
			string text = obj.Text.ToLower();
			oldValue = oldValue.ToLower();
			obj.Text = text.Replace(oldValue, newValue);
		}
	}

	public void Replace(string oldValue, DateTime newValue)
	{
		IRange[] array = Find(oldValue);
		int num = ((array != null) ? array.Length : 0);
		for (int i = 0; i < num; i++)
		{
			array[i].DateTime = newValue;
		}
	}

	public void Replace(string oldValue, double newValue)
	{
		IRange[] array = Find(oldValue);
		int num = ((array != null) ? array.Length : 0);
		for (int i = 0; i < num; i++)
		{
			array[i].Number = newValue;
		}
	}

	public void Replace(string oldValue, string[] newValues, bool isVertical)
	{
		IRange[] array = Find(oldValue);
		int num = ((array != null) ? array.Length : 0);
		for (int i = 0; i < num; i++)
		{
			((RangeImpl)array[i]).Replace(oldValue, newValues, isVertical);
		}
	}

	public void Replace(string oldValue, int[] newValues, bool isVertical)
	{
		IRange[] array = Find(oldValue);
		int num = ((array != null) ? array.Length : 0);
		for (int i = 0; i < num; i++)
		{
			((RangeImpl)array[i]).Replace(oldValue, newValues, isVertical);
		}
	}

	public void Replace(string oldValue, double[] newValues, bool isVertical)
	{
		IRange[] array = Find(oldValue);
		int num = ((array != null) ? array.Length : 0);
		for (int i = 0; i < num; i++)
		{
			((RangeImpl)array[i]).Replace(oldValue, newValues, isVertical);
		}
	}

	public void Remove()
	{
		ParseData();
		m_book.InnerWorksheets.InnerRemove(base.Index);
		m_names.Clear();
		Dispose();
	}

	public void Move(int iNewIndex)
	{
		int realIndex = base.RealIndex;
		int iNewIndex2 = FindWorksheetNotBefore(iNewIndex);
		m_book.Objects.Move(realIndex, iNewIndex);
		m_book.InnerWorksheets.Move(base.Index, iNewIndex2);
	}

	private int FindWorksheetNotBefore(int iNewIndex)
	{
		int i = iNewIndex;
		for (int objectCount = m_book.ObjectCount; i < objectCount; i++)
		{
			if (m_book.Objects[i] is IWorksheet worksheet)
			{
				return worksheet.Index;
			}
		}
		return m_book.Worksheets.Count - 1;
	}

	public void SetColumnWidth(int iColumn, double value)
	{
		if (iColumn < 1 || iColumn > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("Column", "Column index cannot be larger then 256 or less then one");
		}
		if (InnerGetColumnWidth(iColumn) == value)
		{
			return;
		}
		ColumnInfoRecord columnInfoRecord = m_arrColumnInfo[iColumn];
		if (columnInfoRecord == null)
		{
			columnInfoRecord = (ColumnInfoRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ColumnInfo);
			ColumnInfoRecord columnInfoRecord2 = columnInfoRecord;
			ushort firstColumn = (columnInfoRecord.LastColumn = (ushort)(iColumn - 1));
			columnInfoRecord2.FirstColumn = firstColumn;
			columnInfoRecord.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
			columnInfoRecord.ColumnWidth = (ushort)(base.Application.StandardWidth * 256.0);
			m_arrColumnInfo[iColumn] = columnInfoRecord;
		}
		if (value == 0.0)
		{
			columnInfoRecord.IsHidden = true;
		}
		else
		{
			if (value > 255.0)
			{
				value = 255.0;
			}
			columnInfoRecord.ColumnWidth = (ushort)(value * 256.0);
			WorksheetHelper.AccessColumn(this, iColumn);
			RaiseColumnWidthChangedEvent(iColumn, value);
		}
		SetChanged();
	}

	public void SetColumnWidthInPixels(int iColumn, int value)
	{
		ParseData();
		double value2 = PixelsToColumnWidth(value);
		SetColumnWidth(iColumn, value2);
	}

	public void SetColumnWidthInPixels(int iStartColumnIndex, int iCount, int value)
	{
		ParseData();
		double value2 = PixelsToColumnWidth(value);
		for (int i = 0; i < iCount; i++)
		{
			SetColumnWidth(iStartColumnIndex++, value2);
		}
	}

	public void SetRowHeight(int iRow, double value)
	{
		InnerSetRowHeight(iRow, value, bIsBadFontHeight: true, MeasureUnits.Point, bRaiseEvents: true);
	}

	public void SetRowHeightInPixels(int iRowIndex, double value)
	{
		if (iRowIndex < 1 || iRowIndex > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("iRowIndex", "Value cannot be less 1 and greater than max row index.");
		}
		if (value < 0.0)
		{
			throw new ArgumentOutOfRangeException("value");
		}
		InnerSetRowHeight(iRowIndex, value, bIsBadFontHeight: true, MeasureUnits.Pixel, bRaiseEvents: true);
	}

	public void SetRowHeightInPixels(int iStartRowIndex, int iCount, double value)
	{
		if (iStartRowIndex < 1 || iStartRowIndex > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("Row Index", "value cannot be less than 1 and greater than max row index");
		}
		if (iStartRowIndex + iCount > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("End Row Index", "Value cannot be greater than max row index");
		}
		if (value < 0.0)
		{
			throw new ArgumentOutOfRangeException("value");
		}
		for (int i = 0; i < iCount; i++)
		{
			InnerSetRowHeight(iStartRowIndex++, value, bIsBadFontHeight: true, MeasureUnits.Pixel, bRaiseEvents: true);
		}
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
		m_book.IsStartsOrEndsWith = true;
		OfficeFindOptions findOptions = ((!ignoreCase) ? OfficeFindOptions.MatchCase : OfficeFindOptions.None);
		return FindFirst(findValue, flags, findOptions);
	}

	public IRange FindStringEndsWith(string findValue, OfficeFindType flags)
	{
		return FindStringEndsWith(findValue, flags, ignoreCase: false);
	}

	public IRange FindStringEndsWith(string findValue, OfficeFindType flags, bool ignoreCase)
	{
		m_book.IsStartsOrEndsWith = false;
		OfficeFindOptions findOptions = ((!ignoreCase) ? OfficeFindOptions.MatchCase : OfficeFindOptions.None);
		return FindFirst(findValue, flags, findOptions);
	}

	public IRange FindFirst(string findValue, OfficeFindType flags, OfficeFindOptions findOptions)
	{
		IRange[] array = Find(UsedRange, findValue, flags, findOptions, bIsFindFirst: true);
		if (array == null)
		{
			return null;
		}
		return array[0];
	}

	public IRange FindFirst(double findValue, OfficeFindType flags)
	{
		IRange[] array = Find(UsedRange, findValue, flags, bIsFindFirst: true);
		if (array == null)
		{
			return null;
		}
		return array[0];
	}

	public IRange FindFirst(bool findValue)
	{
		IRange[] array = Find(UsedRange, findValue ? ((byte)1) : ((byte)0), bIsError: false, bIsFindFirst: true);
		if (array == null)
		{
			return null;
		}
		return array[0];
	}

	public IRange FindFirst(DateTime findValue)
	{
		double findValue2 = UtilityMethods.ConvertDateTimeToNumber(findValue);
		IRange[] array = Find(UsedRange, findValue2, OfficeFindType.Number, bIsFindFirst: true);
		if (array == null)
		{
			return null;
		}
		return array[0];
	}

	public IRange FindFirst(TimeSpan findValue)
	{
		double findValue2 = (double)findValue.Days + (double)(findValue.Hours * 360000 + findValue.Minutes * 6000 + findValue.Seconds * 100 + findValue.Milliseconds) / 8640000.0;
		IRange[] array = Find(UsedRange, findValue2, OfficeFindType.Number, bIsFindFirst: true);
		if (array == null)
		{
			return null;
		}
		return array[0];
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags)
	{
		return FindAll(findValue, flags, OfficeFindOptions.None);
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags, OfficeFindOptions findOptions)
	{
		if (findValue == null || findValue.Length == 0)
		{
			return null;
		}
		return Find(UsedRange, findValue, flags, findOptions, bIsFindFirst: false);
	}

	public IRange[] FindAll(double findValue, OfficeFindType flags)
	{
		bool num = (flags & OfficeFindType.FormulaValue) == OfficeFindType.FormulaValue;
		bool flag = (flags & OfficeFindType.Number) == OfficeFindType.Number;
		if (!(num || flag))
		{
			throw new ArgumentException("Parameter flags is not valid.", "flags");
		}
		return Find(UsedRange, findValue, flags, bIsFindFirst: false);
	}

	public IRange[] FindAll(bool findValue)
	{
		return Find(UsedRange, findValue ? ((byte)1) : ((byte)0), bIsError: false, bIsFindFirst: false);
	}

	public IRange[] FindAll(DateTime findValue)
	{
		double findValue2 = UtilityMethods.ConvertDateTimeToNumber(findValue);
		return FindAll(findValue2, OfficeFindType.Number | OfficeFindType.FormulaValue);
	}

	public IRange[] FindAll(TimeSpan findValue)
	{
		double totalDays = findValue.TotalDays;
		return FindAll(totalDays, OfficeFindType.Number | OfficeFindType.FormulaValue);
	}

	public void SaveAs(Stream stream, string separator)
	{
		SaveAs(stream, separator, Encoding.UTF8);
	}

	public void SaveAsInternal(Stream stream, string separator, Encoding encoding)
	{
		ParseData();
		StreamWriter streamWriter = new StreamWriter(stream, encoding);
		for (int i = m_iFirstRow; i <= m_iLastRow; i++)
		{
			if (!IsRowEmpty(i, bCheckStyle: false))
			{
				for (int j = m_iFirstColumn; j <= m_iLastColumn; j++)
				{
					long cellIndex = RangeImpl.GetCellIndex(j, i);
					TRangeValueType cellType = m_dicRecordsCells.GetCellType(i, j);
					string empty = string.Empty;
					if (cellType != 0)
					{
						empty = m_dicRecordsCells.GetValue(cellIndex, i, j, Range, separator);
						if (empty.Contains('\n'.ToString()) || empty.Contains(separator))
						{
							empty = "\"" + empty + "\"";
						}
						streamWriter.Write(empty);
					}
					if (j != m_iLastColumn)
					{
						streamWriter.Write(separator);
					}
				}
			}
			streamWriter.WriteLine();
		}
		streamWriter.Flush();
		stream.Flush();
	}

	public void SaveAs(Stream stream, string separator, Encoding encoding)
	{
		if (stream == null)
		{
			throw new ArgumentException("stream");
		}
		if (separator == null || separator.Length == 0)
		{
			throw new ArgumentException("separator");
		}
		SaveAsInternal(stream, separator, encoding);
	}

	public void SetDefaultColumnStyle(int iColumnIndex, IStyle defaultStyle)
	{
		SetDefaultRowColumnStyle(iColumnIndex, iColumnIndex, defaultStyle, m_arrColumnInfo, CreateColumnOutline, bIsRow: false);
		WorksheetHelper.AccessRow(this, iColumnIndex);
	}

	public void SetDefaultColumnStyle(int iStartColumnIndex, int iEndColumnIndex, IStyle defaultStyle)
	{
		ParseData();
		ushort num = (ushort)ConvertStyleToCorrectIndex(defaultStyle);
		for (int i = iStartColumnIndex; i <= iEndColumnIndex; i++)
		{
			IOutline outline = m_arrColumnInfo[i];
			if (outline == null)
			{
				outline = CreateColumnOutline(i);
			}
			outline.ExtendedFormatIndex = num;
			SetCellStyle(i, num);
		}
		WorksheetHelper.AccessColumn(this, iStartColumnIndex);
		WorksheetHelper.AccessColumn(this, iEndColumnIndex);
	}

	public void SetDefaultRowStyle(int iRowIndex, IStyle defaultStyle)
	{
		ushort num = (ushort)ConvertStyleToCorrectIndex(defaultStyle);
		WorksheetHelper.AccessRow(this, iRowIndex);
		iRowIndex--;
		RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, iRowIndex, bCreate: true);
		if (Rows.Length != 0)
		{
			IRange[] cells = Rows[iRowIndex].Cells;
			foreach (IRange range in cells)
			{
				orCreateRow.SetCellStyle(iRowIndex, range.Column - 1, num, base.Application.RowStorageAllocationBlockSize);
			}
		}
		orCreateRow.ExtendedFormatIndex = num;
	}

	public void SetDefaultRowStyle(int iStartRowIndex, int iEndRowIndex, IStyle defaultStyle)
	{
		ushort num = (ushort)ConvertStyleToCorrectIndex(defaultStyle);
		WorksheetHelper.AccessRow(this, iStartRowIndex);
		WorksheetHelper.AccessRow(this, iEndRowIndex);
		iStartRowIndex--;
		iEndRowIndex--;
		for (int i = iStartRowIndex; i <= iEndRowIndex; i++)
		{
			RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, i, bCreate: true);
			if (Rows.Length != 0)
			{
				IRange[] cells = Rows[i].Cells;
				foreach (IRange range in cells)
				{
					orCreateRow.SetCellStyle(i, range.Column - 1, num, base.Application.RowStorageAllocationBlockSize);
				}
			}
			orCreateRow.ExtendedFormatIndex = num;
		}
	}

	private void SetCellStyle(int iColIndex, ushort XFindex)
	{
		for (int i = CellRecords.FirstRow - 1; i <= CellRecords.LastRow; i++)
		{
			RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, i, bCreate: false);
			if (orCreateRow != null && orCreateRow.ExtendedFormatIndex != 0)
			{
				ICellPositionFormat cellRecord = m_dicRecordsCells.GetCellRecord(i + 1, iColIndex);
				if (cellRecord != null)
				{
					cellRecord.ExtendedFormatIndex = XFindex;
					m_dicRecordsCells.AddRecord(cellRecord, bIgnoreStyles: false);
				}
				else
				{
					cellRecord = m_dicRecordsCells.CreateCell(i + 1, iColIndex, TBIFFRecord.Blank);
					cellRecord.ExtendedFormatIndex = XFindex;
					m_dicRecordsCells.AddRecord(cellRecord, bIgnoreStyles: false);
				}
			}
		}
	}

	public IStyle GetDefaultColumnStyle(int iColumnIndex)
	{
		ParseData();
		if (iColumnIndex < 1 || iColumnIndex > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("iColumnIndex", "Value cannot be less than 1 and greater than m_book.MaxColumnCount.");
		}
		int iXFIndex = ((int?)((IOutline)m_arrColumnInfo[iColumnIndex])?.ExtendedFormatIndex) ?? m_book.DefaultXFIndex;
		return new ExtendedFormatWrapper(m_book, iXFIndex);
	}

	public IStyle GetDefaultRowStyle(int iRowIndex)
	{
		if (iRowIndex < 1 || iRowIndex > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("iRowIndex", "Value cannot be less than 1 and greater than m_book.MaxColumnCount.");
		}
		RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, iRowIndex - 1, bCreate: false);
		int iXFIndex = ((orCreateRow != null && m_book.IsFormatted(orCreateRow.ExtendedFormatIndex)) ? orCreateRow.ExtendedFormatIndex : m_book.DefaultXFIndex);
		return new ExtendedFormatWrapper(m_book, iXFIndex);
	}

	public void FreeRange(IRange range)
	{
		int i = range.Row;
		for (int lastRow = range.LastRow; i <= lastRow; i++)
		{
			int j = range.Column;
			for (int lastColumn = range.LastColumn; j <= lastColumn; j++)
			{
				FreeRange(i, j);
			}
		}
	}

	public void FreeRange(int iRow, int iColumn)
	{
		ParseData();
		CellRecords.FreeRange(iRow, iColumn);
	}

	[CLSCompliant(false)]
	public override void Serialize(OffsetArrayList records)
	{
		if (base.ParseOnDemand)
		{
			records.AddList(m_arrRecords);
		}
		else
		{
			Serialize(records, bClipboard: false);
		}
	}

	private void SerializeNotParsedWorksheet(OffsetArrayList records)
	{
		throw new NotImplementedException();
	}

	[CLSCompliant(false)]
	public void SerializeForClipboard(OffsetArrayList records)
	{
		Serialize(records, bClipboard: true);
	}

	[CLSCompliant(false)]
	protected void SerializeColumnInfo(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		int num = SerializeGroupColumnInfo(records);
		if (num < 255)
		{
			ColumnInfoRecord columnInfoRecord = (ColumnInfoRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ColumnInfo);
			columnInfoRecord.FirstColumn = (ushort)(num + 1);
			columnInfoRecord.LastColumn = 255;
			columnInfoRecord.ColumnWidth = (ushort)(StandardWidth * 256.0);
			columnInfoRecord.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
			records.Add(columnInfoRecord);
		}
	}

	[CLSCompliant(false)]
	protected int SerializeGroupColumnInfo(OffsetArrayList records)
	{
		int i = 1;
		int num = 1;
		ColumnInfoRecord columnInfoRecord = null;
		ColumnInfoRecord columnInfoRecord2 = null;
		while (i <= 256)
		{
			for (; i <= 256; i++)
			{
				columnInfoRecord = m_arrColumnInfo[i];
				if (columnInfoRecord != null)
				{
					break;
				}
			}
			if (columnInfoRecord == null)
			{
				break;
			}
			num = i;
			do
			{
				num++;
				columnInfoRecord2 = m_arrColumnInfo[num];
				if (columnInfoRecord.CompareTo(columnInfoRecord2) != 0)
				{
					columnInfoRecord2 = null;
				}
			}
			while (num <= 256 && columnInfoRecord2 != null);
			if (columnInfoRecord2 == null)
			{
				num--;
				columnInfoRecord2 = m_arrColumnInfo[num];
			}
			ColumnInfoRecord columnInfoRecord3;
			if (i == num)
			{
				columnInfoRecord3 = (ColumnInfoRecord)columnInfoRecord.Clone();
			}
			else
			{
				columnInfoRecord3 = (ColumnInfoRecord)columnInfoRecord.Clone();
				columnInfoRecord3.LastColumn = columnInfoRecord2.LastColumn;
			}
			columnInfoRecord3.ColumnWidth = columnInfoRecord3.ColumnWidth;
			records.Add(columnInfoRecord3);
			i = num + 1;
		}
		return num - 1;
	}

	private bool CompareDVWithoutRanges(DVRecord curDV, DVRecord dvToAdd)
	{
		if (curDV == null)
		{
			return dvToAdd == null;
		}
		if (curDV.Condition == dvToAdd.Condition && curDV.DataType == dvToAdd.DataType && curDV.ErrorBoxText == dvToAdd.ErrorBoxText && curDV.ErrorBoxTitle == dvToAdd.ErrorBoxTitle && curDV.ErrorStyle == dvToAdd.ErrorStyle && curDV.IsEmptyCell == dvToAdd.IsEmptyCell && curDV.IsShowErrorBox == dvToAdd.IsShowErrorBox && curDV.IsShowPromptBox == dvToAdd.IsShowPromptBox && curDV.IsStrListExplicit == dvToAdd.IsStrListExplicit && curDV.IsSuppressArrow == dvToAdd.IsSuppressArrow && curDV.PromtBoxText == dvToAdd.PromtBoxText && curDV.PromtBoxTitle == dvToAdd.PromtBoxTitle && Ptg.CompareArrays(curDV.FirstFormulaTokens, dvToAdd.FirstFormulaTokens))
		{
			return Ptg.CompareArrays(curDV.SecondFormulaTokens, dvToAdd.SecondFormulaTokens);
		}
		return false;
	}

	private void MergeDVRanges(DVRecord curDv, DVRecord dvToAdd)
	{
		if (curDv == null)
		{
			throw new ArgumentNullException("curDv");
		}
		if (dvToAdd == null)
		{
			throw new ArgumentNullException("dvToAdd");
		}
		curDv.AddRange(dvToAdd.AddrList);
	}

	[CLSCompliant(false)]
	protected override void SerializeMsoDrawings(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		base.SerializeMsoDrawings(records);
		if ((base.Application.SkipOnSave & OfficeSkipExtRecords.Drawings) == OfficeSkipExtRecords.Drawings || m_arrNotesByCellIndex == null)
		{
			return;
		}
		foreach (NoteRecord value in m_arrNotesByCellIndex.Values)
		{
			records.Add(value);
		}
	}

	private void Serialize(OffsetArrayList records, bool bClipboard)
	{
		if (m_arrNotes != null)
		{
			m_arrNotes.Clear();
			m_arrNotesByCellIndex.Clear();
		}
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (!base.IsSupported)
		{
			records.AddList(m_arrRecords);
			return;
		}
		if (!base.IsParsed)
		{
			SerializeNotParsedWorksheet(records);
			return;
		}
		m_bof.Type = BOFRecord.TType.TYPE_WORKSHEET;
		records.Add(m_bof);
		IndexRecord indexRecord = null;
		int num = m_iLastRow - m_iFirstRow + 1;
		int num2 = 0;
		if (num > 0)
		{
			int num3 = num % 32;
			num2 = num / 32;
			if (num3 != 0)
			{
				num2++;
			}
		}
		if (!bClipboard)
		{
			indexRecord = (IndexRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Index);
			indexRecord.DbCells = new int[num2];
			indexRecord.FirstRow = ((m_iLastRow != m_iFirstRow || m_iFirstRow != -1) ? (m_iFirstRow - 1) : 0);
			indexRecord.LastRow = ((m_iLastRow != m_iFirstRow || m_iFirstRow != -1) ? m_iLastRow : 0);
			records.Add(indexRecord);
		}
		m_book.InnerCalculation.Serialize(records);
		records.Add(m_pageSetup);
		SerializeProtection(records, bContentNotNecessary: false);
		DefaultColWidthRecord defaultColWidthRecord = (DefaultColWidthRecord)BiffRecordFactory.GetRecord(TBIFFRecord.DefaultColWidth);
		defaultColWidthRecord.Width = (ushort)m_dStandardColWidth;
		records.Add(defaultColWidthRecord);
		SerializeColumnInfo(records);
		if (m_arrSortRecords != null)
		{
			records.AddList(m_arrSortRecords);
		}
		DimensionsRecord dimensionsRecord = (DimensionsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Dimensions);
		dimensionsRecord.LastRow = ((m_iLastRow != m_iFirstRow || m_iFirstRow != -1) ? m_iLastRow : 0);
		dimensionsRecord.LastColumn = (ushort)((m_iLastColumn != m_iFirstColumn || m_iFirstColumn != int.MaxValue) ? ((ushort)m_iLastColumn) : 0);
		dimensionsRecord.FirstRow = ((m_iLastRow != m_iFirstRow || m_iFirstRow != -1) ? (m_iFirstRow - 1) : 0);
		dimensionsRecord.FirstColumn = (ushort)((m_iLastColumn != m_iFirstColumn || m_iFirstColumn != int.MaxValue) ? ((uint)(m_iFirstColumn - 1)) : 0u);
		records.Add(dimensionsRecord);
		List<DBCellRecord> list = new List<DBCellRecord>();
		num2 = m_dicRecordsCells.Serialize(records, list);
		if (!bClipboard)
		{
			indexRecord.DbCellRecords = list;
		}
		SerializeMsoDrawings(records);
		if (m_arrDConRecords != null)
		{
			records.AddList(m_arrDConRecords);
		}
		SerializeHeaderFooterPictures(records);
		SerializeWindowTwo(records);
		SerializePageLayoutView(records);
		SerializeWindowZoom(records);
		if (m_pane != null)
		{
			if (VerticalSplit == 0 && HorizontalSplit == 0)
			{
				m_pane.ActivePane = 3;
			}
			else if (VerticalSplit == 0)
			{
				m_pane.ActivePane = 2;
			}
			else if (HorizontalSplit == 0)
			{
				m_pane.ActivePane = 1;
			}
			records.Add(m_pane);
		}
		CreateAllSelections();
		records.AddList(m_arrSelections);
		if (m_mergedCells != null)
		{
			m_mergedCells.Serialize(records);
		}
		records.AddList(PreserveExternalConnection);
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.UnkMarker));
		SerializeMacrosSupport(records);
		SerializeSheetLayout(records);
		SerializeSheetProtection(records);
		if (m_tableRecords != null)
		{
			records.AddRange(m_tableRecords);
		}
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.EOF));
		if (m_arrNotes != null)
		{
			m_arrNotes.Clear();
			m_arrNotesByCellIndex.Clear();
		}
	}

	protected void RaiseColumnWidthChangedEvent(int iColumn, double dNewValue)
	{
		if (this.ColumnWidthChanged != null)
		{
			ValueChangedEventArgs e = new ValueChangedEventArgs(iColumn, dNewValue, "ColumnWidth");
			this.ColumnWidthChanged(this, e);
		}
	}

	protected void RaiseRowHeightChangedEvent(int iRow, double dNewValue)
	{
		if (this.RowHeightChanged != null)
		{
			ValueChangedEventArgs e = new ValueChangedEventArgs(iRow, dNewValue, "RowHeight");
			this.RowHeightChanged(this, e);
		}
	}

	private void NormalFont_OnAfterChange(object sender, EventArgs e)
	{
		if (m_iFirstRow <= 0)
		{
			return;
		}
		for (int i = m_iFirstRow; i <= m_iLastRow; i++)
		{
			RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, i, bCreate: false);
			if (orCreateRow != null && !orCreateRow.IsBadFontHeight)
			{
				AutofitRow(i);
			}
		}
	}

	public void SetFormulaValue(int iRow, int iColumn, string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value[0] == '#' && FormulaUtil.ErrorNameToCode.ContainsKey(value))
		{
			SetFormulaErrorValue(iRow, iColumn, value);
			return;
		}
		IRange range = Range[iRow, iColumn];
		bool result2;
		if (double.TryParse(value, out var result) && (!range.NumberFormat.Contains("@") || range.NumberFormat.Length != 1))
		{
			SetFormulaNumberValue(iRow, iColumn, result);
		}
		else if (bool.TryParse(value, out result2))
		{
			SetFormulaBoolValue(iRow, iColumn, result2);
		}
		else
		{
			SetFormulaStringValue(iRow, iColumn, value);
		}
	}

	public void SetValue(int iRow, int iColumn, string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.Length == 0)
		{
			SetBlankRecord(iRow, iColumn);
			return;
		}
		if (value[0] == '=')
		{
			SetFormula(iRow, iColumn, value.Substring(1));
			return;
		}
		if (value[0] == '#' && FormulaUtil.ErrorNameToCode.ContainsKey(value) && GetFormulaErrorValue(iRow, iColumn) != null)
		{
			SetFormulaErrorValue(iRow, iColumn, value);
			return;
		}
		if (this[iRow, iColumn].HasFormula)
		{
			SetFormulaValue(iRow, iColumn, value);
			return;
		}
		RangeImpl rangeImpl = Range[iRow, iColumn] as RangeImpl;
		DateTime dateValue;
		bool flag = rangeImpl.TryParseDateTime(value, out dateValue);
		if (double.TryParse(value, out var result) && !flag)
		{
			SetNumber(iRow, iColumn, result);
		}
		else if (flag)
		{
			rangeImpl.Value = value;
		}
		else
		{
			SetString(iRow, iColumn, value);
		}
	}

	public void SetNumber(int iRow, int iColumn, double value)
	{
		int iXFIndex = RemoveString(iRow, iColumn);
		RKRecord rKRecord = TryCreateRkRecord(iRow, iColumn, value, iXFIndex);
		if (rKRecord != null)
		{
			InnerSetCell(iColumn, iRow, rKRecord);
		}
		else
		{
			SetNumberRecord(iRow, iColumn, value, iXFIndex);
		}
	}

	public void SetBoolean(int iRow, int iColumn, bool value)
	{
		int iXFIndex = RemoveString(iRow, iColumn);
		BoolErrRecord boolErrRecord = (BoolErrRecord)GetRecord(TBIFFRecord.BoolErr, iRow, iColumn, iXFIndex);
		boolErrRecord.IsErrorCode = false;
		boolErrRecord.BoolOrError = (value ? ((byte)1) : ((byte)0));
		InnerSetCell(iColumn, iRow, boolErrRecord);
	}

	public void SetText(int iRow, int iColumn, string value)
	{
		if (value == null || value.Length == 0)
		{
			throw new ArgumentOutOfRangeException("Text value cannot be null or empty");
		}
		SetString(iRow, iColumn, value);
	}

	public void SetFormula(int iRow, int iColumn, string value)
	{
		SetFormula(iRow, iColumn, value, bIsR1C1: false);
	}

	public void SetFormula(int iRow, int iColumn, string value, bool bIsR1C1)
	{
		if (value == null || value.Length == 0 || value[0] == '=')
		{
			throw new ArgumentOutOfRangeException("Text value cannot be null or empty. First symbol of formula cannot be '='");
		}
		SetFormulaValue(iRow, iColumn, value, bIsR1C1);
	}

	public void SetError(int iRow, int iColumn, string value)
	{
		if (value == null || value.Length == 0 || value[0] != '#')
		{
			throw new ArgumentOutOfRangeException("Text value cannot be null or empty. First symbol must be '#'");
		}
		SetError(iRow, iColumn, value, isSetText: false);
	}

	public void SetBlank(int iRow, int iColumn)
	{
		SetBlankRecord(iRow, iColumn);
	}

	private void SetBlankRecord(int iRow, int iColumn)
	{
		int iXFIndex = RemoveString(iRow, iColumn);
		BiffRecordRaw record = GetRecord(TBIFFRecord.Blank, iRow, iColumn, iXFIndex);
		InnerSetCell(iColumn, iRow, record);
	}

	private void SetNumberRecord(int iRow, int iColumn, double value, int iXFIndex)
	{
		NumberRecord numberRecord = (NumberRecord)GetRecord(TBIFFRecord.Number, iRow, iColumn, iXFIndex);
		numberRecord.Value = value;
		InnerSetCell(iColumn, iRow, numberRecord);
	}

	private void SetRKRecord(int iRow, int iColumn, double value)
	{
		RKRecord rKRecord = (RKRecord)GetRecord(TBIFFRecord.RK, iRow, iColumn);
		rKRecord.RKNumber = value;
		InnerSetCell(iColumn, iRow, rKRecord);
	}

	private void SetFormulaValue(int iRow, int iColumn, string value, bool bIsR1C1)
	{
		int iXFIndex = RemoveString(iRow, iColumn);
		FormulaRecord formulaRecord = (FormulaRecord)GetRecord(TBIFFRecord.Formula, iRow, iColumn, iXFIndex);
		formulaRecord.ParsedExpression = m_book.FormulaUtil.ParseString(value, this, null, iRow - 1, iColumn - 1, bIsR1C1);
		InnerSetCell(iColumn, iRow, formulaRecord);
	}

	public void SetFormulaNumberValue(int iRow, int iColumn, double value)
	{
		if ((GetCellType(iRow, iColumn, bNeedFormulaSubType: false) & TRangeValueType.Formula) != TRangeValueType.Formula)
		{
			throw new ArgumentException("Cannot sets formula value in cell that doesn't contain formula");
		}
		SetFormulaValue(iRow, iColumn, value);
	}

	public void SetFormulaErrorValue(int iRow, int iColumn, string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!FormulaUtil.ErrorNameToCode.ContainsKey(value))
		{
			throw new ArgumentOutOfRangeException("Value does not valid error string.");
		}
		if ((GetCellType(iRow, iColumn, bNeedFormulaSubType: false) & TRangeValueType.Formula) != TRangeValueType.Formula)
		{
			throw new ArgumentException("Cannot sets formula value in cell that doesn't contain formula");
		}
		double boolErrorValue = FormulaRecord.GetBoolErrorValue((byte)FormulaUtil.ErrorNameToCode[value], bIsError: true);
		SetFormulaValue(iRow, iColumn, boolErrorValue);
	}

	public void SetFormulaBoolValue(int iRow, int iColumn, bool value)
	{
		if ((GetCellType(iRow, iColumn, bNeedFormulaSubType: false) & TRangeValueType.Formula) != TRangeValueType.Formula)
		{
			throw new ArgumentException("Cannot sets formula value in cell that doesn't contain formula");
		}
		double boolErrorValue = FormulaRecord.GetBoolErrorValue(value ? ((byte)1) : ((byte)0), bIsError: false);
		SetFormulaValue(iRow, iColumn, boolErrorValue);
	}

	public void SetFormulaStringValue(int iRow, int iColumn, string value)
	{
		if ((GetCellType(iRow, iColumn, bNeedFormulaSubType: false) & TRangeValueType.Formula) != TRangeValueType.Formula)
		{
			throw new ArgumentException("Cannot sets formula value in cell that doesn't contain formula");
		}
		StringRecord stringRecord = (StringRecord)RecordExtractor.GetRecord(519);
		stringRecord.Value = value;
		double dEF_STRING_VALUE = FormulaRecord.DEF_STRING_VALUE;
		SetFormulaValue(iRow, iColumn, dEF_STRING_VALUE, stringRecord);
	}

	public void SetError(int iRow, int iColumn, string value, bool isSetText)
	{
		if (!FormulaUtil.ErrorNameToCode.TryGetValue(value, out var value2))
		{
			if (!isSetText)
			{
				throw new ArgumentOutOfRangeException("Cannot parse error code.");
			}
			SetString(iRow, iColumn, value);
		}
		else
		{
			int iXFIndex = RemoveString(iRow, iColumn);
			BoolErrRecord boolErrRecord = (BoolErrRecord)GetRecord(TBIFFRecord.BoolErr, iRow, iColumn, iXFIndex);
			boolErrRecord.IsErrorCode = true;
			boolErrRecord.BoolOrError = (byte)value2;
			InnerSetCell(iColumn, iRow, boolErrRecord);
		}
	}

	private void SetString(int iRow, int iColumn, string value)
	{
		int iXFIndex = RemoveString(iRow, iColumn);
		int sSTIndex = m_book.InnerSST.AddIncrease(value);
		LabelSSTRecord labelSSTRecord = (LabelSSTRecord)GetRecord(TBIFFRecord.LabelSST, iRow, iColumn, iXFIndex);
		labelSSTRecord.SSTIndex = sSTIndex;
		InnerSetCell(iColumn, iRow, labelSSTRecord);
	}

	private int RemoveString(int iRow, int iColumn)
	{
		ParseData();
		ICellPositionFormat cellRecord = m_dicRecordsCells.GetCellRecord(iRow, iColumn);
		int num = m_book.DefaultXFIndex;
		if (cellRecord != null)
		{
			num = cellRecord.ExtendedFormatIndex;
		}
		else
		{
			RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, iRow - 1, bCreate: false);
			if (orCreateRow != null)
			{
				num = orCreateRow.ExtendedFormatIndex;
			}
			if (num == 0 || num == m_book.DefaultXFIndex)
			{
				ColumnInfoRecord columnInfoRecord = m_arrColumnInfo[iColumn];
				if (columnInfoRecord != null)
				{
					num = columnInfoRecord.ExtendedFormatIndex;
				}
			}
		}
		if (cellRecord is LabelSSTRecord { SSTIndex: var sSTIndex })
		{
			m_book.InnerSST.RemoveDecrease(sSTIndex);
		}
		return num;
	}

	internal int GetXFIndex(int iRow, int iColumn)
	{
		ParseData();
		int extendedFormatIndex = m_dicRecordsCells.GetExtendedFormatIndex(iRow, iColumn);
		if (extendedFormatIndex < 0)
		{
			RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, iRow - 1, bCreate: false);
			int num = ((orCreateRow != null && m_book.IsFormatted(orCreateRow.ExtendedFormatIndex)) ? orCreateRow.ExtendedFormatIndex : 0);
			if (num != 0 && num != m_book.DefaultXFIndex)
			{
				extendedFormatIndex = orCreateRow.ExtendedFormatIndex;
			}
			else
			{
				ColumnInfoRecord columnInfoRecord = m_arrColumnInfo[iColumn];
				if (columnInfoRecord != null)
				{
					extendedFormatIndex = columnInfoRecord.ExtendedFormatIndex;
				}
			}
		}
		if (extendedFormatIndex >= 0)
		{
			return extendedFormatIndex;
		}
		return m_book.DefaultXFIndex;
	}

	internal int GetXFIndex(int iRow)
	{
		ParseData();
		int num = m_dicRecordsCells.GetExtendedFormatIndexByRow(iRow);
		if (num < 0)
		{
			RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, iRow - 1, bCreate: false);
			int num2 = ((orCreateRow != null && m_book.IsFormatted(orCreateRow.ExtendedFormatIndex)) ? orCreateRow.ExtendedFormatIndex : 0);
			if (num2 != 0 && num2 != m_book.DefaultXFIndex)
			{
				num = orCreateRow.ExtendedFormatIndex;
			}
		}
		if (num >= 0)
		{
			return num;
		}
		return m_book.DefaultXFIndex;
	}

	internal int GetColumnXFIndex(int firstColumn)
	{
		ParseData();
		int num = m_dicRecordsCells.GetExtendedFormatIndexByColumn(firstColumn);
		if (num < 0)
		{
			ColumnInfoRecord columnInfoRecord = (ColumnInfoRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ColumnInfo);
			columnInfoRecord.FirstColumn = (ushort)(firstColumn - 1);
			int num2 = ((columnInfoRecord != null && m_book.IsFormatted(columnInfoRecord.ExtendedFormatIndex)) ? columnInfoRecord.ExtendedFormatIndex : 0);
			if (num2 != 0 && num2 != m_book.DefaultXFIndex)
			{
				num = columnInfoRecord.ExtendedFormatIndex;
			}
		}
		if (num >= 0)
		{
			return num;
		}
		return m_book.DefaultXFIndex;
	}

	[CLSCompliant(false)]
	protected internal RKRecord TryCreateRkRecord(int iRow, int iColumn, double value)
	{
		ParseData();
		int num = RKRecord.ConvertToRKNumber(value);
		if (num != int.MaxValue)
		{
			RKRecord obj = (RKRecord)GetRecord(TBIFFRecord.RK, iRow, iColumn);
			obj.SetConvertedNumber(num);
			return obj;
		}
		return null;
	}

	[CLSCompliant(false)]
	protected internal RKRecord TryCreateRkRecord(int iRow, int iColumn, double value, int iXFIndex)
	{
		ParseData();
		int num = RKRecord.ConvertToRKNumber(value);
		if (num != int.MaxValue)
		{
			RKRecord obj = (RKRecord)GetRecord(TBIFFRecord.RK, iRow, iColumn, iXFIndex);
			obj.SetConvertedNumber(num);
			return obj;
		}
		return null;
	}

	[CLSCompliant(false)]
	public BiffRecordRaw GetRecord(TBIFFRecord recordCode, int iRow, int iColumn)
	{
		return GetRecord(recordCode, iRow, iColumn, GetXFIndex(iRow, iColumn));
	}

	private BiffRecordRaw GetRecord(TBIFFRecord recordCode, int iRow, int iColumn, int iXFIndex)
	{
		ICellPositionFormat obj = RecordExtractor.GetRecord((int)recordCode) as ICellPositionFormat;
		obj.Row = iRow - 1;
		obj.Column = iColumn - 1;
		obj.ExtendedFormatIndex = (ushort)iXFIndex;
		return obj as BiffRecordRaw;
	}

	private void SetFormulaValue(int iRow, int iColumn, double value)
	{
		SetFormulaValue(iRow, iColumn, value, null);
	}

	private void SetFormulaValue(int iRow, int iColumn, double value, StringRecord strRecord)
	{
		ParseData();
		m_dicRecordsCells.Table.SetFormulaValue(iRow, iColumn, value, strRecord);
	}

	public string GetFormula(int row, int column, bool bR1C1)
	{
		return GetFormula(row, column, bR1C1, isForSerialization: false);
	}

	public string GetFormula(int row, int column, bool bR1C1, bool isForSerialization)
	{
		return GetFormula(row, column, bR1C1, m_book.FormulaUtil, isForSerialization);
	}

	public string GetFormula(int row, int column, bool bR1C1, FormulaUtil formulaUtil, bool isForSerialization)
	{
		ParseData();
		Ptg[] formulaValue = m_dicRecordsCells.Table.GetFormulaValue(row, column);
		row--;
		column--;
		return GetFormula(row, column, formulaValue, bR1C1, formulaUtil, isForSerialization);
	}

	private string GetFormula(int row, int column, Ptg[] arrTokens, bool bR1C1, FormulaUtil formulaUtil, bool isForSerialization)
	{
		if (arrTokens == null)
		{
			return null;
		}
		return "=" + formulaUtil.ParsePtgArray(arrTokens, row, column, bR1C1, null, bRemoveSheetNames: false, isForSerialization, this);
	}

	private string GetFormulaArray(FormulaRecord formula)
	{
		ArrayRecord arrayRecord = CellRecords.GetArrayRecord(formula.Row + 1, formula.Column + 1);
		if (arrayRecord == null)
		{
			return null;
		}
		return m_book.FormulaUtil.ParsePtgArray(arrayRecord.Formula, arrayRecord.FirstRow, arrayRecord.FirstColumn, bR1C1: false, null, bRemoveSheetNames: false, isForSerialization: false, this);
	}

	public string GetStringValue(long cellIndex)
	{
		ParseData();
		return GetText(RangeImpl.GetRowFromCellIndex(cellIndex), RangeImpl.GetColumnFromCellIndex(cellIndex));
	}

	public string GetText(int row, int column)
	{
		ParseData();
		return m_dicRecordsCells.Table.GetStringValue(row, column, m_book.InnerSST);
	}

	public string GetFormulaStringValue(int row, int column)
	{
		ParseData();
		return m_dicRecordsCells.Table.GetFormulaStringValue(row, column, m_book.InnerSST);
	}

	public double GetNumber(int row, int column)
	{
		ParseData();
		return m_dicRecordsCells.Table.GetNumberValue(row, column);
	}

	public double GetFormulaNumberValue(int row, int column)
	{
		ParseData();
		return m_dicRecordsCells.Table.GetFormulaNumberValue(row, column);
	}

	public string GetError(int row, int column)
	{
		ParseData();
		return m_dicRecordsCells.Table.GetErrorValue(row, column);
	}

	internal string GetErrorValueToString(byte value, int row)
	{
		return m_dicRecordsCells.Table.GetErrorValue(value, row);
	}

	public string GetFormulaErrorValue(int row, int column)
	{
		ParseData();
		return m_dicRecordsCells.Table.GetFormulaErrorValue(row, column);
	}

	public bool GetBoolean(int row, int column)
	{
		ParseData();
		return m_dicRecordsCells.Table.GetBoolValue(row, column) > 0;
	}

	public bool GetFormulaBoolValue(int row, int column)
	{
		ParseData();
		return m_dicRecordsCells.Table.GetFormulaBoolValue(row, column) > 0;
	}

	public bool HasArrayFormulaRecord(int row, int column)
	{
		ParseData();
		Ptg[] formulaValue = m_dicRecordsCells.Table.GetFormulaValue(row, column);
		return HasArrayFormula(formulaValue);
	}

	public bool HasArrayFormula(Ptg[] arrTokens)
	{
		if (arrTokens == null || arrTokens.Length != 1)
		{
			return false;
		}
		Ptg ptg = arrTokens[0];
		if (ptg.TokenCode != FormulaToken.tExp)
		{
			return false;
		}
		ControlPtg controlPtg = ptg as ControlPtg;
		return m_dicRecordsCells.Table.HasFormulaArrayRecord(controlPtg.RowIndex, controlPtg.ColumnIndex);
	}

	public TRangeValueType GetCellType(int row, int column, bool bNeedFormulaSubType)
	{
		ParseData();
		if (m_dicRecordsCells != null && m_dicRecordsCells.Table != null)
		{
			return m_dicRecordsCells.Table.GetCellType(row, column, bNeedFormulaSubType);
		}
		return TRangeValueType.Error;
	}

	public bool IsExternalFormula(int row, int column)
	{
		ParseData();
		Ptg[] formulaValue = m_dicRecordsCells.Table.GetFormulaValue(row, column);
		if (formulaValue != null)
		{
			int i = 0;
			for (int num = formulaValue.Length; i < num; i++)
			{
				if (formulaValue[i] is ISheetReference sheetReference)
				{
					int refIndex = sheetReference.RefIndex;
					if (m_book.IsExternalReference(refIndex))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	internal void OnCellValueChanged(object oldValue, object newValue, IRange range)
	{
		if (this.CellValueChanged != null)
		{
			CellValueChangedEventArgs cellValueChangedEventArgs = new CellValueChangedEventArgs();
			cellValueChangedEventArgs.OldValue = oldValue;
			cellValueChangedEventArgs.NewValue = newValue;
			cellValueChangedEventArgs.Range = range;
			this.CellValueChanged(this, cellValueChangedEventArgs);
		}
	}

	public int GetFirstRow()
	{
		return Rows[0].Row;
	}

	public int GetLastRow()
	{
		return Rows[Rows.Length - 1].Row;
	}

	public int GetRowCount()
	{
		return Rows.Length;
	}

	public int GetFirstColumn()
	{
		return Columns[0].Column;
	}

	public int GetLastColumn()
	{
		return Columns[Columns.Length - 1].Column;
	}

	public int GetColumnCount()
	{
		return Columns.Length;
	}

	internal ApplicationImpl GetAppImpl()
	{
		return base.AppImplementation;
	}

	internal int GetViewColumnWidthPixel(int column)
	{
		CheckColumnIndex(column);
		double columnWidth = GetColumnWidth(column + 1);
		double num = ((View == OfficeSheetView.PageLayout) ? 1.05 : 1.0);
		if (columnWidth > 1.0)
		{
			int num2 = (int)(columnWidth * (double)GetAppImpl().GetFontCalc2() + 0.5);
			int num3 = (int)((double)(GetAppImpl().GetFontCalc2() * GetAppImpl().GetFontCalc1()) / 256.0 + 0.5);
			return (int)((double)(num2 + num3) * num + 0.5);
		}
		return (int)((double)(int)(columnWidth * (double)(GetAppImpl().GetFontCalc2() + (int)((double)(GetAppImpl().GetFontCalc2() * GetAppImpl().GetFontCalc1()) / 256.0 + 0.5)) + 0.5) * num + 0.5);
	}

	internal double CharacterWidth(double width)
	{
		ApplicationImpl appImpl = GetAppImpl();
		int num = (int)(width * (double)appImpl.GetFontCalc2() + 0.5);
		int fontCalc = appImpl.GetFontCalc2();
		int fontCalc2 = appImpl.GetFontCalc1();
		int fontCalc3 = appImpl.GetFontCalc3();
		if (num < fontCalc + fontCalc3)
		{
			return 1.0 * (double)num / (double)(fontCalc + fontCalc3);
		}
		double num2 = (double)(int)((double)(num - (int)((double)(fontCalc * fontCalc2) / 256.0 + 0.5)) * 100.0 / (double)fontCalc + 0.5) / 100.0;
		if (num2 > 255.0)
		{
			num2 = 255.0;
		}
		return num2;
	}

	internal static int CharacterWidth(double width, ApplicationImpl application)
	{
		if (width > 1.0)
		{
			int num = (int)(width * (double)application.GetFontCalc2() + 0.5);
			int num2 = (int)((double)(application.GetFontCalc2() * application.GetFontCalc1()) / 256.0 + 0.5);
			return num + num2;
		}
		return (int)(width * (double)(application.GetFontCalc2() + (int)((double)(application.GetFontCalc2() * application.GetFontCalc1()) / 256.0 + 0.5)) + 0.5);
	}

	internal static void CheckColumnIndex(int columnIndex)
	{
		if (columnIndex < 0 || columnIndex > 16383)
		{
			throw new ArgumentException("Invalid column index.");
		}
	}

	internal static void CheckRowIndex(int rowIndex)
	{
		if (rowIndex < 0 || rowIndex > 1048575)
		{
			throw new ArgumentException("Invalid row index.");
		}
	}

	internal bool TryGetIntersectRange(string name, out IRange intersect)
	{
		intersect = null;
		bool flag = false;
		bool flag2 = false;
		string text = name.TrimStart('(').TrimEnd(')');
		string strSheetName = string.Empty;
		string[] array = text.Split(' ');
		if (array.Length > 1)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = AddSheetName(array[i]);
			}
			string[] array2 = array;
			string strRow;
			string strColumn;
			string strRow2;
			string strColumn2;
			foreach (string text2 in array2)
			{
				if (string.IsNullOrEmpty(text2) || text2.Trim().Length == 0)
				{
					continue;
				}
				if ((Names as WorksheetNamesCollection).Contains(text2, isFormulaNamedrange: true))
				{
					flag = true;
					continue;
				}
				if (((base.Workbook as WorkbookImpl).FormulaUtil.IsCellRange3D(text2, bR1C1: false, out strSheetName, out strRow, out strColumn, out strRow2, out strColumn2) || (base.Workbook as WorkbookImpl).FormulaUtil.IsCellRange(text2, bR1C1: false, out strRow, out strColumn, out strRow2, out strColumn2)) && GetRangeByString(text2, hasFormula: false) != null)
				{
					flag = true;
					continue;
				}
				if (IsEntireRange(text2))
				{
					flag = true;
					continue;
				}
				flag = false;
				break;
			}
			if (!flag)
			{
				array2 = array;
				foreach (string text3 in array2)
				{
					if (string.IsNullOrEmpty(text3) || text3.Trim().Length == 0)
					{
						continue;
					}
					if ((base.Workbook.Names as WorkbookNamesCollection).Contains(text3, isFormulaNamedrange: true))
					{
						flag = true;
						flag2 = true;
						continue;
					}
					if (((base.Workbook as WorkbookImpl).FormulaUtil.IsCellRange3D(text3, bR1C1: false, out strSheetName, out strRow, out strColumn, out strRow2, out strColumn2) || (base.Workbook as WorkbookImpl).FormulaUtil.IsCellRange(text3, bR1C1: false, out strRow, out strColumn, out strRow2, out strColumn2)) && GetRangeByString(text3, hasFormula: false) != null)
					{
						flag = true;
						continue;
					}
					if (IsEntireRange(text3))
					{
						flag = true;
						continue;
					}
					flag = false;
					flag2 = false;
					break;
				}
			}
			if (flag)
			{
				if (flag2)
				{
					intersect = GetIntersectionRange(base.Workbook.Names, array);
				}
				else
				{
					intersect = GetIntersectionRange(Names, array);
				}
			}
		}
		else
		{
			intersect = null;
		}
		if (flag)
		{
			return intersect != null;
		}
		return false;
	}

	private IRange GetIntersectionRange(INames names, string[] nameRanges)
	{
		IRange range = null;
		IRange range2 = null;
		IRange range3 = null;
		if (names is WorkbookNamesCollection)
		{
			if ((names as WorkbookNamesCollection).Contains(nameRanges[0], isFormulaNamedrange: true))
			{
				range = names[nameRanges[0]].RefersToRange;
			}
		}
		else if ((names as WorksheetNamesCollection).Contains(nameRanges[0], isFormulaNamedrange: true))
		{
			range = names[nameRanges[0]].RefersToRange;
		}
		if (range == null)
		{
			range = GetRangeByString(nameRanges[0], hasFormula: false);
		}
		for (int i = 1; i < nameRanges.Length; i++)
		{
			if (!string.IsNullOrEmpty(nameRanges[i]) && nameRanges[i].Trim().Length != 0)
			{
				if (names is WorkbookNamesCollection)
				{
					if ((names as WorkbookNamesCollection).Contains(nameRanges[i], isFormulaNamedrange: true))
					{
						range2 = names[nameRanges[i]].RefersToRange;
					}
				}
				else if ((names as WorksheetNamesCollection).Contains(nameRanges[i], isFormulaNamedrange: true))
				{
					range2 = names[nameRanges[i]].RefersToRange;
				}
				if (range2 == null)
				{
					range2 = GetRangeByString(nameRanges[i], hasFormula: false);
				}
				if (range is NameImpl)
				{
					range = (range as NameImpl).RefersToRange;
				}
				if (range2 is NameImpl)
				{
					range2 = (range2 as NameImpl).RefersToRange;
				}
				if (range != null && range2 != null)
				{
					range3 = range.IntersectWith(range2);
				}
			}
			range = range3;
			range2 = null;
		}
		return range3;
	}

	private bool IsEntireRange(string nameRange)
	{
		string text = nameRange;
		if (nameRange.IndexOf("!") >= 0)
		{
			text = nameRange.Substring(nameRange.IndexOf("!") + 1);
		}
		Match match = FormulaUtil.FullRowRangeRegex.Match(text);
		if (match.Success && match.Index == 0 && match.Length == text.Length)
		{
			return true;
		}
		match = FormulaUtil.FullColumnRangeRegex.Match(text);
		if (match.Success && match.Index == 0 && match.Length == text.Length)
		{
			return true;
		}
		return false;
	}

	private string AddSheetName(string address)
	{
		string text = string.Empty;
		string empty = string.Empty;
		int result = 0;
		if (address.Length > 2 && address[0] == '!')
		{
			for (int i = 1; i < address.Length && address[i] != '!'; i++)
			{
				text += address[i];
			}
		}
		if (int.TryParse(text, out result))
		{
			empty = base.Workbook.Worksheets[result].Name;
			address = address.Replace("!" + text + "!", empty + "!");
		}
		return address;
	}

	internal bool TryGetExternalIntersectRange(string name, out string intersect)
	{
		intersect = null;
		bool flag = false;
		string text = name.TrimStart('(').TrimEnd(')');
		string text2 = string.Empty;
		string[] array = text.Split(' ');
		if (array.Length > 1)
		{
			for (int i = 0; i < array.Length; i++)
			{
				string formula = array[i];
				if (!string.IsNullOrEmpty(formula) && formula.Trim().Length != 0)
				{
					if (!TryGetExternRangeAddress(base.Workbook as WorkbookImpl, ref formula))
					{
						flag = false;
						break;
					}
					if (text2 == string.Empty)
					{
						text2 = formula.Substring(0, formula.LastIndexOf('!') + 1);
					}
					if (!(text2 == formula.Substring(0, formula.LastIndexOf('!') + 1)))
					{
						flag = false;
						break;
					}
					array[i] = base.Name + "!" + formula.Substring(formula.LastIndexOf('!') + 1);
					flag = true;
				}
			}
			if (flag)
			{
				IRange intersectionRange = GetIntersectionRange(base.Workbook.Names, array);
				intersect = text2 + intersectionRange.Address.Substring(intersectionRange.Address.LastIndexOf('!') + 1);
			}
		}
		else
		{
			intersect = null;
		}
		if (flag)
		{
			return intersect != null;
		}
		return false;
	}

	internal void Calculate()
	{
		bool enabledCalcEngine = m_book.EnabledCalcEngine;
		if (!enabledCalcEngine)
		{
			EnableSheetCalculations();
		}
		IMigrantRange migrantRange = new MigrantRangeImpl(base.Application, this);
		for (int i = FirstRow; i <= LastRow; i++)
		{
			for (int j = FirstColumn; j <= LastColumn; j++)
			{
				migrantRange.ResetRowColumn(i, j);
				if ((migrantRange as RangeImpl).CellType == RangeImpl.TCellType.Formula)
				{
					_ = (migrantRange as RangeImpl).CalculatedValue;
				}
			}
		}
		if (!enabledCalcEngine)
		{
			DisableSheetCalculations();
		}
	}
}
