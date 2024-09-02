using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfExtendedAppearance : IPdfWrapper
{
	private PdfAppearanceState m_normal;

	private PdfAppearanceState m_pressed;

	private PdfAppearanceState m_mouseHover;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public PdfAppearanceState Normal
	{
		get
		{
			if (m_normal == null)
			{
				m_normal = new PdfAppearanceState();
				m_dictionary.SetProperty("N", new PdfReferenceHolder(m_normal));
			}
			return m_normal;
		}
	}

	public PdfAppearanceState MouseHover
	{
		get
		{
			if (m_mouseHover == null)
			{
				m_mouseHover = new PdfAppearanceState();
				m_dictionary.SetProperty("R", new PdfReferenceHolder(m_mouseHover));
			}
			return m_mouseHover;
		}
	}

	public PdfAppearanceState Pressed
	{
		get
		{
			if (m_pressed == null)
			{
				m_pressed = new PdfAppearanceState();
				m_dictionary.SetProperty("D", new PdfReferenceHolder(m_pressed));
			}
			return m_pressed;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;
}
