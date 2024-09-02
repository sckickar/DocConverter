using System;

namespace DocGen.Pdf;

internal class StdDequantizer : Dequantizer
{
	private QuantTypeSpec qts;

	private QuantStepSizeSpec qsss;

	private GuardBitsSpec gbs;

	private DataBlockInt inblk;

	private int outdtype;

	internal StdDequantizer(CBlkQuantDataSrcDec src, int[] utrb, DecodeHelper decSpec)
		: base(src, utrb, decSpec)
	{
		if (utrb.Length != src.NumComps)
		{
			throw new ArgumentException("Invalid rb argument");
		}
		qsss = decSpec.qsss;
		qts = decSpec.qts;
		gbs = decSpec.gbs;
	}

	public override int getFixedPoint(int c)
	{
		return 0;
	}

	public override DataBlock getCodeBlock(int c, int m, int n, SubbandSyn sb, DataBlock cblk)
	{
		return getInternCodeBlock(c, m, n, sb, cblk);
	}

	public override DataBlock getInternCodeBlock(int c, int m, int n, SubbandSyn sb, DataBlock cblk)
	{
		bool flag = qts.isReversible(tIdx, c);
		bool flag2 = qts.isDerived(tIdx, c);
		StdDequantizerParams stdDequantizerParams = (StdDequantizerParams)qsss.getTileCompVal(tIdx, c);
		_ = (int)gbs.getTileCompVal(tIdx, c);
		outdtype = cblk.DataType;
		if (flag && outdtype != 3)
		{
			throw new ArgumentException("Reversible quantizations must use int data");
		}
		int[] array = null;
		float[] array2 = null;
		int[] array3 = null;
		switch (outdtype)
		{
		case 3:
			cblk = src.getCodeBlock(c, m, n, sb, cblk);
			array = (int[])cblk.Data;
			break;
		case 4:
			inblk = (DataBlockInt)src.getInternCodeBlock(c, m, n, sb, inblk);
			array3 = inblk.DataInt;
			if (cblk == null)
			{
				cblk = new DataBlockFloat();
			}
			cblk.ulx = inblk.ulx;
			cblk.uly = inblk.uly;
			cblk.w = inblk.w;
			cblk.h = inblk.h;
			cblk.offset = 0;
			cblk.scanw = cblk.w;
			cblk.progressive = inblk.progressive;
			array2 = (float[])cblk.Data;
			if (array2 == null || array2.Length < cblk.w * cblk.h)
			{
				array2 = (float[])(cblk.Data = new float[cblk.w * cblk.h]);
			}
			break;
		}
		int magbits = sb.magbits;
		if (flag)
		{
			int num = 31 - magbits;
			for (int num2 = array.Length - 1; num2 >= 0; num2--)
			{
				int num3 = array[num2];
				array[num2] = ((num3 >= 0) ? (num3 >> num) : (-((num3 & 0x7FFFFFFF) >> num)));
			}
		}
		else
		{
			float num4;
			if (flag2)
			{
				int resLvl = src.getSynSubbandTree(TileIdx, c).resLvl;
				num4 = stdDequantizerParams.nStep[0][0] * (float)(1L << rb[c] + sb.anGainExp + resLvl - sb.level);
			}
			else
			{
				num4 = stdDequantizerParams.nStep[sb.resLvl][sb.sbandIdx] * (float)(1L << rb[c] + sb.anGainExp);
			}
			int num = 31 - magbits;
			num4 /= (float)(1 << num);
			switch (outdtype)
			{
			case 3:
			{
				for (int num2 = array.Length - 1; num2 >= 0; num2--)
				{
					int num3 = array[num2];
					array[num2] = (int)((float)((num3 >= 0) ? num3 : (-(num3 & 0x7FFFFFFF))) * num4);
				}
				break;
			}
			case 4:
			{
				int w = cblk.w;
				int h = cblk.h;
				int num2 = w * h - 1;
				int num5 = inblk.offset + (h - 1) * inblk.scanw + w - 1;
				int num6 = w * (h - 1);
				while (num2 >= 0)
				{
					while (num2 >= num6)
					{
						int num3 = array3[num5];
						array2[num2] = (float)((num3 >= 0) ? num3 : (-(num3 & 0x7FFFFFFF))) * num4;
						num5--;
						num2--;
					}
					num5 -= inblk.scanw - w;
					num6 -= w;
				}
				break;
			}
			}
		}
		return cblk;
	}
}
