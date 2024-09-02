namespace DocGen.Pdf.Native;

internal struct EMR_STRETCHDIBITS
{
	public RECT rclBounds;

	public int xDest;

	public int yDest;

	public int xSrc;

	public int ySrc;

	public int cxSrc;

	public int cySrc;

	public int offBmiSrc;

	public int cbBmiSrc;

	public int offBitsSrc;

	public uint cbBitsSrc;

	public int iUsageSrc;

	public uint dwRop;

	public int cxDest;

	public int cyDest;
}
