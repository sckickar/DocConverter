using System;

namespace DocGen.Pdf.Security;

internal class PublicKeyInformation : Asn1Encode
{
	private Algorithms m_algorithms;

	private DerBitString m_publicKey;

	internal Algorithms Algorithm => m_algorithms;

	internal DerBitString PublicKey => m_publicKey;

	internal static PublicKeyInformation GetPublicKeyInformation(object obj)
	{
		if (obj is PublicKeyInformation)
		{
			return (PublicKeyInformation)obj;
		}
		if (obj != null)
		{
			return new PublicKeyInformation(Asn1Sequence.GetSequence(obj));
		}
		return null;
	}

	internal PublicKeyInformation(Algorithms algorithms, Asn1Encode publicKey)
	{
		m_publicKey = new DerBitString(publicKey);
		m_algorithms = algorithms;
	}

	internal PublicKeyInformation(Algorithms algorithms, byte[] publicKey)
	{
		m_publicKey = new DerBitString(publicKey);
		m_algorithms = algorithms;
	}

	private PublicKeyInformation(Asn1Sequence sequence)
	{
		if (sequence.Count != 2)
		{
			throw new ArgumentException("Invalid length in sequence");
		}
		m_algorithms = Algorithms.GetAlgorithms(sequence[0]);
		m_publicKey = DerBitString.GetString(sequence[1]);
	}

	internal Asn1 GetPublicKey()
	{
		return Asn1.FromByteArray(m_publicKey.GetBytes());
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(m_algorithms, m_publicKey);
	}
}
