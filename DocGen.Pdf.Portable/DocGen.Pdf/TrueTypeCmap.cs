using System.Collections.Generic;

namespace DocGen.Pdf;

internal class TrueTypeCmap : TableBase
{
	private int m_id = 2;

	public FontEncoding[] encodings;

	private Dictionary<FontEncoding, CmapTables> encodingtable;

	private ushort noofSubtable;

	private uint subOffset;

	internal override int Id => m_id;

	public TrueTypeCmap(FontFile2 fontsource)
		: base(fontsource)
	{
	}

	public override void Read(ReadFontArray reader)
	{
		reader.getnextUshort();
		noofSubtable = reader.getnextUshort();
		encodings = new FontEncoding[noofSubtable];
		encodingtable = new Dictionary<FontEncoding, CmapTables>(noofSubtable);
		for (int i = 0; i < noofSubtable; i++)
		{
			FontEncoding fontEncoding = new FontEncoding();
			fontEncoding.ReadEncodingDeatils(reader);
			encodings[i] = fontEncoding;
		}
	}

	public CmapTables GetCmaptable(ushort platformid, ushort encodingid)
	{
		FontEncoding fontEncoding = null;
		for (int i = 0; i < noofSubtable; i++)
		{
			if (encodings[i].PlatformId == platformid && encodings[i].EncodingId == encodingid)
			{
				fontEncoding = encodings[i];
			}
		}
		if (fontEncoding == null)
		{
			return null;
		}
		return GetCmapTable(fontEncoding, base.Reader);
	}

	public CmapTables GetCmapTable(FontEncoding encode, ReadFontArray reader)
	{
		if (!encodingtable.TryGetValue(encode, out CmapTables value))
		{
			reader.Pointer = (int)encode.Offset + base.Offset;
			value = CmapTables.ReadCmapTable(reader);
			encodingtable[encode] = value;
		}
		return value;
	}
}
