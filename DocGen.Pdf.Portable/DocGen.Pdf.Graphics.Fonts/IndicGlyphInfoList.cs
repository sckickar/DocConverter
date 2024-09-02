using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal class IndicGlyphInfoList : OtfGlyphInfoList
{
	private int m_glyphInfoStart;

	private int m_glyphInfoEnd;

	internal int GlyphInfoStart
	{
		get
		{
			return m_glyphInfoStart;
		}
		set
		{
			m_glyphInfoStart = value;
		}
	}

	internal int GlyphInfoEnd
	{
		get
		{
			return m_glyphInfoEnd;
		}
		set
		{
			m_glyphInfoEnd = value;
		}
	}

	internal IndicGlyphInfo this[int index]
	{
		get
		{
			if (!(base.Glyphs[index] is IndicGlyphInfo))
			{
				base.Glyphs[index] = new IndicGlyphInfo(base.Glyphs[index]);
			}
			return base.Glyphs[index] as IndicGlyphInfo;
		}
	}

	internal IndicGlyphInfoList(OtfGlyphInfoList glyphInfoList, int start, int end, string text)
		: base(glyphInfoList, start, end)
	{
		List<OtfGlyphInfo> list = new List<OtfGlyphInfo>();
		foreach (OtfGlyphInfo glyph in base.Glyphs)
		{
			list.Add((glyph is IndicGlyphInfo) ? glyph : new IndicGlyphInfo(glyph));
		}
		SetGlyphs(list);
		GlyphInfoStart = start;
		GlyphInfoEnd = end;
		base.Text = new List<string>(base.Glyphs.Count);
		List<string> list2 = new List<string> { text };
		for (int i = 0; i < base.Glyphs.Count; i++)
		{
			base.Text.Add(list2.ToString());
		}
	}

	internal void DoOrder()
	{
		Order(base.Glyphs, 0, base.Glyphs.Count - 1);
	}

	private void Order<T>(List<T> glyphList, int l, int r)
	{
		if (r > l)
		{
			int num = (r + l) / 2;
			Order(glyphList, l, num);
			Order(glyphList, num + 1, r);
			Order(glyphList, l, num + 1, r, new GlyphComparer() as IComparer<T>);
		}
	}

	private void Order<T>(List<T> glyphList, int l, int m, int r, IComparer<T> comparer)
	{
		int num = m - 1;
		int num2 = r - l + 1;
		List<T> list = new List<T>(num2);
		while (l <= num && m <= r)
		{
			if (comparer.Compare(glyphList[l], glyphList[m]) <= 0)
			{
				list.Add(glyphList[l++]);
			}
			else
			{
				list.Add(glyphList[m++]);
			}
		}
		while (l <= num)
		{
			list.Add(glyphList[l++]);
		}
		while (m <= r)
		{
			list.Add(glyphList[m++]);
		}
		for (int num3 = num2 - 1; num3 >= 0; num3--)
		{
			glyphList[r--] = list[num3];
		}
	}

	internal virtual string GetText()
	{
		if (base.Text.GetEnumerator().MoveNext())
		{
			return base.Text.GetEnumerator().Current;
		}
		return null;
	}

	internal override void CombineAlternateGlyphs(OtfTable table, int flag, int length, int glyphIndex)
	{
		IndicGlyphInfo indicGlyphInfo = this[base.Index];
		base.CombineAlternateGlyphs(table, flag, length, glyphIndex);
		base.Glyphs[base.Index] = new IndicGlyphInfo(base.Glyphs[base.Index], indicGlyphInfo.Group, indicGlyphInfo.Position, indicGlyphInfo.Mask, substitute: true, ligate: true);
	}

	internal override void CombineAlternateGlyphs(OtfTable table, int glyphIndex)
	{
		IndicGlyphInfo indicGlyphInfo = this[base.Index];
		base.CombineAlternateGlyphs(table, glyphIndex);
		base.Glyphs[base.Index] = new IndicGlyphInfo(base.Glyphs[base.Index], indicGlyphInfo.Group, indicGlyphInfo.Position, indicGlyphInfo.Mask, substitute: true, ligate: false);
	}

	internal override void CombineAlternateGlyphs(OtfTable table, int[] glyphIndexes)
	{
		IndicGlyphInfo indicGlyphInfo = this[base.Index];
		base.CombineAlternateGlyphs(table, glyphIndexes);
		for (int i = 0; i < glyphIndexes.Length; i++)
		{
			if (base.Index + i < base.Glyphs.Count)
			{
				base.Glyphs[base.Index + i] = new IndicGlyphInfo(base.Glyphs[base.Index + i], indicGlyphInfo.Group, indicGlyphInfo.Position, indicGlyphInfo.Mask, substitute: true, ligate: false);
			}
		}
	}

	internal new OtfGlyphInfoList SubSet(int start, int end)
	{
		OtfGlyphInfoList otfGlyphInfoList = base.SubSet(start, end);
		return new IndicGlyphInfoList(otfGlyphInfoList, otfGlyphInfoList.Start, otfGlyphInfoList.End, GetText())
		{
			GlyphInfoStart = GlyphInfoStart,
			GlyphInfoEnd = GlyphInfoEnd
		};
	}

	internal virtual void Rearrange(int d, int s, int count)
	{
		List<OtfGlyphInfo> list = new List<OtfGlyphInfo>();
		for (int i = 0; i < count; i++)
		{
			list.Add(base.Glyphs[s + i]);
		}
		for (int j = 0; j < count; j++)
		{
			base.Glyphs[d + j] = list[j];
		}
	}

	public override bool Equals(object obj)
	{
		bool flag = false;
		if (this == obj)
		{
			return true;
		}
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		IndicGlyphInfoList indicGlyphInfoList = (IndicGlyphInfoList)obj;
		if (base.Glyphs.Count == indicGlyphInfoList.Glyphs.Count)
		{
			flag = true;
			for (int i = 0; i < base.Glyphs.Count; i++)
			{
				if (!base.Glyphs[i].CharCode.Equals(indicGlyphInfoList.Glyphs[i].CharCode))
				{
					flag = false;
					break;
				}
			}
		}
		else
		{
			flag = false;
		}
		return flag;
	}
}
