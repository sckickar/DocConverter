namespace DocGen.Pdf;

internal abstract class CmapTables
{
	private ushort m_firstcode;

	public abstract ushort FirstCode { get; }

	public static CmapTables ReadCmapTable(ReadFontArray reader)
	{
		switch (reader.getnextUint16())
		{
		case 4:
		{
			Cmap4 cmap3 = new Cmap4();
			cmap3.Read(reader);
			return cmap3;
		}
		case 6:
		{
			Cmap6 cmap2 = new Cmap6();
			cmap2.Read(reader);
			return cmap2;
		}
		default:
		{
			Cmap0 cmap = new Cmap0();
			cmap.Read(reader);
			return cmap;
		}
		}
	}

	public abstract ushort GetGlyphId(ushort charCode);

	public abstract void Read(ReadFontArray reader);
}
