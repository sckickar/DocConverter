using System;

namespace DocGen.Pdf;

internal abstract class InvWTAdapter : InvWT, WaveletTransform, ImageData
{
	internal DecodeHelper decSpec;

	internal MultiResImgData mressrc;

	internal int reslvl;

	internal int maxImgRes;

	public virtual int ImgResLevel
	{
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("Resolution level index cannot be negative.");
			}
			reslvl = value;
		}
	}

	public virtual int TileWidth
	{
		get
		{
			int tileIdx = TileIdx;
			int num = 10000;
			int numComps = mressrc.NumComps;
			for (int i = 0; i < numComps; i++)
			{
				int resLvl = mressrc.getSynSubbandTree(tileIdx, i).resLvl;
				if (resLvl < num)
				{
					num = resLvl;
				}
			}
			return mressrc.getTileWidth(num);
		}
	}

	public virtual int TileHeight
	{
		get
		{
			int tileIdx = TileIdx;
			int num = 10000;
			int numComps = mressrc.NumComps;
			for (int i = 0; i < numComps; i++)
			{
				int resLvl = mressrc.getSynSubbandTree(tileIdx, i).resLvl;
				if (resLvl < num)
				{
					num = resLvl;
				}
			}
			return mressrc.getTileHeight(num);
		}
	}

	public virtual int NomTileWidth => mressrc.NomTileWidth;

	public virtual int NomTileHeight => mressrc.NomTileHeight;

	public virtual int ImgWidth => mressrc.getImgWidth(reslvl);

	public virtual int ImgHeight => mressrc.getImgHeight(reslvl);

	public virtual int NumComps => mressrc.NumComps;

	public virtual int TileIdx => mressrc.TileIdx;

	public virtual int ImgULX => mressrc.getImgULX(reslvl);

	public virtual int ImgULY => mressrc.getImgULY(reslvl);

	public virtual int TilePartULX => mressrc.TilePartULX;

	public virtual int TilePartULY => mressrc.TilePartULY;

	internal InvWTAdapter(MultiResImgData src, DecodeHelper decSpec)
	{
		mressrc = src;
		this.decSpec = decSpec;
		maxImgRes = decSpec.dls.Min;
	}

	public virtual int getCompSubsX(int c)
	{
		return mressrc.getCompSubsX(c);
	}

	public virtual int getCompSubsY(int c)
	{
		return mressrc.getCompSubsY(c);
	}

	public virtual int getTileComponentWidth(int t, int c)
	{
		int resLvl = mressrc.getSynSubbandTree(t, c).resLvl;
		return mressrc.getTileCompWidth(t, c, resLvl);
	}

	public virtual int getTileComponentHeight(int t, int c)
	{
		int resLvl = mressrc.getSynSubbandTree(t, c).resLvl;
		return mressrc.getTileCompHeight(t, c, resLvl);
	}

	public virtual int getCompImgWidth(int c)
	{
		int minInComp = decSpec.dls.getMinInComp(c);
		return mressrc.getCompImgWidth(c, minInComp);
	}

	public virtual int getCompImgHeight(int c)
	{
		int minInComp = decSpec.dls.getMinInComp(c);
		return mressrc.getCompImgHeight(c, minInComp);
	}

	public virtual void setTile(int x, int y)
	{
		mressrc.setTile(x, y);
	}

	public virtual void nextTile()
	{
		mressrc.nextTile();
	}

	public virtual JPXImageCoordinates getTile(JPXImageCoordinates co)
	{
		return mressrc.getTile(co);
	}

	public virtual int getCompUpperLeftCornerX(int c)
	{
		int tileIdx = TileIdx;
		int resLvl = mressrc.getSynSubbandTree(tileIdx, c).resLvl;
		return mressrc.getResULX(c, resLvl);
	}

	public virtual int getCompUpperLeftCornerY(int c)
	{
		int tileIdx = TileIdx;
		int resLvl = mressrc.getSynSubbandTree(tileIdx, c).resLvl;
		return mressrc.getResULY(c, resLvl);
	}

	public virtual JPXImageCoordinates getNumTiles(JPXImageCoordinates co)
	{
		return mressrc.getNumTiles(co);
	}

	public virtual int getNumTiles()
	{
		return mressrc.getNumTiles();
	}

	internal virtual SubbandSyn getSynSubbandTree(int t, int c)
	{
		return mressrc.getSynSubbandTree(t, c);
	}

	public abstract bool isReversible(int param1, int param2);

	public abstract int getNomRangeBits(int param1);

	public abstract int getImplementationType(int param1);
}
