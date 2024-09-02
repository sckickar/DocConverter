using System;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal class InvWTFull : WaveletTransformInverse
{
	private int cblkToDecode;

	private CBlkWTDataSrcDec src;

	private int dtype;

	private DataBlock[] reconstructedComps;

	private int[] ndl;

	private Dictionary<int, bool[]> reversible = new Dictionary<int, bool[]>();

	internal InvWTFull(CBlkWTDataSrcDec src, DecodeHelper decSpec)
		: base(src, decSpec)
	{
		this.src = src;
		int numComps = src.NumComps;
		reconstructedComps = new DataBlock[numComps];
		ndl = new int[numComps];
	}

	private bool isSubbandReversible(Subband subband)
	{
		if (subband.isNode)
		{
			if (isSubbandReversible(subband.LL) && isSubbandReversible(subband.HL) && isSubbandReversible(subband.LH) && isSubbandReversible(subband.HH) && ((SubbandSyn)subband).hFilter.Reversible)
			{
				return ((SubbandSyn)subband).vFilter.Reversible;
			}
			return false;
		}
		return true;
	}

	public override bool isReversible(int t, int c)
	{
		if (reversible[t] == null)
		{
			reversible[t] = new bool[NumComps];
			for (int num = reversible[t].Length - 1; num >= 0; num--)
			{
				reversible[t][num] = isSubbandReversible(src.getSynSubbandTree(t, num));
			}
		}
		return reversible[t][c];
	}

	public override int getNomRangeBits(int c)
	{
		return src.getNomRangeBits(c);
	}

	public override int getFixedPoint(int c)
	{
		return src.getFixedPoint(c);
	}

	public override DataBlock getInternCompData(DataBlock blk, int c)
	{
		int tileIdx = TileIdx;
		if (src.getSynSubbandTree(tileIdx, c).HorWFilter == null)
		{
			dtype = 3;
		}
		else
		{
			dtype = src.getSynSubbandTree(tileIdx, c).HorWFilter.DataType;
		}
		if (reconstructedComps[c] == null)
		{
			switch (dtype)
			{
			case 4:
				reconstructedComps[c] = new DataBlockFloat(0, 0, getTileComponentWidth(tileIdx, c), getTileComponentHeight(tileIdx, c));
				break;
			case 3:
				reconstructedComps[c] = new DataBlockInt(0, 0, getTileComponentWidth(tileIdx, c), getTileComponentHeight(tileIdx, c));
				break;
			}
			waveletTreeReconstruction(reconstructedComps[c], src.getSynSubbandTree(tileIdx, c), c);
		}
		if (blk.DataType != dtype)
		{
			blk = ((dtype != 3) ? ((DataBlock)new DataBlockFloat(blk.ulx, blk.uly, blk.w, blk.h)) : ((DataBlock)new DataBlockInt(blk.ulx, blk.uly, blk.w, blk.h)));
		}
		blk.Data = reconstructedComps[c].Data;
		blk.offset = reconstructedComps[c].w * blk.uly + blk.ulx;
		blk.scanw = reconstructedComps[c].w;
		blk.progressive = false;
		return blk;
	}

	public override DataBlock getCompData(DataBlock blk, int c)
	{
		object data = null;
		switch (blk.DataType)
		{
		case 3:
		{
			int[] array2 = (int[])blk.Data;
			if (array2 == null || array2.Length < blk.w * blk.h)
			{
				array2 = new int[blk.w * blk.h];
			}
			data = array2;
			break;
		}
		case 4:
		{
			float[] array = (float[])blk.Data;
			if (array == null || array.Length < blk.w * blk.h)
			{
				array = new float[blk.w * blk.h];
			}
			data = array;
			break;
		}
		}
		blk = getInternCompData(blk, c);
		blk.Data = data;
		blk.offset = 0;
		blk.scanw = blk.w;
		return blk;
	}

	private void wavelet2DReconstruction(DataBlock db, SubbandSyn sb, int c)
	{
		if (sb.w == 0 || sb.h == 0)
		{
			return;
		}
		object data = db.Data;
		int ulx = sb.ulx;
		int uly = sb.uly;
		int w = sb.w;
		int h = sb.h;
		object obj = null;
		switch (sb.HorWFilter.DataType)
		{
		case 3:
			obj = new int[(w >= h) ? w : h];
			break;
		case 4:
			obj = new float[(w >= h) ? w : h];
			break;
		}
		int num = (uly - db.uly) * db.w + ulx - db.ulx;
		if (sb.ulcx % 2 == 0)
		{
			int num2 = 0;
			while (num2 < h)
			{
				Array.Copy((Array)data, num, (Array)obj, 0, w);
				sb.hFilter.synthetize_lpf(obj, 0, (w + 1) / 2, 1, obj, (w + 1) / 2, w / 2, 1, data, num, 1);
				num2++;
				num += db.w;
			}
		}
		else
		{
			int num2 = 0;
			while (num2 < h)
			{
				Array.Copy((Array)data, num, (Array)obj, 0, w);
				sb.hFilter.synthetize_hpf(obj, 0, w / 2, 1, obj, w / 2, (w + 1) / 2, 1, data, num, 1);
				num2++;
				num += db.w;
			}
		}
		num = (uly - db.uly) * db.w + ulx - db.ulx;
		switch (sb.VerWFilter.DataType)
		{
		case 3:
		{
			int[] array3 = (int[])data;
			int[] array4 = (int[])obj;
			int num3;
			if (sb.ulcy % 2 == 0)
			{
				num3 = 0;
				while (num3 < w)
				{
					int num2 = h - 1;
					int num4 = num + num2 * db.w;
					while (num2 >= 0)
					{
						array4[num2] = array3[num4];
						num2--;
						num4 -= db.w;
					}
					sb.vFilter.synthetize_lpf(obj, 0, (h + 1) / 2, 1, obj, (h + 1) / 2, h / 2, 1, data, num, db.w);
					num3++;
					num++;
				}
				break;
			}
			num3 = 0;
			while (num3 < w)
			{
				int num2 = h - 1;
				int num4 = num + num2 * db.w;
				while (num2 >= 0)
				{
					array4[num2] = array3[num4];
					num2--;
					num4 -= db.w;
				}
				sb.vFilter.synthetize_hpf(obj, 0, h / 2, 1, obj, h / 2, (h + 1) / 2, 1, data, num, db.w);
				num3++;
				num++;
			}
			break;
		}
		case 4:
		{
			float[] array = (float[])data;
			float[] array2 = (float[])obj;
			int num3;
			if (sb.ulcy % 2 == 0)
			{
				num3 = 0;
				while (num3 < w)
				{
					int num2 = h - 1;
					int num4 = num + num2 * db.w;
					while (num2 >= 0)
					{
						array2[num2] = array[num4];
						num2--;
						num4 -= db.w;
					}
					sb.vFilter.synthetize_lpf(obj, 0, (h + 1) / 2, 1, obj, (h + 1) / 2, h / 2, 1, data, num, db.w);
					num3++;
					num++;
				}
				break;
			}
			num3 = 0;
			while (num3 < w)
			{
				int num2 = h - 1;
				int num4 = num + num2 * db.w;
				while (num2 >= 0)
				{
					array2[num2] = array[num4];
					num2--;
					num4 -= db.w;
				}
				sb.vFilter.synthetize_hpf(obj, 0, h / 2, 1, obj, h / 2, (h + 1) / 2, 1, data, num, db.w);
				num3++;
				num++;
			}
			break;
		}
		}
	}

	private void waveletTreeReconstruction(DataBlock img, SubbandSyn sb, int c)
	{
		if (!sb.isNode)
		{
			if (sb.w == 0 || sb.h == 0)
			{
				return;
			}
			DataBlock dataBlock = ((dtype != 3) ? ((DataBlock)new DataBlockFloat()) : ((DataBlock)new DataBlockInt()));
			JPXImageCoordinates numCb = sb.numCb;
			object data = img.Data;
			for (int i = 0; i < numCb.y; i++)
			{
				for (int j = 0; j < numCb.x; j++)
				{
					dataBlock = src.getInternCodeBlock(c, i, j, sb, dataBlock);
					object data2 = dataBlock.Data;
					for (int num = dataBlock.h - 1; num >= 0; num--)
					{
						Array.Copy((Array)data2, dataBlock.offset + num * dataBlock.scanw, (Array)data, (dataBlock.uly + num) * img.w + dataBlock.ulx, dataBlock.w);
					}
				}
			}
		}
		else if (sb.isNode)
		{
			waveletTreeReconstruction(img, (SubbandSyn)sb.LL, c);
			if (sb.resLvl <= reslvl - maxImgRes + ndl[c])
			{
				waveletTreeReconstruction(img, (SubbandSyn)sb.HL, c);
				waveletTreeReconstruction(img, (SubbandSyn)sb.LH, c);
				waveletTreeReconstruction(img, (SubbandSyn)sb.HH, c);
				wavelet2DReconstruction(img, sb, c);
			}
		}
	}

	public override int getImplementationType(int c)
	{
		return WaveletTransform_Fields.WT_IMPL_FULL;
	}

	public override void setTile(int x, int y)
	{
		base.setTile(x, y);
		int numComps = src.NumComps;
		int tileIdx = src.TileIdx;
		for (int i = 0; i < numComps; i++)
		{
			ndl[i] = src.getSynSubbandTree(tileIdx, i).resLvl;
		}
		if (reconstructedComps != null)
		{
			for (int num = reconstructedComps.Length - 1; num >= 0; num--)
			{
				reconstructedComps[num] = null;
			}
		}
		cblkToDecode = 0;
		for (int j = 0; j < numComps; j++)
		{
			SubbandSyn synSubbandTree = src.getSynSubbandTree(tileIdx, j);
			for (int k = 0; k <= reslvl - maxImgRes + synSubbandTree.resLvl; k++)
			{
				SubbandSyn subbandSyn;
				if (k == 0)
				{
					subbandSyn = (SubbandSyn)synSubbandTree.getSubbandByIdx(0, 0);
					if (subbandSyn != null)
					{
						cblkToDecode += subbandSyn.numCb.x * subbandSyn.numCb.y;
					}
					continue;
				}
				subbandSyn = (SubbandSyn)synSubbandTree.getSubbandByIdx(k, 1);
				if (subbandSyn != null)
				{
					cblkToDecode += subbandSyn.numCb.x * subbandSyn.numCb.y;
				}
				subbandSyn = (SubbandSyn)synSubbandTree.getSubbandByIdx(k, 2);
				if (subbandSyn != null)
				{
					cblkToDecode += subbandSyn.numCb.x * subbandSyn.numCb.y;
				}
				subbandSyn = (SubbandSyn)synSubbandTree.getSubbandByIdx(k, 3);
				if (subbandSyn != null)
				{
					cblkToDecode += subbandSyn.numCb.x * subbandSyn.numCb.y;
				}
			}
		}
	}

	public override void nextTile()
	{
		base.nextTile();
		int numComps = src.NumComps;
		int tileIdx = src.TileIdx;
		for (int i = 0; i < numComps; i++)
		{
			ndl[i] = src.getSynSubbandTree(tileIdx, i).resLvl;
		}
		if (reconstructedComps != null)
		{
			for (int num = reconstructedComps.Length - 1; num >= 0; num--)
			{
				reconstructedComps[num] = null;
			}
		}
	}
}
