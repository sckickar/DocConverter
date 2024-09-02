using System;
using System.Text;

namespace DocGen.Pdf;

internal class ContentLexer
{
	private StringBuilder m_operatorParams = new StringBuilder();

	private TokenType m_tType;

	private char m_currentChar;

	private bool m_isArtifactContentEnds;

	internal bool IsContainsArtifacts;

	private char m_nextChar;

	private byte[] m_contentStream;

	private int m_charPointer;

	private static string[] m_textShowers = new string[4] { "Tj", "'", "TJ", "\"" };

	private bool m_isContentEnded;

	internal TokenType Token => m_tType;

	internal StringBuilder OperatorParams => m_operatorParams;

	public ContentLexer(byte[] contentStream)
	{
		m_contentStream = contentStream;
	}

	public TokenType GetNextToken()
	{
		ResetToken();
		char c = MoveToNextChar();
		switch (c)
		{
		case '%':
			return m_tType = GetComment();
		case '/':
			return m_tType = GetName();
		case '+':
		case '-':
			return m_tType = GetNumber();
		case '(':
		case '[':
			return m_tType = GetLiteralString();
		case '<':
			return m_tType = GetHexadecimalString();
		case '.':
			return m_tType = GetNumber();
		case '"':
		case '\'':
			return m_tType = GetOperator();
		default:
			if (char.IsDigit(c))
			{
				return m_tType = GetNumber();
			}
			if (char.IsLetter(c))
			{
				return m_tType = GetOperator();
			}
			if (c == '\uffff')
			{
				return m_tType = TokenType.Eof;
			}
			return TokenType.None;
		}
	}

	private void ResetToken()
	{
		m_operatorParams.Length = 0;
	}

	private char MoveToNextChar()
	{
		for (; m_currentChar != '\uffff'; GetNextChar())
		{
			char currentChar = m_currentChar;
			switch (currentChar)
			{
			case '\u0001':
				if (currentChar == '\u0001')
				{
					continue;
				}
				break;
			case '\0':
			case '\b':
			case '\t':
			case '\n':
			case '\f':
			case '\r':
			case ' ':
			case '\u0085':
				continue;
			}
			return m_currentChar;
		}
		return m_currentChar;
	}

	internal void ResetContentPointer(int count)
	{
		m_charPointer -= count;
	}

	internal char GetNextChar()
	{
		if (m_contentStream.Length <= m_charPointer)
		{
			if (m_nextChar == 'Q' || (m_currentChar == 'D' && m_nextChar == 'o'))
			{
				m_currentChar = m_nextChar;
				m_nextChar = '\uffff';
				return m_currentChar;
			}
			m_currentChar = '\uffff';
			m_nextChar = '\uffff';
		}
		else
		{
			m_currentChar = m_nextChar;
			m_nextChar = (char)m_contentStream[m_charPointer++];
			if (m_currentChar == '\r')
			{
				if (m_nextChar == '\n')
				{
					m_currentChar = m_nextChar;
					if (m_contentStream.Length <= m_charPointer)
					{
						m_nextChar = '\uffff';
					}
					else
					{
						m_nextChar = (char)m_contentStream[m_charPointer++];
					}
				}
				else
				{
					m_currentChar = '\n';
				}
			}
		}
		return m_currentChar;
	}

	internal char GetNextInlineChar()
	{
		if (m_contentStream.Length <= m_charPointer)
		{
			m_currentChar = '\uffff';
			m_nextChar = '\uffff';
		}
		else
		{
			m_currentChar = m_nextChar;
			m_nextChar = (char)m_contentStream[m_charPointer++];
			if (m_currentChar == '\r')
			{
				if (m_nextChar == '\n')
				{
					m_currentChar = '\r';
				}
				else
				{
					m_currentChar = '\n';
				}
			}
		}
		return m_currentChar;
	}

	internal char GetNextCharforInlineStream()
	{
		if (m_contentStream.Length <= m_charPointer)
		{
			m_currentChar = '\uffff';
			m_nextChar = '\uffff';
		}
		else
		{
			m_currentChar = m_nextChar;
			m_nextChar = (char)m_contentStream[m_charPointer++];
			if (m_currentChar == '\r' && m_nextChar == '\n' && m_contentStream.Length <= m_charPointer)
			{
				m_currentChar = m_nextChar;
				m_nextChar = '\uffff';
			}
		}
		return m_currentChar;
	}

	internal char GetNextChar(bool value)
	{
		return m_nextChar;
	}

	private TokenType GetComment()
	{
		ResetToken();
		char c;
		while ((c = ConsumeValue()) != '\n' && c != '\uffff')
		{
		}
		return TokenType.Comment;
	}

	private TokenType GetName()
	{
		ResetToken();
		char ch;
		do
		{
			ch = ConsumeValue();
		}
		while (!IsWhiteSpace(ch) && !IsDelimiter(ch));
		return TokenType.Name;
	}

	private TokenType GetNumber()
	{
		char c = m_currentChar;
		if (c == '+' || c == '-')
		{
			m_operatorParams.Append(m_currentChar);
			c = GetNextChar();
		}
		while (true)
		{
			if (char.IsDigit(c))
			{
				m_operatorParams.Append(m_currentChar);
			}
			else
			{
				if (c != '.')
				{
					break;
				}
				m_operatorParams.Append(m_currentChar);
			}
			c = GetNextChar();
		}
		return TokenType.Integer;
	}

	private TokenType GetLiteralString()
	{
		ResetToken();
		char c = ((m_currentChar != '(') ? m_currentChar : m_currentChar);
		char c2 = ConsumeValue();
		while (true)
		{
			if (c == '(')
			{
				string literals = GetLiterals(c2);
				m_operatorParams.Append(literals);
				c2 = GetNextChar();
				break;
			}
			switch (c2)
			{
			case '(':
			{
				c2 = ConsumeValue();
				string literals = GetLiterals(c2);
				m_operatorParams.Append(literals);
				c2 = GetNextChar();
				continue;
			}
			case ']':
				break;
			default:
				c2 = ConsumeValue();
				continue;
			}
			c2 = ConsumeValue();
			break;
		}
		return TokenType.String;
	}

	private string GetLiterals(char ch)
	{
		int num = 0;
		string text = "";
		while (true)
		{
			switch (ch)
			{
			case '\\':
				text += ch;
				ch = GetNextChar();
				text += ch;
				ch = GetNextChar();
				continue;
			case '(':
				num++;
				text += ch;
				ch = GetNextChar();
				continue;
			case ')':
				if (num != 0)
				{
					text += ch;
					ch = GetNextChar();
					num--;
					continue;
				}
				break;
			}
			if (ch == ')' && num == 0)
			{
				break;
			}
			text += ch;
			ch = GetNextChar();
		}
		return text + ch;
	}

	private TokenType GetHexadecimalString()
	{
		char c = '<';
		char c2 = '>';
		char c3 = ' ';
		int num = 0;
		char c4 = ConsumeValue();
		while (true)
		{
			if (c4 == c)
			{
				num++;
				c4 = ConsumeValue();
				continue;
			}
			if (c4 == c2 && !m_isArtifactContentEnds)
			{
				switch (num)
				{
				case 0:
					ConsumeValue();
					break;
				case 1:
					c4 = ConsumeValue();
					if (c4.Equals('>'))
					{
						num--;
					}
					if (num != 1 || (c4 != c3 && (!IsContainsArtifacts || c4 != 'B')))
					{
						continue;
					}
					break;
				default:
					if (c4.Equals('>'))
					{
						num--;
					}
					c4 = ConsumeValue();
					continue;
				}
				break;
			}
			c4 = ConsumeValue();
			if (c4 == '\uffff')
			{
				break;
			}
		}
		m_isContentEnded = false;
		IsContainsArtifacts = false;
		return TokenType.HexString;
	}

	private TokenType GetOperator()
	{
		ResetToken();
		char ch = m_currentChar;
		while (IsOperator(ch))
		{
			ch = ConsumeValue();
		}
		return TokenType.Operator;
	}

	private bool IsOperator(char ch)
	{
		if (char.IsLetter(ch))
		{
			return true;
		}
		switch (ch)
		{
		case '"':
		case '\'':
		case '*':
		case '0':
		case '1':
			return true;
		default:
			return false;
		}
	}

	private char ConsumeValue()
	{
		m_operatorParams.Append(m_currentChar);
		if (IsContainsArtifacts && m_operatorParams.ToString().Contains("/Contents") && !m_isContentEnded)
		{
			m_isArtifactContentEnds = true;
			if (m_nextChar == ')' && m_currentChar != '\\')
			{
				m_isArtifactContentEnds = false;
				m_isContentEnded = true;
			}
		}
		return GetNextChar();
	}

	private bool IsWhiteSpace(char ch)
	{
		switch (ch)
		{
		case '\0':
		case '\t':
		case '\n':
		case '\f':
		case '\r':
		case ' ':
			return true;
		default:
			return false;
		}
	}

	private bool IsDelimiter(char ch)
	{
		switch (ch)
		{
		case '%':
		case '(':
		case ')':
		case '/':
		case '<':
		case '>':
		case '[':
		case ']':
			return true;
		default:
			return false;
		}
	}

	private bool CheckForTextOperator()
	{
		char ch = m_nextChar;
		int num = 0;
		if (Array.IndexOf(m_textShowers, ch.ToString()) < 0)
		{
			if (IsWhiteSpace(ch))
			{
				ch = (char)m_contentStream[m_charPointer];
				num++;
			}
			string text = ch.ToString();
			ch = (char)m_contentStream[m_charPointer + num];
			text += ch;
			if (Array.IndexOf(m_textShowers, text) < 0)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	internal void Dispose()
	{
		if (m_contentStream != null)
		{
			m_contentStream = null;
		}
	}
}
