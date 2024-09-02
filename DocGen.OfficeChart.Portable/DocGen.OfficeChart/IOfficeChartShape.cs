namespace DocGen.OfficeChart;

internal interface IOfficeChartShape : IShape, IParentApplication, IOfficeChart
{
	int TopRow { get; set; }

	int BottomRow { get; set; }

	int LeftColumn { get; set; }

	int RightColumn { get; set; }
}
