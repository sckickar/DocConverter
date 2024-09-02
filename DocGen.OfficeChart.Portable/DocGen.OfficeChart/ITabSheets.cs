namespace DocGen.OfficeChart;

internal interface ITabSheets
{
	int Count { get; }

	ITabSheet this[int index] { get; }

	void Move(int iOldIndex, int iNewIndex);

	void MoveBefore(ITabSheet sheetToMove, ITabSheet sheetForPlacement);

	void MoveAfter(ITabSheet sheetToCopy, ITabSheet sheetForPlacement);
}
