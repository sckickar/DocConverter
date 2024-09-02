namespace DocGen.Pdf;

internal interface InvWTData : MultiResImgData
{
	int CbULX { get; }

	int CbULY { get; }

	new SubbandSyn getSynSubbandTree(int t, int c);
}
