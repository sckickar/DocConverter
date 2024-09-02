namespace DocGen.Pdf;

internal abstract class SynWTFilter : WaveletFilter
{
	public abstract int AnHighPosSupport { get; }

	public abstract int AnLowNegSupport { get; }

	public abstract int AnLowPosSupport { get; }

	public abstract bool Reversible { get; }

	public abstract int ImplType { get; }

	public abstract int SynHighNegSupport { get; }

	public abstract int SynHighPosSupport { get; }

	public abstract int AnHighNegSupport { get; }

	public abstract int DataType { get; }

	public abstract int SynLowNegSupport { get; }

	public abstract int SynLowPosSupport { get; }

	public abstract void synthetize_lpf(object lowSig, int lowOff, int lowLen, int lowStep, object highSig, int highOff, int highLen, int highStep, object outSig, int outOff, int outStep);

	public abstract void synthetize_hpf(object lowSig, int lowOff, int lowLen, int lowStep, object highSig, int highOff, int highLen, int highStep, object outSig, int outOff, int outStep);

	public abstract bool isSameAsFullWT(int param1, int param2, int param3);
}
