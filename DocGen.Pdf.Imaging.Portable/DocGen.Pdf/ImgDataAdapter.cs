namespace DocGen.Pdf;

internal abstract class ImgDataAdapter : ImageData
{
	internal int tIdx;

	internal ImageData imgdatasrc;

	public virtual int TileWidth => imgdatasrc.TileWidth;

	public virtual int TileHeight => imgdatasrc.TileHeight;

	public virtual int NomTileWidth => imgdatasrc.NomTileWidth;

	public virtual int NomTileHeight => imgdatasrc.NomTileHeight;

	public virtual int ImgWidth => imgdatasrc.ImgWidth;

	public virtual int ImgHeight => imgdatasrc.ImgHeight;

	public virtual int NumComps => imgdatasrc.NumComps;

	public virtual int TileIdx => imgdatasrc.TileIdx;

	public virtual int TilePartULX => imgdatasrc.TilePartULX;

	public virtual int TilePartULY => imgdatasrc.TilePartULY;

	public virtual int ImgULX => imgdatasrc.ImgULX;

	public virtual int ImgULY => imgdatasrc.ImgULY;

	internal ImgDataAdapter(ImageData src)
	{
		imgdatasrc = src;
	}

	public virtual int getCompSubsX(int c)
	{
		return imgdatasrc.getCompSubsX(c);
	}

	public virtual int getCompSubsY(int c)
	{
		return imgdatasrc.getCompSubsY(c);
	}

	public virtual int getTileComponentWidth(int t, int c)
	{
		return imgdatasrc.getTileComponentWidth(t, c);
	}

	public virtual int getTileComponentHeight(int t, int c)
	{
		return imgdatasrc.getTileComponentHeight(t, c);
	}

	public virtual int getCompImgWidth(int c)
	{
		return imgdatasrc.getCompImgWidth(c);
	}

	public virtual int getCompImgHeight(int c)
	{
		return imgdatasrc.getCompImgHeight(c);
	}

	public virtual int getNomRangeBits(int c)
	{
		return imgdatasrc.getNomRangeBits(c);
	}

	public virtual void setTile(int x, int y)
	{
		imgdatasrc.setTile(x, y);
		tIdx = TileIdx;
	}

	public virtual void nextTile()
	{
		imgdatasrc.nextTile();
		tIdx = TileIdx;
	}

	public virtual JPXImageCoordinates getTile(JPXImageCoordinates co)
	{
		return imgdatasrc.getTile(co);
	}

	public virtual int getCompUpperLeftCornerX(int c)
	{
		return imgdatasrc.getCompUpperLeftCornerX(c);
	}

	public virtual int getCompUpperLeftCornerY(int c)
	{
		return imgdatasrc.getCompUpperLeftCornerY(c);
	}

	public virtual JPXImageCoordinates getNumTiles(JPXImageCoordinates co)
	{
		return imgdatasrc.getNumTiles(co);
	}

	public virtual int getNumTiles()
	{
		return imgdatasrc.getNumTiles();
	}
}
