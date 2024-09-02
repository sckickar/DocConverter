namespace DocGen.Pdf;

internal class Segments
{
	private ushort startCode;

	private ushort endCode;

	private short idDelta;

	private ushort[] map;

	internal Segments()
	{
	}

	public Segments(ushort startCode, ushort endCode, short idDelta)
	{
		this.startCode = startCode;
		this.endCode = endCode;
		this.idDelta = idDelta;
	}

	public Segments(ushort startCode, ushort endCode, short idDelta, ushort[] mapval)
	{
		this.startCode = startCode;
		this.endCode = endCode;
		this.idDelta = idDelta;
		map = mapval;
	}

	public bool IsContain(ushort charCode)
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
}
