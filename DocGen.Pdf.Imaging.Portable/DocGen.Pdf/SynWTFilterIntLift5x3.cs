namespace DocGen.Pdf;

internal class SynWTFilterIntLift5x3 : SynWTFilterInt
{
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

	public override void synthetize_lpf(int[] lowSig, int lowOff, int lowLen, int lowStep, int[] highSig, int highOff, int highLen, int highStep, int[] outSig, int outOff, int outStep)
	{
		int num = lowLen + highLen;
		int num2 = 2 * outStep;
		int num3 = lowOff;
		int num4 = highOff;
		int num5 = outOff;
		if (num > 1)
		{
			outSig[num5] = lowSig[num3] - (highSig[num4] + 1 >> 1);
		}
		else
		{
			outSig[num5] = lowSig[num3];
		}
		num3 += lowStep;
		num4 += highStep;
		num5 += num2;
		for (int i = 2; i < num - 1; i += 2)
		{
			outSig[num5] = lowSig[num3] - (highSig[num4 - highStep] + highSig[num4] + 2 >> 2);
			num3 += lowStep;
			num4 += highStep;
			num5 += num2;
		}
		if (num % 2 == 1 && num > 2)
		{
			outSig[num5] = lowSig[num3] - (2 * highSig[num4 - highStep] + 2 >> 2);
		}
		num4 = highOff;
		num5 = outOff + outStep;
		for (int i = 1; i < num - 1; i += 2)
		{
			outSig[num5] = highSig[num4] + (outSig[num5 - outStep] + outSig[num5 + outStep] >> 1);
			num4 += highStep;
			num5 += num2;
		}
		if (num % 2 == 0 && num > 1)
		{
			outSig[num5] = highSig[num4] + outSig[num5 - outStep];
		}
	}

	public override void synthetize_hpf(int[] lowSig, int lowOff, int lowLen, int lowStep, int[] highSig, int highOff, int highLen, int highStep, int[] outSig, int outOff, int outStep)
	{
		int num = lowLen + highLen;
		int num2 = 2 * outStep;
		int num3 = lowOff;
		int num4 = highOff;
		int num5 = outOff + outStep;
		for (int i = 1; i < num - 1; i += 2)
		{
			outSig[num5] = lowSig[num3] - (highSig[num4] + highSig[num4 + highStep] + 2 >> 2);
			num3 += lowStep;
			num4 += highStep;
			num5 += num2;
		}
		if (num > 1 && num % 2 == 0)
		{
			outSig[num5] = lowSig[num3] - (2 * highSig[num4] + 2 >> 2);
		}
		num4 = highOff;
		num5 = outOff;
		if (num > 1)
		{
			outSig[num5] = highSig[num4] + outSig[num5 + outStep];
		}
		else
		{
			outSig[num5] = highSig[num4] >> 1;
		}
		num4 += highStep;
		num5 += num2;
		for (int i = 2; i < num - 1; i += 2)
		{
			outSig[num5] = highSig[num4] + (outSig[num5 - outStep] + outSig[num5 + outStep] >> 1);
			num4 += highStep;
			num5 += num2;
		}
		if (num % 2 == 1 && num > 1)
		{
			outSig[num5] = highSig[num4] + outSig[num5 - outStep];
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
		return "w5x3 (lifting)";
	}
}
