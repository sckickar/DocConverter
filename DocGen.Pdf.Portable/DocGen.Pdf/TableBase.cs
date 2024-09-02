namespace DocGen.Pdf;

internal abstract class TableBase
{
	private readonly FontFile2 fontSource;

	private int m_offset;

	internal abstract int Id { get; }

	public int Offset
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

	protected ReadFontArray Reader => fontSource.FontArrayReader;

	protected FontFile2 FontSource => fontSource;

	public TableBase(FontFile2 fontSource)
	{
		this.fontSource = fontSource;
	}

	public TableBase()
	{
	}

	public abstract void Read(ReadFontArray reader);
}
