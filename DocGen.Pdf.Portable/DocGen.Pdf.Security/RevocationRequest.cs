using System;

namespace DocGen.Pdf.Security;

internal class RevocationRequest : Asn1Encode
{
	private CertificateIdentityHelper m_certificateID;

	private X509Extensions m_singleRequestExtensions;

	internal RevocationRequest(CertificateIdentityHelper certificateID, X509Extensions singleRequestExtensions)
	{
		if (certificateID == null)
		{
			throw new ArgumentNullException("certificateID");
		}
		m_certificateID = certificateID;
		m_singleRequestExtensions = singleRequestExtensions;
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_certificateID);
		if (m_singleRequestExtensions != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: true, 0, m_singleRequestExtensions));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
