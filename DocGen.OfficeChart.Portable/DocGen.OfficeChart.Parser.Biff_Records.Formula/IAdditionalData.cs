namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

internal interface IAdditionalData
{
	int AdditionalDataSize { get; }

	int ReadArray(DataProvider provider, int offset);
}
