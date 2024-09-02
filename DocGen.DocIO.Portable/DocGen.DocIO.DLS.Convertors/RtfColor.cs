namespace DocGen.DocIO.DLS.Convertors;

internal class RtfColor
{
	private int m_redN;

	private int m_greenN;

	private int m_blueN;

	internal int RedN
	{
		get
		{
			return m_redN;
		}
		set
		{
			m_redN = value;
		}
	}

	internal int GreenN
	{
		get
		{
			return m_greenN;
		}
		set
		{
			m_greenN = value;
		}
	}

	internal int BlueN
	{
		get
		{
			return m_blueN;
		}
		set
		{
			m_blueN = value;
		}
	}
}
