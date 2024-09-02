namespace DocGen.OfficeChart;

internal struct TextFrameColumns
{
	private int number;

	private int spacingPt;

	public int Number
	{
		get
		{
			return number;
		}
		set
		{
			number = value;
		}
	}

	public int SpacingPt
	{
		get
		{
			return spacingPt;
		}
		set
		{
			spacingPt = value;
		}
	}
}
