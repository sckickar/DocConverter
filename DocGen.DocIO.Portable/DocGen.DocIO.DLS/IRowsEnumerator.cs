namespace DocGen.DocIO.DLS;

public interface IRowsEnumerator
{
	string[] ColumnNames { get; }

	int RowsCount { get; }

	int CurrentRowIndex { get; }

	string TableName { get; }

	bool IsEnd { get; }

	bool IsLast { get; }

	void Reset();

	bool NextRow();

	object GetCellValue(string columnName);
}
