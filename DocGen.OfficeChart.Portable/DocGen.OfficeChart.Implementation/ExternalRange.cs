using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Calculate;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation;

internal class ExternalRange : IRange, IParentApplication, IEnumerable, INativePTG, ICombinedRange
{
	private ExternWorksheetImpl m_sheet;

	private int m_iFirstRow;

	private int m_iFirstColumn;

	private int m_iLastRow;

	private int m_iLastColumn;

	private bool m_bIsNumReference;

	private bool m_bIsMultiReference;

	private bool m_bIsStringReference;

	internal string CalculatedValue
	{
		get
		{
			if (Parent is WorksheetImpl && ((WorksheetImpl)Parent).CalcEngine != null)
			{
				string cellRef = RangeInfo.GetAlphaLabel(Column) + Row;
				return ((WorksheetImpl)Parent).CalcEngine.PullUpdatedValue(cellRef);
			}
			return null;
		}
	}

	public string Address
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string AddressLocal => RangeImpl.GetAddressLocal(m_iFirstRow, m_iFirstColumn, m_iLastRow, m_iLastColumn);

	public string AddressGlobal
	{
		get
		{
			string addressLocal = RangeImpl.GetAddressLocal(m_iFirstRow, m_iFirstColumn, m_iLastRow, m_iLastColumn);
			return $"[{m_sheet.Workbook.Index + 1}]{m_sheet.Name}!{addressLocal}";
		}
	}

	public string AddressR1C1
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string AddressR1C1Local
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool Boolean
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IBorders Borders
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange[] Cells
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int Column => m_iFirstColumn;

	public int ColumnGroupLevel
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public double ColumnWidth
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int Count => (m_iLastColumn - m_iFirstColumn + 1) * (m_iLastRow - m_iFirstRow + 1);

	public DateTime DateTime
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string DisplayText
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange End
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange EntireColumn
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange EntireRow
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string Error
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string Formula
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string FormulaArray
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string FormulaArrayR1C1
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool FormulaHidden
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public DateTime FormulaDateTime
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string FormulaR1C1
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool FormulaBoolValue
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string FormulaErrorValue
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasDataValidation
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasBoolean
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasDateTime
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasFormula
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasFormulaArray
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasNumber
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasRichText
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasString => m_sheet.CellRecords.GetCellType(Row, Column) == WorksheetImpl.TRangeValueType.String;

	public bool HasStyle
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public OfficeHAlign HorizontalAlignment
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int IndentLevel
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsBlank
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsBoolean
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsError
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsGroupedByColumn
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsGroupedByRow
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsInitialized
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int LastColumn => m_iLastColumn;

	public int LastRow => m_iLastRow;

	public double Number
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string NumberFormat
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int Row => m_iFirstRow;

	public int RowGroupLevel
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public double RowHeight
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange[] Rows
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange[] Columns
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IStyle CellStyle
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string CellStyleName
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string Text
	{
		get
		{
			long cellIndex = RangeImpl.GetCellIndex(Column, Row);
			return m_sheet.CellRecords.GetText(cellIndex);
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public TimeSpan TimeSpan
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string Value
	{
		get
		{
			long cellIndex = RangeImpl.GetCellIndex(m_iFirstRow, m_iFirstColumn);
			if (!IsSingleCell)
			{
				return null;
			}
			return m_sheet.CellRecords.GetText(cellIndex);
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public object Value2
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public OfficeVAlign VerticalAlignment
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IWorksheet Worksheet => m_sheet;

	public IRange this[int row, int column]
	{
		get
		{
			return new ExternalRange(m_sheet, row, column);
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange this[int row, int column, int lastRow, int lastColumn]
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange this[string name]
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange this[string name, bool IsR1C1Notation]
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string FormulaStringValue
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public double FormulaNumberValue
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasFormulaBoolValue
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasFormulaErrorValue
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasFormulaDateTime
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasFormulaNumberValue
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasFormulaStringValue
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRichTextString RichText
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsMerged
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange MergeArea
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool WrapText
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool HasExternalFormula
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public ExcelIgnoreError IgnoreErrorOptions
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool? IsStringsPreserved
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public BuiltInStyles? BuiltInStyle
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string AddressGlobal2007 => AddressGlobal;

	public IApplication Application => m_sheet.Application;

	public object Parent => m_sheet;

	public bool IsSingleCell
	{
		get
		{
			if (m_iFirstColumn == m_iLastColumn)
			{
				return m_iFirstRow == m_iLastRow;
			}
			return false;
		}
	}

	public int CellsCount => Count;

	public string WorksheetName => Worksheet.Name;

	public ExternWorksheetImpl ExternSheet => m_sheet;

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

	public IRange Activate()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange Activate(bool scroll)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange Group(OfficeGroupBy groupBy)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange Group(OfficeGroupBy groupBy, bool bCollapsed)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SubTotal(int groupBy, ConsolidationFunction function, int[] totalList)
	{
		throw new NotSupportedException();
	}

	public void SubTotal(int groupBy, ConsolidationFunction function, int[] totalList, bool replace, bool pageBreaks, bool summaryBelowData)
	{
		throw new NotSupportedException();
	}

	public void Merge()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Merge(bool clearCells)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange Ungroup(OfficeGroupBy groupBy)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void UnMerge()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void FreezePanes()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Clear(OfficeClearOptions option)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Clear()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Clear(bool isClearFormat)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Clear(OfficeMoveDirection direction)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Clear(OfficeMoveDirection direction, OfficeCopyRangeOptions options)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void MoveTo(IRange destination)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange CopyTo(IRange destination)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange CopyTo(IRange destination, OfficeCopyRangeOptions options)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange IntersectWith(IRange range)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange MergeWith(IRange range)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void AutofitRows()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void AutofitColumns()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindFirst(string findValue, OfficeFindType flags)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindFirst(double findValue, OfficeFindType flags)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindFirst(bool findValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindFirst(DateTime findValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindFirst(TimeSpan findValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange[] FindAll(double findValue, OfficeFindType flags)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange[] FindAll(bool findValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange[] FindAll(DateTime findValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange[] FindAll(TimeSpan findValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void BorderAround()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void BorderAround(OfficeLineStyle borderLine)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void BorderAround(OfficeLineStyle borderLine, Color borderColor)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void BorderAround(OfficeLineStyle borderLine, OfficeKnownColors borderColor)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void BorderInside()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void BorderInside(OfficeLineStyle borderLine)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void BorderInside(OfficeLineStyle borderLine, Color borderColor)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void BorderInside(OfficeLineStyle borderLine, OfficeKnownColors borderColor)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void BorderNone()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void CollapseGroup(OfficeGroupBy groupBy)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void ExpandGroup(OfficeGroupBy groupBy)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void ExpandGroup(OfficeGroupBy groupBy, ExpandCollapseFlags flags)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public ExternalRange(ExternWorksheetImpl sheet, int row, int column)
		: this(sheet, row, column, row, column)
	{
	}

	public ExternalRange(ExternWorksheetImpl sheet, int row, int column, int lastRow, int lastColumn)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		m_sheet = sheet;
		m_iFirstRow = row;
		m_iFirstColumn = column;
		m_iLastRow = lastRow;
		m_iLastColumn = lastColumn;
	}

	public Ptg[] GetNativePtg()
	{
		int referenceIndex = m_sheet.ReferenceIndex;
		Ptg ptg;
		if (IsSingleCell)
		{
			Ref3DPtg obj = (Ref3DPtg)FormulaUtil.CreatePtg(FormulaToken.tRef3d1);
			obj.RefIndex = (ushort)referenceIndex;
			obj.RowIndex = m_iFirstRow - 1;
			obj.ColumnIndex = m_iFirstColumn - 1;
			ptg = obj;
		}
		else
		{
			Area3DPtg obj2 = (Area3DPtg)FormulaUtil.CreatePtg(FormulaToken.tArea3d1);
			obj2.RefIndex = (ushort)referenceIndex;
			obj2.FirstRow = m_iFirstRow - 1;
			obj2.FirstColumn = m_iFirstColumn - 1;
			obj2.LastRow = m_iLastRow - 1;
			obj2.LastColumn = m_iLastColumn - 1;
			ptg = obj2;
		}
		return new Ptg[1] { ptg };
	}

	public string GetNewAddress(Dictionary<string, string> names, out string strSheetName)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange Clone(object parent, Dictionary<string, string> hashNewNames, WorkbookImpl book)
	{
		int index = m_sheet.Workbook.Index;
		return new ExternalRange(book.ExternWorkbooks[index].Worksheets[m_sheet.Index], m_iFirstRow, m_iFirstColumn, m_iLastRow, m_iLastColumn);
	}

	public void ClearConditionalFormats()
	{
		throw new NotSupportedException();
	}

	public Rectangle[] GetRectangles()
	{
		return new Rectangle[1] { Rectangle.FromLTRB(m_iFirstColumn, m_iFirstRow, m_iLastColumn, m_iLastRow) };
	}

	public int GetRectanglesCount()
	{
		return 1;
	}

	public IEnumerator GetEnumerator()
	{
		throw new NotImplementedException();
	}
}
