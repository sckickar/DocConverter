using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfAnnotationActions : IPdfWrapper
{
	private PdfAction m_mouseEnter;

	private PdfAction m_mouseLeave;

	private PdfAction m_mouseDown;

	private PdfAction m_mouseUp;

	private PdfAction m_gotFocus;

	private PdfAction m_lostFocus;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public PdfAction MouseEnter
	{
		get
		{
			return m_mouseEnter;
		}
		set
		{
			if (m_mouseEnter != value)
			{
				m_mouseEnter = value;
				m_dictionary.SetProperty("E", m_mouseEnter);
			}
		}
	}

	public PdfAction MouseLeave
	{
		get
		{
			return m_mouseLeave;
		}
		set
		{
			if (m_mouseLeave != value)
			{
				m_mouseLeave = value;
				m_dictionary.SetProperty("X", m_mouseLeave);
			}
		}
	}

	public PdfAction MouseDown
	{
		get
		{
			return m_mouseDown;
		}
		set
		{
			if (m_mouseDown != value)
			{
				m_mouseDown = value;
				m_dictionary.SetProperty("D", m_mouseDown);
			}
		}
	}

	public PdfAction MouseUp
	{
		get
		{
			return m_mouseUp;
		}
		set
		{
			if (m_mouseUp != value)
			{
				m_mouseUp = value;
				m_dictionary.SetProperty("U", m_mouseUp);
			}
		}
	}

	public PdfAction GotFocus
	{
		get
		{
			return m_gotFocus;
		}
		set
		{
			if (m_gotFocus != value)
			{
				m_gotFocus = value;
				m_dictionary.SetProperty("Fo", m_gotFocus);
			}
		}
	}

	public PdfAction LostFocus
	{
		get
		{
			return m_lostFocus;
		}
		set
		{
			if (m_lostFocus != value)
			{
				m_lostFocus = value;
				m_dictionary.SetProperty("Bl", m_lostFocus);
			}
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;
}
