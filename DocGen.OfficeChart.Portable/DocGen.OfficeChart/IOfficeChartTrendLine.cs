namespace DocGen.OfficeChart;

public interface IOfficeChartTrendLine
{
	IOfficeChartBorder Border { get; }

	double Backward { get; set; }

	double Forward { get; set; }

	bool DisplayEquation { get; set; }

	bool DisplayRSquared { get; set; }

	double Intercept { get; set; }

	bool InterceptIsAuto { get; set; }

	OfficeTrendLineType Type { get; set; }

	int Order { get; set; }

	bool NameIsAuto { get; set; }

	string Name { get; set; }

	IOfficeChartTextArea DataLabel { get; }

	IShadow Shadow { get; }

	IThreeDFormat Chart3DOptions { get; }

	void ClearFormats();
}
