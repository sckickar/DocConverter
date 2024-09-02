using System;
using System.Collections;
using DocGen.Drawing;

namespace DocGen.OfficeChart;

internal interface IRange : IParentApplication, IEnumerable
{
	string Address { get; }

	string AddressLocal { get; }

	string AddressGlobal { get; }

	string AddressR1C1 { get; }

	string AddressR1C1Local { get; }

	bool Boolean { get; set; }

	IBorders Borders { get; }

	IRange[] Cells { get; }

	int Column { get; }

	int ColumnGroupLevel { get; }

	double ColumnWidth { get; set; }

	int Count { get; }

	DateTime DateTime { get; set; }

	string DisplayText { get; }

	IRange End { get; }

	IRange EntireColumn { get; }

	IRange EntireRow { get; }

	string Error { get; set; }

	string Formula { get; set; }

	string FormulaArray { get; set; }

	string FormulaArrayR1C1 { get; set; }

	bool FormulaHidden { get; set; }

	DateTime FormulaDateTime { get; set; }

	string FormulaR1C1 { get; set; }

	bool FormulaBoolValue { get; set; }

	string FormulaErrorValue { get; set; }

	bool HasBoolean { get; }

	bool HasDateTime { get; }

	bool HasFormula { get; }

	bool HasFormulaArray { get; }

	bool HasNumber { get; }

	bool HasRichText { get; }

	bool HasString { get; }

	bool HasStyle { get; }

	OfficeHAlign HorizontalAlignment { get; set; }

	int IndentLevel { get; set; }

	bool IsBlank { get; }

	bool IsBoolean { get; }

	bool IsError { get; }

	bool IsGroupedByColumn { get; }

	bool IsGroupedByRow { get; }

	bool IsInitialized { get; }

	int LastColumn { get; }

	int LastRow { get; }

	double Number { get; set; }

	string NumberFormat { get; set; }

	int Row { get; }

	int RowGroupLevel { get; }

	double RowHeight { get; set; }

	IRange[] Rows { get; }

	IRange[] Columns { get; }

	IStyle CellStyle { get; set; }

	string CellStyleName { get; set; }

	string Text { get; set; }

	TimeSpan TimeSpan { get; set; }

	string Value { get; set; }

	object Value2 { get; set; }

	OfficeVAlign VerticalAlignment { get; set; }

	IWorksheet Worksheet { get; }

	IRange this[int row, int column] { get; set; }

	IRange this[int row, int column, int lastRow, int lastColumn] { get; }

	IRange this[string name] { get; }

	IRange this[string name, bool IsR1C1Notation] { get; }

	string FormulaStringValue { get; set; }

	double FormulaNumberValue { get; set; }

	bool HasFormulaBoolValue { get; }

	bool HasFormulaErrorValue { get; }

	bool HasFormulaDateTime { get; }

	bool HasFormulaNumberValue { get; }

	bool HasFormulaStringValue { get; }

	IRichTextString RichText { get; }

	bool IsMerged { get; }

	IRange MergeArea { get; }

	bool WrapText { get; set; }

	bool HasExternalFormula { get; }

	BuiltInStyles? BuiltInStyle { get; set; }

	IRange Activate();

	IRange Activate(bool scroll);

	IRange Group(OfficeGroupBy groupBy);

	IRange Group(OfficeGroupBy groupBy, bool bCollapsed);

	void SubTotal(int groupBy, ConsolidationFunction function, int[] totalList);

	void SubTotal(int groupBy, ConsolidationFunction function, int[] totalList, bool replace, bool pageBreaks, bool summaryBelowData);

	void Merge();

	void Merge(bool clearCells);

	IRange Ungroup(OfficeGroupBy groupBy);

	void UnMerge();

	void FreezePanes();

	void Clear();

	void Clear(bool isClearFormat);

	void Clear(OfficeClearOptions option);

	void Clear(OfficeMoveDirection direction);

	void Clear(OfficeMoveDirection direction, OfficeCopyRangeOptions options);

	void MoveTo(IRange destination);

	IRange CopyTo(IRange destination);

	IRange CopyTo(IRange destination, OfficeCopyRangeOptions options);

	IRange IntersectWith(IRange range);

	IRange MergeWith(IRange range);

	IRange FindFirst(string findValue, OfficeFindType flags);

	IRange FindFirst(double findValue, OfficeFindType flags);

	IRange FindFirst(bool findValue);

	IRange FindFirst(DateTime findValue);

	IRange FindFirst(TimeSpan findValue);

	IRange[] FindAll(string findValue, OfficeFindType flags);

	IRange[] FindAll(double findValue, OfficeFindType flags);

	IRange[] FindAll(bool findValue);

	IRange[] FindAll(DateTime findValue);

	IRange[] FindAll(TimeSpan findValue);

	void BorderAround();

	void BorderAround(OfficeLineStyle borderLine);

	void BorderAround(OfficeLineStyle borderLine, Color borderColor);

	void BorderAround(OfficeLineStyle borderLine, OfficeKnownColors borderColor);

	void BorderInside();

	void BorderInside(OfficeLineStyle borderLine);

	void BorderInside(OfficeLineStyle borderLine, Color borderColor);

	void BorderInside(OfficeLineStyle borderLine, OfficeKnownColors borderColor);

	void BorderNone();

	void CollapseGroup(OfficeGroupBy groupBy);

	void ExpandGroup(OfficeGroupBy groupBy);

	void ExpandGroup(OfficeGroupBy groupBy, ExpandCollapseFlags flags);
}
