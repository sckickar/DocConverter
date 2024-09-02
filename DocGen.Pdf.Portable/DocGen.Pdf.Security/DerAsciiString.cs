using System;
using System.Text;

namespace DocGen.Pdf.Security;

internal class DerAsciiString : DerString
{
	private string m_value;

	internal DerAsciiString(byte[] bytes)
		: this(Encoding.UTF8.GetString(bytes, 0, bytes.Length), isValid: false)
	{
	}

	internal DerAsciiString(string value)
		: this(value, isValid: false)
	{
	}

	internal DerAsciiString(string value, bool isValid)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (isValid && !IsAsciiString(value))
		{
			throw new ArgumentException("Invalid characters found");
		}
		m_value = value;
	}

	public override string GetString()
	{
		return m_value;
	}

	internal byte[] AsnEncode()
	{
		return Asn1Encode(GetOctets());
	}

	internal byte[] GetOctets()
	{
		return Encoding.UTF8.GetBytes(m_value);
	}

	internal override void Encode(DerStream stream)
	{
		stream.WriteEncoded(22, GetOctets());
	}

	public override int GetHashCode()
	{
		return m_value.GetHashCode();
	}

	protected override bool IsEquals(Asn1 asn1)
	{
		if (!(asn1 is DerAsciiString derAsciiString))
		{
			return false;
		}
		return m_value.Equals(derAsciiString.m_value);
	}

	internal static DerAsciiString GetAsciiString(object obj)
	{
		if (obj == null || obj is DerAsciiString)
		{
			return (DerAsciiString)obj;
		}
		throw new ArgumentException("Invalid entry");
	}

	internal static DerAsciiString GetAsciiString(Asn1Tag tag, bool isExplicit)
	{
		Asn1 @object = tag.GetObject();
		if (isExplicit || @object is DerAsciiString)
		{
			return GetAsciiString(@object);
		}
		return new DerAsciiString(((Asn1Octet)@object).GetOctets());
	}

	internal static bool IsAsciiString(string value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i] > '\u007f')
			{
				return false;
			}
		}
		return true;
	}
}
