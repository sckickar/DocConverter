using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

internal class WidgetBorder : IPdfWrapper
{
	private float m_width = 1f;

	private PdfBorderStyle m_style;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
			int num = (int)m_width;
			if (m_width - (float)num == 0f)
			{
				m_dictionary.SetNumber("W", num);
			}
			else
			{
				m_dictionary.SetNumber("W", m_width);
			}
		}
	}

	public PdfBorderStyle Style
	{
		get
		{
			return m_style;
		}
		set
		{
			m_style = value;
			m_dictionary.SetName("S", StyleToString(m_style));
		}
	}

	public IPdfPrimitive Element => m_dictionary;

	public WidgetBorder()
	{
		m_dictionary.SetProperty("Type", new PdfName("Border"));
		m_dictionary.SetName("S", StyleToString(m_style));
	}

	private string StyleToString(PdfBorderStyle style)
	{
		return style switch
		{
			PdfBorderStyle.Beveled => "B", 
			PdfBorderStyle.Dashed => "D", 
			PdfBorderStyle.Inset => "I", 
			PdfBorderStyle.Underline => "U", 
			_ => "S", 
		};
	}
}
