namespace DocGen.Pdf;

internal abstract class ImgWriter
{
	public const int DEF_STRIP_HEIGHT = 64;

	internal BlockImageDataSource src;

	internal int w;

	internal int h;

	public abstract void close();

	public abstract void flush();

	~ImgWriter()
	{
		flush();
	}

	public abstract void write();

	public virtual void writeAll()
	{
		JPXImageCoordinates numTiles = src.getNumTiles(null);
		for (int i = 0; i < numTiles.y; i++)
		{
			for (int j = 0; j < numTiles.x; j++)
			{
				src.setTile(j, i);
				write();
			}
		}
	}

	public abstract void write(int ulx, int uly, int w, int h);
}
