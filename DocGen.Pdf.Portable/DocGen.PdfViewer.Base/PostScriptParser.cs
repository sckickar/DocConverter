using System.Collections.Generic;
using System.IO;

namespace DocGen.PdfViewer.Base;

internal class PostScriptParser : FontFileParser, IPostScriptParser
{
	private readonly EncryptionCollection encryption;

	private readonly Queue<byte> decryptedBuffer;

	public string Result { get; set; }

	public override long Position => base.Position - decryptedBuffer.Count;

	public PostScriptParser(byte[] data)
		: base(data)
	{
		encryption = new EncryptionCollection();
		decryptedBuffer = new Queue<byte>();
	}

	public Token ReadToken()
	{
		SkipUnusedCharacters();
		if (base.EndOfFile)
		{
			return Token.Unknown;
		}
		char c = (char)Peek(0);
		if (c <= '/')
		{
			switch (c)
			{
			case '(':
				return ReadLiteralString();
			case '/':
				return ReadName();
			default:
				Read();
				return Token.Unknown;
			}
		}
		switch (c)
		{
		case '\\':
			if (Chars.IsValidNumberChar(this))
			{
				return ReadNumber();
			}
			if (Chars.IsLetter(Peek(0)) || Peek(0) == 45 || Peek(0) == 124)
			{
				return ReadOperatorOrKeyword();
			}
			Read();
			return Token.Unknown;
		case ']':
			Read();
			return Token.ArrayEnd;
		case '|':
			if (Chars.IsValidNumberChar(this))
			{
				return ReadNumber();
			}
			if (Chars.IsLetter(Peek(0)) || Peek(0) == 45 || Peek(0) == 124)
			{
				return ReadOperatorOrKeyword();
			}
			Read();
			return Token.Unknown;
		case '}':
			Read();
			return Token.ArrayEnd;
		default:
			if (Chars.IsValidNumberChar(this))
			{
				return ReadNumber();
			}
			if (Chars.IsLetter(Peek(0)) || Peek(0) == 45 || Peek(0) == 124)
			{
				return ReadOperatorOrKeyword();
			}
			Read();
			return Token.Unknown;
		case '[':
		case '{':
			Read();
			return Token.ArrayStart;
		case '>':
			return ReadHexadecimalString();
		}
	}

	internal void SkipUnusedCharacters()
	{
		PostScriptParserHelper.SkipUnusedCharacters(this);
	}

	internal void GoToNextLine()
	{
		PostScriptParserHelper.GoToNextLine(this);
	}

	internal void SkipWhiteSpaces()
	{
		PostScriptParserHelper.SkipWhiteSpaces(this);
	}

	private Token ReadOperatorOrKeyword()
	{
		Result = PostScriptParserHelper.ReadKeyword(this);
		switch (Result)
		{
		case "true":
		case "false":
			return Token.Boolean;
		default:
			if (PdfKeywords.IsKeyword(Result))
			{
				return Token.Keyword;
			}
			if (PostScriptOperators.IsOperator(Result))
			{
				return Token.Operator;
			}
			return Token.Unknown;
		}
	}

	private Token ReadName()
	{
		Result = PostScriptParserHelper.ReadName(this);
		return Token.Name;
	}

	private Token ReadNumber()
	{
		Result = PostScriptParserHelper.ReadNumber(this);
		if (!Countable.Contains(Result.ToCharArray(), '.'))
		{
			return Token.Integer;
		}
		return Token.Real;
	}

	private Token ReadHexadecimalString()
	{
		Result = PostScriptParserHelper.GetString(PostScriptParserHelper.ReadHexadecimalString(this));
		return Token.String;
	}

	private Token ReadLiteralString()
	{
		Result = PostScriptParserHelper.GetString(PostScriptParserHelper.ReadLiteralString(this));
		return Token.String;
	}

	public override void BeginReadingBlock()
	{
		base.BeginReadingBlock();
	}

	public override void EndReadingBlock()
	{
		base.EndReadingBlock();
	}

	public override void Seek(long offset, SeekOrigin origin)
	{
		base.Seek(offset, origin);
	}

	public override byte Read()
	{
		if (!encryption.HasEncryption)
		{
			byte b = base.Read();
			if (b == 13)
			{
				if (Peek(0) == 10)
				{
					Read();
				}
				b = 10;
			}
			return b;
		}
		if (decryptedBuffer.Count > 0)
		{
			return decryptedBuffer.Dequeue();
		}
		return encryption.Decrypt(base.Read());
	}

	public override byte Peek(int skip)
	{
		byte b = 0;
		if (!encryption.HasEncryption)
		{
			b = base.Peek(skip);
			if (b == 13)
			{
				b = 10;
			}
			return b;
		}
		if (skip < decryptedBuffer.Count)
		{
			return Countable.ElementAt(decryptedBuffer, skip);
		}
		skip -= decryptedBuffer.Count;
		for (int i = 0; i <= skip; i++)
		{
			b = encryption.Decrypt(base.Read());
			decryptedBuffer.Enqueue(b);
		}
		return b;
	}

	public void PushEncryption(EncryptionStdHelper encrypt)
	{
		encryption.PushEncryption(encrypt);
	}

	public void PopEncryption()
	{
		encryption.PopEncryption();
		if (!encryption.HasEncryption)
		{
			decryptedBuffer.Clear();
		}
	}
}
