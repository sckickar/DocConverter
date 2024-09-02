namespace DocGen.OfficeChart.Calculate;

internal interface ISheetData : ICalcData
{
	int GetFirstRow();

	int GetLastRow();

	int GetRowCount();

	int GetFirstColumn();

	int GetLastColumn();

	int GetColumnCount();
}
