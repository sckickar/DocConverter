using System;

namespace DocGen.Pdf;

internal class ImgDataJoiner : BlockImageDataSource, ImageData
{
	private int w;

	private int h;

	private int nc;

	private BlockImageDataSource[] imageData;

	private int[] compIdx;

	private int[] subsX;

	private int[] subsY;

	public virtual int TileWidth => w;

	public virtual int TileHeight => h;

	public virtual int NomTileWidth => w;

	public virtual int NomTileHeight => h;

	public virtual int ImgWidth => w;

	public virtual int ImgHeight => h;

	public virtual int NumComps => nc;

	public virtual int TileIdx => 0;

	public virtual int TilePartULX => 0;

	public virtual int TilePartULY => 0;

	public virtual int ImgULX => 0;

	public virtual int ImgULY => 0;

	internal ImgDataJoiner(BlockImageDataSource[] imD, int[] cIdx)
	{
		imageData = imD;
		compIdx = cIdx;
		if (imageData.Length != compIdx.Length)
		{
			throw new ArgumentException("imD and cIdx must have the same length");
		}
		nc = imD.Length;
		subsX = new int[nc];
		subsY = new int[nc];
		for (int i = 0; i < nc; i++)
		{
			if (imD[i].getNumTiles() != 1 || imD[i].getCompUpperLeftCornerX(cIdx[i]) != 0 || imD[i].getCompUpperLeftCornerY(cIdx[i]) != 0)
			{
				throw new ArgumentException("All input components must, not use tiles and must have the origin at the canvas origin");
			}
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < nc; i++)
		{
			if (imD[i].getCompImgWidth(cIdx[i]) > num)
			{
				num = imD[i].getCompImgWidth(cIdx[i]);
			}
			if (imD[i].getCompImgHeight(cIdx[i]) > num2)
			{
				num2 = imD[i].getCompImgHeight(cIdx[i]);
			}
		}
		w = num;
		h = num2;
		for (int i = 0; i < nc; i++)
		{
			subsX[i] = (num + imD[i].getCompImgWidth(cIdx[i]) - 1) / imD[i].getCompImgWidth(cIdx[i]);
			subsY[i] = (num2 + imD[i].getCompImgHeight(cIdx[i]) - 1) / imD[i].getCompImgHeight(cIdx[i]);
			if ((num + subsX[i] - 1) / subsX[i] == imD[i].getCompImgWidth(cIdx[i]))
			{
				_ = (num2 + subsY[i] - 1) / subsY[i];
				imD[i].getCompImgHeight(cIdx[i]);
			}
		}
	}

	public virtual int getCompSubsX(int c)
	{
		return subsX[c];
	}

	public virtual int getCompSubsY(int c)
	{
		return subsY[c];
	}

	public virtual int getTileComponentWidth(int t, int c)
	{
		return imageData[c].getTileComponentWidth(t, compIdx[c]);
	}

	public virtual int getTileComponentHeight(int t, int c)
	{
		return imageData[c].getTileComponentHeight(t, compIdx[c]);
	}

	public virtual int getCompImgWidth(int c)
	{
		return imageData[c].getCompImgWidth(compIdx[c]);
	}

	public virtual int getCompImgHeight(int n)
	{
		return imageData[n].getCompImgHeight(compIdx[n]);
	}

	public virtual int getNomRangeBits(int c)
	{
		return imageData[c].getNomRangeBits(compIdx[c]);
	}

	public virtual int getFixedPoint(int c)
	{
		return imageData[c].getFixedPoint(compIdx[c]);
	}

	public virtual DataBlock getInternCompData(DataBlock blk, int c)
	{
		return imageData[c].getInternCompData(blk, compIdx[c]);
	}

	public virtual DataBlock getCompData(DataBlock blk, int c)
	{
		return imageData[c].getCompData(blk, compIdx[c]);
	}

	public virtual void setTile(int x, int y)
	{
		if (x != 0 || y != 0)
		{
			throw new ArgumentException();
		}
	}

	public virtual void nextTile()
	{
		throw new Exception();
	}

	public virtual JPXImageCoordinates getTile(JPXImageCoordinates co)
	{
		if (co != null)
		{
			co.x = 0;
			co.y = 0;
			return co;
		}
		return new JPXImageCoordinates(0, 0);
	}

	public virtual int getCompUpperLeftCornerX(int c)
	{
		return 0;
	}

	public virtual int getCompUpperLeftCornerY(int c)
	{
		return 0;
	}

	public virtual JPXImageCoordinates getNumTiles(JPXImageCoordinates co)
	{
		if (co != null)
		{
			co.x = 1;
			co.y = 1;
			return co;
		}
		return new JPXImageCoordinates(1, 1);
	}

	public virtual int getNumTiles()
	{
		return 1;
	}

	public override string ToString()
	{
		string text = "ImgDataJoiner: WxH = " + w + "x" + h;
		for (int i = 0; i < nc; i++)
		{
			text = text + "\n- Component " + i + " " + imageData[i];
		}
		return text;
	}
}
