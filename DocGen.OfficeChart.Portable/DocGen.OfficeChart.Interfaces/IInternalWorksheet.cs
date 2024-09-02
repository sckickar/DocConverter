using System.Collections.Generic;
using DocGen.OfficeChart.Calculate;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Collections;

namespace DocGen.OfficeChart.Interfaces;

internal interface IInternalWorksheet : IWorksheet, ITabSheet, IParentApplication, ICalcData
{
	int DefaultRowHeight { get; }

	int FirstRow { get; set; }

	int FirstColumn { get; set; }

	int LastRow { get; set; }

	int LastColumn { get; set; }

	CellRecordCollection CellRecords { get; }

	WorkbookImpl ParentWorkbook { get; }

	OfficeVersion Version { get; }

	bool IsArrayFormula(long index);

	IInternalWorksheet GetClonedObject(Dictionary<string, string> hashNewNames, WorkbookImpl book);
}
