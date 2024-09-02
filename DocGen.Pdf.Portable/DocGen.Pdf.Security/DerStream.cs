using System.IO;

namespace DocGen.Pdf.Security;

internal class DerStream
{
	internal Stream m_stream;

	internal DerStream(Stream stream)
	{
		m_stream = stream;
	}

	internal DerStream()
	{
	}

	private void WriteLength(int length)
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

	internal void WriteEncoded(int tagNumber, byte[] bytes)
	{
		m_stream.WriteByte((byte)tagNumber);
		WriteLength(bytes.Length);
		m_stream.Write(bytes, 0, bytes.Length);
	}

	internal void WriteEncoded(int tagNumber, byte[] bytes, int offset, int length)
	{
		m_stream.WriteByte((byte)tagNumber);
		WriteLength(length);
		m_stream.Write(bytes, offset, length);
	}

	internal void WriteTag(int flag, int tagNumber)
	{
		if (tagNumber < 31)
		{
			m_stream.WriteByte((byte)(flag | tagNumber));
			return;
		}
		m_stream.WriteByte((byte)((uint)flag | 0x1Fu));
		if (tagNumber < 128)
		{
			m_stream.WriteByte((byte)tagNumber);
			return;
		}
		byte[] array = new byte[5];
		int num = array.Length;
		array[--num] = (byte)((uint)tagNumber & 0x7Fu);
		do
		{
			tagNumber >>= 7;
			array[--num] = (byte)(((uint)tagNumber & 0x7Fu) | 0x80u);
		}
		while (tagNumber > 127);
		m_stream.Write(array, num, array.Length - num);
	}

	internal void WriteEncoded(int flag, int tagNumber, byte[] bytes)
	{
		WriteTag(flag, tagNumber);
		WriteLength(bytes.Length);
		m_stream.Write(bytes, 0, bytes.Length);
	}

	public void Close()
	{
		m_stream.Dispose();
	}

	internal virtual void WriteObject(object obj)
	{
		if (obj == null)
		{
			m_stream.WriteByte(5);
			m_stream.WriteByte(0);
			return;
		}
		if (obj is Asn1)
		{
			((Asn1)obj).Encode(this);
			return;
		}
		if (obj is Asn1Encode)
		{
			((Asn1Encode)obj).GetAsn1().Encode(this);
			return;
		}
		throw new IOException("Invalid object specified");
	}
}
