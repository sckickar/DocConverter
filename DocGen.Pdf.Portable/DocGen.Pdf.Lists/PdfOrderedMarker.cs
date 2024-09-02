using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Lists;

public class PdfOrderedMarker : PdfMarker
{
	private PdfNumberStyle m_style;

	private int m_startNumber = 1;

	private string m_delimiter;

	private string m_suffix;

	private int m_currentIndex;

	public PdfNumberStyle Style
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

	public int StartNumber
	{
		get
		{
			return m_startNumber;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentException("Start number should be greater than 0");
			}
			m_startNumber = value;
		}
	}

	public string Delimiter
	{
		get
		{
			if (m_delimiter == string.Empty || m_delimiter == null)
			{
				return ".";
			}
			return m_delimiter;
		}
		set
		{
			m_delimiter = value;
		}
	}

	public string Suffix
	{
		get
		{
			if (m_suffix == null || m_suffix == string.Empty)
			{
				return ".";
			}
			return m_suffix;
		}
		set
		{
			m_suffix = value;
		}
	}

	internal int CurrentIndex
	{
		get
		{
			return m_currentIndex;
		}
		set
		{
			m_currentIndex = value;
		}
	}

	public PdfOrderedMarker(PdfNumberStyle style, string delimiter, string suffix, PdfFont font)
	{
		m_style = style;
		m_delimiter = delimiter;
		m_suffix = suffix;
		base.Font = font;
	}

	public PdfOrderedMarker(PdfNumberStyle style, string suffix, PdfFont font)
		: this(style, string.Empty, suffix, font)
	{
	}

	public PdfOrderedMarker(PdfNumberStyle style, PdfFont font)
		: this(style, string.Empty, string.Empty, font)
	{
	}

	internal void Draw(PdfGraphics graphics, PointF point)
	{
		string number = GetNumber();
		graphics.DrawString(number + Suffix, base.Font, base.Brush, point);
	}

	internal void Draw(PdfPage page, PointF point)
	{
		Draw(page.Graphics, point);
	}

	internal string GetNumber()
	{
		return PdfNumbersConvertor.Convert(m_startNumber + m_currentIndex, m_style);
	}
}
