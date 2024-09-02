using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.Pdf.IO;

internal class PdfReader : TextReader
{
	private Stream m_stream;

	private string m_delimiters = "()<>[]{}/%";

	private string m_jsonDelimiters = "()<>[]{}/%,\":";

	private int m_peekedByte;

	private bool m_bBytePeeked;

	internal bool m_importFormData;

	internal PdfCrossTable crossTable;

	public long Position
	{
		get
		{
			return m_stream.Position;
		}
		set
		{
			m_stream.Position = value;
		}
	}

	public Stream Stream => m_stream;

	public PdfReader(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_stream = stream;
	}

	protected override void Dispose(bool disposing)
	{
		m_stream = null;
		base.Dispose(disposing);
	}

	public override string ReadLine()
	{
		string text = string.Empty;
		int num = m_stream.ReadByte();
		while (num != -1 && !IsEol((char)num))
		{
			text = text.Insert(text.Length, ((char)num).ToString());
			num = m_stream.ReadByte();
		}
		if (num == 13 && m_stream.ReadByte() != 10)
		{
			m_stream.Position--;
		}
		return text;
	}

	internal void ReadLine(byte[] data, bool skipWhiteSpace)
	{
		int num = -1;
		bool flag = false;
		int num2 = 0;
		int num3 = data.Length;
		if (num2 < num3)
		{
			while (IsWhitespaceCharcater(num = Read(), skipWhiteSpace))
			{
			}
		}
		while (!flag && num2 < num3)
		{
			switch (num)
			{
			case -1:
			case 10:
				flag = true;
				break;
			case 13:
			{
				flag = true;
				long position = m_stream.Position;
				if (Read() != 10)
				{
					m_stream.Seek(position, SeekOrigin.Begin);
				}
				break;
			}
			default:
				data[num2++] = (byte)num;
				break;
			}
			if (flag || num3 <= num2)
			{
				break;
			}
			num = Read();
		}
		if (num2 >= num3)
		{
			flag = false;
			while (!flag)
			{
				switch (num = Read())
				{
				case -1:
				case 10:
					flag = true;
					break;
				case 13:
				{
					flag = true;
					long position2 = m_stream.Position;
					if (Read() != 10)
					{
						m_stream.Seek(position2, SeekOrigin.Begin);
					}
					break;
				}
				}
			}
		}
		if ((num != -1 || num2 != 0) && num2 + 2 <= num3)
		{
			data[num2++] = 32;
			data[num2] = 88;
		}
	}

	internal string ReadContent()
	{
		string text = string.Empty;
		long position = m_stream.Position;
		int num = m_stream.ReadByte();
		if (num != -1 && !IsEol((char)num))
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				m_stream.Position = 0L;
				m_stream.CopyTo(memoryStream);
				memoryStream.Position = position;
				text = Encoding.GetEncoding("ascii").GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length);
			}
			int num2 = (int)position;
			int num3 = text.IndexOf('\n', num2);
			int num4 = text.IndexOf('\r', num2);
			int num5 = ((num3 <= 0 || num4 <= 0) ? Math.Max(num3, num4) : Math.Min(num3, num4));
			if (num5 < 0)
			{
				num5 = (int)m_stream.Length - 1;
			}
			text = text.Substring(num2, num5 - num2);
			m_stream.Position = num5 - 1;
			num = m_stream.ReadByte();
		}
		if (num == 13 && m_stream.ReadByte() != 10)
		{
			m_stream.Position--;
		}
		return text;
	}

	private bool IsWhitespaceCharcater(int character, bool isWhitespace)
	{
		if ((!isWhitespace || character != 0) && character != 9 && character != 10 && character != 12 && character != 13)
		{
			return character == 32;
		}
		return true;
	}

	public override int Read()
	{
		if (m_bBytePeeked)
		{
			GetPeeked(out var byteValue);
			return byteValue;
		}
		return m_stream.ReadByte();
	}

	public override int Peek()
	{
		int byteValue;
		if (m_bBytePeeked)
		{
			GetPeeked(out byteValue);
		}
		else
		{
			m_peekedByte = Read();
			byteValue = m_peekedByte;
		}
		if (m_peekedByte != -1)
		{
			m_bBytePeeked = true;
		}
		return byteValue;
	}

	public override int Read(char[] buffer, int index, int count)
	{
		if (count < 0)
		{
			throw new ArgumentException("The value can't be less then zero", "count");
		}
		int num = index;
		if (m_bBytePeeked && count > 0)
		{
			buffer[num] = (char)m_peekedByte;
			m_bBytePeeked = false;
			count--;
			num++;
		}
		if (count > 0)
		{
			byte[] array = new byte[count];
			count = m_stream.Read(array, 0, count);
			for (int i = 0; i < count; i++)
			{
				char c = (char)array[i];
				buffer[num + i] = c;
			}
			num += count;
		}
		return num - index;
	}

	public override int ReadBlock(char[] buffer, int index, int count)
	{
		return Read(buffer, index, count);
	}

	public override string ReadToEnd()
	{
		string text = string.Empty;
		for (int num = Read(); num != -1; num = m_stream.ReadByte())
		{
			text = text.Insert(text.Length, ((char)num).ToString());
		}
		return text;
	}

	internal string ReadStream()
	{
		return new StreamReader(m_stream).ReadToEnd();
	}

	public bool IsEol(char character)
	{
		if (character != '\n')
		{
			return character == '\r';
		}
		return true;
	}

	public bool IsSeparator(char character)
	{
		if (!char.IsWhiteSpace(character))
		{
			return IsDelimiter(character);
		}
		return true;
	}

	internal bool IsJsonSeparator(char character)
	{
		if (!char.IsWhiteSpace(character))
		{
			return IsJsonDelimiter(character);
		}
		return true;
	}

	public bool IsDelimiter(char character)
	{
		string delimiters = m_delimiters;
		for (int i = 0; i < delimiters.Length; i++)
		{
			if (delimiters[i] == character)
			{
				return true;
			}
		}
		return false;
	}

	internal bool IsJsonDelimiter(char character)
	{
		string jsonDelimiters = m_jsonDelimiters;
		for (int i = 0; i < jsonDelimiters.Length; i++)
		{
			if (jsonDelimiters[i] == character)
			{
				return true;
			}
		}
		return false;
	}

	public long SearchBack(string token)
	{
		long position = Position;
		SkipWSBack();
		if (Position < token.Length)
		{
			return -1L;
		}
		string text = ReadBack(token.Length);
		position = Position - token.Length;
		while (text.CompareTo(token) != 0)
		{
			if (position < 0)
			{
				throw new PdfDocumentException("Invalid/Unknown/Unsupported format\nUnable to find token '" + token + "'");
			}
			Position--;
			if (Position < token.Length)
			{
				return -1L;
			}
			text = ReadBack(token.Length);
			position = Position - token.Length;
		}
		while (token == "xref")
		{
			long num = position;
			if (SearchBack("startxref") == num - 5)
			{
				text = "startxref";
				while (text.CompareTo(token) != 0)
				{
					if (position < 0)
					{
						throw new PdfDocumentException("Invalid/Unknown/Unsupported format\nUnable to find token '" + token + "'");
					}
					Position--;
					if (Position < token.Length)
					{
						return -1L;
					}
					text = ReadBack(token.Length);
					position = Position - token.Length;
				}
				continue;
			}
			position = num;
			break;
		}
		Position = position;
		return position;
	}

	public long SearchForward(string token)
	{
		Encoding uTF = Encoding.UTF8;
		byte[] array = new byte[token.Length];
		string text = "startxref";
		bool flag = false;
		while (true)
		{
			long position = Position;
			int num = Read();
			array[0] = (byte)num;
			if (array[0] == token[0])
			{
				if (!flag)
				{
					position = Position - 1;
					int num2 = m_stream.Read(array, 1, token.Length - 1);
					Position = position;
					if (num2 < token.Length - 1)
					{
						return -1L;
					}
					if (token.CompareTo(uTF.GetString(array, 0, array.Length)) == 0)
					{
						return position;
					}
					Position++;
				}
			}
			else if (array[0] == text[0])
			{
				long position2 = Position;
				flag = false;
				position = Position - 1;
				m_stream.Position = position;
				long num3 = position;
				byte[] array2 = new byte[text.Length];
				m_stream.Read(array2, 0, text.Length);
				if (text.CompareTo(uTF.GetString(array2, 0, array2.Length)) == 0)
				{
					flag = true;
					num3 = (Position = num3 + 1);
				}
				else if (crossTable != null && !crossTable.isTemplateMerging)
				{
					m_stream.Position = position2;
				}
			}
			else if (num == -1)
			{
				break;
			}
		}
		return -1L;
	}

	public string ReadBack(int length)
	{
		Encoding uTF = Encoding.UTF8;
		byte[] array = new byte[length];
		if (Position < length)
		{
			throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
		}
		Position -= length;
		if (m_stream.Read(array, 0, length) < length)
		{
			throw new PdfDocumentException("Read failure.");
		}
		return uTF.GetString(array, 0, array.Length);
	}

	public void SkipWSBack()
	{
		if (Position == 0L)
		{
			throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
		}
		Position--;
		while (char.IsWhiteSpace((char)Read()))
		{
			Position -= 2L;
		}
	}

	public void SkipWS()
	{
		if (Position != m_stream.Length)
		{
			int num;
			do
			{
				num = Read();
			}
			while (char.IsWhiteSpace((char)num));
			if (num == -1)
			{
				Position = m_stream.Length;
			}
			else
			{
				Position--;
			}
		}
	}

	public string GetNextToken()
	{
		string line = string.Empty;
		SkipWS();
		int num = Peek();
		if (IsDelimiter((char)num))
		{
			num = AppendChar(ref line);
			return line;
		}
		while (num != -1 && !IsSeparator((char)num) && line != "\0")
		{
			num = AppendChar(ref line);
			num = Peek();
		}
		return line;
	}

	internal string GetNextJsonToken()
	{
		string line = string.Empty;
		List<byte> list = new List<byte>();
		if (!m_importFormData)
		{
			SkipWS();
		}
		int num = Peek();
		if (IsJsonDelimiter((char)num))
		{
			num = AppendChar(ref line);
			return line;
		}
		char c = '\0';
		while (num != -1 && (!IsJsonDelimiter((char)num) || c == '\\') && line != "\0")
		{
			list.Add((byte)num);
			c = (char)num;
			Read();
			num = Peek();
		}
		line = string.Empty;
		if (list.Count > 0)
		{
			line = Encoding.UTF8.GetString(list.ToArray(), 0, list.Count);
		}
		list.Clear();
		return line;
	}

	public long Seek(long offset, SeekOrigin origin)
	{
		return m_stream.Seek(offset, origin);
	}

	private int AppendChar(ref string line)
	{
		int num = Read();
		if (num != -1)
		{
			line = line.Insert(line.Length, ((char)num).ToString());
		}
		return num;
	}

	private bool GetPeeked(out int byteValue)
	{
		bool bBytePeeked = m_bBytePeeked;
		if (m_bBytePeeked)
		{
			m_bBytePeeked = false;
			byteValue = m_peekedByte;
			return bBytePeeked;
		}
		byteValue = 0;
		return bBytePeeked;
	}
}
