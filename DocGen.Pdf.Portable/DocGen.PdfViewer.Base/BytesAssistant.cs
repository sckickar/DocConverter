using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal static class BytesAssistant
{
	public const int CID_BYTES_COUNT = 4;

	public static char GetUnicodeChar(int i)
	{
		return (char)i;
	}

	public static int GetInt(byte[] bytes, int offset, int count)
	{
		byte[] array = new byte[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = bytes[offset + i];
		}
		return GetInt(array);
	}

	public static int GetInt(byte[] bytes)
	{
		byte[] bytes2 = GetBytes(bytes);
		if (bytes2 == null)
		{
			return -1;
		}
		int num = 0;
		int num2 = bytes2.Length;
		for (int i = 0; i < num2; i++)
		{
			num |= ((num2 > i) ? (bytes2[i] & 0xFF) : 0);
			if (i < num2 - 1)
			{
				num <<= 8;
			}
		}
		return num;
	}

	private static byte[] GetBytes(byte[] bytes)
	{
		if (bytes == null || bytes.Length > 4)
		{
			return null;
		}
		if (bytes.Length == 0)
		{
			return bytes;
		}
		int num = bytes.Length;
		int num2 = (num / 2 + num % 2) * 2;
		byte[] array = new byte[num2];
		if (num == 1)
		{
			array[0] = 0;
			array[1] = bytes[0];
		}
		else if (num2 == 2)
		{
			array[0] = bytes[0];
			array[1] = bytes[1];
		}
		else if (num == 3)
		{
			array[0] = 0;
			array[1] = bytes[0];
			array[2] = bytes[1];
			array[3] = bytes[2];
		}
		else
		{
			array[0] = bytes[0];
			array[1] = bytes[1];
			array[2] = bytes[2];
			array[3] = bytes[3];
		}
		return array;
	}

	public static int ToInt16(byte[] bytes, int offset, int count)
	{
		if (bytes.Length <= offset)
		{
			return -1;
		}
		List<byte> list = new List<byte>();
		for (int i = 0; i < count; i++)
		{
			if (offset + i < bytes.Length)
			{
				list.Add(bytes[offset + i]);
			}
			else
			{
				list.Insert(0, 0);
			}
		}
		return GetInt(list.ToArray());
	}

	public static int ToInt32(byte[] bytes, int offset, int count)
	{
		if (bytes.Length - 3 <= offset)
		{
			return -1;
		}
		List<byte> list = new List<byte>();
		for (int i = 0; i < count; i++)
		{
			if (offset + i < bytes.Length)
			{
				list.Add(bytes[offset + i]);
			}
			else
			{
				list.Insert(0, 0);
			}
		}
		return GetInt(list.ToArray());
	}

	public static byte[] RemoveWhiteSpace(byte[] data)
	{
		int num = data.Length;
		int num2 = 0;
		int num3 = 0;
		while (num3 < num)
		{
			byte b = data[num3];
			if (b == 0)
			{
				num2--;
			}
			switch (b)
			{
			case 9:
			case 10:
			case 12:
			case 13:
				num2--;
				break;
			case 32:
				num2--;
				break;
			}
			if (num3 != num2)
			{
				data[num2] = data[num3];
			}
			if (num2 < num)
			{
				byte[] array = data;
				data = new byte[num2];
				for (int i = 0; i < num2; i++)
				{
					data[i] = array[i];
				}
			}
			num3++;
			num2++;
		}
		return data;
	}

	public static void GetBytes(int b, byte[] output)
	{
		byte[] array = new byte[4]
		{
			(byte)((uint)b & 0xFFu),
			(byte)((uint)(b >> 8) & 0xFFu),
			(byte)((uint)(b >> 16) & 0xFFu),
			(byte)(b >> 24)
		};
		for (int i = 0; i < output.Length; i++)
		{
			output[i] = array[i];
		}
	}
}
