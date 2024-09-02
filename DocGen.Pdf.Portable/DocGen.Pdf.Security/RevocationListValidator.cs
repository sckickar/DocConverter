using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class RevocationListValidator
{
	private RevocationList m_crl;

	private RevocationResult m_result;

	internal PdfSignatureValidationResult result;

	internal RevocationListValidator(X509Certificate signerCertificate, RevocationResult result)
	{
		m_crl = new RevocationList(signerCertificate);
		m_result = result;
	}

	internal bool Validate(X509Certificate signerCertificate, X509Certificate issuerCertificate, DateTime signDate, List<CertificateCollection> certificateCollections)
	{
		DateTime now = DateTime.Now;
		foreach (CertificateCollection certificateCollection in certificateCollections)
		{
			RevocationListHelper revocationListHelper = new RevocationListHelper(certificateCollection);
			if (revocationListHelper.Validate(signerCertificate, issuerCertificate, signDate) || revocationListHelper.Validate(signerCertificate, issuerCertificate, now))
			{
				return true;
			}
		}
		return false;
	}

	internal bool OnlineValidate(X509Certificate signerCertificate, X509Certificate issuerCertificate, DateTime signDate)
	{
		ICollection<byte[]> encoded = m_crl.GetEncoded(signerCertificate, null);
		List<CertificateCollection> list = new List<CertificateCollection>();
		foreach (byte[] item in encoded)
		{
			CertificateCollection certificateList = CertificateCollection.GetCertificateList((Asn1Sequence)new Asn1Stream(item).ReadAsn1());
			if (certificateList != null)
			{
				list.Add(certificateList);
			}
		}
		return Validate(signerCertificate, issuerCertificate, signDate, list);
	}
}
