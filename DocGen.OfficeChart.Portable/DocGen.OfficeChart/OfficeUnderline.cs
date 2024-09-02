using System;

namespace DocGen.OfficeChart;

public enum OfficeUnderline
{
	None = 0,
	Single = 1,
	Double = 2,
	[Obsolete("OfficeUnderline - SingleAccounting is not in use anymore. Please use Single instead.")]
	SingleAccounting = 33,
	[Obsolete("OfficeUnderline - DoubleAccounting is not in use anymore. Please use Double instead.")]
	DoubleAccounting = 34,
	Dash = 35,
	DotDotDashHeavy = 36,
	DotDashHeavy = 37,
	DashHeavy = 38,
	DashLong = 39,
	DashLongHeavy = 40,
	DotDash = 41,
	DotDotDash = 42,
	Dotted = 43,
	DottedHeavy = 44,
	Heavy = 45,
	Wavy = 46,
	WavyDouble = 47,
	WavyHeavy = 48,
	Words = 49
}
