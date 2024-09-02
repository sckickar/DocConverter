namespace BitMiracle.LibTiff.Classic;

public class TiffFieldInfo
{
	private TiffTag m_tag;

	private short m_readCount;

	private short m_writeCount;

	private TiffType m_type;

	private short m_bit;

	private bool m_okToChange;

	private bool m_passCount;

	private string m_name;

	public const short Variable = -1;

	public const short Spp = -2;

	public const short Variable2 = -3;

	public TiffTag Tag => m_tag;

	public short ReadCount => m_readCount;

	public short WriteCount => m_writeCount;

	public TiffType Type => m_type;

	public short Bit => m_bit;

	public bool OkToChange => m_okToChange;

	public bool PassCount => m_passCount;

	public string Name
	{
		get
		{
			return m_name;
		}
		internal set
		{
			m_name = value;
		}
	}

	public TiffFieldInfo(TiffTag tag, short readCount, short writeCount, TiffType type, short bit, bool okToChange, bool passCount, string name)
	{
		m_tag = tag;
		m_readCount = readCount;
		m_writeCount = writeCount;
		m_type = type;
		m_bit = bit;
		m_okToChange = okToChange;
		m_passCount = passCount;
		m_name = name;
	}

	public override string ToString()
	{
		if (m_bit != 65 || m_name.Length == 0)
		{
			return m_tag.ToString();
		}
		return m_name;
	}
}
