using System;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.DocIO;
using DocGen.DocIO.DLS;

namespace DocGen.Layouting;

internal class StringParser
{
	public const char WhiteSpace = ' ';

	public const char Tab = '\t';

	public const char Hyphen = '-';

	public static readonly char[] Spaces = new char[3] { ' ', '\t', '-' };

	private const RegexOptions c_regexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;

	private const string c_whiteSpacePatterm = "^[ \\t-]+$";

	private static Regex s_whiteSpaceRegex = new Regex("^[ \\t-]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private string m_text;

	private int m_position;

	public bool EOF => m_position == m_text.Length;

	public int Length => m_text.Length;

	public int Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	public StringParser(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		m_text = text;
	}

	public static int GetCharsCount(string text, char symbol)
	{
		if (text == null)
		{
			throw new ArgumentNullException("wholeText");
		}
		int num = 0;
		int num2 = 0;
		do
		{
			num2 = text.IndexOf(symbol, num2);
			if (num2 == -1)
			{
				break;
			}
			num++;
			num2++;
		}
		while (num2 != text.Length);
		return num;
	}

	public static int GetCharsCount(string text, char[] symbols)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (symbols == null)
		{
			throw new ArgumentNullException("symbols");
		}
		int num = 0;
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			char symbol = text[i];
			if (Contains(symbols, symbol))
			{
				num++;
			}
		}
		return num;
	}

	public string ReadLine()
	{
		int num = m_position;
		while (num < Length)
		{
			char c = m_text[num];
			if (c == '\n' || c == '\r')
			{
				if (c == '\r' && num + 1 < m_text.Length && m_text[num + 1] == '\n' && (num + 2 >= m_text.Length || m_text[num + 2] != '\r'))
				{
					if (num <= 0 || m_text[num - 1] != '\n')
					{
						num++;
						continue;
					}
				}
				else if (c == '\n' && num + 1 < m_text.Length && m_text[num + 1] == '\r' && (num + 2 >= m_text.Length || m_text[num + 2] != '\n') && (num <= 0 || m_text[num - 1] != '\r'))
				{
					num++;
					continue;
				}
				string text = m_text.Substring(m_position, num - m_position);
				m_position = num + 1;
				if (c == '\r' && m_position < Length && m_text[m_position] == ControlChar.LineFeedChar)
				{
					m_position++;
				}
				if (text == "")
				{
					text = " ";
				}
				return text;
			}
			num++;
		}
		if (num > m_position)
		{
			string result = m_text.Substring(m_position, num - m_position);
			m_position = num;
			return result;
		}
		if (num > 0 && num == m_position && num == Length && (m_text[num - 1] == ControlChar.LineFeedChar || m_text[num - 1] == '\r'))
		{
			return " ";
		}
		return null;
	}

	public string PeekLine()
	{
		int position = m_position;
		string result = ReadLine();
		m_position = position;
		return result;
	}

	public string ReadWord(IEntity strWidget)
	{
		int num = m_position;
		while (num < Length)
		{
			char c = m_text[num];
			switch (c)
			{
			case '\n':
			case '\r':
			{
				string result3 = m_text.Substring(m_position, num - m_position);
				m_position = num + 1;
				if (c == '\r' && m_position < Length && m_text[m_position] == ControlChar.LineFeedChar)
				{
					m_position++;
				}
				return result3;
			}
			case '\t':
			case ' ':
			case '-':
			{
				WParagraph ownerParagraph = (strWidget as ParagraphItem).OwnerParagraph;
				if (c == ControlChar.SpaceChar && ownerParagraph != null && ((strWidget as ParagraphItem).NextSibling != null || num < m_text.TrimEnd(ControlChar.SpaceChar).Length) && ownerParagraph.IsTextContainsNonBreakingSpaceCharacter(m_text) && ownerParagraph.IsNonBreakingCharacterCombinedWithSpace(m_text, num))
				{
					num++;
					break;
				}
				if (num == m_position || c == '-')
				{
					num++;
				}
				string result2 = m_text.Substring(m_position, num - m_position);
				m_position = num;
				return result2;
			}
			default:
				num++;
				if (IsUnicodeChineseText(c.ToString()) && num > m_position)
				{
					string result = m_text.Substring(m_position, num - m_position);
					m_position = num;
					return result;
				}
				break;
			}
		}
		if (num > m_position)
		{
			string result4 = m_text.Substring(m_position, num - m_position);
			m_position = num;
			return result4;
		}
		return null;
	}

	public string PeekWord(IEntity strWidget)
	{
		int position = m_position;
		string result = ReadWord(strWidget);
		m_position = position;
		return result;
	}

	public char Read()
	{
		char result = '\0';
		if (!EOF)
		{
			result = m_text[m_position];
			m_position++;
		}
		return result;
	}

	public string Read(int count)
	{
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		while (!EOF && num < count)
		{
			char value = Read();
			stringBuilder.Append(value);
			num++;
		}
		return stringBuilder.ToString();
	}

	public string ReadToSymbol(char symbol, bool readSymbol)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (!EOF)
		{
			char c = Peek();
			if (c == symbol)
			{
				if (readSymbol)
				{
					Read();
					stringBuilder.Append(c);
				}
				break;
			}
			stringBuilder.Append(c);
			Read();
		}
		return stringBuilder.ToString();
	}

	public char Peek()
	{
		char result = '\0';
		if (!EOF)
		{
			result = m_text[m_position];
		}
		return result;
	}

	public void Close()
	{
		m_text = null;
	}

	public string ReadToEnd()
	{
		string result = ((m_position != 0) ? m_text.Substring(m_position, Length - m_position) : m_text);
		m_position = Length;
		return result;
	}

	internal static bool IsWhitespace(string token)
	{
		if (token == null)
		{
			return false;
		}
		try
		{
			return s_whiteSpaceRegex.Match(token).Success;
		}
		catch
		{
			return false;
		}
	}

	internal static bool IsSpace(char token)
	{
		return token == ' ';
	}

	internal static bool IsTab(char token)
	{
		return token == '\t';
	}

	internal static int GetWhitespaceCount(string line, bool start)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		int num = 0;
		if (line.Length > 0)
		{
			int num2 = ((!start) ? (line.Length - 1) : 0);
			while (num2 >= 0 && num2 < line.Length)
			{
				char token = line[num2];
				if (!IsSpace(token) && !IsTab(token))
				{
					break;
				}
				num++;
				num2 = (start ? (num2 + 1) : (num2 - 1));
			}
		}
		return num;
	}

	internal static bool IsUnicodeChineseText(string text)
	{
		bool result = false;
		if (text != null)
		{
			char[] array = text.ToCharArray();
			foreach (char c in array)
			{
				if ((c >= '\u3000' && c <= 'ヿ') || (c >= '\uff00' && c <= '\uffef') || (c >= '一' && c <= '龯') || (c >= '㐀' && c <= '䶿'))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private static bool Contains(char[] array, char symbol)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		bool result = false;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == symbol)
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
