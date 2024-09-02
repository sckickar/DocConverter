using System;

namespace DocGen.Pdf;

internal class InverseComponetTransformation : ImgDataAdapter, BlockImageDataSource, ImageData
{
	public const int NONE = 0;

	public const char OPT_PREFIX = 'M';

	private static readonly string[][] pinfo;

	public const int INV_RCT = 1;

	public const int INV_ICT = 2;

	private BlockImageDataSource src;

	private CompTransfSpec cts;

	private SynWTFilterSpec wfs;

	private int transfType;

	private int[][] outdata = new int[3][];

	private DataBlock block0;

	private DataBlock block1;

	private DataBlock block2;

	private DataBlockInt dbi = new DataBlockInt();

	private int[] utdepth;

	private bool noCompTransf;

	public static string[][] ParameterInfo => pinfo;

	public virtual bool Reversible
	{
		get
		{
			switch (transfType)
			{
			case 0:
			case 1:
				return true;
			case 2:
				return false;
			default:
				throw new ArgumentException("Non JPEG 2000 part I component transformation");
			}
		}
	}

	internal InverseComponetTransformation(BlockImageDataSource imgSrc, DecodeHelper decSpec, int[] utdepth, JPXParameters pl)
		: base(imgSrc)
	{
		cts = decSpec.cts;
		wfs = decSpec.wfs;
		src = imgSrc;
		this.utdepth = utdepth;
		noCompTransf = !pl.getBooleanParameter("comp_transf");
	}

	public override string ToString()
	{
		return transfType switch
		{
			1 => "Inverse RCT", 
			2 => "Inverse ICT", 
			0 => "No component transformation", 
			_ => throw new ArgumentException("Non JPEG 2000 part I component transformation"), 
		};
	}

	public virtual int getFixedPoint(int c)
	{
		return src.getFixedPoint(c);
	}

	public static int[] calcMixedBitDepths(int[] utdepth, int ttype, int[] tdepth)
	{
		if (utdepth.Length < 3 && ttype != 0)
		{
			throw new ArgumentException();
		}
		if (tdepth == null)
		{
			tdepth = new int[utdepth.Length];
		}
		switch (ttype)
		{
		case 0:
			Array.Copy(utdepth, 0, tdepth, 0, utdepth.Length);
			break;
		case 1:
			if (utdepth.Length > 3)
			{
				Array.Copy(utdepth, 3, tdepth, 3, utdepth.Length - 3);
			}
			tdepth[0] = MathUtil.log2((1 << utdepth[0]) + (2 << utdepth[1]) + (1 << utdepth[2]) - 1) - 2 + 1;
			tdepth[1] = MathUtil.log2((1 << utdepth[2]) + (1 << utdepth[1]) - 1) + 1;
			tdepth[2] = MathUtil.log2((1 << utdepth[0]) + (1 << utdepth[1]) - 1) + 1;
			break;
		case 2:
			if (utdepth.Length > 3)
			{
				Array.Copy(utdepth, 3, tdepth, 3, utdepth.Length - 3);
			}
			tdepth[0] = MathUtil.log2((int)Math.Floor((double)(1 << utdepth[0]) * 0.299072 + (double)(1 << utdepth[1]) * 0.586914 + (double)(1 << utdepth[2]) * 0.114014) - 1) + 1;
			tdepth[1] = MathUtil.log2((int)Math.Floor((double)(1 << utdepth[0]) * 0.168701 + (double)(1 << utdepth[1]) * 0.331299 + (double)(1 << utdepth[2]) * 0.5) - 1) + 1;
			tdepth[2] = MathUtil.log2((int)Math.Floor((double)(1 << utdepth[0]) * 0.5 + (double)(1 << utdepth[1]) * 0.418701 + (double)(1 << utdepth[2]) * 0.081299) - 1) + 1;
			break;
		}
		return tdepth;
	}

	public override int getNomRangeBits(int c)
	{
		return utdepth[c];
	}

	public virtual DataBlock getCompData(DataBlock blk, int c)
	{
		if (c >= 3 || transfType == 0 || noCompTransf)
		{
			return src.getCompData(blk, c);
		}
		return getInternCompData(blk, c);
	}

	public virtual DataBlock getInternCompData(DataBlock blk, int c)
	{
		if (noCompTransf)
		{
			return src.getInternCompData(blk, c);
		}
		return transfType switch
		{
			0 => src.getInternCompData(blk, c), 
			1 => invRCT(blk, c), 
			2 => invICT(blk, c), 
			_ => throw new ArgumentException("Non JPEG 2000 part I component transformation"), 
		};
	}

	private DataBlock invRCT(DataBlock blk, int c)
	{
		if (c >= 3 && c < NumComps)
		{
			return src.getInternCompData(blk, c);
		}
		if (outdata[c] == null || dbi.ulx > blk.ulx || dbi.uly > blk.uly || dbi.ulx + dbi.w < blk.ulx + blk.w || dbi.uly + dbi.h < blk.uly + blk.h)
		{
			int w = blk.w;
			int h = blk.h;
			outdata[c] = (int[])blk.Data;
			if (outdata[c] == null || outdata[c].Length != h * w)
			{
				outdata[c] = new int[h * w];
				blk.Data = outdata[c];
			}
			outdata[(c + 1) % 3] = new int[outdata[c].Length];
			outdata[(c + 2) % 3] = new int[outdata[c].Length];
			if (block0 == null || block0.DataType != 3)
			{
				block0 = new DataBlockInt();
			}
			if (block1 == null || block1.DataType != 3)
			{
				block1 = new DataBlockInt();
			}
			if (block2 == null || block2.DataType != 3)
			{
				block2 = new DataBlockInt();
			}
			block0.w = (block1.w = (block2.w = blk.w));
			block0.h = (block1.h = (block2.h = blk.h));
			block0.ulx = (block1.ulx = (block2.ulx = blk.ulx));
			block0.uly = (block1.uly = (block2.uly = blk.uly));
			block0 = (DataBlockInt)src.getInternCompData(block0, 0);
			int[] array = (int[])block0.Data;
			block1 = (DataBlockInt)src.getInternCompData(block1, 1);
			int[] array2 = (int[])block1.Data;
			block2 = (DataBlockInt)src.getInternCompData(block2, 2);
			int[] array3 = (int[])block2.Data;
			blk.progressive = block0.progressive || block1.progressive || block2.progressive;
			blk.offset = 0;
			blk.scanw = w;
			dbi.progressive = blk.progressive;
			dbi.ulx = blk.ulx;
			dbi.uly = blk.uly;
			dbi.w = blk.w;
			dbi.h = blk.h;
			int num = w * h - 1;
			int num2 = block0.offset + (h - 1) * block0.scanw + w - 1;
			int num3 = block1.offset + (h - 1) * block1.scanw + w - 1;
			int num4 = block2.offset + (h - 1) * block2.scanw + w - 1;
			for (int num5 = h - 1; num5 >= 0; num5--)
			{
				int num6 = num - w;
				while (num > num6)
				{
					outdata[1][num] = array[num2] - (array2[num3] + array3[num4] >> 2);
					outdata[0][num] = array3[num4] + outdata[1][num];
					outdata[2][num] = array2[num3] + outdata[1][num];
					num--;
					num2--;
					num3--;
					num4--;
				}
				num2 -= block0.scanw - w;
				num3 -= block1.scanw - w;
				num4 -= block2.scanw - w;
			}
			outdata[c] = null;
		}
		else
		{
			if (c < 0 || c >= 3)
			{
				throw new ArgumentException();
			}
			blk.Data = outdata[c];
			blk.progressive = dbi.progressive;
			blk.offset = (blk.uly - dbi.uly) * dbi.w + blk.ulx - dbi.ulx;
			blk.scanw = dbi.w;
			outdata[c] = null;
		}
		return blk;
	}

	private DataBlock invICT(DataBlock blk, int c)
	{
		if (c >= 3 && c < NumComps)
		{
			int w = blk.w;
			int h = blk.h;
			int[] array = (int[])blk.Data;
			if (array == null)
			{
				array = (int[])(blk.Data = new int[h * w]);
			}
			DataBlockFloat dataBlockFloat = new DataBlockFloat(blk.ulx, blk.uly, w, h);
			src.getInternCompData(dataBlockFloat, c);
			float[] array3 = (float[])dataBlockFloat.Data;
			int num = w * h - 1;
			int num2 = dataBlockFloat.offset + (h - 1) * dataBlockFloat.scanw + w - 1;
			for (int num3 = h - 1; num3 >= 0; num3--)
			{
				int num4 = num - w;
				while (num > num4)
				{
					array[num] = (int)array3[num2];
					num--;
					num2--;
				}
				num2 -= dataBlockFloat.scanw - w;
			}
			blk.progressive = dataBlockFloat.progressive;
			blk.offset = 0;
			blk.scanw = w;
		}
		else if (outdata[c] == null || dbi.ulx > blk.ulx || dbi.uly > blk.uly || dbi.ulx + dbi.w < blk.ulx + blk.w || dbi.uly + dbi.h < blk.uly + blk.h)
		{
			int w2 = blk.w;
			int h2 = blk.h;
			outdata[c] = (int[])blk.Data;
			if (outdata[c] == null || outdata[c].Length != w2 * h2)
			{
				outdata[c] = new int[h2 * w2];
				blk.Data = outdata[c];
			}
			outdata[(c + 1) % 3] = new int[outdata[c].Length];
			outdata[(c + 2) % 3] = new int[outdata[c].Length];
			if (block0 == null || block0.DataType != 4)
			{
				block0 = new DataBlockFloat();
			}
			if (block2 == null || block2.DataType != 4)
			{
				block2 = new DataBlockFloat();
			}
			if (block1 == null || block1.DataType != 4)
			{
				block1 = new DataBlockFloat();
			}
			block0.w = (block2.w = (block1.w = blk.w));
			block0.h = (block2.h = (block1.h = blk.h));
			block0.ulx = (block2.ulx = (block1.ulx = blk.ulx));
			block0.uly = (block2.uly = (block1.uly = blk.uly));
			block0 = (DataBlockFloat)src.getInternCompData(block0, 0);
			float[] array4 = (float[])block0.Data;
			block2 = (DataBlockFloat)src.getInternCompData(block2, 1);
			float[] array5 = (float[])block2.Data;
			block1 = (DataBlockFloat)src.getInternCompData(block1, 2);
			float[] array6 = (float[])block1.Data;
			blk.progressive = block0.progressive || block1.progressive || block2.progressive;
			blk.offset = 0;
			blk.scanw = w2;
			dbi.progressive = blk.progressive;
			dbi.ulx = blk.ulx;
			dbi.uly = blk.uly;
			dbi.w = blk.w;
			dbi.h = blk.h;
			int num5 = w2 * h2 - 1;
			int num6 = block0.offset + (h2 - 1) * block0.scanw + w2 - 1;
			int num7 = block2.offset + (h2 - 1) * block2.scanw + w2 - 1;
			int num8 = block1.offset + (h2 - 1) * block1.scanw + w2 - 1;
			for (int num9 = h2 - 1; num9 >= 0; num9--)
			{
				int num10 = num5 - w2;
				while (num5 > num10)
				{
					outdata[0][num5] = (int)(array4[num6] + 1.402f * array6[num8] + 0.5f);
					outdata[1][num5] = (int)(array4[num6] - 0.34413f * array5[num7] - 0.71414f * array6[num8] + 0.5f);
					outdata[2][num5] = (int)(array4[num6] + 1.772f * array5[num7] + 0.5f);
					num5--;
					num6--;
					num7--;
					num8--;
				}
				num6 -= block0.scanw - w2;
				num7 -= block2.scanw - w2;
				num8 -= block1.scanw - w2;
			}
			outdata[c] = null;
		}
		else
		{
			if (c < 0 || c > 3)
			{
				throw new ArgumentException();
			}
			blk.Data = outdata[c];
			blk.progressive = dbi.progressive;
			blk.offset = (blk.uly - dbi.uly) * dbi.w + blk.ulx - dbi.ulx;
			blk.scanw = dbi.w;
			outdata[c] = null;
		}
		return blk;
	}

	public override void setTile(int x, int y)
	{
		src.setTile(x, y);
		tIdx = TileIdx;
		if ((int)cts.getTileDef(tIdx) == 0)
		{
			transfType = 0;
			return;
		}
		int num = ((src.NumComps > 3) ? 3 : src.NumComps);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 += (wfs.isReversible(tIdx, i) ? 1 : 0);
		}
		switch (num2)
		{
		case 3:
			transfType = 1;
			break;
		case 0:
			transfType = 2;
			break;
		default:
			throw new ArgumentException("Wavelet transformation and component transformation not coherent in tile" + tIdx);
		}
	}

	public override void nextTile()
	{
		src.nextTile();
		tIdx = TileIdx;
		if ((int)cts.getTileDef(tIdx) == 0)
		{
			transfType = 0;
			return;
		}
		int num = ((src.NumComps > 3) ? 3 : src.NumComps);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 += (wfs.isReversible(tIdx, i) ? 1 : 0);
		}
		switch (num2)
		{
		case 3:
			transfType = 1;
			break;
		case 0:
			transfType = 2;
			break;
		default:
			throw new ArgumentException("Wavelet transformation and component transformation not coherent in tile" + tIdx);
		}
	}
}
