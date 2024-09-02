namespace DocGen.OfficeChart;

internal interface IMergeCells
{
	void AddMerge(int RowFrom, int RowTo, int ColFrom, int ColTo, OfficeMergeOperation operation);

	void DeleteMerge(int CellIndex);
}
