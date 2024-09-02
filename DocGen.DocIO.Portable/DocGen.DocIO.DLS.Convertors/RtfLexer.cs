using System;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS.Convertors;

public class RtfLexer
{
	private const char c_groupStart = '{';

	private const char c_groupEnd = '}';

	private const char c_controlStart = '\\';

	private const char c_space = ' ';

	private const char c_whiteSpace = '\r';

	private const char c_newLine = '\n';

	private const char c_semiColon = ';';

	private const char c_doubleQuotes = '"';

	private const char c_backQuote = '`';

	private const char c_openParenthesis = '(';

	private const char c_closeParenthesis = ')';

	private const char c_ambersion = '&';

	private const char c_percentage = '%';

	private const char c_dollarSign = '$';

	private const char c_hash = '#';

	private const char c_atsymbol = '@';

	private const char c_exclamation = '!';

	private const char c_plus = '+';

	private const char c_caret = '^';

	private const char c_openBracket = '[';

	private const char c_closeBracket = ']';

	private const char c_forwardSlash = '/';

	private const char c_questionmark = '?';

	private const char c_greaterthan = '>';

	private const char c_lesserthan = '<';

	private const char c_comma = ',';

	private const char c_verticalBar = '|';

	private const char c_colon = ':';

	private RtfTableType m_currRtfTableType;

	private RtfReader m_rtfReader;

	private string m_token;

	internal char m_prevChar;

	private bool m_bIsImageBytes;

	internal bool m_bIsReadNewChar = true;

	private RtfTokenType m_rtfTokenType;

	private char m_newChar;

	private List<string> m_commentStartRange;

	private int m_commentCount = 2;

	private string m_prevToken = string.Empty;

	private string m_commStartRangeTokenKey = string.Empty;

	internal char[] m_delimeters = new char[28]
	{
		'{', '}', '\\', ' ', '\r', '\n', ';', '"', '`', '(',
		')', '[', ']', '&', '%', '$', '#', '@', '!', '+',
		'^', '/', '?', '>', '<', ',', '|', ':'
	};

	public bool IsImageBytes
	{
		get
		{
			return m_bIsImageBytes;
		}
		set
		{
			m_bIsImageBytes = value;
		}
	}

	public List<string> CommentRangeStartId
	{
		get
		{
			if (m_commentStartRange == null)
			{
				m_commentStartRange = new List<string>();
			}
			return m_commentStartRange;
		}
		set
		{
			m_commentStartRange = value;
		}
	}

	public RtfTableType CurrRtfTableType
	{
		get
		{
			return m_currRtfTableType;
		}
		set
		{
			m_currRtfTableType = value;
		}
	}

	public RtfTokenType CurrRtfTokenType
	{
		get
		{
			return m_rtfTokenType;
		}
		set
		{
			m_rtfTokenType = value;
		}
	}

	public RtfLexer(RtfReader rtfReader)
	{
		m_rtfReader = rtfReader;
	}

	public string ReadNextToken(string prevTokenKey)
	{
		return ReadNextToken(prevTokenKey, isLevelText: false);
	}

	internal string ReadNextToken(string prevTokenKey, bool isLevelText)
	{
		m_token = null;
		if (m_bIsReadNewChar)
		{
			m_newChar = m_rtfReader.ReadChar();
		}
		else
		{
			m_newChar = m_prevChar;
		}
		switch (m_newChar)
		{
		case '{':
			m_bIsReadNewChar = true;
			return m_newChar.ToString();
		case '}':
			m_bIsReadNewChar = true;
			return m_newChar.ToString();
		case '\\':
			m_token = '\\'.ToString();
			m_token = ReadControlWord(m_token, prevTokenKey, isLevelText);
			return m_token;
		case ' ':
			m_token = ' '.ToString();
			m_token = ReadDocumentElement(m_token, prevTokenKey);
			if (!m_prevToken.Equals(string.Empty) && m_token != m_prevToken && m_token != "-")
			{
				m_token = GenerateCommentInfo(m_prevToken, m_token);
				m_prevToken = string.Empty;
			}
			return m_token;
		case '\n':
		case '\r':
			m_bIsReadNewChar = true;
			m_token = m_newChar.ToString();
			return m_token;
		default:
			if (IsImageBytes)
			{
				m_token = m_newChar + m_rtfReader.ReadImageBytes();
				m_bIsReadNewChar = true;
				return m_token;
			}
			m_bIsReadNewChar = true;
			if (m_commStartRangeTokenKey != string.Empty)
			{
				m_token += m_newChar;
				m_token = ReadDocumentElement(m_token, prevTokenKey);
				m_commStartRangeTokenKey = string.Empty;
				return m_token;
			}
			return m_newChar.ToString();
		}
	}

	private string ReadControlWord(string token, string prevTokenKey, bool isLevelText)
	{
		m_newChar = m_rtfReader.ReadChar();
		if (m_newChar == '\\' || m_newChar == '{' || m_newChar == '}')
		{
			m_bIsReadNewChar = true;
			return token + m_newChar;
		}
		m_bIsReadNewChar = false;
		while ((Array.IndexOf(m_delimeters, m_newChar) == -1 || (isLevelText && (m_newChar == ')' || m_newChar == '('))) && (!StartsWithExt(token, "\\u") || token.Length <= 2 || !char.IsNumber(token[2]) || char.IsNumber(m_newChar)))
		{
			token += m_newChar;
			if (m_rtfReader.Position >= m_rtfReader.Length)
			{
				break;
			}
			m_newChar = m_rtfReader.ReadChar();
			if (token.Equals("\\atrfstart") || token.Equals("\\atrfend") || token.Equals("\\atnref") || token.Equals("\\atnid") || StartsWithExt(token, "\\atndate") || token.Equals("\\atnauthor") || token.Equals("\\atnparent"))
			{
				m_prevToken = token;
			}
			if (token.Equals("\\annotation"))
			{
				m_commentCount++;
				string text = m_commentCount.ToString();
				token += text;
			}
			if ((StartsWithExt(token, "\\bin") || prevTokenKey == "bin") && m_newChar == '\u0001')
			{
				m_bIsReadNewChar = true;
				break;
			}
		}
		m_prevChar = m_newChar;
		if (token == null)
		{
			return m_newChar.ToString();
		}
		return token;
	}

	private string GenerateCommentInfo(string prevToken, string token)
	{
		if (prevToken.Equals("\\atrfstart"))
		{
			m_commStartRangeTokenKey = "atrfstart";
			m_commentCount++;
			CommentRangeStartId.Add(token);
			prevToken += token.TrimStart();
		}
		if (prevToken.Equals("\\atrfend"))
		{
			foreach (string item in CommentRangeStartId)
			{
				if (item.Equals(token))
				{
					prevToken += token.TrimStart();
				}
			}
		}
		if (prevToken.Equals("\\atnref"))
		{
			foreach (string item2 in CommentRangeStartId)
			{
				if (item2.Equals(token))
				{
					prevToken += token.TrimStart();
				}
			}
		}
		if (prevToken.Equals("\\atndate"))
		{
			prevToken += token.TrimStart();
		}
		if (prevToken.Equals("\\atnid"))
		{
			prevToken += token.TrimStart();
		}
		if (prevToken.Equals("\\atnauthor"))
		{
			prevToken += token.TrimStart();
		}
		if (prevToken.Equals("\\atnparent"))
		{
			prevToken += token;
		}
		return prevToken;
	}

	private string ReadDocumentElement(string token, string prevTokenKey)
	{
		if (IsImageBytes)
		{
			m_newChar = m_rtfReader.ReadChar();
			if (m_newChar != '\\' && m_newChar != ';' && m_newChar != '{' && m_newChar != '}' && m_newChar != '\r' && m_newChar != '\n')
			{
				token = m_newChar.ToString();
				if (!(prevTokenKey == "bin") || m_newChar != '\u0001')
				{
					token += m_rtfReader.ReadImageBytes();
				}
				m_bIsReadNewChar = true;
				return token;
			}
			m_bIsReadNewChar = false;
			m_prevChar = m_newChar;
			return token;
		}
		m_newChar = m_rtfReader.ReadChar();
		while (m_newChar != '\\' && m_newChar != ';' && m_newChar != '{' && m_newChar != '}' && m_newChar != '\r' && m_newChar != '\n')
		{
			token += m_newChar;
			if (m_rtfReader.Position >= m_rtfReader.Length)
			{
				break;
			}
			m_newChar = m_rtfReader.ReadChar();
			if ((m_newChar == '{' || m_newChar == '}') && m_prevChar == '\u0084')
			{
				token += m_newChar;
				if (m_rtfReader.Position >= m_rtfReader.Length)
				{
					break;
				}
				m_newChar = m_rtfReader.ReadChar();
			}
			m_prevChar = m_newChar;
		}
		m_prevChar = m_newChar;
		m_bIsReadNewChar = false;
		if (token == null)
		{
			if (m_prevChar != '\\')
			{
				m_bIsReadNewChar = true;
			}
			return m_newChar.ToString();
		}
		return token;
	}

	public void Close()
	{
		m_rtfReader.Close();
	}

	internal bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}
}
