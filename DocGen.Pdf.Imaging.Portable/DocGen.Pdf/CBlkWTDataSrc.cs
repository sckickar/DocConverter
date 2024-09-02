namespace DocGen.Pdf;

internal interface CBlkWTDataSrc : ForwWTDataProps, ImageData
{
	int getFixedPoint(int c);

	int getDataType(int t, int c);

	CBlkWTData getNextCodeBlock(int c, CBlkWTData cblk);

	CBlkWTData getNextInternCodeBlock(int c, CBlkWTData cblk);
}
