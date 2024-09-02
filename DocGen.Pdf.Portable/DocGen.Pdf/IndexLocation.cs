namespace DocGen.Pdf;

internal class IndexLocation : TableBase
{
	private int m_id = 3;

	private uint[] m_offset;

	private int p;

	internal override int Id => m_id;

	public new uint[] Offset
	{
		get
		{
			return m_offset;
		}
		set
		{
			m_offset = value;
		}
	}

	public IndexLocation(FontFile2 fontsource)
		: base(fontsource)
	{
	}

	public long GetOffset(ushort index)
	{
		if (Offset == null || index >= Offset.Length || (index < Offset.Length - 1 && Offset[index + 1] == Offset[index]))
		{
			return -1L;
		}
		return Offset[index];
	}

	public override void Read(ReadFontArray reader)
	{
		p = reader.Pointer;
		m_offset = new uint[base.FontSource.NumGlyphs + 1];
		reader.Pointer = p;
		for (int i = 0; i < m_offset.Length; i++)
		{
			if (base.FontSource.Header.IndexToLocFormat == 0)
			{
				m_offset[i] = (uint)(reader.getnextUshort() * 2);
			}
			else
			{
				m_offset[i] = (uint)reader.getnextULong();
			}
		}
	}
}
