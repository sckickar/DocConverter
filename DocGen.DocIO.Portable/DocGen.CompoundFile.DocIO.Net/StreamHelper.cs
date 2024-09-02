using System;
using System.IO;
using System.Text;

namespace DocGen.CompoundFile.DocIO.Net;

public static class StreamHelper
{
	public const int IntSize = 4;

	private const int ShortSize = 2;

	private const int DoubleSize = 8;

	public static short ReadInt16(Stream stream, byte[] buffer)
	{
		if (stream.Read(buffer, 0, 2) != 2)
		{
			throw new Exception("Invalid Data");
		}
		return BitConverter.ToInt16(buffer, 0);
	}

	public static int ReadInt32(Stream stream, byte[] buffer)
	{
		if (stream.Read(buffer, 0, 4) != 4)
		{
			throw new Exception("Invalid data");
		}
		return BitConverter.ToInt32(buffer, 0);
	}

	public static double ReadDouble(Stream stream, byte[] buffer)
	{
		if (stream.Read(buffer, 0, 8) != 8)
		{
			throw new Exception("Invalid data");
		}
		return BitConverter.ToDouble(buffer, 0);
	}

	public static int WriteInt16(Stream stream, short value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, 2);
		return 2;
	}

	public static int WriteInt32(Stream stream, int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, 4);
		return 4;
	}

	public static int WriteDouble(Stream stream, double value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, 8);
		return 8;
	}

	public static string GetAsciiString(Stream stream, int roundedSize)
	{
		byte[] buffer = new byte[4];
		int num = ReadInt32(stream, buffer);
		byte[] array = new byte[num];
		if (stream.Read(array, 0, num) != num)
		{
			throw new IOException();
		}
		for (int i = 0; i < num; i++)
		{
			if (array[i] == 0)
			{
				num = i;
				break;
			}
		}
		Encoding uTF = Encoding.UTF8;
		if (num == 0)
		{
			return string.Empty;
		}
		return RemoveLastZero(uTF.GetString(array, 0, num));
	}

	internal static string GetAsciiString(Stream stream, int roundedSize, int codePage)
	{
		byte[] buffer = new byte[4];
		int num = ReadInt32(stream, buffer);
		byte[] array = new byte[num];
		if (stream.Read(array, 0, num) != num)
		{
			throw new IOException();
		}
		roundedSize = num;
		for (int i = 0; i < num; i++)
		{
			if (array[i] == 0)
			{
				num = i;
				break;
			}
		}
		Encoding encoding = GetEncoding(GetCodePageName(codePage));
		if (num == 0)
		{
			return string.Empty;
		}
		return RemoveLastZero(encoding.GetString(array, 0, num));
	}

	private static string GetCodePageName(int codepage)
	{
		return codepage switch
		{
			1252 => "Windows-1252", 
			10000 => "macintosh", 
			10001 => "x-mac-japanese", 
			10003 => "x-mac-korean", 
			10008 => "x-mac-chinesesimp", 
			10002 => "x-mac-chinesetrad", 
			10005 => "x-mac-hebrew", 
			10004 => "x-mac-arabic", 
			10006 => "x-mac-greek", 
			10081 => "x-mac-turkish", 
			10021 => "x-mac-thai", 
			10029 => "x-mac-ce", 
			10007 => "x-mac-cyrillic", 
			932 => "shift_jis", 
			949 => "ks_c_5601-1987", 
			1361 => "Johab", 
			936 => "gb2312", 
			950 => "big5", 
			1253 => "windows-1253", 
			1254 => "windows-1254", 
			1258 => "windows-1258", 
			1255 => "windows-1255", 
			1256 => "windows-1256", 
			1257 => "windows-1257", 
			1251 => "windows-1251", 
			874 => "windows-874", 
			1250 => "windows-1250", 
			437 => "IBM437", 
			850 => "ibm850", 
			_ => "utf-8", 
		};
	}

	private static Encoding GetEncoding(string codePage)
	{
		try
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			return Encoding.GetEncoding(codePage);
		}
		catch (Exception)
		{
			return Encoding.UTF8;
		}
	}

	public static string GetUnicodeString(Stream stream, int roundedSize)
	{
		byte[] buffer = new byte[4];
		int num = ReadInt32(stream, buffer) * 2;
		byte[] array = new byte[num];
		if (stream.Read(array, 0, num) != num)
		{
			throw new IOException();
		}
		string value = RemoveLastZero(Encoding.Unicode.GetString(array, 0, num));
		int num2 = num % 4;
		if (num2 != 0)
		{
			stream.Position += 4 - num2;
		}
		return RemoveLastZero(value);
	}

	public static int WriteAsciiString(Stream stream, string value, bool align)
	{
		Encoding uTF = Encoding.UTF8;
		return WriteString(stream, value, uTF, align);
	}

	public static int WriteUnicodeString(Stream stream, string value)
	{
		return WriteString(stream, value, Encoding.Unicode, align: true);
	}

	public static int WriteString(Stream stream, string value, Encoding encoding, bool align)
	{
		if (string.IsNullOrEmpty(value))
		{
			value = "\0\0\0\0";
		}
		else if (value[value.Length - 1] != 0)
		{
			value += "\0";
		}
		if (value.Length % 4 != 0)
		{
			int num = value.Length % 4;
			for (int i = 0; i < 4 - num; i++)
			{
				value += "\0";
			}
		}
		byte[] bytes = encoding.GetBytes(value);
		int num2 = bytes.Length;
		byte[] bytes2 = BitConverter.GetBytes(value.Length);
		stream.Write(bytes2, 0, bytes2.Length);
		stream.Write(bytes, 0, num2);
		int num3 = 4 + num2;
		if (align)
		{
			int num4 = num3 % 4;
			if (num4 != 0)
			{
				int j = 0;
				for (int num5 = 4 - num4; j < num5; j++)
				{
					stream.WriteByte(0);
				}
				num3 += 4 - num4;
			}
		}
		return num3;
	}

	public static void AddPadding(Stream stream, ref int iWrittenSize)
	{
		int num = iWrittenSize % 4;
		if (num != 0)
		{
			int num2 = 0;
			int num3 = 4 - num;
			while (num2 < num3)
			{
				stream.WriteByte(0);
				num2++;
				iWrittenSize++;
			}
		}
	}

	private static string RemoveLastZero(string value)
	{
		int num = value?.Length ?? 0;
		while (num > 0 && value[num - 1] == '\0')
		{
			value = value.Substring(0, num - 1);
			num--;
		}
		return value;
	}
}
