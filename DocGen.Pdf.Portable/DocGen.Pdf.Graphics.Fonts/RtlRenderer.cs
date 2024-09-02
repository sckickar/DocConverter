using System;
using System.Text;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Fonts;

internal class RtlRenderer
{
	private const char c_openBracket = '(';

	private const char c_closeBracket = ')';

	private RtlRenderer()
	{
		throw new NotImplementedException();
	}

	public static string[] Layout(LineInfo line, PdfTrueTypeFont font, bool rtl, bool wordSpace, PdfStringFormat format)
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
		return font.Unicode ? CustomLayout(line, font, rtl, wordSpace, format) : new string[1] { line.Text };
	}

	internal static string[] SplitLayout(string line, PdfTrueTypeFont font, bool rtl, bool wordSpace, PdfStringFormat format)
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

	internal static bool GetGlyphIndices(string line, PdfTrueTypeFont font, bool rtl, out ushort[] glyphs, bool custom)
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

	private static string[] CustomLayout(LineInfo line, PdfTrueTypeFont font, bool rtl, bool wordSpace, PdfStringFormat format)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		string text = null;
		if (line.BidiLevels == null)
		{
			text = ((format == null || format.TextDirection == PdfTextDirection.None) ? CustomLayout(line.Text, rtl, format) : CustomLayout(new ArabicShapeRenderer().Shape(line.Text.ToCharArray(), 0), rtl, format));
		}
		else
		{
			string inputText = new ArabicShapeRenderer().Shape(line.Text.ToCharArray(), 0);
			text = new Bidi().DoBidiReverseOrder(inputText, line.BidiLevels, line.TrimCount);
		}
		text = TrimLRM(text);
		string[] array = null;
		if (!wordSpace)
		{
			array = new string[1] { AddChars(font, text, format) };
		}
		else
		{
			string[] array2 = text.Split((char[]?)null);
			int num = array2.Length;
			for (int i = 0; i < num; i++)
			{
				array2[i] = AddChars(font, array2[i], format);
			}
			array = array2;
		}
		return array;
	}

	internal static string TrimLRM(string text)
	{
		string text2 = text;
		if (text2 != null)
		{
			char[] array = text2.ToCharArray();
			if (array.Length != 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < array.Length; i++)
				{
					if (!InvisibleCharacter(array[i]))
					{
						stringBuilder.Append(array[i]);
					}
				}
				text2 = stringBuilder.ToString();
			}
		}
		return text2;
	}

	internal static bool InvisibleCharacter(int character)
	{
		if (character < 8203 || character > 8207)
		{
			if (character >= 8234)
			{
				return character <= 8238;
			}
			return false;
		}
		return true;
	}

	private static string CustomLayout(string line, bool rtl, PdfStringFormat format)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		string result = line;
		if (format != null && format.TextDirection != 0)
		{
			Bidi obj = new Bidi
			{
				m_isVisualOrder = false
			};
			result = obj.GetLogicalToVisualString(line, rtl);
			obj.m_isVisualOrder = true;
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
				throw new PdfException("Internal error.");
			}
		}
		return array;
	}

	private static string AddChars(PdfTrueTypeFont font, ushort[] glyphs, PdfStringFormat format)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (glyphs == null)
		{
			throw new ArgumentNullException("glyphs");
		}
		if (font.InternalFont is UnicodeTrueTypeFont unicodeTrueTypeFont && format != null && format.ComplexScript)
		{
			unicodeTrueTypeFont.SetSymbols(glyphs, openType: true);
		}
		else
		{
			font.SetSymbols(glyphs);
		}
		char[] array = new char[glyphs.Length];
		for (int i = 0; i < glyphs.Length; i++)
		{
			array[i] = (char)glyphs[i];
		}
		return PdfString.ByteToString(PdfString.ToUnicodeArray(new string(array), bAddPrefix: false));
	}

	private static string AddChars(PdfTrueTypeFont font, string line, PdfStringFormat format)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		string text = line;
		UnicodeTrueTypeFont unicodeTrueTypeFont = font.InternalFont as UnicodeTrueTypeFont;
		TtfReader ttfReader = unicodeTrueTypeFont.TtfReader;
		if (format != null && format.ComplexScript)
		{
			unicodeTrueTypeFont.SetSymbols(line, opentype: true);
		}
		else
		{
			font.SetSymbols(text);
		}
		text = ttfReader.ConvertString(text);
		return PdfString.ByteToString(PdfString.ToUnicodeArray(text, bAddPrefix: false));
	}

	private static string[] CustomSplitLayout(string line, PdfTrueTypeFont font, bool rtl, bool wordSpace, PdfStringFormat format)
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
