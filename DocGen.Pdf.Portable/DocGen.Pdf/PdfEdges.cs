namespace DocGen.Pdf;

public class PdfEdges
{
	private int m_left;

	private int m_right;

	private int m_top;

	private int m_bottom;

	public int Left
	{
		get
		{
			return m_left;
		}
		set
		{
			m_left = value;
		}
	}

	public int Right
	{
		get
		{
			return m_right;
		}
		set
		{
			m_right = value;
		}
	}

	public int Top
	{
		get
		{
			return m_top;
		}
		set
		{
			m_top = value;
		}
	}

	public int Bottom
	{
		get
		{
			return m_bottom;
		}
		set
		{
			m_bottom = value;
		}
	}

	public int All
	{
		set
		{
			m_left = (m_right = (m_top = (m_bottom = value)));
		}
	}

	internal bool IsAll
	{
		get
		{
			if (m_left == m_right && m_left == m_top)
			{
				return m_left == m_bottom;
			}
			return false;
		}
	}

	public PdfEdges()
	{
	}

	public PdfEdges(int left, int right, int top, int bottom)
	{
		m_left = left;
		m_right = right;
		m_top = top;
		m_bottom = bottom;
	}
}
