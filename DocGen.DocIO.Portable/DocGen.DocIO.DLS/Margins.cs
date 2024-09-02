namespace DocGen.DocIO.DLS;

public sealed class Margins
{
	private int m_iLeft;

	private int m_iRight;

	private int m_iTop;

	private int m_iBottom;

	public int All
	{
		get
		{
			if (!IsAll)
			{
				return 0;
			}
			return m_iLeft;
		}
		set
		{
			if (!IsAll && m_iLeft != value)
			{
				m_iLeft = (m_iRight = (m_iTop = (m_iBottom = value)));
			}
		}
	}

	public int Left
	{
		get
		{
			return m_iLeft;
		}
		set
		{
			if (value != m_iLeft)
			{
				m_iLeft = value;
			}
		}
	}

	public int Right
	{
		get
		{
			return m_iRight;
		}
		set
		{
			if (value != m_iRight)
			{
				m_iRight = value;
			}
		}
	}

	public int Top
	{
		get
		{
			return m_iTop;
		}
		set
		{
			if (value != m_iTop)
			{
				m_iTop = value;
			}
		}
	}

	public int Bottom
	{
		get
		{
			return m_iBottom;
		}
		set
		{
			if (value != m_iBottom)
			{
				m_iBottom = value;
			}
		}
	}

	private bool IsAll
	{
		get
		{
			if (m_iLeft == m_iRight && m_iRight == m_iTop)
			{
				return m_iTop == m_iBottom;
			}
			return false;
		}
	}

	public Margins()
	{
	}

	public Margins(int left, int top, int right, int bottom)
	{
		m_iLeft = left;
		m_iTop = top;
		m_iRight = right;
		m_iBottom = bottom;
	}
}
