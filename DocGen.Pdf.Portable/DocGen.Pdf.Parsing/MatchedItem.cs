using DocGen.Drawing;

namespace DocGen.Pdf.Parsing;

public class MatchedItem
{
	private string m_text;

	private int m_pageNumber;

	private RectangleF m_boundingBox;

	private Color m_textColor;

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	internal Color TextColor
	{
		get
		{
			return m_textColor;
		}
		set
		{
			m_textColor = value;
		}
	}

	internal int PageNumber
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

	public RectangleF Bounds
	{
		get
		{
			return m_boundingBox;
		}
		set
		{
			m_boundingBox = value;
		}
	}
}
