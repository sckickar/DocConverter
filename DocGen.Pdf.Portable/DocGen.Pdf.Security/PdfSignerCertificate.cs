using System.Security.Cryptography.X509Certificates;

namespace DocGen.Pdf.Security;

public class PdfSignerCertificate
{
	private PdfRevocationCertificate ocspCertificate;

	private PdfRevocationCertificate crlCertificate;

	private X509Certificate2 certificate;

	public PdfRevocationCertificate OcspCertificate
	{
		get
		{
			return ocspCertificate;
		}
		internal set
		{
			ocspCertificate = value;
		}
	}

	public PdfRevocationCertificate CrlCertificate
	{
		get
		{
			return crlCertificate;
		}
		internal set
		{
			crlCertificate = value;
		}
	}

	public X509Certificate2 Certificate
	{
		get
		{
			return certificate;
		}
		internal set
		{
			certificate = value;
		}
	}
}
