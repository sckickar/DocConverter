using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Lists;

public abstract class PdfMarker
{
	private PdfFont m_font;

	private PdfBrush m_brush;

	private PdfPen m_pen;

	private PdfStringFormat m_format;

	private PdfListMarkerAlignment m_alignment;

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

	public PdfBrush Brush
	{
		get
		{
			return m_brush;
		}
		set
		{
			m_brush = value;
		}
	}

	public PdfPen Pen
	{
		get
		{
			return m_pen;
		}
		set
		{
			m_pen = value;
		}
	}

	public PdfStringFormat StringFormat
	{
		get
		{
			return m_format;
		}
		set
		{
			m_format = value;
		}
	}

	public PdfListMarkerAlignment Alignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			m_alignment = value;
		}
	}

	internal bool RightToLeft => m_alignment == PdfListMarkerAlignment.Right;
}
