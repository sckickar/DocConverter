using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public abstract class PdfMultipleNumberValueField : PdfMultipleValueField
{
	private PdfNumberStyle m_numberStyle = PdfNumberStyle.Numeric;

	public PdfNumberStyle NumberStyle
	{
		get
		{
			return m_numberStyle;
		}
		set
		{
			m_numberStyle = value;
		}
	}

	public PdfMultipleNumberValueField()
	{
	}

	public PdfMultipleNumberValueField(PdfFont font)
		: base(font)
	{
	}

	public PdfMultipleNumberValueField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfMultipleNumberValueField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}
}
