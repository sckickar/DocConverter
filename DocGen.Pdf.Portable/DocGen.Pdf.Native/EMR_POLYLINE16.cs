namespace DocGen.Pdf.Native;

internal struct EMR_POLYLINE16
{
	public RECT rclBounds;

	public uint cpts;

	public POINTS[] apts;
}
