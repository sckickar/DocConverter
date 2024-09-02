namespace DocGen.Pdf;

internal abstract class SynWTFilterFloat : SynWTFilter
{
	public override int DataType => 4;

	public abstract void synthetize_lpf(float[] lowSig, int lowOff, int lowLen, int lowStep, float[] highSig, int highOff, int highLen, int highStep, float[] outSig, int outOff, int outStep);

	public override void synthetize_lpf(object lowSig, int lowOff, int lowLen, int lowStep, object highSig, int highOff, int highLen, int highStep, object outSig, int outOff, int outStep)
	{
		synthetize_lpf((float[])lowSig, lowOff, lowLen, lowStep, (float[])highSig, highOff, highLen, highStep, (float[])outSig, outOff, outStep);
	}

	public abstract void synthetize_hpf(float[] lowSig, int lowOff, int lowLen, int lowStep, float[] highSig, int highOff, int highLen, int highStep, float[] outSig, int outOff, int outStep);

	public override void synthetize_hpf(object lowSig, int lowOff, int lowLen, int lowStep, object highSig, int highOff, int highLen, int highStep, object outSig, int outOff, int outStep)
	{
		synthetize_hpf((float[])lowSig, lowOff, lowLen, lowStep, (float[])highSig, highOff, highLen, highStep, (float[])outSig, outOff, outStep);
	}
}
