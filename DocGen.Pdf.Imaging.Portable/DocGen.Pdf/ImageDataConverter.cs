using System;

namespace DocGen.Pdf;

internal class ImageDataConverter : ImgDataAdapter, BlockImageDataSource, ImageData
{
	private DataBlock srcBlk = new DataBlockInt();

	private BlockImageDataSource src;

	private int fp;

	internal ImageDataConverter(BlockImageDataSource imgSrc, int fp)
		: base(imgSrc)
	{
		src = imgSrc;
		this.fp = fp;
	}

	internal ImageDataConverter(BlockImageDataSource imgSrc)
		: base(imgSrc)
	{
		src = imgSrc;
		fp = 0;
	}

	public virtual int getFixedPoint(int c)
	{
		return fp;
	}

	public virtual DataBlock getCompData(DataBlock blk, int c)
	{
		return getData(blk, c, intern: false);
	}

	public DataBlock getInternCompData(DataBlock blk, int c)
	{
		return getData(blk, c, intern: true);
	}

	private DataBlock getData(DataBlock blk, int c, bool intern)
	{
		int dataType = blk.DataType;
		DataBlock dataBlock;
		if (dataType == srcBlk.DataType)
		{
			dataBlock = blk;
		}
		else
		{
			dataBlock = srcBlk;
			dataBlock.ulx = blk.ulx;
			dataBlock.uly = blk.uly;
			dataBlock.w = blk.w;
			dataBlock.h = blk.h;
		}
		if (intern)
		{
			srcBlk = src.getInternCompData(dataBlock, c);
		}
		else
		{
			srcBlk = src.getCompData(dataBlock, c);
		}
		if (srcBlk.DataType == dataType)
		{
			return srcBlk;
		}
		int w = srcBlk.w;
		int h = srcBlk.h;
		switch (dataType)
		{
		case 4:
		{
			float[] array4 = (float[])blk.Data;
			if (array4 == null || array4.Length < w * h)
			{
				array4 = (float[])(blk.Data = new float[w * h]);
			}
			blk.scanw = srcBlk.w;
			blk.offset = 0;
			blk.progressive = srcBlk.progressive;
			int[] array6 = (int[])srcBlk.Data;
			fp = src.getFixedPoint(c);
			int num2;
			int num3;
			int num4;
			if (fp != 0)
			{
				float num = 1f / (float)(1 << fp);
				num2 = h - 1;
				num3 = w * h - 1;
				num4 = srcBlk.offset + (h - 1) * srcBlk.scanw + w - 1;
				while (num2 >= 0)
				{
					int num5 = num3 - w;
					while (num3 > num5)
					{
						array4[num3] = (float)array6[num4] * num;
						num3--;
						num4--;
					}
					num4 -= srcBlk.scanw - w;
					num2--;
				}
				break;
			}
			num2 = h - 1;
			num3 = w * h - 1;
			num4 = srcBlk.offset + (h - 1) * srcBlk.scanw + w - 1;
			while (num2 >= 0)
			{
				int num5 = num3 - w;
				while (num3 > num5)
				{
					array4[num3] = array6[num4];
					num3--;
					num4--;
				}
				num4 -= srcBlk.scanw - w;
				num2--;
			}
			break;
		}
		case 3:
		{
			int[] array = (int[])blk.Data;
			if (array == null || array.Length < w * h)
			{
				array = (int[])(blk.Data = new int[w * h]);
			}
			blk.scanw = srcBlk.w;
			blk.offset = 0;
			blk.progressive = srcBlk.progressive;
			float[] array3 = (float[])srcBlk.Data;
			int num2;
			int num3;
			int num4;
			if (fp != 0)
			{
				float num = 1 << fp;
				num2 = h - 1;
				num3 = w * h - 1;
				num4 = srcBlk.offset + (h - 1) * srcBlk.scanw + w - 1;
				while (num2 >= 0)
				{
					int num5 = num3 - w;
					while (num3 > num5)
					{
						if (array3[num4] > 0f)
						{
							array[num3] = (int)(array3[num4] * num + 0.5f);
						}
						else
						{
							array[num3] = (int)(array3[num4] * num - 0.5f);
						}
						num3--;
						num4--;
					}
					num4 -= srcBlk.scanw - w;
					num2--;
				}
				break;
			}
			num2 = h - 1;
			num3 = w * h - 1;
			num4 = srcBlk.offset + (h - 1) * srcBlk.scanw + w - 1;
			while (num2 >= 0)
			{
				int num5 = num3 - w;
				while (num3 > num5)
				{
					if (array3[num4] > 0f)
					{
						array[num3] = (int)(array3[num4] + 0.5f);
					}
					else
					{
						array[num3] = (int)(array3[num4] - 0.5f);
					}
					num3--;
					num4--;
				}
				num4 -= srcBlk.scanw - w;
				num2--;
			}
			break;
		}
		default:
			throw new ArgumentException("Only integer and float data are supported by JJ2000");
		}
		return blk;
	}
}
