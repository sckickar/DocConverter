using System;

namespace DocGen.Drawing;

public enum UnderlineStyle
{
	None = 0,
	Single = 1,
	Words = 2,
	Double = 3,
	Dotted = 4,
	[Obsolete("This enumeration option has been deprecated. On using this enumeration, None style will be set instead of DotDot.")]
	DotDot = 5,
	Thick = 6,
	Dash = 7,
	DashLong = 39,
	DotDash = 9,
	DotDotDash = 10,
	Wavy = 11,
	DottedHeavy = 20,
	DashHeavy = 23,
	DashLongHeavy = 55,
	DotDashHeavy = 25,
	DotDotDashHeavy = 26,
	WavyHeavy = 27,
	WavyDouble = 43
}
