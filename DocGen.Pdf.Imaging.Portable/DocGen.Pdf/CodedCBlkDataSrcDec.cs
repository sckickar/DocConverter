namespace DocGen.Pdf;

internal interface CodedCBlkDataSrcDec : InvWTData, MultiResImgData
{
	DecLyrdCBlk getCodeBlock(int c, int m, int n, SubbandSyn sb, int fl, int nl, DecLyrdCBlk ccb);
}
