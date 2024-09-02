using System;

namespace DocGen.Pdf.Security;

internal class DerTeleText : DerString
{
	private string m_value;

	internal DerTeleText(byte[] bytes)
		: this(FromByteArray(bytes))
	{
	}

	public DerTeleText(string value)
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

	internal override void Encode(DerStream stream)
	{
		stream.WriteEncoded(20, GetBytes());
	}

	public byte[] GetBytes()
	{
		return ToByteArray(m_value);
	}

	protected override bool IsEquals(Asn1 asn1)
	{
		if (!(asn1 is DerTeleText derTeleText))
		{
			return false;
		}
		return m_value.Equals(derTeleText.m_value);
	}

	private byte[] ToByteArray(string value)
	{
		byte[] array = new byte[value.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Convert.ToByte(value[i]);
		}
		return array;
	}

	internal static DerTeleText GetTeleText(object obj)
	{
		if (obj == null || obj is DerTeleText)
		{
			return (DerTeleText)obj;
		}
		throw new ArgumentException("Invalid entry");
	}

	internal static DerTeleText GetTeleText(Asn1Tag tag, bool isExplicit)
	{
		Asn1 @object = tag.GetObject();
		if (isExplicit || @object is DerTeleText)
		{
			return GetTeleText(@object);
		}
		return new DerTeleText(Asn1Octet.GetOctetString(@object).GetOctets());
	}

	internal new static string FromByteArray(byte[] bytes)
	{
		char[] array = new char[bytes.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Convert.ToChar(bytes[i]);
		}
		return new string(array);
	}
}
