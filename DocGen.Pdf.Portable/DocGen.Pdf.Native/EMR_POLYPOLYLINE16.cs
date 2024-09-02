namespace DocGen.Pdf.Native;

internal struct EMR_POLYPOLYLINE16
{
	public RECT rclBounds;

	public uint nPolys;

	public uint cpts;

	public uint[] aPolyCounts;

	public POINTS[] apts;
}
