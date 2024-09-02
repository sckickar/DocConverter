using System;

namespace DocGen.Pdf.Security;

internal class EncryptedPrivateKey : Asn1Encode
{
	private Algorithms m_algorithms;

	private Asn1Octet m_octet;

	internal Algorithms EncryptionAlgorithm => m_algorithms;

	internal byte[] EncryptedData => m_octet.GetOctets();

	private EncryptedPrivateKey(Asn1Sequence sequence)
	{
		if (sequence.Count != 2)
		{
			throw new ArgumentException("Invalid length in sequence");
		}
		m_algorithms = Algorithms.GetAlgorithms(sequence[0]);
		m_octet = Asn1Octet.GetOctetString(sequence[1]);
	}

	internal static EncryptedPrivateKey GetEncryptedPrivateKeyInformation(object obj)
	{
		if (obj is EncryptedPrivateKey)
		{
			return (EncryptedPrivateKey)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new EncryptedPrivateKey((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(m_algorithms, m_octet);
	}
}
