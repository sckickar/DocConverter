namespace DocGen.PdfViewer.Base;

internal static class Chars
{
	public const char NotDef = '\0';

	public const char Null = '\0';

	public const byte SingleByteSpace = 32;

	public const char Ap = '\'';

	public const char Qu = '"';

	public const char Space = ' ';

	public const char Tab = '\t';

	public const char LF = '\n';

	public const char FF = '\f';

	public const char CR = '\r';

	public const char DecimalPoint = '.';

	public const char LiteralStringStart = '(';

	public const char LiteralStringEnd = ')';

	public const char HexadecimalStringStart = '<';

	public const char HexadecimalStringEnd = '>';

	public const char Comment = '%';

	public const char Name = '/';

	public const char ArrayStart = '[';

	public const char ArrayEnd = ']';

	public const char ExecuteableArrayStart = '{';

	public const char ExecuteableArrayEnd = '}';

	public const char StringEscapeCharacter = '\\';

	public const char Minus = '-';

	public const char Plus = '+';

	public const char VerticalBar = '|';

	public static readonly char[] FontFamilyDelimiters = new char[2] { ',', '-' };

	public static bool IsWhiteSpace(int b)
	{
		return char.IsWhiteSpace((char)b);
	}

	public static bool IsOctalChar(int b)
	{
		char c = (char)b;
		if ('0' <= c)
		{
			return c <= '7';
		}
		return false;
	}

	public static bool IsHexChar(int b)
	{
		char c = (char)b;
		if (('0' > c || c > '9') && ('A' > c || c > 'F'))
		{
			if ('a' <= c)
			{
				return c <= 'f';
			}
			return false;
		}
		return true;
	}

	public static bool IsDelimiter(int b)
	{
		char c = (char)b;
		if (char.IsWhiteSpace(c))
		{
			return true;
		}
		char c2 = c;
		if (c2 <= '/')
		{
			switch (c2)
			{
			case '&':
			case '\'':
				return false;
			default:
				return false;
			case '\0':
			case '%':
			case '(':
			case ')':
			case '/':
				break;
			}
		}
		else
		{
			switch (c2)
			{
			case '=':
				return false;
			case '\\':
				return false;
			case '|':
				return false;
			default:
				return false;
			case '<':
			case '>':
			case '[':
			case ']':
			case '{':
			case '}':
				break;
			}
		}
		return true;
	}

	public static bool IsLetter(int b)
	{
		return char.IsLetter((char)b);
	}

	public static bool IsValidNumberChar(IPostScriptParser reader)
	{
		char c = (char)reader.Peek(0);
		if (c == '+' || c == '-')
		{
			return IsDigitOrDecimalPoint((char)reader.Peek(1));
		}
		return IsDigitOrDecimalPoint(c);
	}

	public static bool IsValidHexCharacter(int ch)
	{
		if (80 <= ch)
		{
			return ch <= 128;
		}
		return false;
	}

	private static bool IsDigitOrDecimalPoint(char ch)
	{
		if (!char.IsDigit(ch))
		{
			return ch == '.';
		}
		return true;
	}
}
