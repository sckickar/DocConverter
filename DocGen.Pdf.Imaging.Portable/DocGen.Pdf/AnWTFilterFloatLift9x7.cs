namespace DocGen.Pdf;

internal class AnWTFilterFloatLift9x7 : AnWTFilterFloat
{
	private static readonly float[] LPSynthesisFilter = new float[7] { -0.091272f, -0.057544f, 0.591272f, 1.115087f, 0.591272f, -0.057544f, -0.091272f };

	private static readonly float[] HPSynthesisFilter = new float[9] { 0.026749f, 0.016864f, -0.078223f, -0.266864f, 0.602949f, -0.266864f, -0.078223f, 0.016864f, 0.026749f };

	public const float ALPHA = -1.5861343f;

	public const float BETA = -0.052980117f;

	public const float GAMMA = 0.8829111f;

	public const float DELTA = 0.44350687f;

	public const float KL = 0.8128931f;

	public const float KH = 1.2301741f;

	public override int AnLowNegSupport => 4;

	public override int AnLowPosSupport => 4;

	public override int AnHighNegSupport => 3;

	public override int AnHighPosSupport => 3;

	public override int SynLowNegSupport => 3;

	public override int SynLowPosSupport => 3;

	public override int SynHighNegSupport => 4;

	public override int SynHighPosSupport => 4;

	public override int ImplType => WaveletFilter_Fields.WT_FILTER_FLOAT_LIFT;

	public override bool Reversible => false;

	public override int FilterType => 0;

	public override void analyze_lpf(float[] inSig, int inOff, int inLen, int inStep, float[] lowSig, int lowOff, int lowStep, float[] highSig, int highOff, int highStep)
	{
		int num = 2 * inStep;
		int num2 = inOff + inStep;
		int num3 = lowOff;
		int num4 = highOff;
		int i = 1;
		for (int num5 = inLen - 1; i < num5; i += 2)
		{
			highSig[num4] = inSig[num2] + -1.5861343f * (inSig[num2 - inStep] + inSig[num2 + inStep]);
			num2 += num;
			num4 += highStep;
		}
		if (inLen % 2 == 0)
		{
			highSig[num4] = inSig[num2] + -3.1722686f * inSig[num2 - inStep];
		}
		num2 = inOff;
		num3 = lowOff;
		num4 = highOff;
		if (inLen > 1)
		{
			lowSig[num3] = inSig[num2] + -0.105960235f * highSig[num4];
		}
		else
		{
			lowSig[num3] = inSig[num2];
		}
		num2 += num;
		num3 += lowStep;
		num4 += highStep;
		i = 2;
		for (int num5 = inLen - 1; i < num5; i += 2)
		{
			lowSig[num3] = inSig[num2] + -0.052980117f * (highSig[num4 - highStep] + highSig[num4]);
			num2 += num;
			num3 += lowStep;
			num4 += highStep;
		}
		if (inLen % 2 == 1 && inLen > 2)
		{
			lowSig[num3] = inSig[num2] + -0.105960235f * highSig[num4 - highStep];
		}
		num3 = lowOff;
		num4 = highOff;
		i = 1;
		for (int num5 = inLen - 1; i < num5; i += 2)
		{
			highSig[num4] += 0.8829111f * (lowSig[num3] + lowSig[num3 + lowStep]);
			num3 += lowStep;
			num4 += highStep;
		}
		if (inLen % 2 == 0)
		{
			highSig[num4] += 1.7658222f * lowSig[num3];
		}
		num3 = lowOff;
		num4 = highOff;
		if (inLen > 1)
		{
			lowSig[num3] += 0.88701373f * highSig[num4];
		}
		num3 += lowStep;
		num4 += highStep;
		i = 2;
		for (int num5 = inLen - 1; i < num5; i += 2)
		{
			lowSig[num3] += 0.44350687f * (highSig[num4 - highStep] + highSig[num4]);
			num3 += lowStep;
			num4 += highStep;
		}
		if (inLen % 2 == 1 && inLen > 2)
		{
			lowSig[num3] += 0.88701373f * highSig[num4 - highStep];
		}
		num3 = lowOff;
		num4 = highOff;
		for (i = 0; i < inLen >> 1; i++)
		{
			lowSig[num3] *= 0.8128931f;
			highSig[num4] *= 1.2301741f;
			num3 += lowStep;
			num4 += highStep;
		}
		if (inLen % 2 == 1 && inLen != 1)
		{
			lowSig[num3] *= 0.8128931f;
		}
	}

	public override void analyze_hpf(float[] inSig, int inOff, int inLen, int inStep, float[] lowSig, int lowOff, int lowStep, float[] highSig, int highOff, int highStep)
	{
		int num = 2 * inStep;
		int num2 = inOff;
		int num3 = lowOff;
		int num4 = highOff;
		if (inLen > 1)
		{
			highSig[num4] = inSig[num2] + -3.1722686f * inSig[num2 + inStep];
		}
		else
		{
			highSig[num4] = inSig[num2] * 2f;
		}
		num2 += num;
		num4 += highStep;
		for (int i = 2; i < inLen - 1; i += 2)
		{
			highSig[num4] = inSig[num2] + -1.5861343f * (inSig[num2 - inStep] + inSig[num2 + inStep]);
			num2 += num;
			num4 += highStep;
		}
		if (inLen % 2 == 1 && inLen > 1)
		{
			highSig[num4] = inSig[num2] + -3.1722686f * inSig[num2 - inStep];
		}
		num2 = inOff + inStep;
		num3 = lowOff;
		num4 = highOff;
		for (int i = 1; i < inLen - 1; i += 2)
		{
			lowSig[num3] = inSig[num2] + -0.052980117f * (highSig[num4] + highSig[num4 + highStep]);
			num2 += num;
			num3 += lowStep;
			num4 += highStep;
		}
		if (inLen > 1 && inLen % 2 == 0)
		{
			lowSig[num3] = inSig[num2] + -0.105960235f * highSig[num4];
		}
		num3 = lowOff;
		num4 = highOff;
		if (inLen > 1)
		{
			highSig[num4] += 1.7658222f * lowSig[num3];
		}
		num4 += highStep;
		for (int i = 2; i < inLen - 1; i += 2)
		{
			highSig[num4] += 0.8829111f * (lowSig[num3] + lowSig[num3 + lowStep]);
			num3 += lowStep;
			num4 += highStep;
		}
		if (inLen > 1 && inLen % 2 == 1)
		{
			highSig[num4] += 1.7658222f * lowSig[num3];
		}
		num3 = lowOff;
		num4 = highOff;
		for (int i = 1; i < inLen - 1; i += 2)
		{
			lowSig[num3] += 0.44350687f * (highSig[num4] + highSig[num4 + highStep]);
			num3 += lowStep;
			num4 += highStep;
		}
		if (inLen > 1 && inLen % 2 == 0)
		{
			lowSig[num3] += 0.88701373f * highSig[num4];
		}
		num3 = lowOff;
		num4 = highOff;
		for (int i = 0; i < inLen >> 1; i++)
		{
			lowSig[num3] *= 0.8128931f;
			highSig[num4] *= 1.2301741f;
			num3 += lowStep;
			num4 += highStep;
		}
		if (inLen % 2 == 1 && inLen != 1)
		{
			highSig[num4] *= 1.2301741f;
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
			if (tailOvrlp >= 4 && headOvrlp >= 3)
			{
				return true;
			}
			return false;
		}
		if (tailOvrlp >= 4 && headOvrlp >= 4)
		{
			return true;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj != this)
		{
			return obj is AnWTFilterFloatLift9x7;
		}
		return true;
	}

	public override string ToString()
	{
		return "w9x7";
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
