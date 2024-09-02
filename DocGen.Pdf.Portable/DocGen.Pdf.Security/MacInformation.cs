using System;

namespace DocGen.Pdf.Security;

internal class MacInformation : Asn1Encode
{
	internal DigestInformation m_digest;

	internal byte[] m_value;

	internal Number m_count;

	internal static MacInformation GetInformation(object obj)
	{
		if (obj is MacInformation)
		{
			return (MacInformation)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new MacInformation((Asn1Sequence)obj);
		}
		throw new Exception("Invalid entry");
	}

	private MacInformation(Asn1Sequence sequence)
	{
		m_digest = DigestInformation.GetDigestInformation(sequence[0]);
		m_value = ((Asn1Octet)sequence[1]).GetOctets();
		if (sequence.Count == 3)
		{
			m_count = ((DerInteger)sequence[2]).Value;
		}
		else
		{
			m_count = Number.One;
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_digest, new DerOctet(m_value));
		if (!m_count.Equals(Number.One))
		{
			asn1EncodeCollection.Add(new DerInteger(m_count));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
