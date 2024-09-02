namespace DocGen.Pdf.Security;

internal class OcspRequestHelper : X509ExtensionBase
{
	private RevocationListRequest m_request;

	public OcspRequestHelper(RevocationListRequest request)
	{
		m_request = request;
	}

	protected override X509Extensions GetX509Extensions()
	{
		return null;
	}

	public byte[] GetEncoded()
	{
		return m_request.GetEncoded();
	}
}
