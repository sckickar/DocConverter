namespace DocGen.OfficeChart;

internal interface IWorksheetCustomProperties
{
	ICustomProperty this[int index] { get; }

	ICustomProperty this[string strName] { get; }

	int Count { get; }

	ICustomProperty Add(string strName);

	bool Contains(string strName);
}
