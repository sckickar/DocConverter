using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public sealed class PointsConverter
{
	public static float FromCm(float centimeter)
	{
		return (float)UnitsConvertor.Instance.ConvertUnits(centimeter, PrintUnits.Centimeter, PrintUnits.Point);
	}

	public static float FromInch(float inch)
	{
		return (float)UnitsConvertor.Instance.ConvertUnits(inch, PrintUnits.Inch, PrintUnits.Point);
	}

	public static float FromPixel(float px)
	{
		return (float)UnitsConvertor.Instance.ConvertUnits(px, PrintUnits.Pixel, PrintUnits.Point);
	}
}
