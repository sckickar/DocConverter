using System;
using System.Collections;

namespace DocGen.Pdf.Security;

internal class SignerDetails : Asn1Encode
{
	private DerInteger m_version;

	private SignerIdentity m_id;

	private Algorithms m_algorithm;

	private Asn1Set m_attributes;

	private Algorithms m_encryptionAlgorithm;

	private Asn1Octet m_encryptedOctet;

	private Asn1Set m_elements;

	internal SignerIdentity ID => m_id;

	internal Asn1Set Attributes => m_attributes;

	internal Algorithms DigestAlgorithm => m_algorithm;

	internal Asn1Octet EncryptedOctet => m_encryptedOctet;

	internal Algorithms EncryptionAlgorithm => m_encryptionAlgorithm;

	internal Asn1Set Elements => m_elements;

	internal static SignerDetails GetSignerDetails(object obj)
	{
		if (obj == null || obj is SignerDetails)
		{
			return (SignerDetails)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new SignerDetails((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in signer details" + obj.GetType().FullName, "obj");
	}

	internal SignerDetails(Asn1Sequence seq)
	{
		IEnumerator enumerator = seq.GetEnumerator();
		enumerator.MoveNext();
		m_version = (DerInteger)enumerator.Current;
		enumerator.MoveNext();
		m_id = SignerIdentity.GetIdentity(enumerator.Current);
		enumerator.MoveNext();
		m_algorithm = Algorithms.GetAlgorithms(enumerator.Current);
		enumerator.MoveNext();
		object current = enumerator.Current;
		if (current is Asn1Tag)
		{
			m_attributes = Asn1Set.GetAsn1Set((Asn1Tag)current, isExplicit: false);
			enumerator.MoveNext();
			m_encryptionAlgorithm = Algorithms.GetAlgorithms(enumerator.Current);
		}
		else
		{
			m_attributes = null;
			m_encryptionAlgorithm = Algorithms.GetAlgorithms(current);
		}
		enumerator.MoveNext();
		m_encryptedOctet = Asn1Octet.GetOctetString(enumerator.Current);
		if (enumerator.MoveNext())
		{
			m_elements = Asn1Set.GetAsn1Set((Asn1Tag)enumerator.Current, isExplicit: false);
		}
		else
		{
			m_elements = null;
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_version, m_id, m_algorithm);
		if (m_attributes != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: false, 0, m_attributes));
		}
		asn1EncodeCollection.Add(m_encryptionAlgorithm, m_encryptedOctet);
		if (m_elements != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: false, 1, m_elements));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
