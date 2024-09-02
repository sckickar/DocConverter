namespace DocGen.DocIO.DLS;

internal class MacroData
{
	private string m_name;

	private string m_bEncrypt;

	private string m_cmg;

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal string Encrypt
	{
		get
		{
			return m_bEncrypt;
		}
		set
		{
			m_bEncrypt = value;
		}
	}

	internal string Cmg
	{
		get
		{
			return m_cmg;
		}
		set
		{
			m_cmg = value;
		}
	}

	internal MacroData()
	{
	}
}
