namespace DocGen.Pdf;

internal class ArbROIMaskGenerator : ROIMaskGenerator
{
	private Quantizer src;

	private int[][] roiMask;

	private int[] maskLineLow;

	private int[] maskLineHigh;

	private int[] paddedMaskLine;

	private new bool roiInTile;

	public ArbROIMaskGenerator(ROI[] rois, int nrc, Quantizer src)
		: base(rois, nrc)
	{
		roiMask = new int[nrc][];
		this.src = src;
	}

	internal override bool getROIMask(DataBlockInt db, Subband sb, int magbits, int c)
	{
		int ulx = db.ulx;
		int uly = db.uly;
		int w = db.w;
		int h = db.h;
		int w2 = sb.w;
		_ = sb.h;
		int[] array = (int[])db.Data;
		if (!tileMaskMade[c])
		{
			makeMask(sb, magbits, c);
			tileMaskMade[c] = true;
		}
		if (!roiInTile)
		{
			return false;
		}
		int[] array2 = roiMask[c];
		int num = (uly + h - 1) * w2 + ulx + w - 1;
		int num2 = w * h - 1;
		int num3 = w2 - w;
		for (int num4 = h; num4 > 0; num4--)
		{
			int num5 = w;
			while (num5 > 0)
			{
				array[num2] = array2[num];
				num5--;
				num--;
				num2--;
			}
			num -= num3;
		}
		return true;
	}

	public override string ToString()
	{
		return "Fast rectangular ROI mask generator";
	}

	internal override void makeMask(Subband sb, int magbits, int c)
	{
		ROI[] array = roi_array;
		int ulcx = sb.ulcx;
		int ulcy = sb.ulcy;
		int w = sb.w;
		int h = sb.h;
		int num = ((w > h) ? w : h);
		int[] array2;
		if (roiMask[c] == null || roiMask[c].Length < w * h)
		{
			roiMask[c] = new int[w * h];
			array2 = roiMask[c];
		}
		else
		{
			array2 = roiMask[c];
			for (int num2 = w * h - 1; num2 >= 0; num2--)
			{
				array2[num2] = 0;
			}
		}
		if (maskLineLow == null || maskLineLow.Length < (num + 1) / 2)
		{
			maskLineLow = new int[(num + 1) / 2];
		}
		if (maskLineHigh == null || maskLineHigh.Length < (num + 1) / 2)
		{
			maskLineHigh = new int[(num + 1) / 2];
		}
		roiInTile = false;
		for (int num3 = array.Length - 1; num3 >= 0; num3--)
		{
			if (array[num3].comp == c)
			{
				if (array[num3].arbShape)
				{
					_ = array[num3].maskPGM;
					int imgULX = src.ImgULX;
					int imgULY = src.ImgULY;
					int num4 = imgULX + src.ImgWidth - 1;
					int num5 = imgULY + src.ImgHeight - 1;
					if (imgULX <= ulcx + w && imgULY <= ulcy + h && num4 >= ulcx && num5 >= ulcy)
					{
						imgULX -= ulcx;
						num4 -= ulcx;
						imgULY -= ulcy;
						num5 -= ulcy;
						int ulx = 0;
						int num6 = 0;
						if (imgULX < 0)
						{
							ulx = -imgULX;
							imgULX = 0;
						}
						if (imgULY < 0)
						{
							num6 = -imgULY;
							imgULY = 0;
						}
						int num7 = ((num4 > w - 1) ? (w - imgULX) : (num4 + 1 - imgULX));
						int num8 = ((num5 > h - 1) ? (h - imgULY) : (num5 + 1 - imgULY));
						DataBlockInt dataBlockInt = new DataBlockInt();
						int num9 = -ImgReaderPGM.DC_OFFSET;
						int num10 = 0;
						dataBlockInt.ulx = ulx;
						dataBlockInt.w = num7;
						dataBlockInt.h = 1;
						int num2 = (imgULY + num8 - 1) * w + imgULX + num7 - 1;
						int num11 = num7;
						int num12 = w - num11;
						for (int num13 = num8; num13 > 0; num13--)
						{
							dataBlockInt.uly = num6 + num13 - 1;
							int[] dataInt = dataBlockInt.DataInt;
							int num14 = num11;
							while (num14 > 0)
							{
								if (dataInt[num14 - 1] != num9)
								{
									array2[num2] = magbits;
									num10++;
								}
								num14--;
								num2--;
							}
							num2 -= num12;
						}
						if (num10 != 0)
						{
							roiInTile = true;
						}
					}
				}
				else if (array[num3].rect)
				{
					int imgULX = array[num3].ulx;
					int imgULY = array[num3].uly;
					int num4 = array[num3].w + imgULX - 1;
					int num5 = array[num3].h + imgULY - 1;
					if (imgULX <= ulcx + w && imgULY <= ulcy + h && num4 >= ulcx && num5 >= ulcy)
					{
						roiInTile = true;
						imgULX -= ulcx;
						num4 -= ulcx;
						imgULY -= ulcy;
						num5 -= ulcy;
						imgULX = ((imgULX >= 0) ? imgULX : 0);
						imgULY = ((imgULY >= 0) ? imgULY : 0);
						int num7 = ((num4 > w - 1) ? (w - imgULX) : (num4 + 1 - imgULX));
						int num8 = ((num5 > h - 1) ? (h - imgULY) : (num5 + 1 - imgULY));
						int num2 = (imgULY + num8 - 1) * w + imgULX + num7 - 1;
						int num11 = num7;
						int num12 = w - num11;
						for (int num13 = num8; num13 > 0; num13--)
						{
							int num14 = num11;
							while (num14 > 0)
							{
								array2[num2] = magbits;
								num14--;
								num2--;
							}
							num2 -= num12;
						}
					}
				}
				else
				{
					int num15 = array[num3].x - ulcx;
					int num16 = array[num3].y - ulcy;
					int r = array[num3].r;
					int num2 = h * w - 1;
					for (int num13 = h - 1; num13 >= 0; num13--)
					{
						int num14 = w - 1;
						while (num14 >= 0)
						{
							if ((num14 - num15) * (num14 - num15) + (num13 - num16) * (num13 - num16) < r * r)
							{
								array2[num2] = magbits;
								roiInTile = true;
							}
							num14--;
							num2--;
						}
					}
				}
			}
		}
		if (sb.isNode)
		{
			WaveletFilter verWFilter = sb.VerWFilter;
			WaveletFilter horWFilter = sb.HorWFilter;
			int num17 = verWFilter.SynLowNegSupport + verWFilter.SynLowPosSupport;
			int num18 = verWFilter.SynHighNegSupport + verWFilter.SynHighPosSupport;
			int num19 = horWFilter.SynLowNegSupport + horWFilter.SynLowPosSupport;
			int num20 = horWFilter.SynHighNegSupport + horWFilter.SynHighPosSupport;
			num17 = ((num17 > num18) ? num17 : num18);
			num19 = ((num19 > num20) ? num19 : num20);
			num17 = ((num17 > num19) ? num17 : num19);
			paddedMaskLine = new int[num + num17];
			if (roiInTile)
			{
				decomp(sb, w, h, c);
			}
		}
	}

	private void decomp(Subband sb, int tilew, int tileh, int c)
	{
		int ulx = sb.ulx;
		int uly = sb.uly;
		int w = sb.w;
		int h = sb.h;
		int num = 0;
		int num2 = 0;
		int[] array = roiMask[c];
		int[] array2 = maskLineLow;
		int[] array3 = maskLineHigh;
		int[] array4 = paddedMaskLine;
		int num3 = 0;
		if (!sb.isNode)
		{
			return;
		}
		WaveletFilter horWFilter = sb.HorWFilter;
		int synLowNegSupport = horWFilter.SynLowNegSupport;
		int synHighNegSupport = horWFilter.SynHighNegSupport;
		int synLowPosSupport = horWFilter.SynLowPosSupport;
		int synHighPosSupport = horWFilter.SynHighPosSupport;
		int num4 = synLowNegSupport + synLowPosSupport + 1;
		int num5 = synHighNegSupport + synHighPosSupport + 1;
		num3 = sb.ulcx % 2;
		int num6;
		int num7;
		if (sb.w % 2 == 0)
		{
			num6 = w / 2 - 1;
			num7 = num6;
		}
		else if (num3 == 0)
		{
			num6 = (w + 1) / 2 - 1;
			num7 = w / 2 - 1;
		}
		else
		{
			num7 = (w + 1) / 2 - 1;
			num6 = w / 2 - 1;
		}
		int num8 = ((synLowNegSupport > synHighNegSupport) ? synLowNegSupport : synHighNegSupport);
		int num9 = ((synLowPosSupport > synHighPosSupport) ? synLowPosSupport : synHighPosSupport);
		for (int num10 = num8 - 1; num10 >= 0; num10--)
		{
			array4[num10] = 0;
		}
		for (int num10 = num8 + w - 1 + num9; num10 >= w; num10--)
		{
			array4[num10] = 0;
		}
		int num11 = (uly + h) * tilew + ulx + w - 1;
		for (int num12 = h - 1; num12 >= 0; num12--)
		{
			num11 -= tilew;
			num2 = num11;
			int num13 = w;
			int num10 = w - 1 + num8;
			while (num13 > 0)
			{
				array4[num10] = array[num2];
				num13--;
				num2--;
				num10--;
			}
			int num14 = num8 + num3 + 2 * num6 + synLowPosSupport;
			num13 = num6;
			while (num13 >= 0)
			{
				num10 = num14;
				int num15 = num4;
				while (num15 > 0)
				{
					int num16 = array4[num10];
					if (num16 > num)
					{
						num = num16;
					}
					num15--;
					num10--;
				}
				array2[num13] = num;
				num = 0;
				num13--;
				num14 -= 2;
			}
			num14 = num8 - num3 + 2 * num7 + 1 + synHighPosSupport;
			num13 = num7;
			while (num13 >= 0)
			{
				num10 = num14;
				int num15 = num5;
				while (num15 > 0)
				{
					int num16 = array4[num10];
					if (num16 > num)
					{
						num = num16;
					}
					num15--;
					num10--;
				}
				array3[num13] = num;
				num = 0;
				num13--;
				num14 -= 2;
			}
			num2 = num11;
			num13 = num7;
			while (num13 >= 0)
			{
				array[num2] = array3[num13];
				num13--;
				num2--;
			}
			num13 = num6;
			while (num13 >= 0)
			{
				array[num2] = array2[num13];
				num13--;
				num2--;
			}
		}
		WaveletFilter verWFilter = sb.VerWFilter;
		synLowNegSupport = verWFilter.SynLowNegSupport;
		synHighNegSupport = verWFilter.SynHighNegSupport;
		synLowPosSupport = verWFilter.SynLowPosSupport;
		synHighPosSupport = verWFilter.SynHighPosSupport;
		num4 = synLowNegSupport + synLowPosSupport + 1;
		num5 = synHighNegSupport + synHighPosSupport + 1;
		num3 = sb.ulcy % 2;
		if (sb.h % 2 == 0)
		{
			num6 = h / 2 - 1;
			num7 = num6;
		}
		else if (sb.ulcy % 2 == 0)
		{
			num6 = (h + 1) / 2 - 1;
			num7 = h / 2 - 1;
		}
		else
		{
			num7 = (h + 1) / 2 - 1;
			num6 = h / 2 - 1;
		}
		num8 = ((synLowNegSupport > synHighNegSupport) ? synLowNegSupport : synHighNegSupport);
		num9 = ((synLowPosSupport > synHighPosSupport) ? synLowPosSupport : synHighPosSupport);
		for (int num10 = num8 - 1; num10 >= 0; num10--)
		{
			array4[num10] = 0;
		}
		for (int num10 = num8 + h - 1 + num9; num10 >= h; num10--)
		{
			array4[num10] = 0;
		}
		num11 = (uly + h - 1) * tilew + ulx + w;
		for (int num12 = w - 1; num12 >= 0; num12--)
		{
			num11--;
			num2 = num11;
			int num13 = h;
			int num10 = num13 - 1 + num8;
			while (num13 > 0)
			{
				array4[num10] = array[num2];
				num13--;
				num2 -= tilew;
				num10--;
			}
			int num14 = num8 + num3 + 2 * num6 + synLowPosSupport;
			num13 = num6;
			while (num13 >= 0)
			{
				num10 = num14;
				int num15 = num4;
				while (num15 > 0)
				{
					int num16 = array4[num10];
					if (num16 > num)
					{
						num = num16;
					}
					num15--;
					num10--;
				}
				array2[num13] = num;
				num = 0;
				num13--;
				num14 -= 2;
			}
			num14 = num8 - num3 + 2 * num7 + 1 + synHighPosSupport;
			num13 = num7;
			while (num13 >= 0)
			{
				num10 = num14;
				int num15 = num5;
				while (num15 > 0)
				{
					int num16 = array4[num10];
					if (num16 > num)
					{
						num = num16;
					}
					num15--;
					num10--;
				}
				array3[num13] = num;
				num = 0;
				num13--;
				num14 -= 2;
			}
			num2 = num11;
			num13 = num7;
			while (num13 >= 0)
			{
				array[num2] = array3[num13];
				num13--;
				num2 -= tilew;
			}
			num13 = num6;
			while (num13 >= 0)
			{
				array[num2] = array2[num13];
				num13--;
				num2 -= tilew;
			}
		}
		if (sb.isNode)
		{
			decomp(sb.HH, tilew, tileh, c);
			decomp(sb.LH, tilew, tileh, c);
			decomp(sb.HL, tilew, tileh, c);
			decomp(sb.LL, tilew, tileh, c);
		}
	}
}
