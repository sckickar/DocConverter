using System;
using System.Collections;
using System.IO;
using System.Text;

internal class SupportClass
{
	internal class Tokenizer : IEnumerator
	{
		private long currentPos;

		private bool includeDelims;

		private char[] chars;

		private string delimiters = " \t\n\r\f";

		public int Count
		{
			get
			{
				long num = currentPos;
				int num2 = 0;
				try
				{
					while (true)
					{
						NextToken();
						num2++;
					}
				}
				catch (ArgumentOutOfRangeException)
				{
					currentPos = num;
					return num2;
				}
			}
		}

		public object Current => NextToken();

		public Tokenizer(string source)
		{
			chars = source.ToCharArray();
		}

		public string NextToken()
		{
			return NextToken(delimiters);
		}

		public string NextToken(string delimiters)
		{
			this.delimiters = delimiters;
			if (currentPos == chars.Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (Array.IndexOf(delimiters.ToCharArray(), chars[currentPos]) != -1 && includeDelims)
			{
				return chars[currentPos++].ToString() ?? "";
			}
			return nextToken(delimiters.ToCharArray());
		}

		private string nextToken(char[] delimiters)
		{
			string text = "";
			long num = currentPos;
			while (Array.IndexOf(delimiters, chars[currentPos]) != -1)
			{
				if (++currentPos == chars.Length)
				{
					currentPos = num;
					throw new ArgumentOutOfRangeException();
				}
			}
			while (Array.IndexOf(delimiters, chars[currentPos]) == -1)
			{
				text += chars[currentPos];
				if (++currentPos == chars.Length)
				{
					break;
				}
			}
			return text;
		}

		public bool HasMoreTokens()
		{
			long num = currentPos;
			try
			{
				NextToken();
			}
			catch (ArgumentOutOfRangeException)
			{
				return false;
			}
			finally
			{
				currentPos = num;
			}
			return true;
		}

		public bool MoveNext()
		{
			return HasMoreTokens();
		}

		public void Reset()
		{
		}
	}

	private class BackStringReader : StringReader
	{
		private char[] buffer;

		private int position = 1;

		public BackStringReader(string s)
			: base(s)
		{
			buffer = new char[position];
		}

		public override int Read()
		{
			if (position >= 0 && position < buffer.Length)
			{
				return buffer[position++];
			}
			return base.Read();
		}

		public override int Read(char[] array, int index, int count)
		{
			int num = buffer.Length - position;
			if (count <= 0)
			{
				return 0;
			}
			if (num > 0)
			{
				if (count < num)
				{
					num = count;
				}
				Array.Copy(buffer, position, array, index, num);
				count -= num;
				index += num;
				position += num;
			}
			if (count > 0)
			{
				count = base.Read(array, index, count);
				if (count == -1)
				{
					if (num == 0)
					{
						return -1;
					}
					return num;
				}
				return num + count;
			}
			return num;
		}

		public void UnRead(int unReadChar)
		{
			position--;
			buffer[position] = (char)unReadChar;
		}

		public void UnRead(char[] array, int index, int count)
		{
			Move(array, index, count);
		}

		public void UnRead(char[] array)
		{
			Move(array, 0, array.Length - 1);
		}

		private void Move(char[] array, int index, int count)
		{
			for (int num = index + count; num >= index; num--)
			{
				UnRead(array[num]);
			}
		}
	}

	internal class StreamTokenizerSupport
	{
		private const string TOKEN = "Token[";

		private const string NOTHING = "NOTHING";

		private const string NUMBER = "number=";

		private const string EOF = "EOF";

		private const string EOL = "EOL";

		private const string QUOTED = "quoted string=";

		private const string LINE = "], Line ";

		private const string DASH = "-.";

		private const string DOT = ".";

		private const int TT_NOTHING = -4;

		private const sbyte ORDINARYCHAR = 0;

		private const sbyte WORDCHAR = 1;

		private const sbyte WHITESPACECHAR = 2;

		private const sbyte COMMENTCHAR = 4;

		private const sbyte QUOTECHAR = 8;

		private const sbyte NUMBERCHAR = 16;

		private const int STATE_NEUTRAL = 0;

		private const int STATE_WORD = 1;

		private const int STATE_NUMBER1 = 2;

		private const int STATE_NUMBER2 = 3;

		private const int STATE_NUMBER3 = 4;

		private const int STATE_NUMBER4 = 5;

		private const int STATE_STRING = 6;

		private const int STATE_LINECOMMENT = 7;

		private const int STATE_DONE_ON_EOL = 8;

		private const int STATE_PROCEED_ON_EOL = 9;

		private const int STATE_POSSIBLEC_COMMENT = 10;

		private const int STATE_POSSIBLEC_COMMENT_END = 11;

		private const int STATE_C_COMMENT = 12;

		private const int STATE_STRING_ESCAPE_SEQ = 13;

		private const int STATE_STRING_ESCAPE_SEQ_OCTAL = 14;

		private const int STATE_DONE = 100;

		private sbyte[] attribute = new sbyte[256];

		private bool eolIsSignificant;

		private bool slashStarComments;

		private bool slashSlashComments;

		private bool lowerCaseMode;

		private bool pushedback;

		private int lineno = 1;

		private BackReader inReader;

		private BackStringReader inStringReader;

		private BackInputStream inStream;

		private StringBuilder buf;

		public const int TT_EOF = -1;

		public const int TT_EOL = 10;

		public const int TT_NUMBER = -2;

		public const int TT_WORD = -3;

		public double nval;

		public string sval;

		public int ttype;

		private int read()
		{
			if (inReader != null)
			{
				return inReader.Read();
			}
			if (inStream != null)
			{
				return inStream.Read();
			}
			return inStringReader.Read();
		}

		private void unread(int ch)
		{
			if (inReader != null)
			{
				inReader.UnRead(ch);
			}
			else if (inStream != null)
			{
				inStream.UnRead(ch);
			}
			else
			{
				inStringReader.UnRead(ch);
			}
		}

		private void init()
		{
			buf = new StringBuilder();
			ttype = -4;
			WordChars(65, 90);
			WordChars(97, 122);
			WordChars(160, 255);
			WhitespaceChars(0, 32);
			CommentChar(47);
			QuoteChar(39);
			QuoteChar(34);
			ParseNumbers();
		}

		private void setAttributes(int low, int hi, sbyte attrib)
		{
			int num = Math.Max(0, low);
			int num2 = Math.Min(255, hi);
			for (int i = num; i <= num2; i++)
			{
				attribute[i] = attrib;
			}
		}

		private bool isWordChar(int data)
		{
			char c = (char)data;
			if (data != -1)
			{
				if (c <= 'ÿ' && attribute[(uint)c] != 1)
				{
					return attribute[(uint)c] == 16;
				}
				return true;
			}
			return false;
		}

		public StreamTokenizerSupport(StringReader reader)
		{
			string text = "";
			for (int num = reader.Read(); num != -1; num = reader.Read())
			{
				text += (char)num;
			}
			inStringReader = new BackStringReader(text);
			init();
		}

		public StreamTokenizerSupport(StreamReader reader)
		{
			inReader = new BackReader(new StreamReader(reader.BaseStream, reader.CurrentEncoding).BaseStream, 2, reader.CurrentEncoding);
			init();
		}

		public StreamTokenizerSupport(Stream stream)
		{
			inStream = new BackInputStream(stream, 2);
			init();
		}

		public virtual void CommentChar(int ch)
		{
			if (ch >= 0 && ch <= 255)
			{
				attribute[ch] = 4;
			}
		}

		public virtual void EOLIsSignificant(bool flag)
		{
			eolIsSignificant = flag;
		}

		public virtual int Lineno()
		{
			return lineno;
		}

		public virtual void LowerCaseMode(bool flag)
		{
			lowerCaseMode = flag;
		}

		public virtual int NextToken()
		{
			char c = '\0';
			char c2 = '\0';
			char c3 = '\0';
			int num = 0;
			if (pushedback)
			{
				pushedback = false;
				return ttype;
			}
			ttype = -4;
			int num2 = 0;
			nval = 0.0;
			sval = null;
			buf.Length = 0;
			do
			{
				int num3 = read();
				c = c2;
				c2 = (char)num3;
				switch (num2)
				{
				case 0:
					if (num3 == -1)
					{
						ttype = -1;
						num2 = 100;
					}
					else if (c2 > 'ÿ')
					{
						buf.Append(c2);
						ttype = -3;
						num2 = 1;
					}
					else if (attribute[(uint)c2] == 4)
					{
						num2 = 7;
					}
					else if (attribute[(uint)c2] == 1)
					{
						buf.Append(c2);
						ttype = -3;
						num2 = 1;
					}
					else if (attribute[(uint)c2] == 16)
					{
						ttype = -2;
						buf.Append(c2);
						num2 = c2 switch
						{
							'-' => 2, 
							'.' => 4, 
							_ => 3, 
						};
					}
					else if (attribute[(uint)c2] == 8)
					{
						c3 = c2;
						ttype = c2;
						num2 = 6;
					}
					else if ((slashSlashComments || slashStarComments) && c2 == '/')
					{
						num2 = 10;
					}
					else if (attribute[(uint)c2] == 0)
					{
						ttype = c2;
						num2 = 100;
					}
					else
					{
						if (c2 != '\n' && c2 != '\r')
						{
							break;
						}
						lineno++;
						if (eolIsSignificant)
						{
							ttype = 10;
							switch (c2)
							{
							case '\n':
								num2 = 100;
								break;
							case '\r':
								num2 = 8;
								break;
							}
						}
						else if (c2 == '\r')
						{
							num2 = 9;
						}
					}
					break;
				case 1:
					if (isWordChar(num3))
					{
						buf.Append(c2);
						break;
					}
					if (num3 != -1)
					{
						unread(c2);
					}
					sval = buf.ToString();
					num2 = 100;
					break;
				case 2:
					if (num3 == -1 || attribute[(uint)c2] != 16 || c2 == '-')
					{
						if (attribute[(uint)c2] == 4 && char.IsNumber(c2))
						{
							buf.Append(c2);
							num2 = 3;
							break;
						}
						if (num3 != -1)
						{
							unread(c2);
						}
						ttype = 45;
						num2 = 100;
					}
					else
					{
						buf.Append(c2);
						num2 = ((c2 != '.') ? 3 : 4);
					}
					break;
				case 3:
					if (num3 == -1 || attribute[(uint)c2] != 16 || c2 == '-')
					{
						if (char.IsNumber(c2) && attribute[(uint)c2] == 1)
						{
							buf.Append(c2);
							break;
						}
						if (c2 == '.' && attribute[(uint)c2] == 2)
						{
							buf.Append(c2);
							break;
						}
						if (num3 != -1 && attribute[(uint)c2] == 4 && char.IsNumber(c2))
						{
							buf.Append(c2);
							break;
						}
						if (num3 != -1)
						{
							unread(c2);
						}
						try
						{
							nval = double.Parse(buf.ToString());
						}
						catch (FormatException)
						{
						}
						num2 = 100;
					}
					else
					{
						buf.Append(c2);
						if (c2 == '.')
						{
							num2 = 4;
						}
					}
					break;
				case 4:
					if (num3 == -1 || attribute[(uint)c2] != 16 || c2 == '-' || c2 == '.')
					{
						if (attribute[(uint)c2] == 4 && char.IsNumber(c2))
						{
							buf.Append(c2);
							break;
						}
						if (num3 != -1)
						{
							unread(c2);
						}
						string text = buf.ToString();
						if (text.Equals("-."))
						{
							unread(46);
							ttype = 45;
						}
						else if (text.Equals(".") && 1 == attribute[(uint)c])
						{
							ttype = 46;
						}
						else
						{
							try
							{
								nval = double.Parse(text);
							}
							catch (FormatException)
							{
							}
						}
						num2 = 100;
					}
					else
					{
						buf.Append(c2);
						num2 = 5;
					}
					break;
				case 5:
					if (num3 == -1 || attribute[(uint)c2] != 16 || c2 == '-' || c2 == '.')
					{
						if (num3 != -1)
						{
							unread(c2);
						}
						try
						{
							nval = double.Parse(buf.ToString());
						}
						catch (FormatException)
						{
						}
						num2 = 100;
					}
					else
					{
						buf.Append(c2);
					}
					break;
				case 7:
					if (num3 == -1)
					{
						ttype = -1;
						num2 = 100;
					}
					else if (c2 == '\n' || c2 == '\r')
					{
						unread(c2);
						num2 = 0;
					}
					break;
				case 8:
					if (c2 != '\n' && num3 != -1)
					{
						unread(c2);
					}
					num2 = 100;
					break;
				case 9:
					if (c2 != '\n' && num3 != -1)
					{
						unread(c2);
					}
					num2 = 0;
					break;
				case 6:
					if (num3 != -1 && c2 != c3)
					{
						switch (c2)
						{
						case '\n':
						case '\r':
							break;
						case '\\':
							num2 = 13;
							goto end_IL_005b;
						default:
							buf.Append(c2);
							goto end_IL_005b;
						}
					}
					sval = buf.ToString();
					if (c2 == '\r' || c2 == '\n')
					{
						unread(c2);
					}
					num2 = 100;
					break;
				case 13:
					if (num3 == -1)
					{
						sval = buf.ToString();
						num2 = 100;
						break;
					}
					num2 = 6;
					switch (c2)
					{
					case 'a':
						buf.Append(7);
						break;
					case 'b':
						buf.Append('\b');
						break;
					case 'f':
						buf.Append(12);
						break;
					case 'n':
						buf.Append('\n');
						break;
					case 'r':
						buf.Append('\r');
						break;
					case 't':
						buf.Append('\t');
						break;
					case 'v':
						buf.Append(11);
						break;
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
						num = c2 - 48;
						num2 = 14;
						break;
					default:
						buf.Append(c2);
						break;
					}
					break;
				case 14:
					if (num3 == -1 || c2 < '0' || c2 > '7')
					{
						buf.Append((char)num);
						if (num3 == -1)
						{
							sval = buf.ToString();
							num2 = 100;
						}
						else
						{
							unread(c2);
							num2 = 6;
						}
					}
					else
					{
						int num4 = num * 8 + (c2 - 48);
						if (num4 < 256)
						{
							num = num4;
							break;
						}
						buf.Append((char)num);
						buf.Append(c2);
						num2 = 6;
					}
					break;
				case 10:
					switch (c2)
					{
					case '*':
						num2 = 12;
						break;
					case '/':
						num2 = 7;
						break;
					default:
						if (num3 != -1)
						{
							unread(c2);
						}
						ttype = 47;
						num2 = 100;
						break;
					}
					break;
				case 12:
					if (c2 == '*')
					{
						num2 = 11;
					}
					if (c2 == '\n')
					{
						lineno++;
					}
					else if (num3 == -1)
					{
						ttype = -1;
						num2 = 100;
					}
					break;
				case 11:
					{
						if (num3 == -1)
						{
							ttype = -1;
							num2 = 100;
							break;
						}
						switch (c2)
						{
						case '/':
							num2 = 0;
							break;
						default:
							num2 = 12;
							break;
						case '*':
							break;
						}
						break;
					}
					end_IL_005b:
					break;
				}
			}
			while (num2 != 100);
			if (ttype == -3 && lowerCaseMode)
			{
				sval = sval.ToLower();
			}
			return ttype;
		}

		public virtual void OrdinaryChar(int ch)
		{
			if (ch >= 0 && ch <= 255)
			{
				attribute[ch] = 0;
			}
		}

		public virtual void OrdinaryChars(int low, int hi)
		{
			setAttributes(low, hi, 0);
		}

		public virtual void ParseNumbers()
		{
			for (int i = 48; i <= 57; i++)
			{
				attribute[i] = 16;
			}
			attribute[46] = 16;
			attribute[45] = 16;
		}

		public virtual void PushBack()
		{
			if (ttype != -4)
			{
				pushedback = true;
			}
		}

		public virtual void QuoteChar(int ch)
		{
			if (ch >= 0 && ch <= 255)
			{
				attribute[ch] = 8;
			}
		}

		public virtual void ResetSyntax()
		{
			OrdinaryChars(0, 255);
		}

		public virtual void SlashSlashComments(bool flag)
		{
			slashSlashComments = flag;
		}

		public virtual void SlashStarComments(bool flag)
		{
			slashStarComments = flag;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder("Token[");
			switch (ttype)
			{
			case -4:
				stringBuilder.Append("NOTHING");
				break;
			case -3:
				stringBuilder.Append(sval);
				break;
			case -2:
				stringBuilder.Append("number=");
				stringBuilder.Append(nval);
				break;
			case -1:
				stringBuilder.Append("EOF");
				break;
			case 10:
				stringBuilder.Append("EOL");
				break;
			}
			if (ttype > 0)
			{
				if (attribute[ttype] == 8)
				{
					stringBuilder.Append("quoted string=");
					stringBuilder.Append(sval);
				}
				else
				{
					stringBuilder.Append('\'');
					stringBuilder.Append((char)ttype);
					stringBuilder.Append('\'');
				}
			}
			stringBuilder.Append("], Line ");
			stringBuilder.Append(lineno);
			return stringBuilder.ToString();
		}

		public virtual void WhitespaceChars(int low, int hi)
		{
			setAttributes(low, hi, 2);
		}

		public virtual void WordChars(int low, int hi)
		{
			setAttributes(low, hi, 1);
		}
	}

	internal class BackReader : StreamReader
	{
		private char[] buffer;

		private int position = 1;

		public BackReader(Stream streamReader, int size, Encoding encoding)
			: base(streamReader, encoding)
		{
			buffer = new char[size];
			position = size;
		}

		public BackReader(Stream streamReader, Encoding encoding)
			: base(streamReader, encoding)
		{
			buffer = new char[position];
		}

		public bool MarkSupported()
		{
			return false;
		}

		public void Mark(int position)
		{
			throw new IOException("Mark operations are not allowed");
		}

		public void Reset()
		{
			throw new IOException("Mark operations are not allowed");
		}

		public override int Read()
		{
			if (position >= 0 && position < buffer.Length)
			{
				return buffer[position++];
			}
			return base.Read();
		}

		public override int Read(char[] array, int index, int count)
		{
			int num = buffer.Length - position;
			if (count <= 0)
			{
				return 0;
			}
			if (num > 0)
			{
				if (count < num)
				{
					num = count;
				}
				Array.Copy(buffer, position, array, index, num);
				count -= num;
				index += num;
				position += num;
			}
			if (count > 0)
			{
				count = base.Read(array, index, count);
				if (count == -1)
				{
					if (num == 0)
					{
						return -1;
					}
					return num;
				}
				return num + count;
			}
			return num;
		}

		public bool IsReady()
		{
			if (position < buffer.Length)
			{
				return BaseStream.Position >= BaseStream.Length;
			}
			return true;
		}

		public void UnRead(int unReadChar)
		{
			position--;
			buffer[position] = (char)unReadChar;
		}

		public void UnRead(char[] array, int index, int count)
		{
			Move(array, index, count);
		}

		public void UnRead(char[] array)
		{
			Move(array, 0, array.Length - 1);
		}

		private void Move(char[] array, int index, int count)
		{
			for (int num = index + count; num >= index; num--)
			{
				UnRead(array[num]);
			}
		}
	}

	internal class BackInputStream : BinaryReader
	{
		private byte[] buffer;

		private int position = 1;

		public BackInputStream(Stream streamReader, int size)
			: base(streamReader)
		{
			buffer = new byte[size];
			position = size;
		}

		public BackInputStream(Stream streamReader)
			: base(streamReader)
		{
			buffer = new byte[position];
		}

		public bool MarkSupported()
		{
			return false;
		}

		public override int Read()
		{
			if (position >= 0 && position < buffer.Length)
			{
				return buffer[position++];
			}
			return base.Read();
		}

		public virtual int Read(sbyte[] array, int index, int count)
		{
			int num = 0;
			int num2 = count + index;
			byte[] array2 = ToByteArray(array);
			num = 0;
			while (position < buffer.Length && index < num2)
			{
				array2[index++] = buffer[position++];
				num++;
			}
			if (index < num2)
			{
				num += base.Read(array2, index, num2 - index);
			}
			for (int i = 0; i < array2.Length; i++)
			{
				array[i] = (sbyte)array2[i];
			}
			return num;
		}

		public void UnRead(int element)
		{
			position--;
			if (position >= 0)
			{
				buffer[position] = (byte)element;
			}
		}

		public void UnRead(byte[] array, int index, int count)
		{
			Move(array, index, count);
		}

		public void UnRead(byte[] array)
		{
			Move(array, 0, array.Length - 1);
		}

		public long Skip(long numberOfBytes)
		{
			return BaseStream.Seek(numberOfBytes, SeekOrigin.Current) - BaseStream.Position;
		}

		private void Move(byte[] array, int index, int count)
		{
			for (int num = index + count; num >= index; num--)
			{
				UnRead(array[num]);
			}
		}
	}

	public static byte[] ToByteArray(sbyte[] sbyteArray)
	{
		byte[] array = null;
		if (sbyteArray != null)
		{
			array = new byte[sbyteArray.Length];
			for (int i = 0; i < sbyteArray.Length; i++)
			{
				array[i] = (byte)sbyteArray[i];
			}
		}
		return array;
	}

	public static byte[] ToByteArray(string sourceString)
	{
		return Encoding.UTF8.GetBytes(sourceString);
	}

	public static byte[] ToByteArray(object[] tempObjectArray)
	{
		byte[] array = null;
		if (tempObjectArray != null)
		{
			array = new byte[tempObjectArray.Length];
			for (int i = 0; i < tempObjectArray.Length; i++)
			{
				array[i] = (byte)tempObjectArray[i];
			}
		}
		return array;
	}

	public static void WriteStackTrace(Exception throwable, TextWriter stream)
	{
		stream.Write(throwable.StackTrace);
		stream.Flush();
	}

	public static sbyte[] ToSByteArray(byte[] byteArray)
	{
		sbyte[] array = null;
		if (byteArray != null)
		{
			array = new sbyte[byteArray.Length];
			for (int i = 0; i < byteArray.Length; i++)
			{
				array[i] = (sbyte)byteArray[i];
			}
		}
		return array;
	}

	public static char[] ToCharArray(sbyte[] sByteArray)
	{
		return Encoding.UTF8.GetChars(ToByteArray(sByteArray));
	}

	public static char[] ToCharArray(byte[] byteArray)
	{
		return Encoding.UTF8.GetChars(byteArray);
	}

	public static long Identity(long literal)
	{
		return literal;
	}

	public static ulong Identity(ulong literal)
	{
		return literal;
	}

	public static float Identity(float literal)
	{
		return literal;
	}

	public static double Identity(double literal)
	{
		return literal;
	}

	public static int URShift(int number, int bits)
	{
		if (number >= 0)
		{
			return number >> bits;
		}
		return (number >> bits) + (2 << ~bits);
	}

	public static int URShift(int number, long bits)
	{
		return URShift(number, (int)bits);
	}

	public static long URShift(long number, int bits)
	{
		if (number >= 0)
		{
			return number >> bits;
		}
		return (number >> bits) + (2L << ~bits);
	}

	public static long URShift(long number, long bits)
	{
		return URShift(number, (int)bits);
	}

	public static int ReadInput(Stream sourceStream, sbyte[] target, int start, int count)
	{
		if (target.Length == 0)
		{
			return 0;
		}
		byte[] array = new byte[target.Length];
		int num = sourceStream.Read(array, start, count);
		if (num == 0)
		{
			return -1;
		}
		for (int i = start; i < start + num; i++)
		{
			target[i] = (sbyte)array[i];
		}
		return num;
	}

	public static int ReadInput(TextReader sourceTextReader, sbyte[] target, int start, int count)
	{
		if (target.Length == 0)
		{
			return 0;
		}
		char[] array = new char[target.Length];
		int num = sourceTextReader.Read(array, start, count);
		if (num == 0)
		{
			return -1;
		}
		for (int i = start; i < start + num; i++)
		{
			target[i] = (sbyte)array[i];
		}
		return num;
	}
}
