namespace DocGen.Pdf;

internal class FontEncoding
{
	private ushort m_platformId;

	private ushort m_encodingId;

	private uint m_offset;

	public uint Offset
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

	public ushort PlatformId
	{
		get
		{
			return m_platformId;
		}
		set
		{
			m_platformId = value;
		}
	}

	public ushort EncodingId
	{
		get
		{
			return m_encodingId;
		}
		set
		{
			m_encodingId = value;
		}
	}

	public void ReadEncodingDeatils(ReadFontArray reader)
	{
		PlatformId = reader.getnextUshort();
		EncodingId = reader.getnextUshort();
		Offset = reader.getULong();
	}
}
