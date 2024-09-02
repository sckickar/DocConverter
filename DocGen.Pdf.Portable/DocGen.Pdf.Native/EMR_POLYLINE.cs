namespace DocGen.Pdf.Native;

internal struct EMR_POLYLINE
{
	public RECT rclBounds;

	public uint cpts;

	public POINT[] apts;
}
