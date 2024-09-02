using System;
using System.Collections.Generic;
using System.Text;

namespace DocGen.Office;

internal class OtfGlyphInfoList
{
	private List<OtfGlyphInfo> m_glyphs;

	private int m_start;

	private int m_end;

	private int m_index;

	private List<string> m_text = new List<string>();

	private bool m_isThaiShape;

	internal List<string> Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	internal List<OtfGlyphInfo> Glyphs
	{
		get
		{
			if (m_glyphs == null)
			{
				m_glyphs = new List<OtfGlyphInfo>();
			}
			return m_glyphs;
		}
		set
		{
			m_glyphs = value;
		}
	}

	internal int Start
	{
		get
		{
			return m_start;
		}
		set
		{
			m_start = value;
		}
	}

	internal int End
	{
		get
		{
			return m_end;
		}
		set
		{
			m_end = value;
		}
	}

	internal int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	internal bool IsThaiShaping
	{
		get
		{
			return m_isThaiShape;
		}
		set
		{
			m_isThaiShape = value;
		}
	}

	internal OtfGlyphInfoList(List<OtfGlyphInfo> glyphs, int start, int end)
	{
		Glyphs = glyphs;
		Start = start;
		End = end;
	}

	internal OtfGlyphInfoList(OtfGlyphInfo[] glyphs, int start, int end)
	{
		Glyphs = new List<OtfGlyphInfo>();
		foreach (OtfGlyphInfo item in glyphs)
		{
			Glyphs.Add(item);
		}
		Start = start;
		End = end;
	}

	internal OtfGlyphInfoList()
	{
	}

	internal OtfGlyphInfoList(List<OtfGlyphInfo> glyphs)
	{
		Glyphs = glyphs;
		Start = 0;
		End = glyphs.Count;
	}

	internal OtfGlyphInfoList(OtfGlyphInfoList glyphList, int start, int end)
	{
		Glyphs = glyphList.Glyphs.GetRange(start, end - start);
		if (glyphList.Text.Count > 0)
		{
			Text = glyphList.Text.GetRange(start, end - start);
		}
		Start = 0;
		End = end - start;
		Index = glyphList.Index - start;
	}

	internal bool HasYPlacement()
	{
		bool result = false;
		if (Glyphs != null && Glyphs.Count > 0)
		{
			foreach (OtfGlyphInfo glyph in Glyphs)
			{
				if (glyph.leadingY != 0)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	internal void SetGlyphs(List<OtfGlyphInfo> glyphs)
	{
		Glyphs = glyphs;
		Start = 0;
		End = glyphs.Count;
		Text = null;
	}

	internal virtual void CombineAlternateGlyphs(OtfTable table, int[] glyphs)
	{
		int index = glyphs[0];
		OtfGlyphInfo glyph = table.GetGlyph(index);
		Glyphs[Index] = glyph;
		if (glyphs.Length > 1)
		{
			IList<OtfGlyphInfo> list = new List<OtfGlyphInfo>(glyphs.Length - 1);
			for (int i = 1; i < glyphs.Length; i++)
			{
				index = glyphs[i];
				glyph = table.GetGlyph(index);
				list.Add(glyph);
			}
			InsertGlyphs(Index + 1, list);
			Index += glyphs.Length - 1;
			End += glyphs.Length - 1;
		}
		if (!IsThaiShaping || glyphs.Length <= 1)
		{
			return;
		}
		int num = Index - 1 + 2;
		int num2 = num - 2;
		while (num2 > 0 && ThaiToneMark(Glyphs[num2 - 1].CharCode))
		{
			num2--;
		}
		if (num2 + 2 >= num)
		{
			return;
		}
		OtfGlyphInfo glyph2 = Glyphs[num - 2];
		MoveGlyph(num2 + 1, num2, num - num2 - 2);
		Set(num2, glyph2);
		StringBuilder stringBuilder = new StringBuilder();
		for (int j = num2 + 1; j <= num - 2; j++)
		{
			if (j <= Glyphs.Count - 1)
			{
				stringBuilder.Append(char.ConvertFromUtf32(Glyphs[j].CharCode));
			}
		}
		stringBuilder.Append("ำ");
		SetText(num2, num, stringBuilder.ToString());
	}

	internal virtual void CombineAlternateGlyphs(OtfTable table, int flag, int length, int glyphIndex)
	{
		GlyphInfoIndex glyphInfoIndex = new GlyphInfoIndex();
		glyphInfoIndex.GlyphInfoList = this;
		glyphInfoIndex.Index = Index;
		StringBuilder stringBuilder = new StringBuilder();
		OtfGlyphInfo otfGlyphInfo = Glyphs[Index];
		if (otfGlyphInfo.Characters != null)
		{
			stringBuilder.Append(otfGlyphInfo.Characters);
		}
		else if (otfGlyphInfo.CharCode > -1)
		{
			stringBuilder.Append(char.ConvertFromUtf32(otfGlyphInfo.CharCode));
		}
		for (int i = 0; i < length; i++)
		{
			glyphInfoIndex.MoveNext(table, flag);
			otfGlyphInfo = Glyphs[glyphInfoIndex.Index];
			if (otfGlyphInfo.Characters != null)
			{
				stringBuilder.Append(otfGlyphInfo.Characters);
			}
			else if (otfGlyphInfo.CharCode > -1)
			{
				stringBuilder.Append(char.ConvertFromUtf32(otfGlyphInfo.CharCode));
			}
			RemoveGlyph(glyphInfoIndex.Index--);
		}
		char[] array = new char[stringBuilder.Length];
		Array.Copy(stringBuilder.ToString().ToCharArray(), 0, array, 0, stringBuilder.Length);
		OtfGlyphInfo otfGlyphInfo2 = table.GetGlyph(glyphIndex);
		if (IsThaiShaping && otfGlyphInfo2.CharCode == 0)
		{
			otfGlyphInfo2 = new OtfGlyphInfo(-1, glyphIndex, 0f);
		}
		otfGlyphInfo2.Characters = array;
		Glyphs[Index] = otfGlyphInfo2;
		End -= length;
	}

	private void RemoveGlyph(int index)
	{
		Glyphs.RemoveAt(index);
		if (Text.Count > 0)
		{
			Text.RemoveAt(index);
		}
	}

	internal virtual void CombineAlternateGlyphs(OtfTable table, int glyphIndex)
	{
		OtfGlyphInfo otfGlyphInfo = Glyphs[Index];
		OtfGlyphInfo otfGlyphInfo2 = table.GetGlyph(glyphIndex);
		if (otfGlyphInfo.Characters != null)
		{
			if (IsThaiShaping && otfGlyphInfo2.CharCode == 0)
			{
				otfGlyphInfo2 = new OtfGlyphInfo(-1, glyphIndex, 0f);
			}
			otfGlyphInfo2.Characters = otfGlyphInfo.Characters;
		}
		else if (otfGlyphInfo2.CharCode > -1)
		{
			otfGlyphInfo2.Characters = char.ConvertFromUtf32(otfGlyphInfo2.CharCode).ToCharArray();
		}
		else if (otfGlyphInfo.CharCode > -1)
		{
			otfGlyphInfo2.Characters = char.ConvertFromUtf32(otfGlyphInfo.CharCode).ToCharArray();
		}
		Glyphs[Index] = otfGlyphInfo2;
	}

	internal virtual OtfGlyphInfo Set(int index, OtfGlyphInfo glyph)
	{
		return Glyphs[index] = glyph;
	}

	internal virtual OtfGlyphInfoList SubSet(int start, int end)
	{
		return new OtfGlyphInfoList
		{
			Start = 0,
			End = end - start,
			Glyphs = Glyphs.GetRange(start, end - start),
			Text = ((Text.Count > 0) ? new List<string>(Text.GetRange(start, end - start)) : new List<string>())
		};
	}

	internal virtual void ReplaceContent(OtfGlyphInfoList glyphList)
	{
		Glyphs.Clear();
		for (int i = 0; i < glyphList.Glyphs.Count; i++)
		{
			Glyphs.Add(glyphList.Glyphs[i]);
		}
		if (Text.Count > 0)
		{
			Text.Clear();
		}
		if (glyphList.Text != null)
		{
			if (Text == null)
			{
				Text = new List<string>();
			}
			for (int j = 0; j < glyphList.Text.Count; j++)
			{
				Text.Add(glyphList.Text[j]);
			}
		}
		Start = glyphList.Start;
		End = glyphList.End;
	}

	internal virtual void SetText(int start, int end, string text)
	{
		if (Text.Count == 0)
		{
			Text = new List<string>(Glyphs.Count);
			for (int i = 0; i < Glyphs.Count; i++)
			{
				Text.Add(null);
			}
		}
		for (int j = start; j < end; j++)
		{
			Text[j] = text;
		}
	}

	private void InsertGlyphs(int index, IList<OtfGlyphInfo> glyphs)
	{
		for (int num = glyphs.Count - 1; num >= 0; num--)
		{
			Glyphs.Insert(index, glyphs[num]);
		}
		if (Text.Count != 0)
		{
			for (int i = 0; i < glyphs.Count; i++)
			{
				Text.Insert(index, null);
			}
		}
	}

	private void MoveGlyph(int end, int start, int count)
	{
		IList<OtfGlyphInfo> list = new List<OtfGlyphInfo>(count);
		for (int i = 0; i < count; i++)
		{
			list.Add(Glyphs[start + i]);
		}
		for (int j = 0; j < count; j++)
		{
			Glyphs[end + j] = list[j];
		}
	}

	private bool ThaiToneMark(int charcode)
	{
		return ThaiGlyphRanges(charcode & -129, 3636, 3639, 3655, 3662, 3633, 3633);
	}

	private bool ThaiGlyphRanges(int charcode, int lowest1, int heighest1, int lowest2, int heighest2, int lowest3, int heighest3)
	{
		if (!ThaiGlyphRange(charcode, lowest1, heighest1) && !ThaiGlyphRange(charcode, lowest2, heighest2))
		{
			return ThaiGlyphRange(charcode, lowest3, heighest3);
		}
		return true;
	}

	private bool ThaiGlyphRange(int charcode, int low, int high)
	{
		if (charcode >= low)
		{
			return charcode <= high;
		}
		return false;
	}
}
