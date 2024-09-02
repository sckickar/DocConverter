using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Esprima.Ast;

namespace Esprima;

public class Scanner
{
	private readonly IErrorHandler _errorHandler;

	private readonly bool _trackComment;

	private readonly bool _adaptRegexp;

	private readonly int _length;

	public readonly string Source;

	public int Index;

	public int LineNumber;

	public int LineStart;

	internal bool IsModule;

	private Stack<string> _curlyStack;

	private readonly StringBuilder strb = new StringBuilder();

	private static readonly HashSet<string> Keywords = new HashSet<string>
	{
		"if", "in", "do", "var", "for", "new", "try", "let", "this", "else",
		"case", "void", "with", "enum", "while", "break", "catch", "throw", "const", "yield",
		"class", "super", "return", "typeof", "delete", "switch", "export", "import", "default", "finally",
		"extends", "function", "continue", "debugger", "instanceof"
	};

	private static readonly HashSet<string> StrictModeReservedWords = new HashSet<string> { "implements", "interface", "package", "private", "protected", "public", "static", "yield", "let" };

	private static readonly HashSet<string> FutureReservedWords = new HashSet<string> { "enum", "export", "import", "super" };

	private static readonly string[] threeCharacterPunctutors = new string[6] { "===", "!==", ">>>", "<<=", ">>=", "**=" };

	private static readonly string[] twoCharacterPunctuators = new string[20]
	{
		"&&", "||", "==", "!=", "+=", "-=", "*=", "/=", "++", "--",
		"<<", ">>", "&=", "|=", "^=", "%=", "<=", ">=", "=>", "**"
	};

	private static int HexValue(char ch)
	{
		if (ch >= 'A')
		{
			if (ch >= 'a')
			{
				if (ch <= 'h')
				{
					return ch - 97 + 10;
				}
			}
			else if (ch <= 'H')
			{
				return ch - 65 + 10;
			}
		}
		else if (ch <= '9')
		{
			return ch - 48;
		}
		return 0;
	}

	private static int OctalValue(char ch)
	{
		return ch - 48;
	}

	public Scanner(string code)
		: this(code, new ParserOptions())
	{
	}

	public Scanner(string code, ParserOptions options)
	{
		Source = code;
		_adaptRegexp = options.AdaptRegexp;
		_errorHandler = options.ErrorHandler;
		_trackComment = options.Comment;
		_length = code.Length;
		Index = 0;
		LineNumber = ((code.Length > 0) ? 1 : 0);
		LineStart = 0;
		_curlyStack = new Stack<string>(20);
	}

	internal ScannerState SaveState()
	{
		return new ScannerState(Index, LineNumber, LineStart, new Stack<string>(_curlyStack));
	}

	internal void RestoreState(in ScannerState state)
	{
		Index = state.Index;
		LineNumber = state.LineNumber;
		LineStart = state.LineStart;
		_curlyStack = state.CurlyStack;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Eof()
	{
		return Index >= _length;
	}

	public void ThrowUnexpectedToken(string message = "Unexpected token ILLEGAL")
	{
		_errorHandler.ThrowError(Index, LineNumber, Index - LineStart + 1, message);
	}

	public void TolerateUnexpectedToken(string message = "Unexpected token ILLEGAL")
	{
		_errorHandler.TolerateError(Index, LineNumber, Index - LineStart + 1, message);
	}

	private StringBuilder GetStringBuilder()
	{
		strb.Clear();
		return strb;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsFutureReservedWord(string id)
	{
		return FutureReservedWords.Contains(id);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsStrictModeReservedWord(string id)
	{
		return StrictModeReservedWords.Contains(id);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRestrictedWord(string id)
	{
		if (!"eval".Equals(id))
		{
			return "arguments".Equals(id);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKeyword(string id)
	{
		return Keywords.Contains(id);
	}

	private ArrayList<Comment> SkipSingleLineComment(int offset)
	{
		ArrayList<Comment> result = default(ArrayList<Comment>);
		int num = 0;
		Loc loc = new Loc();
		if (_trackComment)
		{
			num = Index - offset;
			loc.Start = new Marker(0, LineNumber, Index - LineStart - offset);
			loc.End = new Marker();
		}
		while (!Eof())
		{
			char c = Source.CharCodeAt(Index);
			Index++;
			if (Character.IsLineTerminator(c))
			{
				if (_trackComment)
				{
					loc.End = new Marker(loc.End.Index, LineNumber, Index - LineStart - 1);
					Comment comment = default(Comment);
					comment.MultiLine = false;
					comment.Slice = new int[2]
					{
						num + offset,
						Index - 1
					};
					comment.Start = num;
					comment.End = Index - 1;
					comment.Loc = loc;
					Comment item = comment;
					result.Add(item);
				}
				if (c == '\r' && Source.CharCodeAt(Index) == '\n')
				{
					Index++;
				}
				LineNumber++;
				LineStart = Index;
				return result;
			}
		}
		if (_trackComment)
		{
			loc.End = new Marker(loc.End.Index, LineNumber, Index - LineStart);
			Comment comment = default(Comment);
			comment.MultiLine = false;
			comment.Slice = new int[2]
			{
				num + offset,
				Index
			};
			comment.Start = num;
			comment.End = Index;
			comment.Loc = loc;
			Comment item2 = comment;
			result.Add(item2);
		}
		return result;
	}

	private ArrayList<Comment> SkipMultiLineComment()
	{
		ArrayList<Comment> result = default(ArrayList<Comment>);
		int num = 0;
		Loc loc = new Loc();
		if (_trackComment)
		{
			num = Index - 2;
			loc.Start = new Marker(loc.Start.Index, LineNumber, Index - LineStart - 2);
		}
		while (!Eof())
		{
			char c = Source.CharCodeAt(Index);
			if (Character.IsLineTerminator(c))
			{
				if (c == '\r' && Source.CharCodeAt(Index + 1) == '\n')
				{
					Index++;
				}
				LineNumber++;
				Index++;
				LineStart = Index;
			}
			else if (c == '*')
			{
				if (Source.CharCodeAt(Index + 1) == '/')
				{
					Index += 2;
					if (_trackComment)
					{
						loc.End = new Marker(loc.End.Index, LineNumber, Index - LineStart);
						Comment comment = default(Comment);
						comment.MultiLine = true;
						comment.Slice = new int[2]
						{
							num + 2,
							Index - 2
						};
						comment.Start = num;
						comment.End = Index;
						comment.Loc = loc;
						Comment item = comment;
						result.Add(item);
					}
					return result;
				}
				Index++;
			}
			else
			{
				Index++;
			}
		}
		if (_trackComment)
		{
			loc.End = new Marker(loc.End.Index, LineNumber, Index - LineStart);
			Comment comment = default(Comment);
			comment.MultiLine = true;
			comment.Slice = new int[2]
			{
				num + 2,
				Index
			};
			comment.Start = num;
			comment.End = Index;
			comment.Loc = loc;
			Comment item2 = comment;
			result.Add(item2);
		}
		TolerateUnexpectedToken();
		return result;
	}

	public IReadOnlyList<Comment> ScanComments()
	{
		return ScanCommentsInternal();
	}

	internal ArrayList<Comment> ScanCommentsInternal()
	{
		ArrayList<Comment> result = default(ArrayList<Comment>);
		bool flag = Index == 0;
		while (!Eof())
		{
			char c = Source.CharCodeAt(Index);
			if (Character.IsWhiteSpace(c))
			{
				Index++;
				continue;
			}
			if (Character.IsLineTerminator(c))
			{
				Index++;
				if (c == '\r' && Source.CharCodeAt(Index) == '\n')
				{
					Index++;
				}
				LineNumber++;
				LineStart = Index;
				flag = true;
				continue;
			}
			if (c == '/')
			{
				switch (Source.CharCodeAt(Index + 1))
				{
				case '/':
				{
					Index += 2;
					ArrayList<Comment> list2 = SkipSingleLineComment(2);
					if (_trackComment)
					{
						result.AddRange(list2);
					}
					flag = true;
					continue;
				}
				case '*':
				{
					Index += 2;
					ArrayList<Comment> list = SkipMultiLineComment();
					if (_trackComment)
					{
						result.AddRange(list);
					}
					continue;
				}
				}
				break;
			}
			if (flag && c == '-')
			{
				if (Source.CharCodeAt(Index + 1) != '-' || Source.CharCodeAt(Index + 2) != '>')
				{
					break;
				}
				Index += 3;
				ArrayList<Comment> list3 = SkipSingleLineComment(3);
				if (_trackComment)
				{
					result.AddRange(list3);
				}
			}
			else
			{
				if (c != '<' || IsModule || Source[Index + 1] != '!' || Source[Index + 2] != '-' || Source[Index + 3] != '-')
				{
					break;
				}
				Index += 4;
				ArrayList<Comment> list4 = SkipSingleLineComment(4);
				if (_trackComment)
				{
					result.AddRange(list4);
				}
			}
		}
		return result;
	}

	public int CodePointAt(int i)
	{
		return char.ConvertToUtf32(Source, i);
	}

	public bool ScanHexEscape(char prefix, out char result)
	{
		int num = ((prefix == 'u') ? 4 : 2);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (!Eof())
			{
				char c = Source[Index];
				if (Character.IsHexDigit(c))
				{
					num2 = num2 * 16 + HexValue(c);
					Index++;
					continue;
				}
				result = '\0';
				return false;
			}
			result = '\0';
			return false;
		}
		result = (char)num2;
		return true;
	}

	public string ScanUnicodeCodePointEscape()
	{
		char c = Source[Index];
		int num = 0;
		if (c == '}')
		{
			ThrowUnexpectedToken();
		}
		while (!Eof())
		{
			c = Source[Index++];
			if (!Character.IsHexDigit(c))
			{
				break;
			}
			num = num * 16 + HexValue(c);
		}
		if (num > 1114111 || c != '}')
		{
			ThrowUnexpectedToken();
		}
		return Character.FromCodePoint(num);
	}

	public string GetIdentifier()
	{
		int num = Index++;
		while (!Eof())
		{
			char c = Source.CharCodeAt(Index);
			if (c == '\\')
			{
				Index = num;
				return GetComplexIdentifier();
			}
			if (c >= '\ud800' && c < '\udfff')
			{
				Index = num;
				return GetComplexIdentifier();
			}
			if (!Character.IsIdentifierPart(c))
			{
				break;
			}
			Index++;
		}
		return Source.Slice(num, Index);
	}

	public string GetComplexIdentifier()
	{
		int num = CodePointAt(Index);
		string text = Character.FromCodePoint(num);
		Index += text.Length;
		if (num == 92)
		{
			if (Source.CharCodeAt(Index) != 'u')
			{
				ThrowUnexpectedToken();
			}
			Index++;
			string text2;
			if (Source[Index] == '{')
			{
				Index++;
				text2 = ScanUnicodeCodePointEscape();
			}
			else
			{
				if (!ScanHexEscape('u', out var result) || result == '\\' || !Character.IsIdentifierStart(result))
				{
					ThrowUnexpectedToken();
				}
				text2 = ParserExtensions.CharToString(result);
			}
			text = text2;
		}
		while (!Eof())
		{
			num = CodePointAt(Index);
			string text2 = Character.FromCodePoint(num);
			if (!Character.IsIdentifierPart(text2))
			{
				break;
			}
			text += text2;
			Index += text2.Length;
			if (num != 92)
			{
				continue;
			}
			text = text.Substring(0, text.Length - 1);
			if (Source.CharCodeAt(Index) != 'u')
			{
				ThrowUnexpectedToken();
			}
			Index++;
			if (Index < Source.Length && Source[Index] == '{')
			{
				Index++;
				text2 = ScanUnicodeCodePointEscape();
			}
			else
			{
				if (!ScanHexEscape('u', out var result2) || result2 == '\\' || !Character.IsIdentifierPart(result2))
				{
					ThrowUnexpectedToken();
				}
				text2 = ParserExtensions.CharToString(result2);
			}
			text += text2;
		}
		return text;
	}

	public OctalValue OctalToDecimal(char ch)
	{
		bool octal = ch != '0';
		int num = OctalValue(ch);
		if (!Eof() && Character.IsOctalDigit(Source.CharCodeAt(Index)))
		{
			octal = true;
			num = num * 8 + OctalValue(Source[Index++]);
			if (ch >= '0' && ch <= '3' && !Eof() && Character.IsOctalDigit(Source.CharCodeAt(Index)))
			{
				num = num * 8 + OctalValue(Source[Index++]);
			}
		}
		return new OctalValue(num, octal);
	}

	public Token ScanIdentifier()
	{
		int index = Index;
		string text = ((Source.CharCodeAt(index) == '\\') ? GetComplexIdentifier() : GetIdentifier());
		TokenType tokenType = ((text.Length == 1) ? TokenType.Identifier : (IsKeyword(text) ? TokenType.Keyword : ("null".Equals(text) ? TokenType.NullLiteral : ((!"true".Equals(text) && !"false".Equals(text)) ? TokenType.Identifier : TokenType.BooleanLiteral))));
		if (tokenType != TokenType.Identifier && index + text.Length != Index)
		{
			int index2 = Index;
			Index = index;
			TolerateUnexpectedToken("Keyword must not contain escaped characters");
			Index = index2;
		}
		return new Token
		{
			Type = tokenType,
			Value = text,
			LineNumber = LineNumber,
			LineStart = LineStart,
			Start = index,
			End = Index
		};
	}

	public Token ScanPunctuator()
	{
		int index = Index;
		char c = Source[Index];
		string text = c.ToString();
		switch (c)
		{
		case '(':
		case '{':
			if (c == '{')
			{
				_curlyStack.Push("{");
			}
			Index++;
			break;
		case '.':
			Index++;
			if (Source.Length >= Index + 2 && Source[Index] == '.' && Source[Index + 1] == '.')
			{
				Index += 2;
				text = "...";
			}
			break;
		case '}':
			Index++;
			if (_curlyStack.Count > 0)
			{
				_curlyStack.Pop();
			}
			break;
		case ')':
		case ',':
		case ':':
		case ';':
		case '?':
		case '[':
		case ']':
		case '~':
			Index++;
			break;
		default:
			text = SafeSubstring(Source, Index, 4);
			if (text == ">>>=")
			{
				Index += 4;
				break;
			}
			text = SafeSubstring(Source, Index, 3);
			if (text.Length == 3 && FindThreeCharEqual(text, threeCharacterPunctutors) != null)
			{
				Index += 3;
				break;
			}
			text = SafeSubstring(Source, Index, 2);
			if (text.Length == 2 && FindTwoCharEqual(text, twoCharacterPunctuators) != null)
			{
				Index += 2;
				break;
			}
			text = Source[Index].ToString();
			if ("<>=!+-*%&|^/".IndexOf(text, StringComparison.Ordinal) >= 0)
			{
				Index++;
			}
			break;
		}
		if (Index == index)
		{
			ThrowUnexpectedToken();
		}
		return new Token
		{
			Type = TokenType.Punctuator,
			Value = text,
			LineNumber = LineNumber,
			LineStart = LineStart,
			Start = index,
			End = Index
		};
		static string SafeSubstring(string s, int startIndex, int length)
		{
			if (startIndex + length <= s.Length)
			{
				return s.Substring(startIndex, length);
			}
			return string.Empty;
		}
	}

	public Token ScanHexLiteral(int start)
	{
		int index = Index;
		while (!Eof() && Character.IsHexDigit(Source.CharCodeAt(Index)))
		{
			Index++;
		}
		string text = Source.Substring(index, Index - index);
		if (text.Length == 0)
		{
			ThrowUnexpectedToken();
		}
		if (Character.IsIdentifierStart(Source.CharCodeAt(Index)))
		{
			ThrowUnexpectedToken();
		}
		double num = 0.0;
		if (text.Length < 16)
		{
			num = Convert.ToInt64(text, 16);
		}
		else if (text.Length > 255)
		{
			num = double.PositiveInfinity;
		}
		else
		{
			double num2 = 1.0;
			string text2 = text.ToLowerInvariant();
			for (int num3 = text2.Length - 1; num3 >= 0; num3--)
			{
				char c = text2[num3];
				num = ((c > '9') ? (num + num2 * (double)(c - 97 + 10)) : (num + num2 * (double)(c - 48)));
				num2 *= 16.0;
			}
		}
		return new Token
		{
			Type = TokenType.NumericLiteral,
			NumericValue = num,
			Value = num,
			LineNumber = LineNumber,
			LineStart = LineStart,
			Start = start,
			End = Index
		};
	}

	public Token ScanBinaryLiteral(int start)
	{
		int index = Index;
		while (!Eof())
		{
			char c = Source[Index];
			if (c != '0' && c != '1')
			{
				break;
			}
			Index++;
		}
		string text = Source.Substring(index, Index - index);
		if (text.Length == 0)
		{
			ThrowUnexpectedToken();
		}
		if (!Eof())
		{
			char c = Source.CharCodeAt(Index);
			if (Character.IsIdentifierStart(c) || Character.IsDecimalDigit(c))
			{
				ThrowUnexpectedToken();
			}
		}
		return new Token
		{
			Type = TokenType.NumericLiteral,
			NumericValue = Convert.ToUInt32(text, 2),
			Value = text,
			LineNumber = LineNumber,
			LineStart = LineStart,
			Start = start,
			End = Index
		};
	}

	public Token ScanOctalLiteral(char prefix, int start)
	{
		StringBuilder stringBuilder = GetStringBuilder();
		bool flag = false;
		if (Character.IsOctalDigit(prefix))
		{
			flag = true;
			stringBuilder.Append("0").Append(Source[Index++]);
		}
		else
		{
			Index++;
		}
		while (!Eof() && Character.IsOctalDigit(Source.CharCodeAt(Index)))
		{
			stringBuilder.Append(Source[Index++]);
		}
		string text = stringBuilder.ToString();
		if (!flag && text.Length == 0)
		{
			ThrowUnexpectedToken();
		}
		if (Character.IsIdentifierStart(Source.CharCodeAt(Index)) || Character.IsDecimalDigit(Source.CharCodeAt(Index)))
		{
			ThrowUnexpectedToken();
		}
		ulong num;
		try
		{
			num = Convert.ToUInt64(text, 8);
		}
		catch (OverflowException)
		{
			ThrowUnexpectedToken("Value " + text + " was either too large or too small for a UInt64.");
			return null;
		}
		return new Token
		{
			Type = TokenType.NumericLiteral,
			NumericValue = num,
			Value = text,
			Octal = flag,
			LineNumber = LineNumber,
			LineStart = LineStart,
			Start = start,
			End = Index
		};
	}

	public bool IsImplicitOctalLiteral()
	{
		for (int i = Index + 1; i < _length; i++)
		{
			char c = Source[i];
			if (c == '8' || c == '9')
			{
				return false;
			}
			if (!Character.IsOctalDigit(c))
			{
				return true;
			}
		}
		return true;
	}

	public Token ScanNumericLiteral()
	{
		StringBuilder stringBuilder = GetStringBuilder();
		int index = Index;
		char c = Source[index];
		if (c != '.')
		{
			char c2 = Source[Index++];
			stringBuilder.Append(c2);
			c = Source.CharCodeAt(Index);
			if (c2 == '0')
			{
				if (c == 'x' || c == 'X')
				{
					Index++;
					return ScanHexLiteral(index);
				}
				if (c == 'b' || c == 'B')
				{
					Index++;
					return ScanBinaryLiteral(index);
				}
				if (c == 'o' || c == 'O')
				{
					return ScanOctalLiteral(c, index);
				}
				if (c > '\0' && Character.IsOctalDigit(c) && IsImplicitOctalLiteral())
				{
					return ScanOctalLiteral(c, index);
				}
			}
			while (Character.IsDecimalDigit(Source.CharCodeAt(Index)))
			{
				stringBuilder.Append(Source[Index++]);
			}
			c = Source.CharCodeAt(Index);
		}
		if (c == '.')
		{
			stringBuilder.Append(Source[Index++]);
			while (Character.IsDecimalDigit(Source.CharCodeAt(Index)))
			{
				stringBuilder.Append(Source[Index++]);
			}
			c = Source.CharCodeAt(Index);
		}
		if (c == 'e' || c == 'E')
		{
			stringBuilder.Append(Source[Index++]);
			c = Source.CharCodeAt(Index);
			if (c == '+' || c == '-')
			{
				stringBuilder.Append(Source[Index++]);
			}
			if (Character.IsDecimalDigit(Source.CharCodeAt(Index)))
			{
				while (Character.IsDecimalDigit(Source.CharCodeAt(Index)))
				{
					stringBuilder.Append(Source[Index++]);
				}
			}
			else
			{
				ThrowUnexpectedToken();
			}
		}
		if (Character.IsIdentifierStart(Source.CharCodeAt(Index)))
		{
			ThrowUnexpectedToken();
		}
		Token token = new Token
		{
			Type = TokenType.NumericLiteral,
			LineNumber = LineNumber,
			LineStart = LineStart,
			Start = index,
			End = Index
		};
		string text = stringBuilder.ToString();
		double result2;
		if (long.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var result))
		{
			token.NumericValue = result;
			token.Value = result;
		}
		else if (double.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out result2))
		{
			token.NumericValue = result2;
			token.Value = result2;
		}
		else
		{
			token.Value = (token.NumericValue = (text.TrimStart(Array.Empty<char>()).StartsWith("-") ? double.NegativeInfinity : double.PositiveInfinity));
		}
		return token;
	}

	public Token ScanStringLiteral()
	{
		int index = Index;
		char c = Source[index];
		Index++;
		bool flag = false;
		StringBuilder stringBuilder = GetStringBuilder();
		while (!Eof())
		{
			char c2 = ((Index < Source.Length) ? Source[Index] : '\0');
			Index++;
			if (c2 == c)
			{
				c = '\0';
				break;
			}
			if (c2 == '\\')
			{
				c2 = ((Index < Source.Length) ? Source[Index] : '\0');
				Index++;
				if (c2 == '\0' || !Character.IsLineTerminator(c2))
				{
					switch (c2)
					{
					case 'u':
					{
						if (Index < Source.Length && Source[Index] == '{')
						{
							Index++;
							stringBuilder.Append(ScanUnicodeCodePointEscape());
							break;
						}
						if (!ScanHexEscape(c2, out var result2))
						{
							ThrowUnexpectedToken();
						}
						stringBuilder.Append(result2);
						break;
					}
					case 'x':
					{
						if (!ScanHexEscape(c2, out var result))
						{
							ThrowUnexpectedToken("Invalid hexadecimal escape sequence");
						}
						stringBuilder.Append(result);
						break;
					}
					case 'n':
						stringBuilder.Append("\n");
						break;
					case 'r':
						stringBuilder.Append("\r");
						break;
					case 't':
						stringBuilder.Append("\t");
						break;
					case 'b':
						stringBuilder.Append("\b");
						break;
					case 'f':
						stringBuilder.Append("\f");
						break;
					case 'v':
						stringBuilder.Append("\v");
						break;
					case '8':
					case '9':
						stringBuilder.Append(c2);
						TolerateUnexpectedToken();
						break;
					default:
						if (c2 != 0 && Character.IsOctalDigit(c2))
						{
							OctalValue octalValue = OctalToDecimal(c2);
							flag = octalValue.Octal || flag;
							stringBuilder.Append((char)octalValue.Code);
						}
						else
						{
							stringBuilder.Append(c2);
						}
						break;
					}
				}
				else
				{
					LineNumber++;
					if (c2 == '\r' && Source[Index] == '\n')
					{
						Index++;
					}
					LineStart = Index;
				}
			}
			else
			{
				if (Character.IsLineTerminator(c2))
				{
					break;
				}
				stringBuilder.Append(c2);
			}
		}
		if (c != 0)
		{
			Index = index;
			ThrowUnexpectedToken();
		}
		return new Token
		{
			Type = TokenType.StringLiteral,
			Value = stringBuilder.ToString(),
			Octal = flag,
			LineNumber = LineNumber,
			LineStart = LineStart,
			Start = index,
			End = Index
		};
	}

	public Token ScanTemplate()
	{
		StringBuilder stringBuilder = GetStringBuilder();
		bool flag = false;
		int index = Index;
		bool flag2 = Source[index] == '`';
		bool tail = false;
		int num = 2;
		Index++;
		while (!Eof())
		{
			char c = Source[Index++];
			switch (c)
			{
			case '`':
				num = 1;
				tail = true;
				flag = true;
				break;
			case '$':
				if (Source[Index] == '{')
				{
					_curlyStack.Push("${");
					Index++;
					flag = true;
					break;
				}
				stringBuilder.Append(c);
				continue;
			case '\\':
				c = Source[Index++];
				if (!Character.IsLineTerminator(c))
				{
					switch (c)
					{
					case 'n':
						stringBuilder.Append("\n");
						break;
					case 'r':
						stringBuilder.Append("\r");
						break;
					case 't':
						stringBuilder.Append("\t");
						break;
					case 'u':
					{
						if (Source[Index] == '{')
						{
							Index++;
							stringBuilder.Append(ScanUnicodeCodePointEscape());
							break;
						}
						int index2 = Index;
						if (ScanHexEscape(c, out var result))
						{
							stringBuilder.Append(result);
							break;
						}
						Index = index2;
						stringBuilder.Append(c);
						break;
					}
					case 'x':
					{
						if (!ScanHexEscape(c, out var result2))
						{
							ThrowUnexpectedToken("Invalid hexadecimal escape sequence");
						}
						stringBuilder.Append(result2);
						break;
					}
					case 'b':
						stringBuilder.Append("\b");
						break;
					case 'f':
						stringBuilder.Append("\f");
						break;
					case 'v':
						stringBuilder.Append("\v");
						break;
					case '0':
						if (Character.IsDecimalDigit(Source.CharCodeAt(Index)))
						{
							ThrowUnexpectedToken("Octal literals are not allowed in template strings.");
						}
						stringBuilder.Append("\0");
						break;
					default:
						if (Character.IsOctalDigit(c))
						{
							ThrowUnexpectedToken("Octal literals are not allowed in template strings.");
						}
						else
						{
							stringBuilder.Append(c);
						}
						break;
					}
				}
				else
				{
					LineNumber++;
					if (c == '\r' && Source[Index] == '\n')
					{
						Index++;
					}
					LineStart = Index;
				}
				continue;
			default:
				if (Character.IsLineTerminator(c))
				{
					LineNumber++;
					if (c == '\r' && Source[Index] == '\n')
					{
						Index++;
					}
					LineStart = Index;
					stringBuilder.Append("\n");
				}
				else
				{
					stringBuilder.Append(c);
				}
				continue;
			}
			break;
		}
		if (!flag)
		{
			ThrowUnexpectedToken();
		}
		if (!flag2)
		{
			_curlyStack.Pop();
		}
		return new Token
		{
			Type = TokenType.Template,
			Value = stringBuilder.ToString(),
			RawTemplate = Source.Slice(index + 1, Index - num),
			Head = flag2,
			Tail = tail,
			LineNumber = LineNumber,
			LineStart = LineStart,
			Start = index,
			End = Index
		};
	}

	public Regex TestRegExp(string pattern, string flags)
	{
		string astralSubstitute = "\uffff";
		string text = pattern;
		if (flags.IndexOf('u') >= 0)
		{
			text = Regex.Replace(text, "\\\\u\\{([0-9a-fA-F]+)\\}|\\\\u([a-fA-F0-9]{4})", delegate(Match match)
			{
				int num2 = (string.IsNullOrEmpty(match.Groups[1].Value) ? Convert.ToInt32(match.Groups[2].Value, 16) : Convert.ToInt32(match.Groups[1].Value, 16));
				if (num2 > 1114111)
				{
					ThrowUnexpectedToken("Invalid regular expression");
				}
				return (num2 <= 65535) ? ParserExtensions.CharToString((char)num2) : astralSubstitute;
			});
			text = Regex.Replace(text, "[\ud800-\udbff][\udc00-\udfff]", astralSubstitute);
		}
		RegexOptions regexOptions = ParseRegexOptions(flags);
		try
		{
			new Regex(text, regexOptions);
		}
		catch
		{
			ThrowUnexpectedToken("Invalid regular expression");
		}
		try
		{
			if (_adaptRegexp && regexOptions.HasFlag(RegexOptions.Multiline))
			{
				int num = 0;
				string text2 = pattern;
				while ((num = text2.IndexOf("$", num, StringComparison.Ordinal)) != -1)
				{
					if (num > 0 && text2[num - 1] != '\\')
					{
						text2 = text2.Substring(0, num) + "\\r?" + text2.Substring(num);
						num += 4;
					}
				}
				pattern = text2;
			}
			return new Regex(pattern, regexOptions);
		}
		catch
		{
			return null;
		}
	}

	public Token ScanRegExpBody()
	{
		char c = Source[Index];
		StringBuilder stringBuilder = GetStringBuilder();
		stringBuilder.Append(Source[Index++]);
		bool flag = false;
		bool flag2 = false;
		while (!Eof())
		{
			c = Source[Index++];
			stringBuilder.Append(c);
			if (c == '\\')
			{
				c = Source[Index++];
				if (Character.IsLineTerminator(c))
				{
					ThrowUnexpectedToken("Invalid regular expression= missing /");
				}
				stringBuilder.Append(c);
				continue;
			}
			if (Character.IsLineTerminator(c))
			{
				ThrowUnexpectedToken("Invalid regular expression= missing /");
				continue;
			}
			if (flag)
			{
				if (c == ']')
				{
					flag = false;
				}
				continue;
			}
			switch (c)
			{
			case '/':
				break;
			case '[':
				flag = true;
				continue;
			default:
				continue;
			}
			flag2 = true;
			break;
		}
		if (!flag2)
		{
			ThrowUnexpectedToken("Invalid regular expression= missing /");
		}
		string text = stringBuilder.ToString();
		return new Token
		{
			Value = text.Substring(1, text.Length - 2),
			Literal = text
		};
	}

	public Token ScanRegExpFlags()
	{
		string text = "";
		string text2 = "";
		while (!Eof())
		{
			char c = Source[Index];
			if (!Character.IsIdentifierPart(c))
			{
				break;
			}
			Index++;
			if (c == '\\' && !Eof())
			{
				c = Source[Index];
				if (c == 'u')
				{
					Index++;
					int i = Index;
					if (ScanHexEscape('u', out c))
					{
						text2 += c;
						text += "\\u";
						for (; i < Index; i++)
						{
							text += Source[i];
						}
					}
					else
					{
						Index = i;
						text2 += "u";
						text += "\\u";
					}
					TolerateUnexpectedToken();
				}
				else
				{
					text += "\\";
					TolerateUnexpectedToken();
				}
			}
			else
			{
				text2 += c;
				text += c;
			}
		}
		return new Token
		{
			Value = text2,
			Literal = text
		};
	}

	public Token ScanRegExp()
	{
		int index = Index;
		Token token = ScanRegExpBody();
		Token token2 = ScanRegExpFlags();
		string flags = (string)token2.Value;
		Regex value = TestRegExp((string)token.Value, flags);
		return new Token
		{
			Type = TokenType.RegularExpression,
			Value = value,
			Literal = token.Literal + token2.Literal,
			RegexValue = new RegexValue((string)token.Value, flags),
			LineNumber = LineNumber,
			LineStart = LineStart,
			Start = index,
			End = Index
		};
	}

	public Token Lex()
	{
		if (Eof())
		{
			return new Token
			{
				Type = TokenType.EOF,
				LineNumber = LineNumber,
				LineStart = LineStart,
				Start = Index,
				End = Index
			};
		}
		char c = Source.CharCodeAt(Index);
		if (Character.IsIdentifierStart(c))
		{
			return ScanIdentifier();
		}
		switch (c)
		{
		case '(':
		case ')':
		case ';':
			return ScanPunctuator();
		case '"':
		case '\'':
			return ScanStringLiteral();
		case '.':
			if (Character.IsDecimalDigit(Source.CharCodeAt(Index + 1)))
			{
				return ScanNumericLiteral();
			}
			return ScanPunctuator();
		default:
			if (Character.IsDecimalDigit(c))
			{
				return ScanNumericLiteral();
			}
			if (c == '`' || (c == '}' && _curlyStack.Count > 0 && _curlyStack.Peek() == "${"))
			{
				return ScanTemplate();
			}
			if (c >= '\ud800' && c < '\udfff' && char.IsLetter(Source, Index))
			{
				return ScanIdentifier();
			}
			return ScanPunctuator();
		}
	}

	public RegexOptions ParseRegexOptions(string flags)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		for (int i = 0; i < flags.Length; i++)
		{
			switch (flags[i])
			{
			case 'g':
				if (flag)
				{
					ThrowUnexpectedToken("Invalid regular expression");
				}
				flag = true;
				break;
			case 'i':
				if (flag3)
				{
					ThrowUnexpectedToken("Invalid regular expression");
				}
				flag3 = true;
				break;
			case 'm':
				if (flag2)
				{
					ThrowUnexpectedToken("Invalid regular expression");
				}
				flag2 = true;
				break;
			case 'u':
				if (flag4)
				{
					ThrowUnexpectedToken("Invalid regular expression");
				}
				flag4 = true;
				break;
			case 'y':
				if (flag5)
				{
					ThrowUnexpectedToken("Invalid regular expression");
				}
				flag5 = true;
				break;
			case 's':
				if (flag6)
				{
					ThrowUnexpectedToken("Invalid regular expression");
				}
				flag6 = true;
				break;
			default:
				ThrowUnexpectedToken("Invalid regular expression");
				break;
			}
		}
		RegexOptions regexOptions = RegexOptions.ECMAScript;
		if (flag2)
		{
			regexOptions |= RegexOptions.Multiline;
		}
		if (flag6)
		{
			regexOptions |= RegexOptions.Singleline;
			regexOptions &= ~RegexOptions.ECMAScript;
		}
		if (flag3)
		{
			regexOptions |= RegexOptions.IgnoreCase;
		}
		return regexOptions;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string FindTwoCharEqual(string input, string[] alternatives)
	{
		char c = input[1];
		char c2 = input[0];
		foreach (string text in alternatives)
		{
			if (c2 == text[0] && c == text[1])
			{
				return text;
			}
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string FindThreeCharEqual(string input, string[] alternatives)
	{
		char c = input[2];
		char c2 = input[1];
		char c3 = input[0];
		for (int i = 0; i < alternatives.Length; i++)
		{
			string text = alternatives[i];
			if (c3 == text[0] && c2 == text[1] && c == text[2])
			{
				return alternatives[i];
			}
		}
		return null;
	}
}
