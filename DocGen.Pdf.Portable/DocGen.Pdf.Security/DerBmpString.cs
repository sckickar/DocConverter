using System;

namespace DocGen.Pdf.Security;

internal class DerBmpString : DerString
{
	private string m_value;

	internal DerBmpString(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		char[] array = new char[bytes.Length / 2];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = (char)((uint)(bytes[2 * i] << 8) | (bytes[2 * i + 1] & 0xFFu));
		}
		m_value = new string(array);
	}

	public override string GetString()
	{
		return m_value;
	}

	protected override bool IsEquals(Asn1 asn1)
	{
		if (!(asn1 is DerBmpString derBmpString))
		{
			return false;
		}
		return m_value.Equals(derBmpString.m_value);
	}

	internal override void Encode(DerStream stream)
	{
		char[] array = m_value.ToCharArray();
		byte[] array2 = new byte[array.Length * 2];
		for (int i = 0; i != array.Length; i++)
		{
			array2[2 * i] = (byte)((int)array[i] >> 8);
			array2[2 * i + 1] = (byte)array[i];
		}
		stream.WriteEncoded(30, array2);
	}
}
