using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class CmsSignedDetails
{
	private CryptographicMessageSyntaxBytes m_content;

	private SignedDetails m_signedCms;

	private ContentInformation m_contentInformation;

	private SignerInformationCollection m_signerInformationCollection;

	internal DerObjectID SignedContentType => m_signedCms.ContentInformation.ContentType;

	internal CryptographicMessageSyntaxBytes SignedContent => m_content;

	internal CmsSignedDetails(ContentInformation sigData)
	{
		m_contentInformation = sigData;
		m_signedCms = new SignedDetails(m_contentInformation.Content as Asn1Sequence);
		if (m_signedCms.ContentInformation.Content != null)
		{
			m_content = new CryptographicMessageSyntaxBytes(((Asn1Octet)m_signedCms.ContentInformation.Content).GetOctets());
		}
	}

	internal SignerInformationCollection GetSignerDetails()
	{
		if (m_signerInformationCollection == null)
		{
			List<CmsSignerDetails> list = new List<CmsSignerDetails>();
			foreach (object item in m_signedCms.SignerInformation)
			{
				SignerDetails signerDetails = SignerDetails.GetSignerDetails(item);
				DerObjectID contentType = m_signedCms.ContentInformation.ContentType;
				list.Add(new CmsSignerDetails(signerDetails, contentType, m_content));
			}
			m_signerInformationCollection = new SignerInformationCollection(list);
		}
		return m_signerInformationCollection;
	}
}
