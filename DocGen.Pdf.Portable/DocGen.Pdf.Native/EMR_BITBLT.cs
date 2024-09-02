namespace DocGen.Pdf.Native;

internal struct EMR_BITBLT
{
	public RECT rclBounds;

	public int xDest;

	public int yDest;

	public int cxDest;

	public int cyDest;

	public RASTER_CODE dwRop;

	public int xSrc;

	public int ySrc;

	public XFORM xformSrc;

	public int crBkColorSrc;

	public int iUsageSrc;

	public int offBmiSrc;

	public int cbBmiSrc;

	public int offBitsSrc;

	public uint cbBitsSrc;
}
