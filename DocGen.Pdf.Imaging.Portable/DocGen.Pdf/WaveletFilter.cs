namespace DocGen.Pdf;

internal interface WaveletFilter
{
	int AnLowNegSupport { get; }

	int AnLowPosSupport { get; }

	int AnHighNegSupport { get; }

	int AnHighPosSupport { get; }

	int SynLowNegSupport { get; }

	int SynLowPosSupport { get; }

	int SynHighNegSupport { get; }

	int SynHighPosSupport { get; }

	int ImplType { get; }

	int DataType { get; }

	bool Reversible { get; }

	bool isSameAsFullWT(int tailOvrlp, int headOvrlp, int inLen);
}
