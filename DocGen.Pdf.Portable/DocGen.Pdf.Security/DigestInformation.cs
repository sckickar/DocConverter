using System;

namespace DocGen.Pdf.Security;

internal class DigestInformation : Asn1Encode
{
	private byte[] m_bytes;

	private Algorithms m_algorithms;

	internal static DigestInformation GetDigestInformation(object obj)
	{
		if (obj is DigestInformation)
		{
			return (DigestInformation)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new DigestInformation((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry");
	}

	internal DigestInformation(Algorithms algorithms, byte[] bytes)
	{
		m_bytes = bytes;
		m_algorithms = algorithms;
	}

	private DigestInformation(Asn1Sequence sequence)
	{
		if (sequence.Count != 2)
		{
			throw new ArgumentException("Invalid length in sequence");
		}
		m_algorithms = Algorithms.GetAlgorithms(sequence[0]);
		m_bytes = Asn1Octet.GetOctetString(sequence[1]).GetOctets();
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(m_algorithms, new DerOctet(m_bytes));
	}
}
