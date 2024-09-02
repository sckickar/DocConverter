namespace DocGen.OfficeChart.Implementation.XmlSerialization.Constants;

internal interface IPageSetupConstantsProvider
{
	string PageMarginsTag { get; }

	string LeftMargin { get; }

	string RightMargin { get; }

	string TopMargin { get; }

	string BottomMargin { get; }

	string HeaderMargin { get; }

	string FooterMargin { get; }

	string Namespace { get; }
}
