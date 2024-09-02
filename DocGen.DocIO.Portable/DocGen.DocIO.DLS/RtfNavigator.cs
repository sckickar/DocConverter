namespace DocGen.DocIO.DLS;

internal abstract class RtfNavigator
{
	internal enum EmfToWmfBitsFlags
	{
		EmfToWmfBitsFlagsDefault = 0,
		EmfToWmfBitsFlagsEmbedEmf = 1,
		EmfToWmfBitsFlagsIncludePlaceable = 2,
		EmfToWmfBitsFlagsNoXORClip = 4
	}

	internal const int c_two = 2;

	internal const int c_twentiethOfPoint = 20;

	internal const int c_quaterPoint = 4;

	internal const int c_fiftiethOfPoint = 50;

	internal const float c_thirtyfive = 35.5f;
}
