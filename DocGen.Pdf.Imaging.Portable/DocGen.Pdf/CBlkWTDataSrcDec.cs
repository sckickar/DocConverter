namespace DocGen.Pdf;

internal interface CBlkWTDataSrcDec : InvWTData, MultiResImgData
{
	int getNomRangeBits(int c);

	int getFixedPoint(int c);

	DataBlock getCodeBlock(int c, int m, int n, SubbandSyn sb, DataBlock cblk);

	DataBlock getInternCodeBlock(int c, int m, int n, SubbandSyn sb, DataBlock cblk);
}
