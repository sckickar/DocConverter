namespace DocGen.OfficeChart;

internal interface IOfficeChartPageSetup : IPageSetupBase, IParentApplication
{
	bool FitToPagesTall { get; set; }

	bool FitToPagesWide { get; set; }
}
