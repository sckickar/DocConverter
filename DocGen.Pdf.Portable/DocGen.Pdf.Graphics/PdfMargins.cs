namespace DocGen.Pdf.Graphics;

public class PdfMargins : ICloneable
{
	private const float PageMargin = 0f;

	private float m_left;

	private float m_top;

	private float m_right;

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
			SetMargins(value);
		}
	}

	public PdfMargins()
	{
		SetMargins(0f);
	}

	internal void SetMargins(float margin)
	{
		m_left = (m_top = (m_right = (m_bottom = margin)));
	}

	internal void SetMargins(float leftRight, float topBottom)
	{
		m_left = (m_right = leftRight);
		m_top = (m_bottom = topBottom);
	}

	internal void SetMargins(float left, float top, float right, float bottom)
	{
		m_left = left;
		m_top = top;
		m_right = right;
		m_bottom = bottom;
	}

	public object Clone()
	{
		return (PdfMargins)MemberwiseClone();
	}
}
