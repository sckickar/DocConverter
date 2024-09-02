using System;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Tables;

public class PdfCellStyle
{
	private PdfFont m_font;

	private PdfStringFormat m_stringFormat;

	private PdfPen m_textPen;

	private PdfBrush m_textBrush;

	private PdfPen m_borderPen;

	private PdfBrush m_backgroundBrush;

	private PdfBorders m_borders;

	public PdfFont Font
	{
		get
		{
			if (m_font == null)
			{
				m_font = PdfDocument.DefaultFont;
			}
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

	public PdfPen TextPen
	{
		get
		{
			return m_textPen;
		}
		set
		{
			m_textPen = value;
		}
	}

	public PdfBrush TextBrush
	{
		get
		{
			return m_textBrush;
		}
		set
		{
			m_textBrush = value;
		}
	}

	public PdfPen BorderPen
	{
		get
		{
			return m_borderPen;
		}
		set
		{
			m_borderPen = value;
		}
	}

	public PdfBrush BackgroundBrush
	{
		get
		{
			return m_backgroundBrush;
		}
		set
		{
			m_backgroundBrush = value;
		}
	}

	internal PdfBorders Borders
	{
		get
		{
			return m_borders;
		}
		set
		{
			m_borders = value;
		}
	}

	public PdfCellStyle()
	{
		m_textBrush = PdfBrushes.Black;
		m_borderPen = PdfPens.Black;
	}

	public PdfCellStyle(PdfFont font, PdfBrush fontBrush, PdfPen borderPen)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (fontBrush == null)
		{
			throw new ArgumentNullException("fontBrush");
		}
		if (borderPen == null)
		{
			throw new ArgumentNullException("borderPen");
		}
		m_font = font;
		m_textBrush = fontBrush;
		m_borderPen = borderPen;
	}
}
