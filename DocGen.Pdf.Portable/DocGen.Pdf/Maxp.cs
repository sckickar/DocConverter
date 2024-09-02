namespace DocGen.Pdf;

internal class Maxp : TableBase
{
	private int m_id = 1;

	private float m_version;

	private ushort m_numGlyphs;

	internal override int Id => m_id;

	public float Version
	{
		get
		{
			return m_version;
		}
		private set
		{
			m_version = value;
		}
	}

	public ushort NumGlyphs
	{
		get
		{
			return m_numGlyphs;
		}
		private set
		{
			m_numGlyphs = value;
		}
	}

	public Maxp(FontFile2 fontsource)
		: base(fontsource)
	{
	}

	public override void Read(ReadFontArray reader)
	{
		m_version = reader.getnextshort();
		m_version = Version + (float)(reader.getnextUshort() / 65536);
		m_numGlyphs = reader.getnextUshort();
	}
}
