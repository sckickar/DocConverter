using System;

namespace DocGen.Pdf;

internal class ForwCompTransf : ImgDataAdapter, BlockImageDataSource, ImageData
{
	public const int NONE = 0;

	public const int FORW_RCT = 1;

	public const int FORW_ICT = 2;

	private BlockImageDataSource src;

	private CompTransfSpec cts;

	private AnWTFilterSpec wfs;

	private int transfType;

	private int[] tdepth;

	private DataBlock outBlk;

	private DataBlockInt block0;

	private DataBlockInt block1;

	private DataBlockInt block2;

	internal const char OPT_PREFIX = 'M';

	private static readonly string[][] pinfo = new string[1][] { new string[4] { "Mct", "[<tile index>] [on|off] ...", "Specifies in which tiles to use a multiple component transform. Note that this multiple component transform can only be applied in tiles that contain at least three components and whose components are processed with the same wavelet filters and quantization type. If the wavelet transform is reversible (w5x3 filter), the Reversible Component Transformation (RCT) is applied. If not (w9x7 filter), the Irreversible Component Transformation (ICT) is used.", null } };

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

	internal ForwCompTransf(BlockImageDataSource imgSrc, EncoderSpecs encSpec)
		: base(imgSrc)
	{
		cts = encSpec.cts;
		wfs = encSpec.wfs;
		src = imgSrc;
	}

	public virtual int getFixedPoint(int c)
	{
		return src.getFixedPoint(c);
	}

	public static int[] calcMixedBitDepths(int[] ntdepth, int ttype, int[] tdepth)
	{
		if (ntdepth.Length < 3 && ttype != 0)
		{
			throw new ArgumentException();
		}
		if (tdepth == null)
		{
			tdepth = new int[ntdepth.Length];
		}
		switch (ttype)
		{
		case 0:
			Array.Copy(ntdepth, 0, tdepth, 0, ntdepth.Length);
			break;
		case 1:
			if (ntdepth.Length > 3)
			{
				Array.Copy(ntdepth, 3, tdepth, 3, ntdepth.Length - 3);
			}
			tdepth[0] = MathUtil.log2((1 << ntdepth[0]) + (2 << ntdepth[1]) + (1 << ntdepth[2]) - 1) - 2 + 1;
			tdepth[1] = MathUtil.log2((1 << ntdepth[2]) + (1 << ntdepth[1]) - 1) + 1;
			tdepth[2] = MathUtil.log2((1 << ntdepth[0]) + (1 << ntdepth[1]) - 1) + 1;
			break;
		case 2:
			if (ntdepth.Length > 3)
			{
				Array.Copy(ntdepth, 3, tdepth, 3, ntdepth.Length - 3);
			}
			tdepth[0] = MathUtil.log2((int)Math.Floor((double)(1 << ntdepth[0]) * 0.299072 + (double)(1 << ntdepth[1]) * 0.586914 + (double)(1 << ntdepth[2]) * 0.114014) - 1) + 1;
			tdepth[1] = MathUtil.log2((int)Math.Floor((double)(1 << ntdepth[0]) * 0.168701 + (double)(1 << ntdepth[1]) * 0.331299 + (double)(1 << ntdepth[2]) * 0.5) - 1) + 1;
			tdepth[2] = MathUtil.log2((int)Math.Floor((double)(1 << ntdepth[0]) * 0.5 + (double)(1 << ntdepth[1]) * 0.418701 + (double)(1 << ntdepth[2]) * 0.081299) - 1) + 1;
			break;
		}
		return tdepth;
	}

	private void initForwRCT()
	{
		int tileIdx = TileIdx;
		if (src.NumComps < 3)
		{
			throw new ArgumentException();
		}
		if (src.getTileComponentWidth(tileIdx, 0) != src.getTileComponentWidth(tileIdx, 1) || src.getTileComponentWidth(tileIdx, 0) != src.getTileComponentWidth(tileIdx, 2) || src.getTileComponentHeight(tileIdx, 0) != src.getTileComponentHeight(tileIdx, 1) || src.getTileComponentHeight(tileIdx, 0) != src.getTileComponentHeight(tileIdx, 2))
		{
			throw new ArgumentException("Can not use RCT on components with different dimensions");
		}
		int[] array = new int[src.NumComps];
		for (int num = array.Length - 1; num >= 0; num--)
		{
			array[num] = src.getNomRangeBits(num);
		}
		tdepth = calcMixedBitDepths(array, 1, null);
	}

	private void initForwICT()
	{
		int tileIdx = TileIdx;
		if (src.NumComps < 3)
		{
			throw new ArgumentException();
		}
		if (src.getTileComponentWidth(tileIdx, 0) != src.getTileComponentWidth(tileIdx, 1) || src.getTileComponentWidth(tileIdx, 0) != src.getTileComponentWidth(tileIdx, 2) || src.getTileComponentHeight(tileIdx, 0) != src.getTileComponentHeight(tileIdx, 1) || src.getTileComponentHeight(tileIdx, 0) != src.getTileComponentHeight(tileIdx, 2))
		{
			throw new ArgumentException("Can not use ICT on components with different dimensions");
		}
		int[] array = new int[src.NumComps];
		for (int num = array.Length - 1; num >= 0; num--)
		{
			array[num] = src.getNomRangeBits(num);
		}
		tdepth = calcMixedBitDepths(array, 2, null);
	}

	public override string ToString()
	{
		return transfType switch
		{
			1 => "Forward RCT", 
			2 => "Forward ICT", 
			0 => "No component transformation", 
			_ => throw new ArgumentException("Non JPEG 2000 part I component transformation"), 
		};
	}

	public override int getNomRangeBits(int c)
	{
		switch (transfType)
		{
		case 1:
		case 2:
			return tdepth[c];
		case 0:
			return src.getNomRangeBits(c);
		default:
			throw new ArgumentException("Non JPEG 2000 part I component transformation");
		}
	}

	public virtual DataBlock getCompData(DataBlock blk, int c)
	{
		if (c >= 3 || transfType == 0)
		{
			return src.getCompData(blk, c);
		}
		return getInternCompData(blk, c);
	}

	public virtual DataBlock getInternCompData(DataBlock blk, int c)
	{
		return transfType switch
		{
			0 => src.getInternCompData(blk, c), 
			1 => forwRCT(blk, c), 
			2 => forwICT(blk, c), 
			_ => throw new ArgumentException("Non JPEG 2000 part 1 component transformation for tile: " + tIdx), 
		};
	}

	private DataBlock forwRCT(DataBlock blk, int c)
	{
		int w = blk.w;
		int h = blk.h;
		if (c >= 0 && c <= 2)
		{
			if (blk.DataType != 3)
			{
				if (outBlk == null || outBlk.DataType != 3)
				{
					outBlk = new DataBlockInt();
				}
				outBlk.w = w;
				outBlk.h = h;
				outBlk.ulx = blk.ulx;
				outBlk.uly = blk.uly;
				blk = outBlk;
			}
			int[] array = (int[])blk.Data;
			if (array == null || array.Length < h * w)
			{
				array = (int[])(blk.Data = new int[h * w]);
			}
			if (block0 == null)
			{
				block0 = new DataBlockInt();
			}
			if (block1 == null)
			{
				block1 = new DataBlockInt();
			}
			if (block2 == null)
			{
				block2 = new DataBlockInt();
			}
			block0.w = (block1.w = (block2.w = blk.w));
			block0.h = (block1.h = (block2.h = blk.h));
			block0.ulx = (block1.ulx = (block2.ulx = blk.ulx));
			block0.uly = (block1.uly = (block2.uly = blk.uly));
			block0 = (DataBlockInt)src.getInternCompData(block0, 0);
			int[] array3 = (int[])block0.Data;
			block1 = (DataBlockInt)src.getInternCompData(block1, 1);
			int[] array4 = (int[])block1.Data;
			block2 = (DataBlockInt)src.getInternCompData(block2, 2);
			int[] array5 = (int[])block2.Data;
			blk.progressive = block0.progressive || block1.progressive || block2.progressive;
			blk.offset = 0;
			blk.scanw = w;
			int num = w * h - 1;
			int num2 = block0.offset + (h - 1) * block0.scanw + w - 1;
			int num3 = block1.offset + (h - 1) * block1.scanw + w - 1;
			int num4 = block2.offset + (h - 1) * block2.scanw + w - 1;
			switch (c)
			{
			case 0:
			{
				for (int num5 = h - 1; num5 >= 0; num5--)
				{
					int num6 = num - w;
					while (num > num6)
					{
						array[num] = array3[num] + 2 * array4[num] + array5[num] >> 2;
						num--;
						num2--;
						num3--;
						num4--;
					}
					num2 -= block0.scanw - w;
					num3 -= block1.scanw - w;
					num4 -= block2.scanw - w;
				}
				break;
			}
			case 1:
			{
				for (int num5 = h - 1; num5 >= 0; num5--)
				{
					int num6 = num - w;
					while (num > num6)
					{
						array[num] = array5[num4] - array4[num3];
						num--;
						num3--;
						num4--;
					}
					num3 -= block1.scanw - w;
					num4 -= block2.scanw - w;
				}
				break;
			}
			case 2:
			{
				for (int num5 = h - 1; num5 >= 0; num5--)
				{
					int num6 = num - w;
					while (num > num6)
					{
						array[num] = array3[num2] - array4[num3];
						num--;
						num2--;
						num3--;
					}
					num2 -= block0.scanw - w;
					num3 -= block1.scanw - w;
				}
				break;
			}
			}
			return blk;
		}
		if (c >= 3)
		{
			return src.getInternCompData(blk, c);
		}
		throw new ArgumentException();
	}

	private DataBlock forwICT(DataBlock blk, int c)
	{
		int w = blk.w;
		int h = blk.h;
		if (blk.DataType != 4)
		{
			if (outBlk == null || outBlk.DataType != 4)
			{
				outBlk = new DataBlockFloat();
			}
			outBlk.w = w;
			outBlk.h = h;
			outBlk.ulx = blk.ulx;
			outBlk.uly = blk.uly;
			blk = outBlk;
		}
		float[] array = (float[])blk.Data;
		if (array == null || array.Length < w * h)
		{
			array = (float[])(blk.Data = new float[h * w]);
		}
		if (c >= 0 && c <= 2)
		{
			if (block0 == null)
			{
				block0 = new DataBlockInt();
			}
			if (block1 == null)
			{
				block1 = new DataBlockInt();
			}
			if (block2 == null)
			{
				block2 = new DataBlockInt();
			}
			block0.w = (block1.w = (block2.w = blk.w));
			block0.h = (block1.h = (block2.h = blk.h));
			block0.ulx = (block1.ulx = (block2.ulx = blk.ulx));
			block0.uly = (block1.uly = (block2.uly = blk.uly));
			block0 = (DataBlockInt)src.getInternCompData(block0, 0);
			int[] array3 = (int[])block0.Data;
			block1 = (DataBlockInt)src.getInternCompData(block1, 1);
			int[] array4 = (int[])block1.Data;
			block2 = (DataBlockInt)src.getInternCompData(block2, 2);
			int[] array5 = (int[])block2.Data;
			blk.progressive = block0.progressive || block1.progressive || block2.progressive;
			blk.offset = 0;
			blk.scanw = w;
			int num = w * h - 1;
			int num2 = block0.offset + (h - 1) * block0.scanw + w - 1;
			int num3 = block1.offset + (h - 1) * block1.scanw + w - 1;
			int num4 = block2.offset + (h - 1) * block2.scanw + w - 1;
			switch (c)
			{
			case 0:
			{
				for (int num5 = h - 1; num5 >= 0; num5--)
				{
					int num6 = num - w;
					while (num > num6)
					{
						array[num] = 0.299f * (float)array3[num2] + 0.587f * (float)array4[num3] + 0.114f * (float)array5[num4];
						num--;
						num2--;
						num3--;
						num4--;
					}
					num2 -= block0.scanw - w;
					num3 -= block1.scanw - w;
					num4 -= block2.scanw - w;
				}
				break;
			}
			case 1:
			{
				for (int num5 = h - 1; num5 >= 0; num5--)
				{
					int num6 = num - w;
					while (num > num6)
					{
						array[num] = -27f / 160f * (float)array3[num2] - 0.33126f * (float)array4[num3] + 0.5f * (float)array5[num4];
						num--;
						num2--;
						num3--;
						num4--;
					}
					num2 -= block0.scanw - w;
					num3 -= block1.scanw - w;
					num4 -= block2.scanw - w;
				}
				break;
			}
			case 2:
			{
				for (int num5 = h - 1; num5 >= 0; num5--)
				{
					int num6 = num - w;
					while (num > num6)
					{
						array[num] = 0.5f * (float)array3[num2] - 0.41869f * (float)array4[num3] - 0.08131f * (float)array5[num4];
						num--;
						num2--;
						num3--;
						num4--;
					}
					num2 -= block0.scanw - w;
					num3 -= block1.scanw - w;
					num4 -= block2.scanw - w;
				}
				break;
			}
			}
			return blk;
		}
		if (c >= 3)
		{
			DataBlockInt dataBlockInt = new DataBlockInt(blk.ulx, blk.uly, w, h);
			src.getInternCompData(dataBlockInt, c);
			int[] array6 = (int[])dataBlockInt.Data;
			int num = w * h - 1;
			int num2 = dataBlockInt.offset + (h - 1) * dataBlockInt.scanw + w - 1;
			for (int num5 = h - 1; num5 >= 0; num5--)
			{
				int num6 = num - w;
				while (num > num6)
				{
					array[num] = array6[num2];
					num--;
					num2--;
				}
				num2 += dataBlockInt.w - w;
			}
			blk.progressive = dataBlockInt.progressive;
			blk.offset = 0;
			blk.scanw = w;
			return blk;
		}
		throw new ArgumentException();
	}

	public override void setTile(int x, int y)
	{
		src.setTile(x, y);
		tIdx = TileIdx;
		string text = (string)cts.getTileDef(tIdx);
		if (text.Equals("none"))
		{
			transfType = 0;
			return;
		}
		if (text.Equals("rct"))
		{
			transfType = 1;
			initForwRCT();
			return;
		}
		if (text.Equals("ict"))
		{
			transfType = 2;
			initForwICT();
			return;
		}
		throw new ArgumentException("Component transformation not recognized");
	}

	public override void nextTile()
	{
		src.nextTile();
		tIdx = TileIdx;
		string text = (string)cts.getTileDef(tIdx);
		if (text.Equals("none"))
		{
			transfType = 0;
			return;
		}
		if (text.Equals("rct"))
		{
			transfType = 1;
			initForwRCT();
			return;
		}
		if (text.Equals("ict"))
		{
			transfType = 2;
			initForwICT();
			return;
		}
		throw new ArgumentException("Component transformation not recognized");
	}
}
