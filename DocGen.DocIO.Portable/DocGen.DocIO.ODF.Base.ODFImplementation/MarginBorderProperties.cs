namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class MarginBorderProperties : BorderProperties
{
	private double m_marginLeft;

	private double m_marginRight;

	private double m_marginTop;

	private double m_marginBottom;

	internal byte m_marginFlag;

	private const byte MarginLeftKey = 0;

	private const byte MarginRightKey = 1;

	private const byte MarginTopKey = 2;

	private const byte MarginBottomKey = 3;

	internal double MarginLeft
	{
		get
		{
			return m_marginLeft;
		}
		set
		{
			m_marginFlag = (byte)((m_marginFlag & 0xFEu) | 1u);
			m_marginLeft = value;
		}
	}

	internal double MarginRight
	{
		get
		{
			return m_marginRight;
		}
		set
		{
			m_marginFlag = (byte)((m_marginFlag & 0xFDu) | 2u);
			m_marginRight = value;
		}
	}

	internal double MarginTop
	{
		get
		{
			return m_marginTop;
		}
		set
		{
			m_marginFlag = (byte)((m_marginFlag & 0xFBu) | 4u);
			m_marginTop = value;
		}
	}

	internal double MarginBottom
	{
		get
		{
			return m_marginBottom;
		}
		set
		{
			m_marginFlag = (byte)((m_marginFlag & 0xF7u) | 8u);
			m_marginBottom = value;
		}
	}
}
