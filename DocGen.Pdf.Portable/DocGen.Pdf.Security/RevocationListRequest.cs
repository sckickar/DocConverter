using System;

namespace DocGen.Pdf.Security;

internal class RevocationListRequest : Asn1Encode
{
	private OcspRequestCollection m_requests;

	public RevocationListRequest(OcspRequestCollection requests)
	{
		if (requests == null)
		{
			throw new ArgumentNullException("requests");
		}
		m_requests = requests;
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(new Asn1EncodeCollection(m_requests));
	}
}
