namespace DocGen.Pdf;

internal interface ForwWTDataProps : ImageData
{
	int CbULX { get; }

	int CbULY { get; }

	bool isReversible(int t, int c);

	SubbandAn getAnSubbandTree(int t, int c);
}
