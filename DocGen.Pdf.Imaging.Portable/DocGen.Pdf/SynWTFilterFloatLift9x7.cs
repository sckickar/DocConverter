namespace DocGen.Pdf;

internal class SynWTFilterFloatLift9x7 : SynWTFilterFloat
{
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

	public override void synthetize_lpf(float[] lowSig, int lowOff, int lowLen, int lowStep, float[] highSig, int highOff, int highLen, int highStep, float[] outSig, int outOff, int outStep)
	{
		int num = lowLen + highLen;
		int num2 = 2 * outStep;
		int num3 = lowOff;
		int num4 = highOff;
		int num5 = outOff;
		if (num > 1)
		{
			outSig[num5] = lowSig[num3] / 0.8128931f - 0.88701373f * highSig[num4] / 1.2301741f;
		}
		else
		{
			outSig[num5] = lowSig[num3];
		}
		num3 += lowStep;
		num4 += highStep;
		num5 += num2;
		int num6 = 2;
		while (num6 < num - 1)
		{
			outSig[num5] = lowSig[num3] / 0.8128931f - 0.44350687f * (highSig[num4 - highStep] + highSig[num4]) / 1.2301741f;
			num6 += 2;
			num5 += num2;
			num3 += lowStep;
			num4 += highStep;
		}
		if (num % 2 == 1 && num > 2)
		{
			outSig[num5] = lowSig[num3] / 0.8128931f - 0.88701373f * highSig[num4 - highStep] / 1.2301741f;
		}
		num3 = lowOff;
		num4 = highOff;
		num5 = outOff + outStep;
		num6 = 1;
		while (num6 < num - 1)
		{
			outSig[num5] = highSig[num4] / 1.2301741f - 0.8829111f * (outSig[num5 - outStep] + outSig[num5 + outStep]);
			num6 += 2;
			num5 += num2;
			num4 += highStep;
			num3 += lowStep;
		}
		if (num % 2 == 0)
		{
			outSig[num5] = highSig[num4] / 1.2301741f - 1.7658222f * outSig[num5 - outStep];
		}
		num5 = outOff;
		if (num > 1)
		{
			outSig[num5] -= -0.105960235f * outSig[num5 + outStep];
		}
		num5 += num2;
		num6 = 2;
		while (num6 < num - 1)
		{
			outSig[num5] -= -0.052980117f * (outSig[num5 - outStep] + outSig[num5 + outStep]);
			num6 += 2;
			num5 += num2;
		}
		if (num % 2 == 1 && num > 2)
		{
			outSig[num5] -= -0.105960235f * outSig[num5 - outStep];
		}
		num5 = outOff + outStep;
		num6 = 1;
		while (num6 < num - 1)
		{
			outSig[num5] -= -1.5861343f * (outSig[num5 - outStep] + outSig[num5 + outStep]);
			num6 += 2;
			num5 += num2;
		}
		if (num % 2 == 0)
		{
			outSig[num5] -= -3.1722686f * outSig[num5 - outStep];
		}
	}

	public override void synthetize_hpf(float[] lowSig, int lowOff, int lowLen, int lowStep, float[] highSig, int highOff, int highLen, int highStep, float[] outSig, int outOff, int outStep)
	{
		int num = lowLen + highLen;
		int num2 = 2 * outStep;
		int num3 = lowOff;
		int num4 = highOff;
		if (num != 1)
		{
			int num5 = num >> 1;
			for (int i = 0; i < num5; i++)
			{
				lowSig[num3] /= 0.8128931f;
				highSig[num4] /= 1.2301741f;
				num3 += lowStep;
				num4 += highStep;
			}
			if (num % 2 == 1)
			{
				highSig[num4] /= 1.2301741f;
			}
		}
		else
		{
			highSig[highOff] /= 2f;
		}
		num3 = lowOff;
		num4 = highOff;
		int num6 = outOff + outStep;
		for (int i = 1; i < num - 1; i += 2)
		{
			outSig[num6] = lowSig[num3] - 0.44350687f * (highSig[num4] + highSig[num4 + highStep]);
			num6 += num2;
			num3 += lowStep;
			num4 += highStep;
		}
		if (num % 2 == 0 && num > 1)
		{
			outSig[num6] = lowSig[num3] - 0.88701373f * highSig[num4];
		}
		num4 = highOff;
		num6 = outOff;
		if (num > 1)
		{
			outSig[num6] = highSig[num4] - 1.7658222f * outSig[num6 + outStep];
		}
		else
		{
			outSig[num6] = highSig[num4];
		}
		num6 += num2;
		num4 += highStep;
		for (int i = 2; i < num - 1; i += 2)
		{
			outSig[num6] = highSig[num4] - 0.8829111f * (outSig[num6 - outStep] + outSig[num6 + outStep]);
			num6 += num2;
			num4 += highStep;
		}
		if (num % 2 == 1 && num > 1)
		{
			outSig[num6] = highSig[num4] - 1.7658222f * outSig[num6 - outStep];
		}
		num6 = outOff + outStep;
		for (int i = 1; i < num - 1; i += 2)
		{
			outSig[num6] -= -0.052980117f * (outSig[num6 - outStep] + outSig[num6 + outStep]);
			num6 += num2;
		}
		if (num % 2 == 0 && num > 1)
		{
			outSig[num6] -= -0.105960235f * outSig[num6 - outStep];
		}
		num6 = outOff;
		if (num > 1)
		{
			outSig[num6] -= -3.1722686f * outSig[num6 + outStep];
		}
		num6 += num2;
		for (int i = 2; i < num - 1; i += 2)
		{
			outSig[num6] -= -1.5861343f * (outSig[num6 - outStep] + outSig[num6 + outStep]);
			num6 += num2;
		}
		if (num % 2 == 1 && num > 1)
		{
			outSig[num6] -= -3.1722686f * outSig[num6 - outStep];
		}
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

	public override string ToString()
	{
		return "w9x7 (lifting)";
	}
}
