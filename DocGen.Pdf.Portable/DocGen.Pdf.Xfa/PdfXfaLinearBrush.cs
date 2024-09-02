using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

public class PdfXfaLinearBrush : PdfXfaBrush
{
	private PdfColor m_startColor;

	private PdfColor m_endColor;

	private PdfXfaLinearType m_type;

	public PdfColor StartColor
	{
		get
		{
			return m_startColor;
		}
		set
		{
			m_startColor = value;
		}
	}

	public PdfColor EndColor
	{
		get
		{
			return m_endColor;
		}
		set
		{
			m_endColor = value;
		}
	}

	public PdfXfaLinearType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	public PdfXfaLinearBrush(PdfColor startColor, PdfColor endColor)
	{
		StartColor = startColor;
		EndColor = endColor;
	}

	public PdfXfaLinearBrush(PdfColor startColor, PdfColor endColor, PdfXfaLinearType type)
	{
		StartColor = startColor;
		EndColor = endColor;
		Type = type;
	}
}
