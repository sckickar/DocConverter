using System;

namespace DocGen.Pdf.Security;

internal class DerInteger : Asn1
{
	internal byte[] m_value;

	internal Number Value => new Number(m_value);

	internal Number PositiveValue => new Number(1, m_value);

	internal DerInteger(int value)
	{
		m_value = Number.ValueOf(value).ToByteArray();
	}

	internal DerInteger(Number value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_value = value.ToByteArray();
	}

	internal DerInteger(byte[] bytes)
	{
		m_value = bytes;
	}

	internal override void Encode(DerStream stream)
	{
		stream.WriteEncoded(2, m_value);
	}

	public override int GetHashCode()
	{
		return Asn1Constants.GetHashCode(m_value);
	}

	protected override bool IsEquals(Asn1 asn1)
	{
		if (!(asn1 is DerInteger derInteger))
		{
			return false;
		}
		return Asn1Constants.AreEqual(m_value, derInteger.m_value);
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	internal static DerInteger GetNumber(object obj)
	{
		if (obj == null || obj is DerInteger)
		{
			return (DerInteger)obj;
		}
		throw new ArgumentException("Invalid entry");
	}

	internal static DerInteger GetNumber(Asn1Tag tag, bool isExplicit)
	{
		if (tag == null)
		{
			throw new ArgumentNullException("tag");
		}
		Asn1 @object = tag.GetObject();
		if (isExplicit || @object is DerInteger)
		{
			return GetNumber(@object);
		}
		return new DerInteger(Asn1Octet.GetOctetString(@object).GetOctets());
	}
}
