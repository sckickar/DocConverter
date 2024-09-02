namespace System.Drawing.Drawing2D;

internal enum PathPointType
{
	Start = 0,
	Line = 1,
	Bezier3 = 3,
	Bezier = 3,
	PathTypeMask = 7,
	DashMode = 16,
	PathMarker = 32,
	CloseSubpath = 128
}
