namespace DocGen.Pdf;

internal abstract class WaveletTransformInverse : InvWTAdapter, BlockImageDataSource, ImageData
{
	internal WaveletTransformInverse(MultiResImgData src, DecodeHelper decSpec)
		: base(src, decSpec)
	{
	}

	internal static WaveletTransformInverse createInstance(CBlkWTDataSrcDec src, DecodeHelper decSpec)
	{
		return new InvWTFull(src, decSpec);
	}

	public abstract int getFixedPoint(int param1);

	public abstract DataBlock getInternCompData(DataBlock param1, int param2);

	public abstract DataBlock getCompData(DataBlock param1, int param2);
}
