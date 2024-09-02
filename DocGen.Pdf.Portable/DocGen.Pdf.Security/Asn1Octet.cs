using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal class Asn1Octet : Asn1, IAsn1Octet, IAsn1
{
	internal byte[] m_value;

	internal IAsn1Octet Parser => this;

	internal Asn1Octet(byte[] value)
		: base(Asn1UniversalTags.OctetString)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_value = value;
	}

	internal Asn1Octet(Asn1Encode obj)
	{
		try
		{
			m_value = obj.GetEncoded("DER");
		}
		catch (IOException ex)
		{
			throw new ArgumentException(ex.ToString());
		}
	}

	public Stream GetOctetStream()
	{
		return new MemoryStream(m_value, writable: false);
	}

	internal virtual byte[] GetOctets()
	{
		return m_value;
	}

	public override int GetHashCode()
	{
		return Asn1Constants.GetHashCode(GetOctets());
	}

	protected override bool IsEquals(Asn1 asn1Object)
	{
		if (!(asn1Object is DerOctet derOctet))
		{
			return false;
		}
		return Asn1Constants.AreEqual(GetOctets(), derOctet.GetOctets());
	}

	public override string ToString()
	{
		return m_value.ToString();
	}

	internal byte[] AsnEncode()
	{
		return Asn1Encode(m_value);
	}

	internal override void Encode(DerStream stream)
	{
		throw new NotImplementedException();
	}

	internal static Asn1Octet GetOctetString(Asn1Tag tag, bool isExplicit)
	{
		Asn1 @object = tag.GetObject();
		if (isExplicit || @object is Asn1Octet)
		{
			return GetOctetString(@object);
		}
		return BerOctet.GetBerOctet(Asn1Sequence.GetSequence(@object));
	}

	internal static Asn1Octet GetOctetString(object obj)
	{
		if (obj == null || obj is Asn1Octet)
		{
			return (Asn1Octet)obj;
		}
		if (obj is Asn1Tag)
		{
			return GetOctetString(((Asn1Tag)obj).GetObject());
		}
		throw new ArgumentException("Invalid object entry");
	}
}
