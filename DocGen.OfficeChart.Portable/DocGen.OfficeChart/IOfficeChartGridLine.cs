namespace DocGen.OfficeChart;

public interface IOfficeChartGridLine : IOfficeChartFillBorder
{
	IOfficeChartBorder Border { get; }

	void Delete();
}
