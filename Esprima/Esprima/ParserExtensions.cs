using System;
using System.Runtime.CompilerServices;

namespace Esprima;

public static class ParserExtensions
{
	private static readonly string[] charToString;

	static ParserExtensions()
	{
		charToString = new string[256];
		for (int i = 0; i < charToString.Length; i++)
		{
			charToString[i] = ((char)i).ToString();
		}
	}

	public static string Slice(this string source, int start, int end)
	{
		int length = source.Length;
		int num = ((start < 0) ? Math.Max(length + start, 0) : Math.Min(start, length));
		int num2 = Math.Max(((end < 0) ? Math.Max(length + end, 0) : Math.Min(end, length)) - num, 0);
		switch (num2)
		{
		case 1:
			return CharToString(source[num]);
		case 2:
			if (source[num] == 'i')
			{
				if (source[num + 1] == 'f')
				{
					return "if";
				}
				if (source[num + 1] == 'd')
				{
					return "id";
				}
				if (source[num + 1] == 'n')
				{
					return "in";
				}
			}
			if (source[num] == '"' && source[num + 1] == '"')
			{
				return "\"\"";
			}
			if (source[num] == '\'' && source[num + 1] == '\'')
			{
				return "''";
			}
			if (source[num] == 'f' && source[num + 1] == 'n')
			{
				return "fn";
			}
			if (source[num] == 'o' && source[num + 1] == 'n')
			{
				return "on";
			}
			if (source[num] == 'd' && source[num + 1] == 'o')
			{
				return "do";
			}
			break;
		case 3:
			if (source[num] == 'a')
			{
				if (source[num + 1] == 'd' && source[num + 2] == 'd')
				{
					return "add";
				}
				if (source[num + 1] == 'r' && source[num + 2] == 'g')
				{
					return "arg";
				}
			}
			if (source[num] == 'f' && source[num + 1] == 'o' && source[num + 2] == 'r')
			{
				return "for";
			}
			if (source[num] == 'v' && source[num + 1] == 'a')
			{
				if (source[num + 2] == 'l')
				{
					return "val";
				}
				if (source[num + 2] == 'r')
				{
					return "var";
				}
			}
			if (source[num] == 'm')
			{
				if (source[num + 1] == 'a')
				{
					if (source[num + 2] == 'p')
					{
						return "map";
					}
					if (source[num + 2] == 'x')
					{
						return "max";
					}
				}
				if (source[num + 1] == 'i' && source[num + 2] == 'n')
				{
					return "min";
				}
			}
			if (source[num] == 'n' && source[num + 1] == 'e' && source[num + 2] == 'w')
			{
				return "new";
			}
			if (source[num] == 'k' && source[num + 1] == 'e' && source[num + 2] == 'y')
			{
				return "key";
			}
			if (source[num] == 'l' && source[num + 1] == 'e' && source[num + 2] == 'n')
			{
				return "len";
			}
			if (source[num] == 'o' && source[num + 1] == 'b' && source[num + 2] == 'j')
			{
				return "obj";
			}
			if (source[num] == 'p' && source[num + 1] == 'o' && source[num + 2] == 'p')
			{
				return "pop";
			}
			if (source[num] == 'u' && source[num + 1] == 'r' && source[num + 2] == 'l')
			{
				return "url";
			}
			if (source[num] == 't' && source[num + 1] == 'r' && source[num + 2] == 'y')
			{
				return "try";
			}
			break;
		case 4:
			if (source[num] == 't')
			{
				if (source[num + 1] == 'e')
				{
					if (source[num + 2] == 'x' && source[num + 3] == 't')
					{
						return "text";
					}
					if (source[num + 2] == 's' && source[num + 3] == 't')
					{
						return "test";
					}
				}
				if (source[num + 1] == 'r' && source[num + 2] == 'u' && source[num + 3] == 'e')
				{
					return "true";
				}
				if (source[num + 1] == 'h')
				{
					if (source[num + 2] == 'i' && source[num + 3] == 's')
					{
						return "this";
					}
					if (source[num + 2] == 'e' && source[num + 3] == 'n')
					{
						return "then";
					}
				}
				if (source[num + 1] == 'y' && source[num + 2] == 'p' && source[num + 3] == 'e')
				{
					return "type";
				}
			}
			if (source[num] == 'e')
			{
				if (source[num + 1] == 'a' && source[num + 2] == 'c' && source[num + 3] == 'h')
				{
					return "each";
				}
				if (source[num + 1] == 'l')
				{
					if (source[num + 2] == 's' && source[num + 3] == 'e')
					{
						return "else";
					}
					if (source[num + 2] == 'e' && source[num + 3] == 'm')
					{
						return "elem";
					}
				}
			}
			if (source[num] == 'a')
			{
				if (source[num + 1] == 'r' && source[num + 2] == 'g' && source[num + 3] == 's')
				{
					return "args";
				}
				if (source[num + 1] == 't' && source[num + 2] == 't' && source[num + 3] == 'r')
				{
					return "attr";
				}
			}
			if (source[num] == 'f')
			{
				if (source[num + 1] == 'i' && source[num + 2] == 'n' && source[num + 3] == 'd')
				{
					return "find";
				}
				if (source[num + 1] == 'r' && source[num + 2] == 'o' && source[num + 3] == 'm')
				{
					return "from";
				}
			}
			if (source[num] == 'd')
			{
				if (source[num + 1] == 'a' && source[num + 2] == 't' && source[num + 3] == 'a')
				{
					return "data";
				}
				if (source[num + 1] == 'o' && source[num + 2] == 'n' && source[num + 3] == 'e')
				{
					return "done";
				}
			}
			if (source[num] == 'M' && source[num + 1] == 'a' && source[num + 2] == 't' && source[num + 3] == 'h')
			{
				return "Math";
			}
			if (source[num] == 'p' && source[num + 1] == 'u' && source[num + 2] == 's' && source[num + 3] == 'h')
			{
				return "push";
			}
			if (source[num] == 'k' && source[num + 1] == 'e' && source[num + 2] == 'y' && source[num + 3] == 's')
			{
				return "keys";
			}
			if (source[num] == 'b' && source[num + 1] == 'i' && source[num + 2] == 'n' && source[num + 3] == 'd')
			{
				return "bind";
			}
			if (source[num] == 'j' && source[num + 1] == 'o' && source[num + 2] == 'i' && source[num + 3] == 'n')
			{
				return "join";
			}
			if (source[num] == 'c' && source[num + 1] == 'a')
			{
				if (source[num + 2] == 's' && source[num + 3] == 'e')
				{
					return "case";
				}
				if (source[num + 2] == 'l' && source[num + 3] == 'l')
				{
					return "call";
				}
			}
			if (source[num] == 'n')
			{
				if (source[num + 1] == 'u' && source[num + 2] == 'l' && source[num + 3] == 'l')
				{
					return "null";
				}
				if (source[num + 1] == 'a' && source[num + 2] == 'm' && source[num + 3] == 'e')
				{
					return "name";
				}
				if (source[num + 1] == 'o' && source[num + 2] == 'd' && source[num + 3] == 'e')
				{
					return "node";
				}
			}
			if (source[num] == 's')
			{
				if (source[num + 1] == 'e' && source[num + 2] == 'l' && source[num + 3] == 'f')
				{
					return "self";
				}
				if (source[num + 1] == 'o' && source[num + 2] == 'r' && source[num + 3] == 't')
				{
					return "sort";
				}
			}
			break;
		case 5:
			if (source[num] == 'a' && source[num + 1] == 'p' && source[num + 2] == 'p' && source[num + 3] == 'l' && source[num + 4] == 'y')
			{
				return "apply";
			}
			if (source[num] == 'b' && source[num + 1] == 'r' && source[num + 2] == 'e' && source[num + 3] == 'a' && source[num + 4] == 'k')
			{
				return "break";
			}
			if (source[num] == 'm' && source[num + 1] == 'a' && source[num + 2] == 't' && source[num + 3] == 'c' && source[num + 4] == 'h')
			{
				return "match";
			}
			if (source[num] == 'f' && source[num + 1] == 'a' && source[num + 2] == 'l' && source[num + 3] == 's' && source[num + 4] == 'e')
			{
				return "false";
			}
			if (source[num] == 'v' && source[num + 1] == 'a' && source[num + 2] == 'l' && source[num + 3] == 'u' && source[num + 4] == 'e')
			{
				return "value";
			}
			if (source[num] == 'e' && source[num + 1] == 'v' && source[num + 2] == 'e')
			{
				if (source[num + 3] == 'r' && source[num + 4] == 'y')
				{
					return "every";
				}
				if (source[num + 3] == 'n' && source[num + 4] == 't')
				{
					return "event";
				}
			}
			if (source[num] == 'i' && source[num + 1] == 'n' && source[num + 2] == 'd' && source[num + 3] == 'e' && source[num + 4] == 'x')
			{
				return "index";
			}
			if (source[num] == 'w' && source[num + 1] == 'h' && source[num + 2] == 'i' && source[num + 3] == 'l' && source[num + 4] == 'e')
			{
				return "while";
			}
			if (source[num] == 't' && source[num + 1] == 'h' && source[num + 2] == 'r' && source[num + 3] == 'o' && source[num + 4] == 'w')
			{
				return "throw";
			}
			if (source[num] == 'c' && source[num + 1] == 'a' && source[num + 2] == 't' && source[num + 3] == 'c' && source[num + 4] == 'h')
			{
				return "catch";
			}
			if (source[num] == 's')
			{
				if (source[num + 1] == 'l' && source[num + 2] == 'i' && source[num + 3] == 'c' && source[num + 4] == 'e')
				{
					return "slice";
				}
				if (source[num + 1] == 'p' && source[num + 2] == 'l' && source[num + 3] == 'i' && source[num + 4] == 't')
				{
					return "split";
				}
				if (source[num + 1] == 'h' && source[num + 2] == 'i' && source[num + 3] == 'f' && source[num + 4] == 't')
				{
					return "shift";
				}
			}
			if (source[num] == 'A' && source[num + 1] == 'r' && source[num + 2] == 'r' && source[num + 3] == 'a' && source[num + 4] == 'y')
			{
				return "Array";
			}
			break;
		case 6:
			if (source[num] == 'o' && source[num + 1] == 'b' && source[num + 2] == 'j' && source[num + 3] == 'e' && source[num + 4] == 'c' && source[num + 5] == 't')
			{
				return "object";
			}
			if (source[num] == 'O' && source[num + 1] == 'b' && source[num + 2] == 'j' && source[num + 3] == 'e' && source[num + 4] == 'c' && source[num + 5] == 't')
			{
				return "Object";
			}
			if (source[num] == 'c' && source[num + 1] == 'o' && source[num + 2] == 'n' && source[num + 3] == 'c' && source[num + 4] == 'a' && source[num + 5] == 't')
			{
				return "concat";
			}
			if (source[num] == 'e' && source[num + 1] == 'x' && source[num + 2] == 't' && source[num + 3] == 'e' && source[num + 4] == 'n' && source[num + 5] == 'd')
			{
				return "extend";
			}
			if (source[num] == 'f' && source[num + 1] == 'i' && source[num + 2] == 'l' && source[num + 3] == 't' && source[num + 4] == 'e' && source[num + 5] == 'r')
			{
				return "filter";
			}
			if (source[num] == 'l' && source[num + 1] == 'e' && source[num + 2] == 'n' && source[num + 3] == 'g' && source[num + 4] == 't' && source[num + 5] == 'h')
			{
				return "length";
			}
			if (source[num] == 'r' && source[num + 1] == 'e')
			{
				if (source[num + 2] == 't' && source[num + 3] == 'u' && source[num + 4] == 'r' && source[num + 5] == 'n')
				{
					return "return";
				}
				if (source[num + 2] == 's' && source[num + 3] == 'u' && source[num + 4] == 'l' && source[num + 5] == 't')
				{
					return "result";
				}
			}
			if (source[num] == 'j' && source[num + 1] == 'Q' && source[num + 2] == 'u' && source[num + 3] == 'e' && source[num + 4] == 'r' && source[num + 5] == 'y')
			{
				return "jQuery";
			}
			if (source[num] == 'm' && source[num + 1] == 'o' && source[num + 2] == 'b' && source[num + 3] == 'i' && source[num + 4] == 'l' && source[num + 5] == 'e')
			{
				return "mobile";
			}
			if (source[num] == 't' && source[num + 1] == 'y' && source[num + 2] == 'p' && source[num + 3] == 'e' && source[num + 4] == 'o' && source[num + 5] == 'f')
			{
				return "typeof";
			}
			if (source[num] == 'w' && source[num + 1] == 'i' && source[num + 2] == 'n' && source[num + 3] == 'd' && source[num + 4] == 'o' && source[num + 5] == 'w')
			{
				return "window";
			}
			if (source[num] == 's')
			{
				if (source[num + 1] == 'u' && source[num + 2] == 'b' && source[num + 3] == 's' && source[num + 4] == 't' && source[num + 5] == 'r')
				{
					return "substr";
				}
				if (source[num + 1] == 'p' && source[num + 2] == 'l' && source[num + 3] == 'i' && source[num + 4] == 'c' && source[num + 5] == 'e')
				{
					return "splice";
				}
			}
			break;
		case 7:
			if (source[num] == 'e' && source[num + 1] == 'l' && source[num + 2] == 'e' && source[num + 3] == 'm' && source[num + 4] == 'e' && source[num + 5] == 'n' && source[num + 6] == 't')
			{
				return "element";
			}
			if (source[num] == 'o' && source[num + 1] == 'p' && source[num + 2] == 't' && source[num + 3] == 'i' && source[num + 4] == 'o' && source[num + 5] == 'n' && source[num + 6] == 's')
			{
				return "options";
			}
			if (source[num] == 'r' && source[num + 1] == 'e')
			{
				if (source[num + 2] == 'p' && source[num + 3] == 'l' && source[num + 4] == 'a' && source[num + 5] == 'c' && source[num + 6] == 'e')
				{
					return "replace";
				}
				if (source[num + 2] == 'v' && source[num + 3] == 'e' && source[num + 4] == 'r' && source[num + 5] == 's' && source[num + 6] == 'e')
				{
					return "reverse";
				}
			}
			if (source[num] == 'f' && source[num + 1] == 'o' && source[num + 2] == 'r' && source[num + 3] == 'E' && source[num + 4] == 'a' && source[num + 5] == 'c' && source[num + 6] == 'h')
			{
				return "forEach";
			}
			if (source[num] == 'i' && source[num + 1] == 'n' && source[num + 2] == 'd' && source[num + 3] == 'e' && source[num + 4] == 'x' && source[num + 5] == 'O' && source[num + 6] == 'f')
			{
				return "indexOf";
			}
			if (source[num] == 'c' && source[num + 1] == 'o' && source[num + 2] == 'n' && source[num + 3] == 't' && source[num + 4] == 'e' && source[num + 5] == 'x' && source[num + 6] == 't')
			{
				return "context";
			}
			if (source[num] == 'i' && source[num + 1] == 's' && source[num + 2] == 'A' && source[num + 3] == 'r' && source[num + 4] == 'r' && source[num + 5] == 'a' && source[num + 6] == 'y')
			{
				return "isArray";
			}
			if (source[num] == 'u' && source[num + 1] == 'n' && source[num + 2] == 's' && source[num + 3] == 'h' && source[num + 4] == 'i' && source[num + 5] == 'f' && source[num + 6] == 't')
			{
				return "unshift";
			}
			break;
		case 8:
			if (source[num] == 'f' && source[num + 1] == 'u' && source[num + 2] == 'n' && source[num + 3] == 'c' && source[num + 4] == 't' && source[num + 5] == 'i' && source[num + 6] == 'o' && source[num + 7] == 'n')
			{
				return "function";
			}
			if (source[num] == 'd' && source[num + 1] == 'o' && source[num + 2] == 'c' && source[num + 3] == 'u' && source[num + 4] == 'm' && source[num + 5] == 'e' && source[num + 6] == 'n' && source[num + 7] == 't')
			{
				return "document";
			}
			if (source[num] == 't' && source[num + 1] == 'o' && source[num + 2] == 'S' && source[num + 3] == 't' && source[num + 4] == 'r' && source[num + 5] == 'i' && source[num + 6] == 'n' && source[num + 7] == 'g')
			{
				return "toString";
			}
			if (source[num] == 's' && source[num + 1] == 'e' && source[num + 2] == 'l' && source[num + 3] == 'e' && source[num + 4] == 'c' && source[num + 5] == 't' && source[num + 6] == 'o' && source[num + 7] == 'r')
			{
				return "selector";
			}
			break;
		case 9:
			if (source[num] == 'u' && source[num + 1] == 'n' && source[num + 2] == 'd' && source[num + 3] == 'e' && source[num + 4] == 'f' && source[num + 5] == 'i' && source[num + 6] == 'n' && source[num + 7] == 'e' && source[num + 8] == 'd')
			{
				return "undefined";
			}
			if (source[num] == 'p' && source[num + 1] == 'r' && source[num + 2] == 'o' && source[num + 3] == 't' && source[num + 4] == 'o' && source[num + 5] == 't' && source[num + 6] == 'y' && source[num + 7] == 'p' && source[num + 8] == 'e')
			{
				return "prototype";
			}
			if (source[num] == 's' && source[num + 1] == 'u' && source[num + 2] == 'b' && source[num + 3] == 's' && source[num + 4] == 't' && source[num + 5] == 'r' && source[num + 6] == 'i' && source[num + 7] == 'n' && source[num + 8] == 'g')
			{
				return "substring";
			}
			break;
		case 10:
			if (source[num] == 'i' && source[num + 1] == 's' && source[num + 2] == 'F' && source[num + 3] == 'u' && source[num + 4] == 'n' && source[num + 5] == 'c' && source[num + 6] == 't' && source[num + 7] == 'i' && source[num + 8] == 'o' && source[num + 9] == 'n')
			{
				return "isFunction";
			}
			break;
		case 11:
			if (source[num] == 't' && source[num + 1] == 'o' && source[num + 2] == 'L' && source[num + 3] == 'o' && source[num + 4] == 'w' && source[num + 5] == 'e' && source[num + 6] == 'r' && source[num + 7] == 'C' && source[num + 8] == 'a' && source[num + 9] == 's' && source[num + 10] == 'e')
			{
				return "toLowerCase";
			}
			break;
		case 14:
			if (source[num] == 'h' && source[num + 1] == 'a' && source[num + 2] == 's' && source[num + 3] == 'O' && source[num + 4] == 'w' && source[num + 5] == 'n' && source[num + 6] == 'P' && source[num + 7] == 'r' && source[num + 8] == 'o' && source[num + 9] == 'p' && source[num + 10] == 'e' && source[num + 11] == 'r' && source[num + 12] == 't' && source[num + 13] == 'y')
			{
				return "hasOwnProperty";
			}
			break;
		}
		return source.Substring(num, num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string CharToString(char c)
	{
		if (c >= '\0' && c < charToString.Length)
		{
			return charToString[(uint)c];
		}
		return c.ToString();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static char CharCodeAt(this string source, int index)
	{
		if (index < 0 || index > source.Length - 1)
		{
			return '\0';
		}
		return source[index];
	}
}
