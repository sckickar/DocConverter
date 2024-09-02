using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Calculate;

namespace DocGen.OfficeChart.Implementation;

internal class InvalidRange : ICombinedRange, IRange, IParentApplication, IEnumerable
{
	private object m_parent;

	private IApplication m_application;

	private int m_iFirstRow;

	private int m_iLastRow;

	private int m_iFirstColumn;

	private int m_iLastColumn;

	public int CellsCount
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public string AddressGlobal2007
	{
		get
		{
			throw new NotImplementedException();
		}
	}

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
			throw new NotImplementedException();
		}
	}

	public string AddressLocal => RangeImpl.GetAddressLocal(m_iFirstRow, m_iFirstColumn, m_iLastRow, m_iLastColumn);

	public string AddressGlobal
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public string AddressR1C1
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public string AddressR1C1Local
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool Boolean
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public IBorders Borders
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRange[] Cells
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public int Column
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public int ColumnGroupLevel
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public double ColumnWidth
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public int Count
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public DateTime DateTime
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string DisplayText
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRange End
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRange EntireColumn
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRange EntireRow
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public string Error
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string Formula
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string FormulaArray
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string FormulaArrayR1C1
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool FormulaHidden
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public DateTime FormulaDateTime
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string FormulaR1C1
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool FormulaBoolValue
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string FormulaErrorValue
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool HasDataValidation
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool HasBoolean
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool HasDateTime
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool HasFormula
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool HasFormulaArray
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool HasNumber
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool HasRichText
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool HasString
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool HasStyle
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public OfficeHAlign HorizontalAlignment
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public int IndentLevel
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool IsBlank
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool IsBoolean
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool IsError
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool IsGroupedByColumn
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool IsGroupedByRow
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool IsInitialized
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public int LastColumn
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public int LastRow
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public double Number
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string NumberFormat
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public int Row
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public int RowGroupLevel
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public double RowHeight
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public IRange[] Rows
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRange[] Columns
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IStyle CellStyle
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string CellStyleName
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string Text
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public TimeSpan TimeSpan
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string Value
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public object Value2
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public OfficeVAlign VerticalAlignment
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public IWorksheet Worksheet
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRange this[int row, int column]
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public IRange this[int row, int column, int lastRow, int lastColumn]
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRange this[string name]
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRange this[string name, bool IsR1C1Notation]
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public string FormulaStringValue
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public double FormulaNumberValue
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool HasFormulaBoolValue
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool HasFormulaErrorValue
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool HasFormulaDateTime
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool HasFormulaNumberValue
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool HasFormulaStringValue
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRichTextString RichText
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool IsMerged
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRange MergeArea
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool WrapText
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool HasExternalFormula
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public ExcelIgnoreError IgnoreErrorOptions
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool? IsStringsPreserved
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public BuiltInStyles? BuiltInStyle
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string WorksheetName => "#REF";

	public IApplication Application => m_application;

	public object Parent => m_parent;

	public string GetNewAddress(Dictionary<string, string> names, out string strSheetName)
	{
		throw new NotImplementedException();
	}

	public IRange Clone(object parent, Dictionary<string, string> hashNewNames, WorkbookImpl book)
	{
		throw new NotImplementedException();
	}

	public void ClearConditionalFormats()
	{
		throw new NotImplementedException();
	}

	public Rectangle[] GetRectangles()
	{
		throw new NotImplementedException();
	}

	public int GetRectanglesCount()
	{
		throw new NotImplementedException();
	}

	public IRange Activate()
	{
		throw new NotImplementedException();
	}

	public IRange Activate(bool scroll)
	{
		throw new NotImplementedException();
	}

	public IRange Group(OfficeGroupBy groupBy)
	{
		throw new NotImplementedException();
	}

	public IRange Group(OfficeGroupBy groupBy, bool bCollapsed)
	{
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}

	public void Merge(bool clearCells)
	{
		throw new NotImplementedException();
	}

	public IRange Ungroup(OfficeGroupBy groupBy)
	{
		throw new NotImplementedException();
	}

	public void UnMerge()
	{
		throw new NotImplementedException();
	}

	public void FreezePanes()
	{
		throw new NotImplementedException();
	}

	public void Clear()
	{
		throw new NotImplementedException();
	}

	public void Clear(bool isClearFormat)
	{
		throw new NotImplementedException();
	}

	public void Clear(OfficeClearOptions option)
	{
		throw new NotImplementedException();
	}

	public void Clear(OfficeMoveDirection direction)
	{
		throw new NotImplementedException();
	}

	public void Clear(OfficeMoveDirection direction, OfficeCopyRangeOptions options)
	{
		throw new NotImplementedException();
	}

	public void MoveTo(IRange destination)
	{
		throw new NotImplementedException();
	}

	public IRange CopyTo(IRange destination)
	{
		throw new NotImplementedException();
	}

	public IRange CopyTo(IRange destination, OfficeCopyRangeOptions options)
	{
		throw new NotImplementedException();
	}

	public IRange IntersectWith(IRange range)
	{
		throw new NotImplementedException();
	}

	public IRange MergeWith(IRange range)
	{
		throw new NotImplementedException();
	}

	public void AutofitRows()
	{
		throw new NotImplementedException();
	}

	public void AutofitColumns()
	{
		throw new NotImplementedException();
	}

	public IRange FindFirst(string findValue, OfficeFindType flags)
	{
		throw new NotImplementedException();
	}

	public IRange FindFirst(double findValue, OfficeFindType flags)
	{
		throw new NotImplementedException();
	}

	public IRange FindFirst(bool findValue)
	{
		throw new NotImplementedException();
	}

	public IRange FindFirst(DateTime findValue)
	{
		throw new NotImplementedException();
	}

	public IRange FindFirst(TimeSpan findValue)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(double findValue, OfficeFindType flags)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(bool findValue)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(DateTime findValue)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(TimeSpan findValue)
	{
		throw new NotImplementedException();
	}

	public void BorderAround()
	{
		throw new NotImplementedException();
	}

	public void BorderAround(OfficeLineStyle borderLine)
	{
		throw new NotImplementedException();
	}

	public void BorderAround(OfficeLineStyle borderLine, Color borderColor)
	{
		throw new NotImplementedException();
	}

	public void BorderAround(OfficeLineStyle borderLine, OfficeKnownColors borderColor)
	{
		throw new NotImplementedException();
	}

	public void BorderInside()
	{
		throw new NotImplementedException();
	}

	public void BorderInside(OfficeLineStyle borderLine)
	{
		throw new NotImplementedException();
	}

	public void BorderInside(OfficeLineStyle borderLine, Color borderColor)
	{
		throw new NotImplementedException();
	}

	public void BorderInside(OfficeLineStyle borderLine, OfficeKnownColors borderColor)
	{
		throw new NotImplementedException();
	}

	public void BorderNone()
	{
		throw new NotImplementedException();
	}

	public void CollapseGroup(OfficeGroupBy groupBy)
	{
		throw new NotImplementedException();
	}

	public void ExpandGroup(OfficeGroupBy groupBy)
	{
		throw new NotImplementedException();
	}

	public void ExpandGroup(OfficeGroupBy groupBy, ExpandCollapseFlags flags)
	{
		throw new NotImplementedException();
	}

	public IEnumerator GetEnumerator()
	{
		throw new NotImplementedException();
	}

	public InvalidRange(object parent, IRange range)
	{
		m_parent = parent;
		m_application = (range as RangeImpl).Application;
		m_iFirstColumn = range.Column;
		m_iFirstRow = range.Row;
		m_iLastRow = range.LastRow;
		m_iLastColumn = range.LastColumn;
	}
}
