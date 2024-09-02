namespace DocGen.DocIO.DLS;

public class ComparisonOptions
{
	private byte m_bFlags;

	public bool DetectFormatChanges
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public ComparisonOptions()
	{
		DetectFormatChanges = true;
	}
}
