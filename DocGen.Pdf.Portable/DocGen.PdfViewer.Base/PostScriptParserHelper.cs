using System.Collections.Generic;
using System.Text;

namespace DocGen.PdfViewer.Base;

internal static class PostScriptParserHelper
{
	private static int[] charValues;

	static PostScriptParserHelper()
	{
		charValues = new int[128];
		for (int i = 0; i < 10; i++)
		{
			charValues[48 + i] = i;
		}
		for (int j = 0; j < 6; j++)
		{
			charValues[97 + j] = 10 + j;
			charValues[65 + j] = 10 + j;
		}
	}

	public static void GoToNextLine(IPostScriptParser reader)
	{
		while (!reader.EndOfFile && reader.Read() != 10)
		{
		}
	}

	public static void SkipUnusedCharacters(IPostScriptParser reader)
	{
		SkipWhiteSpaces(reader);
		while (!reader.EndOfFile && reader.Peek(0) == 37)
		{
			GoToNextLine(reader);
			SkipWhiteSpaces(reader);
		}
	}

	public static void SkipWhiteSpaces(IPostScriptParser reader)
	{
		while (!reader.EndOfFile && Chars.IsWhiteSpace(reader.Peek(0)))
		{
			reader.Read();
		}
	}

	public static string GetString(byte[] bytes)
	{
		return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
	}

	public static string ReadNumber(IPostScriptParser reader)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (!reader.EndOfFile && Chars.IsValidNumberChar(reader))
		{
			stringBuilder.Append((char)reader.Read());
		}
		return stringBuilder.ToString();
	}

	public static string ReadName(IPostScriptParser reader)
	{
		StringBuilder stringBuilder = new StringBuilder();
		reader.Read();
		while (!reader.EndOfFile && !Chars.IsDelimiter(reader.Peek(0)))
		{
			char c = (char)reader.Read();
			if (c == '#')
			{
				if (TryReadHexChar(reader, out string result))
				{
					stringBuilder.Append(result);
					continue;
				}
				stringBuilder.Append(c);
				stringBuilder.Append(result);
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static string ReadKeyword(IPostScriptParser reader)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (!reader.EndOfFile && !Chars.IsDelimiter(reader.Peek(0)) && !char.IsNumber((char)reader.Peek(0)))
		{
			stringBuilder.Append((char)reader.Read());
		}
		return stringBuilder.ToString();
	}

	public static byte[] ReadHexadecimalString(IPostScriptParser reader)
	{
		StringBuilder stringBuilder = new StringBuilder();
		reader.Read();
		while (!reader.EndOfFile && reader.Peek(0) != 62)
		{
			if (Chars.IsHexChar(reader.Peek(0)))
			{
				stringBuilder.Append((char)reader.Read());
			}
			else
			{
				reader.Read();
			}
		}
		if (!reader.EndOfFile)
		{
			reader.Read();
		}
		if (stringBuilder.Length % 2 == 1)
		{
			stringBuilder.Append("0");
		}
		return GetBytesFromHexString(stringBuilder.ToString());
	}

	public static byte[] ReadLiteralString(IPostScriptParser reader)
	{
		List<byte> list = new List<byte>();
		reader.Read();
		int num = 1;
		while (num > 0)
		{
			if (reader.Peek(0) == 92)
			{
				if (IsValidEscape(reader.Peek(1)))
				{
					reader.Read();
					if (reader.Peek(0) != 13 && reader.Peek(0) != 10)
					{
						list.Add((byte)GetSymbolFromEscapeSymbol(reader.Read()));
					}
					else
					{
						SkipWhiteSpaces(reader);
					}
				}
				else if (Chars.IsOctalChar(reader.Peek(1)))
				{
					int num2 = 3;
					reader.Read();
					List<byte> list2 = new List<byte>();
					while (Chars.IsOctalChar(reader.Peek(0)) && num2 > 0)
					{
						list2.Add((byte)(reader.Read() - 48));
						num2--;
					}
					while (list2.Count < 3)
					{
						list2.Insert(0, 0);
					}
					int num3 = 0;
					int num4 = 1;
					for (int num5 = 2; num5 >= 0; num5--)
					{
						num3 += list2[num5] * num4;
						num4 *= 8;
					}
					list.Add((byte)num3);
				}
				else
				{
					reader.Read();
				}
				continue;
			}
			if (reader.Peek(0) == 40)
			{
				num++;
			}
			else if (reader.Peek(0) == 41)
			{
				num--;
				if (num == 0)
				{
					break;
				}
			}
			list.Add(reader.Read());
		}
		reader.Read();
		return list.ToArray();
	}

	public static byte[] GetBytesFromHexString(string hexString)
	{
		if (string.IsNullOrEmpty(hexString))
		{
			return new byte[0];
		}
		if (hexString.Length % 2 != 0)
		{
			hexString = "0" + hexString;
		}
		byte[] array = new byte[hexString.Length >> 1];
		int i = 0;
		for (int length = hexString.Length; i < length; i += 2)
		{
			array[i >> 1] = (byte)((charValues[(uint)hexString[i]] << 4) | charValues[(uint)hexString[i + 1]]);
		}
		return array;
	}

	public static bool IsValidEscape(int b)
	{
		if (b <= 92)
		{
			if (b <= 13)
			{
				if (b != 10 && b != 13)
				{
					return false;
				}
			}
			else if ((uint)(b - 40) > 1u && b != 92)
			{
				return false;
			}
		}
		else if (b <= 102)
		{
			if (b != 98 && b != 102)
			{
				return false;
			}
		}
		else
		{
			switch (b)
			{
			case 115:
				return false;
			default:
				return false;
			case 110:
			case 114:
			case 116:
				break;
			}
		}
		return true;
	}

	public static char GetSymbolFromEscapeSymbol(int symbol)
	{
		return symbol switch
		{
			40 => '(', 
			41 => ')', 
			92 => '\\', 
			98 => '\b', 
			102 => '\f', 
			110 => '\n', 
			114 => '\r', 
			116 => '\t', 
			_ => '\0', 
		};
	}

	private static bool TryReadHexChar(IPostScriptParser reader, out string result)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < 2; i++)
		{
			char c = (char)reader.Read();
			if (reader.EndOfFile || Chars.IsDelimiter(c) || !Chars.IsHexChar(c))
			{
				result = stringBuilder.ToString();
				return false;
			}
			stringBuilder.Append(c);
		}
		char c2 = (char)GetBytesFromHexString(stringBuilder.ToString())[0];
		result = c2.ToString();
		return true;
	}
}
