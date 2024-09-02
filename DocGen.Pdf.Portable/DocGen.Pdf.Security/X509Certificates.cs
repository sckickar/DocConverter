namespace DocGen.Pdf.Security;

internal class X509Certificates
{
	private X509Certificate m_certificate;

	internal X509Certificate Certificate => m_certificate;

	internal X509Certificates(X509Certificate certificates)
	{
		m_certificate = certificates;
	}

	public override int GetHashCode()
	{
		return m_certificate.GetHashCode();
	}
}
