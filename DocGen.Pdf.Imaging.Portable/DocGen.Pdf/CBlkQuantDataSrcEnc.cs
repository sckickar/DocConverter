namespace DocGen.Pdf;

internal interface CBlkQuantDataSrcEnc : ForwWTDataProps, ImageData
{
	CBlkWTData getNextCodeBlock(int c, CBlkWTData cblk);

	CBlkWTData getNextInternCodeBlock(int c, CBlkWTData cblk);
}
