namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class CommonTableParaProperties : MarginBorderProperties
{
	private string m_backgroundColor;

	private KeepTogether m_keepWithNext;

	private int m_pageNumber;

	private AfterBreak m_afterBreak;

	private BeforeBreak m_beforeBreak;

	private string m_shadowType;

	private WritingMode m_writingMode;

	internal byte m_CommonstyleFlags;

	private const byte WritingModeKey = 0;

	private const byte BeforeBreakKey = 1;

	private const byte AfterBreakKey = 2;

	private const byte KeepWithNextKey = 3;

	private const byte ShadowTypeKey = 4;

	private const byte PageNumberKey = 5;

	private const byte BackgroundColorKey = 6;

	internal WritingMode WritingMode
	{
		get
		{
			return m_writingMode;
		}
		set
		{
			m_CommonstyleFlags = (byte)((m_CommonstyleFlags & 0xFEu) | 1u);
			m_writingMode = value;
		}
	}

	internal BeforeBreak BeforeBreak
	{
		get
		{
			return m_beforeBreak;
		}
		set
		{
			m_CommonstyleFlags = (byte)((m_CommonstyleFlags & 0xFDu) | 2u);
			m_beforeBreak = value;
		}
	}

	internal AfterBreak AfterBreak
	{
		get
		{
			return m_afterBreak;
		}
		set
		{
			m_CommonstyleFlags = (byte)((m_CommonstyleFlags & 0xFBu) | 4u);
			m_afterBreak = value;
		}
	}

	internal KeepTogether KeepWithNext
	{
		get
		{
			return m_keepWithNext;
		}
		set
		{
			m_CommonstyleFlags = (byte)((m_CommonstyleFlags & 0xF7u) | 8u);
			m_keepWithNext = value;
		}
	}

	internal string ShadowType
	{
		get
		{
			return m_shadowType;
		}
		set
		{
			m_CommonstyleFlags = (byte)((m_CommonstyleFlags & 0xEFu) | 0x10u);
			m_shadowType = value;
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
			m_CommonstyleFlags = (byte)((m_CommonstyleFlags & 0xDFu) | 0x20u);
			m_pageNumber = value;
		}
	}

	internal string BackgroundColor
	{
		get
		{
			return m_backgroundColor;
		}
		set
		{
			m_CommonstyleFlags = (byte)((m_CommonstyleFlags & 0xBFu) | 0x40u);
			m_backgroundColor = value;
		}
	}
}
