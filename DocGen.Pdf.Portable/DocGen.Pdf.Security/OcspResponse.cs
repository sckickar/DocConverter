using System;

namespace DocGen.Pdf.Security;

internal class OcspResponse : Asn1Encode
{
	private OcspResponseStatus m_responseStatus;

	private RevocationResponseBytes m_responseBytes;

	internal OcspResponseStatus ResponseStatus => m_responseStatus;

	internal RevocationResponseBytes ResponseBytes => m_responseBytes;

	private OcspResponse(Asn1Sequence sequence)
	{
		DerCatalogue derCatalogue = new DerCatalogue();
		m_responseStatus = new OcspResponseStatus(derCatalogue.GetEnumeration(sequence[0]));
		if (sequence.Count == 2)
		{
			RevocationResponseBytes revocationResponseBytes = new RevocationResponseBytes();
			m_responseBytes = revocationResponseBytes.GetResponseBytes((Asn1Tag)sequence[1], isExplicit: true);
		}
	}

	internal OcspResponse()
	{
	}

	internal OcspResponse GetOcspResponse(object obj)
	{
		if (obj == null || obj is OcspResponse)
		{
			return (OcspResponse)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new OcspResponse((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_responseStatus);
		if (m_responseBytes != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: true, 0, m_responseBytes));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
