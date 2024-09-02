namespace DocGen.Pdf;

internal abstract class AnWTFilter : WaveletFilter
{
	public const char OPT_PREFIX = 'F';

	private static readonly string[][] pinfo = new string[1][] { new string[4] { "Ffilters", "[<tile-component idx>] <id> [ [<tile-component idx>] <id> ...]", "Specifies which filters to use for specified tile-component. If this option is not used, the encoder choses the filters  of the tile-components according to their quantization  type. If this option is used, a component transformation is applied to the three first components.\n<tile-component idx>: see general note\n<id>: ',' separates horizontal and vertical filters, ':' separates decomposition levels filters. JPEG 2000 part 1 only supports w5x3 and w9x7 filters.", null } };

	public abstract int FilterType { get; }

	public static string[][] ParameterInfo => pinfo;

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

	public abstract void analyze_lpf(object inSig, int inOff, int inLen, int inStep, object lowSig, int lowOff, int lowStep, object highSig, int highOff, int highStep);

	public abstract void analyze_hpf(object inSig, int inOff, int inLen, int inStep, object lowSig, int lowOff, int lowStep, object highSig, int highOff, int highStep);

	public abstract float[] getLPSynthesisFilter();

	public abstract float[] getHPSynthesisFilter();

	public virtual float[] getLPSynWaveForm(float[] in_Renamed, float[] out_Renamed)
	{
		return upsampleAndConvolve(in_Renamed, getLPSynthesisFilter(), out_Renamed);
	}

	public virtual float[] getHPSynWaveForm(float[] in_Renamed, float[] out_Renamed)
	{
		return upsampleAndConvolve(in_Renamed, getHPSynthesisFilter(), out_Renamed);
	}

	private static float[] upsampleAndConvolve(float[] in_Renamed, float[] wf, float[] out_Renamed)
	{
		if (in_Renamed == null)
		{
			in_Renamed = new float[1] { 1f };
		}
		if (out_Renamed == null)
		{
			out_Renamed = new float[in_Renamed.Length * 2 + wf.Length - 2];
		}
		int i = 0;
		for (int num = in_Renamed.Length * 2 + wf.Length - 2; i < num; i++)
		{
			float num2 = 0f;
			int num3 = (i - wf.Length + 2) / 2;
			if (num3 < 0)
			{
				num3 = 0;
			}
			int num4 = i / 2 + 1;
			if (num4 > in_Renamed.Length)
			{
				num4 = in_Renamed.Length;
			}
			int num5 = 2 * num3 - i + wf.Length - 1;
			while (num3 < num4)
			{
				num2 += in_Renamed[num3] * wf[num5];
				num3++;
				num5 += 2;
			}
			out_Renamed[i] = num2;
		}
		return out_Renamed;
	}

	public abstract bool isSameAsFullWT(int param1, int param2, int param3);
}
