using System;

namespace DocGen.OfficeChart.Implementation;

internal class BitConverterGeneral
{
	public static long DoubleToInt64Bits(double value)
	{
		return BitConverter.DoubleToInt64Bits(value);
	}

	public static double Int64BitsToDouble(long value)
	{
		return BitConverter.Int64BitsToDouble(value);
	}
}
