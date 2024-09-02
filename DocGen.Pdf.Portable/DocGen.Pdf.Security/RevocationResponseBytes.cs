using System;

namespace DocGen.Pdf.Security;

internal class RevocationResponseBytes : Asn1Encode
{
	private DerObjectID m_responseType;

	private Asn1Octet m_response;

	public DerObjectID ResponseType => m_responseType;

	public Asn1Octet Response => m_response;

	private RevocationResponseBytes(Asn1Sequence sequence)
	{
		if (sequence.Count != 2)
		{
			throw new ArgumentException("Invalid length in sequence");
		}
		m_responseType = DerObjectID.GetID(sequence[0]);
		m_response = Asn1Octet.GetOctetString(sequence[1]);
	}

	internal RevocationResponseBytes()
	{
	}

	public RevocationResponseBytes GetResponseBytes(Asn1Tag tag, bool isExplicit)
	{
		return GetResponseBytes(Asn1Sequence.GetSequence(tag, isExplicit));
	}

	public RevocationResponseBytes GetResponseBytes(object obj)
	{
		if (obj == null || obj is RevocationResponseBytes)
		{
			return (RevocationResponseBytes)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new RevocationResponseBytes((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(m_responseType, m_response);
	}
}
