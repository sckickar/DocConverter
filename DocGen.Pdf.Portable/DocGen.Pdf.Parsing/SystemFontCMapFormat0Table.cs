using System;

namespace DocGen.Pdf.Parsing;

internal class SystemFontCMapFormat0Table : SystemFontCMapTable
{
	private const int GLYPH_IDS = 256;

	private byte[] glyphIdArray;

	public override ushort FirstCode
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override ushort GetGlyphId(ushort charCode)
	{
		if (charCode >= 0 && charCode < glyphIdArray.Length)
		{
			return glyphIdArray[charCode];
		}
		return 0;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadUShort();
		reader.ReadUShort();
		glyphIdArray = new byte[256];
		for (int i = 0; i < 256; i++)
		{
			glyphIdArray[i] = reader.Read();
		}
	}

	public override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(0);
		for (int i = 0; i < 256; i++)
		{
			writer.Write(glyphIdArray[i]);
		}
	}

	public override void Import(SystemFontOpenTypeFontReader reader)
	{
		glyphIdArray = new byte[256];
		for (int i = 0; i < 256; i++)
		{
			glyphIdArray[i] = reader.Read();
		}
	}
}
