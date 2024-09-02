using System;
using System.Text;

namespace DocGen.Pdf.Graphics;

internal abstract class CatalogedReaderBase
{
	private bool m_isBigEndian;

	internal bool IsBigEndian
	{
		get
		{
			return m_isBigEndian;
		}
		set
		{
			m_isBigEndian = value;
		}
	}

	internal abstract long Length { get; }

	internal CatalogedReaderBase(bool isBigEndian)
	{
		m_isBigEndian = isBigEndian;
	}

	internal abstract CatalogedReaderBase WithByteOrder(bool isBigEndian);

	internal abstract CatalogedReaderBase WithShiftedBaseOffset(int shift);

	internal abstract int ToUnshiftedOffset(int offset);

	internal abstract byte ReadByte(int index);

	internal abstract byte[] GetBytes(int index, int count);

	internal abstract bool IsValidIndex(int index, int length);

	internal sbyte ReadSignedByte(int index)
	{
		return (sbyte)ReadByte(index);
	}

	internal ushort ReadUInt16(int index)
	{
		if (m_isBigEndian)
		{
			return (ushort)((ReadByte(index) << 8) | ReadByte(index + 1));
		}
		return (ushort)((ReadByte(index + 1) << 8) | ReadByte(index));
	}

	internal short ReadInt16(int index)
	{
		if (m_isBigEndian)
		{
			return (short)((ReadByte(index) << 8) | ReadByte(index + 1));
		}
		return (short)((ReadByte(index + 1) << 8) | ReadByte(index));
	}

	internal int ReadInt24(int index)
	{
		if (m_isBigEndian)
		{
			return (ReadByte(index) << 16) | (ReadByte(index + 1) << 8) | ReadByte(index + 2);
		}
		return (ReadByte(index + 2) << 16) | (ReadByte(index + 1) << 8) | ReadByte(index);
	}

	internal uint ReadUInt32(int index)
	{
		if (m_isBigEndian)
		{
			return (uint)((ReadByte(index) << 24) | (ReadByte(index + 1) << 16) | (ReadByte(index + 2) << 8) | ReadByte(index + 3));
		}
		return (uint)((ReadByte(index + 3) << 24) | (ReadByte(index + 2) << 16) | (ReadByte(index + 1) << 8) | ReadByte(index));
	}

	internal int ReadInt32(int index)
	{
		if (m_isBigEndian)
		{
			return (ReadByte(index) << 24) | (ReadByte(index + 1) << 16) | (ReadByte(index + 2) << 8) | ReadByte(index + 3);
		}
		return (ReadByte(index + 3) << 24) | (ReadByte(index + 2) << 16) | (ReadByte(index + 1) << 8) | ReadByte(index);
	}

	internal long ReadInt64(int index)
	{
		if (m_isBigEndian)
		{
			return (long)(((ulong)ReadByte(index) << 56) | ((ulong)ReadByte(index + 1) << 48) | ((ulong)ReadByte(index + 2) << 40) | ((ulong)ReadByte(index + 3) << 32) | ((ulong)ReadByte(index + 4) << 24) | ((ulong)ReadByte(index + 5) << 16) | ((ulong)ReadByte(index + 6) << 8) | ReadByte(index + 7));
		}
		return (long)(((ulong)ReadByte(index + 7) << 56) | ((ulong)ReadByte(index + 6) << 48) | ((ulong)ReadByte(index + 5) << 40) | ((ulong)ReadByte(index + 4) << 32) | ((ulong)ReadByte(index + 3) << 24) | ((ulong)ReadByte(index + 2) << 16) | ((ulong)ReadByte(index + 1) << 8) | ReadByte(index));
	}

	internal float ReadFloat32(int index)
	{
		return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt32(index)), 0);
	}

	internal double ReadDouble64(int index)
	{
		return BitConverter.Int64BitsToDouble(ReadInt64(index));
	}

	internal string ReadString(int index, int length, Encoding encoding)
	{
		byte[] bytes = GetBytes(index, length);
		return encoding.GetString(bytes, 0, bytes.Length);
	}

	internal string ReadNullTerminatedString(int index, int length, Encoding encoding)
	{
		byte[] array = ReadNullTerminatedBytes(index, length);
		return ((encoding == null) ? Encoding.UTF8 : encoding).GetString(array, 0, array.Length);
	}

	internal string ReadNullTerminatedStringValue(int index, int length, Encoding encoding)
	{
		byte[] array = ReadNullTerminatedBytes(index, length);
		return encoding.GetString(array, 0, array.Length);
	}

	internal byte[] ReadNullTerminatedBytes(int index, int maxLength)
	{
		byte[] bytes = GetBytes(index, maxLength);
		int i;
		for (i = 0; i < bytes.Length && bytes[i] != 0; i++)
		{
		}
		if (i == maxLength)
		{
			return bytes;
		}
		byte[] array = new byte[i];
		if (i > 0)
		{
			Array.Copy(bytes, 0, array, 0, i);
		}
		return array;
	}
}
