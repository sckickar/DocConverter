namespace DocGen.Pdf;

internal interface BlockImageDataSource : ImageData
{
	int getFixedPoint(int c);

	DataBlock getInternCompData(DataBlock blk, int c);

	DataBlock getCompData(DataBlock blk, int c);
}
