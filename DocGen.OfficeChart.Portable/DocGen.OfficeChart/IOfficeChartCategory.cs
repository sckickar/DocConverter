namespace DocGen.OfficeChart;

public interface IOfficeChartCategory : IParentApplication
{
	bool IsFiltered { get; set; }

	string Name { get; }

	IOfficeDataRange CategoryLabel { get; }

	IOfficeDataRange Values { get; }
}
