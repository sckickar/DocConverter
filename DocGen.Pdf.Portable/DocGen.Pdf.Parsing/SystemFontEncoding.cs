using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontEncoding : SystemFontCFFTable, ISystemFontEncoding
{
	private readonly SystemFontCharset charset;

	private List<ushort> gids;

	public SystemFontSupplementalEncoding SupplementalEncoding { get; private set; }

	public SystemFontEncoding(SystemFontCFFFontFile file, SystemFontCharset charset, long offset)
		: base(file, offset)
	{
		this.charset = charset;
	}

	private void ReadFormat0(SystemFontCFFFontReader reader)
	{
		byte b = reader.ReadCard8();
		gids = new List<ushort>(b);
		for (int i = 0; i < b; i++)
		{
			gids.Add(reader.ReadCard8());
		}
	}

	private void ReadFormat1(SystemFontCFFFontReader reader)
	{
		byte b = reader.ReadCard8();
		gids = new List<ushort>();
		for (int i = 0; i < b; i++)
		{
			byte b2 = reader.ReadCard8();
			byte b3 = reader.ReadCard8();
			gids.Add(b2);
			for (int j = 0; j < b3; j++)
			{
				gids.Add((byte)(b2 + j + 1));
			}
		}
	}

	public override void Read(SystemFontCFFFontReader reader)
	{
		byte n = reader.ReadCard8();
		if (SystemFontBitsHelper.GetBit(n, 0))
		{
			ReadFormat1(reader);
		}
		else
		{
			ReadFormat0(reader);
		}
		if (SystemFontBitsHelper.GetBit(n, 7))
		{
			SupplementalEncoding = new SystemFontSupplementalEncoding();
			SupplementalEncoding.Read(reader);
		}
	}

	public string GetGlyphName(SystemFontCFFFontFile fontFile, ushort index)
	{
		int num = gids.IndexOf(index);
		if (num < 0)
		{
			return ".notdef";
		}
		return charset[(ushort)num];
	}
}
