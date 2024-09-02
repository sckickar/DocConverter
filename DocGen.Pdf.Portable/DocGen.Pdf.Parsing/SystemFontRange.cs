namespace DocGen.Pdf.Parsing;

internal class SystemFontRange
{
	public int RangeStart { get; private set; }

	public int RangeEnd { get; private set; }

	public SystemFontRange()
	{
		RangeStart = (RangeEnd = 0);
	}

	public SystemFontRange(int start, int end)
	{
		RangeStart = start;
		RangeEnd = end;
	}

	public bool IsInRange(int value)
	{
		if (RangeStart <= value)
		{
			return value <= RangeEnd;
		}
		return false;
	}
}
