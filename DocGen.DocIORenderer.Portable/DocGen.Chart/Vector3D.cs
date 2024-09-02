using System;

namespace DocGen.Chart;

internal struct Vector3D
{
	internal double m_x;

	internal double m_y;

	internal double m_z;

	public static readonly Vector3D Empty = new Vector3D(0.0, 0.0, 0.0);

	public double X => m_x;

	public double Y => m_y;

	public double Z => m_z;

	public bool IsEmpty
	{
		get
		{
			if (m_x == 0.0 && m_y == 0.0)
			{
				return m_z == 0.0;
			}
			return false;
		}
	}

	public bool IsValid
	{
		get
		{
			if (!double.IsNaN(m_x) && !double.IsNaN(m_y))
			{
				return !double.IsNaN(m_z);
			}
			return false;
		}
	}

	public Vector3D(double x, double y, double z)
	{
		m_x = x;
		m_y = y;
		m_z = z;
	}

	public static Vector3D operator -(Vector3D v1, Vector3D v2)
	{
		return new Vector3D(v1.m_x - v2.m_x, v1.m_y - v2.m_y, v1.m_z - v2.m_z);
	}

	public static Vector3D operator +(Vector3D v1, Vector3D v2)
	{
		return new Vector3D(v1.m_x + v2.m_x, v1.m_y + v2.m_y, v1.m_z + v2.m_z);
	}

	public static Vector3D operator *(Vector3D v1, Vector3D v2)
	{
		double x = v1.m_y * v2.m_z - v2.m_y * v1.m_z;
		double y = v1.m_z * v2.m_x - v2.m_z * v1.m_x;
		double z = v1.m_x * v2.m_y - v2.m_x * v1.m_y;
		return new Vector3D(x, y, z);
	}

	public static double operator &(Vector3D v1, Vector3D v2)
	{
		return v1.m_x * v2.m_x + v1.m_y * v2.m_y + v1.m_z * v2.m_z;
	}

	public static Vector3D operator *(Vector3D v1, double val)
	{
		double x = v1.m_x * val;
		double y = v1.m_y * val;
		double z = v1.m_z * val;
		return new Vector3D(x, y, z);
	}

	public static Vector3D operator !(Vector3D v1)
	{
		return new Vector3D(0.0 - v1.m_x, 0.0 - v1.m_y, 0.0 - v1.m_z);
	}

	public static bool operator ==(Vector3D v1, Vector3D v2)
	{
		if (v1.m_x == v2.m_x && v1.m_y == v2.m_y)
		{
			return v1.m_z == v2.m_z;
		}
		return false;
	}

	public static bool operator !=(Vector3D v1, Vector3D v2)
	{
		if (v1.m_x == v2.m_x && v1.m_y == v2.m_y)
		{
			return v1.m_z != v2.m_z;
		}
		return true;
	}

	public double GetLength()
	{
		return Math.Sqrt(this & this);
	}

	public void Normalize()
	{
		double length = GetLength();
		m_x /= length;
		m_y /= length;
		m_z /= length;
	}

	public override string ToString()
	{
		return $"X = {m_x}, Y = {m_y}, Z = {m_z}";
	}

	public override bool Equals(object obj)
	{
		bool result = false;
		if (obj is Vector3D vector3D)
		{
			result = vector3D.m_x == m_x && vector3D.m_y == m_y && vector3D.m_z == m_z;
		}
		return result;
	}

	public override int GetHashCode()
	{
		return m_x.GetHashCode() ^ m_y.GetHashCode() ^ m_z.GetHashCode();
	}
}
