using System;

namespace DocGen.Pdf.Security;

internal class KeyIdentifier : Asn1Encode
{
	internal Asn1Octet m_keyIdentifier;

	internal DerInteger m_serialNumber;

	internal byte[] KeyID
	{
		get
		{
			if (m_keyIdentifier != null)
			{
				return m_keyIdentifier.GetOctets();
			}
			return null;
		}
	}

	internal static KeyIdentifier GetKeyIdentifier(object obj)
	{
		if (obj is KeyIdentifier)
		{
			return (KeyIdentifier)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new KeyIdentifier((Asn1Sequence)obj);
		}
		if (obj is X509Extension)
		{
			return GetKeyIdentifier(X509Extension.ConvertValueToObject((X509Extension)obj));
		}
		throw new ArgumentException("Invalid entry");
	}

	protected internal KeyIdentifier(Asn1Sequence sequence)
	{
		foreach (Asn1Tag item in sequence)
		{
			switch (item.TagNumber)
			{
			case 0:
				m_keyIdentifier = Asn1Octet.GetOctetString(item);
				break;
			case 2:
				m_serialNumber = DerInteger.GetNumber(item, isExplicit: false);
				break;
			default:
				throw new ArgumentException("Invalid entry in sequence");
			case 1:
				break;
			}
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		if (m_keyIdentifier != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: false, 0, m_keyIdentifier));
		}
		if (m_serialNumber != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: false, 2, m_serialNumber));
		}
		return new DerSequence(asn1EncodeCollection);
	}

	public override string ToString()
	{
		return "AuthorityKeyIdentifier: KeyID(" + m_keyIdentifier.GetOctets()?.ToString() + ")";
	}
}
