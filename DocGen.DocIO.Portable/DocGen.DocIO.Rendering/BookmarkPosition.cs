using DocGen.Drawing;

namespace DocGen.DocIO.Rendering;

internal class BookmarkPosition
{
	private RectangleF m_bounds;

	private int m_pageNumber;

	private int m_bookmarkStyle;

	private string m_bookmarkName;

	internal int BookmarkStyle
	{
		get
		{
			return m_bookmarkStyle;
		}
		set
		{
			m_bookmarkStyle = value;
		}
	}

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

	public int PageNumber
	{
		get
		{
			return m_pageNumber;
		}
		set
		{
			m_pageNumber = value;
		}
	}

	public string BookmarkName
	{
		get
		{
			return m_bookmarkName;
		}
		set
		{
			m_bookmarkName = value;
		}
	}

	public BookmarkPosition(string bookmarkName, int pageNumber, RectangleF bounds)
	{
		BookmarkName = bookmarkName;
		PageNumber = pageNumber;
		Bounds = bounds;
	}

	internal BookmarkPosition(string bookmarkName, int pageNumber, RectangleF bounds, int level)
		: this(bookmarkName, pageNumber, bounds)
	{
		m_bookmarkStyle = level + 1;
	}
}
