namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

internal interface IDataStructure
{
	int Length { get; }

	void Parse(byte[] arrData, int iOffset);

	int Save(byte[] arrData, int iOffset);
}
