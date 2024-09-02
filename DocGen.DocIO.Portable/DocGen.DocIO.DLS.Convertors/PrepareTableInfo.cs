namespace DocGen.DocIO.DLS.Convertors;

internal struct PrepareTableInfo
{
	internal bool InTable;

	internal int Level;

	internal int PrevLevel;

	internal PrepareTableState State;

	internal PrepareTableInfo(bool inTable, int currLevel, int prevLevel)
	{
		InTable = inTable;
		PrevLevel = prevLevel;
		Level = currLevel;
		if (Level > PrevLevel)
		{
			State = PrepareTableState.EnterTable;
		}
		else if (Level < PrevLevel)
		{
			State = PrepareTableState.LeaveTable;
		}
		else
		{
			State = PrepareTableState.NoChange;
		}
	}
}
