using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Interfaces;

internal interface IInternalExtendedFormat : IExtendedFormat, IParentApplication
{
	ChartColor BottomBorderColor { get; }

	ChartColor TopBorderColor { get; }

	ChartColor LeftBorderColor { get; }

	ChartColor RightBorderColor { get; }

	ChartColor DiagonalBorderColor { get; }

	OfficeLineStyle LeftBorderLineStyle { get; set; }

	OfficeLineStyle RightBorderLineStyle { get; set; }

	OfficeLineStyle TopBorderLineStyle { get; set; }

	OfficeLineStyle BottomBorderLineStyle { get; set; }

	OfficeLineStyle DiagonalUpBorderLineStyle { get; set; }

	OfficeLineStyle DiagonalDownBorderLineStyle { get; set; }

	bool DiagonalUpVisible { get; set; }

	bool DiagonalDownVisible { get; set; }

	WorkbookImpl Workbook { get; }

	void BeginUpdate();

	void EndUpdate();
}
