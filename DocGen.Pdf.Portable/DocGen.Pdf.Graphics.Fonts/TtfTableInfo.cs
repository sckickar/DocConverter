namespace DocGen.Pdf.Graphics.Fonts;

internal struct TtfTableInfo
{
	public int Offset;

	public int Length;

	public int Checksum;

	public bool Empty
	{
		get
		{
			if (Offset == Length && Length == Checksum)
			{
				return Checksum == 0;
			}
			return false;
		}
	}
}
