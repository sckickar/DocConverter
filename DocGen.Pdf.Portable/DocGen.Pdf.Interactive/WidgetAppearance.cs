using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

internal class WidgetAppearance : IPdfWrapper
{
	private PdfColor m_borderColor = new PdfColor(0, 0, 0);

	private PdfColor m_backColor = new PdfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private string m_normalCaption = string.Empty;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private int m_rotationAngle;

	internal int RotationAngle
	{
		get
		{
			return m_rotationAngle;
		}
		set
		{
			m_rotationAngle = value;
			m_dictionary.SetProperty("R", new PdfNumber(m_rotationAngle));
		}
	}

	public PdfColor BorderColor
	{
		get
		{
			return m_borderColor;
		}
		set
		{
			if (m_borderColor != value)
			{
				m_borderColor = value;
				if (value.A == 0)
				{
					m_dictionary.Remove("BC");
				}
				else
				{
					m_dictionary.SetProperty("BC", m_borderColor.ToArray());
				}
			}
		}
	}

	public PdfColor BackColor
	{
		get
		{
			return m_backColor;
		}
		set
		{
			if (m_backColor != value)
			{
				m_backColor = value;
				if (m_backColor.A == 0)
				{
					m_dictionary.Remove("BG");
				}
				else
				{
					m_dictionary.SetProperty("BG", m_backColor.ToArray());
				}
			}
		}
	}

	public string NormalCaption
	{
		get
		{
			return m_normalCaption;
		}
		set
		{
			if (m_normalCaption != value)
			{
				m_normalCaption = value;
				m_dictionary.SetString("CA", m_normalCaption);
			}
		}
	}

	public IPdfPrimitive Element => m_dictionary;

	public WidgetAppearance()
	{
		m_dictionary.SetProperty("BC", m_borderColor.ToArray());
		m_dictionary.SetProperty("BG", m_backColor.ToArray());
	}
}
