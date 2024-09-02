using System;
using System.Text;

namespace DocGen.Pdf.Security;

internal class DerUtf8String : DerString
{
	private string m_value;

	internal static DerUtf8String GetUtf8String(object obj)
	{
		if (obj == null || obj is DerUtf8String)
		{
			return (DerUtf8String)obj;
		}
		throw new ArgumentException("Invalid entry");
	}

	internal static DerUtf8String GetUtf8String(Asn1Tag tag, bool isExplicit)
	{
		Asn1 @object = tag.GetObject();
		if (isExplicit || @object is DerUtf8String)
		{
			return GetUtf8String(@object);
		}
		return new DerUtf8String(Asn1Octet.GetOctetString(@object).GetOctets());
	}

	internal DerUtf8String(byte[] bytes)
		: this(Encoding.UTF8.GetString(bytes, 0, bytes.Length))
	{
	}

	internal DerUtf8String(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_value = value;
	}

	public override string GetString()
	{
		return m_value;
	}

	protected override bool IsEquals(Asn1 asn1)
	{
		if (!(asn1 is DerUtf8String derUtf8String))
		{
			return false;
		}
		return m_value.Equals(derUtf8String.m_value);
	}

	internal override void Encode(DerStream stream)
	{
		stream.WriteEncoded(12, Encoding.UTF8.GetBytes(m_value));
	}
}
