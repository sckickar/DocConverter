using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.OfficeChart.Calculate;
using DocGen.OfficeChart.FormatParser;
using DocGen.OfficeChart.FormatParser.FormatTokens;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation;

internal class RangeImpl : IRange, IParentApplication, IEnumerable, IEnumerable<IRange>, IReparse, ICombinedRange, ICellPositionFormat, INativePTG, IDisposable
{
	public enum TCellType
	{
		Number = 515,
		RK = 638,
		LabelSST = 253,
		Blank = 513,
		Formula = 6,
		BoolErr = 517,
		RString = 214,
		Label = 516
	}

	private delegate IOutline OutlineGetter(int iOutlineIndex);

	public delegate void CellValueChangedEventHandler(object sender, CellValueChangedEventArgs e);

	public const string DEF_DATE_FORMAT = "mm/dd/yyyy";

	public const string DEF_TIME_FORMAT = "h:mm:ss";

	public const string DEF_UK_DATETIME_FORMAT = "dd/MM/yyyy HH:mm";

	public const int DEF_DATETIME_INDEX = 22;

	private const double DEF_OLE_DOUBLE = 2958465.9999999884;

	private const double DEF_MAX_DOUBLE = 2958466.0;

	public const string DEF_NUMBER_FORMAT = "0.00";

	public const string DEF_TEXT_FORMAT = "@";

	public const string DEF_GENERAL_FORMAT = "General";

	internal const string DEF_FORMULAARRAY_FORMAT = "{{{0}}}";

	private const string DEF_SINGLECELL_ERROR = "This method should be called for single cells only.";

	public const string DEF_DEFAULT_STYLE = "Normal";

	internal const int DEF_NORMAL_STYLE_INDEX = 15;

	private const bool DEF_WRAPTEXT_VALUE = false;

	private const string DEF_EMPTY_DIGIT = " ";

	private static readonly TCellType[] DEF_DATETIMECELLTYPES = new TCellType[3]
	{
		TCellType.RK,
		TCellType.Number,
		TCellType.Formula
	};

	private static readonly ExcelAutoFormat[] DEF_AUTOFORMAT_RIGHT = new ExcelAutoFormat[7]
	{
		ExcelAutoFormat.Classic_2,
		ExcelAutoFormat.Classic_3,
		ExcelAutoFormat.Accounting_1,
		ExcelAutoFormat.Accounting_2,
		ExcelAutoFormat.Accounting_3,
		ExcelAutoFormat.Colorful_2,
		ExcelAutoFormat.Colorful_3
	};

	private static readonly ExcelAutoFormat[] DEF_AUTOFORMAT_NUMBER = new ExcelAutoFormat[4]
	{
		ExcelAutoFormat.Accounting_1,
		ExcelAutoFormat.Accounting_2,
		ExcelAutoFormat.Accounting_3,
		ExcelAutoFormat.Accounting_4
	};

	private const char DEF_CELL_NAME_SEPARATER = '$';

	private const char DEF_R1C1_COLUMN = 'C';

	private const char DEF_R1C1_ROW = 'R';

	private const char DEF_R1C1_OPENBRACKET = '[';

	private const char DEF_R1C1_CLOSEBRACKET = ']';

	private const string DEF_R1C1_FORMAT = "R{0}C{1}";

	private const long DEF_MIN_OADATE = 31241376000000000L;

	internal static readonly DateTime DEF_MIN_DATETIME = new DateTime(1900, 1, 1, 0, 0, 0, 0);

	private static readonly long MinAllowedDateTicks = new DateTime(1900, 1, 1, 0, 0, 0, 0).Ticks;

	private const int DEF_AUTOFORMAT_NUMBER_INDEX = 0;

	private const int DEF_AUTOFORMAT_NUMBER_INDEX_1 = 43;

	private const int DEF_AUTOFORMAT_NUMBER_INDEX_2 = 44;

	private const int ColumnBitsInCellIndex = 32;

	private const int ArrayFormulaXFFlag = -1;

	private static readonly OfficeLineStyle[] ThinBorders = new OfficeLineStyle[3]
	{
		OfficeLineStyle.None,
		OfficeLineStyle.Hair,
		OfficeLineStyle.Thin
	};

	private const int FormulaLengthXls = 255;

	private const int FormulaLengthXlsX = 8192;

	internal const char SingleQuote = '\'';

	private const string NEW_LINE = "\n";

	private const string DEF_PERCENTAGE_FORMAT = "0%";

	private const string DEF_DECIMAL_PERCENTAGE_FORMAT = "0.00%";

	private const string DEF_EXPONENTIAL_FORMAT = "0.00E+00";

	private const string DEF_CULTUREINFO_TIMETOKEN = "tt";

	private const string DEF_TIMETOKEN_FORMAT = "AM/PM";

	private string[] DEF_DATETIME_FORMULA = new string[4] { "TIME", "DATE", "TODAY", "NOW" };

	private WorksheetImpl m_worksheet;

	private WorkbookImpl m_book;

	protected int m_iLeftColumn;

	protected int m_iRightColumn;

	protected int m_iTopRow;

	protected int m_iBottomRow;

	private bool m_bIsNumReference;

	private bool m_bIsMultiReference;

	private bool m_bIsStringReference;

	private List<IRange> m_cells;

	protected CellStyle m_style;

	private bool m_bCells;

	[ThreadStatic]
	private static string m_dateSeparator = null;

	[ThreadStatic]
	private static string m_timeSeparator = null;

	protected IRTFWrapper m_rtfString;

	private char[] unnecessaryChar = new char[3] { '_', '?', '*' };

	private string[] osCultureSpecficFormats = new string[2] { "m/d/yyyy", "m/d/yy h:mm" };

	private string[] floatNumberStyleCultures = new string[5] { "de-AT", "de-DE", "de-CH", "de-LI", "de-LU" };

	internal const string UKCultureName = "cy-GB";

	private bool m_isEntireRow;

	private bool m_isEntireColumn;

	private bool m_hasDefaultFormat = true;

	internal bool updateCellValue = true;

	private Dictionary<int, List<Point>> m_outlineLevels;

	private int m_noOfSubtotals;

	internal string CalculatedValue
	{
		get
		{
			if (Formula != null)
			{
				if (Parent is WorksheetImpl && ((WorksheetImpl)Parent).CalcEngine != null)
				{
					string cellRef = RangeInfo.GetAlphaLabel(Column) + Row;
					return ((WorksheetImpl)Parent).CalcEngine.PullUpdatedValue(cellRef);
				}
				return null;
			}
			return Value;
		}
	}

	internal string[] DefaultStyleNames => m_book.AppImplementation.DefaultStyleNames;

	public string Address
	{
		get
		{
			CheckDisposed();
			return m_worksheet.QuotedName + "!" + AddressLocal;
		}
	}

	public string AddressLocal
	{
		get
		{
			CheckDisposed();
			return GetAddressLocal(FirstRow, FirstColumn, LastRow, LastColumn);
		}
	}

	public string AddressR1C1
	{
		get
		{
			CheckDisposed();
			return m_worksheet.QuotedName + "!" + AddressR1C1Local;
		}
	}

	public string AddressR1C1Local
	{
		get
		{
			CheckDisposed();
			string text = $"R{Row}C{Column}";
			if (!IsSingleCell)
			{
				text = text + ":" + $"R{LastRow}C{LastColumn}";
			}
			return text;
		}
	}

	public bool Boolean
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (!m_worksheet.GetBoolean(i, j))
					{
						return false;
					}
				}
			}
			return true;
		}
		set
		{
			CheckDisposed();
			TryRemoveFormulaArrays();
			if (IsSingleCell)
			{
				if (Boolean != value)
				{
					OnCellValueChanged(Boolean, value, this);
				}
				SetBoolean(value);
				SetChanged();
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.Boolean = value;
				}
			}
		}
	}

	public IBorders Borders
	{
		get
		{
			CheckDisposed();
			return CellStyle.Borders;
		}
	}

	public IRange[] Cells
	{
		get
		{
			CheckDisposed();
			if (m_cells == null && !m_bCells)
			{
				InfillCells();
			}
			if (m_cells == null)
			{
				throw new ArgumentNullException();
			}
			return m_cells.ToArray();
		}
	}

	public int Column
	{
		get
		{
			CheckDisposed();
			return FirstColumn;
		}
	}

	public int ColumnGroupLevel
	{
		get
		{
			CheckDisposed();
			int firstColumn = FirstColumn;
			int lastColumn = LastColumn;
			int num;
			if (firstColumn == lastColumn)
			{
				num = m_worksheet.ColumnInformation[firstColumn]?.OutlineLevel ?? 0;
			}
			else
			{
				int firstRow = FirstRow;
				num = this[firstRow, firstColumn].ColumnGroupLevel;
				for (int i = firstColumn + 1; i <= lastColumn; i++)
				{
					if (num != this[firstRow, i].ColumnGroupLevel)
					{
						return -1;
					}
				}
			}
			return num;
		}
	}

	public double ColumnWidth
	{
		get
		{
			CheckDisposed();
			double num = double.MinValue;
			if (m_iLeftColumn == m_iRightColumn)
			{
				num = m_worksheet.InnerGetColumnWidth(m_iLeftColumn);
			}
			else
			{
				num = m_worksheet.InnerGetColumnWidth(m_iLeftColumn);
				for (int i = m_iLeftColumn + 1; i <= m_iRightColumn; i++)
				{
					if (num != m_worksheet.InnerGetColumnWidth(i))
					{
						num = double.MinValue;
						break;
					}
				}
			}
			return num;
		}
		set
		{
			CheckDisposed();
			if (value < 0.0 || value > 255.0)
			{
				throw new ArgumentOutOfRangeException("ColumnWidth", "Column Width cannot be larger then 255 or zeroless");
			}
			int i = FirstColumn;
			for (int lastColumn = LastColumn; i <= lastColumn; i++)
			{
				m_worksheet.SetColumnWidth(i, value);
			}
		}
	}

	public int Count
	{
		get
		{
			CheckDisposed();
			return (LastColumn - FirstColumn + 1) * (LastRow - FirstRow + 1);
		}
	}

	internal RowStorage RowStorage => WorksheetHelper.GetOrCreateRow(Worksheet as IInternalWorksheet, Row - 1, bCreate: false);

	public DateTime DateTime
	{
		get
		{
			CheckDisposed();
			double number = m_worksheet.GetNumber(Row, Column);
			if (number < 0.0 || InnerNumberFormat.GetFormatType(number) != OfficeFormatType.DateTime)
			{
				return Convert.ToDateTime(m_worksheet.GetText(Row, Column));
			}
			if (number == double.NaN || InnerNumberFormat.GetFormatType(number) != OfficeFormatType.DateTime)
			{
				return DateTime.MinValue;
			}
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					double number2 = m_worksheet.GetNumber(i, j);
					if (number2 == double.NaN || number != number2 || InnerNumberFormat.GetFormatType(number2) != OfficeFormatType.DateTime)
					{
						return DateTime.MinValue;
					}
				}
			}
			return UtilityMethods.ConvertNumberToDateTime(number, m_book.Date1904);
		}
		set
		{
			CheckDisposed();
			if (m_book.Date1904)
			{
				value = CalcEngineHelper.FromOADate(value.ToOADate() - 1462.0);
			}
			if (IsSingleCell)
			{
				FormatType = OfficeFormatType.DateTime;
				DateTime dateTime = DateTime;
				_ = dateTime != value;
				OnCellValueChanged(dateTime, value, this);
				SetDateTime(value);
				SetChanged();
				return;
			}
			TryRemoveFormulaArrays();
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.DateTime = value;
				}
			}
		}
	}

	public string DisplayText => GetDisplayText(Row, Column, InnerNumberFormat);

	public IRange End
	{
		get
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				return this;
			}
			return m_worksheet.InnerGetCell(LastColumn, LastRow);
		}
	}

	public bool IsEntireRow
	{
		get
		{
			return m_isEntireRow;
		}
		set
		{
			m_isEntireRow = value;
		}
	}

	public bool IsEntireColumn
	{
		get
		{
			return m_isEntireColumn;
		}
		set
		{
			m_isEntireColumn = value;
		}
	}

	public IRange EntireColumn
	{
		get
		{
			CheckDisposed();
			int row = 1;
			int maxRowCount = m_book.MaxRowCount;
			RangeImpl obj = this[row, FirstColumn, maxRowCount, LastColumn] as RangeImpl;
			obj.IsEntireColumn = true;
			return obj;
		}
	}

	public IRange EntireRow
	{
		get
		{
			CheckDisposed();
			int column = 1;
			int maxColumnCount = m_book.MaxColumnCount;
			RangeImpl obj = this[FirstRow, column, LastRow, maxColumnCount] as RangeImpl;
			obj.IsEntireRow = true;
			return obj;
		}
	}

	public string Error
	{
		get
		{
			CheckDisposed();
			string error = m_worksheet.GetError(Row, Column);
			if (error == null)
			{
				return null;
			}
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (error != m_worksheet.GetError(i, j))
					{
						return null;
					}
				}
			}
			return error;
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				SetError(value);
				SetChanged();
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.Error = value;
				}
			}
		}
	}

	public string Formula
	{
		get
		{
			CheckDisposed();
			string text = null;
			if (IsSingleCell)
			{
				text = (HasFormulaArray ? $"{{{FormulaArray}}}" : m_worksheet.GetFormula(Row, Column, bR1C1: false));
			}
			else
			{
				MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
				migrantRangeImpl.ResetRowColumn(Row, Column);
				text = migrantRangeImpl.Formula;
				if (text != null)
				{
					int i = Row;
					for (int lastRow = LastRow; i <= lastRow; i++)
					{
						int j = Column;
						for (int lastColumn = LastColumn; j <= lastColumn; j++)
						{
							migrantRangeImpl.ResetRowColumn(i, j);
							string formula = migrantRangeImpl.Formula;
							if (text != formula)
							{
								text = null;
								break;
							}
						}
					}
				}
			}
			return text;
		}
		set
		{
			if (Workbook.Version == OfficeVersion.Excel97to2003 && value.Length > 255)
			{
				throw new ArgumentException("The formula is too long.Formulas length should not be longer then 255");
			}
			if (value.Length > 8192)
			{
				throw new ArgumentException("The formula is too long.Formulas length should not be longer then 8192");
			}
			CheckDisposed();
			TryRemoveFormulaArrays();
			if (value[0] != '=')
			{
				value = "=" + value;
			}
			Value = value;
		}
	}

	public string FormulaArray
	{
		get
		{
			CheckDisposed();
			return GetFormulaArray(bR1C1: false);
		}
		set
		{
			CheckDisposed();
			SetFormulaArray(value, bR1C1: false);
		}
	}

	public string FormulaStringValue
	{
		get
		{
			CheckDisposed();
			string formulaStringValue = m_worksheet.GetFormulaStringValue(Row, Column);
			if (!IsSingleCell && formulaStringValue != null)
			{
				int i = Row;
				for (int lastRow = LastRow; i <= lastRow; i++)
				{
					int j = Column;
					for (int lastColumn = LastColumn; j <= lastColumn; j++)
					{
						if (formulaStringValue != m_worksheet.GetFormulaStringValue(i, j))
						{
							return null;
						}
					}
				}
			}
			return formulaStringValue;
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				m_worksheet.CellRecords.SetStringValue(CellIndex, value);
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.FormulaStringValue = value;
				}
			}
		}
	}

	public double FormulaNumberValue
	{
		get
		{
			CheckDisposed();
			double formulaNumberValue = m_worksheet.GetFormulaNumberValue(Row, Column);
			if (!IsSingleCell && !double.IsNaN(formulaNumberValue))
			{
				int i = Row;
				for (int lastRow = LastRow; i <= lastRow; i++)
				{
					int j = Column;
					for (int lastColumn = LastColumn; j <= lastColumn; j++)
					{
						double formulaNumberValue2 = m_worksheet.GetFormulaNumberValue(i, j);
						if (formulaNumberValue != formulaNumberValue2)
						{
							return double.NaN;
						}
					}
				}
			}
			return formulaNumberValue;
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				if (!(Record is FormulaRecord formulaRecord))
				{
					throw new NotSupportedException("This property is only for formula ranges");
				}
				formulaRecord.Value = value;
				Record = formulaRecord;
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.FormulaNumberValue = value;
				}
			}
		}
	}

	public bool FormulaBoolValue
	{
		get
		{
			CheckDisposed();
			bool formulaBoolValue = m_worksheet.GetFormulaBoolValue(Row, Column);
			if (!IsSingleCell && formulaBoolValue)
			{
				int i = Row;
				for (int lastRow = LastRow; i <= lastRow; i++)
				{
					int j = Column;
					for (int lastColumn = LastColumn; j <= lastColumn; j++)
					{
						if (formulaBoolValue != m_worksheet.GetFormulaBoolValue(i, j))
						{
							return false;
						}
					}
				}
			}
			return formulaBoolValue;
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				if (!(Record is FormulaRecord formulaRecord))
				{
					throw new NotSupportedException("This property is only for formula ranges");
				}
				formulaRecord.BooleanValue = value;
				Record = formulaRecord;
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.FormulaBoolValue = value;
				}
			}
		}
	}

	public string FormulaErrorValue
	{
		get
		{
			CheckDisposed();
			string formulaErrorValue = m_worksheet.GetFormulaErrorValue(Row, Column);
			if (!IsSingleCell && formulaErrorValue != null)
			{
				int i = Row;
				for (int lastRow = LastRow; i <= lastRow; i++)
				{
					int j = Column;
					for (int lastColumn = LastColumn; j <= lastColumn; j++)
					{
						if (formulaErrorValue != m_worksheet.GetFormulaErrorValue(i, j))
						{
							return null;
						}
					}
				}
			}
			return formulaErrorValue;
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				if (!(Record is FormulaRecord formulaRecord))
				{
					throw new NotSupportedException("This property is only for formula ranges");
				}
				int num = GetErrorCodeByString(value);
				if (num == -1)
				{
					num = 0;
				}
				formulaRecord.ErrorValue = (byte)num;
				Record = formulaRecord;
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.FormulaErrorValue = value;
				}
			}
		}
	}

	public object FormulaValue
	{
		get
		{
			if (HasFormula)
			{
				string formulaStringValue = FormulaStringValue;
				if (formulaStringValue != null)
				{
					return formulaStringValue;
				}
				if (HasFormulaDateTime)
				{
					return FormulaDateTime;
				}
				if (HasFormulaBoolValue)
				{
					return FormulaBoolValue;
				}
				if (HasFormulaErrorValue)
				{
					return FormulaErrorValue;
				}
				return FormulaNumberValue;
			}
			return null;
		}
	}

	public bool FormulaHidden
	{
		get
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				return CellStyle.FormulaHidden;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			bool formulaHidden = m_worksheet[FirstRow, FirstColumn].FormulaHidden;
			if (formulaHidden)
			{
				for (int i = FirstRow; i <= LastRow && formulaHidden; i++)
				{
					for (int j = FirstColumn; j <= LastColumn && formulaHidden; j++)
					{
						migrantRangeImpl.ResetRowColumn(i, j);
						formulaHidden = migrantRangeImpl.FormulaHidden;
					}
				}
			}
			return formulaHidden;
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				CellStyle.FormulaHidden = value;
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.FormulaHidden = value;
				}
			}
		}
	}

	public DateTime FormulaDateTime
	{
		get
		{
			CheckDisposed();
			double formulaNumberValue = m_worksheet.GetFormulaNumberValue(Row, Column);
			if (formulaNumberValue == double.NaN || InnerNumberFormat.GetFormatType(formulaNumberValue) != OfficeFormatType.DateTime)
			{
				return DateTime.MinValue;
			}
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					double formulaNumberValue2 = m_worksheet.GetFormulaNumberValue(i, j);
					if (formulaNumberValue2 == double.NaN || formulaNumberValue != formulaNumberValue2 || InnerNumberFormat.GetFormatType(formulaNumberValue2) != OfficeFormatType.DateTime)
					{
						return DateTime.MinValue;
					}
				}
			}
			return UtilityMethods.ConvertNumberToDateTime(formulaNumberValue, m_book.Date1904);
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				if (CellType != TCellType.Formula)
				{
					throw new NotSupportedException("This property is only for formula ranges");
				}
				FormatType = OfficeFormatType.DateTime;
				m_worksheet.SetFormulaNumberValue(Row, Column, value.ToOADate());
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.FormulaDateTime = value;
				}
			}
		}
	}

	public string FormulaR1C1
	{
		get
		{
			CheckDisposed();
			string text = null;
			if (IsSingleCell)
			{
				if (!HasFormulaArray)
				{
					return m_worksheet.GetFormula(Row, Column, bR1C1: true);
				}
				return $"{{{FormulaArrayR1C1}}}";
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			migrantRangeImpl.ResetRowColumn(Row, Column);
			text = migrantRangeImpl.FormulaR1C1;
			if (text != null)
			{
				int i = Row;
				for (int lastRow = LastRow; i <= lastRow; i++)
				{
					int j = Column;
					for (int lastColumn = LastColumn; j <= lastColumn; j++)
					{
						migrantRangeImpl.ResetRowColumn(i, j);
						string formulaR1C = migrantRangeImpl.FormulaR1C1;
						if (text != formulaR1C)
						{
							text = null;
							break;
						}
					}
				}
			}
			return text;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value[0] == '=')
			{
				value = value.Substring(1);
			}
			for (int i = m_iTopRow; i <= m_iBottomRow; i++)
			{
				for (int j = m_iLeftColumn; j <= m_iRightColumn; j++)
				{
					m_worksheet.SetFormula(i, j, value, bIsR1C1: true);
				}
			}
		}
	}

	public string FormulaArrayR1C1
	{
		get
		{
			CheckDisposed();
			return GetFormulaArray(bR1C1: true);
		}
		set
		{
			CheckDisposed();
			SetFormulaArray(value, bR1C1: true);
		}
	}

	public bool HasFormula
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (m_worksheet.GetCellType(i, j, bNeedFormulaSubType: false) != WorksheetImpl.TRangeValueType.Formula)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool HasFormulaArray
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (!m_worksheet.HasArrayFormulaRecord(i, j))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public OfficeHAlign HorizontalAlignment
	{
		get
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				return CellStyle.HorizontalAlignment;
			}
			return OfficeHAlign.HAlignGeneral;
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				CellStyle.HorizontalAlignment = value;
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.HorizontalAlignment = value;
				}
			}
		}
	}

	public int IndentLevel
	{
		get
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				return CellStyle.IndentLevel;
			}
			return int.MinValue;
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				CellStyle.IndentLevel = (ushort)value;
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.IndentLevel = value;
				}
			}
		}
	}

	public bool IsBoolean
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (m_worksheet.GetCellType(i, j, bNeedFormulaSubType: false) != WorksheetImpl.TRangeValueType.Boolean)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool IsError
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (m_worksheet.GetCellType(i, j, bNeedFormulaSubType: false) != WorksheetImpl.TRangeValueType.Error)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool IsGroupedByColumn
	{
		get
		{
			CheckDisposed();
			int firstColumn = FirstColumn;
			int lastColumn = LastColumn;
			if (firstColumn == lastColumn)
			{
				ColumnInfoRecord columnInfoRecord = m_worksheet.ColumnInformation[firstColumn];
				if (columnInfoRecord == null)
				{
					return false;
				}
				return columnInfoRecord.OutlineLevel != 0;
			}
			int firstRow = FirstRow;
			for (int i = firstColumn; i <= lastColumn; i++)
			{
				if (!this[firstRow, i].IsGroupedByColumn)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool IsGroupedByRow
	{
		get
		{
			CheckDisposed();
			int firstRow = FirstRow;
			int lastRow = LastRow;
			if (firstRow == lastRow)
			{
				IOutline rowOutline = WorksheetHelper.GetRowOutline(m_worksheet, firstRow);
				if (rowOutline == null)
				{
					return false;
				}
				return rowOutline.OutlineLevel != 0;
			}
			int firstColumn = FirstColumn;
			for (int i = firstRow; i <= lastRow; i++)
			{
				if (!this[i, firstColumn].IsGroupedByRow)
				{
					return false;
				}
			}
			return true;
		}
	}

	public int LastColumn
	{
		[DebuggerStepThrough]
		get
		{
			return m_iRightColumn;
		}
		set
		{
			if (value < 1 || value > m_book.MaxColumnCount)
			{
				throw new ArgumentOutOfRangeException("FirstRow");
			}
			if (value != LastColumn)
			{
				m_iRightColumn = value;
				OnLastColumnChanged();
			}
		}
	}

	public int LastRow
	{
		[DebuggerStepThrough]
		get
		{
			return m_iBottomRow;
		}
		set
		{
			if (value < 1 || value > m_book.MaxRowCount)
			{
				throw new ArgumentOutOfRangeException("FirstRow");
			}
			if (value != LastRow)
			{
				m_iBottomRow = value;
				OnLastRowChanged();
			}
		}
	}

	public double Number
	{
		get
		{
			CheckDisposed();
			double number = m_worksheet.GetNumber(Row, Column);
			if (number == double.NaN)
			{
				return number;
			}
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (number != m_worksheet.GetNumber(i, j) && double.IsNaN(number))
					{
						return double.NaN;
					}
				}
			}
			return number;
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				if (BitConverter.DoubleToInt64Bits(value) == BitConverter.DoubleToInt64Bits(-0.0))
				{
					value = 0.0;
				}
				if (double.IsNaN(value) || double.IsInfinity(value))
				{
					double number = Number;
					if (number != value)
					{
						OnCellValueChanged(number, value, this);
					}
					if (m_rtfString == null)
					{
						CreateRichTextString();
					}
					m_rtfString.BeginUpdate();
					m_rtfString.Text = "#N/A";
					if (NumberFormat != "General")
					{
						NumberFormat = "@";
					}
					m_rtfString.ClearFormatting();
					m_rtfString.EndUpdate();
					SetChanged();
				}
				else
				{
					double number2 = Number;
					if (number2 != value)
					{
						OnCellValueChanged(number2, value, this);
					}
					SetNumberAndFormat(value, isPreserveFormat: false);
					SetChanged();
				}
				return;
			}
			TryRemoveFormulaArrays();
			int i = FirstRow;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = FirstColumn;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					this[i, j].Number = value;
				}
			}
		}
	}

	public string NumberFormat
	{
		get
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				return GetNumberFormat();
			}
			return GetNumberFormat(CellsList);
		}
		set
		{
			CheckDisposed();
			value = AmPmToken.CheckAndApplyAMPM(value);
			if (IsSingleCell)
			{
				CellStyle.NumberFormat = value;
				SetChanged();
			}
			else
			{
				MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
				for (int i = FirstRow; i <= LastRow; i++)
				{
					for (int j = FirstColumn; j <= LastColumn; j++)
					{
						migrantRangeImpl.ResetRowColumn(i, j);
						migrantRangeImpl.NumberFormat = value;
					}
				}
			}
			m_hasDefaultFormat = false;
		}
	}

	public int Row
	{
		get
		{
			CheckDisposed();
			return FirstRow;
		}
	}

	public int RowGroupLevel
	{
		get
		{
			CheckDisposed();
			int firstRow = FirstRow;
			int lastRow = LastRow;
			if (firstRow == lastRow)
			{
				return ((IOutline)WorksheetHelper.GetOrCreateRow(m_worksheet, firstRow - 1, bCreate: false))?.OutlineLevel ?? 0;
			}
			int firstColumn = FirstColumn;
			int rowGroupLevel = this[firstRow, firstColumn].RowGroupLevel;
			for (int i = firstRow + 1; i <= lastRow; i++)
			{
				if (rowGroupLevel != this[i, firstColumn].RowGroupLevel)
				{
					return -1;
				}
			}
			return rowGroupLevel;
		}
	}

	public double RowHeight
	{
		get
		{
			CheckDisposed();
			double num = double.MinValue;
			if (m_iTopRow == m_iBottomRow)
			{
				num = m_worksheet.GetRowHeight(Row);
			}
			else
			{
				num = m_worksheet.GetRowHeight(m_iTopRow);
				for (int i = m_iTopRow + 1; i <= m_iBottomRow; i++)
				{
					if (num != m_worksheet.GetRowHeight(i))
					{
						num = double.MinValue;
						break;
					}
				}
			}
			return num;
		}
		set
		{
			CheckDisposed();
			SetRowHeight(value, bIsBadFontHeight: true);
		}
	}

	public IRange[] Rows
	{
		get
		{
			CheckDisposed();
			int num = ((FirstColumn != 0 && LastColumn != 0 && LastRow != 0) ? (LastRow - FirstRow + 1) : 0);
			IRange[] array = new IRange[num];
			if (num > 0)
			{
				for (int i = FirstRow; i <= LastRow; i++)
				{
					array[i - FirstRow] = m_worksheet.Range[i, FirstColumn, i, LastColumn];
				}
			}
			return array;
		}
	}

	public IRange[] Columns
	{
		get
		{
			CheckDisposed();
			if (FirstColumn == 0 || FirstColumn > m_book.MaxColumnCount)
			{
				return new IRange[0];
			}
			IRange[] array = new IRange[LastColumn - FirstColumn + 1];
			for (int i = FirstColumn; i <= LastColumn; i++)
			{
				array[i - FirstColumn] = m_worksheet.Range[FirstRow, i, LastRow, i];
			}
			return array;
		}
	}

	public IStyle CellStyle
	{
		get
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				if (m_style == null)
				{
					CreateStyle();
				}
				if (HasRichText)
				{
					(m_style.Font as FontWrapper).Range = this;
				}
				return m_style;
			}
			if (IsEntireRow || IsEntireColumn)
			{
				return CreateStyleForEntireRowEntireColumn();
			}
			return new StyleArrayWrapper(this);
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				SetChanged();
				TCellType cellType = CellType;
				ushort extendedFormatIndex;
				if (value is ExtendedFormatWrapper)
				{
					extendedFormatIndex = (ushort)(value as ExtendedFormatWrapper).Wrapped.Index;
				}
				else
				{
					string name = ((value == null) ? DefaultStyleNames[0] : value.Name);
					extendedFormatIndex = (ushort)((StyleImpl)m_book.Styles[name]).Wrapped.Index;
				}
				ExtendedFormatIndex = extendedFormatIndex;
				BiffRecordRaw record = Record;
				if ((record != null && record.TypeCode == TBIFFRecord.Formula) || record == null)
				{
					_ = Value;
				}
				OnStyleChanged(cellType);
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.CellStyle = value;
				}
			}
		}
	}

	public string CellStyleName
	{
		get
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				return GetStyleName();
			}
			return GetCellStyleName(CellsList);
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				TCellType cellType = CellType;
				if (value == null)
				{
					_ = DefaultStyleNames[0];
				}
				ChangeStyleName(value);
				_ = Value;
				OnStyleChanged(cellType);
				SetChanged();
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.CellStyleName = value;
				}
			}
		}
	}

	public BuiltInStyles? BuiltInStyle
	{
		get
		{
			string cellStyleName = CellStyleName;
			return (BuiltInStyles)Array.IndexOf(DefaultStyleNames, cellStyleName);
		}
		set
		{
			string cellStyleName = DefaultStyleNames[(int)value.Value];
			CellStyleName = cellStyleName;
		}
	}

	public string Text
	{
		get
		{
			CheckDisposed();
			string text = m_worksheet.GetText(Row, Column);
			if (text == null)
			{
				return null;
			}
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (text != m_worksheet.GetText(i, j))
					{
						return null;
					}
				}
			}
			if (ExtendedFormat.IsFirstSymbolApostrophe)
			{
				text = "'" + text;
			}
			return text;
		}
		set
		{
			CheckDisposed();
			if (value == null)
			{
				throw new ArgumentNullException("Text");
			}
			TryRemoveFormulaArrays();
			if (IsSingleCell)
			{
				if (value.Length == 0)
				{
					Value = value;
				}
				else
				{
					if (Text != value)
					{
						OnCellValueChanged(Text, value, this);
					}
					value = CheckApostrophe(value);
					if (m_rtfString == null)
					{
						CreateRichTextString();
					}
					m_rtfString.BeginUpdate();
					m_rtfString.Text = value;
					if (NumberFormat != "General" && FormatType != 0)
					{
						NumberFormat = "@";
					}
					m_rtfString.EndUpdate();
					SetChanged();
				}
			}
			else
			{
				MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
				for (int i = FirstRow; i <= LastRow; i++)
				{
					for (int j = FirstColumn; j <= LastColumn; j++)
					{
						migrantRangeImpl.ResetRowColumn(i, j);
						migrantRangeImpl.Text = value;
					}
				}
			}
			if (value.Contains(Environment.NewLine) || value.Contains("\n"))
			{
				WrapText = true;
			}
		}
	}

	public TimeSpan TimeSpan
	{
		get
		{
			CheckDisposed();
			double number = m_worksheet.GetNumber(Row, Column);
			if (number == double.NaN || InnerNumberFormat.GetFormatType(number) != OfficeFormatType.DateTime)
			{
				return TimeSpan.MinValue;
			}
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					double number2 = m_worksheet.GetNumber(i, j);
					if (number2 == double.NaN || number != number2 || InnerNumberFormat.GetFormatType(number2) != OfficeFormatType.DateTime)
					{
						return TimeSpan.MinValue;
					}
				}
			}
			if (number < 2958466.0)
			{
				return TimeSpan.FromDays(number);
			}
			return TimeSpan.FromDays(2958465.9999999884);
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				FormatType = OfficeFormatType.DateTime;
				TimeSpan timeSpan = TimeSpan;
				if (timeSpan != value)
				{
					OnCellValueChanged(timeSpan, value, this);
				}
				SetTimeSpan(value);
				SetChanged();
				return;
			}
			TryRemoveFormulaArrays();
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.TimeSpan = value;
				}
			}
		}
	}

	public string Value
	{
		get
		{
			CheckDisposed();
			string text = null;
			if (IsSingleCell)
			{
				text = m_worksheet.GetValue(Record as ICellPositionFormat, preserveOLEDate: false);
			}
			else
			{
				MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
				migrantRangeImpl.ResetRowColumn(Row, Column);
				text = migrantRangeImpl.Value;
				int i = Row;
				for (int lastRow = LastRow; i <= lastRow; i++)
				{
					int j = Column;
					for (int lastColumn = LastColumn; j <= lastColumn; j++)
					{
						migrantRangeImpl.ResetRowColumn(i, j);
						string value = migrantRangeImpl.Value;
						if (text != value)
						{
							text = null;
							break;
						}
					}
				}
			}
			if (Workbook.Date1904 && HasDateTime)
			{
				double doubleOLEValue = 0.0;
				if (DateTime.TryParse(text, out var result))
				{
					doubleOLEValue = result.ToOADate();
				}
				text = CalcEngineHelper.FromOADate(doubleOLEValue).Date.ToString();
			}
			return text;
		}
		set
		{
			CheckDisposed();
			TryRemoveFormulaArrays();
			if (IsSingleCell)
			{
				string value2 = Value;
				if (value != value2)
				{
					OnValueChanged(value2, value);
				}
			}
			else
			{
				MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
				for (int i = FirstRow; i <= LastRow; i++)
				{
					for (int j = FirstColumn; j <= LastColumn; j++)
					{
						migrantRangeImpl.ResetRowColumn(i, j);
						migrantRangeImpl.Value = value;
					}
				}
			}
			if (value != null && (value.Contains(Environment.NewLine) || value.Contains("\n")))
			{
				WrapText = true;
			}
		}
	}

	public object Value2
	{
		get
		{
			object obj = null;
			CheckDisposed();
			obj = TryCreateValue2();
			if (obj == null)
			{
				obj = Value;
			}
			return obj;
		}
		set
		{
			CheckDisposed();
			TryRemoveFormulaArrays();
			if (IsSingleCell)
			{
				SetSingleCellValue2(value);
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.Value2 = value;
				}
			}
		}
	}

	internal bool IsNumReference
	{
		get
		{
			return m_bIsNumReference;
		}
		set
		{
			m_bIsNumReference = value;
		}
	}

	internal bool IsStringReference
	{
		get
		{
			return m_bIsStringReference;
		}
		set
		{
			m_bIsStringReference = value;
		}
	}

	internal bool IsMultiReference
	{
		get
		{
			return m_bIsMultiReference;
		}
		set
		{
			m_bIsMultiReference = value;
		}
	}

	public OfficeVAlign VerticalAlignment
	{
		get
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				return CellStyle.VerticalAlignment;
			}
			return OfficeVAlign.VAlignBottom;
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				CellStyle.VerticalAlignment = value;
				return;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					migrantRangeImpl.VerticalAlignment = value;
				}
			}
		}
	}

	public IWorksheet Worksheet
	{
		get
		{
			CheckDisposed();
			return m_worksheet;
		}
	}

	public IRange this[int row, int column]
	{
		get
		{
			CheckDisposed();
			CheckRange(row, column);
			return m_worksheet.InnerGetCell(column, row);
		}
		set
		{
			CheckDisposed();
			CheckRange(row, column);
			m_worksheet.InnerSetCell(column, row, (RangeImpl)value);
			SetChanged();
		}
	}

	public IRange this[int row, int column, int lastRow, int lastColumn]
	{
		get
		{
			CheckDisposed();
			row = NormalizeRowIndex(row, column, lastColumn);
			lastRow = NormalizeRowIndex(lastRow, column, lastColumn);
			column = NormalizeColumnIndex(column, row, lastRow);
			lastColumn = NormalizeColumnIndex(lastColumn, row, lastRow);
			CheckRange(row, column);
			CheckRange(lastRow, lastColumn);
			if (row != lastRow || column != lastColumn)
			{
				return AppImplementation.CreateRange(Parent, column, row, lastColumn, lastRow);
			}
			return this[row, column];
		}
	}

	public IRange this[string name] => this[name, false];

	public IRange this[string name, bool IsR1C1Notation]
	{
		get
		{
			CheckDisposed();
			string worksheetName = GetWorksheetName(ref name);
			if (worksheetName != null && m_worksheet.Name != worksheetName)
			{
				return FindWorksheet(worksheetName).Range[name];
			}
			IName name2 = m_worksheet.Names[name];
			if (name2 != null)
			{
				return name2.RefersToRange;
			}
			name2 = m_book.Names[name];
			if (name2 != null)
			{
				return name2.RefersToRange;
			}
			name = name.ToUpper();
			if (IsR1C1Notation)
			{
				return ParseR1C1Reference(name);
			}
			int iFirstRow;
			int iFirstColumn;
			int iLastRow;
			int iLastColumn;
			return ParseRangeString(name, Workbook, out iFirstRow, out iFirstColumn, out iLastRow, out iLastColumn) switch
			{
				1 => this[iFirstRow, iFirstColumn], 
				2 => this[iFirstRow, iFirstColumn, iLastRow, iLastColumn], 
				_ => throw new ArgumentException(), 
			};
		}
	}

	public bool HasFormulaBoolValue
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					WorksheetImpl.TRangeValueType cellType = m_worksheet.GetCellType(i, j, bNeedFormulaSubType: true);
					if ((cellType & WorksheetImpl.TRangeValueType.Formula) != WorksheetImpl.TRangeValueType.Formula || (cellType & WorksheetImpl.TRangeValueType.Boolean) != WorksheetImpl.TRangeValueType.Boolean)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool HasFormulaErrorValue
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					WorksheetImpl.TRangeValueType cellType = m_worksheet.GetCellType(i, j, bNeedFormulaSubType: true);
					if ((cellType & WorksheetImpl.TRangeValueType.Formula) != WorksheetImpl.TRangeValueType.Formula || (cellType & WorksheetImpl.TRangeValueType.Error) != WorksheetImpl.TRangeValueType.Error)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool HasFormulaDateTime
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					double formulaNumberValue = m_worksheet.GetFormulaNumberValue(i, j);
					if (double.IsNaN(formulaNumberValue) || InnerNumberFormat.GetFormatType(formulaNumberValue) != OfficeFormatType.DateTime)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool HasFormulaNumberValue
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					double formulaNumberValue = m_worksheet.GetFormulaNumberValue(i, j);
					if (double.IsNaN(formulaNumberValue) || InnerNumberFormat.GetFormatType(formulaNumberValue) == OfficeFormatType.DateTime)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool HasFormulaStringValue
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (m_worksheet[i, j].FormulaStringValue == null)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool IsBlank
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (m_worksheet.GetCellType(i, j, bNeedFormulaSubType: false) != 0)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool IsBlankorHasStyle
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (m_worksheet.GetCellType(i, j, bNeedFormulaSubType: false) != 0 || (Worksheet[i, j].HasStyle && Worksheet[i, j].CellStyle.FillPattern != 0) || Worksheet[i, j].CellStyle.HasBorder)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool HasBoolean
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (m_worksheet.GetCellType(i, j, bNeedFormulaSubType: false) != WorksheetImpl.TRangeValueType.Boolean)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool HasDateTime
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					double number = m_worksheet.GetNumber(i, j);
					if (double.IsNaN(number) || InnerNumberFormat.GetFormatType(number) != OfficeFormatType.DateTime)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool HasNumber
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					double number = m_worksheet.GetNumber(i, j);
					if (!double.IsNaN(number))
					{
						OfficeFormatType formatType = InnerNumberFormat.GetFormatType(number);
						if (formatType == OfficeFormatType.Unknown && number == 0.0)
						{
							formatType = InnerNumberFormat.GetFormatType(1.0);
						}
						if (formatType != OfficeFormatType.DateTime && formatType != OfficeFormatType.Text)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	public bool HasString
	{
		get
		{
			CheckDisposed();
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (m_worksheet.GetCellType(i, j, bNeedFormulaSubType: false) != WorksheetImpl.TRangeValueType.String)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public IRichTextString RichText
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

	public bool HasRichText
	{
		get
		{
			CheckDisposed();
			if (IsSingleCell && HasString)
			{
				return RichText.IsFormatted;
			}
			return false;
		}
	}

	public bool IsMerged
	{
		get
		{
			CheckDisposed();
			Rectangle rect = new Rectangle(FirstColumn - 1, FirstRow - 1, 0, 0);
			if (IsSingleCell)
			{
				return m_worksheet.MergeCells[rect] != null;
			}
			Rectangle rect2 = new Rectangle(LastColumn - 1, LastRow - 1, 0, 0);
			MergeCellsRecord.MergedRegion mergedRegion = m_worksheet.MergeCells[rect];
			MergeCellsRecord.MergedRegion mergedRegion2 = m_worksheet.MergeCells[rect2];
			if (mergedRegion != null || mergedRegion2 != null)
			{
				return true;
			}
			return false;
		}
	}

	public IRange MergeArea
	{
		get
		{
			CheckDisposed();
			MergeCellsRecord.MergedRegion parentMergeRegion = ParentMergeRegion;
			if (parentMergeRegion == null)
			{
				return null;
			}
			return m_worksheet[parentMergeRegion.RowFrom + 1, parentMergeRegion.ColumnFrom + 1, parentMergeRegion.RowTo + 1, parentMergeRegion.ColumnTo + 1];
		}
	}

	public bool IsInitialized
	{
		get
		{
			CheckDisposed();
			if (IsBlank)
			{
				return HasStyle;
			}
			return true;
		}
	}

	public bool HasStyle
	{
		get
		{
			CheckDisposed();
			if (IsSingleCell || IsEntireRow || IsEntireColumn)
			{
				bool num = m_style != null && m_style.IsInitialized;
				int extendedFormatIndex = ExtendedFormatIndex;
				bool flag = extendedFormatIndex != 0 && extendedFormatIndex != m_book.DefaultXFIndex;
				return num || flag;
			}
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			migrantRangeImpl.ResetRowColumn(Row, Column);
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					if (m_worksheet.Range[i, j].HasStyle)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool WrapText
	{
		get
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				return GetWrapText();
			}
			return GetWrapText(CellsList);
		}
		set
		{
			CheckDisposed();
			if (IsSingleCell)
			{
				CellStyle.WrapText = value;
				RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(Worksheet as IInternalWorksheet, Row - 1, bCreate: false);
				if (orCreateRow != null && !Workbook.IsWorkbookOpening)
				{
					orCreateRow.IsWrapText = value;
				}
			}
			else
			{
				MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
				for (int i = FirstRow; i <= LastRow; i++)
				{
					for (int j = FirstColumn; j <= LastColumn; j++)
					{
						migrantRangeImpl.ResetRowColumn(i, j);
						migrantRangeImpl.CellStyle.WrapText = value;
					}
					RowStorage orCreateRow2 = WorksheetHelper.GetOrCreateRow(Worksheet as IInternalWorksheet, i - 1, bCreate: false);
					if (orCreateRow2 != null && !orCreateRow2.IsBadFontHeight && !Workbook.IsWorkbookOpening)
					{
						orCreateRow2.IsWrapText = value;
					}
				}
			}
			SetChanged();
		}
	}

	public bool HasExternalFormula
	{
		get
		{
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (!m_worksheet.IsExternalFormula(i, j))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool? IsStringsPreserved
	{
		get
		{
			return m_worksheet.GetStringPreservedValue(this);
		}
		set
		{
			m_worksheet.SetStringPreservedValue(this, value);
		}
	}

	public IApplication Application => m_worksheet.Application;

	public object Parent => m_worksheet;

	private ApplicationImpl AppImplementation => m_worksheet.AppImplementation;

	public string AddressGlobal => m_worksheet.QuotedName + "!" + AddressGlobalWithoutSheetName;

	public string AddressGlobalWithoutSheetName
	{
		get
		{
			string empty = string.Empty;
			string cellNameWithDollars = GetCellNameWithDollars(FirstColumn, FirstRow);
			if (IsSingleCell)
			{
				return empty + cellNameWithDollars;
			}
			string cellNameWithDollars2 = GetCellNameWithDollars(LastColumn, LastRow);
			return empty + cellNameWithDollars + ":" + cellNameWithDollars2;
		}
	}

	internal List<IRange> CellsList
	{
		get
		{
			CheckDisposed();
			if (m_cells == null && !m_bCells)
			{
				InfillCells();
			}
			if (m_cells == null)
			{
				throw new ArgumentNullException();
			}
			return m_cells;
		}
	}

	protected internal bool IsSingleCell
	{
		get
		{
			if (m_iLeftColumn == m_iRightColumn)
			{
				return m_iTopRow == m_iBottomRow;
			}
			return false;
		}
	}

	protected internal int FirstRow
	{
		[DebuggerStepThrough]
		get
		{
			return m_iTopRow;
		}
		set
		{
			if (value == 0 || value > m_book.MaxRowCount)
			{
				throw new ArgumentOutOfRangeException("FirstRow");
			}
			if (value != FirstRow)
			{
				m_iTopRow = value;
				OnFirstRowChanged();
			}
		}
	}

	protected internal int FirstColumn
	{
		[DebuggerStepThrough]
		get
		{
			return m_iLeftColumn;
		}
		set
		{
			if (value < 1 || value > m_book.MaxColumnCount)
			{
				throw new ArgumentOutOfRangeException("FirstRow", "Value was out of range.");
			}
			if (value != FirstColumn)
			{
				m_iLeftColumn = value;
				OnFirstColumnChanged();
			}
		}
	}

	protected internal string CellName
	{
		get
		{
			if (IsSingleCell)
			{
				return GetCellName(FirstColumn, FirstRow);
			}
			return null;
		}
	}

	protected internal long CellIndex
	{
		get
		{
			if (IsSingleCell)
			{
				return GetCellIndex(FirstColumn, FirstRow);
			}
			return -1L;
		}
	}

	protected internal TCellType CellType
	{
		get
		{
			if (Record != null)
			{
				return (TCellType)Record.TypeCode;
			}
			return TCellType.Blank;
		}
	}

	[CLSCompliant(false)]
	protected internal ushort StyleXFIndex
	{
		get
		{
			if (IsSingleCell)
			{
				if (m_style != null)
				{
					return (ushort)m_style.Wrapped.Index;
				}
				if (Record != null)
				{
					return ((ICellPositionFormat)Record).ExtendedFormatIndex;
				}
			}
			return (ushort)m_book.DefaultXFIndex;
		}
	}

	[CLSCompliant(false)]
	public ushort ExtendedFormatIndex
	{
		get
		{
			if (IsEntireRow)
			{
				return (ushort)m_worksheet.GetXFIndex(m_iTopRow);
			}
			if (IsEntireColumn)
			{
				return (ushort)m_worksheet.GetColumnXFIndex(m_iLeftColumn);
			}
			return (ushort)m_worksheet.GetXFIndex(m_iTopRow, m_iLeftColumn);
		}
		set
		{
			if (!IsSingleCell && !IsEntireRow && !IsEntireColumn)
			{
				throw new ArgumentException("This property can be used only for single cell not a range");
			}
			SetXFormatIndex(value);
		}
	}

	[CLSCompliant(false)]
	protected internal MulRKRecord.RkRec RKSubRecord
	{
		get
		{
			if (CellType != TCellType.RK)
			{
				throw new ArgumentException("This property can be accessed only when range represent RKRecord");
			}
			return new MulRKRecord.RkRec(StyleXFIndex, RKRecord.ConvertToRKNumber(((RKRecord)Record).RKNumber));
		}
	}

	protected internal WorkbookImpl Workbook => m_book;

	private MergeCellsRecord.MergedRegion ParentMergeRegion
	{
		get
		{
			Rectangle rect = new Rectangle(FirstColumn - 1, FirstRow - 1, 0, 0);
			if (IsSingleCell)
			{
				return m_worksheet.MergeCells[rect];
			}
			Rectangle rect2 = new Rectangle(LastColumn - 1, LastRow - 1, 0, 0);
			MergeCellsRecord.MergedRegion mergedRegion = m_worksheet.MergeCells[rect];
			MergeCellsRecord.MergedRegion region = m_worksheet.MergeCells[rect2];
			if (!MergeCellsRecord.MergedRegion.Equals(mergedRegion, region))
			{
				return null;
			}
			return mergedRegion;
		}
	}

	protected internal WorksheetImpl InnerWorksheet => m_worksheet;

	[CLSCompliant(false)]
	protected internal BiffRecordRaw Record
	{
		get
		{
			return (BiffRecordRaw)m_worksheet.GetRecord(FirstRow, FirstColumn);
		}
		set
		{
			m_worksheet.CellRecords.AddRecord(value, bIgnoreStyles: false);
		}
	}

	public Dictionary<ArrayRecord, object> FormulaArrays
	{
		get
		{
			Dictionary<ArrayRecord, object> dictionary = null;
			if (IsSingleCell)
			{
				ArrayRecord arrayRecord = m_worksheet.CellRecords.GetArrayRecord(m_iTopRow, m_iLeftColumn);
				if (arrayRecord != null)
				{
					dictionary = new Dictionary<ArrayRecord, object>();
					dictionary[arrayRecord] = null;
				}
			}
			else
			{
				dictionary = new Dictionary<ArrayRecord, object>();
				Dictionary<long, object> dictionary2 = new Dictionary<long, object>();
				CellRecordCollection cellRecords = m_worksheet.CellRecords;
				int i = FirstRow;
				for (int lastRow = LastRow; i <= lastRow; i++)
				{
					int j = FirstColumn;
					for (int lastColumn = LastColumn; j <= lastColumn; j++)
					{
						ArrayRecord arrayRecord2 = cellRecords.GetArrayRecord(i, j);
						if (arrayRecord2 != null)
						{
							long cellIndex = GetCellIndex(arrayRecord2.FirstColumn, arrayRecord2.FirstRow);
							if (!dictionary2.ContainsKey(cellIndex))
							{
								dictionary[arrayRecord2] = null;
								dictionary2.Add(cellIndex, null);
							}
						}
					}
				}
				dictionary2.Clear();
			}
			return dictionary;
		}
	}

	public bool AreFormulaArraysNotSeparated => GetAreArrayFormulasNotSeparated(null);

	public int CellsCount => (LastRow - FirstRow + 1) * (LastColumn - FirstColumn + 1);

	public FormatImpl InnerNumberFormat
	{
		get
		{
			int num = m_book.GetExtFormat(ExtendedFormatIndex).NumberFormatIndex;
			if (m_book.InnerFormats.Count > 14 && !m_book.InnerFormats.Contains(num))
			{
				num = 14;
			}
			return m_book.InnerFormats[num];
		}
	}

	public string AddressGlobal2007 => AddressGlobal;

	internal static string DateSeperator
	{
		get
		{
			if (m_dateSeparator == null)
			{
				m_dateSeparator = GetDateSeperator();
			}
			return m_dateSeparator;
		}
	}

	internal static string TimeSeparator
	{
		get
		{
			if (m_timeSeparator == null)
			{
				m_timeSeparator = GetTimeSeperator();
			}
			return m_timeSeparator;
		}
	}

	[CLSCompliant(false)]
	protected FormatRecord Format
	{
		get
		{
			int numberFormatIndex = m_book.GetExtFormat(ExtendedFormatIndex).NumberFormatIndex;
			return m_book.InnerFormats[numberFormatIndex].Record;
		}
	}

	public bool ContainsNumber
	{
		get
		{
			switch (CellType)
			{
			case TCellType.Formula:
				return FormulaStringValue == null;
			case TCellType.Number:
			case TCellType.RK:
				return true;
			default:
				return false;
			}
		}
	}

	[CLSCompliant(false)]
	protected OfficeFormatType FormatType
	{
		get
		{
			if (!ContainsNumber)
			{
				return InnerNumberFormat.GetFormatType(m_worksheet.GetValue(Record as ICellPositionFormat, preserveOLEDate: false));
			}
			return InnerNumberFormat.GetFormatType(GetNumber());
		}
		set
		{
			if (value == FormatType)
			{
				return;
			}
			switch (value)
			{
			case OfficeFormatType.DateTime:
				if (m_worksheet.IsImporting)
				{
					NumberFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Replace("tt", "AM/PM");
				}
				else
				{
					NumberFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
				}
				break;
			case OfficeFormatType.Number:
				NumberFormat = "0.00";
				break;
			case OfficeFormatType.Text:
				NumberFormat = "@";
				break;
			case OfficeFormatType.Percentage:
				NumberFormat = "0%";
				break;
			case OfficeFormatType.DecimalPercentage:
				NumberFormat = "0.00%";
				break;
			case OfficeFormatType.Exponential:
				NumberFormat = "0.00E+00";
				break;
			case OfficeFormatType.Currency:
			{
				string value2 = null;
				m_book.InnerFormats.CurrencyFormatStrings.TryGetValue(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, out value2);
				NumberFormat = value2;
				break;
			}
			}
		}
	}

	public string WorksheetName => Worksheet.Name;

	internal bool IsSingleCellContainsString
	{
		get
		{
			int i = Row;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = Column;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					if (m_worksheet.GetCellType(i, j, bNeedFormulaSubType: false) == WorksheetImpl.TRangeValueType.String)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	internal ExtendedFormatImpl ExtendedFormat
	{
		get
		{
			int extendedFormatIndex = ExtendedFormatIndex;
			return m_book.InnerExtFormats[extendedFormatIndex];
		}
	}

	TBIFFRecord ICellPositionFormat.TypeCode => (TBIFFRecord)CellType;

	int ICellPositionFormat.Column
	{
		get
		{
			return FirstColumn - 1;
		}
		set
		{
			if (!IsSingleCell)
			{
				throw new ArgumentException("This property can be called only for single cell ranges");
			}
			int firstColumn = (LastColumn = value + 1);
			FirstColumn = firstColumn;
		}
	}

	int ICellPositionFormat.Row
	{
		get
		{
			return FirstRow - 1;
		}
		set
		{
			if (!IsSingleCell)
			{
				throw new ArgumentException("This property can be called only for single cell ranges");
			}
			int firstRow = (LastRow = value + 1);
			FirstRow = firstRow;
		}
	}

	private string GetCultureFormat(string result, double dNumber, FormatImpl numberFormat)
	{
		if (numberFormat.FormatType == OfficeFormatType.DateTime && CheckOSSpecificDateFormats(numberFormat) && result != string.Empty)
		{
			CultureInfo cultureInfo = new CultureInfo(CultureInfo.CurrentCulture.Name);
			DateTime dt = CalcEngineHelper.FromOADate(dNumber);
			result = ((!HasFormulaErrorValue) ? (numberFormat.IsTimeFormat(dNumber) ? dt.TimeOfDay.ToString() : (numberFormat.IsDateFormat(dNumber) ? dt.ToString("d", cultureInfo) : dt.ToString(cultureInfo))) : FormulaErrorValue);
			if (numberFormat.Index == 22)
			{
				result = GetCultureDateTime(cultureInfo, dt);
			}
		}
		return result;
	}

	private string GetCultureDateTime(CultureInfo culture, DateTime dt)
	{
		if (culture.Name == "en-GB")
		{
			return dt.ToString("dd/MM/yyyy HH:mm", culture);
		}
		return dt.ToString(culture);
	}

	private bool CheckOSSpecificDateFormats(FormatImpl InnerNumberFormat)
	{
		if (InnerNumberFormat == null)
		{
			throw new ArgumentNullException("InnerNumberFormat");
		}
		if (Array.IndexOf(osCultureSpecficFormats, InnerNumberFormat.FormatString) >= 0 && CultureInfo.CurrentCulture.Name != "en-US")
		{
			return true;
		}
		return false;
	}

	private IStyle CreateStyleForEntireRowEntireColumn()
	{
		List<IRange> list = new List<IRange>();
		if (m_style == null)
		{
			CreateStyle();
			list.Add(this);
			if (IsEntireRow)
			{
				for (int i = Row; i <= LastRow; i++)
				{
					for (int j = Column; j <= LastColumn; j++)
					{
						if (m_worksheet.CellRecords.GetCellRecord(i, j) != null)
						{
							RangeImpl item = this[i, j] as RangeImpl;
							list.Add(item);
						}
					}
				}
			}
			if (IsEntireColumn)
			{
				for (int k = Column; k <= LastColumn; k++)
				{
					for (int l = Row; l <= LastRow; l++)
					{
						if (m_worksheet.CellRecords.GetCellRecord(l, k) != null)
						{
							RangeImpl item2 = this[l, k] as RangeImpl;
							list.Add(item2);
						}
					}
				}
			}
			if (list.Count == 1)
			{
				return m_style;
			}
			return new StyleArrayWrapper(Application, list, m_worksheet);
		}
		return m_style;
	}

	protected void OnValueChanged(string old, string value)
	{
		CultureInfo cultureInfo = AppImplementation.CheckAndApplySeperators();
		SetChanged();
		if (old != value)
		{
			OnCellValueChanged(old, value, this);
		}
		int numberFormatIndex = m_book.InnerExtFormats[ExtendedFormatIndex].NumberFormatIndex;
		FormatImpl formatImpl = m_book.InnerFormats[numberFormatIndex];
		int num = value?.Length ?? 0;
		if (num == 0)
		{
			if (!(Record is BlankRecord))
			{
				Record = CreateRecordWithoutAdd(TBIFFRecord.Blank);
			}
		}
		else
		{
			if (value == old)
			{
				return;
			}
			bool? flag = IsStringsPreserved;
			if (!flag.HasValue)
			{
				flag = m_worksheet.IsStringsPreserved;
			}
			if (flag == true)
			{
				Text = value;
			}
			else if (value[0] == '=' && num > 1 && value[1] != '&')
			{
				SetFormula(value);
			}
			else
			{
				if (DetectAndSetBoolErrValue(value) || (formatImpl.FormatType == OfficeFormatType.Number && DetectAndSetFractionValue(value)))
				{
					return;
				}
				DateTime dateValue = CalcEngineHelper.FromOADate(0.0);
				double result;
				bool flag2 = double.TryParse(value, (Array.IndexOf(floatNumberStyleCultures, CultureInfo.CurrentCulture.Name) >= 0) ? NumberStyles.Float : NumberStyles.Any, cultureInfo, out result);
				if (!flag2)
				{
					flag2 = double.TryParse(value, (Array.IndexOf(floatNumberStyleCultures, CultureInfo.CurrentCulture.Name) >= 0) ? NumberStyles.Float : NumberStyles.Any, CultureInfo.InvariantCulture, out result);
				}
				if (flag2)
				{
					flag2 = m_worksheet.checkIsNumber(value, cultureInfo);
				}
				bool flag3 = !flag2 && TryParseDateTime(value, out dateValue);
				if (!(flag2 || flag3))
				{
					string text = null;
					if (value.Contains("-"))
					{
						text = value.Replace("-", "/");
					}
					else if (value.Contains("/"))
					{
						text = value.Replace("/", "-");
					}
					if (text != null)
					{
						flag3 = TryParseDateTime(text, out dateValue);
					}
				}
				if (flag3)
				{
					long ticks = dateValue.Ticks;
					if (ticks < MinAllowedDateTicks && ticks != 0L)
					{
						flag3 = false;
					}
					else
					{
						result = dateValue.ToOADate();
						if (result < 60.0 && Worksheet.IsDisplayZeros)
						{
							result -= 1.0;
						}
					}
				}
				if ((flag2 || flag3) && value != double.NaN.ToString() && (formatImpl.FormatType == OfficeFormatType.General || formatImpl.FormatType == OfficeFormatType.Number || formatImpl.FormatType == OfficeFormatType.DateTime))
				{
					if (flag3)
					{
						FormatType = OfficeFormatType.DateTime;
					}
					SetNumber(result);
				}
				else
				{
					value = CheckApostrophe(value);
					RichText.Text = value;
				}
			}
		}
	}

	protected internal bool CheckFormulaArraysNotSeparated(ICollection<ArrayRecord> colFormulas)
	{
		if (colFormulas == null)
		{
			throw new ArgumentNullException("colFormulas");
		}
		int firstRow = FirstRow;
		int firstColumn = FirstColumn;
		int lastRow = LastRow;
		int lastColumn = LastColumn;
		foreach (ArrayRecord colFormula in colFormulas)
		{
			if (colFormula.FirstRow + 1 < firstRow || colFormula.LastRow + 1 > lastRow || colFormula.FirstColumn + 1 < firstColumn || colFormula.LastColumn + 1 > lastColumn)
			{
				return false;
			}
		}
		return true;
	}

	protected int CurrentStyleNumber(string pre)
	{
		return m_book.CurrentStyleNumber(pre);
	}

	protected void OnLastColumnChanged()
	{
	}

	protected void OnFirstColumnChanged()
	{
	}

	protected void OnLastRowChanged()
	{
	}

	protected void OnFirstRowChanged()
	{
	}

	protected void OnStyleChanged(TCellType oldType)
	{
		if (oldType == TCellType.LabelSST && CellType != TCellType.LabelSST)
		{
			string value = Value;
			if (value != null && value.Length != 0)
			{
				m_rtfString.Clear();
			}
		}
		SetChanged();
	}

	private string CheckApostrophe(string value)
	{
		if (value == null || value.Length == 0 || m_book.IsWorkbookOpening)
		{
			return value;
		}
		if (value[0] == '\'')
		{
			CellStyle.IsFirstSymbolApostrophe = true;
			value = value.Substring(1);
		}
		else if (m_book.InnerExtFormats[ExtendedFormatIndex].IsFirstSymbolApostrophe)
		{
			CellStyle.IsFirstSymbolApostrophe = false;
		}
		return value;
	}

	protected double ObjectToDouble(object value)
	{
		if (value is double)
		{
			return (double)value;
		}
		if (value is int)
		{
			return Convert.ToDouble((int)value);
		}
		double result = 0.0;
		if (double.TryParse(value.ToString(), out result))
		{
			return result;
		}
		return double.NaN;
	}

	protected RangeImpl ToggleGroup(OfficeGroupBy groupBy, bool isGroup, bool bCollapsed)
	{
		IOutline outline = null;
		WorksheetImpl obj = Worksheet as WorksheetImpl;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		dictionary = obj.IndexAndLevels;
		if (isGroup)
		{
			SetWorksheetSize();
		}
		if (m_outlineLevels == null)
		{
			m_outlineLevels = new Dictionary<int, List<Point>>();
		}
		obj.OutlineWrappers = null;
		int num;
		int num2;
		OutlineGetter outlineGetter;
		if (groupBy == OfficeGroupBy.ByRows)
		{
			num = FirstRow;
			num2 = LastRow;
			outlineGetter = GetRowOutline;
		}
		else
		{
			num = FirstColumn;
			num2 = LastColumn;
			outlineGetter = GetColumnOutline;
		}
		int num3 = 0;
		for (int i = num; i <= num2; i++)
		{
			outline = outlineGetter(i);
			if (isGroup && outline.OutlineLevel < 7)
			{
				outline.OutlineLevel++;
				if (groupBy == OfficeGroupBy.ByColumns)
				{
					if (dictionary.ContainsKey(i))
					{
						dictionary[i]++;
					}
					else
					{
						dictionary.Add(i, outline.OutlineLevel);
					}
				}
			}
			else if (!isGroup && outline.OutlineLevel > 0)
			{
				num3 = outline.OutlineLevel--;
				if (groupBy == OfficeGroupBy.ByColumns)
				{
					if (num3 > 1)
					{
						dictionary[i]--;
					}
					else
					{
						dictionary.Remove(i);
					}
				}
			}
			if (outline.OutlineLevel == 0)
			{
				outline.IsHidden = false;
			}
			else if (isGroup && (outline.OutlineLevel >= 1 || bCollapsed))
			{
				outline.IsHidden = bCollapsed;
				outline.IsCollapsed = bCollapsed;
			}
		}
		if (groupBy != 0)
		{
			int[] array = new int[dictionary.Count];
			dictionary.Keys.CopyTo(array, 0);
			List<int> list = new List<int>(array);
			list.Sort();
			Dictionary<int, int> dictionary2 = dictionary;
			dictionary = new Dictionary<int, int>();
			foreach (int item in list)
			{
				dictionary.Add(item, dictionary2[item]);
			}
		}
		return this;
	}

	public IOutline GetRowOutline(int iRowIndex)
	{
		return WorksheetHelper.GetOrCreateRow(m_worksheet, iRowIndex - 1, bCreate: true);
	}

	public void SubTotal(int groupBy, ConsolidationFunction function, int[] totalList)
	{
		SubTotal(groupBy, function, totalList, replace: true, pageBreaks: false, summaryBelowData: true);
	}

	public void SubTotal(int groupBy, ConsolidationFunction function, int[] totalList, bool replace, bool pageBreaks, bool summaryBelowData)
	{
		if (totalList != null)
		{
			SubTotalImpl subTotalImpl = new SubTotalImpl(m_worksheet);
			m_noOfSubtotals++;
			LastRow = subTotalImpl.CalculateSubTotal(FirstRow, FirstColumn - 1, LastRow, LastColumn - 1, groupBy, function, m_noOfSubtotals, totalList, replace, pageBreaks, summaryBelowData);
		}
	}

	public IOutline GetColumnOutline(int iColumnIndex)
	{
		ColumnInfoRecord columnInfoRecord = m_worksheet.ColumnInformation[iColumnIndex];
		if (columnInfoRecord == null)
		{
			columnInfoRecord = (ColumnInfoRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ColumnInfo);
			ColumnInfoRecord columnInfoRecord2 = columnInfoRecord;
			ushort firstColumn = (columnInfoRecord.LastColumn = (ushort)(iColumnIndex - 1));
			columnInfoRecord2.FirstColumn = firstColumn;
			columnInfoRecord.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
			m_worksheet.ColumnInformation[iColumnIndex] = columnInfoRecord;
		}
		return columnInfoRecord;
	}

	private void SetWorksheetSize()
	{
		if (m_worksheet.FirstRow > FirstRow || m_worksheet.FirstRow == -1)
		{
			m_worksheet.FirstRow = FirstRow;
		}
		if (m_worksheet.LastRow < LastRow)
		{
			m_worksheet.LastRow = LastRow;
		}
		if (m_worksheet.FirstColumn > FirstColumn || m_worksheet.FirstColumn == int.MaxValue)
		{
			m_worksheet.FirstColumn = FirstColumn;
		}
		if (m_worksheet.LastColumn < LastColumn || m_worksheet.LastColumn == int.MaxValue)
		{
			m_worksheet.LastColumn = LastColumn;
		}
	}

	internal void SetWorkbook(WorkbookImpl book)
	{
		m_book = book;
	}

	private IOutline GetOrCreateOutline(OfficeGroupBy groupBy, IDictionary information, int iIndex, bool bThrowExceptions)
	{
		if (information == null)
		{
			throw new ArgumentNullException("information");
		}
		if (iIndex < 1)
		{
			throw new ArgumentOutOfRangeException("iIndex");
		}
		IOutline outline = null;
		if (information.Contains(iIndex))
		{
			outline = (IOutline)information[iIndex];
		}
		else
		{
			if (groupBy == OfficeGroupBy.ByRows)
			{
				if (iIndex > m_book.MaxRowCount)
				{
					if (bThrowExceptions)
					{
						throw new ArgumentOutOfRangeException("iIndex");
					}
					return null;
				}
				RowRecord obj = (RowRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Row);
				obj.RowNumber = (ushort)(iIndex - 1);
				obj.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
				obj.Height = (ushort)m_worksheet.DefaultRowHeight;
				obj.IsBadFontHeight = false;
				outline = obj;
			}
			else
			{
				if (iIndex > m_book.MaxColumnCount)
				{
					if (bThrowExceptions)
					{
						throw new ArgumentOutOfRangeException("iIndex");
					}
					return null;
				}
				ColumnInfoRecord obj2 = (ColumnInfoRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ColumnInfo);
				ushort lastColumn = (obj2.FirstColumn = (ushort)(iIndex - 1));
				obj2.LastColumn = lastColumn;
				obj2.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
				outline = obj2;
			}
			information.Add(iIndex, outline);
		}
		return outline;
	}

	protected string GetDisplayString()
	{
		switch (CellType)
		{
		case TCellType.RString:
		case TCellType.LabelSST:
		case TCellType.Label:
			return m_worksheet.GetText(m_iTopRow, m_iLeftColumn);
		case TCellType.Formula:
		{
			string formulaStringValue = FormulaStringValue;
			if (formulaStringValue != null && formulaStringValue.Length != 0)
			{
				return FormulaStringValue;
			}
			break;
		}
		case TCellType.BoolErr:
			return Value;
		case TCellType.RK:
			return ParseNumberFormat();
		}
		return string.Empty;
	}

	private string ParseNumberFormat()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = GetNumberFormat().Split(new char[1] { ';' });
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (Array.IndexOf(array[i].ToCharArray(), '@') < 0)
			{
				continue;
			}
			string[] array2 = array[i - 1].Split(new char[1] { '"' });
			foreach (string text in array2)
			{
				if (text.Contains("*"))
				{
					stringBuilder.Append(" ");
				}
				else if (!CheckUnnecessaryChar(text))
				{
					stringBuilder.Append(text);
				}
				else
				{
					if (!text.Contains("?"))
					{
						continue;
					}
					char[] array3 = text.ToCharArray();
					for (int k = 0; k < array3.Length; k++)
					{
						if (array3[k] == '?')
						{
							stringBuilder.Append(" ");
							stringBuilder.Append(" ");
						}
					}
				}
			}
		}
		return stringBuilder.ToString();
	}

	private bool CheckUnnecessaryChar(string splitFormat)
	{
		bool result = false;
		char[] array = splitFormat.ToCharArray();
		foreach (char value in array)
		{
			if (Array.IndexOf(unnecessaryChar, value) >= 0)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	protected DateTime GetDateTime()
	{
		return UtilityMethods.ConvertNumberToDateTime(GetNumber(), m_book.Date1904);
	}

	internal void SetDifferedColumnWidth(RangeImpl sourceRange, RangeImpl destinationRange)
	{
		int num = sourceRange.Columns.Length;
		for (int i = 0; i < num; i++)
		{
			destinationRange.Columns[i].ColumnWidth = sourceRange.Columns[i].ColumnWidth;
		}
	}

	internal void SetDifferedRowHeight(RangeImpl sourceRange, RangeImpl destinationRange)
	{
		int num = sourceRange.Rows.Length;
		for (int i = 0; i < num; i++)
		{
			destinationRange.Rows[i].RowHeight = sourceRange.Rows[i].RowHeight;
		}
	}

	protected void SetDateTime(DateTime value)
	{
		double num = UtilityMethods.ConvertDateTimeToNumber(value);
		if (num >= 0.0)
		{
			SetNumber(num);
		}
		else
		{
			Text = value.Date.ToString();
			NumberFormat = "mm/dd/yyyy";
		}
		if (m_rtfString != null)
		{
			m_rtfString.Clear();
		}
	}

	protected void SetTimeSpan(TimeSpan time)
	{
		string shortDatePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
		string customizedFormat = GetCustomizedFormat(shortDatePattern);
		if (NumberFormat == shortDatePattern || NumberFormat == customizedFormat)
		{
			NumberFormat = "h:mm:ss";
		}
		SetNumber((double)time.Ticks / 864000000000.0);
		if (m_rtfString != null)
		{
			m_rtfString.Clear();
		}
	}

	protected double GetNumber()
	{
		double num = double.NaN;
		if (IsSingleCell)
		{
			switch (CellType)
			{
			case TCellType.RK:
				num = ((RKRecord)Record).RKNumber;
				break;
			case TCellType.Number:
				num = ((NumberRecord)Record).Value;
				break;
			case TCellType.Formula:
				num = ((FormulaRecord)Record).Value;
				break;
			}
		}
		else
		{
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			migrantRangeImpl.ResetRowColumn(Row, Column);
			num = migrantRangeImpl.GetNumber();
			if (!double.IsNaN(num))
			{
				int i = Row;
				for (int lastRow = LastRow; i <= lastRow; i++)
				{
					int j = Column;
					for (int lastColumn = LastColumn; j <= lastColumn; j++)
					{
						migrantRangeImpl.ResetRowColumn(i, j);
						double number = migrantRangeImpl.GetNumber();
						if (num != number)
						{
							num = double.NaN;
							break;
						}
					}
				}
			}
		}
		return num;
	}

	protected void SetNumber(double value)
	{
		TryRemoveFormulaArrays();
		BiffRecordRaw record = CreateNumberRecord(value);
		Record = record;
		if (m_rtfString != null)
		{
			m_rtfString.Clear();
		}
	}

	private void SetNumberAndFormat(double value, bool isPreserveFormat)
	{
		TryRemoveFormulaArrays();
		BiffRecordRaw biffRecordRaw = CreateNumberRecord(value);
		ICellPositionFormat cellPositionFormat = biffRecordRaw as ICellPositionFormat;
		int extendedFormatIndex = cellPositionFormat.ExtendedFormatIndex;
		ExtendedFormatImpl extendedFormatImpl = m_book.InnerExtFormats[extendedFormatIndex];
		int numberFormatIndex = extendedFormatImpl.NumberFormatIndex;
		FormatImpl formatImpl = m_book.InnerFormats[numberFormatIndex];
		if (formatImpl.FormatString != "General")
		{
			OfficeFormatType formatType = formatImpl.GetFormatType(value);
			if (formatType != OfficeFormatType.Number && formatType != OfficeFormatType.General && !isPreserveFormat && formatType != 0)
			{
				numberFormatIndex = m_book.InnerFormats.FindOrCreateFormat("0.00");
				extendedFormatImpl = extendedFormatImpl.Clone() as ExtendedFormatImpl;
				extendedFormatImpl.NumberFormatIndex = numberFormatIndex;
				extendedFormatImpl = m_book.InnerExtFormats.Add(extendedFormatImpl);
				cellPositionFormat.ExtendedFormatIndex = (ushort)extendedFormatImpl.Index;
			}
		}
		Record = biffRecordRaw;
		if (m_rtfString != null)
		{
			m_rtfString.Clear();
		}
	}

	private BiffRecordRaw CreateNumberRecord(double value)
	{
		BiffRecordRaw biffRecordRaw = m_worksheet.TryCreateRkRecord(m_iTopRow, m_iLeftColumn, value);
		if (biffRecordRaw == null)
		{
			NumberRecord obj = (NumberRecord)CreateRecordWithoutAdd(TBIFFRecord.Number);
			obj.Value = value;
			biffRecordRaw = obj;
		}
		return biffRecordRaw;
	}

	protected void SetBoolean(bool value)
	{
		BoolErrRecord boolErrRecord = (BoolErrRecord)CreateRecordWithoutAdd(TBIFFRecord.BoolErr);
		boolErrRecord.IsErrorCode = false;
		boolErrRecord.BoolOrError = (value ? ((byte)1) : ((byte)0));
		Record = boolErrRecord;
		if (m_rtfString != null)
		{
			m_rtfString.Clear();
		}
	}

	protected void SetError(string strError)
	{
		if (strError == null)
		{
			throw new ArgumentNullException("strError");
		}
		if (strError.Length == 0)
		{
			throw new ArgumentException("string can't be empty");
		}
		int errorCodeByString = GetErrorCodeByString(strError);
		if (errorCodeByString != -1)
		{
			BoolErrRecord boolErrRecord = (BoolErrRecord)CreateRecordWithoutAdd(TBIFFRecord.BoolErr);
			boolErrRecord.IsErrorCode = true;
			boolErrRecord.BoolOrError = (byte)errorCodeByString;
			Record = boolErrRecord;
			if (m_rtfString != null)
			{
				m_rtfString.Clear();
			}
			return;
		}
		throw new ArgumentOutOfRangeException("Not error string");
	}

	private int GetErrorCodeByString(string strError)
	{
		if (strError == null || strError.Length == 0)
		{
			throw new ArgumentNullException("strError");
		}
		strError = strError.ToUpper();
		if (strError[0] != '#')
		{
			strError = "#" + strError;
		}
		if (!FormulaUtil.ErrorNameToCode.TryGetValue(strError, out var value))
		{
			return -1;
		}
		return value;
	}

	[CLSCompliant(false)]
	protected internal void SetFormula(FormulaRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		Record = record;
	}

	protected void SetChanged()
	{
		m_worksheet.SetChanged();
	}

	protected void CheckRange(int row, int column)
	{
		if (row < 1 || row > m_book.MaxRowCount || column < 1 || column > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException();
		}
	}

	protected IWorksheet FindWorksheet(string sheetName)
	{
		return m_book.Worksheets[sheetName] ?? throw new ArgumentOutOfRangeException("sheetName");
	}

	private void MoveCellsUp(OfficeCopyRangeOptions options)
	{
		int num = LastRow + 1;
		int firstColumn = FirstColumn;
		int lastRow = m_worksheet.UsedRange.LastRow;
		int lastColumn = LastColumn;
		if (num <= lastRow)
		{
			IRange destination = m_worksheet.Range[FirstRow, FirstColumn];
			IRange source = m_worksheet.Range[num, firstColumn, lastRow, lastColumn];
			m_worksheet.MoveRange(destination, source, options, bUpdateRowRecords: true);
		}
	}

	private void MoveCellsLeft(OfficeCopyRangeOptions options)
	{
		int firstRow = FirstRow;
		int num = LastColumn + 1;
		int lastRow = LastRow;
		int lastColumn = m_worksheet.UsedRange.LastColumn;
		if (num <= lastColumn)
		{
			IRange destination = m_worksheet.Range[FirstRow, FirstColumn];
			IRange source = m_worksheet.Range[firstRow, num, lastRow, lastColumn];
			m_worksheet.MoveRange(destination, source, options, bUpdateRowRecords: false);
		}
	}

	private string ParseLabelSST(LabelSSTRecord label)
	{
		return m_book.InnerSST.GetStringByIndex(label.SSTIndex);
	}

	private string ParseFormula(FormulaRecord formula)
	{
		return ParseFormula(formula, bR1C1ReferenceStyle: false);
	}

	private string ParseFormula(FormulaRecord formula, bool bR1C1ReferenceStyle)
	{
		try
		{
			FormulaUtil formulaUtil = m_book.FormulaUtil;
			ArrayRecord arrayRecord = m_worksheet.CellRecords.GetArrayRecord(formula.Row + 1, formula.Column + 1);
			string text;
			if (arrayRecord != null)
			{
				text = formulaUtil.ParsePtgArray(arrayRecord.Formula, arrayRecord.FirstRow, arrayRecord.FirstColumn, bR1C1ReferenceStyle, isForSerialization: false);
			}
			else
			{
				formula.RecalculateAlways = true;
				formula.CalculateOnOpen = true;
				text = formulaUtil.ParsePtgArray(formula.ParsedExpression, Row - 1, Column - 1, bR1C1ReferenceStyle, isForSerialization: false);
			}
			return "=" + text;
		}
		catch (ParseException)
		{
			if (!m_book.IsWorkbookOpening)
			{
				throw;
			}
			m_book.AddForReparse(this);
		}
		catch (Exception)
		{
			throw;
		}
		return null;
	}

	public void SetRowHeight(double value, bool bIsBadFontHeight)
	{
		if (value < 0.0 || value > 409.5)
		{
			throw new ArgumentOutOfRangeException("RowHeight", "Row Height must be in range from 0 to 409.5");
		}
		int num = FirstRow;
		int num2 = LastRow;
		if (LastRow - FirstRow > m_book.MaxRowCount - (LastRow - FirstRow) && LastRow == m_book.MaxRowCount)
		{
			num = 1;
			num2 = FirstRow - 1;
			m_worksheet.IsZeroHeight = true;
			m_worksheet.IsVisible = true;
		}
		else
		{
			m_worksheet.IsVisible = false;
		}
		int i = num;
		for (int num3 = num2; i <= num3; i++)
		{
			m_worksheet.InnerSetRowHeight(i, value, bIsBadFontHeight, MeasureUnits.Point, bRaiseEvents: true);
		}
	}

	protected void CreateRichTextString()
	{
		if (IsSingleCell)
		{
			m_rtfString = new RangeRichTextString(Application, m_worksheet, m_iTopRow, m_iLeftColumn);
		}
		else
		{
			m_rtfString = new RTFStringArray(this);
		}
	}

	private object TryCreateValue2()
	{
		if (IsBoolean)
		{
			return Boolean;
		}
		if (HasNumber)
		{
			return Number;
		}
		bool flag = false;
		if (NumberFormat.Equals(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, StringComparison.OrdinalIgnoreCase) && Record.TypeCode == TBIFFRecord.LabelSST)
		{
			flag = true;
		}
		if (HasDateTime && !flag)
		{
			FormatImpl innerNumberFormat = InnerNumberFormat;
			if (Number < CultureInfo.CurrentCulture.DateTimeFormat.Calendar.MaxSupportedDateTime.ToOADate())
			{
				if (!innerNumberFormat.IsTimeFormat(Number))
				{
					return DateTime;
				}
				return TimeSpan;
			}
			return Number;
		}
		return null;
	}

	private bool DetectAndSetBoolErrValue(string strValue)
	{
		if (string.Compare(strValue, bool.TrueString, StringComparison.CurrentCultureIgnoreCase) == 0)
		{
			Boolean = true;
			return true;
		}
		if (string.Compare(strValue, bool.FalseString, StringComparison.CurrentCultureIgnoreCase) == 0)
		{
			Boolean = false;
			return true;
		}
		if (FormulaUtil.ErrorNameToCode.ContainsKey(strValue))
		{
			Error = strValue;
			return true;
		}
		return false;
	}

	protected internal void SetLabelSSTIndex(int index)
	{
		if (index == -1)
		{
			if (CellType != TCellType.Blank)
			{
				Record = CreateRecordWithoutAdd(TBIFFRecord.Blank);
			}
			return;
		}
		if (index < 0 || index >= m_book.InnerSST.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		LabelSSTRecord labelSSTRecord = (LabelSSTRecord)CreateRecordWithoutAdd(TBIFFRecord.LabelSST);
		labelSSTRecord.SSTIndex = index;
		Record = labelSSTRecord;
	}

	private void TryRemoveFormulaArrays()
	{
		Dictionary<ArrayRecord, object> formulaArrays = FormulaArrays;
		if (formulaArrays != null && formulaArrays.Count != 0)
		{
			ICollection<ArrayRecord> keys = formulaArrays.Keys;
			if (!CheckFormulaArraysNotSeparated(keys))
			{
				throw new InvalidRangeException("Can't set value.");
			}
			m_worksheet.RemoveArrayFormulas(keys, bClearRange: false);
		}
	}

	private void BlankCell()
	{
		Record = CreateRecord(TBIFFRecord.Blank);
	}

	protected internal void SetParent(WorksheetImpl parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		if (Parent != parent)
		{
			m_worksheet = parent;
			m_book = parent.ParentWorkbook;
		}
	}

	public void UpdateNamedRangeIndexes(int[] arrNewIndex)
	{
		if (Record is FormulaRecord formulaRecord)
		{
			if (arrNewIndex == null)
			{
				throw new ArgumentNullException("arrNewIndex");
			}
			Ptg[] parsedExpression = formulaRecord.ParsedExpression;
			if (m_book.FormulaUtil.UpdateNameIndex(parsedExpression, arrNewIndex))
			{
				formulaRecord.ParsedExpression = parsedExpression;
			}
		}
	}

	private BiffRecordRaw CreateRecord(TBIFFRecord recordType)
	{
		BiffRecordRaw biffRecordRaw = CreateRecordWithoutAdd(recordType);
		m_worksheet.InnerSetCell(m_iLeftColumn, m_iTopRow, biffRecordRaw);
		return biffRecordRaw;
	}

	private BiffRecordRaw CreateRecordWithoutAdd(TBIFFRecord recordType)
	{
		return m_worksheet.GetRecord(recordType, m_iTopRow, m_iLeftColumn);
	}

	public void UpdateRange(int iFirstRow, int iFirstColumn, int iLastRow, int iLastColumn)
	{
		FirstRow = iFirstRow;
		FirstColumn = iFirstColumn;
		LastRow = iLastRow;
		LastColumn = iLastColumn;
		ResetCells();
	}

	internal bool TryParseDateTime(string value, out DateTime dateValue)
	{
		if (!m_book.DetectDateTimeInValue || !value.Contains(DateSeperator))
		{
			dateValue = DateTime.MinValue;
			return false;
		}
		return DateTime.TryParse(value, out dateValue);
	}

	private static string GetDateSeperator()
	{
		string text = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
		if (text.EndsWith("yyyy") || text.EndsWith("YYYY"))
		{
			text = text[text.Length - 5].ToString();
		}
		else if (text.EndsWith("yy") || text.EndsWith("YY"))
		{
			text = text[text.Length - 3].ToString();
		}
		return text;
	}

	private static string GetTimeSeperator()
	{
		string text = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
		if (text.StartsWith("h"))
		{
			text = text.Substring(1, 1);
		}
		else if (text.StartsWith("hh"))
		{
			text = text.Substring(2, 1);
		}
		return text;
	}

	private IRange ParseR1C1Reference(string strReference)
	{
		if (strReference == null)
		{
			throw new ArgumentNullException("strReference");
		}
		if (strReference.Length == 0)
		{
			throw new ArgumentException("strReference - string cannot be empty.");
		}
		string[] array = strReference.Split(':');
		int num = array.Length;
		if (num > 2)
		{
			throw new ArgumentOutOfRangeException("strReference");
		}
		Rectangle rec2 = ParseR1C1Expression(rec: Rectangle.FromLTRB(1, 1, m_book.MaxColumnCount, m_book.MaxRowCount), strName: array[0], bIsFirst: true);
		if (num == 2)
		{
			rec2 = ParseR1C1Expression(array[1], rec2, bIsFirst: false);
		}
		return this[rec2.Top, rec2.Left, rec2.Bottom, rec2.Right];
	}

	private Rectangle ParseR1C1Expression(string strName, Rectangle rec, bool bIsFirst)
	{
		if (strName == null)
		{
			throw new ArgumentNullException("strName");
		}
		if (strName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("strName is empty.");
		}
		int num = strName.IndexOf('C');
		bool num2 = strName[0] == 'R';
		bool flag = num != -1;
		if (!num2 && !flag)
		{
			throw new ArgumentOutOfRangeException("strReference", "Can't locate row or column section.");
		}
		string strValue = (flag ? strName.Substring(num + 1) : null);
		int length = (flag ? num : strName.Length) - 1;
		string strValue2 = (num2 ? strName.Substring(1, length) : null);
		int indexFromR1C = GetIndexFromR1C1(strValue2, bRow: true);
		int indexFromR1C2 = GetIndexFromR1C1(strValue, bRow: false);
		if (num2)
		{
			if (bIsFirst)
			{
				rec.Y = indexFromR1C;
				rec.Height = 0;
			}
			else
			{
				rec.Height = indexFromR1C - rec.Y;
			}
		}
		if (flag)
		{
			if (bIsFirst)
			{
				rec.X = indexFromR1C2;
				rec.Width = 0;
			}
			else
			{
				rec.Width = indexFromR1C2 - rec.X;
			}
		}
		return rec;
	}

	private int GetIndexFromR1C1(string strValue, bool bRow)
	{
		if (strValue == null)
		{
			if (!bRow)
			{
				return m_book.MaxColumnCount;
			}
			return m_book.MaxRowCount;
		}
		int length = strValue.Length;
		if (length == 0)
		{
			if (!bRow)
			{
				return Column;
			}
			return Row;
		}
		bool flag = false;
		if (strValue[0] == '[' && strValue[length - 1] == ']')
		{
			strValue = strValue.Substring(1, length - 2);
			flag = true;
		}
		if (double.TryParse(strValue, NumberStyles.Integer, null, out var result) && result >= -2147483648.0 && result <= 2147483647.0)
		{
			int num = (int)result;
			if (flag)
			{
				num += (bRow ? Row : Column);
			}
			return num;
		}
		throw new ApplicationException("Cannot parse expression.");
	}

	private string GetFormulaArray(bool bR1C1)
	{
		if (CellType != TCellType.Formula && !IsSingleCell)
		{
			return null;
		}
		string text = null;
		if (IsSingleCell)
		{
			if (Record is FormulaRecord formula && m_worksheet.IsArrayFormula(formula))
			{
				text = ParseFormula(formula, bR1C1);
			}
		}
		else
		{
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			migrantRangeImpl.ResetRowColumn(Row, Column);
			text = migrantRangeImpl.GetFormulaArray(bR1C1);
			if (text != null)
			{
				int i = Row;
				for (int lastRow = LastRow; i <= lastRow; i++)
				{
					int j = Column;
					for (int lastColumn = LastColumn; j <= lastColumn; j++)
					{
						migrantRangeImpl.ResetRowColumn(i, j);
						string formulaArray = migrantRangeImpl.GetFormulaArray(bR1C1);
						if (text != formulaArray)
						{
							text = null;
							break;
						}
					}
				}
			}
		}
		return text;
	}

	private void SetFormulaArray(string value, bool bR1C1)
	{
		if (value == null)
		{
			throw new ArgumentNullException("FormulaArray");
		}
		int length = value.Length;
		if (length == 0)
		{
			throw new ArgumentException("FormulaArray can't be empty");
		}
		if (value.StartsWith("{=") && value[length - 1] == '}')
		{
			value = value.Substring(2, length - 3);
			length -= 3;
		}
		else if (value[0] == '=')
		{
			value = value.Substring(1, length - 1);
		}
		TryRemoveFormulaArrays();
		OfficeParseFormulaOptions officeParseFormulaOptions = OfficeParseFormulaOptions.RootLevel | OfficeParseFormulaOptions.InArray;
		if (bR1C1)
		{
			officeParseFormulaOptions |= OfficeParseFormulaOptions.UseR1C1;
		}
		Ptg[] formula = m_book.FormulaUtil.ParseString(value, m_worksheet, null, 0, null, officeParseFormulaOptions, Row - 1, Column - 1);
		ArrayRecord arrayRecord = (ArrayRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Array);
		arrayRecord.Formula = formula;
		arrayRecord.IsRecalculateOnOpen = true;
		arrayRecord.FirstRow = FirstRow - 1;
		arrayRecord.FirstColumn = FirstColumn - 1;
		arrayRecord.LastRow = LastRow - 1;
		arrayRecord.LastColumn = LastColumn - 1;
		SetFormulaArrayRecord(arrayRecord);
	}

	[CLSCompliant(false)]
	public void SetFormulaArrayRecord(ArrayRecord record)
	{
		SetFormulaArrayRecord(record, -1);
	}

	[CLSCompliant(false)]
	public void SetFormulaArrayRecord(ArrayRecord record, int iXFIndex)
	{
		Ptg ptg = FormulaUtil.CreatePtg(FormulaToken.tExp, record.FirstRow, record.FirstColumn);
		Ptg[] parsedExpression = new Ptg[1] { ptg };
		FormulaRecord formulaRecord = (FormulaRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Formula);
		formulaRecord.ParsedExpression = parsedExpression;
		if (iXFIndex != -1)
		{
			formulaRecord.ExtendedFormatIndex = (ushort)iXFIndex;
		}
		if (IsSingleCell)
		{
			UpdateRecord(formulaRecord, this, iXFIndex);
			Record = formulaRecord;
		}
		else
		{
			RangeImpl rangeImpl = (RangeImpl)m_worksheet[FirstRow, FirstColumn];
			UpdateRecord(formulaRecord, rangeImpl, iXFIndex);
			rangeImpl.Record = formulaRecord;
			for (int i = FirstRow; i <= LastRow; i++)
			{
				for (int j = FirstColumn; j <= LastColumn; j++)
				{
					rangeImpl = (RangeImpl)m_worksheet[i, j];
					formulaRecord = (FormulaRecord)formulaRecord.Clone();
					UpdateRecord(formulaRecord, rangeImpl, iXFIndex);
					rangeImpl.SetFormula(formulaRecord);
				}
			}
		}
		m_worksheet.CellRecords.SetArrayFormula(record);
	}

	private void UpdateRecord(ICellPositionFormat record, RangeImpl cell, int iXFIndex)
	{
		record.Row = cell.FirstRow - 1;
		record.Column = cell.FirstColumn - 1;
		if (iXFIndex == -1)
		{
			record.ExtendedFormatIndex = cell.ExtendedFormatIndex;
		}
	}

	private int NormalizeRowIndex(int iRow, int iStartCol, int iEndCol)
	{
		return iRow switch
		{
			-1 => 1, 
			-2 => m_book.MaxRowCount, 
			-4 => m_worksheet.CellRecords.GetMinimumRowIndex(iStartCol, iEndCol), 
			-3 => m_worksheet.CellRecords.GetMaximumRowIndex(iStartCol, iEndCol), 
			_ => iRow, 
		};
	}

	private int NormalizeColumnIndex(int iColumn, int iStartRow, int iEndRow)
	{
		return iColumn switch
		{
			-1 => 1, 
			-2 => m_book.MaxColumnCount, 
			-4 => m_worksheet.CellRecords.GetMinimumColumnIndex(iStartRow, iEndRow), 
			-3 => m_worksheet.CellRecords.GetMaximumColumnIndex(iStartRow, iEndRow), 
			_ => iColumn, 
		};
	}

	public void PartialClear()
	{
		m_cells = null;
		m_style = null;
		m_bCells = false;
		m_rtfString = null;
	}

	protected void SetBorderToSingleCell(OfficeBordersIndex borderIndex, OfficeLineStyle borderLine, OfficeKnownColors borderColor)
	{
		if (!IsSingleCell)
		{
			throw new NotSupportedException("Supports only for single cell.");
		}
		IBorder border = Borders[borderIndex];
		border.LineStyle = borderLine;
		border.Color = borderColor;
	}

	private void CollapseExpand(OfficeGroupBy groupBy, bool isCollapsed, ExpandCollapseFlags flags)
	{
		int iStartIndex;
		int iEndIndex;
		int iMaxIndex;
		bool bLastIndex;
		OutlineGetter outlineGetter;
		if (groupBy == OfficeGroupBy.ByRows)
		{
			iStartIndex = Row;
			iEndIndex = LastRow;
			iMaxIndex = m_book.MaxRowCount;
			bLastIndex = m_worksheet.PageSetup.IsSummaryRowBelow;
			outlineGetter = GetRowOutline;
		}
		else
		{
			iStartIndex = Column;
			iEndIndex = LastColumn;
			iMaxIndex = m_book.MaxColumnCount;
			bLastIndex = m_worksheet.PageSetup.IsSummaryColumnRight;
			outlineGetter = GetColumnOutline;
		}
		CollapseExpand(isCollapsed, iStartIndex, iEndIndex, iMaxIndex, bLastIndex, outlineGetter, flags);
	}

	private void CollapseExpand(bool isCollapsed, int iStartIndex, int iEndIndex, int iMaxIndex, bool bLastIndex, OutlineGetter outlineGetter, ExpandCollapseFlags flags)
	{
		bool includeSubgroups = (flags & ExpandCollapseFlags.IncludeSubgroups) != 0;
		int num = (bLastIndex ? iEndIndex : (iStartIndex - 1));
		if (num <= iMaxIndex && num > 0)
		{
			outlineGetter(num).IsCollapsed = isCollapsed;
		}
		IOutline outline = outlineGetter(iStartIndex);
		Math.Min(val2: outlineGetter(iEndIndex).OutlineLevel, val1: outline.OutlineLevel);
		int iStartIndex2 = iStartIndex;
		int iEndIndex2 = iEndIndex;
		bool flag = IsParentGroupVisible(ref iStartIndex2, ref iEndIndex2, iMaxIndex, outlineGetter);
		if (!flag && (flags & ExpandCollapseFlags.ExpandParent) != 0)
		{
			flag = true;
			CollapseExpand(isCollapsed, iStartIndex2, iEndIndex2, iMaxIndex, bLastIndex, outlineGetter, ExpandCollapseFlags.ExpandParent);
		}
		if (isCollapsed)
		{
			SetHiddenState(iStartIndex, iEndIndex, outlineGetter, state: true);
		}
		else if (flag)
		{
			ExpandOutlines(iStartIndex, iEndIndex, outlineGetter, includeSubgroups, bLastIndex);
		}
	}

	private void SetHiddenState(int iStartIndex, int iEndIndex, OutlineGetter outlineGetter, bool state)
	{
		for (int i = iStartIndex; i <= iEndIndex; i++)
		{
			IOutline outline = outlineGetter(i);
			outline.IsHidden = state;
			outline.IsCollapsed = state;
		}
	}

	private void ExpandOutlines(int iStartIndex, int iEndIndex, OutlineGetter outlineGetter, bool includeSubgroups, bool bLastIndex)
	{
		if (includeSubgroups)
		{
			SetHiddenState(iStartIndex, iEndIndex, outlineGetter, state: false);
			return;
		}
		int num;
		if (bLastIndex)
		{
			SwapValues(ref iStartIndex, ref iEndIndex);
			num = -1;
		}
		else
		{
			num = 1;
		}
		int i = iStartIndex;
		for (int num2 = iEndIndex + num; i != num2; i += num)
		{
			IOutline outline = outlineGetter(i);
			if (outline.IsCollapsed)
			{
				IOutline outline2 = outlineGetter(i + num);
				if (outline.OutlineLevel >= outline2.OutlineLevel)
				{
					outline.IsCollapsed = false;
					outline.IsHidden = false;
				}
				else
				{
					i = FindGroupEdge(i + num, num, int.MaxValue, outlineGetter, outline2.OutlineLevel);
					outline.IsHidden = false;
				}
			}
			else
			{
				outline.IsHidden = false;
			}
		}
	}

	private void SwapValues(ref int iStartIndex, ref int iEndIndex)
	{
		int num = iEndIndex;
		iEndIndex = iStartIndex;
		iStartIndex = num;
	}

	private bool IsParentGroupVisible(ref int iStartIndex, ref int iEndIndex, int iMaxIndex, OutlineGetter outlineGetter)
	{
		if (outlineGetter(iStartIndex).OutlineLevel <= 1)
		{
			return true;
		}
		int num = FindFirstWithLowerLevel(iStartIndex, -1, iMaxIndex, outlineGetter);
		int num2 = FindFirstWithLowerLevel(iEndIndex, 1, iMaxIndex, outlineGetter);
		int val = ((num > 0) ? outlineGetter(num).OutlineLevel : 0);
		int val2 = ((num2 > 0) ? outlineGetter(num2).OutlineLevel : 0);
		int num3 = Math.Min(val, val2);
		if (num3 == 0)
		{
			return true;
		}
		int num4 = FindGroupEdge(iStartIndex, -1, iMaxIndex, outlineGetter, num3);
		int num5 = FindGroupEdge(iEndIndex, 1, iMaxIndex, outlineGetter, num3);
		int outlineLevel = outlineGetter(num4).OutlineLevel;
		iStartIndex = num4;
		iEndIndex = num5;
		return FindVisibleOutline(iStartIndex, iEndIndex, outlineGetter, outlineLevel) != -1;
	}

	private int FindFirstWithLowerLevel(int startIndex, int delta, int maximum, OutlineGetter outlineGetter)
	{
		int outlineLevel = outlineGetter(startIndex).OutlineLevel;
		int result = -1;
		for (int i = startIndex + delta; i > 0 && i <= maximum; i += delta)
		{
			if (outlineGetter(i).OutlineLevel < outlineLevel)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private int FindGroupEdge(int startIndex, int delta, int maximum, OutlineGetter outlineGetter, int parentGroupLevel)
	{
		int num = startIndex;
		IOutline outline;
		do
		{
			num += delta;
			outline = outlineGetter(num);
		}
		while (num > 0 && num <= maximum && outline.OutlineLevel >= parentGroupLevel);
		return num - delta;
	}

	private int FindVisibleOutline(int startIndex, int endIndex, OutlineGetter outlineGetter, int outlineLevel)
	{
		int result = -1;
		for (int i = startIndex; i <= endIndex; i++)
		{
			IOutline outline = outlineGetter(i);
			if (outline.OutlineLevel == outlineLevel && !outline.IsHidden)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private string GetFormulaValue(int row, int column, FormatImpl formatImpl)
	{
		string result = null;
		TCellType cellType = (m_worksheet[row, column] as RangeImpl).CellType;
		string formulaStringValue = m_worksheet.GetFormulaStringValue(row, column);
		if ((m_worksheet.GetCellType(row, column, bNeedFormulaSubType: true) & WorksheetImpl.TRangeValueType.Boolean) == WorksheetImpl.TRangeValueType.Boolean)
		{
			result = m_worksheet.GetFormulaBoolValue(row, column).ToString();
		}
		else if (formulaStringValue == null || cellType == TCellType.RK || cellType == TCellType.Number)
		{
			double formulaNumberValue = m_worksheet.GetFormulaNumberValue(row, column);
			result = (double.IsNaN(formulaNumberValue) ? "" : formatImpl.ApplyFormat(formulaNumberValue));
		}
		else
		{
			switch (cellType)
			{
			case TCellType.Formula:
			case TCellType.RString:
			case TCellType.LabelSST:
			case TCellType.Label:
			{
				string formulaStringValue2 = m_worksheet.GetFormulaStringValue(row, column);
				formulaStringValue2 = formatImpl.ApplyFormat(formulaStringValue2);
				break;
			}
			case TCellType.Blank:
				result = "";
				break;
			default:
				result = (m_worksheet[row, column] as RangeImpl).GetDisplayString();
				break;
			}
		}
		return result;
	}

	private bool DetectAndSetFractionValue(string value)
	{
		string[] array = null;
		Fraction fraction = null;
		double result = 0.0;
		double result2 = 0.0;
		Fraction fraction2 = null;
		Fraction fraction3 = null;
		string text = value;
		if (value.Contains("/"))
		{
			if (value.Contains(" "))
			{
				array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				double.TryParse(array[0], out result);
				fraction2 = new Fraction(result, 1.0);
				if (array.Length > 1)
				{
					text = array[1];
				}
			}
			else
			{
				fraction2 = new Fraction(0.0, 1.0);
			}
			array = text.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			double.TryParse(array[0], out result);
			if (array.Length > 1)
			{
				if (double.TryParse(array[1], out result2) && result2 != 0.0)
				{
					fraction3 = new Fraction(result, result2);
				}
				if (fraction3 != null)
				{
					fraction = fraction2 + fraction3;
					SetNumber(fraction.Numerator / fraction.Denumerator);
					return true;
				}
			}
		}
		return false;
	}

	private void SetTimeFormat(string value)
	{
		bool flag = false;
		int num = 0;
		string aMDesignator = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator;
		string pMDesignator = CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator;
		string text = value;
		if ((text.Contains(aMDesignator) || text.Contains(pMDesignator)) && aMDesignator != "" && pMDesignator != "")
		{
			flag = true;
		}
		while (text.Contains(TimeSeparator))
		{
			num++;
			int startIndex = text.IndexOf(TimeSeparator);
			text = text.Remove(startIndex, 1);
		}
		switch (num)
		{
		case 1:
			if (flag)
			{
				NumberFormat = "h" + TimeSeparator + "mm AM/PM";
			}
			else
			{
				NumberFormat = "h" + TimeSeparator + "mm";
			}
			break;
		case 2:
			if (flag)
			{
				NumberFormat = "h" + TimeSeparator + "mm" + TimeSeparator + "ss AM/PM";
			}
			else
			{
				NumberFormat = "h" + TimeSeparator + "mm" + TimeSeparator + "ss";
			}
			break;
		}
	}

	protected internal void SetFormula(string value)
	{
		SetFormula(value, null, bR1C1: false);
	}

	protected internal void SetFormula(string value, Dictionary<string, string> hashWorksheetNames, bool bR1C1)
	{
		if (Workbook.Version == OfficeVersion.Excel97to2003 && value.Length > 255)
		{
			throw new ArgumentException("The formula is too long. Length should not be longer than 1024");
		}
		if (value.Length > 8192)
		{
			throw new ArgumentException("The formula is too long.Formulas length should not be longer then 8192");
		}
		if (value[0] == '=')
		{
			value = value.Substring(1, value.Length - 1);
		}
		int iCellRow = Row - 1;
		int iCellColumn = Column - 1;
		Ptg[] array = m_book.FormulaUtil.ParseString(value, m_worksheet, hashWorksheetNames, iCellRow, iCellColumn, bR1C1);
		FormulaRecord formulaRecord = (FormulaRecord)CreateRecordWithoutAdd(TBIFFRecord.Formula);
		formulaRecord.ParsedExpression = array;
		Record = formulaRecord;
		FormulaUtil.RaiseFormulaEvaluation(this, new EvaluateEventArgs(this, array));
	}

	private void SetAutoFormatPattern(OfficeKnownColors color, int iRow, int iLastRow, int iCol, int iLastCol)
	{
		SetAutoFormatPattern(color, iRow, iLastRow, iCol, iLastCol, OfficeKnownColors.Black, OfficePattern.Solid);
	}

	private void SetAutoFormatPattern(OfficeKnownColors color, int iRow, int iLastRow, int iCol, int iLastCol, OfficeKnownColors patCol, OfficePattern pat)
	{
		MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, Worksheet);
		for (int i = iRow; i <= iLastRow; i++)
		{
			for (int j = iCol; j <= iLastCol; j++)
			{
				migrantRangeImpl.ResetRowColumn(i, j);
				IStyle cellStyle = migrantRangeImpl.CellStyle;
				cellStyle.FillPattern = pat;
				cellStyle.ColorIndex = color;
				cellStyle.PatternColorIndex = patCol;
			}
		}
	}

	private void SetAutoFormatPatterns(ExcelAutoFormat type)
	{
		int firstRow = FirstRow;
		int lastRow = LastRow;
		int firstColumn = FirstColumn;
		int lastColumn = LastColumn;
		OfficeKnownColors officeKnownColors = (OfficeKnownColors)65;
		OfficeKnownColors officeKnownColors2 = OfficeKnownColors.BlackCustom;
		switch (type)
		{
		case ExcelAutoFormat.Classic_2:
			SetAutoFormatPattern(officeKnownColors, firstRow + 1, lastRow, firstColumn + 1, lastColumn, officeKnownColors2, OfficePattern.None);
			SetAutoFormatPattern(OfficeKnownColors.Grey_25_percent, firstRow + 1, lastRow, firstColumn, firstColumn);
			SetAutoFormatPattern(OfficeKnownColors.Violet, firstRow, firstRow, firstColumn, lastColumn);
			break;
		case ExcelAutoFormat.Classic_3:
			SetAutoFormatPattern(officeKnownColors, lastRow, lastRow, firstColumn, lastColumn, officeKnownColors2, OfficePattern.None);
			SetAutoFormatPattern(OfficeKnownColors.Grey_25_percent, firstRow + 1, lastRow - 1, firstColumn, lastColumn);
			SetAutoFormatPattern(OfficeKnownColors.Dark_blue, firstRow, firstRow, firstColumn, lastColumn);
			break;
		case ExcelAutoFormat.Colorful_1:
			SetAutoFormatPattern(OfficeKnownColors.Dark_blue, firstRow + 1, lastRow, firstColumn, lastColumn);
			SetAutoFormatPattern(OfficeKnownColors.Teal, firstRow + 1, lastRow, firstColumn + 1, lastColumn);
			SetAutoFormatPattern(OfficeKnownColors.Black, firstRow, firstRow, firstColumn, lastColumn);
			break;
		case ExcelAutoFormat.Colorful_2:
		{
			int iLastCol = ((firstColumn == lastColumn) ? lastColumn : (lastColumn - 1));
			SetAutoFormatPattern(OfficeKnownColors.Grey_25_percent, firstRow + 1, lastRow, lastColumn, lastColumn);
			SetAutoFormatPattern(OfficeKnownColors.YellowCustom, firstRow + 1, lastRow, firstColumn, iLastCol, OfficeKnownColors.WhiteCustom, OfficePattern.Percent70);
			SetAutoFormatPattern(OfficeKnownColors.Dark_red, firstRow, firstRow, firstColumn, lastColumn);
			break;
		}
		case ExcelAutoFormat.Colorful_3:
			SetAutoFormatPattern(OfficeKnownColors.Black, firstRow, lastRow, firstColumn, lastColumn);
			break;
		case ExcelAutoFormat.List_1:
			SetListAutoFormatPattern(bIsList_1: true, officeKnownColors, officeKnownColors2);
			break;
		case ExcelAutoFormat.List_2:
			SetListAutoFormatPattern(bIsList_1: false, officeKnownColors, officeKnownColors2);
			break;
		case ExcelAutoFormat.Effect3D_1:
		case ExcelAutoFormat.Effect3D_2:
			SetAutoFormatPattern(OfficeKnownColors.Grey_25_percent, firstRow, lastRow, firstColumn, lastColumn);
			break;
		default:
			SetAutoFormatPattern(officeKnownColors, firstRow, lastRow, firstColumn, lastColumn, officeKnownColors2, OfficePattern.None);
			break;
		}
	}

	private void SetListAutoFormatPattern(bool bIsList_1, OfficeKnownColors foreCol, OfficeKnownColors backColor)
	{
		int firstRow = FirstRow;
		int lastRow = LastRow;
		int firstColumn = FirstColumn;
		int lastColumn = LastColumn;
		SetAutoFormatPattern(foreCol, lastRow, lastRow, firstColumn, lastColumn, backColor, OfficePattern.None);
		OfficeKnownColors color = (bIsList_1 ? OfficeKnownColors.Grey_25_percent : OfficeKnownColors.Light_green);
		int num = (bIsList_1 ? 2 : 4);
		int i = 0;
		for (int num2 = lastRow - firstRow - 1; i < num2; i++)
		{
			if (i % num < num / 2)
			{
				SetAutoFormatPattern(color, i + firstRow + 1, i + firstRow + 1, firstColumn, lastColumn);
			}
			else
			{
				SetAutoFormatPattern(foreCol, i + firstRow + 1, i + firstRow + 1, firstColumn, lastColumn, backColor, OfficePattern.None);
			}
		}
		if (bIsList_1)
		{
			SetAutoFormatPattern(color, firstRow, firstRow, firstColumn, lastColumn);
		}
		else
		{
			SetAutoFormatPattern(OfficeKnownColors.Green, firstRow, firstRow, firstColumn, lastColumn, OfficeKnownColors.Teal, OfficePattern.Percent70);
		}
	}

	private void SetAutoFormatAlignments(ExcelAutoFormat type)
	{
		int firstRow = FirstRow;
		int lastRow = LastRow;
		int firstColumn = FirstColumn;
		int lastColumn = LastColumn;
		if (type == ExcelAutoFormat.None)
		{
			SetAutoFormatAlignment(OfficeHAlign.HAlignGeneral, firstRow, lastRow, firstColumn, lastColumn);
			return;
		}
		OfficeHAlign align = OfficeHAlign.HAlignLeft;
		SetAutoFormatAlignment(OfficeHAlign.HAlignGeneral, firstRow + 1, lastRow, firstColumn + 1, lastColumn);
		SetAutoFormatAlignment(OfficeHAlign.HAlignGeneral, firstRow, firstRow, firstColumn, firstColumn);
		if (firstRow != lastRow)
		{
			SetAutoFormatAlignment(OfficeHAlign.HAlignLeft, lastRow, lastRow, firstColumn, firstColumn);
		}
		if (type == ExcelAutoFormat.List_3)
		{
			align = OfficeHAlign.HAlignGeneral;
		}
		SetAutoFormatAlignment(align, firstRow + 1, lastRow - 1, firstColumn, firstColumn);
		align = OfficeHAlign.HAlignCenter;
		if (Array.IndexOf(DEF_AUTOFORMAT_RIGHT, type) != -1)
		{
			align = OfficeHAlign.HAlignRight;
		}
		SetAutoFormatAlignment(align, firstRow, firstRow, firstColumn + 1, lastColumn);
	}

	private void SetAutoFormatAlignment(OfficeHAlign align, int iRow, int iLastRow, int iCol, int iLastCol)
	{
		MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, Worksheet);
		for (int i = iRow; i <= iLastRow; i++)
		{
			for (int j = iCol; j <= iLastCol; j++)
			{
				migrantRangeImpl.ResetRowColumn(i, j);
				IStyle cellStyle = migrantRangeImpl.CellStyle;
				cellStyle.HorizontalAlignment = align;
				cellStyle.VerticalAlignment = OfficeVAlign.VAlignBottom;
				cellStyle.Rotation = 0;
				cellStyle.IndentLevel = 0;
			}
		}
	}

	private void SetAutoFormatWidthHeight(ExcelAutoFormat type)
	{
		if (type != ExcelAutoFormat.None)
		{
			int i = FirstRow;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				Worksheet.AutofitRow(i);
			}
			int j = FirstColumn;
			for (int lastColumn = LastColumn; j <= lastColumn; j++)
			{
				Worksheet.AutofitColumn(j);
			}
		}
	}

	private void SetAutoFormatNumbers(ExcelAutoFormat type)
	{
		bool flag = type == ExcelAutoFormat.None;
		if (!flag && Array.IndexOf(DEF_AUTOFORMAT_NUMBER, type) == -1)
		{
			return;
		}
		MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
		int numberFormatIndex = 0;
		int i = Row + 1;
		for (int lastRow = LastRow; i <= lastRow; i++)
		{
			int j = Column;
			for (int lastColumn = LastColumn; j <= lastColumn; j++)
			{
				migrantRangeImpl.ResetRowColumn(i, j);
				IStyle cellStyle = migrantRangeImpl.CellStyle;
				if (!flag)
				{
					numberFormatIndex = ((i == Row + 1) ? 44 : 43);
				}
				cellStyle.NumberFormatIndex = numberFormatIndex;
			}
		}
	}

	private void SetAutoFormatFontBorder(ExcelAutoFormat type, bool bIsFont, bool bIsBorder)
	{
		if (bIsFont || bIsBorder)
		{
			switch (type)
			{
			case ExcelAutoFormat.Simple:
				SetAutoFormatSimpleFontBorder(bIsFont, bIsBorder);
				break;
			case ExcelAutoFormat.Classic_1:
				SetAutoFormatFontBorderClassic_1(bIsFont, bIsBorder);
				break;
			case ExcelAutoFormat.Classic_2:
				SetAutoFormatFontBorderClassic_2(bIsFont, bIsBorder);
				break;
			case ExcelAutoFormat.Classic_3:
				SetAutoFormatFontBorderClassic_3(bIsFont, bIsBorder);
				break;
			case ExcelAutoFormat.Accounting_1:
				SetAutoFormatFontBorderAccounting_1(bIsFont, bIsBorder);
				break;
			case ExcelAutoFormat.Accounting_2:
				SetAutoFormatFontBorderAccounting_2(bIsFont, bIsBorder);
				break;
			case ExcelAutoFormat.Accounting_3:
				SetAutoFormatFontBorderAccounting_3(bIsFont, bIsBorder);
				break;
			case ExcelAutoFormat.Accounting_4:
				SetAutoFormatFontBorderAccounting_4(bIsFont, bIsBorder);
				break;
			default:
				throw new NotSupportedException("Unknown auto format type.");
			}
		}
	}

	private void SetAutoFormatSimpleFontBorder(bool bIsFont, bool bIsBorder)
	{
		if (!bIsFont && !bIsBorder)
		{
			return;
		}
		int firstRow = FirstRow;
		int lastRow = LastRow;
		int firstColumn = FirstColumn;
		int lastColumn = LastColumn;
		bool flag = firstRow == lastRow;
		if (bIsFont)
		{
			FontImpl fontImpl = (FontImpl)m_book.InnerFonts[0];
			fontImpl = fontImpl.Clone(m_book.InnerFonts);
			SetAutoFormatFont(fontImpl, firstRow + 1, lastRow, firstColumn + 1, lastColumn);
			int iLastRow = (flag ? lastRow : (lastRow - 1));
			SetAutoFormatFont(fontImpl, firstRow, iLastRow, firstColumn, firstColumn);
			fontImpl = fontImpl.Clone(m_book.InnerFonts);
			fontImpl.Bold = true;
			SetAutoFormatFont(fontImpl, firstRow, firstRow, firstColumn + 1, lastColumn);
			if (!flag)
			{
				SetAutoFormatFont(fontImpl, lastRow, lastRow, firstColumn, firstColumn);
			}
		}
	}

	private void SetAutoFormatFontBorderClassic_1(bool bIsFont, bool bIsBorder)
	{
		if (!bIsFont && !bIsBorder)
		{
			return;
		}
		int firstRow = FirstRow;
		int lastRow = LastRow;
		int firstColumn = FirstColumn;
		int lastColumn = LastColumn;
		bool flag = firstRow == lastRow;
		if (bIsFont)
		{
			FontImpl fontImpl = (FontImpl)m_book.InnerFonts[0];
			fontImpl = fontImpl.Clone(m_book.InnerFonts);
			SetAutoFormatFont(fontImpl, firstRow + 1, lastRow, firstColumn + 1, lastColumn);
			int iLastRow = (flag ? lastRow : (lastRow - 1));
			SetAutoFormatFont(fontImpl, firstRow, iLastRow, firstColumn, firstColumn);
			fontImpl = fontImpl.Clone(m_book.InnerFonts);
			fontImpl.Bold = true;
			SetAutoFormatFont(fontImpl, firstRow, firstRow, firstColumn + 1, lastColumn);
			if (!flag)
			{
				SetAutoFormatFont(fontImpl, lastRow, lastRow, firstColumn, firstColumn);
			}
			if (firstColumn != lastColumn)
			{
				SetAutoFormatFont(fontImpl, firstRow, firstRow, lastColumn, lastColumn);
			}
			fontImpl = fontImpl.Clone(m_book.InnerFonts);
			fontImpl.Bold = false;
			fontImpl.Italic = true;
			SetAutoFormatFont(fontImpl, firstRow, firstRow, firstColumn + 1, lastColumn - 1);
		}
	}

	private void SetAutoFormatFontBorderClassic_2(bool bIsFont, bool bIsBorder)
	{
		if (!bIsFont && !bIsBorder)
		{
			return;
		}
		int firstRow = FirstRow;
		int lastRow = LastRow;
		int firstColumn = FirstColumn;
		int lastColumn = LastColumn;
		FontsCollection innerFonts = m_book.InnerFonts;
		if (bIsFont)
		{
			FontImpl fontImpl = (FontImpl)innerFonts[0];
			fontImpl = fontImpl.Clone(innerFonts);
			SetAutoFormatFont(fontImpl, firstRow + 1, lastRow, firstColumn + 1, lastColumn);
			SetAutoFormatFont(fontImpl, firstRow, firstRow, firstColumn, firstColumn);
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Bold = true;
			SetAutoFormatFont(fontImpl, firstRow + 1, lastRow - 1, firstColumn, firstColumn);
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Color = OfficeKnownColors.Dark_blue;
			if (firstRow != lastRow)
			{
				SetAutoFormatFont(fontImpl, lastRow, lastRow, firstColumn, firstColumn);
			}
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Bold = false;
			fontImpl.Size = 9.0;
			fontImpl.Color = OfficeKnownColors.White;
			SetAutoFormatFont(fontImpl, firstRow, firstRow, firstColumn + 1, lastColumn - 1);
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Bold = true;
			if (firstColumn != lastColumn)
			{
				SetAutoFormatFont(fontImpl, firstRow, firstRow, lastColumn, lastColumn);
			}
		}
	}

	private void SetAutoFormatFontBorderClassic_3(bool bIsFont, bool bIsBorder)
	{
		if (bIsFont || bIsBorder)
		{
			int firstRow = FirstRow;
			int lastRow = LastRow;
			int firstColumn = FirstColumn;
			int lastColumn = LastColumn;
			FontsCollection innerFonts = m_book.InnerFonts;
			if (bIsFont)
			{
				FontImpl fontImpl = (FontImpl)innerFonts[0];
				fontImpl = fontImpl.Clone(innerFonts);
				fontImpl.Color = OfficeKnownColors.Dark_blue;
				SetAutoFormatFont(fontImpl, firstRow + 1, lastRow, firstColumn + 1, lastColumn);
				SetAutoFormatFont(fontImpl, firstRow, firstRow, firstColumn, firstColumn);
				fontImpl = fontImpl.Clone(innerFonts);
				fontImpl.Bold = true;
				fontImpl.Color = OfficeKnownColors.Black;
				SetAutoFormatFont(fontImpl, firstRow + 1, lastRow, firstColumn, firstColumn);
				fontImpl = fontImpl.Clone(innerFonts);
				fontImpl.Color = OfficeKnownColors.White;
				fontImpl.Italic = true;
				fontImpl.Size = 9.0;
				SetAutoFormatFont(fontImpl, firstRow, firstRow, firstColumn + 1, lastColumn);
			}
		}
	}

	private void SetAutoFormatFontBorderAccounting_1(bool bIsFont, bool bIsBorder)
	{
		if (!bIsFont && !bIsBorder)
		{
			return;
		}
		int firstRow = FirstRow;
		int lastRow = LastRow;
		int firstColumn = FirstColumn;
		int lastColumn = LastColumn;
		FontsCollection innerFonts = m_book.InnerFonts;
		if (bIsFont)
		{
			FontImpl fontImpl = (FontImpl)innerFonts[0];
			fontImpl = fontImpl.Clone(innerFonts);
			SetAutoFormatFont(fontImpl, firstRow + 1, lastRow, firstColumn + 1, lastColumn);
			SetAutoFormatFont(fontImpl, firstRow, firstRow, firstColumn, firstColumn);
			SetAutoFormatFont(fontImpl, firstRow + 1, lastRow - 1, firstColumn, firstColumn);
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Italic = true;
			if (firstRow != lastRow)
			{
				SetAutoFormatFont(fontImpl, lastRow, lastRow, firstColumn, firstColumn);
			}
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Bold = true;
			fontImpl.Size = 9.0;
			if (firstColumn != lastColumn)
			{
				SetAutoFormatFont(fontImpl, firstRow, firstRow, lastColumn, lastColumn);
			}
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Color = OfficeKnownColors.Grey_50_percent;
			SetAutoFormatFont(fontImpl, firstRow, firstRow, firstColumn + 1, lastColumn - 1);
		}
	}

	private void SetAutoFormatFontBorderAccounting_2(bool bIsFont, bool bIsBorder)
	{
		if (bIsFont || bIsBorder)
		{
			FontsCollection innerFonts = m_book.InnerFonts;
			if (bIsFont)
			{
				FontImpl fontImpl = (FontImpl)innerFonts[0];
				fontImpl = fontImpl.Clone(innerFonts);
				SetAutoFormatFont(fontImpl, Row, LastRow, Column, LastColumn);
			}
		}
	}

	private void SetAutoFormatFontBorderAccounting_3(bool bIsFont, bool bIsBorder)
	{
		if (!bIsFont && !bIsBorder)
		{
			return;
		}
		int firstRow = FirstRow;
		int lastRow = LastRow;
		int firstColumn = FirstColumn;
		int lastColumn = LastColumn;
		FontsCollection innerFonts = m_book.InnerFonts;
		if (bIsFont)
		{
			FontImpl fontImpl = (FontImpl)innerFonts[0];
			fontImpl = fontImpl.Clone(innerFonts);
			SetAutoFormatFont(fontImpl, firstRow, firstRow, firstColumn, firstColumn);
			SetAutoFormatFont(fontImpl, firstRow + 1, lastRow, firstColumn + 1, lastColumn);
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Italic = true;
			SetAutoFormatFont(fontImpl, firstRow + 1, lastRow - 1, firstColumn, firstColumn);
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Size = 9.0;
			SetAutoFormatFont(fontImpl, firstRow, firstRow, firstColumn + 1, lastColumn - 1);
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Bold = true;
			fontImpl.Italic = true;
			if (firstColumn != lastColumn)
			{
				SetAutoFormatFont(fontImpl, firstRow, firstRow, lastColumn, lastColumn);
			}
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Bold = true;
			fontImpl.Italic = false;
			fontImpl.Size = 10.0;
			if (firstRow != lastRow)
			{
				SetAutoFormatFont(fontImpl, lastRow, lastRow, firstColumn, firstColumn);
			}
		}
	}

	private void SetAutoFormatFontBorderAccounting_4(bool bIsFont, bool bIsBorder)
	{
		if (!bIsFont && !bIsBorder)
		{
			return;
		}
		int firstRow = FirstRow;
		int lastRow = LastRow;
		int firstColumn = FirstColumn;
		int lastColumn = LastColumn;
		FontsCollection innerFonts = m_book.InnerFonts;
		if (bIsFont)
		{
			FontImpl fontImpl = (FontImpl)innerFonts[0];
			fontImpl = fontImpl.Clone(innerFonts);
			SetAutoFormatFont(fontImpl, firstRow, lastRow, firstColumn, firstColumn);
			SetAutoFormatFont(fontImpl, firstRow + 1, lastRow - 2, firstColumn + 1, lastColumn);
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Underline = OfficeUnderline.SingleAccounting;
			SetAutoFormatFont(fontImpl, firstRow, firstRow, firstColumn + 1, lastColumn);
			if (lastRow - firstRow > 1)
			{
				SetAutoFormatFont(fontImpl, lastRow - 1, lastRow - 1, firstColumn + 1, lastColumn);
			}
			fontImpl = fontImpl.Clone(innerFonts);
			fontImpl.Underline = OfficeUnderline.DoubleAccounting;
			if (firstRow != lastRow)
			{
				SetAutoFormatFont(fontImpl, lastRow, lastRow, firstColumn + 1, lastColumn);
			}
		}
	}

	private void SetAutoFormatFont(IOfficeFont font, int iRow, int iLastRow, int iCol, int iLastCol)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (iRow > iLastRow || iCol > iLastCol)
		{
			return;
		}
		MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, Worksheet);
		migrantRangeImpl.ResetRowColumn(iRow, iCol);
		IOfficeFont font2 = migrantRangeImpl.CellStyle.Font;
		font2.BeginUpdate();
		font2.Bold = font.Bold;
		font2.Color = font.Color;
		font2.FontName = font.FontName;
		font2.Italic = font.Italic;
		font2.MacOSOutlineFont = font.MacOSOutlineFont;
		font2.MacOSShadow = font.MacOSShadow;
		font2.Size = font.Size;
		font2.Strikethrough = font.Strikethrough;
		font2.Subscript = font.Subscript;
		font2.Superscript = font.Superscript;
		font2.Strikethrough = font.Strikethrough;
		font2.Underline = font.Underline;
		font2.EndUpdate();
		int fontIndex = migrantRangeImpl.m_style.FontIndex;
		for (int i = iRow; i <= iLastRow; i++)
		{
			for (int j = iCol; j <= iLastCol; j++)
			{
				migrantRangeImpl.ResetRowColumn(i, j);
				((ExtendedFormatWrapper)migrantRangeImpl.CellStyle).FontIndex = fontIndex;
			}
		}
	}

	public RangeImpl(IApplication application, object parent)
	{
		SetParents(parent);
	}

	[CLSCompliant(false)]
	public RangeImpl(IApplication application, object parent, BiffReader reader)
		: this(application, parent)
	{
		Parse(reader);
	}

	[CLSCompliant(false)]
	public RangeImpl(IApplication application, object parent, BiffRecordRaw[] data, int position)
		: this(application, parent)
	{
		Parse(data, ref position);
	}

	[CLSCompliant(false)]
	public RangeImpl(IApplication application, object parent, BiffRecordRaw[] data, ref int position)
		: this(application, parent)
	{
		Parse(data, ref position);
	}

	[CLSCompliant(false)]
	public RangeImpl(IApplication application, object parent, BiffRecordRaw[] data, ref int position, bool ignoreStyles)
		: this(application, parent)
	{
		Parse(data, ref position, ignoreStyles);
	}

	public RangeImpl(IApplication application, object parent, List<BiffRecordRaw> data, ref int position, bool ignoreStyles)
		: this(application, parent)
	{
		Parse(data, ref position, ignoreStyles);
	}

	public RangeImpl(IApplication application, object parent, int firstCol, int firstRow, int lastCol, int lastRow)
		: this(application, parent)
	{
		if (firstCol > lastCol)
		{
			throw new ArgumentOutOfRangeException("firstCol or lastCol");
		}
		if (firstRow > lastRow)
		{
			throw new ArgumentOutOfRangeException("firstRow or lastRow");
		}
		FirstColumn = firstCol;
		FirstRow = firstRow;
		LastColumn = lastCol;
		LastRow = lastRow;
	}

	public RangeImpl(IApplication application, object parent, int column, int row)
		: this(application, parent)
	{
		FirstColumn = column;
		LastColumn = column;
		FirstRow = row;
		LastRow = row;
	}

	[CLSCompliant(false)]
	public RangeImpl(IApplication application, object parent, BiffRecordRaw record, bool bIgnoreStyles)
		: this(application, parent, new BiffRecordRaw[1] { record }, 0)
	{
	}

	protected internal void InfillCells()
	{
		if (m_bCells)
		{
			return;
		}
		m_cells = new List<IRange>();
		if (FirstRow > 0 && FirstColumn > 0)
		{
			int i = FirstRow;
			for (int lastRow = LastRow; i <= lastRow; i++)
			{
				int j = FirstColumn;
				for (int lastColumn = LastColumn; j <= lastColumn; j++)
				{
					m_cells.Add(m_worksheet.InnerGetCell(j, i));
				}
			}
		}
		m_bCells = true;
	}

	protected internal void ResetCells()
	{
		if (m_cells != null)
		{
			m_cells.Clear();
		}
		m_cells = null;
		m_bCells = false;
	}

	public void Dispose()
	{
		if (m_style != null)
		{
			m_style = null;
		}
		if (m_rtfString != null)
		{
			m_rtfString.Dispose();
		}
		GC.SuppressFinalize(this);
	}

	private void CheckDisposed()
	{
	}

	private void SetParents(object parent)
	{
		m_worksheet = parent as WorksheetImpl;
		if (m_worksheet == null)
		{
			throw new ApplicationException("Range object must be a child of worksheet object tree");
		}
		m_book = m_worksheet.ParentWorkbook;
	}

	[CLSCompliant(false)]
	public void Parse(BiffReader reader)
	{
		throw new NotImplementedException();
	}

	[CLSCompliant(false)]
	public void Parse(BiffRecordRaw[] data, ref int position)
	{
		Parse(data, ref position, ignoreStyles: false);
	}

	public void Parse(IList data, ref int position, bool ignoreStyles)
	{
		BiffRecordRaw biffRecordRaw = (BiffRecordRaw)data[position];
		ICellPositionFormat cellPositionFormat = (ICellPositionFormat)biffRecordRaw;
		int firstColumn = (LastColumn = cellPositionFormat.Column + 1);
		FirstColumn = firstColumn;
		firstColumn = (LastRow = cellPositionFormat.Row + 1);
		FirstRow = firstColumn;
		switch (biffRecordRaw.TypeCode)
		{
		case TBIFFRecord.Number:
		case TBIFFRecord.RK:
			ParseDouble((IDoubleValue)biffRecordRaw);
			break;
		case TBIFFRecord.Blank:
			ParseBlank((BlankRecord)biffRecordRaw);
			break;
		case TBIFFRecord.Formula:
			ParseFormula((FormulaRecord)biffRecordRaw, data, ref position);
			break;
		case TBIFFRecord.BoolErr:
			ParseBoolError((BoolErrRecord)biffRecordRaw);
			break;
		case TBIFFRecord.RString:
			ParseRString((RStringRecord)biffRecordRaw);
			break;
		default:
			throw new ArgumentException("Unknown to Range biff record type");
		case TBIFFRecord.LabelSST:
		case TBIFFRecord.Label:
			break;
		}
	}

	protected string ParseDouble(IDoubleValue value)
	{
		double doubleValue = value.DoubleValue;
		if (InnerNumberFormat.GetFormatType(doubleValue) == OfficeFormatType.DateTime)
		{
			if (doubleValue < 2958466.0)
			{
				return DateTime.ToString();
			}
			return doubleValue.ToString();
		}
		return doubleValue.ToString();
	}

	[CLSCompliant(false)]
	protected string ParseBlank(BlankRecord blank)
	{
		return string.Empty;
	}

	[CLSCompliant(false)]
	protected void ReParseFormula(FormulaRecord formula)
	{
		throw new NotImplementedException();
	}

	[CLSCompliant(false)]
	protected void ParseFormula(FormulaRecord formula, IList data, ref int pos)
	{
	}

	[CLSCompliant(false)]
	public static string ParseBoolError(BoolErrRecord error)
	{
		if (error.IsErrorCode)
		{
			if (FormulaUtil.ErrorCodeToName.ContainsKey(error.BoolOrError))
			{
				return FormulaUtil.ErrorCodeToName[error.BoolOrError];
			}
			return "#N/A";
		}
		return (error.BoolOrError == 1).ToString().ToUpper();
	}

	[CLSCompliant(false)]
	protected string ParseRString(RStringRecord rstring)
	{
		return string.Empty;
	}

	private void AddRemoveEventListenersForNameX(Ptg[] parsedFormula, int iBookIndex, int iNameIndex, bool bAdd)
	{
		if (parsedFormula == null)
		{
			throw new ArgumentNullException("parsedFormula");
		}
		NameImpl.NameIndexChangedEventHandler handler = OnNameXIndexChanged;
		AttachDetachNameIndexChangedEvent(m_book, handler, parsedFormula, iBookIndex, iNameIndex, bAdd);
	}

	public static void AttachDetachNameIndexChangedEvent(WorkbookImpl book, NameImpl.NameIndexChangedEventHandler handler, Ptg[] parsedFormula, int iBookIndex, int iNewIndex, bool bAdd)
	{
		try
		{
			Dictionary<long, object> indexes = new Dictionary<long, object>();
			int i = 0;
			for (int num = parsedFormula.Length; i < num; i++)
			{
				if (FormulaUtil.IndexOf(FormulaUtil.NameXCodes, parsedFormula[i].TokenCode) != -1)
				{
					NameXPtg nameXPtg = (NameXPtg)parsedFormula[i];
					_ = nameXPtg.NameIndex;
					_ = nameXPtg.RefIndex;
					AttachDetachExternNameEvent(book, nameXPtg, iBookIndex, iNewIndex, handler, indexes, bAdd);
				}
				else if (FormulaUtil.IndexOf(FormulaUtil.NameCodes, parsedFormula[i].TokenCode) != -1)
				{
					NamePtg namePtg = (NamePtg)parsedFormula[i];
					_ = namePtg.ExternNameIndex;
					AttachDetachLocalNameEvent(book, namePtg, iBookIndex, iNewIndex, handler, indexes, bAdd);
				}
			}
		}
		catch (Exception innerException)
		{
			if (book.IsWorkbookOpening)
			{
				throw new ParseException("Parse exception", innerException);
			}
			throw;
		}
	}

	private static void AttachDetachExternNameEvent(WorkbookImpl book, NameXPtg namex, int iBookIndex, int iNewIndex, NameImpl.NameIndexChangedEventHandler handler, Dictionary<long, object> indexes, bool bAdd)
	{
		if (namex == null)
		{
			throw new ArgumentNullException("namex");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		int bookIndex = book.GetBookIndex(namex.RefIndex);
		bool flag = namex.RefIndex < 0 || book.IsLocalReference(namex.RefIndex);
		long index = GetIndex(flag ? (-1) : bookIndex, namex.NameIndex);
		if (indexes.ContainsKey(index))
		{
			return;
		}
		bool flag2 = iBookIndex == -1 && iNewIndex == -1;
		if (flag && (namex.NameIndex == iNewIndex || flag2))
		{
			((NameImpl)book.Names[namex.NameIndex - 1]).NameIndexChanged += handler;
			indexes.Add(index, null);
		}
		else if (!flag && ((iBookIndex == bookIndex && iBookIndex != -1 && namex.NameIndex == iNewIndex) || flag2))
		{
			ExternNameImpl externNameImpl = book.ExternWorkbooks[bookIndex].ExternNames[namex.NameIndex - 1];
			if (bAdd)
			{
				externNameImpl.NameIndexChanged += handler;
			}
			else
			{
				externNameImpl.NameIndexChanged -= handler;
			}
			indexes.Add(index, null);
		}
	}

	private static void AttachDetachLocalNameEvent(WorkbookImpl book, NamePtg name, int iBookIndex, int iNewIndex, NameImpl.NameIndexChangedEventHandler handler, Dictionary<long, object> indexes, bool bAdd)
	{
		if (name == null)
		{
			throw new ArgumentNullException("namex");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		long index = GetIndex(-1, name.ExternNameIndex);
		bool flag = iBookIndex == -1 && iNewIndex == -1;
		if (!indexes.ContainsKey(index) && ((iBookIndex == -1 && name.ExternNameIndex == iNewIndex) || flag))
		{
			NameImpl nameImpl = (NameImpl)book.Names[name.ExternNameIndex - 1];
			if (bAdd)
			{
				nameImpl.NameIndexChanged += handler;
			}
			else
			{
				nameImpl.NameIndexChanged -= handler;
			}
			indexes.Add(index, null);
		}
	}

	private static long GetIndex(int iBookIndex, int iNameIndex)
	{
		return iBookIndex << 32 + iNameIndex;
	}

	private void OnNameXIndexChanged(object sender, NameIndexChangedEventArgs e)
	{
		((INameIndexChangedEventProvider)sender).NameIndexChanged -= OnNameXIndexChanged;
		if (sender is NameImpl)
		{
			LocalIndexChanged((NameImpl)sender, e);
		}
		else if (sender is ExternNameImpl)
		{
			ExternIndexChanged((ExternNameImpl)sender, e);
		}
	}

	private void LocalIndexChanged(NameImpl sender, NameIndexChangedEventArgs e)
	{
		if (CellType != TCellType.Formula)
		{
			return;
		}
		FormulaRecord formulaRecord = (FormulaRecord)Record;
		Ptg[] parsedExpression = formulaRecord.ParsedExpression;
		int i = 0;
		for (int num = parsedExpression.Length; i < num; i++)
		{
			Ptg ptg = parsedExpression[i];
			if (ptg is NameXPtg)
			{
				NameXPtg nameXPtg = ptg as NameXPtg;
				formulaRecord.IsFillFromExpression = true;
				if (nameXPtg.RefIndex == ushort.MaxValue || (m_book.IsLocalReference(nameXPtg.RefIndex) && e.OldIndex == nameXPtg.NameIndex - 1))
				{
					nameXPtg.NameIndex = (ushort)(e.NewIndex + 1);
				}
			}
			if (ptg is NamePtg)
			{
				NamePtg namePtg = ptg as NamePtg;
				formulaRecord.IsFillFromExpression = true;
				if (e.OldIndex == namePtg.ExternNameIndex - 1)
				{
					namePtg.ExternNameIndex = (ushort)(e.NewIndex + 1);
				}
			}
		}
	}

	private void ExternIndexChanged(ExternNameImpl sender, NameIndexChangedEventArgs e)
	{
		if (CellType != TCellType.Formula)
		{
			return;
		}
		FormulaRecord formulaRecord = (FormulaRecord)Record;
		Ptg[] parsedExpression = formulaRecord.ParsedExpression;
		int i = 0;
		for (int num = parsedExpression.Length; i < num; i++)
		{
			Ptg ptg = parsedExpression[i];
			if (ptg is NameXPtg)
			{
				NameXPtg nameXPtg = ptg as NameXPtg;
				formulaRecord.IsFillFromExpression = true;
				if (nameXPtg.RefIndex == sender.BookIndex && e.OldIndex == nameXPtg.NameIndex - 1)
				{
					nameXPtg.NameIndex = (ushort)(e.NewIndex + 1);
				}
			}
		}
	}

	public IRange Activate()
	{
		CheckDisposed();
		if (IsSingleCell)
		{
			m_worksheet.SetActiveCell(this);
			return this;
		}
		return null;
	}

	public virtual IRange Activate(bool scroll)
	{
		Activate();
		if (scroll)
		{
			m_worksheet.TopLeftCell = this;
		}
		m_worksheet.Activate();
		return this;
	}

	public IRange Group(OfficeGroupBy groupBy, bool bCollapsed)
	{
		CheckDisposed();
		return ToggleGroup(groupBy, isGroup: true, bCollapsed);
	}

	public IRange Group(OfficeGroupBy groupBy)
	{
		CheckDisposed();
		return Group(groupBy, bCollapsed: false);
	}

	public void Merge()
	{
		Merge(clearCells: false);
	}

	public void Merge(bool clearCells)
	{
		CheckDisposed();
		if (IsSingleCell)
		{
			return;
		}
		m_worksheet.MergeCells.AddMerge(this, OfficeMergeOperation.Delete);
		m_worksheet.ClearExceptFirstCell(this, clearCells);
		if (m_book.IsLoaded)
		{
			return;
		}
		int column = Column;
		int row = Row;
		int lastRow = LastRow;
		_ = LastColumn;
		if (row != lastRow)
		{
			return;
		}
		int i = column;
		for (int lastColumn = LastColumn; i <= lastColumn; i++)
		{
			if (this[row, i] != null)
			{
				RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(Worksheet as WorksheetImpl, row - 1, bCreate: false);
				if (this[row, i].WrapText && !orCreateRow.IsBadFontHeight)
				{
					m_worksheet.AutofitRow(row, i, lastColumn, bRaiseEvents: true);
				}
			}
		}
	}

	internal void MergeWithoutCheck()
	{
		if (!IsSingleCell)
		{
			m_worksheet.MergeCells.AddMerge(this, OfficeMergeOperation.Leave);
		}
	}

	public IRange Ungroup(OfficeGroupBy groupBy)
	{
		CheckDisposed();
		return ToggleGroup(groupBy, isGroup: false, bCollapsed: false);
	}

	public void UnMerge()
	{
		CheckDisposed();
		Rectangle range = Rectangle.FromLTRB(FirstColumn - 1, FirstRow - 1, LastColumn - 1, LastRow - 1);
		m_worksheet.MergeCells.DeleteMerge(range);
	}

	public void FreezePanes()
	{
		CheckDisposed();
		if (IsSingleCell)
		{
			if (Column == 1)
			{
				m_worksheet.TopLeftCell = m_worksheet[FirstColumn, FirstColumn];
			}
			m_worksheet.SetPaneCell(this);
			m_worksheet.SetActiveCell(m_worksheet[FirstRow, FirstColumn]);
		}
		else
		{
			RangeImpl paneCell = Worksheet[FirstRow, FirstColumn] as RangeImpl;
			m_worksheet.SetPaneCell(paneCell);
		}
	}

	public void Clear()
	{
		CheckDisposed();
		Clear(isClearFormat: false);
	}

	public void Clear(bool isClearFormat)
	{
		CheckDisposed();
		if (IsSingleCell)
		{
			BlankCell();
		}
		else
		{
			MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
			int lastRow = LastRow;
			int lastColumn = LastColumn;
			if (lastRow == m_book.MaxRowCount)
			{
				lastRow = m_worksheet.UsedRange.LastRow;
			}
			if (lastColumn == m_book.MaxColumnCount)
			{
				lastColumn = m_worksheet.UsedRange.LastColumn;
			}
			for (int i = FirstRow; i <= lastRow; i++)
			{
				for (int j = FirstColumn; j <= lastColumn; j++)
				{
					migrantRangeImpl.ResetRowColumn(i, j);
					if (!migrantRangeImpl.IsBlank)
					{
						migrantRangeImpl.BlankCell();
					}
				}
			}
		}
		if (isClearFormat)
		{
			CellStyleName = "Normal";
		}
	}

	public void FullClear()
	{
		CheckDisposed();
		if (IsSingleCell)
		{
			Clear(isClearFormat: true);
			return;
		}
		MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
		for (int i = FirstRow; i <= LastRow; i++)
		{
			for (int j = FirstColumn; j <= LastColumn; j++)
			{
				migrantRangeImpl.ResetRowColumn(i, j);
				migrantRangeImpl.FullClear();
			}
		}
	}

	public void Clear(OfficeMoveDirection direction)
	{
		CheckDisposed();
		Clear(direction, OfficeCopyRangeOptions.None);
	}

	public void Clear(OfficeMoveDirection direction, OfficeCopyRangeOptions options)
	{
		CheckDisposed();
		switch (direction)
		{
		case OfficeMoveDirection.None:
			Clear(isClearFormat: true);
			break;
		case OfficeMoveDirection.MoveUp:
			Clear(isClearFormat: true);
			MoveCellsUp(options);
			break;
		case OfficeMoveDirection.MoveLeft:
			Clear(isClearFormat: true);
			MoveCellsLeft(options);
			break;
		}
	}

	internal void ClearOption(OfficeClearOptions option)
	{
		CheckDisposed();
		switch (option)
		{
		case OfficeClearOptions.ClearFormat:
		{
			CellStyleName = "Normal";
			int num = 0;
			int num2 = 0;
			List<IRange> cellsList2 = CellsList;
			int j = 0;
			for (int count2 = cellsList2.Count; j < count2; j++)
			{
				RangeImpl rangeImpl = (RangeImpl)cellsList2[j];
				if (num == 0 && !rangeImpl.IsBlank)
				{
					num = rangeImpl.Row;
				}
				if (!rangeImpl.IsBlank)
				{
					num2 = rangeImpl.Row;
				}
			}
			if (IsEntireRow && IsBlankorHasStyle)
			{
				if (LastRow == m_worksheet.LastRow)
				{
					m_worksheet.LastRow = Row - 1;
				}
				else if (LastRow <= m_worksheet.LastRow && Row == m_worksheet.FirstRow)
				{
					m_worksheet.FirstRow = LastRow + 1;
				}
				break;
			}
			if (num != FirstRow && num2 != LastRow && m_worksheet.FirstRow >= FirstRow && m_worksheet.LastRow == LastRow)
			{
				m_worksheet.FirstRow = num;
				m_worksheet.LastRow = num2;
			}
			if (num2 == LastRow && m_worksheet.FirstRow >= FirstRow)
			{
				m_worksheet.FirstRow = num;
			}
			if (num == FirstRow && m_worksheet.LastRow == LastRow)
			{
				m_worksheet.LastRow = num2;
			}
			break;
		}
		case OfficeClearOptions.ClearContent:
		{
			List<IRange> cellsList6 = CellsList;
			int n = 0;
			for (int count6 = cellsList6.Count; n < count6; n++)
			{
				((RangeImpl)cellsList6[n]).Value = null;
			}
			break;
		}
		case OfficeClearOptions.ClearComment:
		{
			List<IRange> cellsList4 = CellsList;
			int l = 0;
			for (int count4 = cellsList4.Count; l < count4; l++)
			{
				((RangeImpl)cellsList4[l]).Comments();
			}
			break;
		}
		case OfficeClearOptions.ClearConditionalFormats:
		{
			List<IRange> cellsList3 = CellsList;
			int k = 0;
			for (int count3 = cellsList3.Count; k < count3; k++)
			{
				_ = (RangeImpl)cellsList3[k];
			}
			break;
		}
		case OfficeClearOptions.ClearDataValidations:
		{
			List<IRange> cellsList5 = CellsList;
			int m = 0;
			for (int count5 = cellsList5.Count; m < count5; m++)
			{
				_ = (RangeImpl)cellsList5[m];
			}
			break;
		}
		default:
		{
			List<IRange> cellsList = CellsList;
			int i = 0;
			for (int count = cellsList.Count; i < count; i++)
			{
				RangeImpl obj = (RangeImpl)cellsList[i];
				obj.Value = null;
				obj.Comments();
			}
			CellStyleName = "Normal";
			if (Row == m_worksheet.FirstRow && LastRow == m_worksheet.LastRow)
			{
				m_worksheet.Clear();
			}
			else if (LastRow == m_worksheet.LastRow)
			{
				m_worksheet.LastRow = Row - 1;
			}
			else if (LastRow <= m_worksheet.LastRow && Row == m_worksheet.FirstRow)
			{
				m_worksheet.FirstRow = LastRow + 1;
			}
			break;
		}
		}
	}

	public void Clear(OfficeClearOptions option)
	{
		ClearOption(option);
	}

	internal void Comments()
	{
	}

	public void MoveTo(IRange destination)
	{
		CheckDisposed();
		MoveTo(destination, OfficeCopyRangeOptions.All);
	}

	public void MoveTo(IRange destination, OfficeCopyRangeOptions options)
	{
		CheckDisposed();
		if (this != destination)
		{
			m_worksheet.MoveRange(destination, this, options, bUpdateRowRecords: false);
		}
	}

	public IRange CopyTo(IRange destination)
	{
		CheckDisposed();
		if (this == destination)
		{
			return destination;
		}
		return m_worksheet.CopyRange(destination, this, OfficeCopyRangeOptions.All);
	}

	public IRange CopyTo(IRange destination, OfficeCopyRangeOptions options)
	{
		CheckDisposed();
		if (this == destination)
		{
			return destination;
		}
		return m_worksheet.CopyRange(destination, this, options);
	}

	public IRange IntersectWith(IRange range)
	{
		CheckDisposed();
		return m_worksheet.IntersectRanges(this, range);
	}

	public IRange MergeWith(IRange range)
	{
		CheckDisposed();
		return m_worksheet.MergeRanges(this, range);
	}

	public SizeF MeasureString(string strMeasure)
	{
		CheckDisposed();
		return (CellStyle.Font as FontWrapper).Wrapped.MeasureString(strMeasure);
	}

	public void AutofitRows()
	{
		CheckDisposed();
		int column = Column;
		int lastColumn = LastColumn;
		int i = FirstRow;
		for (int lastRow = LastRow; i <= lastRow; i++)
		{
			m_worksheet.AutofitRow(i, column, lastColumn, bRaiseEvents: true);
		}
	}

	public void AutofitColumns()
	{
		CheckDisposed();
		_ = Row;
		_ = LastRow;
		AutoFitToColumn(FirstColumn, LastColumn);
	}

	public void AutoFitToColumn(int firstColumn, int lastColumn)
	{
		int firstRow = FirstRow;
		int lastRow = LastRow;
		if (firstRow == 0 || lastRow == 0 || firstRow > lastRow)
		{
			return;
		}
		if (firstColumn < 1 || firstColumn > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("firstColumn");
		}
		if (lastColumn < 1 || lastColumn > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("lastColumn");
		}
		using AutoFitManager autoFitManager = new AutoFitManager(firstRow, firstColumn, lastRow, lastColumn, this);
		autoFitManager.MeasureToFitColumn();
	}

	internal static bool IsMergedCell(MergeCellsImpl mergedCells, int iRow, int iColumn, bool isRow, ref int num4)
	{
		if (mergedCells != null)
		{
			Rectangle rect = Rectangle.FromLTRB(iColumn - 1, iRow - 1, iColumn - 1, iRow - 1);
			MergeCellsRecord.MergedRegion mergedRegion = mergedCells[rect];
			if (mergedRegion != null && mergedRegion.RowFrom <= iRow - 1 && mergedRegion.RowTo >= iRow - 1 && mergedRegion.ColumnFrom <= iColumn - 1 && mergedRegion.ColumnTo >= iColumn - 1)
			{
				if (isRow)
				{
					if (mergedRegion.RowFrom == mergedRegion.RowTo)
					{
						num4 = mergedRegion.ColumnTo - mergedRegion.ColumnFrom;
					}
				}
				else if (mergedRegion.ColumnFrom == mergedRegion.ColumnTo)
				{
					num4 = mergedRegion.RowTo - mergedRegion.RowFrom;
				}
				return true;
			}
		}
		return false;
	}

	internal string GetDisplayText(int row, int column, FormatImpl formatImpl)
	{
		WorksheetImpl.TRangeValueType cellType = m_worksheet.GetCellType(row, column, bNeedFormulaSubType: false);
		string result = null;
		switch (cellType)
		{
		case WorksheetImpl.TRangeValueType.Blank:
			return string.Empty;
		case WorksheetImpl.TRangeValueType.String:
		{
			string text = m_worksheet.GetText(row, column);
			return formatImpl.ApplyFormat(text);
		}
		case WorksheetImpl.TRangeValueType.Number:
		{
			double number = m_worksheet.GetNumber(row, column);
			return GetNumberOrDateTime(formatImpl, number);
		}
		case WorksheetImpl.TRangeValueType.Boolean:
			result = m_worksheet.GetBoolean(row, column).ToString().ToUpper();
			result = formatImpl.ApplyFormat(result);
			if (result == null)
			{
				result = "";
			}
			return result;
		case WorksheetImpl.TRangeValueType.Error:
			return m_worksheet.GetError(row, column);
		case WorksheetImpl.TRangeValueType.Formula:
			switch ((int)m_worksheet.GetCellType(row, column, bNeedFormulaSubType: true))
			{
			case 8:
				return formatImpl.ApplyFormat(GetDisplayString());
			case 9:
				return m_worksheet.GetFormulaErrorValue(row, column);
			case 10:
				result = m_worksheet.GetFormulaBoolValue(row, column).ToString().ToUpper();
				result = formatImpl.ApplyFormat(result);
				if (result == null)
				{
					result = "";
				}
				return result;
			case 12:
			{
				double formulaNumberValue = m_worksheet.GetFormulaNumberValue(row, column);
				return GetNumberOrDateTime(formatImpl, formulaNumberValue);
			}
			case 24:
			{
				string formulaStringValue = m_worksheet.GetFormulaStringValue(row, column);
				return formatImpl.ApplyFormat(formulaStringValue);
			}
			}
			break;
		default:
			if (HasFormulaStringValue)
			{
				return FormulaStringValue;
			}
			break;
		}
		return result;
	}

	private string GetNumberOrDateTime(FormatImpl formatImpl, double dValue)
	{
		string text = string.Empty;
		OfficeFormatType formatType = formatImpl.GetFormatType(0.0);
		if (dValue == 0.0 && !m_worksheet.WindowTwo.IsDisplayZeros)
		{
			if (formatType == OfficeFormatType.Number || formatType == OfficeFormatType.General)
			{
				return text = GetDisplayString();
			}
			if (formatImpl.ApplyFormat(GetDisplayString()).Length == 0)
			{
				return text = string.Empty;
			}
		}
		switch (formatType)
		{
		case OfficeFormatType.General:
		case OfficeFormatType.Text:
		case OfficeFormatType.Number:
			if (!double.IsNaN(dValue) && text == string.Empty)
			{
				return text = (double.IsNaN(dValue) ? dValue.ToString() : formatImpl.ApplyFormat(dValue));
			}
			return text;
		case OfficeFormatType.DateTime:
			if (text == string.Empty)
			{
				if (m_book.Date1904)
				{
					dValue += 1462.0;
				}
				else if (dValue < 60.0 && m_worksheet.WindowTwo.IsDisplayZeros)
				{
					dValue += 1.0;
				}
				text = ((!(dValue > CultureInfo.CurrentCulture.DateTimeFormat.Calendar.MaxSupportedDateTime.ToOADate())) ? formatImpl.ApplyFormat(dValue) : "######");
			}
			if (m_hasDefaultFormat)
			{
				return GetCultureFormat(text, dValue, formatImpl);
			}
			return text;
		default:
			return formatImpl.ApplyFormat(dValue);
		}
	}

	public void Replace(string oldValue, string newValue)
	{
		CheckDisposed();
		if (IsSingleCell && Text == oldValue)
		{
			Text = newValue;
		}
	}

	public void Replace(string oldValue, double newValue)
	{
		CheckDisposed();
		if (IsSingleCell && Text == oldValue)
		{
			Number = newValue;
		}
	}

	public void Replace(string oldValue, DateTime newValue)
	{
		CheckDisposed();
		if (IsSingleCell && Text == oldValue)
		{
			DateTime = newValue;
		}
	}

	public void Replace(string oldValue, string[] newValues, bool isVertical)
	{
		CheckDisposed();
		if (IsSingleCell && Text == oldValue)
		{
			m_worksheet.ImportArray(newValues, Row, Column, isVertical);
		}
	}

	public void Replace(string oldValue, int[] newValues, bool isVertical)
	{
		CheckDisposed();
		if (IsSingleCell && Text == oldValue)
		{
			m_worksheet.ImportArray(newValues, Row, Column, isVertical);
		}
	}

	public void Replace(string oldValue, double[] newValues, bool isVertical)
	{
		CheckDisposed();
		if (IsSingleCell && Text == oldValue)
		{
			m_worksheet.ImportArray(newValues, Row, Column, isVertical);
		}
	}

	public IRange FindFirst(string findValue, OfficeFindType flags)
	{
		CheckDisposed();
		if (findValue == null || findValue.Length == 0)
		{
			return null;
		}
		bool flag = (flags & OfficeFindType.Formula) == OfficeFindType.Formula;
		bool flag2 = (flags & OfficeFindType.Text) == OfficeFindType.Text;
		bool flag3 = (flags & OfficeFindType.FormulaStringValue) == OfficeFindType.FormulaStringValue;
		bool flag4 = (flags & OfficeFindType.Error) == OfficeFindType.Error;
		if (!(flag || flag2 || flag3 || flag4))
		{
			throw new ArgumentException("Parameter flag is not valid.", "flags");
		}
		if (IsSingleCell)
		{
			if (flag4 && IsError && Error == findValue)
			{
				return this;
			}
			if (flag && HasFormula && Formula == findValue)
			{
				return this;
			}
			if (flag3 && FormulaStringValue != null && FormulaStringValue == findValue)
			{
				return this;
			}
			if (flag2 && HasString && Text == findValue)
			{
				return this;
			}
			return null;
		}
		IRange[] array = m_worksheet.Find(this, findValue, flags, bIsFindFirst: true);
		if (array == null)
		{
			return null;
		}
		return array[0];
	}

	public IRange FindFirst(double findValue, OfficeFindType flags)
	{
		CheckDisposed();
		bool flag = (flags & OfficeFindType.FormulaValue) == OfficeFindType.FormulaValue;
		bool flag2 = (flags & OfficeFindType.Number) == OfficeFindType.Number;
		if (!(flag || flag2))
		{
			throw new ArgumentException("Parameter flag is not valid.", "flags");
		}
		if (IsSingleCell)
		{
			if (flag2 && HasNumber && Number == findValue)
			{
				return this;
			}
			if (flag && HasFormula && FormulaNumberValue == findValue)
			{
				return this;
			}
			return null;
		}
		IRange[] array = m_worksheet.Find(this, findValue, flags, bIsFindFirst: true);
		if (array == null)
		{
			return null;
		}
		return array[0];
	}

	public IRange FindFirst(bool findValue)
	{
		CheckDisposed();
		if (IsSingleCell)
		{
			if (!IsBoolean || Boolean != findValue)
			{
				return null;
			}
			return this;
		}
		byte findValue2 = (findValue ? ((byte)1) : ((byte)0));
		IRange[] array = m_worksheet.Find(this, findValue2, bIsError: false, bIsFindFirst: true);
		if (array == null)
		{
			return null;
		}
		return array[0];
	}

	public IRange FindFirst(DateTime findValue)
	{
		CheckDisposed();
		if (IsSingleCell)
		{
			if (HasDateTime && DateTime == findValue)
			{
				return this;
			}
			return null;
		}
		double findValue2 = findValue.ToOADate();
		return FindFirst(findValue2, OfficeFindType.Number | OfficeFindType.FormulaValue);
	}

	public IRange FindFirst(TimeSpan findValue)
	{
		CheckDisposed();
		if (IsSingleCell)
		{
			if (HasDateTime && TimeSpan == findValue)
			{
				return this;
			}
			return null;
		}
		double totalDays = findValue.TotalDays;
		return FindFirst(totalDays, OfficeFindType.Number | OfficeFindType.FormulaValue);
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags)
	{
		CheckDisposed();
		if (findValue == null || findValue.Length == 0)
		{
			return null;
		}
		bool flag = (flags & OfficeFindType.Formula) == OfficeFindType.Formula;
		bool flag2 = (flags & OfficeFindType.Text) == OfficeFindType.Text;
		bool flag3 = (flags & OfficeFindType.FormulaStringValue) == OfficeFindType.FormulaStringValue;
		bool flag4 = (flags & OfficeFindType.Error) == OfficeFindType.Error;
		if (!(flag || flag2 || flag3 || flag4))
		{
			throw new ArgumentException("Parameter flag is not valid.", "flags");
		}
		if (IsSingleCell)
		{
			if ((flag4 && IsError && Error == findValue) || (flag && HasFormula && Formula == findValue) || (flag3 && FormulaStringValue != null && FormulaStringValue == findValue) || (flag2 && HasString && Text == findValue))
			{
				return new IRange[1] { this };
			}
			return null;
		}
		return m_worksheet.Find(this, findValue, flags, bIsFindFirst: false);
	}

	public IRange[] FindAll(double findValue, OfficeFindType flags)
	{
		CheckDisposed();
		bool flag = (flags & OfficeFindType.FormulaValue) == OfficeFindType.FormulaValue;
		bool flag2 = (flags & OfficeFindType.Number) == OfficeFindType.Number;
		if (!(flag || flag2))
		{
			throw new ArgumentException("Parameter flag is not valid.", "flags");
		}
		if (IsSingleCell)
		{
			if ((flag2 && HasNumber && Number == findValue) || (flag && HasFormula && FormulaNumberValue == findValue))
			{
				return new IRange[1] { this };
			}
			return null;
		}
		return m_worksheet.Find(this, findValue, flags, bIsFindFirst: false);
	}

	public IRange[] FindAll(bool findValue)
	{
		CheckDisposed();
		if (IsSingleCell)
		{
			if (!IsBoolean || Boolean != findValue)
			{
				return null;
			}
			return new IRange[1] { this };
		}
		byte findValue2 = (findValue ? ((byte)1) : ((byte)0));
		return m_worksheet.Find(this, findValue2, bIsError: false, bIsFindFirst: false);
	}

	public IRange[] FindAll(DateTime findValue)
	{
		CheckDisposed();
		List<IRange> list = new List<IRange>();
		if (IsSingleCell)
		{
			if (HasDateTime && DateTime == findValue)
			{
				list.Add(this);
			}
			if (list.Count == 0)
			{
				return null;
			}
			return list.ToArray();
		}
		double findValue2 = findValue.ToOADate();
		return FindAll(findValue2, OfficeFindType.Number | OfficeFindType.FormulaValue);
	}

	public IRange[] FindAll(TimeSpan findValue)
	{
		CheckDisposed();
		List<IRange> list = new List<IRange>();
		if (IsSingleCell)
		{
			if (HasDateTime && TimeSpan == findValue)
			{
				list.Add(this);
			}
			if (list.Count == 0)
			{
				return null;
			}
			return list.ToArray();
		}
		double totalDays = findValue.TotalDays;
		return FindAll(totalDays, OfficeFindType.Number | OfficeFindType.FormulaValue);
	}

	public void BorderAround()
	{
		BorderAround(OfficeLineStyle.Thin);
	}

	public void BorderAround(OfficeLineStyle borderLine)
	{
		BorderAround(borderLine, OfficeKnownColors.Black);
	}

	public void BorderAround(OfficeLineStyle borderLine, Color borderColor)
	{
		OfficeKnownColors nearestColor = m_book.GetNearestColor(borderColor);
		BorderAround(borderLine, nearestColor);
	}

	public void BorderAround(OfficeLineStyle borderLine, OfficeKnownColors borderColor)
	{
		if (IsSingleCell)
		{
			SetBorderToSingleCell(OfficeBordersIndex.EdgeLeft, borderLine, borderColor);
			SetBorderToSingleCell(OfficeBordersIndex.EdgeRight, borderLine, borderColor);
			SetBorderToSingleCell(OfficeBordersIndex.EdgeTop, borderLine, borderColor);
			SetBorderToSingleCell(OfficeBordersIndex.EdgeBottom, borderLine, borderColor);
			return;
		}
		int column = Column;
		int lastColumn = LastColumn;
		int row = Row;
		int lastRow = LastRow;
		MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
		for (int i = column; i <= lastColumn; i++)
		{
			migrantRangeImpl.ResetRowColumn(row, i);
			migrantRangeImpl.SetBorderToSingleCell(OfficeBordersIndex.EdgeTop, borderLine, borderColor);
			migrantRangeImpl.ResetRowColumn(lastRow, i);
			migrantRangeImpl.SetBorderToSingleCell(OfficeBordersIndex.EdgeBottom, borderLine, borderColor);
		}
		for (int j = row; j <= lastRow; j++)
		{
			migrantRangeImpl.ResetRowColumn(j, column);
			migrantRangeImpl.SetBorderToSingleCell(OfficeBordersIndex.EdgeLeft, borderLine, borderColor);
			migrantRangeImpl.ResetRowColumn(j, lastColumn);
			migrantRangeImpl.SetBorderToSingleCell(OfficeBordersIndex.EdgeRight, borderLine, borderColor);
		}
	}

	public void BorderInside()
	{
		BorderInside(OfficeLineStyle.Thin);
	}

	public void BorderInside(OfficeLineStyle borderLine)
	{
		BorderInside(borderLine, OfficeKnownColors.Black);
	}

	public void BorderInside(OfficeLineStyle borderLine, Color borderColor)
	{
		OfficeKnownColors nearestColor = m_book.GetNearestColor(borderColor);
		BorderInside(borderLine, nearestColor);
	}

	public void BorderInside(OfficeLineStyle borderLine, OfficeKnownColors borderColor)
	{
		if (IsSingleCell)
		{
			throw new NotSupportedException("This method doesn't support for single cell.");
		}
		int column = Column;
		int lastColumn = LastColumn;
		int row = Row;
		int lastRow = LastRow;
		MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
		for (int i = column; i <= lastColumn; i++)
		{
			for (int j = row; j <= lastRow; j++)
			{
				if (i != LastColumn)
				{
					migrantRangeImpl.ResetRowColumn(j, i);
					migrantRangeImpl.SetBorderToSingleCell(OfficeBordersIndex.EdgeRight, borderLine, borderColor);
				}
				if (j != LastRow)
				{
					migrantRangeImpl.ResetRowColumn(j, i);
					migrantRangeImpl.SetBorderToSingleCell(OfficeBordersIndex.EdgeBottom, borderLine, borderColor);
				}
			}
		}
	}

	public void BorderNone()
	{
		int i = FirstColumn;
		for (int lastColumn = LastColumn; i <= lastColumn; i++)
		{
			int j = FirstRow;
			for (int lastRow = LastRow; j <= lastRow; j++)
			{
				MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(Application, m_worksheet);
				if (i == FirstColumn && i > 0)
				{
					migrantRangeImpl.ResetRowColumn(j, i - 1);
					migrantRangeImpl.Borders[OfficeBordersIndex.EdgeRight].LineStyle = OfficeLineStyle.None;
				}
				if (i == LastColumn && i < Workbook.MaxColumnCount)
				{
					migrantRangeImpl.ResetRowColumn(j, i + 1);
					migrantRangeImpl.Borders[OfficeBordersIndex.EdgeLeft].LineStyle = OfficeLineStyle.None;
				}
				if (j == FirstRow && j > 0)
				{
					migrantRangeImpl.ResetRowColumn(j - 1, i);
					migrantRangeImpl.Borders[OfficeBordersIndex.EdgeBottom].LineStyle = OfficeLineStyle.None;
				}
				if (j == LastRow && j < Workbook.MaxRowCount)
				{
					migrantRangeImpl.ResetRowColumn(j + 1, i);
					migrantRangeImpl.Borders[OfficeBordersIndex.EdgeTop].LineStyle = OfficeLineStyle.None;
				}
				migrantRangeImpl.ResetRowColumn(j, i);
				migrantRangeImpl.Borders.LineStyle = OfficeLineStyle.None;
			}
		}
	}

	public void SetAutoFormat(ExcelAutoFormat format)
	{
		SetAutoFormat(format, ExcelAutoFormatOptions.All);
	}

	public void SetAutoFormat(ExcelAutoFormat format, ExcelAutoFormatOptions options)
	{
		if (IsSingleCell)
		{
			throw new NotSupportedException("Auto format doesn't suport in single cell.");
		}
		if (options != 0)
		{
			bool num = (options & ExcelAutoFormatOptions.Patterns) == ExcelAutoFormatOptions.Patterns;
			bool flag = (options & ExcelAutoFormatOptions.Alignment) == ExcelAutoFormatOptions.Alignment;
			bool flag2 = (options & ExcelAutoFormatOptions.Width_Height) == ExcelAutoFormatOptions.Width_Height;
			bool flag3 = (options & ExcelAutoFormatOptions.Number) == ExcelAutoFormatOptions.Number;
			bool bIsFont = (options & ExcelAutoFormatOptions.Font) == ExcelAutoFormatOptions.Font;
			bool bIsBorder = (options & ExcelAutoFormatOptions.Border) == ExcelAutoFormatOptions.Border;
			if (num)
			{
				SetAutoFormatPatterns(format);
			}
			if (flag)
			{
				SetAutoFormatAlignments(format);
			}
			if (flag2)
			{
				SetAutoFormatWidthHeight(format);
			}
			if (flag3)
			{
				SetAutoFormatNumbers(format);
			}
			SetAutoFormatFontBorder(format, bIsFont, bIsBorder);
		}
	}

	private void SetSingleCellValue2(object value)
	{
		bool isPreserveFormat = true;
		if (value != null)
		{
			bool? flag = IsStringsPreserved;
			if (!flag.HasValue)
			{
				flag = m_worksheet.IsStringsPreserved;
			}
			if (flag == false)
			{
				if (value is DateTime && (DateTime)value >= DEF_MIN_DATETIME)
				{
					DateTime dateTime = (DateTime)value;
					DateTime = dateTime;
				}
				else if (value is TimeSpan timeSpan)
				{
					TimeSpan = timeSpan;
				}
				else if (value is double)
				{
					Number = (double)value;
				}
				else if (value is int)
				{
					SetNumberAndFormat((int)value, isPreserveFormat);
				}
				else
				{
					Value = value.ToString();
				}
			}
			else
			{
				Value = value.ToString();
			}
		}
		else
		{
			Text = "";
		}
	}

	public void CollapseGroup(OfficeGroupBy groupBy)
	{
		CollapseExpand(groupBy, isCollapsed: true, ExpandCollapseFlags.Default);
	}

	public void ExpandGroup(OfficeGroupBy groupBy)
	{
		ExpandGroup(groupBy, ExpandCollapseFlags.Default);
	}

	public void ExpandGroup(OfficeGroupBy groupBy, ExpandCollapseFlags flags)
	{
		CollapseExpand(groupBy, isCollapsed: false, flags);
	}

	public string GetNewAddress(Dictionary<string, string> names, out string strSheetName)
	{
		strSheetName = m_worksheet.Name;
		if (names == null || !names.ContainsKey(strSheetName))
		{
			return Address;
		}
		strSheetName = names[strSheetName];
		return "'" + strSheetName.Replace("'", "''") + "'!" + AddressLocal;
	}

	public IRange Clone(object parent, Dictionary<string, string> hashNewNames, WorkbookImpl book)
	{
		string text = m_worksheet.Name;
		if (hashNewNames != null && hashNewNames.ContainsKey(text))
		{
			text = hashNewNames[text];
		}
		WorksheetImpl worksheetImpl = (WorksheetImpl)book.Worksheets[text];
		IRange range = null;
		if (worksheetImpl != null)
		{
			return worksheetImpl.Range[FirstRow, FirstColumn, LastRow, LastColumn];
		}
		return m_worksheet.Range[FirstRow, FirstColumn, LastRow, LastColumn];
	}

	public void ClearConditionalFormats()
	{
	}

	public void ClearDataValidations()
	{
	}

	public Rectangle[] GetRectangles()
	{
		return new Rectangle[1] { Rectangle.FromLTRB(FirstColumn - 1, FirstRow - 1, LastColumn - 1, LastRow - 1) };
	}

	public int GetRectanglesCount()
	{
		return 1;
	}

	public static string GetR1C1AddresFromCellIndex(long cellIndex)
	{
		int rowFromCellIndex = GetRowFromCellIndex(cellIndex);
		int columnFromCellIndex = GetColumnFromCellIndex(cellIndex);
		return GetAddressLocal(rowFromCellIndex, columnFromCellIndex, rowFromCellIndex, columnFromCellIndex, bR1C1: true);
	}

	public static long CellNameToIndex(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length < 2)
		{
			throw new ArgumentException("name cannot be less then 2 symbols");
		}
		int iRow = 0;
		int iColumn = 0;
		CellNameToRowColumn(name, out iRow, out iColumn);
		return GetCellIndex(iColumn, iRow);
	}

	public static void CellNameToRowColumn(string name, out int iRow, out int iColumn)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length < 2)
		{
			throw new ArgumentException("name cannot be less then 2 symbols");
		}
		int num = -1;
		int num2 = 0;
		int num3 = -1;
		int num4 = 0;
		int i = 0;
		for (int length = name.Length; i < length; i++)
		{
			char c = name[i];
			if (char.IsDigit(c))
			{
				if (num3 < 0)
				{
					num3 = i;
				}
				num4++;
			}
			else if (char.IsLetter(c))
			{
				if (num < 0)
				{
					num = i;
				}
				num2++;
			}
			else if (!char.IsPunctuation(c) && !char.IsWhiteSpace(c) && c != '$')
			{
				throw new ArgumentOutOfRangeException("name", "Character " + c + " was not expected.");
			}
		}
		if (num3 == -1)
		{
			num3 = 1;
			num4++;
		}
		if (num == -1)
		{
			num = 0;
			num2++;
		}
		string text = name.Substring(num3, num4);
		string columnName = name.Substring(num, num2);
		if (char.IsLetter(text[0]))
		{
			text = ((int)text[0]).ToString();
		}
		iRow = int.Parse(text, NumberStyles.None, NumberFormatInfo.InvariantInfo);
		iColumn = GetColumnIndex(columnName);
	}

	public static int GetColumnIndex(string columnName)
	{
		int num = 0;
		int i = 0;
		for (int length = columnName.Length; i < length; i++)
		{
			char c = columnName[i];
			num *= 26;
			num += 1 + ((c >= 'a') ? (c - 97) : (c - 65));
		}
		if (num < 0)
		{
			num = -num;
		}
		return num;
	}

	public static string GetColumnName(int iColumn)
	{
		if (iColumn < 1)
		{
			throw new ArgumentOutOfRangeException("iColumn", "Value cannot be less than 1.");
		}
		iColumn--;
		string text = string.Empty;
		do
		{
			int num = iColumn % 26;
			iColumn = iColumn / 26 - 1;
			text = Convert.ToChar(65 + num) + text;
		}
		while (iColumn >= 0);
		return text;
	}

	public static string GetCellName(int firstColumn, int firstRow)
	{
		return GetCellName(firstColumn, firstRow, bR1C1: false);
	}

	public static string GetCellName(int firstColumn, int firstRow, bool bR1C1)
	{
		return GetCellName(firstColumn, firstRow, bR1C1, bUseSeparater: false);
	}

	public static string GetCellName(int firstColumn, int firstRow, bool bR1C1, bool bUseSeparater)
	{
		if (firstRow < 1)
		{
			throw new ArgumentOutOfRangeException("Row index is wrong. It cannot be less then 1");
		}
		if (bR1C1)
		{
			return $"R{firstRow}C{firstColumn}";
		}
		if (!bUseSeparater)
		{
			return GetColumnName(firstColumn) + firstRow;
		}
		return '$' + GetColumnName(firstColumn) + '$' + firstRow;
	}

	public static string GetAddressLocal(int iFirstRow, int iFirstColumn, int iLastRow, int iLastColumn)
	{
		string cellName = GetCellName(iFirstColumn, iFirstRow);
		if (iFirstRow == iLastRow && iFirstColumn == iLastColumn)
		{
			return cellName;
		}
		string cellName2 = GetCellName(iLastColumn, iLastRow);
		return cellName + ":" + cellName2;
	}

	public static string GetAddressLocal(int iFirstRow, int iFirstColumn, int iLastRow, int iLastColumn, bool bR1C1)
	{
		string cellName = GetCellName(iFirstColumn, iFirstRow, bR1C1);
		if (iFirstRow == iLastRow && iFirstColumn == iLastColumn)
		{
			return cellName;
		}
		string cellName2 = GetCellName(iLastColumn, iLastRow, bR1C1);
		return cellName + ":" + cellName2;
	}

	public static string GetCellNameWithDollars(int firstColumn, int firstRow)
	{
		if (firstColumn < 1 || firstRow < 1)
		{
			throw new ArgumentOutOfRangeException("column or row index is wrong. It cannot be less then 1");
		}
		string columnName = GetColumnName(firstColumn);
		return "$" + columnName + "$" + firstRow;
	}

	public static long GetCellIndex(int firstColumn, int firstRow)
	{
		if (firstColumn == -1 || firstRow == -1)
		{
			return -1L;
		}
		if (firstRow < 0 || firstColumn < 0)
		{
			throw new ArgumentOutOfRangeException("wrong row or column index");
		}
		return ((long)firstRow << 32) + firstColumn;
	}

	[DebuggerStepThrough]
	public static int GetRowFromCellIndex(long index)
	{
		return (int)(index >>> 32);
	}

	[DebuggerStepThrough]
	public static int GetColumnFromCellIndex(long index)
	{
		return (int)(index & 0xFFFFFFFFu);
	}

	public static string GetWorksheetName(ref string rangeName)
	{
		if (rangeName == null)
		{
			throw new ArgumentNullException("rangeName");
		}
		if (rangeName.Length == 0)
		{
			throw new ArgumentException("rangeName - string cannot be empty");
		}
		int num = 0;
		string text = rangeName;
		int num2 = 0;
		int num3 = 0;
		string text2 = null;
		string text3 = null;
		char[] array = new char[rangeName.Length];
		array = rangeName.ToCharArray();
		for (int i = 0; i < array.Length - 1; i++)
		{
			if (array[i] == '\'' && array[i + 1] != '\'' && num3 == 0)
			{
				num = i;
				num3++;
			}
			if (array[i] == '!' && i > 2 && array[i - 1] == '\'' && array[i - 2] != '\'')
			{
				num2 = i;
				break;
			}
		}
		int num4 = rangeName.IndexOf('!');
		if (num4 != -1)
		{
			text2 = rangeName.Substring(num, num2 - num).Replace("''", "'");
			text = rangeName.Substring(num4 + 1, rangeName.Length - num4 - 1);
			if (text2 == "")
			{
				text3 = rangeName.Substring(0, num4);
				if (text3.Contains("("))
				{
					text3 = text3.Substring(text3.IndexOf('(') + 1);
				}
				text2 = text3;
				if (!text.Contains(text2))
				{
					rangeName = rangeName.Substring(num4 + 1, rangeName.Length - num4 - 1);
				}
			}
			else if (!text.Contains(text2))
			{
				rangeName = rangeName.Substring(num2 + 1, rangeName.Length - num2 - 1);
			}
			if (num2 != 0)
			{
				int length = text2.Length;
				if (text2[0] == '\'' && text2[length - 1] == '\'')
				{
					text2 = text2.Substring(1, length - 2);
				}
			}
		}
		return text2;
	}

	public static bool GetWrapText(IList rangeColection)
	{
		if (rangeColection == null)
		{
			throw new ArgumentNullException("rangeColection");
		}
		bool flag = true;
		int count = rangeColection.Count;
		int num = 0;
		while (flag && num < count)
		{
			if (!(rangeColection[num] as IRange).WrapText)
			{
				flag = false;
				break;
			}
			num++;
		}
		return flag;
	}

	public static void SetWrapText(IList rangeColection, bool wrapText)
	{
		int i = 0;
		for (int count = rangeColection.Count; i < count; i++)
		{
			((IRange)rangeColection[i]).WrapText = wrapText;
		}
	}

	public static string GetNumberFormat(IList rangeColection)
	{
		int count = rangeColection.Count;
		if (count == 0)
		{
			return null;
		}
		IRange range = (IRange)rangeColection[0];
		string numberFormat = range.NumberFormat;
		for (int i = 1; i < count; i++)
		{
			range = (IRange)rangeColection[i];
			if (numberFormat != range.NumberFormat)
			{
				return null;
			}
		}
		return numberFormat;
	}

	public static string GetCellStyleName(IList<IRange> rangeColection)
	{
		int count = rangeColection.Count;
		if (count == 0)
		{
			return null;
		}
		IRange range = rangeColection[0];
		string cellStyleName = range.CellStyleName;
		for (int i = 1; i < count; i++)
		{
			range = rangeColection[i];
			if (cellStyleName != range.CellStyleName)
			{
				return null;
			}
		}
		return cellStyleName;
	}

	public static int ParseRangeString(string range, IWorkbook book, out int iFirstRow, out int iFirstColumn, out int iLastRow, out int iLastColumn)
	{
		iLastColumn = (iLastRow = (iFirstColumn = (iFirstRow = -1)));
		string[] array = range.Split(':');
		int num = array.Length;
		Match match = FormulaUtil.FullRowRangeRegex.Match(range);
		if (match.Success && match.Index == 0 && match.Length == range.Length)
		{
			iFirstColumn = 1;
			iLastColumn = book.MaxColumnCount;
			string value = UtilityMethods.RemoveFirstCharUnsafe(match.Groups["Row1"].Value);
			string value2 = UtilityMethods.RemoveFirstCharUnsafe(match.Groups["Row2"].Value);
			iFirstRow = Convert.ToInt32(value);
			iLastRow = Convert.ToInt32(value2);
			return num;
		}
		match = FormulaUtil.FullColumnRangeRegex.Match(range);
		if (match.Success && match.Index == 0 && match.Length == range.Length)
		{
			string columnName = UtilityMethods.RemoveFirstCharUnsafe(match.Groups["Column1"].Value);
			string columnName2 = UtilityMethods.RemoveFirstCharUnsafe(match.Groups["Column2"].Value);
			iFirstColumn = GetColumnIndex(columnName);
			iLastColumn = GetColumnIndex(columnName2);
			iFirstRow = 1;
			iLastRow = book.MaxRowCount;
			return num;
		}
		long num2 = -1L;
		if (num >= 1)
		{
			num2 = CellNameToIndex(array[0]);
			iLastRow = (iFirstRow = GetRowFromCellIndex(num2));
			iLastColumn = (iFirstColumn = GetColumnFromCellIndex(num2));
		}
		if (num == 2)
		{
			long num3 = CellNameToIndex(array[1]);
			if (num2 != num3)
			{
				iLastRow = GetRowFromCellIndex(num3);
				iLastColumn = GetColumnFromCellIndex(num3);
			}
		}
		else if (num > 2)
		{
			throw new ArgumentException();
		}
		return num;
	}

	public static Rectangle GetRectangeOfRange(IRange range, bool bThrowExcONNullRange)
	{
		Rectangle result = new Rectangle(-1, -1, -1, -1);
		if (range == null)
		{
			if (bThrowExcONNullRange)
			{
				throw new ArgumentNullException("range");
			}
			return result;
		}
		result.Y = range.Row;
		result.Height = range.LastRow - result.Y;
		result.X = range.Column;
		result.Width = range.LastColumn - result.X;
		return result;
	}

	protected internal void wrapStyle_OnNumberFormatChanged(object sender, EventArgs e)
	{
		TCellType cellType = CellType;
		_ = Value;
		OnStyleChanged(cellType);
	}

	private void AttachEventToStyle()
	{
		int extendedFormatIndex = ExtendedFormatIndex;
		extendedFormatIndex = m_book.InnerExtFormats[extendedFormatIndex].ParentIndex;
		StyleImpl byXFIndex = m_book.InnerStyles.GetByXFIndex(extendedFormatIndex);
		AttachEvent(byXFIndex, wrapStyle_OnNumberFormatChanged);
	}

	private void AttachEventToCellStyle()
	{
		AttachEvent(m_style, wrapStyle_OnNumberFormatChanged);
	}

	private void AttachEvent(ExtendedFormatWrapper wrapper, EventHandler handler)
	{
		wrapper.NumberFormatChanged += handler;
	}

	protected void CreateStyle()
	{
		int value = m_book.DefaultXFIndex;
		BiffRecordRaw record = Record;
		if (record != null)
		{
			value = ((ICellPositionFormat)record).ExtendedFormatIndex;
		}
		CreateStyleWrapper(value);
	}

	protected void CreateStyleWrapper(int value)
	{
		if (!IsSingleCell && !IsEntireRow && !IsEntireColumn)
		{
			throw new ArgumentException("This method can be used only for single cell not a range");
		}
		_ = m_style;
		m_style = new CellStyle(this, value);
	}

	internal static IStyle CreateTempStyleWrapperWithoutRange(RangeImpl rangeImpl, int value)
	{
		return new CellStyle(rangeImpl, value);
	}

	public void SetXFormatIndex(int index)
	{
		if (!IsSingleCell && !IsEntireRow && !IsEntireColumn)
		{
			throw new ApplicationException("This method should be called for single range cells only");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Value cannot be less than 0");
		}
		ExtendedFormatImpl extendedFormatImpl = m_book.InnerExtFormats[index];
		extendedFormatImpl = ((extendedFormatImpl.Record.XFType != 0) ? extendedFormatImpl.CreateChildFormat() : extendedFormatImpl.CreateChildFormat(m_book.InnerExtFormats[ExtendedFormatIndex]));
		index = extendedFormatImpl.Index;
		if (IsEntireRow)
		{
			int row = Row;
			int lastRow = LastRow;
			for (int i = row; i <= lastRow; i++)
			{
				m_worksheet.CellRecords.SetCellStyle(i, index);
			}
		}
		else if (IsEntireColumn)
		{
			int firstColumn = FirstColumn;
			int lastColumn = LastColumn;
			for (int j = firstColumn; j <= lastColumn; j++)
			{
				ColumnInfoRecord columnInfoRecord = (ColumnInfoRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ColumnInfo);
				columnInfoRecord.FirstColumn = (ushort)(firstColumn - 1);
				columnInfoRecord.LastColumn = (ushort)(LastColumn - 1);
				columnInfoRecord.ExtendedFormatIndex = (ushort)index;
				m_worksheet.ColumnInformation[j] = columnInfoRecord;
			}
		}
		else
		{
			m_worksheet.CellRecords.SetCellStyle(Row, Column, index);
		}
		if (m_style != null)
		{
			m_style.SetFormatIndex(index);
		}
		if (Array.IndexOf(ThinBorders, extendedFormatImpl.BottomBorderLineStyle) < 0)
		{
			RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(m_worksheet, Row - 1, bCreate: false);
			if (orCreateRow != null)
			{
				orCreateRow.IsSpaceBelowRow = true;
			}
		}
	}

	private StyleImpl ChangeStyleName(string strNewName)
	{
		int num = m_book.DefaultXFIndex;
		StyleImpl styleImpl = null;
		if (strNewName != null && strNewName.Length > 0)
		{
			if (!m_book.InnerStyles.ContainsName(strNewName) && m_book.Version != 0)
			{
				Array.IndexOf(DefaultStyleNames, strNewName);
				styleImpl = m_book.InnerStyles.CreateBuiltInStyle(strNewName);
			}
			else
			{
				styleImpl = (StyleImpl)m_book.Styles[strNewName];
			}
			if (styleImpl != null)
			{
				if (ExtendedFormat.IsFirstSymbolApostrophe)
				{
					ExtendedFormatImpl obj = Workbook.CreateExtFormat(bForceAdd: true) as ExtendedFormatImpl;
					obj.ParentIndex = styleImpl.Index;
					obj.IsFirstSymbolApostrophe = ExtendedFormat.IsFirstSymbolApostrophe;
					num = obj.Index;
				}
				else
				{
					num = styleImpl.Index;
				}
			}
		}
		else
		{
			styleImpl = (StyleImpl)m_book.Styles[num];
		}
		ExtendedFormatIndex = (ushort)num;
		return styleImpl;
	}

	private string GetStyleName()
	{
		if (m_style != null)
		{
			return m_style.Name;
		}
		int extendedFormatIndex = ExtendedFormatIndex;
		StyleImpl styleImpl = m_book.InnerStyles.GetByXFIndex(extendedFormatIndex);
		if (styleImpl == null)
		{
			ExtendedFormatImpl extendedFormatImpl = m_book.InnerExtFormats[extendedFormatIndex];
			styleImpl = m_book.InnerStyles.GetByXFIndex(extendedFormatImpl.ParentIndex);
			if (styleImpl == null)
			{
				styleImpl = m_book.InnerStyles["Normal"] as StyleImpl;
				ExtendedFormatIndex = (ushort)styleImpl.Index;
			}
		}
		return styleImpl.Name;
	}

	private bool GetWrapText()
	{
		if (m_style != null)
		{
			return m_style.WrapText;
		}
		return ExtendedFormat.WrapText;
	}

	private string GetNumberFormat()
	{
		if (m_style != null)
		{
			return m_style.NumberFormat;
		}
		return ExtendedFormat.NumberFormat;
	}

	internal bool TryGetDateTimeByCulture(string strDateTime, bool isUKCulture, out DateTime dtValue)
	{
		if (strDateTime == null)
		{
			throw new ArgumentNullException("strDateTime");
		}
		_ = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
		dtValue = DateTime.Now;
		if (ExtendedFormat.NumberFormatIndex == 14)
		{
			if (DateTime.TryParse(strDateTime, CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault, out dtValue))
			{
				return true;
			}
			if (DateTime.TryParse(strDateTime, new CultureInfo("cy-GB"), DateTimeStyles.None, out dtValue))
			{
				return true;
			}
		}
		else if (isUKCulture && DateTime.TryParse(strDateTime, new CultureInfo("cy-GB"), DateTimeStyles.None, out dtValue))
		{
			return true;
		}
		return false;
	}

	public void UpdateRecord()
	{
		if (m_style != null)
		{
			ICellPositionFormat cellPositionFormat = (ICellPositionFormat)Record;
			m_style.SetFormatIndex(cellPositionFormat.ExtendedFormatIndex);
		}
	}

	public bool GetAreArrayFormulasNotSeparated(Dictionary<ArrayRecord, object> hashToSkip)
	{
		if (hashToSkip == null)
		{
			hashToSkip = new Dictionary<ArrayRecord, object>();
		}
		CellRecordCollection cellRecords = m_worksheet.CellRecords;
		int i = FirstRow;
		for (int lastRow = LastRow; i <= lastRow; i++)
		{
			int num = FirstColumn;
			int lastColumn = LastColumn;
			while (num <= lastColumn)
			{
				num = cellRecords.FindRecord(TBIFFRecord.Formula, i, num, lastColumn);
				if (num <= lastColumn)
				{
					ArrayRecord arrayRecord = cellRecords.GetArrayRecord(i, num);
					if (arrayRecord != null && !hashToSkip.ContainsKey(arrayRecord))
					{
						if (arrayRecord.FirstRow + 1 < FirstRow || arrayRecord.LastRow + 1 > lastRow || arrayRecord.FirstColumn + 1 < FirstColumn || arrayRecord.LastColumn + 1 > lastColumn)
						{
							return false;
						}
						hashToSkip.Add(arrayRecord, null);
					}
				}
				num++;
			}
		}
		return true;
	}

	private string GetCustomizedFormat(string format)
	{
		string text = format.ToLower();
		string[] array = new string[4] { ",", " ", ".", "-" };
		foreach (string text2 in array)
		{
			if (text.Contains(text2))
			{
				string newValue = text2.PadLeft(2, '\\');
				text = text.Replace(text2, newValue);
			}
		}
		return text;
	}

	public void Reparse()
	{
		TCellType cellType = CellType;
		if (cellType == TCellType.Formula)
		{
			ReParseFormula((FormulaRecord)Record);
		}
		else
		{
			_ = 253;
		}
	}

	public Ptg[] GetNativePtg()
	{
		int num = m_book.AddSheetReference(m_worksheet.Name);
		Ptg ptg = ((!IsSingleCell) ? FormulaUtil.CreatePtg(FormulaToken.tArea3d1, num, FirstRow - 1, FirstColumn - 1, LastRow - 1, LastColumn - 1, (byte)0, (byte)0) : FormulaUtil.CreatePtg(FormulaToken.tRef3d1, num, FirstRow - 1, FirstColumn - 1, (byte)0));
		return new Ptg[1] { ptg };
	}

	public IEnumerator<IRange> GetEnumerator()
	{
		return ((IEnumerable<IRange>)CellsList).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)CellsList).GetEnumerator();
	}

	internal void OnCellValueChanged(object oldValue, object newValue, IRange range)
	{
		(Worksheet as WorksheetImpl).OnCellValueChanged(oldValue, newValue, range);
	}
}
