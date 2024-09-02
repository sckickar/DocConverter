using System;
using System.IO;
using System.Text;

namespace DocGen.Office;

internal class BigEndianReader
{
	internal const int Int32Size = 4;

	internal const int Int16Size = 2;

	internal const int Int64Size = 8;

	private readonly Encoding c_encoding = new Windows1252Encoding();

	private const float c_fraction = 16384f;

	private BinaryReader m_reader;

	public BinaryReader Reader
	{
		get
		{
			return m_reader;
		}
		set
		{
			m_reader = value;
		}
	}

	public Stream BaseStream => m_reader.BaseStream;

	public BigEndianReader(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		m_reader = reader;
	}

	public void Close()
	{
		if (m_reader != null)
		{
			if (m_reader.BaseStream != null)
			{
				m_reader.BaseStream.Dispose();
			}
			m_reader.Dispose();
			m_reader = null;
		}
	}

	public void Seek(long position)
	{
		if (m_reader.BaseStream.CanSeek && position <= int.MaxValue)
		{
			m_reader.BaseStream.Position = position;
		}
	}

	public void Skip(long numBytes)
	{
		Seek(m_reader.BaseStream.Position + numBytes);
	}

	public byte[] Reverse(byte[] buffer)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		Array.Reverse(buffer);
		return buffer;
	}

	public long ReadInt64()
	{
		byte[] buffer = m_reader.ReadBytes(8);
		buffer = Reverse(buffer);
		long result = 0L;
		if (buffer.Length < 8)
		{
			return result;
		}
		return BitConverter.ToInt64(buffer, 0);
	}

	public ulong ReadUInt64()
	{
		byte[] buffer = m_reader.ReadBytes(8);
		buffer = Reverse(buffer);
		ulong result = 0uL;
		if (buffer.Length < 8)
		{
			return result;
		}
		return BitConverter.ToUInt64(buffer, 0);
	}

	public int ReadInt32()
	{
		byte[] buffer = m_reader.ReadBytes(4);
		buffer = Reverse(buffer);
		int result = 0;
		if (buffer.Length < 4)
		{
			return result;
		}
		return BitConverter.ToInt32(buffer, 0);
	}

	public uint ReadUInt32()
	{
		byte[] buffer = m_reader.ReadBytes(4);
		buffer = Reverse(buffer);
		uint result = 0u;
		if (buffer.Length < 4)
		{
			return result;
		}
		return BitConverter.ToUInt32(buffer, 0);
	}

	public short ReadInt16()
	{
		byte[] buffer = m_reader.ReadBytes(2);
		buffer = Reverse(buffer);
		short result = 0;
		if (buffer.Length < 2)
		{
			return result;
		}
		return BitConverter.ToInt16(buffer, 0);
	}

	public ushort ReadUInt16()
	{
		byte[] buffer = m_reader.ReadBytes(2);
		buffer = Reverse(buffer);
		ushort result = 0;
		if (buffer.Length < 2)
		{
			return result;
		}
		return BitConverter.ToUInt16(buffer, 0);
	}

	public byte ReadByte()
	{
		return m_reader.ReadByte();
	}

	public float ReadFixed()
	{
		byte[] buffer = m_reader.ReadBytes(2);
		buffer = Reverse(buffer);
		short num = BitConverter.ToInt16(buffer, 0);
		buffer = m_reader.ReadBytes(2);
		buffer = Reverse(buffer);
		float num2 = (float)BitConverter.ToInt16(buffer, 0) / 16384f;
		return (float)num + num2;
	}

	public byte[] ReadBytes(int count)
	{
		return m_reader.ReadBytes(count);
	}

	public string ReadString(int len)
	{
		return ReadString(len, unicode: false);
	}

	public string ReadString(int len, bool unicode)
	{
		string text = null;
		if (unicode)
		{
			byte[] array = ReadBytes(len);
			return Encoding.BigEndianUnicode.GetString(array, 0, array.Length);
		}
		byte[] array2 = ReadBytes(len);
		return c_encoding.GetString(array2, 0, array2.Length);
	}

	public string ReadUtf8String(int len)
	{
		byte[] array = ReadBytes(len);
		return Encoding.UTF8.GetString(array, 0, array.Length);
	}

	public int Read(byte[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		int num = 0;
		int num2 = 0;
		do
		{
			num2 = m_reader.Read(buffer, index + num, count - num);
			num += num2;
		}
		while (num < count);
		return num;
	}
}
