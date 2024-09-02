using System;

namespace DocGen.Pdf.Security;

internal class DerCatalogue : Asn1
{
	private readonly byte[] m_bytes;

	internal Number Value => new Number(m_bytes);

	internal DerCatalogue GetEnumeration(object obj)
	{
		if (obj == null || obj is DerCatalogue)
		{
			return (DerCatalogue)obj;
		}
		throw new ArgumentException("Invalid entry" + obj.GetType().Name);
	}

	internal DerCatalogue(int value)
	{
		m_bytes = Number.ValueOf(value).ToByteArray();
	}

	internal DerCatalogue()
	{
	}

	internal DerCatalogue(byte[] bytes)
	{
		m_bytes = bytes;
	}

	internal override void Encode(DerStream stream)
	{
		stream.WriteEncoded(10, m_bytes);
	}

	protected override bool IsEquals(Asn1 asn1)
	{
		if (!(asn1 is DerCatalogue derCatalogue))
		{
			return false;
		}
		return Asn1Constants.AreEqual(m_bytes, derCatalogue.m_bytes);
	}

	public override int GetHashCode()
	{
		return Asn1Constants.GetHashCode(m_bytes);
	}
}
