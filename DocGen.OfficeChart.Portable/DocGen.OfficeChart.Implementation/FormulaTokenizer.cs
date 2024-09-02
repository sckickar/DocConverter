using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation;

internal class FormulaTokenizer
{
	private const char FormulaEnd = '\u0001';

	private const char Whitespace = ' ';

	private const char Colon = ':';

	private char m_chCurrent;

	private int m_iFormulaLength;

	private int m_iPos;

	private string m_strFormula;

	private int m_iStartPos;

	public FormulaToken TokenType;

	public FormulaToken PreviousTokenType;

	private int m_iPrevPos;

	private StringBuilder m_value = new StringBuilder();

	private WorkbookImpl m_book;

	private char m_chArgumentSeparator = ',';

	private NumberFormatInfo m_numberFormat = NumberFormatInfo.CurrentInfo;

	private int m_lastIndexQuote = -1;

	public string TokenString => m_value.ToString();

	public char ArgumentSeparator
	{
		get
		{
			return m_chArgumentSeparator;
		}
		set
		{
			m_chArgumentSeparator = value;
		}
	}

	public NumberFormatInfo NumberFormat
	{
		get
		{
			return m_numberFormat;
		}
		set
		{
			m_numberFormat = value;
		}
	}

	public FormulaTokenizer(WorkbookImpl book)
	{
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		m_book = book;
	}

	public void Prepare(string formula)
	{
		m_strFormula = formula;
		m_iFormulaLength = formula.Length;
		m_iPos = 0;
		NextChar();
	}

	private void NextChar()
	{
		if (m_iPos < m_iFormulaLength)
		{
			m_chCurrent = m_strFormula[m_iPos];
			m_iPos++;
		}
		else
		{
			m_chCurrent = '\u0001';
		}
	}

	private void MoveBack(char charToMoveTo)
	{
		int startIndex = Math.Min(m_strFormula.Length - 1, m_iPos);
		int num = m_strFormula.LastIndexOf(charToMoveTo, startIndex);
		if (num >= 0)
		{
			int num2 = m_iPos - num;
			m_iPos = num + 1;
			m_value.Remove(m_value.Length - num2 + 1, num2 - 1);
			m_chCurrent = charToMoveTo;
		}
	}

	public void NextToken()
	{
		PreviousTokenType = TokenType;
		m_iPrevPos = m_iStartPos;
		m_value.Length = 0;
		if (TokenType != FormulaToken.DDELink)
		{
			TokenType = FormulaToken.None;
		}
		char c = m_numberFormat.NumberDecimalSeparator[0];
		char c2 = ' ';
		if (m_strFormula.IndexOf(m_chCurrent) > 0)
		{
			_ = m_strFormula[m_strFormula.IndexOf(m_chCurrent) - 1];
		}
		if (m_strFormula.IndexOf(m_chCurrent) < m_strFormula.Length - 1)
		{
			c2 = m_strFormula[m_strFormula.IndexOf(m_chCurrent) + 1];
		}
		while (true)
		{
			m_iStartPos = m_iPos;
			if (char.IsDigit(m_chCurrent))
			{
				ParseNumber();
			}
			else if ((m_chCurrent == '.' || m_chCurrent == c) && char.IsDigit(c2) && PreviousTokenType != FormulaToken.CloseParenthesis)
			{
				ParseNumber();
			}
			else
			{
				switch (m_chCurrent)
				{
				case '\u0001':
					TokenType = FormulaToken.EndOfFormula;
					break;
				case '-':
					NextChar();
					TokenType = FormulaToken.tSub;
					break;
				case '+':
					NextChar();
					TokenType = FormulaToken.tAdd;
					break;
				case '*':
					NextChar();
					TokenType = FormulaToken.tMul;
					break;
				case '/':
					NextChar();
					TokenType = FormulaToken.tDiv;
					break;
				case '%':
					NextChar();
					TokenType = FormulaToken.tPercent;
					break;
				case '(':
					NextChar();
					TokenType = FormulaToken.tParentheses;
					break;
				case ')':
					NextChar();
					TokenType = FormulaToken.CloseParenthesis;
					break;
				case '<':
					ProcessLess();
					break;
				case '>':
					ProcessGreater();
					break;
				case '=':
					NextChar();
					TokenType = FormulaToken.tEqual;
					break;
				case '\'':
					m_lastIndexQuote = -1;
					if (m_strFormula.Contains("'"))
					{
						int num = CharOccurs(m_strFormula, '\'');
						if (num % 2 != 0 || (num / CharOccurs(m_strFormula, '!', '\'') <= 2 && (CharOccurs(m_strFormula, '(') + CharOccurs(m_strFormula, ')')) % 2 != 0))
						{
							m_lastIndexQuote = m_strFormula.IndexOf('!') - 1;
						}
					}
					ParseString(InQuote: true);
					if (PreviousTokenType == FormulaToken.DDELink)
					{
						NextChar();
						break;
					}
					TokenType = FormulaToken.Identifier;
					ParseIdentifier();
					break;
				case '"':
					m_lastIndexQuote = -1;
					ParseString(InQuote: true);
					TokenType = FormulaToken.tStringConstant;
					break;
				case '#':
					ParseError();
					break;
				case '&':
					NextChar();
					TokenType = FormulaToken.tConcat;
					break;
				case ':':
					NextChar();
					TokenType = FormulaToken.tCellRange;
					break;
				case '{':
					ParseArray();
					TokenType = FormulaToken.tArray1;
					break;
				case '^':
					TokenType = FormulaToken.tPower;
					NextChar();
					break;
				default:
					if (m_chCurrent == m_chArgumentSeparator)
					{
						NextChar();
						TokenType = FormulaToken.Comma;
					}
					else if (m_chCurrent > ' ')
					{
						ParseIdentifier();
					}
					else if (m_chCurrent == ' ')
					{
						ParseSpace();
					}
					break;
				}
			}
			if (TokenType != 0)
			{
				break;
			}
			NextChar();
		}
	}

	private int CharOccurs(string stringToSearch, char charToFind)
	{
		int num = 0;
		char[] array = stringToSearch.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == charToFind)
			{
				num++;
			}
		}
		return num;
	}

	private int CharOccurs(string stringToSearch, char charToFind, char previousChar)
	{
		int num = 0;
		int num2 = 0;
		char[] array = stringToSearch.ToCharArray();
		char[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i] == charToFind && array[num2 - 1] == previousChar)
			{
				num++;
			}
			num2++;
		}
		return num;
	}

	public void SaveState()
	{
		m_iPrevPos = m_iStartPos;
		PreviousTokenType = TokenType;
	}

	public void RestoreState()
	{
		m_iStartPos = m_iPrevPos;
		TokenType = PreviousTokenType;
		m_chCurrent = m_strFormula[m_iStartPos];
		m_value.Length = 0;
	}

	private void ParseNumber()
	{
		TokenType = FormulaToken.tInteger;
		AppendNumbers();
		if (m_chCurrent == m_numberFormat.NumberDecimalSeparator[0] && char.IsDigit(m_strFormula[m_iPos]))
		{
			TokenType = FormulaToken.tNumber;
			m_value.Append(m_chCurrent);
			NextChar();
			AppendNumbers();
		}
		if (char.ToUpper(m_chCurrent) == 'E')
		{
			TokenType = FormulaToken.tNumber;
			m_value.Append(m_chCurrent);
			NextChar();
			if (m_chCurrent == '-' || m_chCurrent == '+')
			{
				m_value.Append(m_chCurrent);
				NextChar();
			}
			AppendNumbers();
		}
		else if (m_chCurrent == ':')
		{
			TokenType = FormulaToken.Identifier;
			m_value.Append(m_chCurrent);
			NextChar();
			AppendNumbers();
		}
	}

	private void ProcessGreater()
	{
		NextChar();
		if (m_chCurrent == '=')
		{
			NextChar();
			TokenType = FormulaToken.tGreaterEqual;
		}
		else
		{
			TokenType = FormulaToken.tGreater;
		}
	}

	private void ProcessLess()
	{
		NextChar();
		if (m_chCurrent == '=')
		{
			NextChar();
			TokenType = FormulaToken.tLessEqual;
		}
		else if (m_chCurrent == '>')
		{
			NextChar();
			TokenType = FormulaToken.tNotEqual;
		}
		else
		{
			TokenType = FormulaToken.tLessThan;
		}
	}

	private void AppendNumbers()
	{
		while (char.IsDigit(m_chCurrent))
		{
			m_value.Append(m_chCurrent);
			NextChar();
		}
	}

	private void ParseIdentifier()
	{
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		bool flag3 = false;
		_ = m_chCurrent;
		int iPos = 0;
		while (char.IsLetterOrDigit(m_chCurrent) || m_chCurrent >= '\u0080' || m_chCurrent == '_' || m_chCurrent == '!' || m_chCurrent == ':' || m_chCurrent == '.' || m_chCurrent == '$' || m_chCurrent == '[' || m_chCurrent == '\'' || m_chCurrent == '#' || m_chCurrent == ']' || m_chCurrent == '?' || (m_chCurrent == ' ' && flag3))
		{
			if (m_chCurrent == '!')
			{
				flag = true;
				num++;
				if (TokenType == FormulaToken.DDELink)
				{
					NextChar();
					break;
				}
				if (num > 1 && flag2)
				{
					MoveBack(':');
					break;
				}
			}
			else if (m_chCurrent == ':')
			{
				flag2 = true;
				iPos = m_iPos;
				flag3 = true;
			}
			if (m_chCurrent == ' ' && flag3)
			{
				NextChar();
				continue;
			}
			m_value.Append(m_chCurrent);
			if (m_chCurrent != ':')
			{
				flag3 = false;
			}
			char c = '\u0001';
			if (m_chCurrent == '[')
			{
				c = ']';
			}
			else if (m_chCurrent == '\'')
			{
				c = m_chCurrent;
			}
			if (c != '\u0001')
			{
				int num2 = 1;
				while (m_chCurrent != '\u0001' && num2 > 0)
				{
					NextChar();
					if (m_chCurrent != '\u0001')
					{
						m_value.Append(m_chCurrent);
					}
					if (m_chCurrent == c)
					{
						num2--;
					}
					if (m_chCurrent == '[')
					{
						num2++;
					}
				}
			}
			NextChar();
		}
		string text = m_value.ToString();
		if (text.Contains(":") && !text.EndsWith(":") && !text.StartsWith(":"))
		{
			string[] array = text.Split(':');
			string obj = array[0];
			string text2 = array[1];
			char c2 = obj[obj.Length - 1];
			char c3 = text2[text2.Length - 1];
			if (!char.IsNumber(c3) && char.IsLetter(c3) && char.IsNumber(c2))
			{
				m_iPos = iPos;
				m_chCurrent = ':';
				m_value.Remove(0, m_value.Length);
				m_value.Append(array[0]);
			}
		}
		if (TokenType != FormulaToken.DDELink)
		{
			if (m_chCurrent == '(')
			{
				TokenType = FormulaToken.tFunction1;
			}
			else if (string.Compare(text, "true", StringComparison.CurrentCultureIgnoreCase) == 0)
			{
				TokenType = FormulaToken.ValueTrue;
			}
			else if (string.Compare(text, "false", StringComparison.CurrentCultureIgnoreCase) == 0)
			{
				TokenType = FormulaToken.ValueFalse;
			}
			else if (m_chCurrent == '|')
			{
				NextChar();
				TokenType = FormulaToken.DDELink;
			}
			else if (m_value.ToString() == "Overview!#REF!")
			{
				TokenType = FormulaToken.Identifier3D;
			}
			else if (m_value.ToString().EndsWith("!#REF!"))
			{
				TokenType = FormulaToken.tError;
			}
			else
			{
				TokenType = (flag ? FormulaToken.Identifier3D : FormulaToken.Identifier);
			}
		}
	}

	private void ParseSpace()
	{
		while (m_chCurrent == ' ')
		{
			m_value.Append(m_chCurrent);
			NextChar();
		}
		TokenType = FormulaToken.Space;
	}

	private void ParseString(bool InQuote)
	{
		char c = '\0';
		if (InQuote)
		{
			c = m_chCurrent;
			NextChar();
		}
		while (m_chCurrent != '\u0001')
		{
			if (InQuote && m_chCurrent == c && CheckQuote())
			{
				NextChar();
				if (m_chCurrent != c)
				{
					return;
				}
				m_value.Append(m_chCurrent);
				NextChar();
			}
			else
			{
				m_value.Append(m_chCurrent);
				NextChar();
			}
		}
		if (InQuote)
		{
			RaiseException("Incomplete string, missing " + c + "; String started", null);
		}
	}

	private bool CheckQuote()
	{
		if (m_lastIndexQuote != -1)
		{
			return m_iPos == m_lastIndexQuote + 1;
		}
		return true;
	}

	private void ParseError()
	{
		Dictionary<string, int>.KeyCollection keys = FormulaUtil.ErrorNameToCode.Keys;
		string text = null;
		foreach (string item in (IEnumerable<string>)keys)
		{
			int length = item.Length;
			if (string.Compare(item, 0, m_strFormula, m_iPos - 1, length, StringComparison.CurrentCultureIgnoreCase) == 0)
			{
				text = item;
				break;
			}
		}
		m_value.Length = 0;
		m_value.Append(text);
		m_iPos += text.Length - 1;
		TokenType = FormulaToken.tError;
		NextChar();
	}

	private void ParseArray()
	{
		m_value.Length = 0;
		while (m_chCurrent != '}' && m_chCurrent != '\u0001')
		{
			m_value.Append(m_chCurrent);
			if (m_chCurrent == '"')
			{
				SkipString();
			}
			else
			{
				NextChar();
			}
		}
		m_value.Append(m_chCurrent);
		if (m_chCurrent == '\u0001')
		{
			RaiseException("Couldn't find end of array", null);
		}
		NextChar();
	}

	private void SkipString()
	{
		char chCurrent = m_chCurrent;
		NextChar();
		while (m_chCurrent != '\u0001')
		{
			m_value.Append(m_chCurrent);
			if (m_chCurrent == chCurrent)
			{
				NextChar();
				if (m_chCurrent != chCurrent)
				{
					break;
				}
				m_value.Append(m_chCurrent);
			}
			NextChar();
		}
		if (m_chCurrent == '\u0001')
		{
			RaiseException("Can't find end of the string", null);
		}
	}

	public void RaiseException(string msg, Exception ex)
	{
		if (ex is ParseException)
		{
			msg = msg + ". " + ex.Message;
		}
		else
		{
			msg = msg + "  at position " + m_iStartPos;
			if (ex != null)
			{
				msg = msg + ". " + ex.Message;
			}
		}
		throw new ParseException(msg, m_strFormula, m_iPos, ex);
	}

	public void RaiseUnexpectedToken(string msg)
	{
		if (msg == null || msg.Length == 0)
		{
			msg = string.Empty;
		}
		string msg2 = $"{msg}Unexpected token type: {TokenType}, string value: {m_value}";
		RaiseException(msg2, null);
	}
}
