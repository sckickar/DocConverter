using System;
using System.Collections;
using System.IO;

namespace DocGen.Pdf.Security;

internal class TimeStampToken
{
	private CmsSignedDetails m_timeStampData;

	private CmsSignerDetails m_sigerDetails;

	private TimeStampIdentifier m_certIdentifier;

	private TimeStampTokenInformation m_timestampTokenInformation;

	internal TimeStampTokenInformation TimeStampInformation => m_timestampTokenInformation;

	internal TimeStampToken(CmsSignedDetails signedData)
	{
		m_timeStampData = signedData;
		if (signedData.SignedContentType.ID.Equals("1.2.840.113549.1.9.16.1.4"))
		{
			ICollection signers = signedData.GetSignerDetails().GetSigners();
			if (signers.Count == 1)
			{
				IEnumerator enumerator = signers.GetEnumerator();
				enumerator.MoveNext();
				m_sigerDetails = (CmsSignerDetails)enumerator.Current;
			}
		}
		try
		{
			CryptographicMessageSyntaxBytes signedContent = m_timeStampData.SignedContent;
			MemoryStream memoryStream = new MemoryStream();
			signedContent.Write(memoryStream);
			TimeStampData timeStampData = new TimeStampData(Asn1.FromByteArray(memoryStream.ToArray()) as Asn1Sequence);
			m_timestampTokenInformation = new TimeStampTokenInformation(timeStampData);
			TimeStampElement timeStampElement = m_sigerDetails.SignedAttributes[PKCSOIDs.Pkcs9AtSigningCertV1];
			if (timeStampElement != null)
			{
				TimeStampCertificate timeStanpCertificate = TimeStampCertificate.GetTimeStanpCertificate(timeStampElement.Values[0]);
				m_certIdentifier = TimeStampIdentifier.GetTimeStampCertID(timeStanpCertificate.Certificates[0]);
				return;
			}
			timeStampElement = m_sigerDetails.SignedAttributes[PKCSOIDs.Pkcs9AtSigningCertV2];
			if (timeStampElement != null)
			{
				TimeStampCertificate timeStanpCertificate2 = TimeStampCertificate.GetTimeStanpCertificate(timeStampElement.Values[0]);
				m_certIdentifier = TimeStampIdentifier.GetTimeStampCertID(timeStanpCertificate2.Certificates[0]);
			}
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message, ex.InnerException);
		}
	}
}
