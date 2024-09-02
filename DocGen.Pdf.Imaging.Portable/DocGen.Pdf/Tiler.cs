using System;

namespace DocGen.Pdf;

internal class Tiler : ImgDataAdapter, BlockImageDataSource, ImageData
{
	private BlockImageDataSource src;

	private int x0siz;

	private int y0siz;

	private int xt0siz;

	private int yt0siz;

	private int xtsiz;

	private int ytsiz;

	private int ntX;

	private int ntY;

	private int[] compW;

	private int[] compH;

	private int[] tcx0;

	private int[] tcy0;

	private int tx;

	private int ty;

	private int tileW;

	private int tileH;

	public override int TileWidth => tileW;

	public override int TileHeight => tileH;

	public override int TileIdx => ty * ntX + tx;

	public override int TilePartULX => xt0siz;

	public override int TilePartULY => yt0siz;

	public override int ImgULX => x0siz;

	public override int ImgULY => y0siz;

	public override int NomTileWidth => xtsiz;

	public override int NomTileHeight => ytsiz;

	internal Tiler(BlockImageDataSource src, int ax, int ay, int px, int py, int nw, int nh)
		: base(src)
	{
		this.src = src;
		x0siz = ax;
		y0siz = ay;
		xt0siz = px;
		yt0siz = py;
		xtsiz = nw;
		ytsiz = nh;
		if (src.getNumTiles() != 1)
		{
			throw new ArgumentException("Source is tiled");
		}
		if (src.ImgULX != 0 || src.ImgULY != 0)
		{
			throw new ArgumentException("Source is \"canvased\"");
		}
		if (x0siz < 0 || y0siz < 0 || xt0siz < 0 || yt0siz < 0 || xtsiz < 0 || ytsiz < 0 || xt0siz > x0siz || yt0siz > y0siz)
		{
			throw new ArgumentException("Invalid image origin, tiling origin or nominal tile size");
		}
		if (xtsiz == 0)
		{
			xtsiz = x0siz + src.ImgWidth - xt0siz;
		}
		if (ytsiz == 0)
		{
			ytsiz = y0siz + src.ImgHeight - yt0siz;
		}
		if (x0siz - xt0siz >= xtsiz)
		{
			xt0siz += (x0siz - xt0siz) / xtsiz * xtsiz;
		}
		if (y0siz - yt0siz >= ytsiz)
		{
			yt0siz += (y0siz - yt0siz) / ytsiz * ytsiz;
		}
		if (x0siz - xt0siz < xtsiz)
		{
			_ = y0siz - yt0siz;
			_ = ytsiz;
		}
		ntX = (int)Math.Ceiling((double)(x0siz + src.ImgWidth) / (double)xtsiz);
		ntY = (int)Math.Ceiling((double)(y0siz + src.ImgHeight) / (double)ytsiz);
	}

	public override int getTileComponentWidth(int t, int c)
	{
		_ = TileIdx;
		return compW[c];
	}

	public override int getTileComponentHeight(int t, int c)
	{
		_ = TileIdx;
		return compH[c];
	}

	public virtual int getFixedPoint(int c)
	{
		return src.getFixedPoint(c);
	}

	public DataBlock getInternCompData(DataBlock blk, int c)
	{
		if (blk.ulx < 0 || blk.uly < 0 || blk.w > compW[c] || blk.h > compH[c])
		{
			throw new ArgumentException("Block is outside the tile");
		}
		int num = (int)Math.Ceiling((double)x0siz / (double)src.getCompSubsX(c));
		int num2 = (int)Math.Ceiling((double)y0siz / (double)src.getCompSubsY(c));
		blk.ulx -= num;
		blk.uly -= num2;
		blk = src.getInternCompData(blk, c);
		blk.ulx += num;
		blk.uly += num2;
		return blk;
	}

	public DataBlock getCompData(DataBlock blk, int c)
	{
		if (blk.ulx < 0 || blk.uly < 0 || blk.w > compW[c] || blk.h > compH[c])
		{
			throw new ArgumentException("Block is outside the tile");
		}
		int num = (int)Math.Ceiling((double)x0siz / (double)src.getCompSubsX(c));
		int num2 = (int)Math.Ceiling((double)y0siz / (double)src.getCompSubsY(c));
		blk.ulx -= num;
		blk.uly -= num2;
		blk = src.getCompData(blk, c);
		blk.ulx += num;
		blk.uly += num2;
		return blk;
	}

	public override void setTile(int x, int y)
	{
		if (x < 0 || y < 0 || x >= ntX || y >= ntY)
		{
			throw new ArgumentException("Tile's indexes out of bounds");
		}
		tx = x;
		ty = y;
		int num = ((x != 0) ? (xt0siz + x * xtsiz) : x0siz);
		int num2 = ((y != 0) ? (yt0siz + y * ytsiz) : y0siz);
		int num3 = ((x != ntX - 1) ? (xt0siz + (x + 1) * xtsiz) : (x0siz + src.ImgWidth));
		int num4 = ((y != ntY - 1) ? (yt0siz + (y + 1) * ytsiz) : (y0siz + src.ImgHeight));
		tileW = num3 - num;
		tileH = num4 - num2;
		int numComps = src.NumComps;
		if (compW == null)
		{
			compW = new int[numComps];
		}
		if (compH == null)
		{
			compH = new int[numComps];
		}
		if (tcx0 == null)
		{
			tcx0 = new int[numComps];
		}
		if (tcy0 == null)
		{
			tcy0 = new int[numComps];
		}
		for (int i = 0; i < numComps; i++)
		{
			tcx0[i] = (int)Math.Ceiling((double)num / (double)src.getCompSubsX(i));
			tcy0[i] = (int)Math.Ceiling((double)num2 / (double)src.getCompSubsY(i));
			compW[i] = (int)Math.Ceiling((double)num3 / (double)src.getCompSubsX(i)) - tcx0[i];
			compH[i] = (int)Math.Ceiling((double)num4 / (double)src.getCompSubsY(i)) - tcy0[i];
		}
	}

	public override void nextTile()
	{
		if (tx == ntX - 1 && ty == ntY - 1)
		{
			throw new Exception();
		}
		if (tx < ntX - 1)
		{
			setTile(tx + 1, ty);
		}
		else
		{
			setTile(0, ty + 1);
		}
	}

	public override JPXImageCoordinates getTile(JPXImageCoordinates co)
	{
		if (co != null)
		{
			co.x = tx;
			co.y = ty;
			return co;
		}
		return new JPXImageCoordinates(tx, ty);
	}

	public override int getCompUpperLeftCornerX(int c)
	{
		return tcx0[c];
	}

	public override int getCompUpperLeftCornerY(int c)
	{
		return tcy0[c];
	}

	public override JPXImageCoordinates getNumTiles(JPXImageCoordinates co)
	{
		if (co != null)
		{
			co.x = ntX;
			co.y = ntY;
			return co;
		}
		return new JPXImageCoordinates(ntX, ntY);
	}

	public override int getNumTiles()
	{
		return ntX * ntY;
	}

	public JPXImageCoordinates getTilingOrigin(JPXImageCoordinates co)
	{
		if (co != null)
		{
			co.x = xt0siz;
			co.y = yt0siz;
			return co;
		}
		return new JPXImageCoordinates(xt0siz, yt0siz);
	}

	public override string ToString()
	{
		return "Tiler: source= " + src?.ToString() + "\n" + getNumTiles() + " tile(s), nominal width=" + xtsiz + ", nominal height=" + ytsiz;
	}
}
