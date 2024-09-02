namespace DocGen.Pdf;

internal interface ForwWT : WaveletTransform, ImageData, ForwWTDataProps
{
	AnWTFilter[] getHorAnWaveletFilters(int t, int c);

	AnWTFilter[] getVertAnWaveletFilters(int t, int c);

	int getDecompLevels(int t, int c);

	int getDecomp(int t, int c);
}
