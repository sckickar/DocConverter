using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Lists;

internal class ListInfo
{
	private int m_index;

	private PdfList m_list;

	private string m_number;

	private PdfBrush m_brush;

	private PdfPen m_pen;

	private PdfFont m_font;

	private PdfStringFormat m_format;

	internal float MarkerWidth;

	internal int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	internal PdfList List
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

	internal string Number
	{
		get
		{
			return m_number;
		}
		set
		{
			m_number = value;
		}
	}

	internal PdfBrush Brush
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

	internal PdfPen Pen
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

	internal PdfFont Font
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

	internal PdfStringFormat Format
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

	internal ListInfo(PdfList list, int index, string number)
	{
		m_list = list;
		m_index = index;
		m_number = number;
	}

	internal ListInfo(PdfList list, int index)
		: this(list, index, string.Empty)
	{
	}
}
