using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontLigatureSet : SystemFontTableBase
{
	private ushort[] ligatureOffsets;

	private SystemFontLigature[] ligatures;

	public SystemFontLigature[] Ligatures
	{
		get
		{
			if (ligatures == null)
			{
				ligatures = new SystemFontLigature[ligatureOffsets.Length];
				for (int i = 0; i < ligatures.Length; i++)
				{
					ligatures[i] = ReadLigature(base.Reader, ligatureOffsets[i]);
				}
			}
			return ligatures;
		}
	}

	public SystemFontLigatureSet(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	private SystemFontLigature ReadLigature(SystemFontOpenTypeFontReader reader, ushort offset)
	{
		reader.BeginReadingBlock();
		reader.Seek(base.Offset + offset, SeekOrigin.Begin);
		SystemFontLigature systemFontLigature = new SystemFontLigature(base.FontSource);
		systemFontLigature.Read(reader);
		reader.EndReadingBlock();
		return systemFontLigature;
	}

	public SystemFontLigature FindLigature(SystemFontGlyphsSequence glyphIDs, int startIndex)
	{
		SystemFontLigature[] array = Ligatures;
		foreach (SystemFontLigature systemFontLigature in array)
		{
			if (systemFontLigature.IsMatch(glyphIDs, startIndex + 1))
			{
				return systemFontLigature;
			}
		}
		return null;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		ligatureOffsets = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			ligatureOffsets[i] = reader.ReadUShort();
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort((ushort)Ligatures.Length);
		for (int i = 0; i < Ligatures.Length; i++)
		{
			Ligatures[i].Write(writer);
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		ligatures = new SystemFontLigature[num];
		for (int i = 0; i < num; i++)
		{
			SystemFontLigature systemFontLigature = new SystemFontLigature(base.FontSource);
			systemFontLigature.Import(reader);
			ligatures[i] = systemFontLigature;
		}
	}
}
