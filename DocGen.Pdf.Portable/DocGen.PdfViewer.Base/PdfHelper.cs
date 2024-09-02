using System;

namespace DocGen.PdfViewer.Base;

internal static class PdfHelper
{
	public static Matrix CalculateTextMatrix(Matrix m, Glyph glyph)
	{
		double offsetX = (glyph.Width * glyph.FontSize + glyph.CharSpacing + glyph.WordSpacing) * (glyph.HorizontalScaling / 100.0);
		return new Matrix(1.0, 0.0, 0.0, 1.0, offsetX, 0.0) * m;
	}

	public static bool UnboxDouble(object obj, out double res)
	{
		res = 0.0;
		if (obj == null)
		{
			return false;
		}
		if (obj is byte)
		{
			res = (int)(byte)obj;
			return true;
		}
		if (obj is int)
		{
			res = (int)obj;
			return true;
		}
		if (obj is double)
		{
			res = (double)obj;
			return true;
		}
		return false;
	}

	public static bool GetBit(int n, byte bit)
	{
		return (n & (1 << (int)bit)) != 0;
	}

	public static double GetDistance(Point p1, Point p2)
	{
		return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
	}

	public static bool UnboxBool(object obj, out bool res)
	{
		res = false;
		if (obj == null)
		{
			return false;
		}
		if (obj is bool)
		{
			res = (bool)obj;
			return true;
		}
		return false;
	}

	public static bool UnboxInt(object obj, out int res)
	{
		res = 0;
		if (obj == null)
		{
			return false;
		}
		if (obj is byte)
		{
			res = (byte)obj;
			return true;
		}
		if (obj is int)
		{
			res = (int)obj;
			return true;
		}
		if (obj is double)
		{
			res = (int)(double)obj;
			return true;
		}
		return false;
	}

	public static bool EnumTryParse<TEnum>(string valueAsString, out TEnum value, bool ignoreCase) where TEnum : struct
	{
		try
		{
			value = (TEnum)Enum.Parse(typeof(TEnum), valueAsString, ignoreCase);
			return true;
		}
		catch
		{
			value = default(TEnum);
			return false;
		}
	}

	public static Rect GetBoundingRect(Rect rect, Matrix matrix)
	{
		if (matrix.IsIdentity())
		{
			return rect;
		}
		Point[] array = new Point[4]
		{
			new Point(rect.Left, rect.Top),
			new Point(rect.Right, rect.Top),
			new Point(rect.Right, rect.Bottom),
			new Point(rect.Left, rect.Bottom)
		};
		TransformPoints(matrix, array);
		double x = Math.Min(Math.Min(array[0].X, array[1].X), Math.Min(array[2].X, array[3].X));
		double x2 = Math.Max(Math.Max(array[0].X, array[1].X), Math.Max(array[2].X, array[3].X));
		double y = Math.Min(Math.Min(array[0].Y, array[1].Y), Math.Min(array[2].Y, array[3].Y));
		return new Rect(point2: new Point(x2, Math.Max(Math.Max(array[0].Y, array[1].Y), Math.Max(array[2].Y, array[3].Y))), point1: new Point(x, y));
	}

	private static void TransformPoints(Matrix matrix, Point[] points)
	{
		for (int i = 0; i < points.Length; i++)
		{
			Point point = matrix.Transform(points[i]);
			points[i].X = point.X;
			points[i].Y = point.Y;
		}
	}
}
