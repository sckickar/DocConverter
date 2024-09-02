using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal class Asn1StreamHelper : BaseStream
{
	private byte[] m_empty = new byte[0];

	private int m_originalLength;

	private int m_remaining;

	internal new int Remaining => m_remaining;

	internal Asn1StreamHelper(Stream stream, int length)
		: base(stream, length)
	{
		if (length < 0)
		{
			throw new ArgumentException("Invalid length specified");
		}
		m_originalLength = length;
		m_remaining = length;
		if (length == 0)
		{
			SetParentEndOfFileDetect(isDetect: true);
		}
	}

	public override int ReadByte()
	{
		if (m_remaining == 0)
		{
			return -1;
		}
		int num = m_input.ReadByte();
		if (num < 0)
		{
			throw new EndOfStreamException("Invalid length in bytes");
		}
		if (--m_remaining == 0)
		{
			SetParentEndOfFileDetect(isDetect: true);
		}
		return num;
	}

	public override int Read(byte[] bytes, int offset, int length)
	{
		if (m_remaining == 0)
		{
			return 0;
		}
		int count = Math.Min(length, m_remaining);
		int num = m_input.Read(bytes, offset, count);
		if (num < 1)
		{
			throw new EndOfStreamException("Object truncated");
		}
		if ((m_remaining -= num) == 0)
		{
			SetParentEndOfFileDetect(isDetect: true);
		}
		return num;
	}

	internal void ReadAll(byte[] bytes)
	{
		if (m_remaining != bytes.Length)
		{
			throw new ArgumentException("Invalid length in bytes");
		}
		if ((m_remaining -= Read(m_input, bytes, 0, bytes.Length)) != 0)
		{
			throw new EndOfStreamException("Object truncated");
		}
		SetParentEndOfFileDetect(isDetect: true);
	}

	internal byte[] ToArray()
	{
		if (m_remaining == 0)
		{
			return m_empty;
		}
		byte[] array = new byte[m_remaining];
		if ((m_remaining -= Read(m_input, array, 0, array.Length)) != 0)
		{
			throw new EndOfStreamException("Object truncated");
		}
		SetParentEndOfFileDetect(isDetect: true);
		return array;
	}

	private int Read(Stream stream, byte[] bytes, int offset, int length)
	{
		int i;
		int num;
		for (i = 0; i < length; i += num)
		{
			num = stream.Read(bytes, offset + i, length - i);
			if (num < 1)
			{
				break;
			}
		}
		return i;
	}
}
