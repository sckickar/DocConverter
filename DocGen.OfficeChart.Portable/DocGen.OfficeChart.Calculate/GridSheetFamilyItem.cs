using System.Collections.Generic;

namespace DocGen.OfficeChart.Calculate;

internal class GridSheetFamilyItem
{
	internal bool isSheeted;

	internal Dictionary<object, object> sheetDependentCells;

	public Dictionary<object, object> ParentObjectToToken;

	internal Dictionary<object, object> sheetFormulaInfoTable;

	internal Dictionary<object, object> sheetDependentFormulaCells;

	public Dictionary<object, object> TokenToParentObject;

	public Dictionary<object, object> SheetNameToToken;

	public Dictionary<object, object> SheetNameToParentObject;
}
