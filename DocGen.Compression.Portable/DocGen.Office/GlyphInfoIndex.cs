namespace DocGen.Office;

internal class GlyphInfoIndex
{
	private OtfGlyphInfoList m_glyphInfoList;

	private OtfGlyphInfo m_glyphInfo;

	private int m_index;

	internal OtfGlyphInfoList GlyphInfoList
	{
		get
		{
			return m_glyphInfoList;
		}
		set
		{
			m_glyphInfoList = value;
		}
	}

	internal OtfGlyphInfo GlyphInfo
	{
		get
		{
			return m_glyphInfo;
		}
		set
		{
			m_glyphInfo = value;
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

	internal virtual void MoveNext(OtfTable table, int flag)
	{
		GlyphInfo = null;
		while (++Index < GlyphInfoList.End)
		{
			OtfGlyphInfo otfGlyphInfo = GlyphInfoList.Glyphs[Index];
			if (!table.GDEFTable.IsSkip(otfGlyphInfo.Index, flag))
			{
				GlyphInfo = otfGlyphInfo;
				break;
			}
		}
	}

	internal virtual void MovePrevious(OtfTable table, int flag)
	{
		GlyphInfo = null;
		while (--Index >= GlyphInfoList.Start)
		{
			OtfGlyphInfo otfGlyphInfo = GlyphInfoList.Glyphs[Index];
			if (!table.GDEFTable.IsSkip(otfGlyphInfo.Index, flag))
			{
				GlyphInfo = otfGlyphInfo;
				break;
			}
		}
	}
}
