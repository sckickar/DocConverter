namespace DocGen.Pdf;

internal abstract class CoordInfo
{
	public int ulx;

	public int uly;

	public int w;

	public int h;

	public CoordInfo(int ulx, int uly, int w, int h)
	{
		this.ulx = ulx;
		this.uly = uly;
		this.w = w;
		this.h = h;
	}

	public CoordInfo()
	{
	}

	public override string ToString()
	{
		return "ulx=" + ulx + ",uly=" + uly + ",w=" + w + ",h=" + h;
	}
}
