namespace DocGen.PdfViewer.Base;

internal class PdfRangeCalculator
{
	public int RangeStart { get; private set; }

	public int RangeEnd { get; private set; }

	public PdfRangeCalculator()
	{
		RangeStart = (RangeEnd = 0);
	}

	public PdfRangeCalculator(int start, int end)
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
