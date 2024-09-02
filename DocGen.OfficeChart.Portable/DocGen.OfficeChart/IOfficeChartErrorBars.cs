namespace DocGen.OfficeChart;

public interface IOfficeChartErrorBars
{
	IOfficeChartBorder Border { get; }

	OfficeErrorBarInclude Include { get; set; }

	bool HasCap { get; set; }

	OfficeErrorBarType Type { get; set; }

	double NumberValue { get; set; }

	IOfficeDataRange PlusRange { get; set; }

	IOfficeDataRange MinusRange { get; set; }

	IShadow Shadow { get; }

	IThreeDFormat Chart3DOptions { get; }

	void ClearFormats();

	void Delete();
}
