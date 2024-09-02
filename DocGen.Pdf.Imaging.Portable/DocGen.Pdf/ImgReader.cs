using System;

namespace DocGen.Pdf;

internal abstract class ImgReader : BlockImageDataSource, ImageData
{
	internal int w;

	internal int h;

	internal int nc;

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

	public abstract void close();

	public virtual int getCompSubsX(int c)
	{
		return 1;
	}

	public virtual int getCompSubsY(int c)
	{
		return 1;
	}

	public virtual int getTileComponentWidth(int t, int c)
	{
		return w;
	}

	public virtual int getTileComponentHeight(int t, int c)
	{
		return h;
	}

	public virtual int getCompImgWidth(int c)
	{
		return w;
	}

	public virtual int getCompImgHeight(int c)
	{
		return h;
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

	public abstract bool isOrigSigned(int c);

	public abstract int getFixedPoint(int param1);

	public abstract DataBlock getInternCompData(DataBlock param1, int param2);

	public abstract int getNomRangeBits(int param1);

	public abstract DataBlock getCompData(DataBlock param1, int param2);
}
