using System;

namespace DocGen.Pdf;

internal class ForwWTFull : ForwardWT
{
	private bool intData;

	private SubbandAn[][] subbTrees;

	private BlockImageDataSource src;

	private int cb0x;

	private int cb0y;

	private IntegerSpec dls;

	private AnWTFilterSpec filters;

	private CBlkSizeSpec cblks;

	private PrecinctSizeSpec pss;

	private DataBlock[] decomposedComps;

	private int[] lastn;

	private int[] lastm;

	internal SubbandAn[] currentSubband;

	internal JPXImageCoordinates ncblks;

	public override int CbULX => cb0x;

	public override int CbULY => cb0y;

	internal ForwWTFull(BlockImageDataSource src, EncoderSpecs encSpec, int cb0x, int cb0y)
		: base(src)
	{
		this.src = src;
		this.cb0x = cb0x;
		this.cb0y = cb0y;
		dls = encSpec.dls;
		filters = encSpec.wfs;
		cblks = encSpec.cblks;
		pss = encSpec.pss;
		int numComps = src.NumComps;
		int numTiles = src.getNumTiles();
		currentSubband = new SubbandAn[numComps];
		decomposedComps = new DataBlock[numComps];
		subbTrees = new SubbandAn[numTiles][];
		for (int i = 0; i < numTiles; i++)
		{
			subbTrees[i] = new SubbandAn[numComps];
		}
		lastn = new int[numComps];
		lastm = new int[numComps];
	}

	public override int getImplementationType(int c)
	{
		return WaveletTransform_Fields.WT_IMPL_FULL;
	}

	public override int getDecompLevels(int t, int c)
	{
		return (int)dls.getTileCompVal(t, c);
	}

	public override int getDecomp(int t, int c)
	{
		return 0;
	}

	public override AnWTFilter[] getHorAnWaveletFilters(int t, int c)
	{
		return filters.getHFilters(t, c);
	}

	public override AnWTFilter[] getVertAnWaveletFilters(int t, int c)
	{
		return filters.getVFilters(t, c);
	}

	public override bool isReversible(int t, int c)
	{
		return filters.isReversible(t, c);
	}

	public override int getFixedPoint(int c)
	{
		return src.getFixedPoint(c);
	}

	public override CBlkWTData getNextInternCodeBlock(int c, CBlkWTData cblk)
	{
		intData = filters.getWTDataType(tIdx, c) == 3;
		if (decomposedComps[c] == null)
		{
			int tileComponentWidth = getTileComponentWidth(tIdx, c);
			int tileComponentHeight = getTileComponentHeight(tIdx, c);
			DataBlock dataBlock;
			if (intData)
			{
				decomposedComps[c] = new DataBlockInt(0, 0, tileComponentWidth, tileComponentHeight);
				dataBlock = new DataBlockInt();
			}
			else
			{
				decomposedComps[c] = new DataBlockFloat(0, 0, tileComponentWidth, tileComponentHeight);
				dataBlock = new DataBlockFloat();
			}
			object data = decomposedComps[c].Data;
			int ulx = (dataBlock.ulx = getCompUpperLeftCornerX(c));
			dataBlock.w = tileComponentWidth;
			dataBlock.h = 1;
			int num = getCompUpperLeftCornerY(c);
			int num2 = 0;
			while (num2 < tileComponentHeight)
			{
				dataBlock.uly = num;
				dataBlock.ulx = ulx;
				dataBlock = src.getInternCompData(dataBlock, c);
				Array.Copy((Array)dataBlock.Data, dataBlock.offset, (Array)data, num2 * tileComponentWidth, tileComponentWidth);
				num2++;
				num++;
			}
			waveletTreeDecomposition(decomposedComps[c], getAnSubbandTree(tIdx, c), c);
			currentSubband[c] = getNextSubband(c);
			lastn[c] = -1;
			lastm[c] = 0;
		}
		while (true)
		{
			ncblks = currentSubband[c].numCb;
			lastn[c]++;
			if (lastn[c] == ncblks.x)
			{
				lastn[c] = 0;
				lastm[c]++;
			}
			if (lastm[c] < ncblks.y)
			{
				break;
			}
			currentSubband[c] = getNextSubband(c);
			lastn[c] = -1;
			lastm[c] = 0;
			if (currentSubband[c] == null)
			{
				decomposedComps[c] = null;
				return null;
			}
		}
		int num3 = cb0x;
		int num4 = cb0y;
		switch (currentSubband[c].sbandIdx)
		{
		case 1:
			num3 = 0;
			break;
		case 2:
			num4 = 0;
			break;
		case 3:
			num3 = 0;
			num4 = 0;
			break;
		}
		if (cblk == null)
		{
			cblk = ((!intData) ? ((CBlkWTData)new CBlkWTDataFloat()) : ((CBlkWTData)new CBlkWTDataInt()));
		}
		int num5 = lastn[c];
		int num6 = lastm[c];
		SubbandAn subbandAn = currentSubband[c];
		cblk.n = num5;
		cblk.m = num6;
		cblk.sb = subbandAn;
		int num7 = (subbandAn.ulcx - num3 + subbandAn.nomCBlkW) / subbandAn.nomCBlkW - 1;
		int num8 = (subbandAn.ulcy - num4 + subbandAn.nomCBlkH) / subbandAn.nomCBlkH - 1;
		if (num5 == 0)
		{
			cblk.ulx = subbandAn.ulx;
		}
		else
		{
			cblk.ulx = (num7 + num5) * subbandAn.nomCBlkW - (subbandAn.ulcx - num3) + subbandAn.ulx;
		}
		if (num6 == 0)
		{
			cblk.uly = subbandAn.uly;
		}
		else
		{
			cblk.uly = (num8 + num6) * subbandAn.nomCBlkH - (subbandAn.ulcy - num4) + subbandAn.uly;
		}
		if (num5 < ncblks.x - 1)
		{
			cblk.w = (num7 + num5 + 1) * subbandAn.nomCBlkW - (subbandAn.ulcx - num3) + subbandAn.ulx - cblk.ulx;
		}
		else
		{
			cblk.w = subbandAn.ulx + subbandAn.w - cblk.ulx;
		}
		if (num6 < ncblks.y - 1)
		{
			cblk.h = (num8 + num6 + 1) * subbandAn.nomCBlkH - (subbandAn.ulcy - num4) + subbandAn.uly - cblk.uly;
		}
		else
		{
			cblk.h = subbandAn.uly + subbandAn.h - cblk.uly;
		}
		cblk.wmseScaling = 1f;
		cblk.offset = cblk.uly * decomposedComps[c].w + cblk.ulx;
		cblk.scanw = decomposedComps[c].w;
		cblk.Data = decomposedComps[c].Data;
		return cblk;
	}

	public override CBlkWTData getNextCodeBlock(int c, CBlkWTData cblk)
	{
		intData = filters.getWTDataType(tIdx, c) == 3;
		object obj = null;
		if (cblk != null)
		{
			obj = cblk.Data;
		}
		cblk = getNextInternCodeBlock(c, cblk);
		if (cblk == null)
		{
			return null;
		}
		if (intData)
		{
			int[] array = (int[])obj;
			if (array == null || array.Length < cblk.w * cblk.h)
			{
				obj = new int[cblk.w * cblk.h];
			}
		}
		else
		{
			float[] array2 = (float[])obj;
			if (array2 == null || array2.Length < cblk.w * cblk.h)
			{
				obj = new float[cblk.w * cblk.h];
			}
		}
		object data = cblk.Data;
		int w = cblk.w;
		int num = w * (cblk.h - 1);
		int num2 = cblk.offset + (cblk.h - 1) * cblk.scanw;
		while (num >= 0)
		{
			Array.Copy((Array)data, num2, (Array)obj, num, w);
			num -= w;
			num2 -= cblk.scanw;
		}
		cblk.Data = obj;
		cblk.offset = 0;
		cblk.scanw = w;
		return cblk;
	}

	public override int getDataType(int t, int c)
	{
		return filters.getWTDataType(t, c);
	}

	private SubbandAn getNextSubband(int c)
	{
		int num = 1;
		int num2 = 0;
		int num3 = num;
		SubbandAn subbandAn = currentSubband[c];
		if (subbandAn == null)
		{
			subbandAn = getAnSubbandTree(tIdx, c);
			if (!subbandAn.isNode)
			{
				return subbandAn;
			}
		}
		while (subbandAn != null)
		{
			if (!subbandAn.isNode)
			{
				switch (subbandAn.orientation)
				{
				case 3:
					subbandAn = (SubbandAn)subbandAn.Parent.LH;
					num3 = num;
					break;
				case 2:
					subbandAn = (SubbandAn)subbandAn.Parent.HL;
					num3 = num;
					break;
				case 1:
					subbandAn = (SubbandAn)subbandAn.Parent.LL;
					num3 = num;
					break;
				case 0:
					subbandAn = (SubbandAn)subbandAn.Parent;
					num3 = num2;
					break;
				}
			}
			else if (subbandAn.isNode)
			{
				if (num3 == num)
				{
					subbandAn = (SubbandAn)subbandAn.HH;
				}
				else if (num3 == num2)
				{
					switch (subbandAn.orientation)
					{
					case 3:
						subbandAn = (SubbandAn)subbandAn.Parent.LH;
						num3 = num;
						break;
					case 2:
						subbandAn = (SubbandAn)subbandAn.Parent.HL;
						num3 = num;
						break;
					case 1:
						subbandAn = (SubbandAn)subbandAn.Parent.LL;
						num3 = num;
						break;
					case 0:
						subbandAn = (SubbandAn)subbandAn.Parent;
						num3 = num2;
						break;
					}
				}
			}
			if (subbandAn == null || !subbandAn.isNode)
			{
				break;
			}
		}
		return subbandAn;
	}

	private void waveletTreeDecomposition(DataBlock band, SubbandAn subband, int c)
	{
		if (subband.isNode)
		{
			wavelet2DDecomposition(band, subband, c);
			waveletTreeDecomposition(band, (SubbandAn)subband.HH, c);
			waveletTreeDecomposition(band, (SubbandAn)subband.LH, c);
			waveletTreeDecomposition(band, (SubbandAn)subband.HL, c);
			waveletTreeDecomposition(band, (SubbandAn)subband.LL, c);
		}
	}

	private void wavelet2DDecomposition(DataBlock band, SubbandAn subband, int c)
	{
		if (subband.w == 0 || subband.h == 0)
		{
			return;
		}
		int ulx = subband.ulx;
		int uly = subband.uly;
		int w = subband.w;
		int h = subband.h;
		int tileComponentWidth = getTileComponentWidth(tIdx, c);
		getTileComponentHeight(tIdx, c);
		if (intData)
		{
			int[] array = new int[Math.Max(w, h)];
			int[] dataInt = ((DataBlockInt)band).DataInt;
			if (subband.ulcy % 2 == 0)
			{
				for (int i = 0; i < w; i++)
				{
					int num = uly * tileComponentWidth + ulx + i;
					for (int j = 0; j < h; j++)
					{
						array[j] = dataInt[num + j * tileComponentWidth];
					}
					subband.vFilter.analyze_lpf(array, 0, h, 1, dataInt, num, tileComponentWidth, dataInt, num + (h + 1) / 2 * tileComponentWidth, tileComponentWidth);
				}
			}
			else
			{
				for (int i = 0; i < w; i++)
				{
					int num = uly * tileComponentWidth + ulx + i;
					for (int j = 0; j < h; j++)
					{
						array[j] = dataInt[num + j * tileComponentWidth];
					}
					subband.vFilter.analyze_hpf(array, 0, h, 1, dataInt, num, tileComponentWidth, dataInt, num + h / 2 * tileComponentWidth, tileComponentWidth);
				}
			}
			if (subband.ulcx % 2 == 0)
			{
				for (int j = 0; j < h; j++)
				{
					int num = (uly + j) * tileComponentWidth + ulx;
					for (int i = 0; i < w; i++)
					{
						array[i] = dataInt[num + i];
					}
					subband.hFilter.analyze_lpf(array, 0, w, 1, dataInt, num, 1, dataInt, num + (w + 1) / 2, 1);
				}
				return;
			}
			for (int j = 0; j < h; j++)
			{
				int num = (uly + j) * tileComponentWidth + ulx;
				for (int i = 0; i < w; i++)
				{
					array[i] = dataInt[num + i];
				}
				subband.hFilter.analyze_hpf(array, 0, w, 1, dataInt, num, 1, dataInt, num + w / 2, 1);
			}
			return;
		}
		float[] array2 = new float[Math.Max(w, h)];
		float[] dataFloat = ((DataBlockFloat)band).DataFloat;
		if (subband.ulcy % 2 == 0)
		{
			for (int k = 0; k < w; k++)
			{
				int num2 = uly * tileComponentWidth + ulx + k;
				for (int l = 0; l < h; l++)
				{
					array2[l] = dataFloat[num2 + l * tileComponentWidth];
				}
				subband.vFilter.analyze_lpf(array2, 0, h, 1, dataFloat, num2, tileComponentWidth, dataFloat, num2 + (h + 1) / 2 * tileComponentWidth, tileComponentWidth);
			}
		}
		else
		{
			for (int k = 0; k < w; k++)
			{
				int num2 = uly * tileComponentWidth + ulx + k;
				for (int l = 0; l < h; l++)
				{
					array2[l] = dataFloat[num2 + l * tileComponentWidth];
				}
				subband.vFilter.analyze_hpf(array2, 0, h, 1, dataFloat, num2, tileComponentWidth, dataFloat, num2 + h / 2 * tileComponentWidth, tileComponentWidth);
			}
		}
		if (subband.ulcx % 2 == 0)
		{
			for (int l = 0; l < h; l++)
			{
				int num2 = (uly + l) * tileComponentWidth + ulx;
				for (int k = 0; k < w; k++)
				{
					array2[k] = dataFloat[num2 + k];
				}
				subband.hFilter.analyze_lpf(array2, 0, w, 1, dataFloat, num2, 1, dataFloat, num2 + (w + 1) / 2, 1);
			}
			return;
		}
		for (int l = 0; l < h; l++)
		{
			int num2 = (uly + l) * tileComponentWidth + ulx;
			for (int k = 0; k < w; k++)
			{
				array2[k] = dataFloat[num2 + k];
			}
			subband.hFilter.analyze_hpf(array2, 0, w, 1, dataFloat, num2, 1, dataFloat, num2 + w / 2, 1);
		}
	}

	public override void setTile(int x, int y)
	{
		base.setTile(x, y);
		if (decomposedComps != null)
		{
			for (int num = decomposedComps.Length - 1; num >= 0; num--)
			{
				decomposedComps[num] = null;
				currentSubband[num] = null;
			}
		}
	}

	public override void nextTile()
	{
		base.nextTile();
		if (decomposedComps != null)
		{
			for (int num = decomposedComps.Length - 1; num >= 0; num--)
			{
				decomposedComps[num] = null;
				currentSubband[num] = null;
			}
		}
	}

	public override SubbandAn getAnSubbandTree(int t, int c)
	{
		if (subbTrees[t][c] == null)
		{
			SubbandAn[] obj = subbTrees[t];
			int tileComponentWidth = getTileComponentWidth(t, c);
			int tileComponentHeight = getTileComponentHeight(t, c);
			int compUpperLeftCornerX = getCompUpperLeftCornerX(c);
			int compUpperLeftCornerY = getCompUpperLeftCornerY(c);
			int decompLevels = getDecompLevels(t, c);
			WaveletFilter[] horAnWaveletFilters = getHorAnWaveletFilters(t, c);
			WaveletFilter[] hfilters = horAnWaveletFilters;
			horAnWaveletFilters = getVertAnWaveletFilters(t, c);
			obj[c] = new SubbandAn(tileComponentWidth, tileComponentHeight, compUpperLeftCornerX, compUpperLeftCornerY, decompLevels, hfilters, horAnWaveletFilters);
			initSubbandsFields(t, c, subbTrees[t][c]);
		}
		return subbTrees[t][c];
	}

	private void initSubbandsFields(int t, int c, Subband sb)
	{
		int cBlkWidth = cblks.getCBlkWidth(3, t, c);
		int cBlkHeight = cblks.getCBlkHeight(3, t, c);
		if (!sb.isNode)
		{
			int pPX = pss.getPPX(t, c, sb.resLvl);
			int pPY = pss.getPPY(t, c, sb.resLvl);
			if (pPX != 65535 || pPY != 65535)
			{
				int num = MathUtil.log2(pPX);
				int num2 = MathUtil.log2(pPY);
				int num3 = MathUtil.log2(cBlkWidth);
				int num4 = MathUtil.log2(cBlkHeight);
				if (sb.resLvl == 0)
				{
					sb.nomCBlkW = ((num3 < num) ? (1 << num3) : (1 << num));
					sb.nomCBlkH = ((num4 < num2) ? (1 << num4) : (1 << num2));
				}
				else
				{
					sb.nomCBlkW = ((num3 < num - 1) ? (1 << num3) : (1 << num - 1));
					sb.nomCBlkH = ((num4 < num2 - 1) ? (1 << num4) : (1 << num2 - 1));
				}
			}
			else
			{
				sb.nomCBlkW = cBlkWidth;
				sb.nomCBlkH = cBlkHeight;
			}
			if (sb.numCb == null)
			{
				sb.numCb = new JPXImageCoordinates();
			}
			if (sb.w != 0 && sb.h != 0)
			{
				int num5 = cb0x;
				int num6 = cb0y;
				switch (sb.sbandIdx)
				{
				case 1:
					num5 = 0;
					break;
				case 2:
					num6 = 0;
					break;
				case 3:
					num5 = 0;
					num6 = 0;
					break;
				}
				if (sb.ulcx - num5 < 0 || sb.ulcy - num6 < 0)
				{
					throw new ArgumentException("Invalid code-blocks partition origin or image offset in the reference grid.");
				}
				int num7 = sb.ulcx - num5 + sb.nomCBlkW;
				sb.numCb.x = (num7 + sb.w - 1) / sb.nomCBlkW - (num7 / sb.nomCBlkW - 1);
				num7 = sb.ulcy - num6 + sb.nomCBlkH;
				sb.numCb.y = (num7 + sb.h - 1) / sb.nomCBlkH - (num7 / sb.nomCBlkH - 1);
			}
			else
			{
				sb.numCb.x = (sb.numCb.y = 0);
			}
		}
		else
		{
			initSubbandsFields(t, c, sb.LL);
			initSubbandsFields(t, c, sb.HL);
			initSubbandsFields(t, c, sb.LH);
			initSubbandsFields(t, c, sb.HH);
		}
	}
}
