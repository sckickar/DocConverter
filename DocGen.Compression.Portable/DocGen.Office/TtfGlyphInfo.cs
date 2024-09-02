using System;

namespace DocGen.Office;

internal struct TtfGlyphInfo : IComparable
{
	public int Index;

	public int Width;

	public int CharCode;

	public bool Empty
	{
		get
		{
			if (Index == Width && Width == CharCode)
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
