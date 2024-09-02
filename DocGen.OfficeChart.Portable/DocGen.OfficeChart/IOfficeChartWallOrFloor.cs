namespace DocGen.OfficeChart;

public interface IOfficeChartWallOrFloor : IOfficeChartGridLine, IOfficeChartFillBorder
{
	uint Thickness { get; set; }

	OfficeChartPictureType PictureUnit { get; set; }
}
