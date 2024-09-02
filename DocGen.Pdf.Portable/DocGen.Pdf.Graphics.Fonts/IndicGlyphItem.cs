namespace DocGen.Pdf.Graphics.Fonts;

internal class IndicGlyphItem
{
	private IndicGlyphInfoList m_glyphList;

	private int m_position;

	internal IndicGlyphInfoList GlyphList
	{
		get
		{
			return m_glyphList;
		}
		set
		{
			m_glyphList = value;
		}
	}

	internal int Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	internal virtual int Length => GlyphList.End - GlyphList.Start;

	internal IndicGlyphItem(IndicGlyphInfoList glyphInfoList, int position)
	{
		GlyphList = glyphInfoList;
		Position = position;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		IndicGlyphItem indicGlyphItem = (IndicGlyphItem)obj;
		if (Length == indicGlyphItem.Length)
		{
			return GlyphList.Equals(indicGlyphItem.GlyphList);
		}
		return false;
	}
}
