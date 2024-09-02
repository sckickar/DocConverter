namespace DocGen.Pdf;

internal class Cmap0 : CmapTables
{
	private ushort m_firstcode;

	public byte[] glyphIdArray;

	public override ushort FirstCode => m_firstcode;

	public override ushort GetGlyphId(ushort charCode)
	{
		if (charCode >= 0 && charCode < glyphIdArray.Length)
		{
			return glyphIdArray[charCode];
		}
		return 0;
	}

	public override void Read(ReadFontArray reader)
	{
		reader.getnextUshort();
		reader.getnextUshort();
		glyphIdArray = new byte[256];
		for (int i = 0; i < 256; i++)
		{
			glyphIdArray[i] = reader.getnextbyte();
		}
	}
}
