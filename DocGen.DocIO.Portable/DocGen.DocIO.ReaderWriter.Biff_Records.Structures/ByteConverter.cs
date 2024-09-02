using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

internal static class ByteConverter
{
	internal static short ReadInt16(byte[] arrData, ref int iOffset)
	{
		short result = BitConverter.ToInt16(arrData, iOffset);
		iOffset += 2;
		return result;
	}

	internal static int ReadInt32(byte[] arrData, ref int iOffset)
	{
		int result = BitConverter.ToInt32(arrData, iOffset);
		iOffset += 4;
		return result;
	}

	internal static long ReadInt64(byte[] arrData, ref int iOffset)
	{
		long result = BitConverter.ToInt64(arrData, iOffset);
		iOffset += 8;
		return result;
	}

	internal static ushort ReadUInt16(byte[] arrData, ref int iOffset)
	{
		ushort result = BitConverter.ToUInt16(arrData, iOffset);
		iOffset += 2;
		return result;
	}

	internal static uint ReadUInt32(byte[] arrData, ref int iOffset)
	{
		uint result = BitConverter.ToUInt32(arrData, iOffset);
		iOffset += 4;
		return result;
	}

	internal static byte[] ReadBytes(byte[] arrData, int length, ref int iOffset)
	{
		byte[] array = new byte[length];
		for (int i = 0; i < length && iOffset + i < arrData.Length; i++)
		{
			array[i] = arrData[iOffset + i];
		}
		iOffset += length;
		return array;
	}

	internal static void WriteInt16(byte[] destination, ref int iOffset, short val)
	{
		byte[] bytes = BitConverter.GetBytes(val);
		WriteBytes(destination, ref iOffset, bytes);
	}

	internal static void WriteUInt16(byte[] destination, ref int iOffset, ushort val)
	{
		byte[] bytes = BitConverter.GetBytes(val);
		WriteBytes(destination, ref iOffset, bytes);
	}

	internal static void WriteInt32(byte[] destination, ref int iOffset, int val)
	{
		byte[] bytes = BitConverter.GetBytes(val);
		WriteBytes(destination, ref iOffset, bytes);
	}

	internal static void WriteInt64(byte[] destination, ref int iOffset, long val)
	{
		byte[] bytes = BitConverter.GetBytes(val);
		WriteBytes(destination, ref iOffset, bytes);
	}

	internal static void WriteUInt32(byte[] destination, ref int iOffset, uint val)
	{
		byte[] bytes = BitConverter.GetBytes(val);
		WriteBytes(destination, ref iOffset, bytes);
	}

	internal static void WriteBytes(byte[] destination, ref int iOffset, byte[] bytes)
	{
		int num = bytes.Length;
		for (int i = 0; i < num; i++)
		{
			destination[iOffset + i] = bytes[i];
		}
		iOffset += num;
	}

	internal static void CopyMemory(byte[] destination, byte[] source, int length)
	{
		for (int i = 0; i < length; i++)
		{
			destination[i] = source[i];
		}
	}
}
