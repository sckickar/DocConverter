namespace DocGen.OfficeChart;

public interface IOfficeChartFillBorder
{
	bool HasInterior { get; }

	bool HasLineProperties { get; }

	bool Has3dProperties { get; }

	bool HasShadowProperties { get; }

	IOfficeChartBorder LineProperties { get; }

	IOfficeChartInterior Interior { get; }

	IOfficeFill Fill { get; }

	IThreeDFormat ThreeD { get; }

	IShadow Shadow { get; }
}
