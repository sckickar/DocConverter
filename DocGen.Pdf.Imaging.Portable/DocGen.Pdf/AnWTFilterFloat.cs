namespace DocGen.Pdf;

internal abstract class AnWTFilterFloat : AnWTFilter
{
	public override int DataType => 4;

	public abstract void analyze_lpf(float[] inSig, int inOff, int inLen, int inStep, float[] lowSig, int lowOff, int lowStep, float[] highSig, int highOff, int highStep);

	public override void analyze_lpf(object inSig, int inOff, int inLen, int inStep, object lowSig, int lowOff, int lowStep, object highSig, int highOff, int highStep)
	{
		analyze_lpf((float[])inSig, inOff, inLen, inStep, (float[])lowSig, lowOff, lowStep, (float[])highSig, highOff, highStep);
	}

	public abstract void analyze_hpf(float[] inSig, int inOff, int inLen, int inStep, float[] lowSig, int lowOff, int lowStep, float[] highSig, int highOff, int highStep);

	public override void analyze_hpf(object inSig, int inOff, int inLen, int inStep, object lowSig, int lowOff, int lowStep, object highSig, int highOff, int highStep)
	{
		analyze_hpf((float[])inSig, inOff, inLen, inStep, (float[])lowSig, lowOff, lowStep, (float[])highSig, highOff, highStep);
	}
}
