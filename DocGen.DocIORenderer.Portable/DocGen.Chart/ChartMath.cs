using System;
using System.ComponentModel;
using DocGen.Drawing;

namespace DocGen.Chart;

internal static class ChartMath
{
	internal delegate double DoubleFunc(double x);

	public const double ToRadians = Math.PI / 180.0;

	public const double ToDegrees = 180.0 / Math.PI;

	public const double DblPI = Math.PI * 2.0;

	public const double HlfPI = Math.PI / 2.0;

	public const double Epsilon = 1E-05;

	internal static double ModAngle(double angle, double modAngle)
	{
		if (!(angle > 0.0))
		{
			return modAngle - angle % modAngle;
		}
		return angle % modAngle;
	}

	public static double Round(double value, double div)
	{
		double num = ((!(value > 0.0)) ? (div * Math.Ceiling(value / div)) : (div * Math.Floor(value / div)));
		if (Math.Abs(value - num) >= 0.5 * div)
		{
			num = ((!(value > 0.0)) ? (num - div) : (num + div));
		}
		return num;
	}

	public static double Round(double value, double div, bool up)
	{
		return div * (up ? Math.Ceiling(value / div) : Math.Floor(value / div));
	}

	internal static double RoundDateTimeRange(double value, double div, bool up)
	{
		if (!up)
		{
			return Math.Floor(div * value / div);
		}
		return Math.Ceiling(div * value / div);
	}

	public static void GetTwoClosestPoints(double[] xs, double point, out int index1, out int index2)
	{
		index1 = 0;
		index2 = 0;
		int num = xs.Length;
		if (num <= 1)
		{
			return;
		}
		int num2 = num - 1;
		int num3 = 0;
		int num4 = xs.Length / 2;
		index2 = 1;
		while (true)
		{
			if (num4 <= 0)
			{
				num4 = 1;
			}
			if (num4 >= num - 1)
			{
				num4 = num - 2;
			}
			if (num4 <= 0)
			{
				break;
			}
			double num5 = point - xs[num4];
			double num6 = xs[num4 - 1] - xs[num4];
			double num7 = xs[num4 + 1] - xs[num4];
			if (num5 < 0.0)
			{
				if (!(num5 < num6))
				{
					index1 = num4 - 1;
					index2 = num4;
					break;
				}
				num2 = num4;
			}
			else
			{
				if (!(num5 > num7))
				{
					index1 = num4;
					index2 = num4 + 1;
					break;
				}
				num3 = num4;
			}
			if (num2 - num3 == 1)
			{
				if (point > xs[num - 1])
				{
					index1 = num - 2;
					index2 = num - 1;
				}
				else if (point < xs[0])
				{
					index1 = 0;
					index2 = 1;
				}
				break;
			}
			num4 = (num2 + num3) / 2;
		}
	}

	public static double Bisection(DoubleFunc fnc, double x1, double x2, double xAccuracy, int maxIterationCount)
	{
		double num = fnc(x1);
		double num2 = fnc(x2);
		if (num * num2 >= 0.0)
		{
			return double.NaN;
		}
		double num3;
		double num4;
		if (num >= 0.0)
		{
			num3 = x2;
			num4 = x1 - x2;
		}
		else
		{
			num3 = x1;
			num4 = x2 - x1;
		}
		for (int i = 0; i < maxIterationCount; i++)
		{
			num4 /= 2.0;
			double num5 = num3 + num4;
			num2 = fnc(num5);
			if (num2 <= 0.0)
			{
				num3 = num5;
			}
			if (Math.Abs(num4) < xAccuracy || num2 == 0.0)
			{
				return num3;
			}
		}
		return double.NaN;
	}

	public static double SmartBisection(DoubleFunc fnc, double x1, double x2, double xAccuracy, int maxIterationCount, int tabulCount)
	{
		double num = Math.Abs((x2 - x1) / (double)tabulCount);
		double num2 = Math.Min(x2, x1);
		for (int i = 0; i < tabulCount - 1; i++)
		{
			double num3 = fnc(num2);
			double num4 = fnc(num2 + num);
			if (num3 * num4 <= 0.0)
			{
				return Bisection(fnc, num2, num2 + num, xAccuracy, maxIterationCount);
			}
			num2 += num;
		}
		return double.NaN;
	}

	public static PointF LineSegmentIntersectionPoint(PointF a, PointF b, PointF c, PointF d)
	{
		PointF result = PointF.Empty;
		PointF pointF = new PointF(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
		PointF pointF2 = new PointF(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
		RectangleF rectangleF = RectangleF.FromLTRB(pointF2.X, pointF2.Y, pointF.X, pointF.Y);
		PointF pointF3 = new PointF(Math.Max(c.X, d.X), Math.Max(c.Y, d.Y));
		PointF pointF4 = new PointF(Math.Min(c.X, d.X), Math.Min(c.Y, d.Y));
		RectangleF rect = RectangleF.FromLTRB(pointF4.X, pointF4.Y, pointF3.X, pointF3.Y);
		if (!rectangleF.IntersectsWith(rect))
		{
			return result;
		}
		PointF p = new PointF(b.X - a.X, b.Y - a.Y);
		PointF p2 = new PointF(d.X - a.X, d.Y - a.Y);
		PointF p3 = new PointF(c.X - a.X, c.Y - a.Y);
		double num = Determinant(p2, p);
		double num2 = Determinant(p3, p);
		PointF p4 = new PointF(a.X - c.X, a.Y - c.Y);
		PointF p5 = new PointF(b.X - c.X, b.Y - c.Y);
		PointF p6 = new PointF(d.X - c.X, d.Y - c.Y);
		double num3 = Determinant(p4, p6);
		double num4 = Determinant(p5, p6);
		if (num * num2 < 0.0 || num3 * num4 < 0.0)
		{
			double num5 = Math.Abs(num2) / (Math.Abs(num2) + Math.Abs(num));
			result = new PointF((float)((double)c.X + (double)p6.X * num5), (float)((double)c.Y + (double)p6.Y * num5));
		}
		return result;
	}

	public static float Determinant(PointF p1, PointF p2)
	{
		return p1.X * p2.Y - p1.Y * p2.X;
	}

	[Obsolete("Use RectangleIntersectsWithLine")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool RectanlgeIntersectsWithLine(RectangleF r, PointF a, PointF b)
	{
		bool result = false;
		if (r.IsEmpty || a.IsEmpty || b.IsEmpty)
		{
			return result;
		}
		PointF a2 = new PointF(r.Left, r.Top);
		PointF b2 = new PointF(r.Right, r.Bottom);
		PointF a3 = new PointF(r.Right, r.Top);
		PointF b3 = new PointF(r.Left, r.Bottom);
		if (!LineSegmentIntersectionPoint(a2, b2, a, b).IsEmpty)
		{
			result = true;
		}
		if (!LineSegmentIntersectionPoint(a3, b3, a, b).IsEmpty)
		{
			result = true;
		}
		return result;
	}

	public static bool RectangleIntersectsWithLine(RectangleF r, PointF a, PointF b)
	{
		bool result = false;
		if (r.IsEmpty || a.IsEmpty || b.IsEmpty)
		{
			return result;
		}
		PointF a2 = new PointF(r.Left, r.Top);
		PointF b2 = new PointF(r.Right, r.Bottom);
		PointF a3 = new PointF(r.Right, r.Top);
		PointF b3 = new PointF(r.Left, r.Bottom);
		if (!LineSegmentIntersectionPoint(a2, b2, a, b).IsEmpty)
		{
			result = true;
		}
		if (!LineSegmentIntersectionPoint(a3, b3, a, b).IsEmpty)
		{
			result = true;
		}
		return result;
	}

	public static double MinMax(double value, double min, double max)
	{
		if (!(value > min))
		{
			return min;
		}
		if (!(value < max))
		{
			return max;
		}
		return value;
	}

	public static float MinMax(float value, float min, float max)
	{
		if (!(value > min))
		{
			return min;
		}
		if (!(value < max))
		{
			return max;
		}
		return value;
	}

	public static int MinMax(int value, int min, int max)
	{
		if (value <= min)
		{
			return min;
		}
		if (value >= max)
		{
			return max;
		}
		return value;
	}

	public static void MinMax(double value1, double value2, out double min, out double max)
	{
		if (value1 > value2)
		{
			max = value1;
			min = value2;
		}
		else
		{
			min = value1;
			max = value2;
		}
	}

	public static double Min(double[] values)
	{
		double num = double.MaxValue;
		for (int i = 0; i < values.Length; i++)
		{
			if (num > values[i])
			{
				num = values[i];
			}
		}
		return num;
	}

	public static double Max(double[] values)
	{
		double num = double.MinValue;
		for (int i = 0; i < values.Length; i++)
		{
			if (num < values[i])
			{
				num = values[i];
			}
		}
		return num;
	}

	public static RectangleF RotatedRectangleBounds(RectangleF r, double angle)
	{
		double num = Math.Sin(angle);
		double num2 = Math.Cos(angle);
		SizeF size = r.Size;
		SizeF sizeF = new SizeF((float)Math.Abs((double)size.Height * num) + (float)Math.Abs((double)size.Width * num2), (float)Math.Abs((double)size.Width * num) + (float)Math.Abs((double)size.Height * num2));
		float left = r.Left;
		float val = (float)((double)left - (double)size.Height * num);
		float val2 = (float)((double)left - (double)size.Height * num + (double)size.Width * num2);
		float x = Math.Min(val2: (float)((double)left + (double)size.Width * num2), val1: Math.Min(Math.Min(left, val), val2));
		float top = r.Top;
		float val4 = (float)((double)top + (double)size.Width * num);
		float val5 = (float)((double)top + (double)size.Width * num + (double)size.Height * num2);
		float val6 = (float)((double)top + (double)size.Height * num2);
		top = Math.Min(top, val4);
		top = Math.Min(top, val5);
		top = Math.Min(top, val6);
		return new RectangleF(x, top, sizeF.Width, sizeF.Height);
	}

	public static RectangleF LeftCenterRotatedRectangleBounds(RectangleF r, double angle)
	{
		double num = Math.Sin(angle);
		double num2 = Math.Cos(angle);
		PointF pointF = new PointF(r.X, r.Y + r.Height / 2f);
		SizeF size = r.Size;
		SizeF sizeF = new SizeF((float)Math.Abs((double)size.Height * num) + (float)Math.Abs((double)size.Width * num2), (float)Math.Abs((double)size.Width * num) + (float)Math.Abs((double)size.Height * num2));
		float x = pointF.X;
		float val = (float)((double)x + (double)size.Height * num / 2.0);
		float val2 = (float)((double)x + (double)size.Width * num2 + (double)size.Height * num / 2.0);
		float x2 = Math.Min(val2: (float)((double)x + (double)size.Width * num2 - (double)size.Height * num / 2.0), val1: Math.Min(Math.Min((float)((double)x - (double)size.Height * num / 2.0), val), val2));
		float y = pointF.Y;
		float val4 = (float)((double)y - (double)size.Height * num2 / 2.0);
		float val5 = (float)((double)y + (double)size.Width * num - (double)size.Height * num2 / 2.0);
		float val6 = (float)((double)y + (double)size.Width * num + (double)size.Height * num2 / 2.0);
		y = Math.Min((float)((double)y + (double)size.Height * num2 / 2.0), val4);
		y = Math.Min(y, val5);
		y = Math.Min(y, val6);
		return new RectangleF(x2, y, sizeF.Width, sizeF.Height);
	}

	public static RectangleF CenterRotatedRectangleBounds(RectangleF r, double angle)
	{
		double num = Math.Sin(angle);
		double num2 = Math.Cos(angle);
		PointF pointF = new PointF(r.X + r.Width / 2f, r.Y + r.Height / 2f);
		SizeF size = r.Size;
		SizeF sizeF = new SizeF((float)Math.Abs((double)size.Height * num) + (float)Math.Abs((double)size.Width * num2), (float)Math.Abs((double)size.Width * num) + (float)Math.Abs((double)size.Height * num2));
		float x = pointF.X;
		float val = (float)((double)x - (double)size.Width * num2 / 2.0 + (double)size.Height * num / 2.0);
		float val2 = (float)((double)x + (double)size.Width * num2 / 2.0 + (double)size.Height * num / 2.0);
		float x2 = Math.Min(val2: (float)((double)x + (double)size.Width * num2 / 2.0 - (double)size.Height * num / 2.0), val1: Math.Min(Math.Min((float)((double)x - (double)size.Width * num2 / 2.0 - (double)size.Height * num / 2.0), val), val2));
		float y = pointF.Y;
		float val4 = (float)((double)y + (double)size.Width * num / 2.0 + (double)size.Height * num2 / 2.0);
		float val5 = (float)((double)y - (double)size.Width * num / 2.0 + (double)size.Height * num2 / 2.0);
		float val6 = (float)((double)y - (double)size.Width * num / 2.0 - (double)size.Height * num2 / 2.0);
		y = Math.Min((float)((double)y + (double)size.Width * num / 2.0 - (double)size.Height * num2 / 2.0), val4);
		y = Math.Min(y, val5);
		y = Math.Min(y, val6);
		return new RectangleF(x2, y, sizeF.Width, sizeF.Height);
	}

	public static float DistanceBetweenPoints(PointF p1, PointF p2)
	{
		return (float)Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
	}

	public static PointF GetPointByAngle(RectangleF rect, double angle, bool isCircle)
	{
		PointF result = rect.Location;
		double num = Math.Cos(angle);
		double num2 = Math.Sin(angle);
		double num3 = 0.5f * rect.Width;
		double num4 = 0.5f * rect.Height;
		if (num3 != 0.0 && num4 != 0.0)
		{
			if (isCircle || num3 == num4)
			{
				result = new PointF((float)((double)rect.X + num3 + num3 * num), (float)((double)rect.Y + num4 + num4 * num2));
			}
			else
			{
				double num5 = 1.0 / Math.Sqrt(num * num / (num3 * num3) + num2 * num2 / (num4 * num4));
				result = new PointF((float)((double)rect.X + num3 + num5 * num), (float)((double)rect.Y + num4 + num5 * num2));
			}
		}
		return result;
	}

	public static RectangleF GetRectByCenter(PointF center, SizeF radius)
	{
		return new RectangleF(center.X - radius.Width, center.Y - radius.Height, 2f * radius.Width, 2f * radius.Height);
	}

	public static PointF GetCenter(RectangleF rect)
	{
		return new PointF(rect.Left + rect.Width / 2f, rect.Top + rect.Height / 2f);
	}

	public static SizeF GetRadius(RectangleF rect)
	{
		return new SizeF(rect.Width / 2f, rect.Height / 2f);
	}

	public static RectangleF CorrectRect(RectangleF rect)
	{
		return new RectangleF(Math.Min(rect.Left, rect.Right), Math.Min(rect.Top, rect.Bottom), Math.Abs(rect.Width), Math.Abs(rect.Height));
	}

	public static RectangleF CorrectRect(float x1, float y1, float x2, float y2)
	{
		return new RectangleF(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1));
	}

	public static PointF AddPoint(PointF pt, SizeF sz)
	{
		return new PointF(pt.X + sz.Width, pt.Y + sz.Height);
	}

	public static PointF AddPoint(PointF pt, Size sz)
	{
		return new PointF(pt.X + (float)sz.Width, pt.Y + (float)sz.Height);
	}

	public static Vector3D GetNormal(Vector3D v1, Vector3D v2, Vector3D v3)
	{
		Vector3D vector3D = (v1 - v2) * (v3 - v2);
		double num = vector3D.GetLength();
		if (num < 1E-05)
		{
			num = 0.0;
		}
		return new Vector3D(vector3D.X / num, vector3D.Y / num, vector3D.Z / num);
	}

	internal static bool SolveQuadraticEquation(double a, double b, double c, out double root1, out double root2)
	{
		root1 = double.NaN;
		root2 = double.NaN;
		if (a != 0.0)
		{
			double num = b * b - 4.0 * a * c;
			if (num >= 0.0)
			{
				double num2 = Math.Sqrt(num);
				root1 = (0.0 - b - num2) / (2.0 * a);
				root2 = (0.0 - b + num2) / (2.0 * a);
				return true;
			}
		}
		else if (b != 0.0)
		{
			root1 = (0.0 - c) / b;
			root2 = (0.0 - c) / b;
			return true;
		}
		return false;
	}

	internal static bool SolveQuadraticEquation(float a, float b, float c, out float root1, out float root2)
	{
		root1 = 0f;
		root2 = 0f;
		if (a != 0f)
		{
			float num = b * b - 4f * a * c;
			if (num >= 0f)
			{
				root1 = (0f - b - (float)Math.Sqrt(num)) / (2f * a);
				root2 = (0f - b + (float)Math.Sqrt(num)) / (2f * a);
				return true;
			}
			return false;
		}
		if (b != 0f)
		{
			root1 = (0f - c) / b;
			root2 = (0f - c) / b;
			return true;
		}
		return false;
	}

	internal static PointF[] InterpolateBezier(PointF p1, PointF p2, PointF p3, PointF p4, int count)
	{
		PointF[] array = new PointF[count];
		float num = 3f * (p2.X - p1.X);
		float num2 = 3f * (p2.Y - p1.Y);
		float num3 = 3f * (p3.X - p2.X) - num;
		float num4 = 3f * (p3.Y - p2.Y) - num2;
		float num5 = p4.X - p1.X - num3 - num;
		float num6 = p4.Y - p1.Y - num4 - num2;
		for (int i = 0; i < count; i++)
		{
			float num7 = (float)i / (float)(count - 1);
			float x = num5 * num7 * num7 * num7 + num3 * num7 * num7 + num * num7 + p1.X;
			float y = num6 * num7 * num7 * num7 + num4 * num7 * num7 + num2 * num7 + p1.Y;
			array[i] = new PointF(x, y);
		}
		return array;
	}

	internal static void SplitBezierCurve(PointF p0, PointF p1, PointF p2, PointF p3, float t0, out PointF pb0, out PointF pb1, out PointF pb2, out PointF pb3, out PointF pe0, out PointF pe1, out PointF pe2, out PointF pe3)
	{
		int num = 4;
		float[,] array = new float[num, num];
		float[,] array2 = new float[num, num];
		array[0, 0] = p0.X;
		array[1, 0] = p1.X;
		array[2, 0] = p2.X;
		array[3, 0] = p3.X;
		array2[0, 0] = p0.Y;
		array2[1, 0] = p1.Y;
		array2[2, 0] = p2.Y;
		array2[3, 0] = p3.Y;
		for (int i = 1; i < num; i++)
		{
			for (int j = 0; j < num - i; j++)
			{
				array[j, i] = array[j, i - 1] * (1f - t0) + array[j + 1, i - 1] * t0;
				array2[j, i] = array2[j, i - 1] * (1f - t0) + array2[j + 1, i - 1] * t0;
			}
		}
		pb0 = new PointF(array[0, 0], array2[0, 0]);
		pb1 = new PointF(array[0, 1], array2[0, 1]);
		pb2 = new PointF(array[0, 2], array2[0, 2]);
		pb3 = new PointF(array[0, 3], array2[0, 3]);
		pe0 = new PointF(array[0, 3], array2[0, 3]);
		pe1 = new PointF(array[1, 2], array2[1, 2]);
		pe2 = new PointF(array[2, 1], array2[2, 1]);
		pe3 = new PointF(array[3, 0], array2[3, 0]);
	}

	internal static void SplitBezierCurve(ChartPoint p0, ChartPoint p1, ChartPoint p2, ChartPoint p3, double t0, out ChartPoint pb0, out ChartPoint pb1, out ChartPoint pb2, out ChartPoint pb3, out ChartPoint pe0, out ChartPoint pe1, out ChartPoint pe2, out ChartPoint pe3)
	{
		int num = 4;
		double[,] array = new double[num, num];
		double[,] array2 = new double[num, num];
		array[0, 0] = p0.X;
		array[1, 0] = p1.X;
		array[2, 0] = p2.X;
		array[3, 0] = p3.X;
		array2[0, 0] = p0.YValues[0];
		array2[1, 0] = p1.YValues[0];
		array2[2, 0] = p2.YValues[0];
		array2[3, 0] = p3.YValues[0];
		for (int i = 1; i < num; i++)
		{
			for (int j = 0; j < num - i; j++)
			{
				array[j, i] = array[j, i - 1] * (1.0 - t0) + array[j + 1, i - 1] * t0;
				array2[j, i] = array2[j, i - 1] * (1.0 - t0) + array2[j + 1, i - 1] * t0;
			}
		}
		pb0 = new ChartPoint(array[0, 0], array2[0, 0]);
		pb1 = new ChartPoint(array[0, 1], array2[0, 1]);
		pb2 = new ChartPoint(array[0, 2], array2[0, 2]);
		pb3 = new ChartPoint(array[0, 3], array2[0, 3]);
		pe0 = new ChartPoint(array[0, 3], array2[0, 3]);
		pe1 = new ChartPoint(array[1, 2], array2[1, 2]);
		pe2 = new ChartPoint(array[2, 1], array2[2, 1]);
		pe3 = new ChartPoint(array[3, 0], array2[3, 0]);
	}
}
