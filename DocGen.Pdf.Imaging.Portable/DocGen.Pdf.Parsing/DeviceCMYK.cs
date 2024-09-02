using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Parsing;

internal class DeviceCMYK : Colorspace
{
	internal override int Components => 4;

	internal override Color GetColor(string[] pars)
	{
		return Colorspace.GetCmykColor(pars);
	}

	internal override Color GetColor(byte[] bytes, int offset)
	{
		return Colorspace.GetCmykColor(bytes, offset);
	}

	internal override PdfBrush GetBrush(string[] pars, PdfPageResources resource)
	{
		return new PdfPen(GetColor(pars)).Brush;
	}
}
