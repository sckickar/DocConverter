using System;
using System.Collections.Generic;
using System.Text;

namespace DocGen.Pdf;

internal class ContentParser
{
	private ContentLexer m_lexer;

	private StringBuilder m_operatorParams;

	private PdfRecordCollection m_recordCollection;

	private List<string> m_operands = new List<string>();

	private List<byte> m_inlineImageBytes = new List<byte>();

	private bool m_isByteOperands;

	internal bool IsTextExtractionProcess;

	internal bool ConformanceEnabled;

	internal List<string> fontkeys = new List<string>();

	private static string[] operators = new string[79]
	{
		"b", "B", "bx", "Bx", "BDC", "BI", "BMC", "BT", "BX", "c",
		"cm", "CS", "cs", "d", "d0", "d1", "Do", "DP", "EI", "EMC",
		"ET", "EX", "f", "F", "fx", "G", "g", "gs", "h", "i",
		"ID", "j", "J", "K", "k", "l", "m", "M", "MP", "n",
		"q", "Q", "re", "RG", "rg", "ri", "s", "S", "SC", "sc",
		"SCN", "scn", "sh", "f*", "Tx", "Tc", "Td", "TD", "Tf", "Tj",
		"TJ", "TL", "Tm", "Tr", "Ts", "Tw", "Tz", "v", "w", "W",
		"W*", "Wx", "y", "T*", "b*", "B*", "'", "\"", "true"
	};

	public ContentParser(byte[] contentStream)
	{
		m_lexer = new ContentLexer(contentStream);
		m_operatorParams = m_lexer.OperatorParams;
		m_recordCollection = new PdfRecordCollection();
	}

	internal void Dispose()
	{
		m_operatorParams.Clear();
		m_lexer.Dispose();
		if (!ConformanceEnabled)
		{
			m_recordCollection.RecordCollection.Clear();
		}
	}

	public PdfRecordCollection ReadContent()
	{
		ParseObject(TokenType.Eof);
		if (IsTextExtractionProcess)
		{
			m_lexer.Dispose();
		}
		return m_recordCollection;
	}

	private void ParseObject(TokenType stop)
	{
		TokenType nextToken;
		while ((nextToken = GetNextToken()) != TokenType.Eof && nextToken != stop)
		{
			switch (nextToken)
			{
			case TokenType.None:
				return;
			case TokenType.Integer:
				if (m_operatorParams.ToString() == "-")
				{
					m_operands.Add("0");
				}
				else
				{
					m_operands.Add(m_operatorParams.ToString());
				}
				break;
			case TokenType.Real:
				m_operands.Add(m_operatorParams.ToString());
				break;
			case TokenType.String:
			case TokenType.HexString:
			case TokenType.UnicodeString:
			case TokenType.UnicodeHexString:
				m_operands.Add(m_operatorParams.ToString());
				break;
			case TokenType.Name:
				if (m_operatorParams.ToString() == "/Artifact")
				{
					m_lexer.IsContainsArtifacts = true;
				}
				m_operands.Add(m_operatorParams.ToString());
				break;
			case TokenType.Operator:
				if (m_operatorParams.ToString() == "true")
				{
					m_operands.Add(m_operatorParams.ToString());
				}
				else if (m_operatorParams.ToString() == "ID")
				{
					CreateRecord();
					m_operands.Clear();
					ConsumeValue();
				}
				else
				{
					CreateRecord();
					m_operands.Clear();
				}
				break;
			case TokenType.EndArray:
				throw new InvalidOperationException("Error while parsing content");
			}
		}
	}

	private TokenType GetNextToken()
	{
		return m_lexer.GetNextToken();
	}

	private void ConsumeValue()
	{
		char nextCharforInlineStream;
		char nextInlineChar;
		if (ConformanceEnabled)
		{
			while (true)
			{
				List<char> list = new List<char>();
				int num = 0;
				nextCharforInlineStream = m_lexer.GetNextCharforInlineStream();
				if (nextCharforInlineStream == 'E')
				{
					nextInlineChar = m_lexer.GetNextInlineChar();
					if (nextInlineChar == 'I')
					{
						char nextInlineChar2 = m_lexer.GetNextInlineChar();
						list.Add(m_lexer.GetNextChar(value: true));
						if ((nextInlineChar2 == ' ' || nextInlineChar2 == '\n' || nextInlineChar2 == '\uffff' || nextInlineChar2 == '\r') && list.Count > 0)
						{
							while (list[list.Count - 1] == ' ' || list[list.Count - 1] == '\r' || list[list.Count - 1] == '\n')
							{
								list.Add(m_lexer.GetNextChar());
								num++;
							}
						}
						if (!IsTextExtractionProcess)
						{
							m_lexer.ResetContentPointer(num);
						}
						if ((nextInlineChar2 == ' ' || nextInlineChar2 == '\n' || nextInlineChar2 == '\uffff' || nextInlineChar2 == '\r') && list.Count > 0)
						{
							if (list[list.Count - 1] == 'Q' || list[list.Count - 1] == '\uffff' || list[list.Count - 1] == 'S')
							{
								break;
							}
							m_inlineImageBytes.Add((byte)nextCharforInlineStream);
							m_inlineImageBytes.Add((byte)nextInlineChar);
							m_inlineImageBytes.Add((byte)nextInlineChar2);
							if (list.Count > 1)
							{
								list.RemoveAt(0);
								list.RemoveAt(list.Count - 1);
							}
							foreach (char item in list)
							{
								m_inlineImageBytes.Add((byte)item);
							}
							nextCharforInlineStream = m_lexer.GetNextCharforInlineStream();
						}
						else
						{
							m_inlineImageBytes.Add((byte)nextCharforInlineStream);
							m_inlineImageBytes.Add((byte)nextInlineChar);
							m_inlineImageBytes.Add((byte)nextInlineChar2);
							if (list.Count > 0)
							{
								m_inlineImageBytes.Add((byte)list[0]);
							}
							nextCharforInlineStream = m_lexer.GetNextCharforInlineStream();
						}
					}
					else
					{
						m_inlineImageBytes.Add((byte)nextCharforInlineStream);
						m_inlineImageBytes.Add((byte)nextInlineChar);
					}
				}
				else
				{
					m_inlineImageBytes.Add((byte)nextCharforInlineStream);
				}
				list.Clear();
			}
			m_operatorParams.Length = 0;
			m_operatorParams.Append(nextCharforInlineStream);
			m_operatorParams.Append(nextInlineChar);
			m_isByteOperands = true;
			CreateRecord();
			m_isByteOperands = false;
			m_inlineImageBytes.Clear();
			nextInlineChar = m_lexer.GetNextInlineChar();
			return;
		}
		while (true)
		{
			int num2 = 0;
			nextCharforInlineStream = m_lexer.GetNextCharforInlineStream();
			if (nextCharforInlineStream == 'E' || nextCharforInlineStream == '\uffff')
			{
				nextInlineChar = m_lexer.GetNextInlineChar();
				if (nextInlineChar == 'I' || (nextInlineChar == '\uffff' && nextCharforInlineStream == '\uffff'))
				{
					char nextInlineChar2 = m_lexer.GetNextInlineChar();
					char nextChar = m_lexer.GetNextChar(value: true);
					while (nextChar == ' ' || nextChar == '\r' || nextChar == '\n')
					{
						nextChar = m_lexer.GetNextChar();
						num2++;
					}
					if (!IsTextExtractionProcess)
					{
						m_lexer.ResetContentPointer(num2);
					}
					if (nextInlineChar2 == ' ' || nextInlineChar2 == '\n' || nextInlineChar2 == '\uffff' || nextInlineChar2 == '\r')
					{
						if (nextChar == 'Q' || nextChar == '\uffff' || nextChar == 'S' || char.IsDigit(nextChar))
						{
							break;
						}
					}
					else
					{
						m_inlineImageBytes.Add((byte)nextCharforInlineStream);
						m_inlineImageBytes.Add((byte)nextInlineChar);
						m_inlineImageBytes.Add((byte)nextInlineChar2);
						m_inlineImageBytes.Add((byte)nextChar);
						nextCharforInlineStream = m_lexer.GetNextCharforInlineStream();
					}
				}
				else
				{
					m_inlineImageBytes.Add((byte)nextCharforInlineStream);
					m_inlineImageBytes.Add((byte)nextInlineChar);
				}
			}
			else
			{
				m_inlineImageBytes.Add((byte)nextCharforInlineStream);
			}
		}
		m_operatorParams.Length = 0;
		m_operatorParams.Append(nextCharforInlineStream);
		m_operatorParams.Append(nextInlineChar);
		m_isByteOperands = true;
		CreateRecord();
		m_isByteOperands = false;
		m_inlineImageBytes.Clear();
		nextInlineChar = m_lexer.GetNextInlineChar();
	}

	private void CreateRecord()
	{
		string text = m_operatorParams.ToString();
		Array.IndexOf(operators, text);
		PdfRecord pdfRecord = null;
		_ = 0;
		pdfRecord = (m_isByteOperands ? new PdfRecord(text, m_inlineImageBytes.ToArray()) : new PdfRecord(text, m_operands.ToArray()));
		if (ConformanceEnabled && (pdfRecord.OperatorName == "Tf" || pdfRecord.OperatorName == "TJ"))
		{
			if (pdfRecord.OperatorName == "TJ")
			{
				fontkeys.Add(pdfRecord.OperatorName);
			}
			else
			{
				string item = pdfRecord.Operands[0].Replace("/", "");
				fontkeys.Add(item);
			}
		}
		m_recordCollection.Add(pdfRecord);
	}
}
