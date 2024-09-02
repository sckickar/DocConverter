using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontTCCHeader
{
	private readonly SystemFontTrueTypeCollection collection;

	private uint[] offsetTable;

	private SystemFontOpenTypeFontSourceBase[] fonts;

	protected SystemFontOpenTypeFontReader Reader => collection.Reader;

	public SystemFontOpenTypeFontSourceBase[] Fonts
	{
		get
		{
			if (fonts == null)
			{
				fonts = new SystemFontOpenTypeFontSourceBase[offsetTable.Length];
				for (int i = 0; i < offsetTable.Length; i++)
				{
					fonts[i] = ReadTrueTypeFontFile(Reader, offsetTable[i]);
				}
			}
			return fonts;
		}
	}

	private static SystemFontOpenTypeFontSource ReadTrueTypeFontFile(SystemFontOpenTypeFontReader reader, uint offset)
	{
		reader.BeginReadingBlock();
		reader.Seek(offset, SeekOrigin.Begin);
		SystemFontOpenTypeFontSource result = new SystemFontOpenTypeFontSource(reader);
		reader.EndReadingBlock();
		return result;
	}

	public SystemFontTCCHeader(SystemFontTrueTypeCollection collection)
	{
		this.collection = collection;
	}

	public void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadULong();
		reader.ReadFixed();
		uint num = reader.ReadULong();
		offsetTable = new uint[num];
		for (int i = 0; i < num; i++)
		{
			offsetTable[i] = reader.ReadULong();
		}
	}
}
