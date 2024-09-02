using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

public class PdfXfaRadialBrush : PdfXfaBrush
{
	private PdfColor m_startColor;

	private PdfColor m_endColor;

	private PdfXfaRadialType m_type;

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

	public PdfXfaRadialType Type
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

	public PdfXfaRadialBrush(PdfColor startColor, PdfColor endColor)
	{
		StartColor = startColor;
		EndColor = endColor;
	}

	public PdfXfaRadialBrush(PdfColor startColor, PdfColor endColor, PdfXfaRadialType type)
	{
		StartColor = startColor;
		EndColor = endColor;
		Type = type;
	}
}
