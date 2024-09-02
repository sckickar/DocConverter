namespace DocGen.Pdf.Native;

internal struct EMR_FILLRGN
{
	public RECT rclBounds;

	public int cbRgnData;

	public int ihBrush;

	public RGNDATA RgnData;
}
