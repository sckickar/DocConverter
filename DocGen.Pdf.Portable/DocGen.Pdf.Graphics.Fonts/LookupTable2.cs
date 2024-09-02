using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal class LookupTable2 : LookupTable
{
	private IList<LookupTable> m_records = new List<LookupTable>();

	internal LookupTable2(OtfTable table, int flag, int[] offsets)
		: base(table, flag, offsets)
	{
		ReadSubTables();
	}

	internal override bool ReplaceGlyph(OtfGlyphInfoList glyphList)
	{
		if (glyphList.Index >= glyphList.End)
		{
			return false;
		}
		if (base.OpenTypeFontTable.GDEFTable.IsSkip(glyphList.Glyphs[glyphList.Index].Index, base.Flag))
		{
			glyphList.Index++;
			return false;
		}
		foreach (LookupTable record in m_records)
		{
			if (record.ReplaceGlyph(glyphList))
			{
				return true;
			}
		}
		int index = glyphList.Index + 1;
		glyphList.Index = index;
		return false;
	}

	internal override void ReadSubTable(int offset)
	{
		base.OpenTypeFontTable.Reader.Seek(offset);
		switch (base.OpenTypeFontTable.Reader.ReadInt16())
		{
		case 1:
			m_records.Add(new GPOSTableFormat(base.OpenTypeFontTable, base.Flag, offset, GPOSTableType.Format1));
			break;
		case 2:
			m_records.Add(new GPOSTableFormat(base.OpenTypeFontTable, base.Flag, offset, GPOSTableType.Format2));
			break;
		}
	}
}
