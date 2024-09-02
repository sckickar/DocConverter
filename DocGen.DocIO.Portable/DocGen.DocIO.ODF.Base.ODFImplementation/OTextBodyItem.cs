namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OTextBodyItem
{
	private byte m_flag;

	private string m_sectionStyleName;

	internal bool IsFirstItemOfSection
	{
		get
		{
			return (m_flag & 1) != 0;
		}
		set
		{
			m_flag = (byte)((m_flag & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsLastItemOfSection
	{
		get
		{
			return (m_flag & 2) >> 1 != 0;
		}
		set
		{
			m_flag = (byte)((m_flag & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal string SectionStyleName
	{
		get
		{
			return m_sectionStyleName;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				m_sectionStyleName = value;
			}
		}
	}
}
