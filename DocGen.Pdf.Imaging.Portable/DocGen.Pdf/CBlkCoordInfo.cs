namespace DocGen.Pdf;

internal class CBlkCoordInfo : CoordInfo
{
	public JPXImageCoordinates idx;

	public CBlkCoordInfo()
	{
		idx = new JPXImageCoordinates();
	}

	public CBlkCoordInfo(int m, int n)
	{
		idx = new JPXImageCoordinates(n, m);
	}

	public override string ToString()
	{
		return base.ToString() + ",idx=" + idx;
	}
}
