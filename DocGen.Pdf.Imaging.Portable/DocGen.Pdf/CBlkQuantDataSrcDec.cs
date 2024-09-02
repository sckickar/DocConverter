namespace DocGen.Pdf;

internal interface CBlkQuantDataSrcDec : InvWTData, MultiResImgData
{
	DataBlock getCodeBlock(int c, int m, int n, SubbandSyn sb, DataBlock cblk);

	DataBlock getInternCodeBlock(int c, int m, int n, SubbandSyn sb, DataBlock cblk);
}
