using System;

namespace DocGen.Pdf.Security;

internal class Asn1Boolean : Asn1
{
	private bool m_value;

	public Asn1Boolean(bool value)
		: base(Asn1UniversalTags.Boolean)
	{
		m_value = value;
	}

	public Asn1Boolean(byte[] bytes)
		: base(Asn1UniversalTags.Boolean)
	{
		m_value = bytes[0] == byte.MaxValue;
	}

	private byte[] ToArray()
	{
		return new byte[1] { (byte)(m_value ? 255 : 0) };
	}

	public byte[] AsnEncode()
	{
		return Asn1Encode(ToArray());
	}

	protected override bool IsEquals(Asn1 asn1Object)
	{
		throw new NotImplementedException();
	}

	public override int GetHashCode()
	{
		throw new NotImplementedException();
	}

	internal override void Encode(DerStream derOut)
	{
		derOut.WriteEncoded(1, ToArray());
	}
}
