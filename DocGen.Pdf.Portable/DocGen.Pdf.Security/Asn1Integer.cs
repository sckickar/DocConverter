using System;

namespace DocGen.Pdf.Security;

internal class Asn1Integer : Asn1
{
	private long m_value;

	private byte[] m_bytes;

	public Asn1Integer(long value)
		: base(Asn1UniversalTags.Integer)
	{
		m_value = value;
	}

	public Asn1Integer(byte[] value)
		: base(Asn1UniversalTags.Integer)
	{
		m_bytes = value;
	}

	private byte[] ToArray()
	{
		if (m_value < 255)
		{
			return new byte[1] { (byte)m_value };
		}
		return BitConverter.GetBytes(m_value);
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

	internal override void Encode(DerStream stream)
	{
		stream.WriteEncoded(2, m_bytes);
	}
}
