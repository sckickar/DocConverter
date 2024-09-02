using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Parsing;

internal class DeviceGray : Colorspace
{
	internal override int Components => 1;

	internal override Color GetColor(string[] pars)
	{
		return Colorspace.GetGrayColor(pars);
	}

	internal override Color GetColor(byte[] bytes, int offset)
	{
		return Colorspace.GetGrayColor(bytes, offset);
	}

	internal override PdfBrush GetBrush(string[] pars, PdfPageResources resource)
	{
		return new PdfPen(GetColor(pars)).Brush;
	}
}
