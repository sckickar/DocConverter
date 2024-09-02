using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart;

internal interface ICombinedRange : IRange, IParentApplication, IEnumerable
{
	int CellsCount { get; }

	string AddressGlobal2007 { get; }

	string WorksheetName { get; }

	string GetNewAddress(Dictionary<string, string> names, out string strSheetName);

	IRange Clone(object parent, Dictionary<string, string> hashNewNames, WorkbookImpl book);

	void ClearConditionalFormats();

	Rectangle[] GetRectangles();

	int GetRectanglesCount();
}
