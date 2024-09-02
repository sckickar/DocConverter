using System;
using System.Text;

namespace DocGen.Pdf.Security;

internal class DerPrintableString : DerString
{
	private string m_value;

	internal DerPrintableString(byte[] bytes)
		: this(Encoding.UTF8.GetString(bytes, 0, bytes.Length))
	{
	}

	internal DerPrintableString(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_value = value;
	}

	internal byte[] Asn1Encode()
	{
		return Asn1Encode(GetBytes());
	}

	public override string GetString()
	{
		return m_value;
	}

	public byte[] GetBytes()
	{
		return Encoding.UTF8.GetBytes(m_value);
	}

	internal override void Encode(DerStream stream)
	{
		stream.WriteEncoded(19, GetBytes());
	}

	protected override bool IsEquals(Asn1 asn1)
	{
		if (!(asn1 is DerPrintableString derPrintableString))
		{
			return false;
		}
		return m_value.Equals(derPrintableString.m_value);
	}
}
