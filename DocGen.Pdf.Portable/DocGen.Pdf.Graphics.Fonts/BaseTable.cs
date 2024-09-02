using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal abstract class BaseTable
{
	private OtfTable m_otfontTable;

	private int m_lookupID;

	internal OtfTable OTFontTable => m_otfontTable;

	internal int LookupID => m_lookupID;

	internal BaseTable(OtfTable openReader, int lookupFlag)
	{
		m_otfontTable = openReader;
		m_lookupID = lookupFlag;
	}

	internal virtual SubsetTable GetTable(OtfGlyphInfoList glyphList)
	{
		if (glyphList.Index < glyphList.End)
		{
			OtfGlyphInfo otfGlyphInfo = glyphList.Glyphs[glyphList.Index];
			foreach (SubsetTable subsetTable in GetSubsetTables(otfGlyphInfo.Index))
			{
				int num = Match(glyphList, subsetTable);
				if (num != -1)
				{
					glyphList.Start = glyphList.Index;
					glyphList.End = num + 1;
					return subsetTable;
				}
			}
			return null;
		}
		return null;
	}

	internal virtual int Match(OtfGlyphInfoList glyphList, SubsetTable subsetTable)
	{
		GlyphInfoIndex glyphInfoIndex = new GlyphInfoIndex();
		glyphInfoIndex.GlyphInfoList = glyphList;
		glyphInfoIndex.Index = glyphList.Index;
		int i;
		for (i = 1; i < subsetTable.Length; i++)
		{
			glyphInfoIndex.MoveNext(OTFontTable, LookupID);
			if (glyphInfoIndex.GlyphInfo == null || !subsetTable.Match(glyphInfoIndex.GlyphInfo.Index, i))
			{
				break;
			}
		}
		if (i == subsetTable.Length)
		{
			return glyphInfoIndex.Index;
		}
		return -1;
	}

	internal abstract IList<SubsetTable> GetSubsetTables(int index);
}
