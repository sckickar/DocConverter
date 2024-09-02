using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public abstract class PdfRectangleArea : PdfFillElement
{
	private RectangleF m_rect;

	public float X
	{
		get
		{
			return m_rect.X;
		}
		set
		{
			m_rect.X = value;
		}
	}

	public float Y
	{
		get
		{
			return m_rect.Y;
		}
		set
		{
			m_rect.Y = value;
		}
	}

	public float Width
	{
		get
		{
			return m_rect.Width;
		}
		set
		{
			m_rect.Width = value;
		}
	}

	public float Height
	{
		get
		{
			return m_rect.Height;
		}
		set
		{
			m_rect.Height = value;
		}
	}

	public SizeF Size
	{
		get
		{
			return m_rect.Size;
		}
		set
		{
			m_rect.Size = value;
		}
	}

	public RectangleF Bounds
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

	protected PdfRectangleArea()
	{
	}

	protected PdfRectangleArea(float x, float y, float width, float height)
		: this()
	{
		m_rect = new RectangleF(x, y, width, height);
	}

	protected PdfRectangleArea(RectangleF rectangle)
		: this()
	{
		m_rect = rectangle;
	}

	protected PdfRectangleArea(PdfPen pen, PdfBrush brush, float x, float y, float width, float height)
		: base(pen, brush)
	{
		m_rect = new RectangleF(x, y, width, height);
	}

	protected PdfRectangleArea(PdfPen pen, PdfBrush brush, RectangleF rectangle)
		: base(pen, brush)
	{
		m_rect = rectangle;
	}

	protected override RectangleF GetBoundsInternal()
	{
		return Bounds;
	}
}
