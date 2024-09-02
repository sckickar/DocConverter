using System;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Lists;

public class PdfListItem
{
	private PdfFont m_font;

	private string m_text;

	private PdfStringFormat m_format;

	private PdfPen m_pen;

	private PdfBrush m_brush;

	private PdfList m_list;

	private float m_textIndent;

	private PdfTag m_tag;

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

	public PdfList SubList
	{
		get
		{
			return m_list;
		}
		set
		{
			m_list = value;
		}
	}

	public float TextIndent
	{
		get
		{
			return m_textIndent;
		}
		set
		{
			m_textIndent = value;
		}
	}

	public PdfTag PdfTag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	public PdfListItem()
		: this(string.Empty)
	{
	}

	public PdfListItem(string text)
		: this(text, null, null, null, null)
	{
	}

	public PdfListItem(string text, PdfFont font)
		: this(text, font, null, null, null)
	{
	}

	public PdfListItem(string text, PdfFont font, PdfStringFormat format)
		: this(text, font, format, null, null)
	{
	}

	public PdfListItem(string text, PdfFont font, PdfStringFormat format, PdfPen pen, PdfBrush brush)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		m_text = text;
		m_font = font;
		m_format = format;
		m_pen = pen;
		m_brush = brush;
	}
}
