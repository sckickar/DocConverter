namespace DocGen.OfficeChart;

public interface IOfficeChartFrameFormat : IOfficeChartFillBorder
{
	bool IsBorderCornersRound { get; set; }

	IOfficeChartBorder Border { get; }

	IOfficeChartLayout Layout { get; set; }

	void Clear();
}
