namespace DocGen.Pdf.Native;

internal struct EMR_POLYPOLYLINE
{
	public RECT rclBounds;

	public uint nPolys;

	public uint cpts;

	public uint[] aPolyCounts;

	public POINT[] apts;
}
