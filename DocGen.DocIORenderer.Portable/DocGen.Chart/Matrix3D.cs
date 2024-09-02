using System;
using System.ComponentModel;

namespace DocGen.Chart;

internal struct Matrix3D
{
	private const int MATRIX_SIZE = 4;

	private double[][] m_data;

	public bool IsAffine
	{
		get
		{
			if (m_data[0][3] == 0.0 && m_data[1][3] == 0.0 && m_data[2][3] == 0.0)
			{
				return m_data[3][3] == 1.0;
			}
			return false;
		}
	}

	public double this[int i, int j]
	{
		get
		{
			return m_data[i][j];
		}
		set
		{
			m_data[i][j] = value;
		}
	}

	public static Matrix3D Identity => GetIdentity();

	private Matrix3D(int size)
	{
		m_data = new double[size][];
		for (int i = 0; i < size; i++)
		{
			m_data[i] = new double[size];
		}
	}

	public Matrix3D(double m11, double m12, double m13, double m14, double m21, double m22, double m23, double m24, double m31, double m32, double m33, double m34, double m41, double m42, double m43, double m44)
		: this(4)
	{
		m_data[0][0] = m11;
		m_data[1][0] = m12;
		m_data[2][0] = m13;
		m_data[3][0] = m14;
		m_data[0][1] = m21;
		m_data[1][1] = m22;
		m_data[2][1] = m23;
		m_data[3][1] = m24;
		m_data[0][2] = m31;
		m_data[1][2] = m32;
		m_data[2][2] = m33;
		m_data[3][2] = m34;
		m_data[0][3] = m41;
		m_data[1][3] = m42;
		m_data[2][3] = m43;
		m_data[3][3] = m44;
	}

	public static Matrix3D operator +(Matrix3D m1, Matrix3D m2)
	{
		Matrix3D result = new Matrix3D(4);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				result[i, j] = m1[i, j] + m2[i, j];
			}
		}
		return result;
	}

	public static Vector3D operator *(Matrix3D m1, Vector3D point)
	{
		double num = m1.m_data[0][0] * point.m_x + m1.m_data[1][0] * point.m_y + m1.m_data[2][0] * point.m_z + m1.m_data[3][0];
		double num2 = m1.m_data[0][1] * point.m_x + m1.m_data[1][1] * point.m_y + m1.m_data[2][1] * point.m_z + m1.m_data[3][1];
		double num3 = m1.m_data[0][2] * point.m_x + m1.m_data[1][2] * point.m_y + m1.m_data[2][2] * point.m_z + m1.m_data[3][2];
		if (!m1.IsAffine)
		{
			double num4 = 1.0 / (m1.m_data[0][3] * point.m_x + m1.m_data[1][3] * point.m_y + m1.m_data[2][3] * point.m_z + m1.m_data[3][3]);
			num *= num4;
			num2 *= num4;
			num3 *= num4;
		}
		return new Vector3D(num, num2, num3);
	}

	public static Vector3D operator &(Matrix3D m1, Vector3D v1)
	{
		double x = m1.m_data[0][0] * v1.m_x + m1.m_data[1][0] * v1.m_y + m1.m_data[2][0] * v1.m_z;
		double y = m1.m_data[0][1] * v1.m_x + m1.m_data[1][1] * v1.m_y + m1.m_data[2][1] * v1.m_z;
		double z = m1.m_data[0][2] * v1.m_x + m1.m_data[1][2] * v1.m_y + m1.m_data[2][2] * v1.m_z;
		return new Vector3D(x, y, z);
	}

	public static Matrix3D operator *(double f1, Matrix3D m1)
	{
		int num = m1.m_data.Length;
		Matrix3D result = new Matrix3D(num);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				result.m_data[i][j] = m1.m_data[i][j] * f1;
			}
		}
		return result;
	}

	public static Matrix3D operator *(Matrix3D m1, Matrix3D m2)
	{
		Matrix3D identity = GetIdentity();
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				double num = 0.0;
				for (int k = 0; k < 4; k++)
				{
					num += m1[k, j] * m2[i, k];
				}
				identity[i, j] = num;
			}
		}
		return identity;
	}

	public static bool operator ==(Matrix3D m1, Matrix3D m2)
	{
		bool result = true;
		for (int i = 0; i < m1.m_data.Length; i++)
		{
			for (int j = 0; j < m1.m_data.Length; j++)
			{
				if (m1.m_data[i][j] != m2.m_data[i][j])
				{
					result = false;
				}
			}
		}
		return result;
	}

	public static bool operator !=(Matrix3D m1, Matrix3D m2)
	{
		bool flag = true;
		for (int i = 0; i < m1.m_data.Length; i++)
		{
			for (int j = 0; j < m1.m_data.Length; j++)
			{
				if (m1.m_data[i][j] != m2.m_data[i][j])
				{
					flag = false;
				}
			}
		}
		return !flag;
	}

	[Obsolete("Use GetInterval")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static Matrix3D GetInvertal(Matrix3D matrix3D)
	{
		Matrix3D identity = Identity;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				identity[i, j] = GetMinor(matrix3D, i, j);
			}
		}
		identity = Transposed(identity);
		return 1.0 / GetD(matrix3D) * identity;
	}

	public static Matrix3D GetInterval(Matrix3D matrix3D)
	{
		Matrix3D identity = Identity;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				identity[i, j] = GetMinor(matrix3D, i, j);
			}
		}
		identity = Transposed(identity);
		return 1.0 / GetD(matrix3D) * identity;
	}

	public static double GetMinor(Matrix3D dd, int columnIndex, int rowIndex)
	{
		return (double)(((columnIndex + rowIndex) % 2 == 0) ? 1 : (-1)) * GetDeterminant(GetMMtr(dd.m_data, columnIndex, rowIndex));
	}

	public static double GetD(Matrix3D matrix3D)
	{
		return GetDeterminant(matrix3D.m_data);
	}

	public static Matrix3D GetIdentity()
	{
		Matrix3D result = new Matrix3D(4);
		for (int i = 0; i < 4; i++)
		{
			result[i, i] = 1.0;
		}
		return result;
	}

	public static Vector3D GetGauss(Matrix3D m1)
	{
		Matrix3D matrix3D = m1;
		for (int i = 0; i < 3; i++)
		{
			for (int j = i; j < 3; j++)
			{
				double num = matrix3D[i, j];
				int num2 = 0;
				while (num != 0.0 && num2 < 4)
				{
					matrix3D[num2, j] *= 1.0 / num;
					num2++;
				}
			}
			for (int k = i; k < 2; k++)
			{
				for (int l = 0; l < 4 && matrix3D[l, i] != 0.0; l++)
				{
					matrix3D[l, k + 1] -= matrix3D[l, i];
				}
			}
		}
		double num3 = matrix3D[3, 2] / matrix3D[2, 2];
		double num4 = (matrix3D[3, 1] - num3 * matrix3D[2, 1]) / matrix3D[1, 1];
		return new Vector3D((matrix3D[3, 0] - num3 * matrix3D[2, 0] - num4 * matrix3D[1, 0]) / m1[0, 0], num4, num3);
	}

	public static Matrix3D Transform(double x, double y, double z)
	{
		Matrix3D identity = GetIdentity();
		identity.m_data[3][0] = x;
		identity.m_data[3][1] = y;
		identity.m_data[3][2] = z;
		return identity;
	}

	public static Matrix3D Turn(double angle)
	{
		Matrix3D identity = GetIdentity();
		identity[0, 0] = Math.Cos(angle);
		identity[2, 0] = 0.0 - Math.Sin(angle);
		identity[0, 2] = Math.Sin(angle);
		identity[2, 2] = Math.Cos(angle);
		return identity;
	}

	public static Matrix3D Tilt(double angle)
	{
		Matrix3D identity = GetIdentity();
		identity[1, 1] = Math.Cos(angle);
		identity[2, 1] = Math.Sin(angle);
		identity[1, 2] = 0.0 - Math.Sin(angle);
		identity[2, 2] = Math.Cos(angle);
		return identity;
	}

	public static Matrix3D Twist(double angle)
	{
		Matrix3D identity = GetIdentity();
		identity[0, 0] = Math.Cos(angle);
		identity[1, 0] = Math.Sin(angle);
		identity[0, 1] = 0.0 - Math.Sin(angle);
		identity[1, 1] = Math.Cos(angle);
		return identity;
	}

	public static Matrix3D Scale(double dx, double dy, double dz)
	{
		Matrix3D identity = GetIdentity();
		identity[0, 0] = dx;
		identity[1, 1] = dy;
		identity[2, 2] = dz;
		return identity;
	}

	public static Matrix3D Transposed(Matrix3D matrix3D)
	{
		Matrix3D identity = Identity;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				identity[i, j] = matrix3D[j, i];
			}
		}
		return identity;
	}

	public static Matrix3D Shear(double xy, double xz, double yx, double yz, double zx, double zy)
	{
		Matrix3D identity = Identity;
		identity[1, 0] = xy;
		identity[2, 0] = xz;
		identity[0, 1] = yx;
		identity[2, 1] = yz;
		identity[0, 2] = zx;
		identity[1, 2] = zy;
		return identity;
	}

	public static Matrix3D RotateAlongOX(double angle)
	{
		Matrix3D identity = Identity;
		identity[1, 1] = Math.Cos(angle);
		identity[1, 2] = 0.0 - Math.Sin(angle);
		identity[2, 1] = Math.Sin(angle);
		identity[2, 2] = Math.Cos(angle);
		return identity;
	}

	public static Matrix3D RotateAlongOY(double angle)
	{
		Matrix3D identity = Identity;
		identity[0, 0] = Math.Cos(angle);
		identity[0, 2] = 0.0 - Math.Sin(angle);
		identity[2, 0] = Math.Sin(angle);
		identity[2, 2] = Math.Cos(angle);
		return identity;
	}

	public static Matrix3D RotateAlongOZ(double angle)
	{
		Matrix3D identity = Identity;
		identity[0, 0] = Math.Cos(angle);
		identity[0, 1] = 0.0 - Math.Sin(angle);
		identity[1, 0] = Math.Sin(angle);
		identity[1, 1] = Math.Cos(angle);
		return identity;
	}

	public override bool Equals(object obj)
	{
		Matrix3D matrix3D = (Matrix3D)obj;
		bool result = true;
		for (int i = 0; i < matrix3D.m_data.Length; i++)
		{
			for (int j = 0; j < matrix3D.m_data.Length; j++)
			{
				if (matrix3D.m_data[i][j] != m_data[i][j])
				{
					result = false;
				}
			}
		}
		return result;
	}

	public override int GetHashCode()
	{
		return m_data.GetHashCode();
	}

	private static double GetDeterminant(double[][] dd)
	{
		int num = dd.Length;
		double num2 = 0.0;
		if (num < 2)
		{
			num2 = dd[0][0];
		}
		else
		{
			int num3 = 1;
			for (int i = 0; i < num; i++)
			{
				double[][] mMtr = GetMMtr(dd, i, 0);
				num2 += (double)num3 * dd[i][0] * GetDeterminant(mMtr);
				num3 = ((num3 <= 0) ? 1 : (-1));
			}
		}
		return num2;
	}

	private static double[][] GetMMtr(double[][] dd, int columnIndex, int rowIndex)
	{
		int num = dd.Length - 1;
		double[][] array = new double[num][];
		for (int i = 0; i < num; i++)
		{
			int num2 = ((i >= columnIndex) ? (i + 1) : i);
			array[i] = new double[num];
			for (int j = 0; j < num; j++)
			{
				int num3 = ((j >= rowIndex) ? (j + 1) : j);
				array[i][j] = dd[num2][num3];
			}
		}
		return array;
	}
}
