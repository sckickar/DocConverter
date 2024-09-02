namespace DocGen.DocIO.ODFConverter.Base.ODFImplementation;

internal class OTableOfContent
{
	private string m_name;

	private bool m_isProtected;

	private string m_protectionKey;

	private string m_protectionKeyDigestAlgorithm;

	internal string ProtectionKeyDigestAlgorithm
	{
		get
		{
			return m_protectionKeyDigestAlgorithm;
		}
		set
		{
			m_protectionKeyDigestAlgorithm = value;
		}
	}

	internal string ProtectionKey
	{
		get
		{
			return m_protectionKey;
		}
		set
		{
			m_protectionKey = value;
		}
	}

	internal bool Isprotected
	{
		get
		{
			return m_isProtected;
		}
		set
		{
			m_isProtected = value;
		}
	}

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
}
