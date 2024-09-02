namespace DocGen.Pdf;

internal class AnWTFilterIntLift5x3 : AnWTFilterInt
{
	private static readonly float[] LPSynthesisFilter = new float[3] { 0.5f, 1f, 0.5f };

	private static readonly float[] HPSynthesisFilter = new float[5] { -0.125f, -0.25f, 0.75f, -0.25f, -0.125f };

	public override int AnLowNegSupport => 2;

	public override int AnLowPosSupport => 2;

	public override int AnHighNegSupport => 1;

	public override int AnHighPosSupport => 1;

	public override int SynLowNegSupport => 1;

	public override int SynLowPosSupport => 1;

	public override int SynHighNegSupport => 2;

	public override int SynHighPosSupport => 2;

	public override int ImplType => WaveletFilter_Fields.WT_FILTER_INT_LIFT;

	public override bool Reversible => true;

	public override int FilterType => 1;

	public override void analyze_lpf(int[] inSig, int inOff, int inLen, int inStep, int[] lowSig, int lowOff, int lowStep, int[] highSig, int highOff, int highStep)
	{
		int num = 2 * inStep;
		int num2 = inOff + inStep;
		int num3 = highOff;
		for (int i = 1; i < inLen - 1; i += 2)
		{
			highSig[num3] = inSig[num2] - (inSig[num2 - inStep] + inSig[num2 + inStep] >> 1);
			num2 += num;
			num3 += highStep;
		}
		if (inLen % 2 == 0)
		{
			highSig[num3] = inSig[num2] - (2 * inSig[num2 - inStep] >> 1);
		}
		num2 = inOff;
		int num4 = lowOff;
		num3 = highOff;
		if (inLen > 1)
		{
			lowSig[num4] = inSig[num2] + (highSig[num3] + 1 >> 1);
		}
		else
		{
			lowSig[num4] = inSig[num2];
		}
		num2 += num;
		num4 += lowStep;
		num3 += highStep;
		for (int i = 2; i < inLen - 1; i += 2)
		{
			lowSig[num4] = inSig[num2] + (highSig[num3 - highStep] + highSig[num3] + 2 >> 2);
			num2 += num;
			num4 += lowStep;
			num3 += highStep;
		}
		if (inLen % 2 == 1 && inLen > 2)
		{
			lowSig[num4] = inSig[num2] + (2 * highSig[num3 - highStep] + 2 >> 2);
		}
	}

	public override void analyze_hpf(int[] inSig, int inOff, int inLen, int inStep, int[] lowSig, int lowOff, int lowStep, int[] highSig, int highOff, int highStep)
	{
		int num = 2 * inStep;
		int num2 = inOff;
		int num3 = highOff;
		if (inLen > 1)
		{
			highSig[num3] = inSig[num2] - inSig[num2 + inStep];
		}
		else
		{
			highSig[num3] = inSig[num2] << 1;
		}
		num2 += num;
		num3 += highStep;
		if (inLen > 3)
		{
			for (int i = 2; i < inLen - 1; i += 2)
			{
				highSig[num3] = inSig[num2] - (inSig[num2 - inStep] + inSig[num2 + inStep] >> 1);
				num2 += num;
				num3 += highStep;
			}
		}
		if (inLen % 2 == 1 && inLen > 1)
		{
			highSig[num3] = inSig[num2] - inSig[num2 - inStep];
		}
		num2 = inOff + inStep;
		int num4 = lowOff;
		num3 = highOff;
		for (int i = 1; i < inLen - 1; i += 2)
		{
			lowSig[num4] = inSig[num2] + (highSig[num3] + highSig[num3 + highStep] + 2 >> 2);
			num2 += num;
			num4 += lowStep;
			num3 += highStep;
		}
		if (inLen > 1 && inLen % 2 == 0)
		{
			lowSig[num4] = inSig[num2] + (2 * highSig[num3] + 2 >> 2);
		}
	}

	public override float[] getLPSynthesisFilter()
	{
		return LPSynthesisFilter;
	}

	public override float[] getHPSynthesisFilter()
	{
		return HPSynthesisFilter;
	}

	public override bool isSameAsFullWT(int tailOvrlp, int headOvrlp, int inLen)
	{
		if (inLen % 2 == 0)
		{
			if (tailOvrlp >= 2 && headOvrlp >= 1)
			{
				return true;
			}
			return false;
		}
		if (tailOvrlp >= 2 && headOvrlp >= 2)
		{
			return true;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj != this)
		{
			return obj is AnWTFilterIntLift5x3;
		}
		return true;
	}

	public override string ToString()
	{
		return "w5x3";
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
