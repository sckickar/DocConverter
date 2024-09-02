using System;

namespace DocGen.Office;

internal class RtlRenderer
{
	private const char c_openBracket = '(';

	private const char c_closeBracket = ')';

	private RtlRenderer()
	{
		throw new NotImplementedException();
	}

	internal static string[] SplitLayout(string line, TrueTypeFont font, bool rtl, bool wordSpace, TrueTypeFontStringFormat format)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		string[] array = null;
		if (0 == 0 || array == null)
		{
			array = CustomSplitLayout(line, font, rtl, wordSpace, format);
		}
		return array;
	}

	private static bool IsEnglish(string word)
	{
		char c = ((word.Length > 0) ? word[0] : '\0');
		if (c >= '\0')
		{
			return c < 'ÿ';
		}
		return false;
	}

	private static void KeepOrder(string[] words, int startIndex, int count, string[] result, int resultIndex)
	{
		int num = 0;
		int num2 = resultIndex - count + 1;
		while (num < count)
		{
			result[num2] = words[num + startIndex];
			num++;
			num2++;
		}
	}

	internal static bool GetGlyphIndices(string line, TrueTypeFont font, bool rtl, out ushort[] glyphs, bool custom)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		glyphs = null;
		if (line.Length == 0)
		{
			return false;
		}
		string text = new ArabicShapeRenderer().Shape(line.ToCharArray(), 0);
		TtfReader ttfReader = (font.InternalFont as UnicodeTrueTypeFont).TtfReader;
		ttfReader.m_missedGlyphCount = 0;
		glyphs = new ushort[text.Length];
		int num = 0;
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			char charCode = text[i];
			TtfGlyphInfo glyph = ttfReader.GetGlyph(charCode);
			if (!glyph.Empty)
			{
				glyphs[num++] = (ushort)glyph.Index;
			}
		}
		return true;
	}

	private static string CustomLayout(string line, bool rtl, TrueTypeFontStringFormat format)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		string result = line;
		if (format != null && format.TextDirection != 0)
		{
			result = new Bidi().GetLogicalToVisualString(line, rtl);
		}
		return result;
	}

	private static string[] ReverseWords(string[] words)
	{
		if (words == null)
		{
			throw new ArgumentNullException("words");
		}
		int num = words.Length;
		string[] array = new string[num];
		string text = null;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = num - 1;
		while (num4 < num)
		{
			switch (num2)
			{
			case 0:
				text = words[num4];
				if (IsEnglish(text))
				{
					num3 = 0;
					num2 = 1;
				}
				else
				{
					array[num5] = text;
					num4++;
					num5--;
				}
				break;
			case 1:
				num3++;
				num4++;
				if (num4 < num)
				{
					text = words[num4];
				}
				if (num4 >= num || !IsEnglish(text))
				{
					KeepOrder(words, num4 - num3, num3, array, num5);
					num5 -= num3;
					num2 = 0;
				}
				break;
			default:
				throw new Exception("Internal error.");
			}
		}
		return array;
	}

	private static string[] CustomSplitLayout(string line, TrueTypeFont font, bool rtl, bool wordSpace, TrueTypeFontStringFormat format)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		return CustomLayout(line, rtl, format).Split((char[]?)null);
	}
}
