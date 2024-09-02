using DocGen.Drawing;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

internal interface IRangeGetterToken : IRangeGetter, IRectGetter
{
	Ptg UpdateRectangle(Rectangle rectangle);

	Ptg ConvertToError();
}
