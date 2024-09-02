using System;

namespace DocGen.OfficeChart;

public enum OfficeGradientStyle
{
	Horizontal = 0,
	Vertical = 1,
	DiagonalUp = 2,
	DiagonalDown = 3,
	FromCorner = 4,
	FromCenter = 5,
	[Obsolete("This enumeration option has been deprecated. You can use DiagonalUp instead of Diagonl_Up.")]
	Diagonl_Up = 2,
	[Obsolete("This enumeration option has been deprecated. You can use DiagonalDown instead of Diagonl_Down.")]
	Diagonl_Down = 3,
	[Obsolete("This enumeration option has been deprecated. You can use FromCorner instead of From_Corner.")]
	From_Corner = 4,
	[Obsolete("This enumeration option has been deprecated. You can use FromCenter instead of From_Center.")]
	From_Center = 5
}
