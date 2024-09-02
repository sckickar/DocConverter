using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class TabDescriptor
{
	internal const int DEF_TAB_LENGTH = 1;

	private TabJustification m_jc;

	private TabLeader m_tlc;

	internal TabJustification Justification
	{
		get
		{
			return m_jc;
		}
		set
		{
			if (value != m_jc)
			{
				m_jc = value;
			}
		}
	}

	internal TabLeader TabLeader
	{
		get
		{
			return m_tlc;
		}
		set
		{
			if (value != m_tlc)
			{
				m_tlc = value;
			}
		}
	}

	internal TabDescriptor(byte options)
	{
		m_jc = (TabJustification)(byte)(options & 7);
		m_tlc = (TabLeader)(byte)((options & 0x38) >> 3);
	}

	internal TabDescriptor(TabJustification justification, TabLeader leader)
	{
		m_jc = justification;
		m_tlc = leader;
	}

	internal byte Save()
	{
		return (byte)(((byte)m_tlc << 3) | (byte)m_jc);
	}
}
