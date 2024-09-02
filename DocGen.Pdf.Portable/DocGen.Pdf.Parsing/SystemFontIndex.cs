namespace DocGen.Pdf.Parsing;

internal class SystemFontIndex : SystemFontCFFTable
{
	public long SkipOffset
	{
		get
		{
			if (Offsets == null)
			{
				return DataOffset;
			}
			return DataOffset + SystemFontEnumerable.Last(Offsets);
		}
	}

	public ushort Count { get; private set; }

	protected uint[] Offsets { get; private set; }

	protected long DataOffset { get; private set; }

	public SystemFontIndex(SystemFontCFFFontFile file, long offset)
		: base(file, offset)
	{
	}

	protected int GetDataLength(int index)
	{
		return (int)(Offsets[index + 1] - Offsets[index]);
	}

	public override void Read(SystemFontCFFFontReader reader)
	{
		Count = reader.ReadCard16();
		if (Count == 0)
		{
			DataOffset = reader.Position;
			return;
		}
		byte offsetSize = reader.ReadOffSize();
		ushort num = (ushort)(Count + 1);
		Offsets = new uint[num];
		for (int i = 0; i < num; i++)
		{
			Offsets[i] = reader.ReadOffset(offsetSize);
		}
		DataOffset = reader.Position - 1;
	}
}
