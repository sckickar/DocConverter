using System;
using System.Text;

namespace DocGen.Pdf.Parsing;

internal class SystemFontOpenTypeFontReader : SystemFontReaderBase
{
	public SystemFontOpenTypeFontReader(byte[] data)
		: base(data)
	{
	}

	public sbyte ReadChar()
	{
		return (sbyte)Read();
	}

	public ushort ReadUShort()
	{
		byte[] array = new byte[2];
		ReadBE(array, 2);
		return BitConverter.ToUInt16(array, 0);
	}

	public short ReadShort()
	{
		byte[] array = new byte[2];
		ReadBE(array, 2);
		return BitConverter.ToInt16(array, 0);
	}

	public uint ReadULong()
	{
		byte[] array = new byte[4];
		ReadBE(array, 4);
		return BitConverter.ToUInt32(array, 0);
	}

	public int ReadLong()
	{
		byte[] array = new byte[4];
		ReadBE(array, 4);
		return BitConverter.ToInt32(array, 0);
	}

	public long ReadLongDateTime()
	{
		byte[] array = new byte[8];
		ReadBE(array, 8);
		return BitConverter.ToInt64(array, 0);
	}

	public float ReadFixed()
	{
		return (float)ReadShort() + (float)(ReadUShort() / 65536);
	}

	public float Read2Dot14()
	{
		return (float)ReadShort() / 16384f;
	}

	public string ReadString()
	{
		byte b = Read();
		byte[] array = new byte[b];
		for (int i = 0; i < b; i++)
		{
			array[i] = Read();
		}
		return Encoding.UTF8.GetString(array, 0, array.Length);
	}
}
