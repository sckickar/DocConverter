using DocGen.OfficeChart.Calculate;

namespace DocGen.OfficeChart;

internal interface IWorksheetGroup : IWorksheet, ITabSheet, IParentApplication, ICalcData
{
	IWorksheet this[int index] { get; }

	bool IsEmpty { get; }

	int Count { get; }

	int Add(ITabSheet sheet);
}
