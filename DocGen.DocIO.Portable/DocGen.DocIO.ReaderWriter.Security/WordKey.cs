namespace DocGen.DocIO.ReaderWriter.Security;

internal class WordKey
{
	private byte[] m_baState = new byte[256];

	private byte m_bX;

	private byte m_bY;

	internal byte[] status
	{
		get
		{
			return m_baState;
		}
		set
		{
			if (m_baState != value)
			{
				m_baState = value;
			}
		}
	}

	internal byte x
	{
		get
		{
			return m_bX;
		}
		set
		{
			if (m_bX != value)
			{
				m_bX = value;
			}
		}
	}

	internal byte y
	{
		get
		{
			return m_bY;
		}
		set
		{
			if (m_bY != value)
			{
				m_bY = value;
			}
		}
	}
}
