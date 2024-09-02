using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class LineBorder : IPdfWrapper
{
	private float m_borderLineWidth = 1f;

	private int m_borderWidth = 1;

	private int m_dashArray;

	private PdfBorderStyle m_borderStyle;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public int BorderWidth
	{
		get
		{
			return m_borderWidth;
		}
		set
		{
			m_borderWidth = value;
			m_dictionary.SetNumber("W", m_borderWidth);
		}
	}

	internal float BorderLineWidth
	{
		get
		{
			return m_borderLineWidth;
		}
		set
		{
			m_borderLineWidth = value;
			m_dictionary.SetNumber("W", m_borderLineWidth);
		}
	}

	public PdfBorderStyle BorderStyle
	{
		get
		{
			return m_borderStyle;
		}
		set
		{
			m_borderStyle = value;
			m_dictionary.SetName("S", StyleToString(m_borderStyle));
		}
	}

	public int DashArray
	{
		get
		{
			return m_dashArray;
		}
		set
		{
			m_dashArray = value;
			PdfArray pdfArray = new PdfArray();
			pdfArray.Insert(0, new PdfNumber(m_dashArray));
			pdfArray.Insert(1, new PdfNumber(m_dashArray));
			m_dictionary.SetProperty("D", pdfArray);
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public LineBorder()
	{
		m_dictionary.SetProperty("Type", new PdfName("Border"));
	}

	private string StyleToString(PdfBorderStyle style)
	{
		switch (style)
		{
		default:
			return "S";
		case PdfBorderStyle.Beveled:
			return "B";
		case PdfBorderStyle.Dashed:
		case PdfBorderStyle.Dot:
			return "D";
		case PdfBorderStyle.Inset:
			return "I";
		case PdfBorderStyle.Underline:
			return "U";
		}
	}
}
