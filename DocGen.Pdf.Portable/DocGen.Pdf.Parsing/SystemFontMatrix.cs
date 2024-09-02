using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Parsing;

internal struct SystemFontMatrix
{
	private SystemFontMatrixTypes type;

	public static SystemFontMatrix Identity => new SystemFontMatrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

	public double Determinant
	{
		get
		{
			switch (type)
			{
			case SystemFontMatrixTypes.Identity:
			case SystemFontMatrixTypes.Translation:
				return 1.0;
			case SystemFontMatrixTypes.Scaling:
			case (SystemFontMatrixTypes)3:
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

	public SystemFontMatrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
	{
		this = default(SystemFontMatrix);
		M11 = m11;
		M12 = m12;
		M21 = m21;
		M22 = m22;
		OffsetX = offsetX;
		OffsetY = offsetY;
		type = SystemFontMatrixTypes.Unknown;
		CheckMatrixType();
	}

	public static SystemFontMatrix operator *(SystemFontMatrix matrix1, SystemFontMatrix matrix2)
	{
		return new SystemFontMatrix(matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21, matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22, matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21, matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22, matrix1.OffsetX * matrix2.M11 + matrix1.OffsetY * matrix2.M21 + matrix2.OffsetX, matrix1.OffsetX * matrix2.M12 + matrix1.OffsetY * matrix2.M22 + matrix2.OffsetY);
	}

	public static bool operator ==(SystemFontMatrix a, SystemFontMatrix b)
	{
		if (a.M11 == b.M11 && a.M21 == b.M21 && a.M12 == b.M12 && a.M22 == b.M22 && a.OffsetX == b.OffsetX)
		{
			return a.OffsetY == b.OffsetY;
		}
		return false;
	}

	public static bool operator !=(SystemFontMatrix a, SystemFontMatrix b)
	{
		return !(a == b);
	}

	public bool IsIdentity()
	{
		return this == Identity;
	}

	public SystemFontMatrix Translate(double offsetX, double offsetY)
	{
		if (type == SystemFontMatrixTypes.Identity)
		{
			SetMatrix(1.0, 0.0, 0.0, 1.0, offsetX, offsetY, SystemFontMatrixTypes.Translation);
		}
		else if (type == SystemFontMatrixTypes.Unknown)
		{
			OffsetX += offsetX;
			OffsetY += offsetY;
		}
		else
		{
			OffsetX += offsetX;
			OffsetY += offsetY;
			type |= SystemFontMatrixTypes.Translation;
		}
		return this;
	}

	public SystemFontMatrix Scale(double scaleX, double scaleY, double centerX, double centerY)
	{
		this = new SystemFontMatrix(scaleX, 0.0, 0.0, scaleY, centerX, centerY) * this;
		return this;
	}

	public SystemFontMatrix ScaleAppend(double scaleX, double scaleY, double centerX, double centerY)
	{
		this *= new SystemFontMatrix(scaleX, 0.0, 0.0, scaleY, centerX, centerY);
		return this;
	}

	public SystemFontMatrix Rotate(double angle, double centerX, double centerY)
	{
		SystemFontMatrix systemFontMatrix = default(SystemFontMatrix);
		angle = Math.PI * angle / 180.0;
		double num = Math.Sin(angle);
		double num2 = Math.Cos(angle);
		double offsetX = centerX * (1.0 - num2) + centerY * num;
		double offsetY = centerY * (1.0 - num2) - centerX * num;
		systemFontMatrix.SetMatrix(num2, num, 0.0 - num, num2, offsetX, offsetY, SystemFontMatrixTypes.Unknown);
		this = systemFontMatrix * this;
		return this;
	}

	public bool Equals(SystemFontMatrix value)
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
		double num = point.X;
		double num2 = point.Y;
		double num3 = num * M11 + num2 * M21 + OffsetX;
		double num4 = num * M12 + num2 * M22 + OffsetY;
		return new Point((int)num3, (int)num4);
	}

	public override int GetHashCode()
	{
		return ((((((17 * 23 + M11.GetHashCode()) * 23 + M12.GetHashCode()) * 23 + M21.GetHashCode()) * 23 + M22.GetHashCode()) * 23 + OffsetX.GetHashCode()) * 23 + OffsetY.GetHashCode()) * 23 + type.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is SystemFontMatrix)
		{
			return Equals((SystemFontMatrix)obj);
		}
		return false;
	}

	public override string ToString()
	{
		return $"{M11} {M12} 0 | {M21} {M22} 0 | {OffsetX} {OffsetY} 1";
	}

	private void CheckMatrixType()
	{
		type = SystemFontMatrixTypes.Identity;
		if (M21 != 0.0 || M12 != 0.0)
		{
			type = SystemFontMatrixTypes.Unknown;
			return;
		}
		if (M11 != 1.0 || M22 != 1.0)
		{
			type = SystemFontMatrixTypes.Scaling;
		}
		if (OffsetX != 0.0 || OffsetY != 0.0)
		{
			type |= SystemFontMatrixTypes.Translation;
		}
		if ((type & (SystemFontMatrixTypes)3) == 0)
		{
			type = SystemFontMatrixTypes.Identity;
		}
	}

	private void SetMatrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY, SystemFontMatrixTypes type)
	{
		M11 = m11;
		M12 = m12;
		M21 = m21;
		M22 = m22;
		OffsetX = offsetX;
		OffsetY = offsetY;
		this.type = type;
	}
}
