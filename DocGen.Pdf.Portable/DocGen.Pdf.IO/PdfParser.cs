using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.IO;

internal class PdfParser
{
	private enum ErrorType
	{
		None,
		Unexpected,
		BadlyFormedReal,
		BadlyFormedInteger,
		BadlyFormedHexString,
		BadlyFormedDictionary,
		UnknownStreamLength
	}

	private CrossTable m_cTable;

	private PdfReader m_reader;

	private PdfLexer m_lexer;

	private TokenType m_next;

	private Queue<int> m_integerQueue = new Queue<int>();

	private PdfCrossTable m_crossTable;

	private bool m_bEncrypt;

	private bool m_colorSpace;

	private bool m_isPassword;

	private bool m_certString;

	private bool m_forceRebuild;

	private bool m_isRGB;

	internal bool fdfImport;

	internal bool Encrypted
	{
		get
		{
			return m_bEncrypt;
		}
		set
		{
			m_bEncrypt = value;
		}
	}

	internal PdfLexer Lexer => m_lexer;

	internal bool ForceRebuild
	{
		get
		{
			return m_forceRebuild;
		}
		set
		{
			m_forceRebuild = value;
		}
	}

	internal long Position => m_reader.Position;

	public PdfParser(CrossTable cTable, PdfReader reader, PdfCrossTable crossTable)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (cTable == null)
		{
			throw new ArgumentNullException("cTable");
		}
		if (crossTable == null)
		{
			throw new ArgumentNullException("crossTable");
		}
		m_reader = reader;
		m_cTable = cTable;
		m_crossTable = crossTable;
		m_lexer = new PdfLexer(reader);
	}

	public IPdfPrimitive Parse(long offset)
	{
		SetOffset(offset);
		Advance();
		return Parse();
	}

	internal void SeekOffset(long offset)
	{
		SetOffset(offset);
		Advance();
		Match(m_next, TokenType.Number);
		Simple();
		Simple();
		Match(m_next, TokenType.ObjectStart);
	}

	public IPdfPrimitive Parse()
	{
		Match(m_next, TokenType.Number);
		Simple();
		Simple();
		Match(m_next, TokenType.ObjectStart);
		Advance();
		IPdfPrimitive result = Simple();
		if (m_next != TokenType.ObjectEnd)
		{
			m_next = TokenType.ObjectEnd;
		}
		Match(m_next, TokenType.ObjectEnd);
		if (!m_lexer.Skip)
		{
			Advance();
			return result;
		}
		m_lexer.Skip = false;
		return result;
	}

	public IPdfPrimitive Trailer(long offset)
	{
		SetOffset(offset);
		return Trailer();
	}

	public IPdfPrimitive Trailer()
	{
		Match(m_next, TokenType.Trailer);
		Advance();
		return Dictionary();
	}

	public long StartXRef()
	{
		Advance();
		SetOffset(m_lexer.Position);
		Match(m_next, TokenType.StartXRef);
		Advance();
		if (m_next == TokenType.Eof)
		{
			ForceRebuild = true;
			return -1L;
		}
		m_lexer.m_checkEof = true;
		PdfNumber pdfNumber = Number() as PdfNumber;
		m_lexer.m_checkEof = false;
		return pdfNumber?.LongValue ?? 0;
	}

	public void SetOffset(long offset)
	{
		m_reader.Position = offset;
		if (m_integerQueue.Count > 0)
		{
			m_integerQueue.Clear();
		}
		m_lexer.Reset();
	}

	public IPdfPrimitive ParseXRefTable(Dictionary<long, ObjectInformation> objects, CrossTable cTable)
	{
		IPdfPrimitive pdfPrimitive = null;
		Advance();
		if (m_next == TokenType.XRef)
		{
			if (!cTable.m_isOpenAndRepair)
			{
				ParseOldXRef(cTable, objects);
			}
			else
			{
				do
				{
					Advance();
					if (m_next == TokenType.Eof && cTable.m_isOpenAndRepair && cTable.Reader.Position == cTable.Reader.Stream.Length)
					{
						cTable.Reader.Position = 0L;
					}
				}
				while (m_next != TokenType.Trailer);
			}
			pdfPrimitive = Trailer();
			PdfDictionary pdfDictionary = pdfPrimitive as PdfDictionary;
			if (pdfDictionary.ContainsKey("Size"))
			{
				int intValue = (pdfDictionary["Size"] as PdfNumber).IntValue;
				int num = 0;
				num = (int)((cTable.m_initialSubsectionCount != cTable.m_initialNumberOfSubsection) ? cTable.m_initialSubsectionCount : cTable.m_initialNumberOfSubsection);
				int num2 = 0;
				num2 = (int)(cTable.m_isOpenAndRepair ? (objects.Count - 1) : cTable.m_totalNumberOfSubsection);
				if (intValue < num + num2 && num > 0 && intValue == num2)
				{
					int num3 = num + num2 - intValue;
					Dictionary<long, ObjectInformation> dictionary = new Dictionary<long, ObjectInformation>();
					foreach (KeyValuePair<long, ObjectInformation> @object in objects)
					{
						dictionary.Add(@object.Key - num3, @object.Value);
					}
					objects = dictionary;
					cTable.m_objects = dictionary;
				}
				else if (intValue != num2 && !cTable.m_isOpenAndRepair && intValue < num2)
				{
					pdfDictionary["Size"] = new PdfNumber(num2);
				}
			}
		}
		else
		{
			pdfPrimitive = Parse();
			if (cTable.m_isOpenAndRepair && PdfCrossTable.Dereference(pdfPrimitive) is PdfDictionary pdfDictionary2 && !pdfDictionary2.ContainsKey("Root"))
			{
				for (int i = 0; i < objects.Count; i++)
				{
					pdfPrimitive = Parse();
					if (PdfCrossTable.Dereference(pdfPrimitive) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("Root"))
					{
						break;
					}
				}
			}
			cTable.ParseNewTable(pdfPrimitive as PdfStream, objects);
		}
		if (pdfPrimitive is PdfDictionary pdfDictionary4 && m_crossTable != null && pdfDictionary4.ContainsKey("XRefStm"))
		{
			try
			{
				long offset = 0L;
				if (PdfCrossTable.Dereference(pdfDictionary4["XRefStm"]) is PdfNumber pdfNumber)
				{
					offset = pdfNumber.LongValue;
				}
				cTable.Parser.SetOffset(offset);
				IPdfPrimitive pdfPrimitive2 = cTable.Parser.Parse(offset);
				if (pdfPrimitive2 is PdfStream)
				{
					cTable.ParseNewTable(pdfPrimitive2 as PdfStream, objects);
				}
			}
			catch
			{
			}
		}
		return pdfPrimitive;
	}

	public void RebuildXrefTable(Dictionary<long, ObjectInformation> newObjects, CrossTable crosstable)
	{
		List<Dictionary<long, ObjectInformation>> list = null;
		Dictionary<long, ObjectInformation> dictionary = null;
		if (crosstable.m_invalidXrefStart)
		{
			list = new List<Dictionary<long, ObjectInformation>>();
			dictionary = new Dictionary<long, ObjectInformation>();
		}
		PdfReader pdfReader = new PdfReader(m_reader.Stream);
		pdfReader.Position = 0L;
		newObjects.Clear();
		byte[] array = new byte[64];
		long num = 0L;
		while (m_reader.Position < pdfReader.Stream.Length - 1)
		{
			long position = pdfReader.Position;
			pdfReader.ReadLine(array, skipWhiteSpace: true);
			if (crosstable.m_invalidXrefStart)
			{
				if (array[0] == 116)
				{
					long position2 = pdfReader.Position;
					if (!PdfString.ByteToString(array).StartsWith("trailer"))
					{
						continue;
					}
					SetOffset(position2 - 8);
					Advance();
					if (m_next == TokenType.Trailer)
					{
						if (Trailer() is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Root"))
						{
							long num2 = 0L;
							num2 = ((num == 0L) ? pdfReader.SearchBack("xref") : num);
							crosstable.m_trailerPosition = num2;
							pdfReader.Position = position2;
							list.Add(dictionary);
							dictionary = new Dictionary<long, ObjectInformation>();
						}
					}
					else
					{
						pdfReader.Position = position2;
					}
				}
				else if (array[0] == 120)
				{
					long position3 = pdfReader.Position;
					if (!PdfString.ByteToString(array).StartsWith("xref"))
					{
						continue;
					}
					num = position3 - 5;
				}
			}
			if (array[0] < 48 || array[0] > 57 || !PdfString.ByteToString(array).Contains("obj"))
			{
				continue;
			}
			long[] array2 = CheckObjectStart(array, m_reader.Position);
			if (array2 == null)
			{
				continue;
			}
			long key = array2[0];
			if (crosstable.m_isOpenAndRepair)
			{
				long position4 = m_reader.Position;
				m_reader.Position = position;
				m_reader.SkipWS();
				if (position != m_reader.Position)
				{
					position = m_reader.Position;
				}
				m_reader.Position = position4;
			}
			ObjectInformation value = new ObjectInformation(ObjectType.Normal, position, null, crosstable);
			if (crosstable.m_invalidXrefStart)
			{
				dictionary[key] = value;
			}
			else
			{
				newObjects[key] = value;
			}
		}
		if (!crosstable.m_invalidXrefStart)
		{
			return;
		}
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				foreach (KeyValuePair<long, ObjectInformation> item in list[i])
				{
					newObjects[item.Key] = item.Value;
				}
			}
		}
		list.Clear();
		dictionary.Clear();
	}

	private PdfParser(byte[] data)
	{
		m_reader = new PdfReader(new MemoryStream(data));
		m_lexer = new PdfLexer(m_reader);
	}

	private long[] CheckObjectStart(byte[] line, long pos)
	{
		PdfParser pdfParser = new PdfParser(line);
		try
		{
			int num = 1;
			int num2 = 3;
			pdfParser.SetOffset(0L);
			pdfParser.Advance();
			int num3 = 0;
			int num4 = 0;
			if (pdfParser.GetNext() != TokenType.Number)
			{
				return null;
			}
			num3 = pdfParser.ParseInteger().IntValue;
			if (pdfParser.GetNext() != TokenType.Number)
			{
				return null;
			}
			num4 = pdfParser.ParseInteger().IntValue;
			if (num3.ToString().Length + num + num4.ToString().Length + num2 + num != pdfParser.Lexer.Position)
			{
				return null;
			}
			if (!pdfParser.Lexer.Text.Equals("obj"))
			{
				return null;
			}
			return new long[2] { num3, num4 };
		}
		catch
		{
		}
		finally
		{
			pdfParser.m_reader.Stream.Dispose();
		}
		return null;
	}

	internal Dictionary<long, ObjectInformation> FindFirstObject(Dictionary<long, ObjectInformation> newObjects, CrossTable crosstable)
	{
		PdfReader pdfReader = new PdfReader(m_reader.Stream);
		pdfReader.Position = 0L;
		newObjects.Clear();
		byte[] array = new byte[64];
		bool flag = false;
		while (!flag && m_reader.Position < pdfReader.Stream.Length - 1)
		{
			long position = pdfReader.Position;
			pdfReader.ReadLine(array, skipWhiteSpace: true);
			if (array[0] < 48 || array[0] > 57 || !PdfString.ByteToString(array).Contains("obj"))
			{
				continue;
			}
			long[] array2 = CheckObjectStart(array, m_reader.Position);
			if (array2 == null)
			{
				continue;
			}
			long key = array2[0];
			if (crosstable.m_isOpenAndRepair)
			{
				long position2 = m_reader.Position;
				m_reader.Position = position;
				m_reader.SkipWS();
				if (position != m_reader.Position)
				{
					position = m_reader.Position;
				}
				m_reader.Position = position2;
			}
			ObjectInformation value = new ObjectInformation(ObjectType.Normal, position, null, crosstable);
			if (!newObjects.ContainsKey(key))
			{
				newObjects.Add(key, value);
			}
			else if (crosstable.m_isOpenAndRepair)
			{
				newObjects[key] = value;
			}
			flag = true;
			break;
		}
		return newObjects;
	}

	internal IPdfPrimitive Simple()
	{
		IPdfPrimitive result;
		if (m_integerQueue.Count != 0)
		{
			result = Number();
		}
		else
		{
			switch (m_next)
			{
			case TokenType.DictionaryStart:
				result = Dictionary();
				break;
			case TokenType.ArrayStart:
				result = Array();
				break;
			case TokenType.HexStringStart:
				result = HexString();
				break;
			case TokenType.String:
				result = ReadString();
				break;
			case TokenType.UnicodeString:
				result = ReadUnicodeString();
				break;
			case TokenType.Name:
				result = ReadName();
				break;
			case TokenType.Boolean:
				result = ReadBoolean();
				break;
			case TokenType.Real:
				result = Real();
				break;
			case TokenType.Number:
				result = Number();
				break;
			case TokenType.Null:
				result = new PdfNull();
				Advance();
				break;
			default:
				result = null;
				break;
			}
		}
		return result;
	}

	internal char GetObjectFlag()
	{
		Match(m_next, TokenType.ObjectType);
		char result = m_lexer.Text[0];
		Advance();
		return result;
	}

	internal void StartFrom(long offset)
	{
		SetOffset(offset);
		Advance();
	}

	internal FdfObject ParseObject()
	{
		PdfNumber objNum = Simple() as PdfNumber;
		PdfNumber genNum = Simple() as PdfNumber;
		Match(m_next, TokenType.ObjectStart);
		Advance();
		IPdfPrimitive obj = Simple();
		if (m_next != TokenType.ObjectEnd)
		{
			m_next = TokenType.ObjectEnd;
		}
		Match(m_next, TokenType.ObjectEnd);
		return new FdfObject(objNum, genNum, obj);
	}

	private void ParseOldXRef(CrossTable cTable, Dictionary<long, ObjectInformation> objects)
	{
		Advance();
		while (IsSubsection())
		{
			cTable.ParseSubsection(this, objects);
		}
	}

	private bool IsSubsection()
	{
		bool flag = false;
		if (m_next == TokenType.Trailer)
		{
			return false;
		}
		if (m_next == TokenType.Number)
		{
			return true;
		}
		throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
	}

	private void Error(ErrorType error, string additional)
	{
		string text = error switch
		{
			ErrorType.Unexpected => "Unexpected token ", 
			ErrorType.BadlyFormedReal => "Badly formed real number ", 
			ErrorType.BadlyFormedInteger => "Badly formed integer number ", 
			ErrorType.UnknownStreamLength => "Unknown stream length", 
			ErrorType.BadlyFormedDictionary => "Badly formed dictionary ", 
			_ => "Internal error.", 
		};
		if (additional != null)
		{
			text = text + additional + " before " + m_lexer.Position;
		}
		throw new PdfException(text);
	}

	private void Match(TokenType token, TokenType match)
	{
		if (token != match)
		{
			Error(ErrorType.Unexpected, token.ToString());
		}
	}

	internal void Advance()
	{
		if (m_cTable != null && m_cTable.validateSyntax)
		{
			m_lexer.objectName = m_next.ToString();
		}
		m_lexer.fdfImport = fdfImport;
		m_next = m_lexer.GetNextToken();
	}

	internal TokenType GetNext()
	{
		return m_next;
	}

	private IPdfPrimitive ReadName()
	{
		Match(m_next, TokenType.Name);
		PdfName result = new PdfName(m_lexer.Text.Substring(1));
		Advance();
		return result;
	}

	private IPdfPrimitive ReadBoolean()
	{
		Match(m_next, TokenType.Boolean);
		PdfBoolean result = new PdfBoolean(m_lexer.Text == "true");
		Advance();
		return result;
	}

	private IPdfPrimitive ReadUnicodeString()
	{
		char[] array = m_lexer.Text.ToCharArray();
		string text = new string(array, 1, array.Length - 2);
		string value = PdfString.ByteToString(Encoding.BigEndianUnicode.GetPreamble());
		if (array.Length > 1)
		{
			if (text.Substring(0, 2).Equals(value))
			{
				if (CheckForExtraSequence(text))
				{
					text = ProcessEscapes(text, isComplete: false);
				}
				ProcessUnicodeWithPreamble(ref text);
			}
			else
			{
				text = ProcessUnicodeEscapes(text);
			}
		}
		else
		{
			text = ProcessUnicodeEscapes(text);
		}
		PdfString result = new PdfString(text);
		if (!m_lexer.Skip)
		{
			Advance();
			return result;
		}
		m_next = TokenType.DictionaryEnd;
		return result;
	}

	private string ProcessUnicodeEscapes(string text)
	{
		StringBuilder stringBuilder = new StringBuilder(text.Length / 2);
		bool flag = true;
		char c = '\0';
		foreach (char c2 in text)
		{
			if (flag)
			{
				if (c2 == ' ')
				{
					stringBuilder.Append(c2);
					flag = !flag;
				}
				else
				{
					c = (char)((uint)c2 << 8);
				}
			}
			else if (c2 != '\\' && c2 != '\r')
			{
				if (c + c2 <= 257)
				{
					c += c2;
					stringBuilder.Append(c);
				}
				else if (stringBuilder.Length > 0)
				{
					c = '\0';
					c += c2;
					stringBuilder.Append(c);
				}
			}
			else
			{
				flag = !flag;
			}
			flag = !flag;
		}
		return ProcessEscapes(stringBuilder.ToString(), isComplete: false);
	}

	private IPdfPrimitive ReadString()
	{
		Match(m_next, TokenType.String);
		string text = m_lexer.StringText.ToString();
		bool unicode = false;
		bool flag = false;
		if (m_isPassword)
		{
			text = ProcessEscapes(text, isComplete: false);
		}
		else if (!m_colorSpace)
		{
			DecodeEscapeProcess(ref text, ref unicode);
		}
		else if (m_crossTable != null && m_crossTable.Document != null && m_crossTable.Document.WasEncrypted)
		{
			text = ProcessEscapes(text, isComplete: false);
		}
		else if (m_colorSpace)
		{
			text = ProcessEscapes(text, isComplete: false);
			text = "ColorFound" + text;
		}
		PdfString pdfString = new PdfString(text);
		if (m_colorSpace)
		{
			pdfString.IsColorSpace = true;
		}
		if (!unicode && !flag)
		{
			pdfString.Encode = PdfString.ForceEncoding.ASCII;
		}
		Advance();
		return pdfString;
	}

	private bool CheckForPreamble(string text)
	{
		string value = PdfString.ByteToString(Encoding.BigEndianUnicode.GetPreamble());
		if (text.Length > 1 && text.Substring(0, 2).Equals(value))
		{
			return true;
		}
		return false;
	}

	private bool CheckEscapeSequence(string text)
	{
		if (text != null && text.Length > 2)
		{
			char[] array = text.ToCharArray();
			if (array[0] == '\\' && (array[1] == '\\' || array[1] == '0') && array[2] == '0')
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckForExtraSequence(string text)
	{
		string text2 = text.Substring(2);
		if (text2.Length >= 3 && text2[2] == '0')
		{
			return true;
		}
		return false;
	}

	private bool CheckUnicodePeramble(string text)
	{
		string value = PdfString.ByteToString(Encoding.Unicode.GetPreamble());
		if (text.Length > 1 && text.Substring(0, 2).Equals(value))
		{
			return true;
		}
		return false;
	}

	private string ProcessEscapes(string text, bool isComplete)
	{
		if (m_isPassword)
		{
			return ProcessEncryptEscapes(text);
		}
		if (!isComplete)
		{
			text = text.Replace("\r", "");
		}
		if (m_isPassword)
		{
			text = text.Replace("\n", "");
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			char c = text[i];
			if (!flag)
			{
				switch (c)
				{
				case '\\':
					flag = true;
					continue;
				case '\0':
					if (!Encrypted && !m_colorSpace && (c != 0 || !m_isPassword))
					{
						break;
					}
					goto default;
				default:
					if (isComplete && char.IsControl(c) && i - 1 >= 0)
					{
						byte[] array = new byte[2]
						{
							32,
							(byte)c
						};
						string @string = Encoding.BigEndianUnicode.GetString(array, 0, array.Length);
						if (text[i - 1] == ' ')
						{
							c = @string.ToCharArray()[0];
							stringBuilder.Remove(stringBuilder.Length - 1, 1);
						}
						else if (i + 1 <= text.Length - 1 && text[i + 1] == ' ')
						{
							c = @string.ToCharArray()[0];
							i++;
						}
					}
					stringBuilder.Append(c);
					continue;
				}
				if (m_certString || (flag4 && c == '\0'))
				{
					stringBuilder.Append(c);
				}
				continue;
			}
			switch (c)
			{
			case 'r':
				stringBuilder.Append('\r');
				break;
			case 'n':
				stringBuilder.Append('\n');
				break;
			case 't':
				stringBuilder.Append('\t');
				break;
			case 'b':
				stringBuilder.Append('\b');
				break;
			case 'f':
				stringBuilder.Append('\f');
				break;
			case '(':
			case ')':
			case '\\':
				stringBuilder.Append(c);
				break;
			default:
				if (c <= '7' && c >= '0')
				{
					c = ProcessOctal(text, ref i);
					if (c == '\n' && !flag2)
					{
						stringBuilder.Append("\n");
					}
					i--;
				}
				if (!flag3 && stringBuilder.Length >= 2)
				{
					flag3 = true;
					flag2 = CheckForPreamble(stringBuilder.ToString());
				}
				if (c >= 'Ā')
				{
					break;
				}
				if (isComplete && (c != 0 || Encrypted || m_colorSpace || (c == '\0' && m_isPassword)))
				{
					if (c == '\r' && i + 1 < length && text[i + 1] == '\n')
					{
						i++;
					}
					else
					{
						stringBuilder.Append(c);
					}
				}
				else if (c != '\n' || flag2)
				{
					stringBuilder.Append(c);
				}
				break;
			}
			if (!flag4 && stringBuilder.Length == 2)
			{
				flag4 = CheckForPreamble(stringBuilder.ToString());
			}
			flag = false;
		}
		return stringBuilder.ToString();
	}

	private char ProcessOctal(string text, ref int i)
	{
		int length = text.Length;
		int num = 0;
		string text2 = string.Empty;
		while (i < length && num < 3)
		{
			char c = text[i];
			if (c <= '7' && c >= '0')
			{
				text2 += c;
			}
			i++;
			num++;
		}
		return (char)Convert.ToInt32(text2, 8);
	}

	private string ProcessEncryptEscapes(string text)
	{
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		bool flag = false;
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			char c = text[i];
			if (!flag && c != '\r' && c != '\n')
			{
				switch (c)
				{
				case '\\':
					flag = true;
					continue;
				case '\0':
					if (!Encrypted && !m_colorSpace && (c != 0 || !m_isPassword))
					{
						continue;
					}
					break;
				}
				stringBuilder.Append(c);
				continue;
			}
			switch (c)
			{
			case 'r':
				stringBuilder.Append('\r');
				break;
			case 'n':
				stringBuilder.Append('\n');
				break;
			case 't':
				stringBuilder.Append('\t');
				break;
			case 'b':
				stringBuilder.Append('\b');
				break;
			case 'f':
				stringBuilder.Append('\f');
				break;
			case '\n':
				if (i - 1 >= 0 && text[i - 1] != '\r')
				{
					stringBuilder.Append(c);
				}
				break;
			case '(':
			case ')':
			case '\\':
				stringBuilder.Append(c);
				break;
			default:
				if (c <= '7' && c >= '0')
				{
					c = ProcessEncryptOctal(text, ref i);
					i--;
				}
				if (c < 'Ā')
				{
					stringBuilder.Append(c);
				}
				break;
			case '\r':
				break;
			}
			flag = false;
		}
		return stringBuilder.ToString();
	}

	private char ProcessEncryptOctal(string text, ref int i)
	{
		int length = text.Length;
		int num = 0;
		int num2 = 0;
		string text2 = string.Empty;
		bool flag = false;
		while (i < length && num < 3)
		{
			char c = text[i];
			if (c == '\\' && m_isPassword)
			{
				flag = true;
				break;
			}
			if (c <= '7' && c >= '0')
			{
				text2 += c;
			}
			i++;
			num++;
		}
		num2 = Convert.ToInt32(text2, 8);
		if (flag && text2.Length == 1)
		{
			num2 = text2[0];
		}
		return (char)num2;
	}

	private IPdfPrimitive HexString()
	{
		Match(m_next, TokenType.HexStringStart);
		Advance();
		StringBuilder stringBuilder = new StringBuilder(100);
		bool flag = true;
		while (m_next != TokenType.HexStringEnd)
		{
			string text = m_lexer.Text;
			if (m_next == TokenType.HexStringWeird)
			{
				flag = false;
			}
			else if (m_next == TokenType.HexStringWeirdEscape)
			{
				flag = false;
				text = text.Substring(1);
			}
			stringBuilder.Append(text);
			Advance();
		}
		Match(m_next, TokenType.HexStringEnd);
		Advance();
		PdfString pdfString = new PdfString(stringBuilder.ToString(), !flag);
		if (m_colorSpace)
		{
			pdfString.IsColorSpace = true;
		}
		if (m_isRGB)
		{
			pdfString.IsRGB = true;
		}
		pdfString.m_isHexString = flag;
		return pdfString;
	}

	private IPdfPrimitive Number()
	{
		PdfNumber pdfNumber;
		if (m_integerQueue.Count > 0)
		{
			pdfNumber = new PdfNumber(m_integerQueue.Dequeue());
		}
		else
		{
			Match(m_next, TokenType.Number);
			pdfNumber = ParseInteger();
		}
		IPdfPrimitive result = pdfNumber;
		if (m_next == TokenType.Number)
		{
			PdfNumber pdfNumber2 = ParseInteger();
			if (m_next == TokenType.Reference)
			{
				result = new PdfReferenceHolder(new PdfReference(pdfNumber.IntValue, pdfNumber2.IntValue), m_crossTable);
				Advance();
			}
			else
			{
				m_integerQueue.Enqueue(pdfNumber2.IntValue);
			}
		}
		return result;
	}

	private PdfNumber ParseInteger()
	{
		double result;
		bool num = double.TryParse(m_lexer.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
		PdfNumber result2 = null;
		if (num)
		{
			result2 = new PdfNumber((long)result);
		}
		else
		{
			Error(ErrorType.BadlyFormedInteger, m_lexer.Text);
		}
		Advance();
		return result2;
	}

	private IPdfPrimitive Real()
	{
		Match(m_next, TokenType.Real);
		double result;
		bool num = double.TryParse(m_lexer.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		PdfNumber result2 = null;
		if (num)
		{
			result2 = new PdfNumber((float)result);
		}
		else
		{
			Error(ErrorType.BadlyFormedReal, m_lexer.Text);
		}
		Advance();
		return result2;
	}

	private IPdfPrimitive Array()
	{
		Match(m_next, TokenType.ArrayStart);
		Advance();
		PdfArray pdfArray = new PdfArray();
		m_lexer.isArray = true;
		IPdfPrimitive element;
		while ((element = Simple()) != null)
		{
			pdfArray.Add(element);
			if (pdfArray[0] is PdfName && (pdfArray[0] as PdfName).ToString() == "/Indexed")
			{
				m_colorSpace = true;
			}
			else
			{
				m_colorSpace = false;
			}
			if (pdfArray != null && pdfArray.Count > 1 && pdfArray[1] != null && pdfArray[1] is PdfName && (pdfArray[1] as PdfName).ToString() == "/DeviceRGB" && pdfArray != null && pdfArray.Count > 2 && pdfArray[2] != null && pdfArray[2] is PdfNumber && (pdfArray[2] as PdfNumber).IntValue == 34)
			{
				m_isRGB = true;
			}
			else
			{
				m_isRGB = false;
			}
			if (m_next == TokenType.Unknown)
			{
				Advance();
			}
		}
		Match(m_next, TokenType.ArrayEnd);
		Advance();
		m_lexer.isArray = false;
		pdfArray.FreezeChanges(this);
		return pdfArray;
	}

	internal IPdfPrimitive Dictionary()
	{
		Match(m_next, TokenType.DictionaryStart);
		Advance();
		PdfDictionary pdfDictionary = new PdfDictionary();
		Pair pair;
		while ((pair = ReadPair()) != Pair.Empty)
		{
			if (pair.Value != null)
			{
				pdfDictionary[pair.Name] = pair.Value;
			}
		}
		if (m_next != TokenType.DictionaryEnd)
		{
			m_next = TokenType.DictionaryEnd;
		}
		Match(m_next, TokenType.DictionaryEnd);
		if (!m_lexer.Skip)
		{
			Advance();
		}
		else
		{
			m_next = TokenType.ObjectEnd;
			m_lexer.Skip = false;
		}
		IPdfPrimitive pdfPrimitive = null;
		pdfPrimitive = ((m_next != TokenType.StreamStart) ? pdfDictionary : ReadStream(pdfDictionary));
		(pdfPrimitive as IPdfChangable).FreezeChanges(this);
		return pdfPrimitive;
	}

	private IPdfPrimitive ReadStream(PdfDictionary dic)
	{
		Match(m_next, TokenType.StreamStart);
		m_lexer.SkipToken();
		if (Encrypted)
		{
			int num;
			while (true)
			{
				num = m_lexer.Read(1)[0];
				switch (num)
				{
				case 0:
				case 9:
				case 12:
				case 32:
					continue;
				default:
					num = m_lexer.Read(1)[0];
					break;
				case 10:
					break;
				}
				break;
			}
			if (num != 10)
			{
				m_lexer.MoveBack();
			}
		}
		else
		{
			m_lexer.SkipNewLine();
		}
		IPdfPrimitive pdfPrimitive = dic["Length"];
		PdfNumber pdfNumber = pdfPrimitive as PdfNumber;
		PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
		if (pdfNumber == null && pdfReferenceHolder == null)
		{
			long position = m_lexer.Position;
			long position2 = m_reader.Position;
			m_reader.Position = position;
			long num2 = m_reader.SearchForward("endstream");
			long num3 = ((num2 <= position) ? (position - num2) : (num2 - position));
			m_reader.Position = position2;
			byte[] data = m_lexer.Read((int)num3);
			PdfStream result = new PdfStream(dic, data);
			Advance();
			if (m_next != TokenType.StreamEnd)
			{
				m_next = TokenType.StreamEnd;
			}
			Match(m_next, TokenType.StreamEnd);
			Advance();
			if (m_next != TokenType.ObjectEnd)
			{
				m_next = TokenType.ObjectEnd;
			}
			return result;
		}
		if (pdfReferenceHolder != null)
		{
			PdfLexer lexer = m_lexer;
			long position3 = m_reader.Position;
			m_lexer = new PdfLexer(m_reader);
			pdfNumber = m_cTable.GetObject(pdfReferenceHolder.Reference) as PdfNumber;
			m_reader.Position = position3;
			m_lexer = lexer;
		}
		int num4 = 0;
		if (pdfNumber != null)
		{
			num4 = pdfNumber.IntValue;
		}
		bool num5 = CheckStreamLength(m_lexer.Position, num4);
		PdfStream pdfStream = null;
		if (num5)
		{
			byte[] data2 = m_lexer.Read(num4);
			pdfStream = new PdfStream(dic, data2);
		}
		else
		{
			long position4 = m_lexer.Position;
			long position5 = m_reader.Position;
			m_reader.Position = position4;
			m_reader.crossTable = m_crossTable;
			long num6 = m_reader.SearchForward("endstream");
			long num7 = ((num6 <= position4) ? (position4 - num6) : (num6 - position4));
			m_reader.Position = position5;
			byte[] data3 = m_lexer.Read((int)num7);
			pdfStream = new PdfStream(dic, data3);
		}
		Advance();
		if (m_next != TokenType.StreamEnd)
		{
			m_next = TokenType.StreamEnd;
		}
		Match(m_next, TokenType.StreamEnd);
		Advance();
		if (m_next != TokenType.ObjectEnd)
		{
			m_next = TokenType.ObjectEnd;
		}
		return pdfStream;
	}

	private bool CheckStreamLength(long lexPosition, int value)
	{
		string text = null;
		bool result = true;
		long position = m_reader.Position;
		m_reader.Position = lexPosition + value;
		char[] array = new char[20];
		m_reader.ReadBlock(array, 0, 20);
		for (int i = 0; i < array.Length; i++)
		{
			text += array[i];
		}
		if (!text.StartsWith("\nendstream") && !text.StartsWith("\r\nendstream") && !text.StartsWith("\rendstream") && !text.StartsWith("endstream"))
		{
			result = false;
		}
		m_reader.Position = position;
		return result;
	}

	private Pair ReadPair()
	{
		IPdfPrimitive pdfPrimitive = null;
		try
		{
			pdfPrimitive = Simple();
		}
		catch
		{
			pdfPrimitive = null;
		}
		if (pdfPrimitive == null || pdfPrimitive is PdfNull)
		{
			return Pair.Empty;
		}
		PdfName pdfName = pdfPrimitive as PdfName;
		if (pdfName == null)
		{
			Error(ErrorType.BadlyFormedDictionary, "next should be a name.");
		}
		if (pdfName == new PdfName("U") || pdfName == new PdfName("O") || pdfName == new PdfName("ID"))
		{
			m_isPassword = true;
		}
		if (pdfPrimitive != null && pdfName != null && pdfName.Value == "Cert")
		{
			m_certString = true;
		}
		pdfPrimitive = Simple();
		m_isPassword = false;
		m_certString = false;
		return new Pair(pdfName, pdfPrimitive);
	}

	private void ProcessUnicodeWithPreamble(ref string text)
	{
		byte[] array = PdfString.StringToByte(text.Substring(2));
		int num = 0;
		string text2 = null;
		bool flag = false;
		for (int i = 0; i < array.Length - 1; i++)
		{
			if (i + 2 <= array.Length - 1 && array[i] == 92 && array[i + 1] == 92 && array[i + 2] == 114)
			{
				MemoryStream memoryStream = new MemoryStream();
				for (int j = 0; j <= i; j++)
				{
					memoryStream.WriteByte(array[j]);
				}
				memoryStream.WriteByte(13);
				for (int k = i + 3; k < array.Length; k++)
				{
					memoryStream.WriteByte(array[k]);
				}
				array = PdfStream.StreamToBytes(memoryStream);
				memoryStream.Dispose();
				i++;
			}
			else if ((array[i] == 92 && (array[i + 1] == 40 || array[i + 1] == 41 || array[i + 1] == 13 || array[i + 1] == 62 || array[i + 1] == 92)) || array[i] == 13)
			{
				for (int l = i; l < array.Length - 1; l++)
				{
					array[l] = array[l + 1];
				}
				byte[] array2 = new byte[array.Length - 1];
				Buffer.BlockCopy(array, 0, array2, 0, array.Length - 1);
				array = array2;
				i--;
			}
			else if (array[i] == 92 && (array[i + 1] == 114 || (i + 2 <= array.Length - 1 && array[i + 1] == 98 && array[i + 2] == 98)))
			{
				MemoryStream memoryStream2 = new MemoryStream();
				for (int m = 0; m < i; m++)
				{
					memoryStream2.WriteByte(array[m]);
				}
				memoryStream2.WriteByte(13);
				for (int n = i + 2; n < array.Length; n++)
				{
					memoryStream2.WriteByte(array[n]);
				}
				array = PdfStream.StreamToBytes(memoryStream2);
				memoryStream2.Dispose();
			}
			else if (array[i] == 92 && array[i + 1] == 110)
			{
				MemoryStream memoryStream3 = new MemoryStream();
				for (int num2 = 0; num2 < i; num2++)
				{
					memoryStream3.WriteByte(array[num2]);
				}
				memoryStream3.WriteByte(10);
				for (int num3 = i + 2; num3 < array.Length; num3++)
				{
					memoryStream3.WriteByte(array[num3]);
				}
				array = PdfStream.StreamToBytes(memoryStream3);
				memoryStream3.Dispose();
			}
		}
		int count = array.Length - num;
		if (flag)
		{
			text = text2;
			text += Encoding.BigEndianUnicode.GetString(array, num, count);
		}
		else
		{
			text = Encoding.BigEndianUnicode.GetString(array, num, count);
		}
	}

	private bool CheckForControlSequence(string text)
	{
		if (text.Length > 1)
		{
			string value = PdfString.ByteToString(new byte[2] { 255, 253 });
			if (text.Substring(text.Length - 2, 2).Equals(value))
			{
				text = text.Substring(0, text.Length - 2);
			}
		}
		byte[] array = PdfString.StringToByte(text);
		Windows1252Encoding windows1252Encoding = new Windows1252Encoding();
		byte[] array2 = new byte[1];
		Dictionary<byte, string> dictionary = new Dictionary<byte, string>();
		for (byte b = 127; b < 164; b++)
		{
			array2[0] = b;
			dictionary.Add(b, windows1252Encoding.GetString(array2, 0, array2.Length));
		}
		for (int i = 0; i < array.Length; i++)
		{
			int num = array[i];
			if (num < 32 && num != 9 && num != 10 && num != 11 && num != 12 && num != 13 && num != 25 && num != 28 && num != 29 && num != 19)
			{
				return true;
			}
			if (num > 127)
			{
				if (!dictionary.ContainsKey(array[i]))
				{
					return true;
				}
				string text2 = dictionary[array[i]];
				char c = (char)array[i];
				if (text2 != c.ToString())
				{
					return true;
				}
			}
		}
		return false;
	}

	private void DecodeEscapeProcess(ref string text, ref bool unicode)
	{
		if (!CheckForPreamble(text) && !CheckUnicodePeramble(text))
		{
			text = ProcessEscapes(text, isComplete: false);
			ProcessUnicodeWithPreamble(ref text, ref unicode);
		}
		else
		{
			ProcessUnicodeWithPreamble(ref text, ref unicode);
		}
	}

	private bool CheckEndByteOrderMark(string text)
	{
		string value = "ÿý";
		if (text.Length > 1 && text.EndsWith(value, StringComparison.Ordinal))
		{
			return true;
		}
		return false;
	}

	private void ProcessUnicodeWithPreamble(ref string text, ref bool unicode)
	{
		if (CheckForPreamble(text))
		{
			do
			{
				text = ProcessBigIndianEscapes(text);
			}
			while (text.Remove(0, 2).StartsWith("\\"));
			if (CheckEndByteOrderMark(text))
			{
				text = text.Remove(text.Length - 3);
			}
			if (text.Length > 1)
			{
				byte[] array = PdfString.StringToByte(text.Substring(2));
				text = Encoding.BigEndianUnicode.GetString(array, 0, array.Length);
				unicode = true;
			}
		}
		else if (CheckUnicodePeramble(text))
		{
			text = text.Remove(0, 2);
			text = ProcessEscapes(text, isComplete: true);
			unicode = true;
		}
	}

	private string ProcessBigIndianEscapes(string text)
	{
		StringBuilder stringBuilder = new StringBuilder();
		MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		StreamReader streamReader = new StreamReader(memoryStream);
		stringBuilder.Length = 0;
		int num = 0;
		while (!streamReader.EndOfStream)
		{
			int num2 = (ushort)streamReader.Read();
			if (num2 == -1)
			{
				break;
			}
			if (num2 == 40)
			{
				num++;
			}
			else if (num2 == 41)
			{
				num--;
			}
			else if (num2 == 92)
			{
				bool flag = false;
				num2 = streamReader.Read();
				switch (num2)
				{
				case 110:
					num2 = 10;
					break;
				case 114:
					num2 = 13;
					break;
				case 116:
					num2 = 9;
					break;
				case 98:
					num2 = 8;
					break;
				case 102:
					num2 = 12;
					break;
				case 13:
					flag = true;
					num2 = streamReader.Read();
					if (num2 != 10 && streamReader.BaseStream.Position > 0)
					{
						streamReader.BaseStream.Seek(-1L, SeekOrigin.Current);
					}
					break;
				case 10:
					flag = true;
					break;
				default:
				{
					if (num2 < 48 || num2 > 55)
					{
						break;
					}
					int num3 = num2 - 48;
					num2 = streamReader.Read();
					if (num2 < 48 || num2 > 55)
					{
						if (streamReader.BaseStream.Position > 0)
						{
							streamReader.BaseStream.Seek(-1L, SeekOrigin.Current);
						}
						num2 = num3;
						break;
					}
					num3 = (num3 << 3) + num2 - 48;
					num2 = (ushort)streamReader.Read();
					if (num2 < 48 || num2 > 55)
					{
						if (streamReader.BaseStream.Position > 0)
						{
							streamReader.BaseStream.Seek(-1L, SeekOrigin.Current);
						}
						num2 = num3;
					}
					else
					{
						num3 = (num3 << 3) + num2 - 48;
						num2 = num3 & 0xFF;
					}
					break;
				}
				case 40:
				case 41:
				case 92:
					break;
				}
				if (flag)
				{
					continue;
				}
				if (num2 < 0)
				{
					break;
				}
			}
			else if (num2 == 13)
			{
				num2 = streamReader.Read();
				if (num2 < 0)
				{
					break;
				}
				if (num2 != 10)
				{
					if (streamReader.BaseStream.Position > 0)
					{
						streamReader.BaseStream.Seek(-1L, SeekOrigin.Current);
					}
					num2 = 10;
				}
			}
			if (num == -1)
			{
				break;
			}
			stringBuilder.Append((char)num2);
		}
		text = stringBuilder.ToString();
		streamReader.Dispose();
		streamReader = null;
		memoryStream.Dispose();
		memoryStream = null;
		return text;
	}
}
