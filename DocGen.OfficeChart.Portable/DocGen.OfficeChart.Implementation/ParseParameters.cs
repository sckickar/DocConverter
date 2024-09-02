using System.Collections.Generic;

namespace DocGen.OfficeChart.Implementation;

internal class ParseParameters
{
	public readonly FormulaUtil FormulaUtility;

	public readonly IWorksheet Worksheet;

	public readonly Dictionary<string, string> WorksheetNames;

	public readonly bool IsR1C1;

	public readonly int CellRow;

	public readonly int CellColumn;

	public readonly IWorkbook Workbook;

	public readonly OfficeVersion Version;

	public ParseParameters(IWorksheet sheet, Dictionary<string, string> worksheetNames, bool r1C1, int cellRow, int cellColumn, FormulaUtil formulaUtility, IWorkbook book)
	{
		Worksheet = sheet;
		WorksheetNames = worksheetNames;
		IsR1C1 = r1C1;
		CellRow = cellRow;
		CellColumn = cellColumn;
		FormulaUtility = formulaUtility;
		Workbook = book;
		Version = ((WorkbookImpl)Workbook).Version;
	}
}
