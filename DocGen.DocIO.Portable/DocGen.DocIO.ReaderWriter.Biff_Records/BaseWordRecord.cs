using System;
using System.IO;
using System.Text;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal abstract class BaseWordRecord
{
	private const int DEF_BITS_IN_BYTE = 8;

	private const int DEF_BITS_IN_SHORT = 16;

	private const int DEF_BITS_IN_INT = 32;

	internal virtual int Length => UnderlyingStructure?.Length ?? 0;

	protected virtual IDataStructure UnderlyingStructure
	{
		get
		{
			throw new Exception("UnderlyingStructure of BiffRecord");
		}
	}

	internal BaseWordRecord()
	{
	}

	internal BaseWordRecord(byte[] data)
	{
		Parse(data);
	}

	internal BaseWordRecord(byte[] arrData, int iOffset)
	{
		ParseBytes(arrData, iOffset);
	}

	internal BaseWordRecord(byte[] arrData, int iOffset, int iCount)
	{
		Parse(arrData, iOffset, iCount);
	}

	internal BaseWordRecord(Stream stream, int iCount)
	{
		Parse(stream, iCount);
	}

	internal static bool GetBit(byte btOptions, int bitPos)
	{
		if (bitPos < 0 || bitPos >= 8)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position cannot be less than 0 or greater than 7.");
		}
		return GetBit((int)btOptions, bitPos);
	}

	internal static bool GetBit(short sOptions, int bitPos)
	{
		switch (bitPos)
		{
		default:
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position cannot be less than 0 or greater 15.");
		case 15:
			return sOptions < 0;
		case 0:
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
			return GetBit((int)sOptions, bitPos);
		}
	}

	internal static bool GetBit(int iOptions, int bitPos)
	{
		switch (bitPos)
		{
		default:
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position cannot be less than 0 or greater 31.");
		case 31:
			return iOptions < 0;
		case 0:
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
			return (iOptions & (1 << bitPos)) != 0;
		}
	}

	internal static int GetBitsByMask(int value, int BitMask, int iStartBit)
	{
		return (value & BitMask) >> iStartBit;
	}

	internal static uint GetBitsByMask(uint value, int BitMask, int iStartBit)
	{
		return (uint)((value & BitMask) >> iStartBit);
	}

	internal static int SetBit(int iValue, int bitPos, bool value)
	{
		switch (bitPos)
		{
		default:
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position cannot be less than 0 or greater 32.");
		case 31:
			iValue = Math.Abs(iValue);
			if (!value)
			{
				iValue = -iValue;
			}
			break;
		case 0:
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
			iValue = ((!value) ? (iValue & ~(1 << bitPos)) : (iValue | (1 << bitPos)));
			break;
		}
		return iValue;
	}

	internal static int SetBitsByMask(int destination, int BitMask, int value)
	{
		destination &= ~BitMask;
		destination += value & BitMask;
		return destination;
	}

	internal static int SetBitsByMask(int destination, int BitMask, int iStartBit, int value)
	{
		destination &= ~BitMask;
		destination += (value << iStartBit) & BitMask;
		return destination;
	}

	internal static uint SetBitsByMask(uint destination, int BitMask, int value)
	{
		destination &= (uint)(~BitMask);
		destination += (uint)(value & BitMask);
		return destination;
	}

	internal static bool GetBit(uint uiOptions, int bitPos)
	{
		if (bitPos < 0 || bitPos > 32)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position cannot be less than 0 or greater 31.");
		}
		return (uiOptions & (1 << bitPos)) != 0;
	}

	internal static uint SetBit(uint uiValue, int bitPos, bool value)
	{
		if (bitPos < 0 || bitPos >= 32)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position can be zeroless or greater 32.");
		}
		uiValue = ((!value) ? (uiValue & (uint)(~(1 << bitPos))) : (uiValue | (uint)(1 << bitPos)));
		return uiValue;
	}

	internal static ushort ReadUInt16(Stream stream)
	{
		byte[] array = new byte[2];
		if (stream.Read(array, 0, 2) != 2)
		{
			throw new StreamReadException();
		}
		return BitConverter.ToUInt16(array, 0);
	}

	internal static uint ReadUInt32(Stream stream)
	{
		byte[] array = new byte[4];
		if (stream.Read(array, 0, 4) != 4)
		{
			throw new StreamReadException();
		}
		return BitConverter.ToUInt32(array, 0);
	}

	internal static short ReadInt16(Stream stream)
	{
		byte[] array = new byte[2];
		if (stream.Read(array, 0, 2) != 2)
		{
			throw new StreamReadException();
		}
		return BitConverter.ToInt16(array, 0);
	}

	internal static int ReadInt32(Stream stream)
	{
		byte[] array = new byte[4];
		if (stream.Read(array, 0, 4) != 4)
		{
			throw new StreamReadException();
		}
		return BitConverter.ToInt32(array, 0);
	}

	internal static ushort ReadUInt16(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length - 2)
		{
			throw new ArgumentOutOfRangeException("iOffset", "Value can not be less 0 and greater arrData.Length - Constants.BytesInWord");
		}
		return BitConverter.ToUInt16(arrData, iOffset);
	}

	internal static ushort ReadUInt16(byte[] arrData, ref int iOffset)
	{
		ushort result = ReadUInt16(arrData, iOffset);
		iOffset += 2;
		return result;
	}

	internal static string ReadString(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		int num = ReadUInt16(stream) * 2;
		if (num + stream.Position > stream.Length)
		{
			return string.Empty;
		}
		byte[] array = new byte[num];
		if (stream.Read(array, 0, num) != num)
		{
			throw new Exception("Unable to read required data from the stream");
		}
		return Encoding.Unicode.GetString(array, 0, array.Length);
	}

	internal static void WriteString(Stream stream, string str)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		WriteUInt16(stream, (ushort)(Encoding.Unicode.GetByteCount(str) / 2));
		byte[] bytes = Encoding.Unicode.GetBytes(str);
		stream.Write(bytes, 0, bytes.Length);
	}

	internal static string ReadString(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length - 2)
		{
			throw new ArgumentOutOfRangeException("iOffset", "Value can not be less 0 and greater arrData.Length - Constants.BytesInWord");
		}
		ushort count = ReadUInt16(arrData, iOffset);
		iOffset += 2;
		return Encoding.Unicode.GetString(arrData, iOffset, count);
	}

	internal static string ReadString(byte[] arrData, int iOffset, ushort usCount)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length - 2)
		{
			throw new ArgumentOutOfRangeException("iOffset", "Value can not be less 0 and greater arrData.Length - Constants.BytesInWord");
		}
		return Encoding.Unicode.GetString(arrData, iOffset, usCount);
	}

	internal static string GetZeroTerminatedString(byte[] arrData, int iOffset, out int iEndPos)
	{
		byte b = arrData[iOffset];
		iOffset += 2;
		string result = string.Empty;
		iEndPos = iOffset + b * 2;
		if (b != 0)
		{
			result = Encoding.Unicode.GetString(arrData, iOffset, b * 2);
		}
		for (int i = iEndPos; i < arrData.Length - 1; i++)
		{
			if (arrData[i] == 0 && arrData[i + 1] == 0)
			{
				iEndPos += 2;
				return result;
			}
		}
		throw new Exception("Stored string should be zero-ended");
	}

	internal static byte[] ToZeroTerminatedArray(string str)
	{
		byte[] array = new byte[str.Length * 2 + 4];
		array[0] = (byte)str.Length;
		Encoding.Unicode.GetBytes(str.ToCharArray(), 0, str.Length, array, 2);
		return array;
	}

	internal static void WriteUInt16(byte[] arrData, ushort usValue, ref int iOffset)
	{
		iOffset = WriteBytes(arrData, BitConverter.GetBytes(usValue), iOffset);
	}

	internal static void WriteUInt32(byte[] arrData, uint uintValue, ref int iOffset)
	{
		iOffset = WriteBytes(arrData, BitConverter.GetBytes(uintValue), iOffset);
	}

	internal static void WriteUInt32(Stream stream, uint value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
	}

	internal static void WriteInt32(Stream stream, int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
	}

	internal static void WriteInt16(Stream stream, short value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
	}

	internal static void WriteUInt16(Stream stream, ushort value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
	}

	internal static void WriteString(byte[] arrData, string strValue, ref int iOffset)
	{
		Encoding unicode = Encoding.Unicode;
		iOffset = WriteBytes(arrData, unicode.GetBytes(strValue), iOffset);
	}

	internal static int WriteBytes(byte[] arrData, byte[] bytes, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length - 2)
		{
			throw new ArgumentOutOfRangeException("iOffset", "Value can not be less 0 and greater arrData.Length - Constants.BytesInWord");
		}
		bytes.CopyTo(arrData, iOffset);
		return iOffset + bytes.Length;
	}

	internal byte[] ReadBytes(Stream stream, int i)
	{
		byte[] array = new byte[i];
		if (stream.Read(array, 0, i) != i)
		{
			throw new StreamReadException();
		}
		return array;
	}

	internal virtual void Parse(byte[] data)
	{
		ParseBytes(data, 0);
	}

	internal virtual void ParseBytes(byte[] arrData, int iOffset)
	{
		Parse(arrData, iOffset, arrData.Length - iOffset);
	}

	internal virtual void Parse(byte[] arrData, int iOffset, int iCount)
	{
		if (UnderlyingStructure == null)
		{
			throw new ArgumentNullException("UnderlyingStructure");
		}
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		if (iCount < 0)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		if (iOffset + iCount > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset + iCount");
		}
		UnderlyingStructure.Parse(arrData, iOffset);
	}

	internal virtual void Parse(Stream stream, int iCount)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (iCount < 0)
		{
			throw new ArgumentOutOfRangeException("iCount cannot be less than 0");
		}
		byte[] array = new byte[iCount];
		if (stream.Read(array, 0, iCount) != iCount)
		{
			throw new Exception("Couldn't read required bytes from the stream");
		}
		Parse(array, 0, iCount);
	}

	internal virtual int Save(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		int result = arrData.Length - iOffset;
		(UnderlyingStructure ?? throw new ArgumentNullException("UnderlyingStructure")).Save(arrData, iOffset);
		return result;
	}

	internal virtual int Save(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		int length = Length;
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("iLength");
		}
		byte[] array = new byte[length];
		Save(array, 0);
		stream.Write(array, 0, length);
		return length;
	}

	internal virtual void Close()
	{
	}
}
