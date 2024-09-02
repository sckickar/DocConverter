using System;

namespace DocGen.Pdf.Graphics.Fonts;

internal struct TtfGlyphInfo : IComparable
{
	public int Index;

	public float Width;

	public int CharCode;

	public bool Empty
	{
		get
		{
			if ((float)Index == Width && Width == (float)CharCode)
			{
				return CharCode == 0;
			}
			return false;
		}
	}

	public int CompareTo(object obj)
	{
		TtfGlyphInfo ttfGlyphInfo = (TtfGlyphInfo)obj;
		return Index - ttfGlyphInfo.Index;
	}
}
