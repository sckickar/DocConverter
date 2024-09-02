using System;
using System.IO;
using System.Text;

namespace DocGen.Pdf;

internal class EndianBinaryReader : BinaryReader
{
	private bool _bigEndian;

	public EndianBinaryReader(Stream input)
		: base(input)
	{
	}

	public EndianBinaryReader(Stream input, Encoding encoding)
		: base(input, encoding)
	{
	}

	public EndianBinaryReader(Stream input, Encoding encoding, bool bigEndian)
		: base(input, encoding)
	{
		_bigEndian = bigEndian;
	}

	public EndianBinaryReader(Stream input, bool bigEndian)
		: base(input, bigEndian ? Encoding.BigEndianUnicode : Encoding.UTF8)
	{
		_bigEndian = bigEndian;
	}

	public override short ReadInt16()
	{
		if (_bigEndian)
		{
			byte[] array = ReadBytes(2);
			Array.Reverse(array);
			return BitConverter.ToInt16(array, 0);
		}
		return base.ReadInt16();
	}

	public override int ReadInt32()
	{
		if (_bigEndian)
		{
			byte[] array = ReadBytes(4);
			Array.Reverse(array);
			return BitConverter.ToInt32(array, 0);
		}
		return base.ReadInt32();
	}

	public override long ReadInt64()
	{
		if (_bigEndian)
		{
			byte[] array = ReadBytes(8);
			Array.Reverse(array);
			return BitConverter.ToInt64(array, 0);
		}
		return base.ReadInt64();
	}

	public override float ReadSingle()
	{
		if (_bigEndian)
		{
			byte[] array = ReadBytes(4);
			Array.Reverse(array);
			return BitConverter.ToSingle(array, 0);
		}
		return base.ReadSingle();
	}

	public override ushort ReadUInt16()
	{
		if (_bigEndian)
		{
			byte[] array = ReadBytes(2);
			Array.Reverse(array);
			return BitConverter.ToUInt16(array, 0);
		}
		return base.ReadUInt16();
	}

	public override uint ReadUInt32()
	{
		if (_bigEndian)
		{
			byte[] array = ReadBytes(4);
			Array.Reverse(array);
			return BitConverter.ToUInt32(array, 0);
		}
		return base.ReadUInt32();
	}

	public override ulong ReadUInt64()
	{
		if (_bigEndian)
		{
			byte[] array = ReadBytes(8);
			Array.Reverse(array);
			return BitConverter.ToUInt64(array, 0);
		}
		return base.ReadUInt64();
	}
}
