using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public abstract class PdfAutomaticField : PdfGraphicsElement
{
	private RectangleF m_bounds = RectangleF.Empty;

	private PdfFont m_font;

	private PdfBrush m_brush;

	private PdfPen m_pen;

	private PdfStringFormat m_stringFormat;

	private SizeF m_templateSize = SizeF.Empty;

	public RectangleF Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	public SizeF Size
	{
		get
		{
			return m_bounds.Size;
		}
		set
		{
			m_bounds.Size = value;
		}
	}

	public PointF Location
	{
		get
		{
			return m_bounds.Location;
		}
		set
		{
			m_bounds.Location = value;
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
			if (value == null)
			{
				throw new ArgumentNullException("Font");
			}
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
			if (value == null)
			{
				throw new ArgumentNullException("Brush");
			}
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
			return m_stringFormat;
		}
		set
		{
			m_stringFormat = value;
		}
	}

	protected PdfAutomaticField()
	{
	}

	protected PdfAutomaticField(PdfFont font)
	{
		Font = font;
	}

	protected PdfAutomaticField(PdfFont font, PdfBrush brush)
	{
		Font = font;
		Brush = brush;
	}

	protected PdfAutomaticField(PdfFont font, RectangleF bounds)
	{
		Font = font;
		Bounds = bounds;
	}

	public override void Draw(PdfGraphics graphics, float x, float y)
	{
		base.Draw(graphics, x, y);
		graphics.AutomaticFields.Add(new PdfAutomaticFieldInfo(this, new PointF(x, y)));
	}

	protected internal abstract string GetValue(PdfGraphics graphics);

	protected internal virtual void PerformDraw(PdfGraphics graphics, PointF location, float scalingX, float scalingY)
	{
		if (Bounds.Height == 0f || Bounds.Width == 0f)
		{
			string value = GetValue(graphics);
			m_templateSize = ObtainFont().MeasureString(value, Size, StringFormat);
		}
	}

	protected SizeF ObtainSize()
	{
		if (Bounds.Height == 0f || Bounds.Width == 0f)
		{
			return m_templateSize;
		}
		return Size;
	}

	protected override void DrawInternal(PdfGraphics graphics)
	{
	}

	protected PdfBrush ObtainBrush()
	{
		if (m_brush != null)
		{
			return m_brush;
		}
		return PdfBrushes.Black;
	}

	protected PdfFont ObtainFont()
	{
		if (m_font != null)
		{
			return m_font;
		}
		return PdfDocument.DefaultFont;
	}
}
