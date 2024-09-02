using System.Collections.Generic;
using System.Text;

namespace DocGen.Pdf.Graphics.Fonts;

internal class OtfGlyphTokenizer
{
	internal OtfGlyphInfoList m_glyphInfoList;

	internal int m_position;

	internal OtfGlyphTokenizer(OtfGlyphInfoList glyphInfoList)
	{
		m_glyphInfoList = glyphInfoList;
	}

	internal OtfGlyphTokenizer()
	{
	}

	internal OtfGlyphInfo[] ReadWord(out string text)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<OtfGlyphInfo> list = new List<OtfGlyphInfo>();
		int i;
		for (i = m_position; i < m_glyphInfoList.Glyphs.Count; i++)
		{
			OtfGlyphInfo otfGlyphInfo = m_glyphInfoList.Glyphs[i];
			if ((otfGlyphInfo.Index != 3 && otfGlyphInfo.CharCode != 32) || otfGlyphInfo.unsupportedGlyph)
			{
				list.Add(otfGlyphInfo);
				if (otfGlyphInfo.Characters != null)
				{
					char[] characters = otfGlyphInfo.Characters;
					foreach (char value in characters)
					{
						stringBuilder.Append(value);
					}
				}
				continue;
			}
			if (list.Count == 0)
			{
				list.Add(otfGlyphInfo);
				if (otfGlyphInfo.Characters != null)
				{
					char[] characters = otfGlyphInfo.Characters;
					foreach (char value2 in characters)
					{
						stringBuilder.Append(value2);
					}
				}
			}
			if (i == m_position)
			{
				m_position++;
			}
			else
			{
				m_position = i;
			}
			break;
		}
		if (i == m_glyphInfoList.Glyphs.Count)
		{
			m_position = i;
		}
		text = stringBuilder.ToString();
		return list.ToArray();
	}

	internal OtfGlyphInfo[][] SplitGlyphs(List<OtfGlyphInfo> glyphs)
	{
		List<OtfGlyphInfo[]> list = new List<OtfGlyphInfo[]>();
		List<OtfGlyphInfo> list2 = new List<OtfGlyphInfo>();
		for (int i = 0; i < glyphs.Count; i++)
		{
			OtfGlyphInfo otfGlyphInfo = glyphs[i];
			if (otfGlyphInfo.Index != 3 && otfGlyphInfo.CharCode != 32)
			{
				list2.Add(otfGlyphInfo);
			}
			else if (list2.Count > 0 && (list2.Count != 1 || list2[0].Index != 32))
			{
				list.Add(list2.ToArray());
				list2 = new List<OtfGlyphInfo>();
			}
		}
		if (list2.Count > 0)
		{
			list.Add(list2.ToArray());
		}
		return list.ToArray();
	}

	internal float GetLineWidth(OtfGlyphInfo[] glyphs, PdfTrueTypeFont font, PdfStringFormat format)
	{
		float num = 0f;
		foreach (OtfGlyphInfo otfGlyphInfo in glyphs)
		{
			num += otfGlyphInfo.Width;
		}
		float size = font.Metrics.GetSize(format);
		num *= 0.001f * size;
		return ApplyFormatSettings(glyphs, format, num);
	}

	internal float GetLineWidth(OtfGlyphInfo[] glyphs, PdfTrueTypeFont font, PdfStringFormat format, string text, out float outWordSpace, out float outCharSpace)
	{
		float num = 0f;
		outWordSpace = 0f;
		outCharSpace = 0f;
		foreach (OtfGlyphInfo otfGlyphInfo in glyphs)
		{
			num += otfGlyphInfo.Width;
		}
		float size = font.Metrics.GetSize(format);
		num *= 0.001f * size;
		if (text != null)
		{
			return ApplyFormatSettings(glyphs, format, num, text, out outWordSpace, out outCharSpace);
		}
		return ApplyFormatSettings(glyphs, format, num);
	}

	internal float GetLineWidth(OtfGlyphInfo glyphs, PdfTrueTypeFont font, PdfStringFormat format)
	{
		float width = glyphs.Width;
		float size = font.Metrics.GetSize(format);
		width *= 0.001f * size;
		return ApplyFormatSettings(new OtfGlyphInfo[1] { glyphs }, format, width);
	}

	protected float ApplyFormatSettings(OtfGlyphInfo[] glyphs, PdfStringFormat format, float width, string text, out float outWordSpace, out float outCharSpace)
	{
		outWordSpace = 0f;
		outCharSpace = 0f;
		if (format != null && width > 0f)
		{
			if (format.CharacterSpacing != 0f && text != null)
			{
				outCharSpace += (float)(text.Length - 1) * format.CharacterSpacing;
			}
			if (format.WordSpacing != 0f && text != null)
			{
				char[] spaces = StringTokenizer.Spaces;
				int charsCount = StringTokenizer.GetCharsCount(text, spaces);
				outWordSpace += (float)charsCount * format.WordSpacing;
			}
		}
		return width;
	}

	protected float ApplyFormatSettings(OtfGlyphInfo[] glyphs, PdfStringFormat format, float width)
	{
		float num = width;
		if (format != null && width > 0f)
		{
			if (format.CharacterSpacing != 0f)
			{
				num += (float)(glyphs.Length - 1) * format.CharacterSpacing;
			}
			if (format.WordSpacing != 0f)
			{
				int charsCount = GetCharsCount(glyphs);
				num += (float)charsCount * format.WordSpacing;
			}
		}
		return num;
	}

	protected int GetCharsCount(OtfGlyphInfo[] glyphs)
	{
		int num = 0;
		for (int i = 0; i < glyphs.Length; i++)
		{
			if (glyphs[i].CharCode == 32)
			{
				num++;
			}
		}
		return num;
	}

	internal OtfGlyphInfoList TrimEndSpaces(OtfGlyphInfoList glyphList)
	{
		List<OtfGlyphInfo> glyphs = glyphList.Glyphs;
		int num = glyphList.Glyphs.Count - 1;
		while (num >= 0 && (glyphList.Glyphs[num].Index == 3 || glyphList.Glyphs[num].CharCode == 32))
		{
			glyphs.RemoveAt(glyphs.Count - 1);
			num--;
		}
		return new OtfGlyphInfoList(glyphs, 0, glyphs.Count);
	}

	internal OtfGlyphInfoList TrimStartSpaces(OtfGlyphInfoList glyphList)
	{
		List<OtfGlyphInfo> glyphs = glyphList.Glyphs;
		for (int i = 0; i <= glyphList.Glyphs.Count - 1 && (glyphList.Glyphs[i].Index == 3 || glyphList.Glyphs[i].CharCode == 32); i++)
		{
			glyphs.RemoveAt(0);
		}
		return new OtfGlyphInfoList(glyphs, 0, glyphs.Count);
	}
}
