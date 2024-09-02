namespace DocGen.Pdf;

internal class Cmap6 : CmapTables
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

	public override void Read(ReadFontArray reader)
	{
		reader.getnextUshort();
		reader.getnextUshort();
		firstCode = reader.getnextUshort();
		ushort num = reader.getnextUshort();
		glyphIdArray = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			glyphIdArray[i] = reader.getnextUshort();
		}
	}
}
