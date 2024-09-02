namespace DocGen.Chart;

internal class Plane3D
{
	protected Vector3D m_normal;

	protected double m_d;

	public Vector3D Normal => m_normal;

	public double A => m_normal.X;

	public double B => m_normal.Y;

	public double C => m_normal.Z;

	public double D => m_d;

	public Plane3D(Vector3D normal, double d)
	{
		m_normal = normal;
		m_d = d;
	}

	public Plane3D(double a, double b, double c, double d)
	{
		m_normal = new Vector3D(a, b, c);
		m_d = d;
	}

	public Plane3D(Vector3D v1, Vector3D v2, Vector3D v3)
	{
		CalcNormal(v1, v2, v3);
	}

	public Vector3D GetPoint(double x, double y)
	{
		double z = (0.0 - (A * x + B * y + D)) / C;
		return new Vector3D(x, y, z);
	}

	public Vector3D GetPoint(Vector3D pos, Vector3D ray)
	{
		double num = ((m_normal * (0.0 - m_d) - pos) & m_normal) / (m_normal & ray);
		return pos + ray * num;
	}

	public virtual void Transform(Matrix3D matrix)
	{
		Vector3D vector3D = matrix * (m_normal * (0.0 - m_d));
		m_normal = matrix & m_normal;
		m_normal.Normalize();
		m_d = 0.0 - (m_normal & vector3D);
	}

	public Plane3D Clone(Matrix3D matrix)
	{
		Plane3D plane3D = new Plane3D(m_normal, m_d);
		plane3D.Transform(matrix);
		return plane3D;
	}

	public bool Test()
	{
		return !m_normal.IsValid;
	}

	protected void CalcNormal(Vector3D v1, Vector3D v2, Vector3D v3)
	{
		m_normal = ChartMath.GetNormal(v1, v2, v3);
		m_d = 0.0 - (A * v1.X + B * v1.Y + C * v1.Z);
	}
}
