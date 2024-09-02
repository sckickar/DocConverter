namespace DocGen.Pdf.Security;

public class LtvVerificationInfo
{
	private bool m_isLtvEnabled;

	private bool m_isCrlEmbedded;

	private bool m_isOcspEmbedded;

	public bool IsLtvEnabled
	{
		get
		{
			return m_isLtvEnabled;
		}
		internal set
		{
			m_isLtvEnabled = value;
		}
	}

	public bool IsCrlEmbedded
	{
		get
		{
			return m_isCrlEmbedded;
		}
		internal set
		{
			m_isCrlEmbedded = value;
		}
	}

	public bool IsOcspEmbedded
	{
		get
		{
			return m_isOcspEmbedded;
		}
		internal set
		{
			m_isOcspEmbedded = value;
		}
	}
}
