using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Calculate;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation;

internal class NameImpl : CommonObject, IName, IParentApplication, INameIndexChangedEventProvider, IParseable, IRange, IEnumerable, INativePTG, ICloneParent, ICombinedRange, IDisposable
{
	internal delegate void NameIndexChangedEventHandler(object sender, NameIndexChangedEventArgs data);

	private const string DEF_SHEETNAME_SEPARATER = "!";

	private const string DEF_RANGE_FORMAT = "{2}!{0}:{1}";

	public const int DEF_NAME_SHEET_INDEX = 65534;

	private static readonly char[] DEF_VALID_SYMBOL = new char[6] { '_', '?', '\\', '№', '.', '#' };

	private const string WorkbookScope = "Workbook";

	private NameRecord m_name;

	private WorkbookImpl m_book;

	private WorksheetImpl m_worksheet;

	private int m_index = -1;

	private bool m_bIsDeleted;

	private bool m_bIsNumReference;

	private bool m_bIsMultiReference;

	private bool m_bIsStringReference;

	private bool m_isQueryRange;

	private int m_sheetindex;

	private bool m_isCommon;

	internal bool m_isFormulaNamedRange;

	internal string CalculatedValue
	{
		get
		{
			if (base.Parent is WorksheetImpl && ((WorksheetImpl)base.Parent).CalcEngine != null)
			{
				string cellRef = RangeInfo.GetAlphaLabel(Column) + Row;
				return ((WorksheetImpl)base.Parent).CalcEngine.PullUpdatedValue(cellRef);
			}
			return null;
		}
	}

	internal bool IsDeleted
	{
		get
		{
			return m_bIsDeleted;
		}
		set
		{
			m_bIsDeleted = value;
		}
	}

	public int Index => m_index;

	int IName.Index
	{
		get
		{
			INames names = ((m_worksheet == null) ? m_book.Names : m_worksheet.Names);
			int num = 0;
			for (int i = 0; i < names.Count; i++)
			{
				NameImpl nameImpl = names[i] as NameImpl;
				if (!nameImpl.IsDeleted)
				{
					if (nameImpl.Name == Name)
					{
						return num;
					}
					num++;
				}
			}
			return -1;
		}
	}

	public string Name
	{
		get
		{
			return m_name.Name;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value != m_name.Name)
			{
				string name = m_name.Name;
				m_name.IsBuinldInName = NameRecord.IsPredefinedName(value);
				m_name.Name = value;
				m_book.InnerNamesColection.IsWorkbookNamesChanged = true;
				if (m_worksheet != null)
				{
					Worksheet.InnerNames.Rename(this, name);
				}
			}
		}
	}

	public string NameLocal
	{
		get
		{
			return m_name.Name;
		}
		set
		{
			m_name.Name = value;
		}
	}

	public IRange RefersToRange
	{
		get
		{
			_ = Value;
			return null;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			Value = value.AddressGlobal;
			m_bIsDeleted = false;
		}
	}

	public string Value
	{
		get
		{
			string result = null;
			try
			{
				result = m_book.FormulaUtil.ParsePtgArray(m_name.FormulaTokens);
			}
			catch (ParseException)
			{
			}
			return result;
		}
		set
		{
			SetValue(value, useR1C1: false);
		}
	}

	public string ValueR1C1
	{
		get
		{
			try
			{
				return m_book.FormulaUtil.ParsePtgArray(m_name.FormulaTokens, 0, 0, bR1C1: true, isForSerialization: false);
			}
			catch
			{
				return null;
			}
		}
		set
		{
			SetValue(value, useR1C1: true);
		}
	}

	public string RefersTo
	{
		get
		{
			return "=" + Value;
		}
		set
		{
			SetValue(value, useR1C1: false);
		}
	}

	public string RefersToR1C1
	{
		get
		{
			return "=" + ValueR1C1;
		}
		set
		{
			SetValue(value, useR1C1: true);
		}
	}

	public bool Visible
	{
		get
		{
			return !m_name.IsNameHidden;
		}
		set
		{
			m_name.IsNameHidden = !value;
		}
	}

	public bool IsLocal => m_name.IndexOrGlobal != 0;

	IWorksheet IName.Worksheet => m_worksheet;

	public bool IsQueryTableRange
	{
		get
		{
			return m_isQueryRange;
		}
		set
		{
			m_isQueryRange = value;
		}
	}

	public int SheetIndex
	{
		get
		{
			return m_sheetindex;
		}
		set
		{
			m_sheetindex = value;
		}
	}

	public string Scope
	{
		get
		{
			if (!IsLocal)
			{
				return "Workbook";
			}
			return m_worksheet.Name;
		}
	}

	public string Address
	{
		get
		{
			if (m_worksheet == null)
			{
				return Name;
			}
			return $"'{m_worksheet.Name}'!{Name}";
		}
	}

	public string AddressLocal => RefersToRange?.AddressLocal;

	public string AddressGlobal
	{
		get
		{
			if (m_worksheet == null)
			{
				return Name;
			}
			return $"'{m_worksheet.Name}'!{Name}";
		}
	}

	public string AddressGlobalWithoutSheetName => ((RangeImpl)RefersToRange).AddressGlobalWithoutSheetName;

	public string AddressR1C1 => RefersToRange.AddressR1C1;

	public string AddressR1C1Local => RefersToRange.AddressR1C1Local;

	public bool Boolean
	{
		get
		{
			return RefersToRange.Boolean;
		}
		set
		{
			RefersToRange.Boolean = value;
		}
	}

	public IBorders Borders => RefersToRange.Borders;

	public IRange[] Cells => RefersToRange.Cells;

	public int Column => RefersToRange?.LastColumn ?? (-1);

	public int ColumnGroupLevel => RefersToRange.ColumnGroupLevel;

	public double ColumnWidth
	{
		get
		{
			return RefersToRange.ColumnWidth;
		}
		set
		{
			RefersToRange.ColumnWidth = value;
		}
	}

	public int Count => RefersToRange?.Count ?? 1;

	public DateTime DateTime
	{
		get
		{
			return RefersToRange.DateTime;
		}
		set
		{
			RefersToRange.DateTime = value;
		}
	}

	public string DisplayText => RefersToRange.DisplayText;

	public IRange End => RefersToRange.End;

	public IRange EntireColumn => RefersToRange.EntireColumn;

	public IRange EntireRow => RefersToRange.EntireRow;

	public string Error
	{
		get
		{
			return RefersToRange.Error;
		}
		set
		{
			RefersToRange.Error = value;
		}
	}

	public string Formula
	{
		get
		{
			return RefersToRange.Formula;
		}
		set
		{
			RefersToRange.Formula = value;
		}
	}

	public string FormulaArray
	{
		get
		{
			return RefersToRange.FormulaArray;
		}
		set
		{
			RefersToRange.FormulaArray = value;
		}
	}

	public string FormulaArrayR1C1
	{
		get
		{
			return RefersToRange.FormulaArrayR1C1;
		}
		set
		{
			RefersToRange.FormulaArrayR1C1 = value;
		}
	}

	public bool FormulaHidden
	{
		get
		{
			return RefersToRange.FormulaHidden;
		}
		set
		{
			RefersToRange.FormulaHidden = value;
		}
	}

	public DateTime FormulaDateTime
	{
		get
		{
			return RefersToRange.FormulaDateTime;
		}
		set
		{
			RefersToRange.FormulaDateTime = value;
		}
	}

	public string FormulaR1C1
	{
		get
		{
			return RefersToRange.FormulaR1C1;
		}
		set
		{
			RefersToRange.FormulaR1C1 = value;
		}
	}

	public bool HasDataValidation => false;

	public bool HasBoolean => RefersToRange.HasBoolean;

	public bool HasDateTime => RefersToRange.HasDateTime;

	public bool HasFormulaBoolValue => RefersToRange.HasFormulaBoolValue;

	public bool HasFormulaErrorValue => RefersToRange.HasFormulaErrorValue;

	public bool HasFormulaDateTime => RefersToRange.HasFormulaDateTime;

	public bool HasFormulaNumberValue => RefersToRange.HasFormulaNumberValue;

	public bool HasFormulaStringValue => RefersToRange.HasFormulaStringValue;

	public bool HasFormula => RefersToRange.HasFormula;

	public bool HasFormulaArray => RefersToRange.HasFormulaArray;

	public bool HasNumber
	{
		get
		{
			double result;
			if (RefersToRange == null)
			{
				return double.TryParse(Value, out result);
			}
			return RefersToRange.HasNumber;
		}
	}

	public bool HasRichText => RefersToRange.HasRichText;

	public bool HasString
	{
		get
		{
			if (RefersToRange == null)
			{
				return false;
			}
			return RefersToRange.HasString;
		}
	}

	public bool HasStyle => RefersToRange.HasStyle;

	public OfficeHAlign HorizontalAlignment
	{
		get
		{
			return RefersToRange.HorizontalAlignment;
		}
		set
		{
			RefersToRange.HorizontalAlignment = value;
		}
	}

	public int IndentLevel
	{
		get
		{
			return RefersToRange.IndentLevel;
		}
		set
		{
			RefersToRange.IndentLevel = value;
		}
	}

	public bool IsBlank
	{
		get
		{
			if (RefersToRange == null)
			{
				return !string.IsNullOrEmpty(Value);
			}
			return RefersToRange.IsBlank;
		}
	}

	public bool IsBoolean => RefersToRange.IsBoolean;

	public bool IsError => RefersToRange.IsError;

	public bool IsGroupedByColumn => RefersToRange.IsGroupedByColumn;

	public bool IsGroupedByRow => RefersToRange.IsGroupedByRow;

	public bool IsInitialized => RefersToRange.IsInitialized;

	public int LastColumn => RefersToRange?.LastColumn ?? (-1);

	public int LastRow => RefersToRange?.LastRow ?? (-1);

	public double Number
	{
		get
		{
			return RefersToRange.Number;
		}
		set
		{
			if (RefersToRange != null)
			{
				RefersToRange.Number = value;
			}
		}
	}

	public string NumberFormat
	{
		get
		{
			return RefersToRange.NumberFormat;
		}
		set
		{
			RefersToRange.NumberFormat = value;
		}
	}

	public int Row => RefersToRange?.LastRow ?? (-1);

	public int RowGroupLevel => RefersToRange.RowGroupLevel;

	public double RowHeight
	{
		get
		{
			return RefersToRange.RowHeight;
		}
		set
		{
			RefersToRange.RowHeight = value;
		}
	}

	public IRange[] Rows => RefersToRange.Rows;

	public IRange[] Columns => RefersToRange.Columns;

	public IStyle CellStyle
	{
		get
		{
			return RefersToRange.CellStyle;
		}
		set
		{
			RefersToRange.CellStyle = value;
		}
	}

	public string CellStyleName
	{
		get
		{
			return RefersToRange.CellStyleName;
		}
		set
		{
			RefersToRange.CellStyleName = value;
		}
	}

	public string Text
	{
		get
		{
			return RefersToRange.Text;
		}
		set
		{
			RefersToRange.Text = value;
		}
	}

	public TimeSpan TimeSpan
	{
		get
		{
			return RefersToRange.TimeSpan;
		}
		set
		{
			RefersToRange.TimeSpan = value;
		}
	}

	string IRange.Value
	{
		get
		{
			return RefersToRange.Value;
		}
		set
		{
			RefersToRange.Value = value;
		}
	}

	public object Value2
	{
		get
		{
			return RefersToRange.Value2;
		}
		set
		{
			RefersToRange.Value2 = value;
		}
	}

	public OfficeVAlign VerticalAlignment
	{
		get
		{
			return RefersToRange.VerticalAlignment;
		}
		set
		{
			RefersToRange.VerticalAlignment = value;
		}
	}

	IWorksheet IRange.Worksheet => RefersToRange?.Worksheet;

	public string FormulaStringValue
	{
		get
		{
			return RefersToRange.FormulaStringValue;
		}
		set
		{
			RefersToRange.FormulaStringValue = value;
		}
	}

	public double FormulaNumberValue
	{
		get
		{
			return RefersToRange.FormulaNumberValue;
		}
		set
		{
			RefersToRange.FormulaNumberValue = value;
		}
	}

	public bool FormulaBoolValue
	{
		get
		{
			return RefersToRange.FormulaBoolValue;
		}
		set
		{
			RefersToRange.FormulaBoolValue = value;
		}
	}

	public string FormulaErrorValue
	{
		get
		{
			return RefersToRange.FormulaErrorValue;
		}
		set
		{
			RefersToRange.FormulaErrorValue = value;
		}
	}

	public IRichTextString RichText => RefersToRange.RichText;

	public bool IsMerged => RefersToRange.IsMerged;

	public IRange MergeArea => RefersToRange.MergeArea;

	public bool WrapText
	{
		get
		{
			return RefersToRange.WrapText;
		}
		set
		{
			RefersToRange.WrapText = value;
		}
	}

	public IRange this[int row, int column]
	{
		get
		{
			return RefersToRange[row, column];
		}
		set
		{
			RefersToRange[row, column] = value;
		}
	}

	public IRange this[int row, int column, int lastRow, int lastColumn] => RefersToRange[row, column, lastRow, lastColumn];

	public IRange this[string name] => this[name, false];

	public IRange this[string name, bool IsR1C1Notation] => RefersToRange[name, IsR1C1Notation];

	public bool HasExternalFormula => RefersToRange.HasExternalFormula;

	public ExcelIgnoreError IgnoreErrorOptions
	{
		get
		{
			return ExcelIgnoreError.All;
		}
		set
		{
		}
	}

	public bool? IsStringsPreserved
	{
		get
		{
			if (!(RefersToRange is ICombinedRange range))
			{
				return null;
			}
			return m_worksheet.GetStringPreservedValue(range);
		}
		set
		{
			if (RefersToRange is ICombinedRange range)
			{
				m_worksheet.SetStringPreservedValue(range, value);
			}
		}
	}

	public BuiltInStyles? BuiltInStyle
	{
		get
		{
			return RefersToRange.BuiltInStyle;
		}
		set
		{
			RefersToRange.BuiltInStyle = value;
		}
	}

	[CLSCompliant(false)]
	public NameRecord Record => m_name;

	public WorksheetImpl Worksheet => m_worksheet;

	public WorkbookImpl Workbook => m_book;

	public bool IsExternName
	{
		get
		{
			if (m_name == null || m_name.FormulaTokens == null)
			{
				return false;
			}
			int i = 0;
			for (int num = m_name.FormulaTokens.Length; i < num; i++)
			{
				if (m_name.FormulaTokens[i] is IReference)
				{
					int refIndex = (m_name.FormulaTokens[i] as IReference).RefIndex;
					if (m_book.IsExternalReference(refIndex))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	[CLSCompliant(false)]
	public MergeCellsRecord.MergedRegion Region
	{
		get
		{
			string addressLocal = AddressLocal;
			if (addressLocal == null)
			{
				return null;
			}
			string[] array = addressLocal.Split(':');
			long num = 0L;
			long index = 0L;
			if (array.Length > 2)
			{
				return null;
			}
			if (array.Length >= 1)
			{
				try
				{
					num = RangeImpl.CellNameToIndex(array[0]);
					index = num;
				}
				catch (ArgumentException)
				{
					return null;
				}
			}
			if (array.Length == 2)
			{
				try
				{
					index = RangeImpl.CellNameToIndex(array[1]);
				}
				catch (ArgumentException)
				{
					return null;
				}
			}
			ushort rowFrom = (ushort)(RangeImpl.GetRowFromCellIndex(num) - 1);
			ushort colFrom = (ushort)(RangeImpl.GetColumnFromCellIndex(num) - 1);
			ushort rowTo = (ushort)(RangeImpl.GetRowFromCellIndex(index) - 1);
			ushort colTo = (ushort)(RangeImpl.GetColumnFromCellIndex(index) - 1);
			return new MergeCellsRecord.MergedRegion(rowFrom, rowTo, colFrom, colTo);
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Region");
			}
			if (Region != value)
			{
				int firstRow = value.RowFrom + 1;
				int firstColumn = value.ColumnFrom + 1;
				int firstRow2 = value.RowTo + 1;
				int firstColumn2 = value.ColumnTo + 1;
				string cellName = RangeImpl.GetCellName(firstColumn, firstRow, bR1C1: false, bUseSeparater: true);
				string cellName2 = RangeImpl.GetCellName(firstColumn2, firstRow2, bR1C1: false, bUseSeparater: true);
				string value2 = Value;
				int num = value2.IndexOf("!");
				if (num < 1)
				{
					throw new NotSupportedException("Cannot find sheet name separater.");
				}
				string text = value2.Substring(0, num);
				if (cellName == cellName2)
				{
					Value = text + "!" + cellName;
				}
				else
				{
					Value = string.Format("{2}!{0}:{1}", cellName, cellName2, text);
				}
			}
		}
	}

	public bool IsBuiltIn
	{
		get
		{
			return m_name.IsBuinldInName;
		}
		set
		{
			m_name.IsBuinldInName = value;
		}
	}

	public int NameIndexChangedHandlersCount
	{
		get
		{
			if (this.NameIndexChanged != null)
			{
				return this.NameIndexChanged.GetInvocationList().Length;
			}
			return 0;
		}
	}

	public bool IsFunction
	{
		get
		{
			return m_name.IsNameFunction;
		}
		set
		{
			m_name.IsNameFunction = value;
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

	internal bool IsCommon
	{
		get
		{
			return m_isCommon;
		}
		set
		{
			m_isCommon = value;
		}
	}

	public int CellsCount
	{
		get
		{
			if (RefersToRange != null)
			{
				return (RefersToRange as ICombinedRange).CellsCount;
			}
			return 0;
		}
	}

	public string AddressGlobal2007
	{
		get
		{
			if (m_worksheet == null)
			{
				return "[0]!" + Name;
			}
			return $"'{m_worksheet.Name}'!{Name}";
		}
	}

	public string WorksheetName => Worksheet.Name;

	public event NameIndexChangedEventHandler NameIndexChanged;

	public NameImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_name = (NameRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Name);
		SetParents();
	}

	[CLSCompliant(false)]
	public NameImpl(IApplication application, object parent, NameRecord name, int index)
		: base(application, parent)
	{
		m_index = index;
		SetParents();
		Parse(name);
	}

	[CLSCompliant(false)]
	public NameImpl(IApplication application, object parent, NameRecord name)
		: this(application, parent, name, -1)
	{
	}

	public NameImpl(IApplication application, object parent, string name, IRange range, int index)
		: this(application, parent, name, range, index, bIsLocal: false)
	{
	}

	public NameImpl(IApplication application, object parent, string name, int index)
		: this(application, parent, name, index, bIsLocal: false)
	{
	}

	public NameImpl(IApplication application, object parent, string name, int index, bool bIsLocal)
		: this(application, parent)
	{
		m_index = index;
		Name = name;
		SetIndexOrGlobal(bIsLocal);
	}

	public NameImpl(IApplication application, object parent, string name, IRange range, int index, bool bIsLocal)
		: this(application, parent)
	{
		m_index = index;
		m_name = (NameRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Name);
		SetIndexOrGlobal(bIsLocal);
		SetParents();
		Name = name;
		RefersToRange = range;
	}

	private void SetIndexOrGlobal(bool bIsLocal)
	{
		m_name.IndexOrGlobal = (ushort)(bIsLocal ? ((ushort)(m_worksheet.RealIndex + 1)) : 0);
	}

	public void CopyToClipboard()
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(TimeSpan findValue)
	{
		return RefersToRange.FindAll(findValue);
	}

	public IRange[] FindAll(DateTime findValue)
	{
		return RefersToRange.FindAll(findValue);
	}

	public IRange[] FindAll(bool findValue)
	{
		return RefersToRange.FindAll(findValue);
	}

	public IRange[] FindAll(double findValue, OfficeFindType flags)
	{
		return RefersToRange.FindAll(findValue, flags);
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags)
	{
		return RefersToRange.FindAll(findValue, flags);
	}

	public IRange FindFirst(TimeSpan findValue)
	{
		return RefersToRange.FindFirst(findValue);
	}

	public IRange FindFirst(DateTime findValue)
	{
		return RefersToRange.FindFirst(findValue);
	}

	public IRange FindFirst(bool findValue)
	{
		return RefersToRange.FindFirst(findValue);
	}

	public IRange FindFirst(double findValue, OfficeFindType flags)
	{
		return RefersToRange.FindFirst(findValue, flags);
	}

	public IRange FindFirst(string findValue, OfficeFindType flags)
	{
		return RefersToRange.FindFirst(findValue, flags);
	}

	public void AutofitColumns()
	{
	}

	public void AutofitRows()
	{
	}

	public IRange MergeWith(IRange range)
	{
		return RefersToRange.MergeWith(range);
	}

	public IRange IntersectWith(IRange range)
	{
		return RefersToRange.IntersectWith(range);
	}

	public IRange CopyTo(IRange destination, OfficeCopyRangeOptions options)
	{
		return RefersToRange.CopyTo(destination, options);
	}

	public IRange CopyTo(IRange destination)
	{
		return RefersToRange.CopyTo(destination);
	}

	public void MoveTo(IRange destination)
	{
		RefersToRange.MoveTo(destination);
	}

	public void Clear(OfficeMoveDirection direction, OfficeCopyRangeOptions options)
	{
		RefersToRange.Clear(direction, options);
	}

	public void Clear(OfficeClearOptions option)
	{
		RefersToRange.Clear(option);
	}

	public void Clear(OfficeMoveDirection direction)
	{
		RefersToRange.Clear(direction);
	}

	public void Clear(bool isClearFormat)
	{
		RefersToRange.Clear(isClearFormat);
	}

	public void Clear()
	{
		RefersToRange.Clear();
	}

	public void FreezePanes()
	{
		RefersToRange.FreezePanes();
	}

	public void UnMerge()
	{
		RefersToRange.UnMerge();
	}

	public IRange Ungroup(OfficeGroupBy groupBy)
	{
		return RefersToRange.Ungroup(groupBy);
	}

	public void Merge()
	{
		RefersToRange.Merge();
	}

	public void Merge(bool clearCells)
	{
		RefersToRange.Merge(clearCells);
	}

	public IRange Group(OfficeGroupBy groupBy, bool bCollapsed)
	{
		return RefersToRange.Group(groupBy, bCollapsed);
	}

	public IRange Group(OfficeGroupBy groupBy)
	{
		return RefersToRange.Group(groupBy);
	}

	public void SubTotal(int groupBy, ConsolidationFunction function, int[] totalList)
	{
		RefersToRange.SubTotal(groupBy, function, totalList);
	}

	public void SubTotal(int groupBy, ConsolidationFunction function, int[] totalList, bool replace, bool pageBreaks, bool summaryBelowData)
	{
		RefersToRange.SubTotal(groupBy, function, totalList, replace, pageBreaks, summaryBelowData);
	}

	public IRange Activate()
	{
		return RefersToRange.Activate();
	}

	public IRange Activate(bool scroll)
	{
		return RefersToRange.Activate(scroll);
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
		RefersToRange.BorderAround(borderLine, borderColor);
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
		RefersToRange.BorderInside(borderLine, borderColor);
	}

	public void BorderNone()
	{
		RefersToRange.BorderNone();
	}

	public void CollapseGroup(OfficeGroupBy groupBy)
	{
		RefersToRange.CollapseGroup(groupBy);
	}

	public void ExpandGroup(OfficeGroupBy groupBy)
	{
		RefersToRange.ExpandGroup(groupBy);
	}

	public void ExpandGroup(OfficeGroupBy groupBy, ExpandCollapseFlags flags)
	{
		RefersToRange.ExpandGroup(groupBy, flags);
	}

	public void Delete()
	{
		m_name.Delete();
		m_bIsDeleted = true;
	}

	private void SetParents()
	{
		m_worksheet = FindParent(typeof(WorksheetImpl)) as WorksheetImpl;
		if (m_worksheet != null)
		{
			m_book = m_worksheet.Workbook as WorkbookImpl;
			return;
		}
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book != null)
		{
			return;
		}
		throw new ArgumentNullException("IName has no parent workbook");
	}

	[CLSCompliant(false)]
	public void Parse(NameRecord name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		m_name = (NameRecord)name.Clone();
	}

	private void OnValueChanged(string oldValue, string newValue, bool useR1C1)
	{
		if (oldValue != newValue)
		{
			Dictionary<Type, ReferenceIndexAttribute> dictionary = new Dictionary<Type, ReferenceIndexAttribute>();
			dictionary.Add(typeof(Area3DPtg), new ReferenceIndexAttribute(1));
			dictionary.Add(typeof(Ref3DPtg), new ReferenceIndexAttribute(1));
			OfficeParseFormulaOptions officeParseFormulaOptions = OfficeParseFormulaOptions.RootLevel | OfficeParseFormulaOptions.InName;
			if (useR1C1)
			{
				officeParseFormulaOptions |= OfficeParseFormulaOptions.UseR1C1;
			}
			m_name.FormulaTokens = m_book.FormulaUtil.ParseString(newValue, m_worksheet, dictionary, 0, null, officeParseFormulaOptions, 0, 0);
			RaiseNameIndexChangedEvent(new NameIndexChangedEventArgs(Index, Index));
		}
	}

	public void SetValue(Ptg[] parsedExpression)
	{
		m_name.FormulaTokens = parsedExpression;
		RaiseNameIndexChangedEvent(new NameIndexChangedEventArgs(Index, Index));
	}

	private void RaiseNameIndexChangedEvent(NameIndexChangedEventArgs e)
	{
		if (this.NameIndexChanged != null)
		{
			_ = this.NameIndexChanged.GetInvocationList().Length;
			this.NameIndexChanged(this, e);
		}
	}

	private bool IsValidName(string str)
	{
		if (str == null || str.Length == 0)
		{
			return false;
		}
		int i = 0;
		for (int length = str.Length; i < length; i++)
		{
			char c = str[i];
			if (!char.IsLetterOrDigit(c) && Array.IndexOf(DEF_VALID_SYMBOL, c) == -1 && c > NameRecord.PREDEFINED_NAMES.Length)
			{
				return false;
			}
		}
		return true;
	}

	private void SetValue(string strValue, bool useR1C1)
	{
		if (strValue != null && strValue.Length > 0 && strValue[0] == '=')
		{
			strValue = strValue.Substring(1);
		}
		string value = Value;
		if (value != strValue)
		{
			OnValueChanged(value, strValue, useR1C1);
		}
	}

	public void ConvertFullRowColumnName(OfficeVersion version)
	{
		FormulaRecord.ConvertFormulaTokens(m_name.FormulaTokens, version == OfficeVersion.Excel97to2003);
	}

	public string GetValue(FormulaUtil formulaUtil)
	{
		return formulaUtil.ParsePtgArray(m_name.FormulaTokens);
	}

	public void SetIndex(int index)
	{
		SetIndex(index, bRaiseEvent: true);
	}

	public void SetIndex(int index, bool bRaiseEvent)
	{
		if (index != m_index)
		{
			int index2 = m_index;
			m_index = index;
			if (bRaiseEvent)
			{
				RaiseNameIndexChangedEvent(new NameIndexChangedEventArgs(index2, index));
			}
		}
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		records.Add(m_name);
	}

	public void SetSheetIndex(int iSheetIndex)
	{
		m_name.IndexOrGlobal = (ushort)(iSheetIndex + 1);
	}

	void IParseable.Parse()
	{
	}

	public Ptg[] GetNativePtg()
	{
		Ptg[] array = new Ptg[1];
		int supIndex = m_book.ExternWorkbooks.InsertSelfSupbook();
		int num = m_book.AddSheetReference(supIndex, 65534, 65534);
		array[0] = FormulaUtil.CreatePtg(FormulaToken.tNameX1, num, Index);
		return array;
	}

	public object Clone(object parent)
	{
		NameImpl nameImpl = (NameImpl)MemberwiseClone();
		nameImpl.SetParent(parent);
		nameImpl.SetParents();
		nameImpl.m_name = (NameRecord)CloneUtils.CloneCloneable(m_name);
		int indexOrGlobal = m_name.IndexOrGlobal;
		if (indexOrGlobal != 0)
		{
			indexOrGlobal--;
			WorksheetImpl worksheetImpl = (WorksheetImpl)nameImpl.m_book.Objects[indexOrGlobal];
			worksheetImpl.InnerNames.AddLocal(nameImpl);
			nameImpl.m_worksheet = worksheetImpl;
		}
		return nameImpl;
	}

	public IEnumerator GetEnumerator()
	{
		return RefersToRange.GetEnumerator();
	}

	public string GetNewAddress(Dictionary<string, string> names, out string strSheetName)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange Clone(object parent, Dictionary<string, string> hashNewNames, WorkbookImpl book)
	{
		if (Worksheet != null)
		{
			string text = Worksheet.Name;
			if (hashNewNames != null && hashNewNames.ContainsKey(text))
			{
				text = hashNewNames[text];
			}
			WorksheetImpl worksheetImpl = (WorksheetImpl)book.Worksheets[text];
			IRange range = null;
			if (worksheetImpl != null)
			{
				_ = worksheetImpl.Names.Count;
				return worksheetImpl.Names[Name] as NameImpl;
			}
			return Worksheet.Names[Name] as NameImpl;
		}
		return book.Names[Name] as NameImpl;
	}

	public void ClearConditionalFormats()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public Rectangle[] GetRectangles()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public int GetRectanglesCount()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	internal void ClearAll()
	{
		if (m_name != null)
		{
			m_name.ClearData();
		}
		m_name = null;
		Dispose();
	}

	void IDisposable.Dispose()
	{
		GC.SuppressFinalize(this);
	}
}
