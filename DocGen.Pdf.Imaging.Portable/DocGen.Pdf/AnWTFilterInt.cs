namespace DocGen.Pdf;

internal abstract class AnWTFilterInt : AnWTFilter
{
	public override int DataType => 3;

	public abstract void analyze_lpf(int[] inSig, int inOff, int inLen, int inStep, int[] lowSig, int lowOff, int lowStep, int[] highSig, int highOff, int highStep);

	public override void analyze_lpf(object inSig, int inOff, int inLen, int inStep, object lowSig, int lowOff, int lowStep, object highSig, int highOff, int highStep)
	{
		analyze_lpf((int[])inSig, inOff, inLen, inStep, (int[])lowSig, lowOff, lowStep, (int[])highSig, highOff, highStep);
	}

	public abstract void analyze_hpf(int[] inSig, int inOff, int inLen, int inStep, int[] lowSig, int lowOff, int lowStep, int[] highSig, int highOff, int highStep);

	public override void analyze_hpf(object inSig, int inOff, int inLen, int inStep, object lowSig, int lowOff, int lowStep, object highSig, int highOff, int highStep)
	{
		analyze_hpf((int[])inSig, inOff, inLen, inStep, (int[])lowSig, lowOff, lowStep, (int[])highSig, highOff, highStep);
	}
}
