using System;
using System.Text;

namespace DocGen.Pdf.Security;

internal class DerBitString : DerString
{
	private static readonly char[] m_table = new char[16]
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'A', 'B', 'C', 'D', 'E', 'F'
	};

	private byte[] m_data;

	private int m_extra;

	internal int ExtraBits => m_extra;

	public int Value
	{
		get
		{
			int num = 0;
			for (int i = 0; i != m_data.Length && i != 4; i++)
			{
				num |= (m_data[i] & 0xFF) << 8 * i;
			}
			return num;
		}
	}

	internal DerBitString(byte data, int pad)
	{
		m_data = new byte[1] { data };
		m_extra = pad;
	}

	internal DerBitString(byte[] data, int pad)
	{
		m_data = data;
		m_extra = pad;
	}

	internal DerBitString(byte[] data)
	{
		m_data = data;
	}

	internal DerBitString(Asn1Encode asn1)
	{
		m_data = asn1.GetDerEncoded();
	}

	public byte[] GetBytes()
	{
		return m_data;
	}

	internal override void Encode(DerStream stream)
	{
		byte[] array = new byte[GetBytes().Length + 1];
		array[0] = (byte)m_extra;
		Array.Copy(GetBytes(), 0, array, 1, array.Length - 1);
		stream.WriteEncoded(3, array);
	}

	public override int GetHashCode()
	{
		return m_extra.GetHashCode() ^ Asn1Constants.GetHashCode(m_data);
	}

	protected override bool IsEquals(Asn1 asn1)
	{
		if (!(asn1 is DerBitString derBitString))
		{
			return false;
		}
		if (m_extra == derBitString.m_extra)
		{
			return Asn1Constants.AreEqual(m_data, derBitString.m_data);
		}
		return false;
	}

	public override string GetString()
	{
		StringBuilder stringBuilder = new StringBuilder("#");
		byte[] derEncoded = GetDerEncoded();
		for (int i = 0; i != derEncoded.Length; i++)
		{
			uint num = derEncoded[i];
			stringBuilder.Append(m_table[(num >> 4) & 0xF]);
			stringBuilder.Append(m_table[derEncoded[i] & 0xF]);
		}
		return stringBuilder.ToString();
	}

	internal static DerBitString FromAsn1Octets(byte[] bytes)
	{
		int pad = bytes[0];
		byte[] array = new byte[bytes.Length - 1];
		Array.Copy(bytes, 1, array, 0, array.Length);
		return new DerBitString(array, pad);
	}

	internal static DerBitString GetString(object obj)
	{
		if (obj == null || obj is DerBitString)
		{
			return (DerBitString)obj;
		}
		throw new ArgumentException("Invalid entry");
	}

	internal static DerBitString GetString(Asn1Tag tag, bool isExplicit)
	{
		Asn1 @object = tag.GetObject();
		if (isExplicit || @object is DerBitString)
		{
			return GetString(@object);
		}
		return FromAsn1Octets(((Asn1Octet)@object).GetOctets());
	}
}
