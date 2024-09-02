using System;
using System.Text;

namespace DocGen.Pdf.Parsing;

internal class SystemFontCFFFontReader : SystemFontReaderBase
{
	public SystemFontCFFFontReader(byte[] data)
		: base(data)
	{
	}

	public byte ReadCard8()
	{
		return Read();
	}

	public ushort ReadCard16()
	{
		byte[] array = new byte[2];
		ReadBE(array, 2);
		return BitConverter.ToUInt16(array, 0);
	}

	public uint ReadCard24()
	{
		byte[] array = new byte[4];
		ReadBE(array, 3);
		return BitConverter.ToUInt32(array, 0);
	}

	public uint ReadCard32()
	{
		byte[] array = new byte[4];
		ReadBE(array, 4);
		return BitConverter.ToUInt32(array, 0);
	}

	public uint ReadOffset(byte offsetSize)
	{
		return offsetSize switch
		{
			1 => ReadCard8(), 
			2 => ReadCard16(), 
			3 => ReadCard24(), 
			4 => ReadCard32(), 
			_ => throw new NotSupportedException(), 
		};
	}

	public byte ReadOffSize()
	{
		return ReadCard8();
	}

	public ushort ReadSID()
	{
		return ReadCard16();
	}

	public string ReadString(int length)
	{
		byte[] array = new byte[length];
		Read(array, length);
		return Encoding.UTF8.GetString(array, 0, array.Length);
	}
}
