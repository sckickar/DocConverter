using DocGen.Drawing;

namespace DocGen.OfficeChart;

internal interface ITabSheet : IParentApplication
{
	OfficeKnownColors TabColor { get; set; }

	Color TabColorRGB { get; set; }

	IOfficeChartShapes Charts { get; }

	IWorkbook Workbook { get; }

	IShapes Shapes { get; }

	bool IsRightToLeft { get; set; }

	bool IsSelected { get; }

	int TabIndex { get; }

	string Name { get; set; }

	OfficeWorksheetVisibility Visibility { get; set; }

	ITextBoxes TextBoxes { get; }

	string CodeName { get; }

	bool ProtectContents { get; }

	bool ProtectDrawingObjects { get; }

	bool ProtectScenarios { get; }

	OfficeSheetProtection Protection { get; }

	bool IsPasswordProtected { get; }

	void Activate();

	void Select();

	void Unselect();

	void Protect(string password);

	void Protect(string password, OfficeSheetProtection options);

	void Unprotect(string password);
}
