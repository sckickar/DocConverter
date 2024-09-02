using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Redaction;

public class PdfRedaction
{
	private RectangleF m_bounds;

	private Color m_fillColor = Color.Transparent;

	private PdfTemplate m_appearance;

	internal bool m_success;

	internal bool AppearanceEnabled;

	internal PdfLoadedPage page;

	internal bool PathRedaction;

	private bool textOnly;

	public Color FillColor
	{
		get
		{
			return m_fillColor;
		}
		set
		{
			m_fillColor = value;
		}
	}

	internal RectangleF Bounds => m_bounds;

	public bool TextOnly
	{
		get
		{
			return textOnly;
		}
		set
		{
			textOnly = value;
		}
	}

	internal bool Success
	{
		get
		{
			return m_success;
		}
		set
		{
			m_success = value;
		}
	}

	public PdfTemplate Appearance
	{
		get
		{
			if (m_appearance == null)
			{
				m_appearance = new PdfTemplate(m_bounds.Width, m_bounds.Height);
				AppearanceEnabled = true;
			}
			return m_appearance;
		}
		internal set
		{
			m_appearance = value;
			AppearanceEnabled = true;
		}
	}

	public PdfRedaction(RectangleF bounds)
	{
		m_bounds = bounds;
	}

	public PdfRedaction(RectangleF bounds, Color fillColor)
	{
		m_bounds = bounds;
		m_fillColor = fillColor;
	}
}
