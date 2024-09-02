using System.IO;

namespace DocGen.Pdf.Security;

internal class TimeStampResponse
{
	private Asn1 m_encodedObject;

	private Asn1Integer m_pkiStatusInfo;

	private Asn1 m_timeStampToken;

	private Asn1Identifier m_contentType;

	internal Asn1 Object => m_encodedObject;

	internal TimeStampResponse(byte[] bytes)
	{
		Asn1Stream stream = new Asn1Stream(bytes);
		ReadTimeStampResponse(stream);
	}

	internal byte[] GetEncoded(Asn1 encodedObject)
	{
		return ReadTimeStampToken(encodedObject);
	}

	private void ReadTimeStampResponse(Asn1Stream stream)
	{
		m_encodedObject = stream.ReadAsn1();
	}

	private byte[] ReadTimeStampToken(Asn1 encodedObject)
	{
		if (encodedObject is Asn1Sequence)
		{
			m_pkiStatusInfo = new Asn1Integer(((encodedObject as Asn1Sequence)[0] as DerObjectID).GetBytes());
			m_timeStampToken = encodedObject;
		}
		return ReadContentInfo();
	}

	private byte[] ReadContentInfo()
	{
		m_contentType = (m_timeStampToken as Asn1Sequence)[0] as Asn1Identifier;
		return ReadTimeStampContent();
	}

	private byte[] ReadTimeStampContent()
	{
		return new Asn1DerStream(new MemoryStream()).ParseTimeStamp(m_timeStampToken);
	}
}
