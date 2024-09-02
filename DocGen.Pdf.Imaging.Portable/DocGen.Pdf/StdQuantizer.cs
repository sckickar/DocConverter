using System;

namespace DocGen.Pdf;

internal class StdQuantizer : Quantizer
{
	public const int QSTEP_MANTISSA_BITS = 11;

	public const int QSTEP_EXPONENT_BITS = 5;

	public static readonly int QSTEP_MAX_MANTISSA = 2047;

	public static readonly int QSTEP_MAX_EXPONENT = 31;

	private static double log2 = Math.Log(2.0);

	private QuantTypeSpec qts;

	private QuantStepSizeSpec qsss;

	private GuardBitsSpec gbs;

	private CBlkWTDataFloat infblk;

	public virtual QuantTypeSpec QuantTypeSpec => qts;

	internal StdQuantizer(CBlkWTDataSrc src, EncoderSpecs encSpec)
		: base(src)
	{
		qts = encSpec.qts;
		qsss = encSpec.qsss;
		gbs = encSpec.gbs;
	}

	public override int getNumGuardBits(int t, int c)
	{
		return (int)gbs.getTileCompVal(t, c);
	}

	public override bool isReversible(int t, int c)
	{
		return qts.isReversible(t, c);
	}

	public override bool isDerived(int t, int c)
	{
		return qts.isDerived(t, c);
	}

	public override CBlkWTData getNextCodeBlock(int c, CBlkWTData cblk)
	{
		return getNextInternCodeBlock(c, cblk);
	}

	public override CBlkWTData getNextInternCodeBlock(int c, CBlkWTData cblk)
	{
		float[] array = null;
		int num = (int)gbs.getTileCompVal(tIdx, c);
		bool flag = src.getDataType(tIdx, c) == 3;
		if (cblk == null)
		{
			cblk = new CBlkWTDataInt();
		}
		CBlkWTDataFloat cBlkWTDataFloat = infblk;
		int[] array2;
		if (flag)
		{
			cblk = src.getNextCodeBlock(c, cblk);
			if (cblk == null)
			{
				return null;
			}
			array2 = (int[])cblk.Data;
		}
		else
		{
			cBlkWTDataFloat = (CBlkWTDataFloat)src.getNextInternCodeBlock(c, cBlkWTDataFloat);
			if (cBlkWTDataFloat == null)
			{
				infblk.Data = null;
				return null;
			}
			infblk = cBlkWTDataFloat;
			array = (float[])cBlkWTDataFloat.Data;
			array2 = (int[])cblk.Data;
			if (array2 == null || array2.Length < cBlkWTDataFloat.w * cBlkWTDataFloat.h)
			{
				array2 = (int[])(cblk.Data = new int[cBlkWTDataFloat.w * cBlkWTDataFloat.h]);
			}
			cblk.m = cBlkWTDataFloat.m;
			cblk.n = cBlkWTDataFloat.n;
			cblk.sb = cBlkWTDataFloat.sb;
			cblk.ulx = cBlkWTDataFloat.ulx;
			cblk.uly = cBlkWTDataFloat.uly;
			cblk.w = cBlkWTDataFloat.w;
			cblk.h = cBlkWTDataFloat.h;
			cblk.wmseScaling = cBlkWTDataFloat.wmseScaling;
			cblk.offset = 0;
			cblk.scanw = cblk.w;
		}
		int w = cblk.w;
		int h = cblk.h;
		SubbandAn sb = cblk.sb;
		if (isReversible(tIdx, c))
		{
			cblk.magbits = num - 1 + src.getNomRangeBits(c) + sb.anGainExp;
			int num2 = 31 - cblk.magbits;
			cblk.convertFactor = 1 << num2;
			for (int num3 = w * h - 1; num3 >= 0; num3--)
			{
				int num4 = array2[num3] << num2;
				array2[num3] = ((num4 < 0) ? (int.MinValue | -num4) : num4);
			}
		}
		else
		{
			float num5 = (float)qsss.getTileCompVal(tIdx, c);
			float step;
			if (isDerived(tIdx, c))
			{
				cblk.magbits = num - 1 + sb.level - (int)Math.Floor(Math.Log(num5) / log2);
				step = num5 / (float)(1 << sb.level);
			}
			else
			{
				cblk.magbits = num - 1 - (int)Math.Floor(Math.Log(num5 / (sb.l2Norm * (float)(1 << sb.anGainExp))) / log2);
				step = num5 / (sb.l2Norm * (float)(1 << sb.anGainExp));
			}
			int num2 = 31 - cblk.magbits;
			step = convertFromExpMantissa(convertToExpMantissa(step));
			float num6 = 1f / ((float)(1L << src.getNomRangeBits(c) + sb.anGainExp) * step);
			num6 *= (float)(1 << num2 - src.getFixedPoint(c));
			cblk.convertFactor = num6;
			cblk.stepSize = (float)(1L << src.getNomRangeBits(c) + sb.anGainExp) * step;
			if (flag)
			{
				for (int num3 = w * h - 1; num3 >= 0; num3--)
				{
					int num4 = (int)((float)array2[num3] * num6);
					array2[num3] = ((num4 < 0) ? (int.MinValue | -num4) : num4);
				}
			}
			else
			{
				int num3 = w * h - 1;
				int num7 = cBlkWTDataFloat.offset + (h - 1) * cBlkWTDataFloat.scanw + w - 1;
				int num8 = w * (h - 1);
				while (num3 >= 0)
				{
					while (num3 >= num8)
					{
						int num4 = (int)(array[num7] * num6);
						array2[num3] = ((num4 < 0) ? (int.MinValue | -num4) : num4);
						num7--;
						num3--;
					}
					num7 -= cBlkWTDataFloat.scanw - w;
					num8 -= w;
				}
			}
		}
		return cblk;
	}

	internal override void calcSbParams(SubbandAn sb, int c)
	{
		if (sb.stepWMSE > 0f)
		{
			return;
		}
		if (!sb.isNode)
		{
			if (isReversible(tIdx, c))
			{
				sb.stepWMSE = (float)Math.Pow(2.0, -(src.getNomRangeBits(c) << 1)) * sb.l2Norm * sb.l2Norm;
				return;
			}
			float num = (float)qsss.getTileCompVal(tIdx, c);
			if (isDerived(tIdx, c))
			{
				sb.stepWMSE = num * num * (float)Math.Pow(2.0, sb.anGainExp - sb.level << 1) * sb.l2Norm * sb.l2Norm;
			}
			else
			{
				sb.stepWMSE = num * num;
			}
		}
		else
		{
			calcSbParams((SubbandAn)sb.LL, c);
			calcSbParams((SubbandAn)sb.HL, c);
			calcSbParams((SubbandAn)sb.LH, c);
			calcSbParams((SubbandAn)sb.HH, c);
			sb.stepWMSE = 1f;
		}
	}

	public static int convertToExpMantissa(float step)
	{
		int num = (int)Math.Ceiling((0.0 - Math.Log(step)) / log2);
		if (num > QSTEP_MAX_EXPONENT)
		{
			return QSTEP_MAX_EXPONENT << 11;
		}
		return (num << 11) | (int)(((0f - step) * (float)(-1 << num) - 1f) * 2048f + 0.5f);
	}

	private static float convertFromExpMantissa(int ems)
	{
		return (-1f - (float)(ems & QSTEP_MAX_MANTISSA) / 2048f) / (float)(-1 << ((ems >> 11) & QSTEP_MAX_EXPONENT));
	}

	public override int getMaxMagBits(int c)
	{
		Subband anSubbandTree = getAnSubbandTree(tIdx, c);
		if (isReversible(tIdx, c))
		{
			return getMaxMagBitsRev(anSubbandTree, c);
		}
		if (isDerived(tIdx, c))
		{
			return getMaxMagBitsDerived(anSubbandTree, tIdx, c);
		}
		return getMaxMagBitsExpounded(anSubbandTree, tIdx, c);
	}

	private int getMaxMagBitsRev(Subband sb, int c)
	{
		int num = 0;
		int num2 = (int)gbs.getTileCompVal(tIdx, c);
		if (!sb.isNode)
		{
			return num2 - 1 + src.getNomRangeBits(c) + sb.anGainExp;
		}
		num = getMaxMagBitsRev(sb.LL, c);
		int maxMagBitsRev = getMaxMagBitsRev(sb.LH, c);
		if (maxMagBitsRev > num)
		{
			num = maxMagBitsRev;
		}
		maxMagBitsRev = getMaxMagBitsRev(sb.HL, c);
		if (maxMagBitsRev > num)
		{
			num = maxMagBitsRev;
		}
		maxMagBitsRev = getMaxMagBitsRev(sb.HH, c);
		if (maxMagBitsRev > num)
		{
			num = maxMagBitsRev;
		}
		return num;
	}

	private int getMaxMagBitsDerived(Subband sb, int t, int c)
	{
		int num = 0;
		int num2 = (int)gbs.getTileCompVal(t, c);
		if (!sb.isNode)
		{
			float num3 = (float)qsss.getTileCompVal(t, c);
			return num2 - 1 + sb.level - (int)Math.Floor(Math.Log(num3) / log2);
		}
		num = getMaxMagBitsDerived(sb.LL, t, c);
		int maxMagBitsDerived = getMaxMagBitsDerived(sb.LH, t, c);
		if (maxMagBitsDerived > num)
		{
			num = maxMagBitsDerived;
		}
		maxMagBitsDerived = getMaxMagBitsDerived(sb.HL, t, c);
		if (maxMagBitsDerived > num)
		{
			num = maxMagBitsDerived;
		}
		maxMagBitsDerived = getMaxMagBitsDerived(sb.HH, t, c);
		if (maxMagBitsDerived > num)
		{
			num = maxMagBitsDerived;
		}
		return num;
	}

	private int getMaxMagBitsExpounded(Subband sb, int t, int c)
	{
		int num = 0;
		int num2 = (int)gbs.getTileCompVal(t, c);
		if (!sb.isNode)
		{
			float num3 = (float)qsss.getTileCompVal(t, c);
			return num2 - 1 - (int)Math.Floor(Math.Log(num3 / (((SubbandAn)sb).l2Norm * (float)(1 << sb.anGainExp))) / log2);
		}
		num = getMaxMagBitsExpounded(sb.LL, t, c);
		int maxMagBitsExpounded = getMaxMagBitsExpounded(sb.LH, t, c);
		if (maxMagBitsExpounded > num)
		{
			num = maxMagBitsExpounded;
		}
		maxMagBitsExpounded = getMaxMagBitsExpounded(sb.HL, t, c);
		if (maxMagBitsExpounded > num)
		{
			num = maxMagBitsExpounded;
		}
		maxMagBitsExpounded = getMaxMagBitsExpounded(sb.HH, t, c);
		if (maxMagBitsExpounded > num)
		{
			num = maxMagBitsExpounded;
		}
		return num;
	}
}
