using System;

namespace DocGen.Pdf.Security;

internal class Asn1Tag : Asn1, IAsn1Tag, IAsn1
{
	internal int m_tagNumber;

	internal bool m_isExplicit = true;

	internal Asn1Encode m_object;

	public int TagNumber => m_tagNumber;

	internal bool IsExplicit => m_isExplicit;

	internal bool IsEmpty => false;

	protected Asn1Tag(int tagNumber, Asn1Encode asn1Encode)
	{
		m_isExplicit = true;
		m_tagNumber = tagNumber;
		m_object = asn1Encode;
	}

	protected Asn1Tag(bool isExplicit, int tagNumber, Asn1Encode asn1Encode)
	{
		m_isExplicit = isExplicit;
		m_tagNumber = tagNumber;
		m_object = asn1Encode;
	}

	internal static Asn1Tag GetTag(Asn1Tag tag, bool isExplicit)
	{
		if (isExplicit)
		{
			return (Asn1Tag)tag.GetObject();
		}
		throw new ArgumentException("Explicit tag is not used");
	}

	internal static Asn1Tag GetTag(object obj)
	{
		if (obj == null || obj is Asn1Tag)
		{
			return (Asn1Tag)obj;
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	protected override bool IsEquals(Asn1 asn1Object)
	{
		if (!(asn1Object is Asn1Tag asn1Tag))
		{
			return false;
		}
		if (m_tagNumber == asn1Tag.m_tagNumber && m_isExplicit == asn1Tag.m_isExplicit)
		{
			return object.Equals(GetObject(), asn1Tag.GetObject());
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = m_tagNumber.GetHashCode();
		if (m_object != null)
		{
			num ^= m_object.GetHashCode();
		}
		return num;
	}

	internal Asn1 GetObject()
	{
		if (m_object != null)
		{
			return m_object.GetAsn1();
		}
		return null;
	}

	public IAsn1 GetParser(int tagNumber, bool isExplicit)
	{
		switch (tagNumber)
		{
		case 17:
			return Asn1Set.GetAsn1Set(this, isExplicit).Parser;
		case 16:
			return Asn1Sequence.GetSequence(this, isExplicit).Parser;
		case 4:
			return Asn1Octet.GetOctetString(this, isExplicit).Parser;
		default:
			if (isExplicit)
			{
				return GetObject();
			}
			throw new Exception("Implicit tagging is not supported : " + tagNumber);
		}
	}

	public override string ToString()
	{
		return "[" + m_tagNumber + "]" + m_object;
	}

	internal override void Encode(DerStream stream)
	{
		throw new NotImplementedException();
	}
}
