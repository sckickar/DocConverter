using System.Collections;

namespace DocGen.Chart;

internal sealed class Path3DCollect : Polygon
{
	private ArrayList m_list = new ArrayList();

	private bool m_needRefresh = true;

	public override Vector3D[] Points
	{
		get
		{
			if (m_needRefresh)
			{
				RefreshPoints();
				m_needRefresh = false;
			}
			return m_points;
		}
	}

	public Path3DCollect(Polygon[] paths)
		: base(paths[0].Normal, paths[0].D)
	{
		m_list.AddRange(paths);
		isClipPolygon = false;
		int num = 3;
		while (Test() && num < Points.Length)
		{
			CalcNormal(Points[num], Points[0], Points[num / 2]);
			num++;
		}
	}

	public Path3DCollect(Polygon paths)
		: base(paths.Normal, paths.D)
	{
		m_list.Add(paths);
		isClipPolygon = false;
		for (int i = 3; i < Points.Length; i++)
		{
			if (!Test())
			{
				break;
			}
			CalcNormal(Points[i], Points[0], Points[i / 2]);
		}
	}

	public Path3DCollect(Path3DCollect p3dc)
		: base(p3dc.m_normal, p3dc.D)
	{
		for (int i = 0; i < p3dc.m_list.Count; i++)
		{
			if (p3dc.m_list[i] != null)
			{
				m_list.Add(((Polygon)p3dc.m_list[i]).Clone());
			}
		}
		isClipPolygon = false;
		for (int j = 3; j < Points.Length; j++)
		{
			if (!Test())
			{
				break;
			}
			CalcNormal(Points[j], Points[0], Points[j / 2]);
		}
	}

	public int Add(Polygon polygon)
	{
		if (polygon != null)
		{
			m_needRefresh = true;
		}
		if (polygon == null)
		{
			return -1;
		}
		return m_list.Add(polygon);
	}

	public override void Draw(Graphics3D g3d)
	{
		int i = 0;
		for (int count = m_list.Count; i < count && m_list[i] != null; i++)
		{
			(m_list[i] as Polygon).Draw(g3d);
		}
	}

	public override Polygon Clone()
	{
		return new Path3DCollect(this);
	}

	public override void Transform(Matrix3D matrix3D)
	{
		foreach (Polygon item in m_list)
		{
			item.Transform(matrix3D);
		}
		RefreshPoints();
		CalcNormal();
	}

	public void RefreshPoints()
	{
		ArrayList arrayList = new ArrayList();
		int i = 0;
		for (int count = m_list.Count; i < count; i++)
		{
			if (m_list[i] != null)
			{
				arrayList.AddRange((m_list[i] as Polygon).Points);
			}
		}
		m_points = (Vector3D[])arrayList.ToArray(typeof(Vector3D));
	}
}
