using System;
using System.Security.Cryptography.X509Certificates;

namespace DocGen.Pdf.Security;

public class PdfRevocationCertificate
{
	private bool isEmbedded;

	private DateTime validFrom;

	private DateTime validTo;

	private X509Certificate2[] certificates;

	private bool isRevokedCRL;

	private RevokedCertificate[] revokedCertificates;

	public bool IsEmbedded
	{
		get
		{
			return isEmbedded;
		}
		internal set
		{
			isEmbedded = value;
		}
	}

	public DateTime ValidFrom
	{
		get
		{
			return validFrom;
		}
		internal set
		{
			validFrom = value;
		}
	}

	public DateTime ValidTo
	{
		get
		{
			return validTo;
		}
		internal set
		{
			validTo = value;
		}
	}

	public X509Certificate2[] Certificates
	{
		get
		{
			return certificates;
		}
		internal set
		{
			certificates = value;
		}
	}

	public bool IsRevokedCRL
	{
		get
		{
			return isRevokedCRL;
		}
		internal set
		{
			isRevokedCRL = value;
		}
	}

	public RevokedCertificate[] RevokedCertificates
	{
		get
		{
			return revokedCertificates;
		}
		internal set
		{
			revokedCertificates = value;
		}
	}
}
