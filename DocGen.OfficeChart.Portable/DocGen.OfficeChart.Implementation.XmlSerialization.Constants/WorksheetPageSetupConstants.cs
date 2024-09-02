namespace DocGen.OfficeChart.Implementation.XmlSerialization.Constants;

internal class WorksheetPageSetupConstants : IPageSetupConstantsProvider
{
	public string PageMarginsTag => "pageMargins";

	public string LeftMargin => "left";

	public string RightMargin => "right";

	public string TopMargin => "top";

	public string BottomMargin => "bottom";

	public string HeaderMargin => "header";

	public string FooterMargin => "footer";

	public string Namespace => null;
}
