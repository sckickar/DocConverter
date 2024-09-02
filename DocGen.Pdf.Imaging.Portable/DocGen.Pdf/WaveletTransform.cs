namespace DocGen.Pdf;

internal interface WaveletTransform : ImageData
{
	bool isReversible(int t, int c);

	int getImplementationType(int c);
}
