using System;

namespace DocGen.PdfViewer.Base;

internal struct Matrix
{
	private MatrixTypes type;

	public static Matrix Identity => new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

	public double Determinant
	{
		get
		{
			switch (type)
			{
			case MatrixTypes.Identity:
			case MatrixTypes.Translation:
				return 1.0;
			case MatrixTypes.Scaling:
			case (MatrixTypes)3:
				return M11 * M22;
			default:
				return M11 * M22 - M12 * M21;
			}
		}
	}

	public double M11 { get; set; }

	public double M12 { get; set; }

	public double M21 { get; set; }

	public double M22 { get; set; }

	public double OffsetX { get; set; }

	public double OffsetY { get; set; }

	public Matrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
	{
		this = default(Matrix);
		M11 = m11;
		M12 = m12;
		M21 = m21;
		M22 = m22;
		OffsetX = offsetX;
		OffsetY = offsetY;
		type = MatrixTypes.Unknown;
		CheckMatrixType();
	}

	public static Matrix operator *(Matrix matrix1, Matrix matrix2)
	{
		return new Matrix(matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21, matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22, matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21, matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22, matrix1.OffsetX * matrix2.M11 + matrix1.OffsetY * matrix2.M21 + matrix2.OffsetX, matrix1.OffsetX * matrix2.M12 + matrix1.OffsetY * matrix2.M22 + matrix2.OffsetY);
	}

	public static bool operator ==(Matrix a, Matrix b)
	{
		if (a.M11 == b.M11 && a.M21 == b.M21 && a.M12 == b.M12 && a.M22 == b.M22 && a.OffsetX == b.OffsetX)
		{
			return a.OffsetY == b.OffsetY;
		}
		return false;
	}

	public static bool operator !=(Matrix a, Matrix b)
	{
		return !(a == b);
	}

	public bool IsIdentity()
	{
		return this == Identity;
	}

	public Matrix Translate(double offsetX, double offsetY)
	{
		if (type == MatrixTypes.Identity)
		{
			SetMatrix(1.0, 0.0, 0.0, 1.0, offsetX, offsetY, MatrixTypes.Translation);
		}
		else if (type == MatrixTypes.Unknown)
		{
			OffsetX += offsetX;
			OffsetY += offsetY;
		}
		else
		{
			OffsetX += offsetX;
			OffsetY += offsetY;
			type |= MatrixTypes.Translation;
		}
		return this;
	}

	public Matrix Scale(double scaleX, double scaleY, double centerX, double centerY)
	{
		this = new Matrix(scaleX, 0.0, 0.0, scaleY, centerX, centerY) * this;
		return this;
	}

	public Matrix ScaleAppend(double scaleX, double scaleY, double centerX, double centerY)
	{
		this *= new Matrix(scaleX, 0.0, 0.0, scaleY, centerX, centerY);
		return this;
	}

	public Matrix Rotate(double angle, double centerX, double centerY)
	{
		Matrix matrix = default(Matrix);
		angle = Math.PI * angle / 180.0;
		double num = Math.Sin(angle);
		double num2 = Math.Cos(angle);
		double offsetX = centerX * (1.0 - num2) + centerY * num;
		double offsetY = centerY * (1.0 - num2) - centerX * num;
		matrix.SetMatrix(num2, num, 0.0 - num, num2, offsetX, offsetY, MatrixTypes.Unknown);
		this = matrix * this;
		return this;
	}

	public bool Equals(Matrix value)
	{
		if (M11 == value.M11 && M12 == value.M12 && M21 == value.M21 && M22 == value.M22 && OffsetX == value.OffsetX && OffsetY == value.OffsetY)
		{
			return type.Equals(value.type);
		}
		return false;
	}

	public double Transform(double d)
	{
		double val = Math.Sqrt(Math.Pow(M11 + M21, 2.0) + Math.Pow(M12 + M22, 2.0));
		double val2 = Math.Sqrt(Math.Pow(M11 - M21, 2.0) + Math.Pow(M12 - M22, 2.0));
		return d * Math.Max(val, val2);
	}

	public Point Transform(Point point)
	{
		double x = point.X;
		double y = point.Y;
		double x2 = x * M11 + y * M21 + OffsetX;
		double y2 = x * M12 + y * M22 + OffsetY;
		return new Point(x2, y2);
	}

	internal Matrix Clone()
	{
		Matrix result = default(Matrix);
		result.M11 = M11;
		result.M12 = M12;
		result.M21 = M21;
		result.M22 = M22;
		result.OffsetX = OffsetX;
		result.OffsetY = OffsetY;
		return result;
	}

	public Rect Transform(Rect rect)
	{
		Point point = new Point(rect.Top, rect.Left);
		return new Rect(point2: Transform(new Point(rect.Bottom, rect.Right)), point1: Transform(point));
	}

	public override int GetHashCode()
	{
		return ((((((17 * 23 + M11.GetHashCode()) * 23 + M12.GetHashCode()) * 23 + M21.GetHashCode()) * 23 + M22.GetHashCode()) * 23 + OffsetX.GetHashCode()) * 23 + OffsetY.GetHashCode()) * 23 + type.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is Matrix)
		{
			return Equals((Matrix)obj);
		}
		return false;
	}

	public override string ToString()
	{
		return $"{M11} {M12} 0 | {M21} {M22} 0 | {OffsetX} {OffsetY} 1";
	}

	private void CheckMatrixType()
	{
		type = MatrixTypes.Identity;
		if (M21 != 0.0 || M12 != 0.0)
		{
			type = MatrixTypes.Unknown;
			return;
		}
		if (M11 != 1.0 || M22 != 1.0)
		{
			type = MatrixTypes.Scaling;
		}
		if (OffsetX != 0.0 || OffsetY != 0.0)
		{
			type |= MatrixTypes.Translation;
		}
		if ((type & (MatrixTypes)3) == 0)
		{
			type = MatrixTypes.Identity;
		}
	}

	private void SetMatrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY, MatrixTypes type)
	{
		M11 = m11;
		M12 = m12;
		M21 = m21;
		M22 = m22;
		OffsetX = offsetX;
		OffsetY = offsetY;
		this.type = type;
	}

	public double TransformX(double x)
	{
		return x * M11 + OffsetX;
	}
}
