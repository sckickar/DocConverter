namespace DocGen.OfficeChart;

internal interface IPageSetup : IPageSetupBase, IParentApplication
{
	int FitToPagesTall { get; set; }

	int FitToPagesWide { get; set; }

	bool PrintGridlines { get; set; }

	bool PrintHeadings { get; set; }

	string PrintArea { get; set; }

	string PrintTitleColumns { get; set; }

	string PrintTitleRows { get; set; }

	bool IsSummaryRowBelow { get; set; }

	bool IsSummaryColumnRight { get; set; }

	bool IsFitToPage { get; set; }
}
