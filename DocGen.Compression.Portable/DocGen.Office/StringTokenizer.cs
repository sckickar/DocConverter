using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DocGen.Office;

internal class StringTokenizer
{
	public const char WhiteSpace = ' ';

	public const char Tab = '\t';

	public static readonly char[] Spaces = new char[2] { ' ', '\t' };

	private const RegexOptions c_regexOptions = RegexOptions.IgnoreCase;

	private const string c_whiteSpacePatterm = "^[ \\t]+$";

	private static Regex s_whiteSpaceRegex = new Regex("^[ \\t]+$", RegexOptions.IgnoreCase);

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

	public StringTokenizer(string text)
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
		int i;
		for (i = m_position; i < Length; i++)
		{
			char c = m_text[i];
			if (c == '\n' || c == '\r')
			{
				string result = m_text.Substring(m_position, i - m_position);
				m_position = i + 1;
				if (c == '\r' && m_position < Length && m_text[m_position] == '\n')
				{
					m_position++;
				}
				return result;
			}
		}
		if (i > m_position)
		{
			string result2 = m_text.Substring(m_position, i - m_position);
			m_position = i;
			return result2;
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

	public string ReadWord()
	{
		int i;
		for (i = m_position; i < Length; i++)
		{
			char c = m_text[i];
			switch (c)
			{
			case '\n':
			case '\r':
			{
				string result2 = m_text.Substring(m_position, i - m_position);
				m_position = i + 1;
				if (c == '\r' && m_position < Length && m_text[m_position] == '\n')
				{
					m_position++;
				}
				return result2;
			}
			case '\t':
			case ' ':
			{
				if (i == m_position)
				{
					i++;
				}
				string result = m_text.Substring(m_position, i - m_position);
				m_position = i;
				return result;
			}
			}
		}
		if (i > m_position)
		{
			string result3 = m_text.Substring(m_position, i - m_position);
			m_position = i;
			return result3;
		}
		return null;
	}

	public string PeekWord()
	{
		int position = m_position;
		string result = ReadWord();
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
