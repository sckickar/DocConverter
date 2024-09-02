using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public class PdfDateTimeField : PdfStaticField
{
	private DateTime m_date = DateTime.Now;

	private string m_formatString = "dd'/'MM'/'yyyy hh':'mm':'ss";

	public string DateFormatString
	{
		get
		{
			return m_formatString;
		}
		set
		{
			m_formatString = value;
		}
	}

	public PdfDateTimeField()
	{
	}

	public PdfDateTimeField(PdfFont font)
		: base(font)
	{
	}

	public PdfDateTimeField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfDateTimeField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}

	protected internal override string GetValue(PdfGraphics graphics)
	{
		return m_date.ToString(m_formatString);
	}
}
