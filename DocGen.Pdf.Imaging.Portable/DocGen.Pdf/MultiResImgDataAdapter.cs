namespace DocGen.Pdf;

internal abstract class MultiResImgDataAdapter : MultiResImgData
{
	internal int tIdx;

	internal MultiResImgData mressrc;

	public virtual int NomTileWidth => mressrc.NomTileWidth;

	public virtual int NomTileHeight => mressrc.NomTileHeight;

	public virtual int NumComps => mressrc.NumComps;

	public virtual int TileIdx => mressrc.TileIdx;

	public virtual int TilePartULX => mressrc.TilePartULX;

	public virtual int TilePartULY => mressrc.TilePartULY;

	internal MultiResImgDataAdapter(MultiResImgData src)
	{
		mressrc = src;
	}

	public virtual int getTileWidth(int rl)
	{
		return mressrc.getTileWidth(rl);
	}

	public virtual int getTileHeight(int rl)
	{
		return mressrc.getTileHeight(rl);
	}

	public virtual int getImgWidth(int rl)
	{
		return mressrc.getImgWidth(rl);
	}

	public virtual int getImgHeight(int rl)
	{
		return mressrc.getImgHeight(rl);
	}

	public virtual int getCompSubsX(int c)
	{
		return mressrc.getCompSubsX(c);
	}

	public virtual int getCompSubsY(int c)
	{
		return mressrc.getCompSubsY(c);
	}

	public virtual int getTileCompWidth(int t, int c, int rl)
	{
		return mressrc.getTileCompWidth(t, c, rl);
	}

	public virtual int getTileCompHeight(int t, int c, int rl)
	{
		return mressrc.getTileCompHeight(t, c, rl);
	}

	public virtual int getCompImgWidth(int c, int rl)
	{
		return mressrc.getCompImgWidth(c, rl);
	}

	public virtual int getCompImgHeight(int c, int rl)
	{
		return mressrc.getCompImgHeight(c, rl);
	}

	public virtual void setTile(int x, int y)
	{
		mressrc.setTile(x, y);
		tIdx = TileIdx;
	}

	public virtual void nextTile()
	{
		mressrc.nextTile();
		tIdx = TileIdx;
	}

	public virtual JPXImageCoordinates getTile(JPXImageCoordinates co)
	{
		return mressrc.getTile(co);
	}

	public virtual int getResULX(int c, int rl)
	{
		return mressrc.getResULX(c, rl);
	}

	public virtual int getResULY(int c, int rl)
	{
		return mressrc.getResULY(c, rl);
	}

	public virtual int getImgULX(int rl)
	{
		return mressrc.getImgULX(rl);
	}

	public virtual int getImgULY(int rl)
	{
		return mressrc.getImgULY(rl);
	}

	public virtual JPXImageCoordinates getNumTiles(JPXImageCoordinates co)
	{
		return mressrc.getNumTiles(co);
	}

	public virtual int getNumTiles()
	{
		return mressrc.getNumTiles();
	}

	public abstract SubbandSyn getSynSubbandTree(int param1, int param2);
}
