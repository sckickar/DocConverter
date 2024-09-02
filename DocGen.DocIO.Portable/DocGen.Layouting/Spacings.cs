namespace DocGen.Layouting;

internal class Spacings
{
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
}
