using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontLookup : SystemFontTableBase
{
	private ushort[] subTableOffsets;

	private SystemFontSubTable[] subTables;

	public ushort Type { get; private set; }

	public ushort Flags { get; private set; }

	public IEnumerable<SystemFontSubTable> SubTables
	{
		get
		{
			if (subTables == null)
			{
				subTables = new SystemFontSubTable[subTableOffsets.Length];
				for (int i = 0; i < subTableOffsets.Length; i++)
				{
					subTables[i] = ReadSubtable(base.Reader, subTableOffsets[i]);
				}
			}
			return subTables;
		}
	}

	internal static bool IsSupported(ushort type)
	{
		if ((uint)(type - 1) <= 1u || type == 4)
		{
			return true;
		}
		return false;
	}

	public SystemFontLookup(SystemFontOpenTypeFontSourceBase fontFile, ushort type)
		: base(fontFile)
	{
		Type = type;
	}

	private SystemFontSubTable ReadSubtable(SystemFontOpenTypeFontReader reader, ushort offset)
	{
		reader.BeginReadingBlock();
		long offset2 = base.Offset + offset;
		reader.Seek(offset2, SeekOrigin.Begin);
		SystemFontSubTable systemFontSubTable = SystemFontSubTable.ReadSubTable(base.FontSource, reader, Type);
		systemFontSubTable.Offset = offset2;
		reader.EndReadingBlock();
		return systemFontSubTable;
	}

	public SystemFontGlyphsSequence Apply(SystemFontGlyphsSequence glyphIDs)
	{
		SystemFontGlyphsSequence systemFontGlyphsSequence = glyphIDs;
		foreach (SystemFontSubTable subTable in SubTables)
		{
			systemFontGlyphsSequence = subTable.Apply(systemFontGlyphsSequence);
		}
		return systemFontGlyphsSequence;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		Flags = reader.ReadUShort();
		ushort num = reader.ReadUShort();
		subTableOffsets = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			subTableOffsets[i] = reader.ReadUShort();
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(Type);
		writer.WriteUShort(Flags);
		writer.WriteUShort((ushort)subTableOffsets.Length);
		foreach (SystemFontSubTable subTable in SubTables)
		{
			subTable.Write(writer);
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		Flags = reader.ReadUShort();
		ushort num = reader.ReadUShort();
		subTables = new SystemFontSubTable[num];
		for (int i = 0; i < num; i++)
		{
			SystemFontSubTable systemFontSubTable = SystemFontSubTable.ImportSubTable(base.FontSource, reader, Type);
			subTables[i] = systemFontSubTable;
		}
	}
}
