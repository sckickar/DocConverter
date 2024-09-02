using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

namespace DocGen.Compression;

internal class XmlConvertExtension
{
	internal static Regex NumberRegex = new Regex("[\\d]+", RegexOptions.IgnorePatternWhitespace);

	internal static readonly char[] WhitespaceChars = new char[4] { ' ', '\t', '\n', '\r' };

	internal static byte ToByte(string s)
	{
		if (byte.TryParse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo, out var result))
		{
			return result;
		}
		return (byte)GetTruncatedValue(s, 255.0);
	}

	internal static short ToInt16(string s)
	{
		if (short.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result))
		{
			return result;
		}
		return (short)GetTruncatedValue(s, 32767.0);
	}

	internal static int ToInt32(string s)
	{
		if (int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result))
		{
			return result;
		}
		return (int)GetTruncatedValue(s, 2147483647.0);
	}

	internal static long ToInt64(string s)
	{
		if (long.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result))
		{
			return result;
		}
		return (long)GetTruncatedValue(s, 9.223372036854776E+18);
	}

	[CLSCompliant(false)]
	internal static ushort ToUInt16(string s)
	{
		if (ushort.TryParse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo, out var result))
		{
			return result;
		}
		return (ushort)GetTruncatedValue(s, 65535.0);
	}

	[CLSCompliant(false)]
	internal static uint ToUInt32(string s)
	{
		if (uint.TryParse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo, out var result))
		{
			return result;
		}
		return (uint)GetTruncatedValue(s, 4294967295.0);
	}

	internal static string TrimString(string value)
	{
		return value.Trim(WhitespaceChars);
	}

	internal static float ToSingle(string s)
	{
		s = TrimString(s);
		if (s == "-INF")
		{
			return float.NegativeInfinity;
		}
		if (s == "INF")
		{
			return float.PositiveInfinity;
		}
		if (!float.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out var result))
		{
			result = (float)GetTruncatedValue(s, 3.4028234663852886E+38);
		}
		if (result == 0f && s[0] == '-')
		{
			return -0f;
		}
		return result;
	}

	internal static double ToDouble(string s)
	{
		s = TrimString(s);
		if (s == "-INF")
		{
			return double.NegativeInfinity;
		}
		if (s == "INF")
		{
			return double.PositiveInfinity;
		}
		if (!double.TryParse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var result))
		{
			result = GetTruncatedValue(s, double.MaxValue);
		}
		if (result == 0.0 && s[0] == '-')
		{
			return -0.0;
		}
		return result;
	}

	internal static bool ToBoolean(string s)
	{
		s = TrimString(s);
		switch (s)
		{
		case "1":
		case "true":
			return true;
		case "0":
		case "false":
			return false;
		default:
			return true;
		}
	}

	internal static double GetTruncatedValue(string input, double maxValue)
	{
		Match match = NumberRegex.Match(input);
		double result = 0.0;
		if (match.Success && double.TryParse(match.Value, out result) && result > maxValue)
		{
			result %= maxValue;
		}
		return result;
	}

	internal static DateTimeOffset ToDateTimeOffset(string value)
	{
		if (string.IsNullOrEmpty(value) || IsWhiteSpace(value))
		{
			return new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
		}
		return XmlConvert.ToDateTimeOffset(value);
	}

	private static bool IsWhiteSpace(string value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			if (!char.IsWhiteSpace(value[i]))
			{
				return false;
			}
		}
		return true;
	}
}
