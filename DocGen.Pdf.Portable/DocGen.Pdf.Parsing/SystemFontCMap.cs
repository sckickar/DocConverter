using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontCMap : SystemFontTrueTypeTableBase
{
	private const ushort UNICODE_PLATFORM_ID = 0;

	private const ushort WINDOWS_PLATFORM_ID = 3;

	private const ushort DEFAULT_SEMANTIC_ID = 0;

	private const ushort SYMBOL_ENCODING_ID = 0;

	private const ushort UNICODE_ENCODING_ID = 1;

	internal const ushort MISSING_GLYPH_ID = 0;

	private SystemFontEncodingRecord[] encodings;

	private Dictionary<SystemFontEncodingRecord, SystemFontCMapTable> tables;

	private bool isInitialized;

	private SystemFontCMapTable encoding;

	internal override uint Tag => SystemFontTags.CMAP_TABLE;

	public SystemFontCMap(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	private void Initialize()
	{
		if (!isInitialized)
		{
			encoding = GetCMapTable(3, 1);
			isInitialized = true;
		}
	}

	private SystemFontCMapTable GetCMapTable(SystemFontOpenTypeFontReader reader, SystemFontEncodingRecord record)
	{
		if (!tables.TryGetValue(record, out SystemFontCMapTable value))
		{
			reader.BeginReadingBlock();
			reader.Seek(base.Offset + record.Offset, SeekOrigin.Begin);
			value = SystemFontCMapTable.ReadCMapTable(reader);
			reader.EndReadingBlock();
			tables[record] = value;
		}
		return value;
	}

	public SystemFontCMapTable GetCMapTable(ushort platformId, ushort encodingId)
	{
		if (encodings == null)
		{
			return null;
		}
		SystemFontEncodingRecord systemFontEncodingRecord = SystemFontEnumerable.FirstOrDefault(encodings, (SystemFontEncodingRecord e) => e.PlatformId == platformId && e.EncodingId == encodingId);
		if (systemFontEncodingRecord == null)
		{
			return null;
		}
		return GetCMapTable(base.Reader, systemFontEncodingRecord);
	}

	public ushort GetGlyphId(ushort unicode)
	{
		Initialize();
		if (encoding == null)
		{
			return 0;
		}
		return encoding.GetGlyphId(unicode);
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadUShort();
		ushort num = reader.ReadUShort();
		encodings = new SystemFontEncodingRecord[num];
		tables = new Dictionary<SystemFontEncodingRecord, SystemFontCMapTable>(num);
		for (int i = 0; i < num; i++)
		{
			SystemFontEncodingRecord systemFontEncodingRecord = new SystemFontEncodingRecord();
			systemFontEncodingRecord.Read(reader);
			encodings[i] = systemFontEncodingRecord;
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		Initialize();
		encoding.Write(writer);
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		encoding = SystemFontCMapTable.ImportCMapTable(reader);
		isInitialized = true;
	}
}
