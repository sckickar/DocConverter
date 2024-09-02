namespace DocGen.Pdf;

internal class DeScalerROI : MultiResImgDataAdapter, CBlkQuantDataSrcDec, InvWTData, MultiResImgData
{
	private MaxShiftSpec mss;

	public const char OPT_PREFIX = 'R';

	private static readonly string[][] pinfo = new string[1][] { new string[4] { "Rno_roi", null, "This argument makes sure that the no ROI de-scaling is performed. Decompression is done like there is no ROI in the image", null } };

	private CBlkQuantDataSrcDec src;

	public virtual int CbULX => src.CbULX;

	public virtual int CbULY => src.CbULY;

	public static string[][] ParameterInfo => pinfo;

	internal DeScalerROI(CBlkQuantDataSrcDec src, MaxShiftSpec mss)
		: base(src)
	{
		this.src = src;
		this.mss = mss;
	}

	public override SubbandSyn getSynSubbandTree(int t, int c)
	{
		return src.getSynSubbandTree(t, c);
	}

	public virtual DataBlock getCodeBlock(int c, int m, int n, SubbandSyn sb, DataBlock cblk)
	{
		return getInternCodeBlock(c, m, n, sb, cblk);
	}

	public virtual DataBlock getInternCodeBlock(int c, int m, int n, SubbandSyn sb, DataBlock cblk)
	{
		cblk = src.getInternCodeBlock(c, m, n, sb, cblk);
		bool flag = false;
		if (mss == null || mss.getTileCompVal(TileIdx, c) == null)
		{
			flag = true;
		}
		if (flag || cblk == null)
		{
			return cblk;
		}
		int[] array = (int[])cblk.Data;
		_ = cblk.ulx;
		_ = cblk.uly;
		int w = cblk.w;
		int h = cblk.h;
		int num = (int)mss.getTileCompVal(TileIdx, c);
		int num2 = (1 << sb.magbits) - 1 << 31 - sb.magbits;
		int num3 = ~num2 & 0x7FFFFFFF;
		int num4 = cblk.scanw - w;
		int num5 = cblk.offset + cblk.scanw * (h - 1) + w - 1;
		for (int num6 = h; num6 > 0; num6--)
		{
			int num7 = w;
			while (num7 > 0)
			{
				int num8 = array[num5];
				if ((num8 & num2) == 0)
				{
					array[num5] = (num8 & int.MinValue) | (num8 << num);
				}
				else if ((num8 & num3) != 0)
				{
					array[num5] = (num8 & ~num3) | (1 << 30 - sb.magbits);
				}
				num7--;
				num5--;
			}
			num5 -= num4;
		}
		return cblk;
	}

	internal static DeScalerROI createInstance(CBlkQuantDataSrcDec src, JPXParameters pl, DecodeHelper decSpec)
	{
		pl.checkList('R', JPXParameters.toNameArray(pinfo));
		if (pl.getParameter("Rno_roi") != null || decSpec.rois == null)
		{
			return new DeScalerROI(src, null);
		}
		return new DeScalerROI(src, decSpec.rois);
	}
}
