namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontCMapTable
{
	public abstract ushort FirstCode { get; }

	internal static SystemFontCMapTable ReadCMapTable(SystemFontOpenTypeFontReader reader)
	{
		switch (reader.ReadUShort())
		{
		case 4:
		{
			SystemFontCMapFormat4Table systemFontCMapFormat4Table = new SystemFontCMapFormat4Table();
			systemFontCMapFormat4Table.Read(reader);
			return systemFontCMapFormat4Table;
		}
		case 6:
		{
			SystemFontCMapFormat6Table systemFontCMapFormat6Table = new SystemFontCMapFormat6Table();
			systemFontCMapFormat6Table.Read(reader);
			return systemFontCMapFormat6Table;
		}
		default:
			return null;
		case 0:
		{
			SystemFontCMapFormat0Table systemFontCMapFormat0Table = new SystemFontCMapFormat0Table();
			systemFontCMapFormat0Table.Read(reader);
			return systemFontCMapFormat0Table;
		}
		}
	}

	internal static SystemFontCMapTable ImportCMapTable(SystemFontOpenTypeFontReader reader)
	{
		SystemFontCMapTable systemFontCMapTable;
		switch (reader.ReadUShort())
		{
		default:
			return null;
		case 4:
			systemFontCMapTable = new SystemFontCMapFormat4Table();
			break;
		case 0:
			systemFontCMapTable = new SystemFontCMapFormat0Table();
			break;
		}
		systemFontCMapTable.Import(reader);
		return systemFontCMapTable;
	}

	public abstract ushort GetGlyphId(ushort charCode);

	public abstract void Read(SystemFontOpenTypeFontReader reader);

	public abstract void Write(SystemFontFontWriter writer);

	public abstract void Import(SystemFontOpenTypeFontReader reader);
}
