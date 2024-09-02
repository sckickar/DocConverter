using System;

namespace DocGen.Pdf.Security;

internal class CmsSignerDetails
{
	private SignerId m_id;

	private SignerDetails m_details;

	private Algorithms m_digestAlgorithm;

	private Algorithms m_encryptionAlgorithm;

	private readonly Asn1Set m_signedSet;

	private readonly Asn1Set m_unsignedSet;

	private CryptographicMessageSyntaxBytes m_bytes;

	private byte[] m_signatureData;

	private DerObjectID m_contentType;

	private TimeStampElements m_signedTable;

	internal SignerId ID => m_id;

	internal TimeStampElements SignedAttributes
	{
		get
		{
			if (m_signedSet != null && m_signedTable == null)
			{
				m_signedTable = new TimeStampElements(m_signedSet);
			}
			return m_signedTable;
		}
	}

	internal CmsSignerDetails(SignerDetails information, DerObjectID contentType, CryptographicMessageSyntaxBytes content)
	{
		m_details = information;
		m_id = new SignerId();
		m_contentType = contentType;
		try
		{
			SignerIdentity iD = information.ID;
			if (iD.IsTagged)
			{
				Asn1Octet octetString = Asn1Octet.GetOctetString(iD.ID);
				m_id.KeyIdentifier = octetString.GetEncoded();
			}
			else
			{
				CertificateInformation certificateInformation = CertificateInformation.GetCertificateInformation(iD.ID);
				m_id.Issuer = certificateInformation.Name;
				m_id.SerialNumber = certificateInformation.SerialNumber.Value;
			}
		}
		catch (Exception)
		{
			throw new ArgumentException("Invalid entry in signer details");
		}
		m_digestAlgorithm = information.DigestAlgorithm;
		m_signedSet = information.Attributes;
		m_unsignedSet = information.Elements;
		m_encryptionAlgorithm = information.EncryptionAlgorithm;
		m_signatureData = information.EncryptedOctet.GetOctets();
		m_bytes = content;
	}
}
