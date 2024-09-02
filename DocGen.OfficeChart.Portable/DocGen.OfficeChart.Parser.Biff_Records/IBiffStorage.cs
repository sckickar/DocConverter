namespace DocGen.OfficeChart.Parser.Biff_Records;

internal interface IBiffStorage
{
	TBIFFRecord TypeCode { get; }

	int RecordCode { get; }

	bool NeedDataArray { get; }

	long StreamPos { get; set; }

	int GetStoreSize(OfficeVersion version);
}
