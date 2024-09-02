using System;
using System.Collections;
using System.IO;

namespace DocGen.Pdf.Security;

internal class KeyInformation : Asn1Encode
{
	private Asn1 m_privateKey;

	private Algorithms m_algorithms;

	private Asn1Set m_attributes;

	internal Algorithms AlgorithmID => m_algorithms;

	internal Asn1 PrivateKey => m_privateKey;

	internal static KeyInformation GetInformation(object obj)
	{
		if (obj is KeyInformation)
		{
			return (KeyInformation)obj;
		}
		if (obj != null)
		{
			return new KeyInformation(Asn1Sequence.GetSequence(obj));
		}
		return null;
	}

	internal KeyInformation(Algorithms algorithms, Asn1 privateKey)
		: this(algorithms, privateKey, null)
	{
	}

	public KeyInformation(Algorithms algorithms, Asn1 privateKey, Asn1Set attributes)
	{
		m_privateKey = privateKey;
		m_algorithms = algorithms;
		m_attributes = attributes;
	}

	private KeyInformation(Asn1Sequence sequence)
	{
		IEnumerator enumerator = sequence.GetEnumerator();
		enumerator.MoveNext();
		_ = ((DerInteger)enumerator.Current).Value;
		enumerator.MoveNext();
		m_algorithms = Algorithms.GetAlgorithms(enumerator.Current);
		try
		{
			enumerator.MoveNext();
			Asn1Octet asn1Octet = (Asn1Octet)enumerator.Current;
			m_privateKey = Asn1.FromByteArray(asn1Octet.GetOctets());
		}
		catch (IOException)
		{
			throw new ArgumentException("Invalid private key in sequence");
		}
		if (enumerator.MoveNext())
		{
			m_attributes = Asn1Set.GetAsn1Set((Asn1Tag)enumerator.Current, isExplicit: false);
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(new DerInteger(0), m_algorithms, new DerOctet(m_privateKey));
		if (m_attributes != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: false, 0, m_attributes));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
