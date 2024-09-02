using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.HtmlToPdf;

public class HtmlToPdfTocStyle
{
	private PdfBrush m_backgroundColor;

	private PdfFont m_font;

	private PdfBrush m_foreColor;

	private PdfPaddings m_Padding;

	public PdfBrush BackgroundColor
	{
		get
		{
			return m_backgroundColor;
		}
		set
		{
			m_backgroundColor = value;
		}
	}

	public PdfFont Font
	{
		get
		{
			return m_font;
		}
		set
		{
			m_font = value;
		}
	}

	public PdfBrush ForeColor
	{
		get
		{
			return m_foreColor;
		}
		set
		{
			m_foreColor = value;
		}
	}

	public PdfPaddings Padding
	{
		get
		{
			return m_Padding;
		}
		set
		{
			m_Padding = value;
		}
	}
}
