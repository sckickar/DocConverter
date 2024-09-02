namespace DocGen.Chart;

internal sealed class BspNode
{
	private Polygon m_plane;

	private BspNode m_front;

	private BspNode m_back;

	public BspNode Back
	{
		get
		{
			return m_back;
		}
		set
		{
			m_back = value;
		}
	}

	public BspNode Front
	{
		get
		{
			return m_front;
		}
		set
		{
			m_front = value;
		}
	}

	public Polygon Plane
	{
		get
		{
			return m_plane;
		}
		set
		{
			m_plane = value;
		}
	}
}
