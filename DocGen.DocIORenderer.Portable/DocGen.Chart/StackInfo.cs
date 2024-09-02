namespace DocGen.Chart;

internal class StackInfo
{
	public ChartSeriesType Type;

	public int All;

	public string GroupName;

	public StackInfo(ChartSeriesType type, int all, string groupname)
	{
		Type = type;
		All = all;
		GroupName = groupname;
	}
}
