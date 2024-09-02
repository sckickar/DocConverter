namespace DocGen.Office;

internal abstract class LookupSubTable : BaseTable
{
	internal LookupSubTable(OtfTable table, int flag)
		: base(table, flag)
	{
	}

	internal override SubsetTable GetTable(OtfGlyphInfoList glyphList)
	{
		if (glyphList.Index < glyphList.End)
		{
			OtfGlyphInfo otfGlyphInfo = glyphList.Glyphs[glyphList.Index];
			foreach (SubsetTable subsetTable in GetSubsetTables(otfGlyphInfo.Index))
			{
				int num = Match(glyphList, subsetTable);
				if (num != -1 && Lookup(glyphList, subsetTable, num) && BackTrack(glyphList, subsetTable))
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

	internal bool Lookup(OtfGlyphInfoList glyphList, SubsetTable subsetTable, int index)
	{
		GlyphInfoIndex glyphInfoIndex = new GlyphInfoIndex();
		glyphInfoIndex.GlyphInfoList = glyphList;
		glyphInfoIndex.Index = index;
		int i;
		for (i = 0; i < subsetTable.LookupLength; i++)
		{
			glyphInfoIndex.MoveNext(base.OTFontTable, base.LookupID);
			if (glyphInfoIndex.GlyphInfo == null || !subsetTable.IsLookup(glyphInfoIndex.GlyphInfo.Index, i))
			{
				break;
			}
		}
		return i == subsetTable.LookupLength;
	}

	internal bool BackTrack(OtfGlyphInfoList glyphList, SubsetTable subsetTable)
	{
		GlyphInfoIndex glyphInfoIndex = new GlyphInfoIndex();
		glyphInfoIndex.GlyphInfoList = glyphList;
		glyphInfoIndex.Index = glyphList.Index;
		int i;
		for (i = 0; i < subsetTable.BTCLength; i++)
		{
			glyphInfoIndex.MovePrevious(base.OTFontTable, base.LookupID);
			if (glyphInfoIndex.GlyphInfo == null || !subsetTable.IsBackTrack(glyphInfoIndex.GlyphInfo.Index, i))
			{
				break;
			}
		}
		return i == subsetTable.BTCLength;
	}
}
