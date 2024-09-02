namespace DocGen.Pdf;

public class PdfPaddings
{
	private float m_left;

	private float m_right;

	private float m_top;

	private float m_bottom;

	public float Left
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

	public float Right
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

	public float Top
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

	public float Bottom
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

	public float All
	{
		set
		{
			m_left = (m_right = (m_top = (m_bottom = value)));
		}
	}

	public PdfPaddings()
	{
		m_left = (m_right = (m_top = (m_bottom = 0.5f)));
	}

	public PdfPaddings(float left, float right, float top, float bottom)
	{
		m_left = left;
		m_right = right;
		m_top = top;
		m_bottom = bottom;
	}
}
