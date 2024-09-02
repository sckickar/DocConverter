namespace DocGen.OfficeChart.Implementation.XmlSerialization.Constants;

internal class ChartPageSetupConstants : IPageSetupConstantsProvider
{
	public string PageMarginsTag => "pageMargins";

	public string LeftMargin => "l";

	public string RightMargin => "r";

	public string TopMargin => "t";

	public string BottomMargin => "b";

	public string HeaderMargin => "header";

	public string FooterMargin => "footer";

	public string Namespace => "http://schemas.openxmlformats.org/drawingml/2006/chart";
}
