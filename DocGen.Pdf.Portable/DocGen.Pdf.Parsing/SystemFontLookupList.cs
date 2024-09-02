using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontLookupList : SystemFontTableBase
{
	private ushort[] lookupOffsets;

	private SystemFontLookup[] lookups;

	public SystemFontLookupList(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	private SystemFontLookup ReadLookup(SystemFontOpenTypeFontReader reader, ushort offset)
	{
		reader.BeginReadingBlock();
		long offset2 = base.Offset + offset;
		reader.Seek(offset2, SeekOrigin.Begin);
		ushort type = reader.ReadUShort();
		if (!SystemFontLookup.IsSupported(type))
		{
			return null;
		}
		SystemFontLookup systemFontLookup = new SystemFontLookup(base.FontSource, type);
		systemFontLookup.Offset = offset2;
		systemFontLookup.Read(reader);
		reader.EndReadingBlock();
		return systemFontLookup;
	}

	public SystemFontLookup GetLookup(ushort index)
	{
		if (lookups[index] == null && lookupOffsets != null)
		{
			lookups[index] = ReadLookup(base.Reader, lookupOffsets[index]);
		}
		return lookups[index];
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		lookupOffsets = new ushort[num];
		lookups = new SystemFontLookup[num];
		for (int i = 0; i < num; i++)
		{
			lookupOffsets[i] = reader.ReadUShort();
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort((ushort)lookupOffsets.Length);
		for (ushort num = 0; num < lookupOffsets.Length; num++)
		{
			SystemFontLookup lookup = GetLookup(num);
			if (lookup == null)
			{
				writer.WriteUShort(SystemFontTags.NULL_TYPE);
			}
			else
			{
				lookup.Write(writer);
			}
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		lookups = new SystemFontLookup[num];
		for (int i = 0; i < num; i++)
		{
			ushort num2 = reader.ReadUShort();
			if (num2 != SystemFontTags.NULL_TYPE)
			{
				new SystemFontLookup(base.FontSource, num2).Import(reader);
			}
		}
	}
}
