using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Lists;

public class PdfUnorderedMarker : PdfMarker
{
	private string m_text;

	private PdfUnorderedMarkerStyle m_style;

	private PdfImage m_image;

	private PdfTemplate m_template;

	private SizeF m_size;

	private PdfFont m_unicodeFont;

	public PdfTemplate Template
	{
		get
		{
			return m_template;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("template");
			}
			m_template = value;
			m_style = PdfUnorderedMarkerStyle.CustomTemplate;
		}
	}

	public PdfImage Image
	{
		get
		{
			return m_image;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("image");
			}
			m_image = value;
			m_style = PdfUnorderedMarkerStyle.CustomImage;
		}
	}

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
				throw new ArgumentNullException("text");
			}
			m_text = value;
			m_style = PdfUnorderedMarkerStyle.CustomString;
		}
	}

	public PdfUnorderedMarkerStyle Style
	{
		get
		{
			return m_style;
		}
		set
		{
			m_style = value;
		}
	}

	internal SizeF Size
	{
		get
		{
			return m_size;
		}
		set
		{
			m_size = value;
		}
	}

	internal PdfFont UnicodeFont
	{
		get
		{
			return m_unicodeFont;
		}
		set
		{
			m_unicodeFont = value;
		}
	}

	public PdfUnorderedMarker(string text, PdfFont font)
	{
		base.Font = font;
		Text = text;
		m_style = PdfUnorderedMarkerStyle.CustomString;
	}

	public PdfUnorderedMarker(PdfUnorderedMarkerStyle style)
	{
		m_style = style;
	}

	public PdfUnorderedMarker(PdfImage image)
	{
		Image = image;
		m_style = PdfUnorderedMarkerStyle.CustomImage;
	}

	public PdfUnorderedMarker(PdfTemplate template)
	{
		Template = template;
		m_style = PdfUnorderedMarkerStyle.CustomTemplate;
	}

	internal void Draw(PdfGraphics graphics, PointF point, PdfBrush brush, PdfPen pen)
	{
		PdfTemplate pdfTemplate = new PdfTemplate(m_size);
		pdfTemplate.Graphics.Tag = graphics.Tag;
		switch (m_style)
		{
		case PdfUnorderedMarkerStyle.CustomTemplate:
			pdfTemplate = new PdfTemplate(m_size);
			pdfTemplate.Graphics.DrawPdfTemplate(m_template, PointF.Empty, m_size);
			break;
		case PdfUnorderedMarkerStyle.CustomImage:
			pdfTemplate.Graphics.DrawImage(m_image, 1f, 1f, m_size.Width - 2f, m_size.Height - 2f);
			break;
		default:
		{
			PointF empty = PointF.Empty;
			if (pen != null)
			{
				empty.X += pen.Width;
				empty.Y += pen.Width;
			}
			pdfTemplate.Graphics.DrawString(GetStyledText(), m_unicodeFont, pen, brush, empty);
			break;
		}
		}
		graphics.DrawPdfTemplate(pdfTemplate, point);
	}

	internal void Draw(PdfPage page, PointF point, PdfBrush brush, PdfPen pen)
	{
		Draw(page.Graphics, point, brush, pen);
	}

	internal string GetStyledText()
	{
		string result = string.Empty;
		switch (m_style)
		{
		case PdfUnorderedMarkerStyle.Disk:
			result = "l";
			break;
		case PdfUnorderedMarkerStyle.Square:
			result = "n";
			break;
		case PdfUnorderedMarkerStyle.Asterisk:
			result = "]";
			break;
		case PdfUnorderedMarkerStyle.Circle:
			result = "m";
			break;
		}
		return result;
	}
}
