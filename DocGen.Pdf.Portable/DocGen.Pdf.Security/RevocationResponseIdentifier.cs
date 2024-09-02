using System;

namespace DocGen.Pdf.Security;

internal class RevocationResponseIdentifier : Asn1Encode
{
	private Asn1Encode m_id;

	public RevocationResponseIdentifier(Asn1Octet id)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		m_id = id;
	}

	public RevocationResponseIdentifier(X509Name id)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		m_id = id;
	}

	internal RevocationResponseIdentifier()
	{
	}

	internal RevocationResponseIdentifier GetResponseID(object obj)
	{
		if (obj == null || obj is RevocationResponseIdentifier)
		{
			return (RevocationResponseIdentifier)obj;
		}
		if (obj is DerOctet)
		{
			return new RevocationResponseIdentifier((DerOctet)obj);
		}
		if (obj is Asn1Tag)
		{
			Asn1Tag asn1Tag = (Asn1Tag)obj;
			if (asn1Tag.TagNumber == 1)
			{
				return new RevocationResponseIdentifier(X509Name.GetName(asn1Tag, isExplicit: true));
			}
			return new RevocationResponseIdentifier(Asn1Octet.GetOctetString(asn1Tag, isExplicit: true));
		}
		return new RevocationResponseIdentifier(X509Name.GetName(obj));
	}

	public override Asn1 GetAsn1()
	{
		if (m_id is Asn1Octet)
		{
			return new DerTag(isExplicit: true, 2, m_id);
		}
		return new DerTag(isExplicit: true, 1, m_id);
	}
}
