namespace DocGen.Pdf.Parsing;

internal class SystemFontSegment
{
	private ushort startCode;

	private ushort endCode;

	private short idDelta;

	private ushort[] map;

	internal SystemFontSegment()
	{
	}

	public SystemFontSegment(ushort startCode, ushort endCode, short idDelta)
	{
		this.startCode = startCode;
		this.endCode = endCode;
		this.idDelta = idDelta;
	}

	public SystemFontSegment(ushort startCode, ushort endCode, short idDelta, ushort[] map)
		: this(startCode, endCode, idDelta)
	{
		this.map = map;
	}

	public bool IsInside(ushort charCode)
	{
		return endCode >= charCode;
	}

	public ushort GetGlyphId(ushort charCode)
	{
		if (charCode < startCode || charCode > endCode)
		{
			return 0;
		}
		if (map == null)
		{
			return (ushort)(charCode + (ushort)idDelta);
		}
		int num = charCode - startCode;
		if (num > map.Length || map[num] == 0)
		{
			return 0;
		}
		return (ushort)(map[num] + (ushort)idDelta);
	}

	public void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(startCode);
		writer.WriteUShort(endCode);
		writer.WriteShort(idDelta);
		if (map != null)
		{
			writer.WriteUShort((ushort)map.Length);
			for (int i = 0; i < map.Length; i++)
			{
				writer.WriteUShort(map[i]);
			}
		}
		else
		{
			writer.WriteUShort(0);
		}
	}

	public void Import(SystemFontOpenTypeFontReader reader)
	{
		startCode = reader.ReadUShort();
		endCode = reader.ReadUShort();
		idDelta = reader.ReadShort();
		ushort num = reader.ReadUShort();
		if (num > 0)
		{
			map = new ushort[num];
			for (int i = 0; i < num; i++)
			{
				map[i] = reader.ReadUShort();
			}
		}
	}

	public override int GetHashCode()
	{
		return (17 * 23 + startCode.GetHashCode()) * 23 + endCode.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is SystemFontSegment systemFontSegment && startCode == systemFontSegment.startCode)
		{
			return endCode == systemFontSegment.endCode;
		}
		return false;
	}
}
