using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfTextElement : PdfLayoutElement
{
	private string m_text = string.Empty;

	private string m_value = string.Empty;

	private PdfPen m_pen;

	private PdfBrush m_brush;

	private PdfFont m_font;

	private PdfStringFormat m_format;

	internal bool ispdfTextElement;

	internal bool m_pdfHtmlTextElement;

	internal bool m_isPdfGrid;

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Text");
			}
			if (m_font != null && m_font is PdfStandardFont && (m_font as PdfStandardFont).fontEncoding != null)
			{
				m_value = PdfStandardFont.Convert(value, (m_font as PdfStandardFont).fontEncoding);
			}
			else if (m_font == null || m_font is PdfStandardFont)
			{
				m_value = PdfStandardFont.Convert(value);
			}
			else
			{
				m_value = value;
			}
			m_text = value;
		}
	}

	internal string Value => m_value;

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
			if (m_font is PdfStandardFont && (m_font as PdfStandardFont).fontEncoding != null && m_text != null)
			{
				m_value = PdfStandardFont.Convert(m_text, (m_font as PdfStandardFont).fontEncoding);
			}
			else if (m_font is PdfStandardFont && m_text != null)
			{
				m_value = PdfStandardFont.Convert(m_text);
			}
			else
			{
				m_value = m_text;
			}
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

	public PdfTextElement()
	{
	}

	public PdfTextElement(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		m_text = text;
		text = PdfStandardFont.Convert(text);
		m_value = text;
	}

	public PdfTextElement(string text, PdfFont font)
		: this(text)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		m_font = font;
		if (m_font is PdfStandardFont && (m_font as PdfStandardFont).fontEncoding != null)
		{
			m_value = PdfStandardFont.Convert(m_text, (m_font as PdfStandardFont).fontEncoding);
		}
		else if (m_font is PdfStandardFont)
		{
			m_value = PdfStandardFont.Convert(m_text);
		}
		else
		{
			m_value = m_text;
		}
	}

	public PdfTextElement(string text, PdfFont font, PdfPen pen)
		: this(text, font)
	{
		m_pen = pen;
	}

	public PdfTextElement(string text, PdfFont font, PdfBrush brush)
		: this(text, font)
	{
		m_brush = brush;
	}

	public PdfTextElement(string text, PdfFont font, PdfPen pen, PdfBrush brush, PdfStringFormat format)
		: this(text, font, pen)
	{
		m_brush = brush;
		m_format = format;
	}

	public new PdfTextLayoutResult Draw(PdfPage page, PointF location, PdfLayoutFormat format)
	{
		RectangleF layoutRectangle = new RectangleF(location, SizeF.Empty);
		return Draw(page, layoutRectangle, format);
	}

	public PdfTextLayoutResult Draw(PdfPage page, PointF location, float width, PdfLayoutFormat format)
	{
		RectangleF layoutRectangle = new RectangleF(location.X, location.Y, width, 0f);
		return Draw(page, layoutRectangle, format);
	}

	public new PdfTextLayoutResult Draw(PdfPage page, RectangleF layoutRectangle, PdfLayoutFormat format)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfLayoutParams pdfLayoutParams = new PdfLayoutParams();
		pdfLayoutParams.Page = page;
		pdfLayoutParams.Bounds = layoutRectangle;
		pdfLayoutParams.Format = ((format != null) ? format : new PdfLayoutFormat());
		return Layout(pdfLayoutParams) as PdfTextLayoutResult;
	}

	internal PdfBrush ObtainBrush()
	{
		if (m_brush != null)
		{
			return m_brush;
		}
		return PdfBrushes.Black;
	}

	protected override void DrawInternal(PdfGraphics graphics)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		if (Font == null)
		{
			throw new ArgumentNullException("Font can't be null");
		}
		if (base.PdfTag != null)
		{
			graphics.Tag = base.PdfTag;
		}
		graphics.DrawString(Value, Font, Pen, ObtainBrush(), PointF.Empty, StringFormat);
	}

	protected override PdfLayoutResult Layout(PdfLayoutParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		if (Font == null)
		{
			throw new ArgumentNullException("Font can't be null");
		}
		return (PdfTextLayoutResult)new TextLayouter(this).Layout(param);
	}
}
