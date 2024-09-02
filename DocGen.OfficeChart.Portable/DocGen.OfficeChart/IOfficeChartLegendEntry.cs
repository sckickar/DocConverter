namespace DocGen.OfficeChart;

public interface IOfficeChartLegendEntry
{
	bool IsDeleted { get; set; }

	bool IsFormatted { get; set; }

	IOfficeChartTextArea TextArea { get; }

	void Clear();

	void Delete();
}
