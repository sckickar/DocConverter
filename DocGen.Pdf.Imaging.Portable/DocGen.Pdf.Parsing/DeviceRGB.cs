using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Parsing;

internal class DeviceRGB : Colorspace
{
	internal override int Components => 3;

	internal override Color GetColor(string[] pars)
	{
		return Colorspace.GetRgbColor(pars);
	}

	internal override Color GetColor(byte[] bytes, int offset)
	{
		return Colorspace.GetRgbColor(bytes, offset);
	}

	internal override PdfBrush GetBrush(string[] pars, PdfPageResources resource)
	{
		return new PdfPen(GetColor(pars)).Brush;
	}
}
