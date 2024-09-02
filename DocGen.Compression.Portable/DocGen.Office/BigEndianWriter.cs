using System;
using System.Text;

namespace DocGen.Office;

internal class BigEndianWriter
{
	internal const int Int32Size = 4;

	internal const int Int16Size = 2;

	internal const int Int64Size = 8;

	private readonly Encoding c_encoding = new Windows1252Encoding();

	private const float c_fraction = 16384f;

	private byte[] m_buffer;

	private int m_position;

	public byte[] Data => m_buffer;

	public int Position => m_position;

	public BigEndianWriter(int capacity)
	{
		m_buffer = new byte[capacity];
	}

	public void Write(short value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse(bytes);
		Flush(bytes);
	}

	public void Write(ushort value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse(bytes);
		Flush(bytes);
	}

	public void Write(int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse(bytes);
		Flush(bytes);
	}

	public void Write(uint value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse(bytes);
		Flush(bytes);
	}

	public void Write(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		byte[] bytes = c_encoding.GetBytes(value);
		Flush(bytes);
	}

	public void Write(byte[] value)
	{
		Flush(value);
	}

	private void Flush(byte[] buff)
	{
		if (buff == null)
		{
			throw new ArgumentNullException("buff");
		}
		Array.Copy(buff, 0, m_buffer, m_position, buff.Length);
		m_position += buff.Length;
	}
}
