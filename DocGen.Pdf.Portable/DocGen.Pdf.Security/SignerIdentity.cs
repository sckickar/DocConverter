using System;

namespace DocGen.Pdf.Security;

internal class SignerIdentity : Asn1Encode
{
	private Asn1Encode m_id;

	internal bool IsTagged => m_id is Asn1Tag;

	internal Asn1Encode ID
	{
		get
		{
			if (m_id is Asn1Tag)
			{
				return Asn1Octet.GetOctetString((Asn1Tag)m_id, isExplicit: false);
			}
			return m_id;
		}
	}

	internal SignerIdentity(CertificateInformation id)
	{
		m_id = id;
	}

	internal SignerIdentity(Asn1Octet id)
	{
		m_id = new DerTag(isExplicit: false, 0, id);
	}

	internal SignerIdentity(Asn1 id)
	{
		m_id = id;
	}

	internal static SignerIdentity GetIdentity(object o)
	{
		if (o == null || o is SignerIdentity)
		{
			return (SignerIdentity)o;
		}
		if (o is CertificateInformation)
		{
			return new SignerIdentity((CertificateInformation)o);
		}
		if (o is Asn1Octet)
		{
			return new SignerIdentity((Asn1Octet)o);
		}
		if (o is Asn1)
		{
			return new SignerIdentity((Asn1)o);
		}
		throw new ArgumentException("Invalid entry in sequence: " + o.GetType().Name);
	}

	public override Asn1 GetAsn1()
	{
		return m_id.GetAsn1();
	}
}
