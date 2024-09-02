using System;
using System.Text;

namespace DocGen.Pdf.Security;

internal class DerVisibleString : DerString
{
	private string m_value;

	internal DerVisibleString(byte[] bytes)
		: this(Encoding.UTF8.GetString(bytes, 0, bytes.Length))
	{
	}

	internal DerVisibleString(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		m_value = str;
	}

	internal static DerVisibleString GetDerVisibleString(object obj)
	{
		if (obj == null || obj is DerVisibleString)
		{
			return (DerVisibleString)obj;
		}
		if (obj is Asn1Octet)
		{
			return new DerVisibleString(((Asn1Octet)obj).GetOctets());
		}
		if (obj is Asn1Tag)
		{
			return GetDerVisibleString(((Asn1Tag)obj).GetObject());
		}
		throw new ArgumentException("Invalid entry");
	}

	public static DerVisibleString GetDerVisibleString(Asn1Tag obj, bool explicitly)
	{
		return GetDerVisibleString(obj.GetObject());
	}

	public override string GetString()
	{
		return m_value;
	}

	protected override bool IsEquals(Asn1 asn1Object)
	{
		if (!(asn1Object is DerVisibleString derVisibleString))
		{
			return false;
		}
		return m_value.Equals(derVisibleString.m_value);
	}

	internal byte[] GetOctets()
	{
		return Encoding.UTF8.GetBytes(m_value);
	}

	internal override void Encode(DerStream derOut)
	{
		derOut.WriteEncoded(26, GetOctets());
	}
}
