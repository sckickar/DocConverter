using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal abstract class Asn1 : Asn1Encode
{
	private Asn1UniversalTags m_tag;

	private MemoryStream m_stream;

	internal Asn1()
	{
	}

	internal Asn1(Asn1UniversalTags tag)
	{
		m_tag = tag;
	}

	internal static Asn1 FromByteArray(byte[] data)
	{
		try
		{
			return new Asn1Stream(data).ReadAsn1();
		}
		catch (InvalidCastException)
		{
			throw new IOException("Invalid entry");
		}
	}

	internal static Asn1 FromStream(Stream stream)
	{
		try
		{
			return new Asn1Stream(stream).ReadAsn1();
		}
		catch (InvalidCastException)
		{
			throw new IOException("Invalid entry");
		}
	}

	public sealed override Asn1 GetAsn1()
	{
		return this;
	}

	internal bool Asn1Equals(Asn1 obj)
	{
		return IsEquals(obj);
	}

	internal int GetAsn1Hash()
	{
		return GetHashCode();
	}

	internal byte[] Asn1Encode(byte[] bytes)
	{
		m_stream = new MemoryStream();
		m_stream.WriteByte((byte)m_tag);
		Write(bytes.Length);
		m_stream.Write(bytes, 0, bytes.Length);
		m_stream.Dispose();
		return m_stream.ToArray();
	}

	internal new byte[] GetDerEncoded()
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			new DerStream(memoryStream).WriteObject(this);
			return memoryStream.ToArray();
		}
		catch (Exception)
		{
			return null;
		}
	}

	internal abstract void Encode(DerStream derOut);

	protected abstract bool IsEquals(Asn1 asn1Object);

	public new abstract int GetHashCode();

	private void Write(int length)
	{
		if (length > 127)
		{
			int num = 1;
			uint num2 = (uint)length;
			while ((num2 >>= 8) != 0)
			{
				num++;
			}
			m_stream.WriteByte((byte)((uint)num | 0x80u));
			for (int num3 = (num - 1) * 8; num3 >= 0; num3 -= 8)
			{
				m_stream.WriteByte((byte)(length >> num3));
			}
		}
		else
		{
			m_stream.WriteByte((byte)length);
		}
	}
}
