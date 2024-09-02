namespace DocGen.Pdf.Parsing;

internal class SystemFontCMapFormat6Table : SystemFontCMapTable
{
	private ushort firstCode;

	private ushort[] glyphIdArray;

	public override ushort FirstCode => firstCode;

	public override ushort GetGlyphId(ushort charCode)
	{
		if (firstCode <= charCode && charCode < firstCode + glyphIdArray.Length)
		{
			return glyphIdArray[charCode - firstCode];
		}
		return 0;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadUShort();
		reader.ReadUShort();
		firstCode = reader.ReadUShort();
		ushort num = reader.ReadUShort();
		glyphIdArray = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			glyphIdArray[i] = reader.ReadUShort();
		}
	}

	public override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(6);
		writer.WriteUShort(firstCode);
		writer.WriteUShort((ushort)glyphIdArray.Length);
		for (int i = 0; i < glyphIdArray.Length; i++)
		{
			writer.WriteUShort(glyphIdArray[i]);
		}
	}

	public override void Import(SystemFontOpenTypeFontReader reader)
	{
		firstCode = reader.ReadUShort();
		ushort num = reader.ReadUShort();
		glyphIdArray = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			glyphIdArray[i] = reader.ReadUShort();
		}
	}
}
