namespace DocGen.Pdf;

internal class RectROIMaskGenerator : ROIMaskGenerator
{
	private int[] ulxs;

	private int[] ulys;

	private int[] lrxs;

	private int[] lrys;

	private int[] nrROIs;

	private SubbandRectROIMask[] sMasks;

	public RectROIMaskGenerator(ROI[] ROIs, int nrc)
		: base(ROIs, nrc)
	{
		int num = ROIs.Length;
		nrROIs = new int[nrc];
		sMasks = new SubbandRectROIMask[nrc];
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			nrROIs[ROIs[num2].comp]++;
		}
	}

	internal override bool getROIMask(DataBlockInt db, Subband sb, int magbits, int c)
	{
		int ulx = db.ulx;
		int uly = db.uly;
		int w = db.w;
		int h = db.h;
		int[] dataInt = db.DataInt;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		if (!tileMaskMade[c])
		{
			makeMask(sb, magbits, c);
			tileMaskMade[c] = true;
		}
		if (!roiInTile)
		{
			return false;
		}
		SubbandRectROIMask subbandRectROIMask = (SubbandRectROIMask)sMasks[c].getSubbandRectROIMask(ulx, uly);
		int[] array = subbandRectROIMask.ulxs;
		int[] array2 = subbandRectROIMask.ulys;
		int[] array3 = subbandRectROIMask.lrxs;
		int[] array4 = subbandRectROIMask.lrys;
		int num5 = array.Length - 1;
		ulx -= subbandRectROIMask.ulx;
		uly -= subbandRectROIMask.uly;
		for (int num6 = num5; num6 >= 0; num6--)
		{
			num = array[num6] - ulx;
			if (num < 0)
			{
				num = 0;
			}
			else if (num >= w)
			{
				num = w;
			}
			num2 = array2[num6] - uly;
			if (num2 < 0)
			{
				num2 = 0;
			}
			else if (num2 >= h)
			{
				num2 = h;
			}
			num3 = array3[num6] - ulx;
			if (num3 < 0)
			{
				num3 = -1;
			}
			else if (num3 >= w)
			{
				num3 = w - 1;
			}
			num4 = array4[num6] - uly;
			if (num4 < 0)
			{
				num4 = -1;
			}
			else if (num4 >= h)
			{
				num4 = h - 1;
			}
			int num7 = w * num4 + num3;
			int num8 = num3 - num;
			int num9 = w - num8 - 1;
			for (int num10 = num4 - num2; num10 >= 0; num10--)
			{
				int num11 = num8;
				while (num11 >= 0)
				{
					dataInt[num7] = magbits;
					num11--;
					num7--;
				}
				num7 -= num9;
			}
		}
		return true;
	}

	public override string ToString()
	{
		return "Fast rectangular ROI mask generator";
	}

	internal override void makeMask(Subband sb, int magbits, int n)
	{
		int num = nrROIs[n];
		int ulcx = sb.ulcx;
		int ulcy = sb.ulcy;
		int w = sb.w;
		int h = sb.h;
		ROI[] array = roi_array;
		ulxs = new int[num];
		ulys = new int[num];
		lrxs = new int[num];
		lrys = new int[num];
		num = 0;
		for (int num2 = array.Length - 1; num2 >= 0; num2--)
		{
			if (array[num2].comp == n)
			{
				int ulx = array[num2].ulx;
				int uly = array[num2].uly;
				int num3 = array[num2].w + ulx - 1;
				int num4 = array[num2].h + uly - 1;
				if (ulx <= ulcx + w - 1 && uly <= ulcy + h - 1 && num3 >= ulcx && num4 >= ulcy)
				{
					ulx -= ulcx;
					num3 -= ulcx;
					uly -= ulcy;
					num4 -= ulcy;
					ulx = ((ulx >= 0) ? ulx : 0);
					uly = ((uly >= 0) ? uly : 0);
					num3 = ((num3 > w - 1) ? (w - 1) : num3);
					num4 = ((num4 > h - 1) ? (h - 1) : num4);
					ulxs[num] = ulx;
					ulys[num] = uly;
					lrxs[num] = num3;
					lrys[num] = num4;
					num++;
				}
			}
		}
		if (num == 0)
		{
			roiInTile = false;
		}
		else
		{
			roiInTile = true;
		}
		sMasks[n] = new SubbandRectROIMask(sb, ulxs, ulys, lrxs, lrys, num);
	}
}
