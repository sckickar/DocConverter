using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DocGen.Pdf.IO;

internal class PdfLexer
{
	private enum YYError
	{
		Internal,
		Match
	}

	private enum State
	{
		HexString = 1,
		String = 2,
		YYINITIAL = 0
	}

	private const int YY_BUFFER_SIZE = 8192;

	private const int YY_F = -1;

	private const int YY_NO_STATE = -1;

	private const int YY_NOT_ACCEPT = 0;

	private const int YY_START = 1;

	private const int YY_END = 2;

	private const int YY_NO_ANCHOR = 4;

	private const int YY_BOL = 256;

	private const int YY_EOF = 257;

	private const string Prefix = "<<";

	private StringBuilder m_string = new StringBuilder();

	private int m_paren;

	private bool m_bSkip;

	internal bool isArray;

	internal string objectName;

	internal bool m_checkEof;

	private TextReader m_yyReader;

	private int m_yyBufferIndex;

	private int m_yyBufferRead;

	private int m_yyBufferStart;

	private int m_yyBufferEnd;

	private char[] m_yyBuffer;

	private int m_yyLine;

	private bool TType = true;

	private bool m_yyAtBol;

	private State m_yyLexicalState;

	internal bool fdfImport;

	private static readonly int[] m_yyStateDtrans = new int[3] { 0, 81, 83 };

	private bool m_yyLastWasCr;

	private string[] m_yyErrorString = new string[2] { "Error: Internal error.\n", "Error: Unmatched input.\n" };

	private int[] m_yyAccept = new int[88]
	{
		0, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 4, 4, 4, 4, 0, 4, 4, 4,
		4, 0, 4, 0, 4, 0, 4, 0, 4, 0,
		4, 0, 4, 0, 4, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0
	};

	private int[] m_yyCmap = UnpackFromString(1, 258, "3,17:8,3,11,17,3,4,17:18,3,17:4,1,17:2,7,2,17,26,17,26,28,16,27:10,17:2,5,17,6,17:2,13:6,17:11,35,17:8,14,12,15,17:3,23,30,13,33,21,22,17:2,36,31,17,24,34,32,29,17:2,19,25,18,20,17:2,37,17:2,10,17,10,17:128,8,9,0:2")[0];

	private int[] m_yyRmap = UnpackFromString(1, 88, "0,1,2,1:2,3,4,1:2,5,6,7,1:3,8,1:18,9,1,10,11,12,13,14,15,16,17,18,19,20,21,7,8:2,22,23,24,25,13,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57")[0];

	private int[][] m_yyNext = UnpackFromString(58, 38, "1,2,3,4:2,5,37,6,3:3,4,3:2,7,8,9,3,42,3:2,44,10,3:2,46,48,11,50,52,3:2,38,3:2,12,3,54,-1:39,2:3,-1,2:6,-1,2:26,-1:5,13,-1:40,36,-1:37,9:2,-1:2,9:2,-1:3,9:21,-1:23,45,-1:41,11,49,-1:36,15,-1:11,35:3,84,35:33,-1:9,55,-1:34,14,-1:51,85,-1:18,63,17,63:8,64,63:26,-1,30:3,82,30:33,-1:20,56,-1:2,57,-1:33,41,-1:51,58,-1:36,43,-1:29,59,-1:31,47,-1:38,86,-1:3,60,-1:45,16,-1:36,51,-1:28,62,-1:35,53,-1:39,18,-1:52,65,-1:26,66,-1:3,67,-1:33,56,-1:31,87,-1:42,19,-1:35,20,-1:16,55:3,-1,55:6,-1,-1:26,-1,64,39,64,63,64:33,-1:24,69,-1:31,70,-1:49,71,-1:30,72,-1:35,74,-1:35,75,-1:49,21,-1:40,22,-1:40,76,-1:19,23,-1:39,77,-1:35,78,-1:41,79,-1:35,80,-1:50,24,-1:25,25,-1:15,1,26:2,27:2,26,28,26:4,27,40,29,26:7,29:3,26:3,29,26:2,29,26:2,29,26:4,-1:11,30,-1:26,1,31,32,31:4,33,31:4,34,31:25,-1:11,35,-1:50,61,-1:34,68,-1:34,73,-1:19");

	internal string Text => YyText();

	internal int Line => m_yyLine;

	internal StringBuilder StringText => m_string;

	internal long Position => (m_yyReader as PdfReader).Position - m_yyBufferRead + m_yyBufferIndex;

	internal bool Skip
	{
		get
		{
			return m_bSkip;
		}
		set
		{
			m_bSkip = value;
		}
	}

	internal void Reset()
	{
		m_yyBuffer = new char[8192];
		m_yyBufferRead = 0;
		m_yyBufferIndex = 0;
		m_yyBufferStart = 0;
		m_yyBufferEnd = 0;
		m_yyLine = 0;
		m_yyAtBol = true;
		m_yyLexicalState = State.YYINITIAL;
	}

	internal byte[] Read(int count)
	{
		List<byte> list = new List<byte>(count);
		YyMarkStart();
		if (m_yyBufferRead - m_yyBufferStart < count)
		{
			while (m_yyBuffer.Length - m_yyBufferStart < count)
			{
				m_yyBuffer = YyDouble(m_yyBuffer);
			}
		}
		int num = YyRead();
		if (m_yyBufferRead - m_yyBufferStart < num && num > count)
		{
			count = num;
		}
		for (int num2 = m_yyBufferStart; num2 < m_yyBufferStart + count; num2 = (m_yyBufferIndex = num2 + 1))
		{
			list.Add((byte)m_yyBuffer[num2]);
		}
		YyMarkStart();
		YyMarkEnd();
		return list.ToArray();
	}

	internal void SkipNewLine()
	{
		m_yyBufferIndex = m_yyBufferStart + 1;
		if (m_yyBuffer[m_yyBufferIndex] == '\r')
		{
			if (m_yyBuffer[m_yyBufferIndex + 1] == '\n')
			{
				m_yyBufferIndex += 2;
			}
		}
		else if (m_yyBuffer[m_yyBufferIndex] == '\n' && m_yyBuffer[m_yyBufferIndex - 1] != '\n')
		{
			m_yyBufferIndex++;
		}
		YyMarkStart();
	}

	internal void SkipToken()
	{
		m_yyBufferStart = m_yyBufferEnd;
	}

	internal void MoveBack()
	{
		m_yyBufferIndex = m_yyBufferStart - 1;
	}

	internal PdfLexer(TextReader reader)
		: this()
	{
		if (reader == null)
		{
			throw new PdfException("Error: Bad input stream initializer.");
		}
		m_yyReader = reader;
	}

	internal PdfLexer(Stream inStream)
		: this()
	{
		if (inStream == null)
		{
			throw new PdfException("Error: Bad input stream initializer.");
		}
		m_yyReader = new StreamReader(inStream);
	}

	private PdfLexer()
	{
		m_yyBuffer = new char[8192];
		m_yyBufferRead = 0;
		m_yyBufferIndex = 0;
		m_yyBufferStart = 0;
		m_yyBufferEnd = 0;
		m_yyLine = 0;
		m_yyAtBol = true;
		m_yyLexicalState = State.YYINITIAL;
	}

	public TokenType GetNextToken()
	{
		if (m_checkEof && m_yyReader is PdfReader { Stream: not null } pdfReader && pdfReader.Position == pdfReader.Stream.Length)
		{
			return TokenType.Eof;
		}
		int num = 4;
		int num2 = m_yyStateDtrans[(int)m_yyLexicalState];
		int num3 = -1;
		int num4 = -1;
		bool flag = true;
		YyMarkStart();
		if (m_yyAccept[num2] != 0)
		{
			num4 = num2;
			YyMarkEnd();
		}
		while (true)
		{
			int num5 = ((!flag || !m_yyAtBol) ? YyAdvance() : 256);
			num3 = -1;
			num3 = m_yyNext[m_yyRmap[num2]][m_yyCmap[num5]];
			if (257 == num5 && flag)
			{
				return TokenType.Eof;
			}
			if (-1 != num3)
			{
				num2 = num3;
				flag = false;
				if (m_yyAccept[num2] != 0)
				{
					num4 = num2;
					YyMarkEnd();
				}
				continue;
			}
			if (-1 == num4)
			{
				break;
			}
			num = m_yyAccept[num4];
			if ((2u & (uint)num) != 0)
			{
				YyMoveEnd();
			}
			YyToMark();
			switch (num4)
			{
			case 5:
				YyBegin(State.HexString);
				return TokenType.HexStringStart;
			case 6:
				YyBegin(State.String);
				StringText.Length = 0;
				break;
			case 7:
				return TokenType.ArrayStart;
			case 8:
				return TokenType.ArrayEnd;
			case 9:
				return TokenType.Name;
			case 10:
				return TokenType.ObjectType;
			case 11:
				if (!fdfImport && m_yyBuffer != null && m_yyBuffer.Length > 1 && m_yyBuffer[0] == '%' && m_yyBuffer[1] == Convert.ToChar(226))
				{
					YyError(YYError.Match, fatal: true);
					break;
				}
				return TokenType.Number;
			case 12:
				return TokenType.Reference;
			case 13:
				return TokenType.DictionaryStart;
			case 14:
				return TokenType.DictionaryEnd;
			case 15:
				return TokenType.Real;
			case 16:
				return TokenType.ObjectStart;
			case 17:
				return TokenType.UnicodeString;
			case 18:
				return TokenType.Boolean;
			case 19:
				return TokenType.Null;
			case 20:
				return TokenType.XRef;
			case 21:
				return TokenType.ObjectEnd;
			case 22:
				return TokenType.StreamStart;
			case 23:
				return TokenType.Trailer;
			case 24:
				return TokenType.StreamEnd;
			case 25:
				return TokenType.StartXRef;
			case 26:
				return TokenType.HexStringWeird;
			case 27:
				return TokenType.WhiteSpace;
			case 28:
				YyBegin(State.YYINITIAL);
				return TokenType.HexStringEnd;
			case 29:
				return TokenType.HexDigit;
			case 30:
				return TokenType.HexStringWeirdEscape;
			case 31:
				StringText.Append(YyText());
				break;
			case 32:
				if (m_paren > 0)
				{
					StringText.Append(YyText());
					m_paren--;
					break;
				}
				YyBegin(State.YYINITIAL);
				return TokenType.String;
			case 33:
				StringText.Append(YyText());
				m_paren++;
				break;
			case 35:
				StringText.Append(YyText());
				break;
			case 37:
				YyError(YYError.Match, fatal: true);
				break;
			case 38:
				return TokenType.ObjectType;
			case 39:
				return TokenType.UnicodeString;
			case 40:
				return TokenType.HexStringWeird;
			case 42:
				return TokenType.Unknown;
			case 44:
				return TokenType.Unknown;
			case 46:
				if (m_yyBuffer[m_yyBufferIndex - 1] != 's' || (m_yyBuffer[m_yyBufferIndex] != 't' && m_yyBuffer[m_yyBufferIndex] != '%'))
				{
					YyError(YYError.Match, fatal: true);
				}
				break;
			case 48:
				YyError(YYError.Match, fatal: true);
				break;
			case 50:
				if (isArray)
				{
					if (double.TryParse(new string(m_yyBuffer, m_yyBufferIndex - 2, 2), NumberStyles.Float, CultureInfo.InvariantCulture, out var _) && m_yyBuffer[m_yyBufferIndex - 1] == '.' && (m_yyBuffer[m_yyBufferIndex] == ' ' || m_yyBuffer[m_yyBufferIndex] == ']'))
					{
						break;
					}
				}
				else if (m_yyBuffer[m_yyBufferIndex - 1] == '.' && m_yyBuffer[m_yyBufferIndex] == '-')
				{
					return TokenType.Unknown;
				}
				YyError(YYError.Match, fatal: true);
				break;
			case 54:
				YyError(YYError.Match, fatal: true);
				break;
			default:
				YyError(YYError.Internal, fatal: false);
				break;
			case -47:
			case -46:
			case -45:
			case -44:
			case -43:
			case -42:
			case -41:
			case -40:
			case -39:
			case -38:
			case -37:
			case -36:
			case -35:
			case -34:
			case -33:
			case -32:
			case -31:
			case -30:
			case -29:
			case -28:
			case -27:
			case -26:
			case -25:
			case -24:
			case -23:
			case -22:
			case -21:
			case -20:
			case -19:
			case -18:
			case -17:
			case -16:
			case -15:
			case -14:
			case -13:
			case -12:
			case -11:
			case -10:
			case -9:
			case -8:
			case -7:
			case -6:
			case -5:
			case -4:
			case -3:
			case -2:
			case 1:
			case 2:
			case 3:
			case 4:
			case 34:
			case 52:
				break;
			}
			flag = true;
			num2 = m_yyStateDtrans[(int)m_yyLexicalState];
			num3 = -1;
			num4 = -1;
			YyMarkStart();
			if (m_yyAccept[num2] != 0)
			{
				num4 = num2;
				YyMarkEnd();
			}
		}
		throw new PdfException("Lexical Error: Unmatched Input.");
	}

	private void YyBegin(State state)
	{
		m_yyLexicalState = state;
	}

	private int YyAdvance()
	{
		if (m_yyBufferIndex < m_yyBufferRead)
		{
			return m_yyBuffer[m_yyBufferIndex++];
		}
		if (m_yyBufferStart != 0)
		{
			int num = m_yyBufferStart;
			int num2 = 0;
			while (num < m_yyBufferRead)
			{
				m_yyBuffer[num2] = m_yyBuffer[num];
				num++;
				num2++;
			}
			m_yyBufferEnd -= m_yyBufferStart;
			m_yyBufferStart = 0;
			m_yyBufferRead = num2;
			m_yyBufferIndex = num2;
			if (YyRead() <= 0)
			{
				return 257;
			}
		}
		while (m_yyBufferIndex >= m_yyBufferRead)
		{
			if (m_yyBufferIndex >= m_yyBuffer.Length)
			{
				m_yyBuffer = YyDouble(m_yyBuffer);
			}
			if (YyRead() <= 0)
			{
				return 257;
			}
		}
		return m_yyBuffer[m_yyBufferIndex++];
	}

	private int YyRead()
	{
		int num = m_yyReader.Read(m_yyBuffer, m_yyBufferRead, m_yyBuffer.Length - m_yyBufferRead);
		if (num > 0)
		{
			m_yyBufferRead += num;
		}
		return num;
	}

	private void YyMoveEnd()
	{
		if (m_yyBufferEnd > m_yyBufferStart && '\n' == m_yyBuffer[m_yyBufferEnd - 1])
		{
			m_yyBufferEnd--;
		}
		if (m_yyBufferEnd > m_yyBufferStart && '\r' == m_yyBuffer[m_yyBufferEnd - 1])
		{
			m_yyBufferEnd--;
		}
	}

	private void YyMarkStart()
	{
		for (int i = m_yyBufferStart; i < m_yyBufferIndex; i++)
		{
			if ('\n' == m_yyBuffer[i] && !m_yyLastWasCr)
			{
				m_yyLine++;
			}
			if ('\r' == m_yyBuffer[i])
			{
				m_yyLine++;
				m_yyLastWasCr = true;
			}
			else
			{
				m_yyLastWasCr = false;
			}
		}
		m_yyBufferStart = m_yyBufferIndex;
	}

	private void YyMarkEnd()
	{
		m_yyBufferEnd = m_yyBufferIndex;
	}

	private void YyToMark()
	{
		m_yyBufferIndex = m_yyBufferEnd;
		m_yyAtBol = m_yyBufferEnd > m_yyBufferStart && ('\r' == m_yyBuffer[m_yyBufferEnd - 1] || '\n' == m_yyBuffer[m_yyBufferEnd - 1] || '\u07ec' == m_yyBuffer[m_yyBufferEnd - 1] || '\u07ed' == m_yyBuffer[m_yyBufferEnd - 1]);
	}

	private string YyText()
	{
		if (m_yyBuffer.Length > 2 && m_yyBufferEnd > 2)
		{
			char c = m_yyBuffer[m_yyBufferEnd - 1];
			char c2 = m_yyBuffer[m_yyBufferEnd - 2];
			int num = m_yyBufferEnd - m_yyBufferStart;
			if (c == ')' && (c2 == '\\' || c2 == '\0') && num > 3)
			{
				int yyBufferEnd = m_yyBufferEnd;
				string text = new string(m_yyBuffer);
				yyBufferEnd = text.IndexOf(c, m_yyBufferStart) + 1;
				int num2 = 0;
				while (text[yyBufferEnd - 2] == '\\')
				{
					yyBufferEnd = text.IndexOf(c, yyBufferEnd) + 1;
					if (yyBufferEnd > 0)
					{
						num2 = yyBufferEnd;
						continue;
					}
					yyBufferEnd = num2;
					break;
				}
				if (text[yyBufferEnd] == '>' && text[yyBufferEnd + 1] == '>')
				{
					m_yyBufferIndex = yyBufferEnd;
					Skip = false;
				}
				else if (text.Length > yyBufferEnd + 2)
				{
					if (text[yyBufferEnd + 2] == '/')
					{
						m_yyBufferIndex = yyBufferEnd;
						Skip = false;
					}
					else if (text[yyBufferEnd + 1] == '/')
					{
						m_yyBufferIndex = yyBufferEnd;
						Skip = false;
					}
					else if (text[yyBufferEnd] == '/')
					{
						m_yyBufferIndex = yyBufferEnd;
						Skip = false;
					}
					else if (text[yyBufferEnd - 1] == ')')
					{
						m_yyBufferIndex = yyBufferEnd;
						Skip = false;
					}
					else
					{
						Skip = true;
					}
				}
				else
				{
					Skip = true;
				}
				if (text.IndexOf(')', m_yyBufferEnd + 1) >= 0 && text[yyBufferEnd - 1] == ')' && m_yyBufferEnd < yyBufferEnd + 1)
				{
					m_yyBufferEnd = m_yyBufferIndex;
				}
				else
				{
					m_yyBufferEnd = yyBufferEnd;
				}
			}
			else if (c == ')' && num > 3)
			{
				int yyBufferEnd2 = m_yyBufferEnd;
				string text2 = new string(m_yyBuffer);
				yyBufferEnd2 = text2.IndexOf(c, m_yyBufferStart) + 1;
				while (text2[yyBufferEnd2 - 2] == '\\')
				{
					yyBufferEnd2 = text2.IndexOf(c, yyBufferEnd2) + 1;
				}
				if (m_yyBufferEnd > yyBufferEnd2 + 1)
				{
					m_yyBufferEnd = yyBufferEnd2;
				}
				if (text2[yyBufferEnd2 - 1] == ')')
				{
					m_yyBufferIndex = yyBufferEnd2 - 1;
					Skip = false;
				}
				else
				{
					Skip = true;
				}
			}
		}
		return new string(m_yyBuffer, m_yyBufferStart, m_yyBufferEnd - m_yyBufferStart);
	}

	private int YyLength()
	{
		return m_yyBufferEnd - m_yyBufferStart;
	}

	private char[] YyDouble(char[] buffer)
	{
		int num = buffer.Length;
		char[] array = new char[2 * num];
		int num2 = 2;
		Buffer.BlockCopy(buffer, 0, array, 0, num * num2);
		return array;
	}

	private void YyError(YYError code, bool fatal)
	{
		if (fatal)
		{
			long position = Position;
			if (objectName != null)
			{
				throw new PdfException(string.Format("Fatal Error occurred at {0}.\n", position + " when reading object type of " + objectName));
			}
			throw new PdfException($"Fatal Error at {position}.\n");
		}
	}

	private static int[][] UnpackFromString(int size1, int size2, string st)
	{
		int num = -1;
		int num2 = 0;
		int num3 = 0;
		int[][] array = new int[size1][];
		for (int i = 0; i < size1; i++)
		{
			array[i] = new int[size2];
		}
		for (int j = 0; j < size1; j++)
		{
			for (int k = 0; k < size2; k++)
			{
				if (num2 != 0)
				{
					array[j][k] = num3;
					num2--;
					continue;
				}
				int num4 = st.IndexOf(',');
				string text = ((num4 == -1) ? st : st.Substring(0, num4));
				st = st.Substring(num4 + 1);
				num = text.IndexOf(':');
				if (num == -1)
				{
					array[j][k] = int.Parse(text, CultureInfo.InvariantCulture);
					continue;
				}
				num2 = int.Parse(text.Substring(num + 1), CultureInfo.InvariantCulture);
				text = text.Substring(0, num);
				num3 = int.Parse(text, CultureInfo.InvariantCulture);
				array[j][k] = num3;
				num2--;
			}
		}
		return array;
	}
}
