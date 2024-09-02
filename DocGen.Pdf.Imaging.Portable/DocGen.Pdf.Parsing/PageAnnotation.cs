using System.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class PageAnnotation
{
	private RectangleF m_rect;

	private string m_uri = string.Empty;

	private float m_border = 1f;

	private string m_annotType;

	private PdfArray m_pageAnnotDestinations;

	internal RectangleF Rect
	{
		get
		{
			return m_rect;
		}
		set
		{
			m_rect = value;
		}
	}

	internal string URI
	{
		get
		{
			return m_uri;
		}
		set
		{
			m_uri = value;
		}
	}

	internal float Border
	{
		get
		{
			return m_border;
		}
		set
		{
			m_border = value;
		}
	}

	internal string AnnotType
	{
		get
		{
			return m_annotType;
		}
		set
		{
			m_annotType = value;
		}
	}

	internal PdfArray PageAnnotDestinations
	{
		get
		{
			return m_pageAnnotDestinations;
		}
		set
		{
			m_pageAnnotDestinations = value;
		}
	}

	public PageAnnotation(RectangleF rec, string uri, float border, string annotType)
	{
		m_rect = rec;
		m_uri = uri;
		m_border = border;
		m_annotType = annotType;
	}

	public PageAnnotation(RectangleF rec, string uri, float border, string annotType, PdfArray pageAnnotDestinations)
	{
		m_rect = rec;
		m_uri = uri;
		m_border = border;
		m_annotType = annotType;
		m_pageAnnotDestinations = pageAnnotDestinations;
	}
}
