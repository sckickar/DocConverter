using DocGen.Drawing;

namespace DocGen.Chart;

internal class Transform3D
{
	private bool m_needUpdate = true;

	private Matrix3D m_centeredMatrix = Matrix3D.Identity;

	private Matrix3D m_viewMatrix = Matrix3D.Identity;

	private Matrix3D m_projectionMatrix = Matrix3D.Identity;

	private Matrix3D m_resultMatrix = Matrix3D.Identity;

	public Matrix3D Centered
	{
		get
		{
			return m_centeredMatrix;
		}
		set
		{
			if (m_centeredMatrix != value)
			{
				m_centeredMatrix = value;
				m_needUpdate = true;
			}
		}
	}

	public Matrix3D View
	{
		get
		{
			return m_viewMatrix;
		}
		set
		{
			if (m_viewMatrix != value)
			{
				m_viewMatrix = value;
				m_needUpdate = true;
			}
		}
	}

	public Matrix3D Projection
	{
		get
		{
			return m_projectionMatrix;
		}
		set
		{
			if (m_projectionMatrix != value)
			{
				m_projectionMatrix = value;
				m_needUpdate = true;
			}
		}
	}

	public Matrix3D Result
	{
		get
		{
			if (m_needUpdate)
			{
				m_resultMatrix = Matrix3D.GetInterval(m_centeredMatrix) * m_projectionMatrix * m_viewMatrix * m_centeredMatrix;
				m_needUpdate = false;
			}
			return m_resultMatrix;
		}
	}

	public void SetCenter(Vector3D center)
	{
		m_centeredMatrix = Matrix3D.Transform(0.0 - center.X, 0.0 - center.Y, 0.0 - center.Z);
		m_needUpdate = true;
	}

	public void SetPerspective(double distance)
	{
		m_projectionMatrix = Matrix3D.GetIdentity();
		m_projectionMatrix[0, 0] = distance;
		m_projectionMatrix[1, 1] = distance;
		m_projectionMatrix[2, 3] = 1.0;
		m_projectionMatrix[3, 3] = distance;
		m_needUpdate = true;
	}

	public void SetLookAt(Vector3D pos, Vector3D dir, Vector3D up)
	{
		m_viewMatrix = Matrix3D.GetIdentity();
		Vector3D vector3D = !dir;
		vector3D.Normalize();
		Vector3D vector3D2 = up * vector3D;
		vector3D2.Normalize();
		Vector3D vector3D3 = vector3D * vector3D2;
		double value = 0.0 - (vector3D2 & pos);
		double value2 = 0.0 - (vector3D3 & pos);
		double value3 = 0.0 - (vector3D & pos);
		m_viewMatrix[0, 0] = vector3D2.X;
		m_viewMatrix[0, 1] = vector3D3.X;
		m_viewMatrix[0, 2] = vector3D.X;
		m_viewMatrix[1, 0] = vector3D2.Y;
		m_viewMatrix[1, 1] = vector3D3.Y;
		m_viewMatrix[1, 2] = vector3D.Y;
		m_viewMatrix[2, 0] = vector3D2.Z;
		m_viewMatrix[2, 1] = vector3D3.Z;
		m_viewMatrix[2, 2] = vector3D.Z;
		m_viewMatrix[3, 0] = value;
		m_viewMatrix[3, 1] = value2;
		m_viewMatrix[3, 2] = value3;
		m_needUpdate = true;
	}

	public PointF ToScreen(Vector3D vector3d)
	{
		vector3d = Result * vector3d;
		return new PointF((float)vector3d.m_x, (float)vector3d.m_y);
	}

	public Vector3D ToPlane(PointF point, Plane3D plane)
	{
		Vector3D vector3D = new Vector3D(point.X, point.Y, 0.0);
		Vector3D vector3D2 = vector3D + new Vector3D(0.0, 0.0, 0.1);
		vector3D = m_centeredMatrix * vector3D;
		vector3D2 = m_centeredMatrix * vector3D2;
		vector3D = Matrix3D.GetInterval(m_projectionMatrix) * vector3D;
		vector3D2 = Matrix3D.GetInterval(m_projectionMatrix) * vector3D2;
		vector3D = plane.GetPoint(vector3D, vector3D2 - vector3D);
		vector3D = Matrix3D.GetInterval(m_viewMatrix) * vector3D;
		return Matrix3D.GetInterval(m_centeredMatrix) * vector3D;
	}
}
