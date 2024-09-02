using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontOpenTypeFontSource : SystemFontOpenTypeFontSourceBase
{
	private Dictionary<ushort, SystemFontGlyphData> glyphsData;

	private SystemFontCMap cmap;

	private SystemFontHorizontalMetrics hmtx;

	private SystemFontKerning kern;

	private SystemFontGlyphSubstitution gsub;

	private Dictionary<uint, SystemFontTableRecord> tables;

	private SystemFontIndexToLocation loca;

	private SystemFontMaxProfile maxp;

	private SystemFontHead head;

	private SystemFontHorizontalHeader hhea;

	private SystemFontSystemFontName name;

	private SystemFontPost post;

	private SystemFontCFFFontSource ccf;

	internal override SystemFontOutlines Outlines
	{
		get
		{
			if (!OffsetTable.HasOpenTypeOutlines)
			{
				return SystemFontOutlines.TrueType;
			}
			return SystemFontOutlines.OpenType;
		}
	}

	internal override SystemFontCMap CMap
	{
		get
		{
			if (cmap == null)
			{
				cmap = new SystemFontCMap(this);
				ReadTableData(cmap);
			}
			return cmap;
		}
	}

	internal override SystemFontHorizontalMetrics HMtx
	{
		get
		{
			if (hmtx == null)
			{
				hmtx = new SystemFontHorizontalMetrics(this);
				ReadTableData(hmtx);
			}
			return hmtx;
		}
	}

	internal override SystemFontKerning Kern
	{
		get
		{
			if (kern == null)
			{
				kern = new SystemFontKerning(this);
				ReadTableData(kern);
			}
			return kern;
		}
	}

	internal override SystemFontGlyphSubstitution GSub
	{
		get
		{
			if (gsub == null)
			{
				gsub = new SystemFontGlyphSubstitution(this);
				ReadTableData(gsub);
			}
			return gsub;
		}
	}

	internal override SystemFontHead Head
	{
		get
		{
			if (head == null)
			{
				head = new SystemFontHead(this);
				ReadTableData(head);
			}
			return head;
		}
	}

	internal override SystemFontHorizontalHeader HHea
	{
		get
		{
			if (hhea == null)
			{
				hhea = new SystemFontHorizontalHeader(this);
				ReadTableData(hhea);
			}
			return hhea;
		}
	}

	internal SystemFontPost Post
	{
		get
		{
			if (post == null)
			{
				post = ReadTableData(SystemFontTags.POST_TABLE, SystemFontPost.ReadPostTable);
			}
			return post;
		}
	}

	internal SystemFontSystemFontName Name
	{
		get
		{
			if (name == null)
			{
				name = ReadTableData(SystemFontTags.NAME_TABLE, SystemFontSystemFontName.ReadNameTable);
			}
			return name;
		}
	}

	internal SystemFontIndexToLocation Loca
	{
		get
		{
			if (loca == null)
			{
				loca = new SystemFontIndexToLocation(this);
				ReadTableData(loca);
			}
			return loca;
		}
	}

	internal SystemFontMaxProfile MaxP
	{
		get
		{
			if (maxp == null)
			{
				maxp = new SystemFontMaxProfile(this);
				ReadTableData(maxp);
			}
			return maxp;
		}
	}

	internal Dictionary<uint, SystemFontTableRecord> Tables => tables;

	internal SystemFontOffsetTable OffsetTable { get; private set; }

	internal override ushort GlyphCount => MaxP.NumGlyphs;

	internal long Offset { get; set; }

	public override string FontFamily => Name.FontFamily;

	public override bool IsBold => Head.IsBold;

	public override bool IsItalic => Head.IsItalic;

	internal override SystemFontCFFFontSource CFF
	{
		get
		{
			if (!OffsetTable.HasOpenTypeOutlines)
			{
				return null;
			}
			if (ccf == null)
			{
				ccf = ReadCFFTable();
			}
			return ccf;
		}
	}

	public SystemFontOpenTypeFontSource(SystemFontOpenTypeFontReader reader)
		: base(reader)
	{
		Offset = 0L;
		Initialize();
	}

	private void ReadTableData<T>(T table) where T : SystemFontTrueTypeTableBase
	{
		if (tables.TryGetValue(table.Tag, out SystemFontTableRecord value))
		{
			long offset = Offset + value.Offset;
			base.Reader.BeginReadingBlock();
			base.Reader.Seek(offset, SeekOrigin.Begin);
			table.Read(base.Reader);
			table.Offset = offset;
			base.Reader.EndReadingBlock();
		}
	}

	private T ReadTableData<T>(uint tag, SystemFontReadTableFormatDelegate<T> readTableDelegate) where T : SystemFontTrueTypeTableBase
	{
		if (!tables.TryGetValue(tag, out SystemFontTableRecord value))
		{
			return null;
		}
		base.Reader.BeginReadingBlock();
		long offset = Offset + value.Offset;
		base.Reader.Seek(offset, SeekOrigin.Begin);
		T val = readTableDelegate(this, base.Reader);
		val.Offset = offset;
		base.Reader.EndReadingBlock();
		return val;
	}

	private int GetTableLength(uint tag)
	{
		SystemFontTableRecord systemFontTableRecord = tables[tag];
		int num = (int)(base.Reader.Length - systemFontTableRecord.Offset);
		foreach (SystemFontTableRecord value in tables.Values)
		{
			int num2 = (int)(value.Offset - systemFontTableRecord.Offset);
			if (num2 > 0 && num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	private SystemFontCFFFontSource ReadCFFTable()
	{
		int tableLength = GetTableLength(SystemFontTags.CFF_TABLE);
		byte[] array = new byte[tableLength];
		base.Reader.BeginReadingBlock();
		long offset = Offset + tables[SystemFontTags.CFF_TABLE].Offset;
		base.Reader.Seek(offset, SeekOrigin.Begin);
		base.Reader.Read(array, tableLength);
		base.Reader.EndReadingBlock();
		return new SystemFontCFFFontFile(array).FontSource;
	}

	private void ReadTableRecords()
	{
		tables = new Dictionary<uint, SystemFontTableRecord>();
		for (int i = 0; i < OffsetTable.NumTables; i++)
		{
			SystemFontTableRecord systemFontTableRecord = new SystemFontTableRecord();
			systemFontTableRecord.Read(base.Reader);
			tables[systemFontTableRecord.Tag] = systemFontTableRecord;
		}
	}

	private void Initialize()
	{
		OffsetTable = new SystemFontOffsetTable();
		OffsetTable.Read(base.Reader);
		ReadTableRecords();
		glyphsData = new Dictionary<ushort, SystemFontGlyphData>();
	}

	internal override SystemFontGlyphData GetGlyphData(ushort glyphIndex)
	{
		if (!glyphsData.TryGetValue(glyphIndex, out SystemFontGlyphData value))
		{
			long offset = Loca.GetOffset(glyphIndex);
			tables.TryGetValue(SystemFontTags.GLYF_TABLE, out SystemFontTableRecord value2);
			if (offset == -1 || value2 == null || offset >= value2.Offset + value2.Length)
			{
				return new SystemFontGlyphData(this, glyphIndex);
			}
			offset += value2.Offset;
			base.Reader.BeginReadingBlock();
			base.Reader.Seek(offset, SeekOrigin.Begin);
			value = SystemFontGlyphData.ReadGlyf(this, glyphIndex);
			base.Reader.EndReadingBlock();
			glyphsData[glyphIndex] = value;
		}
		return value;
	}
}
