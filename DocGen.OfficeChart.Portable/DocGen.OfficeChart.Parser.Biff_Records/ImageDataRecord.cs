namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.ImageData)]
internal class ImageDataRecord : BiffRecordWithContinue
{
	public override void ParseStructure()
	{
	}

	public override void InfillInternalData(OfficeVersion version)
	{
	}
}
