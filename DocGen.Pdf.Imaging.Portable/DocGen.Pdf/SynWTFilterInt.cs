namespace DocGen.Pdf;

internal abstract class SynWTFilterInt : SynWTFilter
{
	public override int DataType => 3;

	public abstract void synthetize_lpf(int[] lowSig, int lowOff, int lowLen, int lowStep, int[] highSig, int highOff, int highLen, int highStep, int[] outSig, int outOff, int outStep);

	public override void synthetize_lpf(object lowSig, int lowOff, int lowLen, int lowStep, object highSig, int highOff, int highLen, int highStep, object outSig, int outOff, int outStep)
	{
		synthetize_lpf((int[])lowSig, lowOff, lowLen, lowStep, (int[])highSig, highOff, highLen, highStep, (int[])outSig, outOff, outStep);
	}

	public abstract void synthetize_hpf(int[] lowSig, int lowOff, int lowLen, int lowStep, int[] highSig, int highOff, int highLen, int highStep, int[] outSig, int outOff, int outStep);

	public override void synthetize_hpf(object lowSig, int lowOff, int lowLen, int lowStep, object highSig, int highOff, int highLen, int highStep, object outSig, int outOff, int outStep)
	{
		synthetize_hpf((int[])lowSig, lowOff, lowLen, lowStep, (int[])highSig, highOff, highLen, highStep, (int[])outSig, outOff, outStep);
	}
}
