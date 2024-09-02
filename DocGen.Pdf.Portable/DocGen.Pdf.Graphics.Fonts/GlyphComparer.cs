using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal sealed class GlyphComparer : IComparer<OtfGlyphInfo>
{
	public int Compare(OtfGlyphInfo otg1, OtfGlyphInfo otg2)
	{
		if (!(otg1 is IndicGlyphInfo) || !(otg2 is IndicGlyphInfo))
		{
			throw new InvalidOperationException();
		}
		IndicGlyphInfo indicGlyphInfo = (IndicGlyphInfo)otg1;
		IndicGlyphInfo indicGlyphInfo2 = (IndicGlyphInfo)otg2;
		if (indicGlyphInfo.Position == indicGlyphInfo2.Position)
		{
			return 0;
		}
		if (indicGlyphInfo.Position < indicGlyphInfo2.Position)
		{
			return -1;
		}
		return 1;
	}
}
