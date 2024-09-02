namespace DocGen.Pdf.Security;

public class RevocationResult
{
	private bool m_isRevokedCRL;

	private RevocationStatus m_revocationStatus;

	public bool IsRevokedCRL
	{
		get
		{
			return m_isRevokedCRL;
		}
		internal set
		{
			m_isRevokedCRL = value;
		}
	}

	public RevocationStatus OcspRevocationStatus
	{
		get
		{
			return m_revocationStatus;
		}
		internal set
		{
			m_revocationStatus = value;
		}
	}
}
