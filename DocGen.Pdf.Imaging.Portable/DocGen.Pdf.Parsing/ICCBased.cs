using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Parsing;

internal class ICCBased : Colorspace
{
	private ICCProfile m_profile;

	internal ICCProfile Profile
	{
		get
		{
			return m_profile;
		}
		set
		{
			m_profile = value;
		}
	}

	internal override int Components => Profile.AlternateColorspace.Components;

	internal override Color GetColor(string[] pars)
	{
		return Profile.AlternateColorspace.GetColor(pars);
	}

	internal override Color GetColor(byte[] bytes, int offset)
	{
		return Profile.AlternateColorspace.GetColor(bytes, offset);
	}

	internal override PdfBrush GetBrush(string[] pars, PdfPageResources resource)
	{
		return new PdfPen(GetColor(pars)).Brush;
	}
}
