using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class Path3D : Polygon
{
	private byte[] m_types;

	public byte[] Types => m_types;

	private Path3D(Plane3D plane)
		: base(plane.Normal, plane.D)
	{
	}

	private Path3D(Vector3D[] vs)
		: base(vs)
	{
	}

	public Path3D(Vector3D[] vs, byte[] types, BrushInfo br, Pen pen)
		: this(vs)
	{
		m_types = types;
		m_brInfo = br;
		m_pen = pen;
	}

	public Path3D(Path3D p3d)
		: base(p3d.Normal, p3d.D)
	{
		m_points = p3d.m_points.Clone() as Vector3D[];
		m_types = (byte[])p3d.Types.Clone();
		m_brInfo = p3d.BrushInfo;
		m_brush = p3d.Brush;
		m_pen = p3d.Pen;
		m_FigurePen = p3d.m_FigurePen;
	}

	public static Path3D FromGraphicsPath(GraphicsPath gp, double z, Brush br, Pen pen)
	{
		Path3D path3D = null;
		if (gp.PointCount > 0)
		{
			PathData pathData = gp.PathData;
			Vector3D[] array = new Vector3D[pathData.Points.Length];
			for (int i = 0; i < pathData.Points.Length; i++)
			{
				array[i] = new Vector3D(pathData.Points[i].X, pathData.Points[i].Y, z);
			}
			path3D = new Path3D(array);
			path3D.m_types = pathData.Types;
			path3D.m_pen = pen;
			path3D.m_brush = br;
		}
		return path3D;
	}

	public static Path3D FromGraphicsPath(GraphicsPath gp, double z, BrushInfo br, Pen pen)
	{
		Path3D path3D = null;
		if (gp.PointCount > 0)
		{
			PathData pathData = gp.PathData;
			Vector3D[] array = new Vector3D[pathData.Points.Length];
			for (int i = 0; i < pathData.Points.Length; i++)
			{
				array[i] = new Vector3D(pathData.Points[i].X, pathData.Points[i].Y, z);
			}
			path3D = new Path3D(array);
			path3D.m_types = pathData.Types;
			path3D.m_pen = pen;
			path3D.m_brInfo = br;
		}
		return path3D;
	}

	public static Path3D FromGraphicsPath(GraphicsPath gp, double z, Brush br)
	{
		Path3D path3D = null;
		if (gp.PointCount > 0)
		{
			PathData pathData = gp.PathData;
			Vector3D[] array = new Vector3D[pathData.Points.Length];
			for (int i = 0; i < pathData.Points.Length; i++)
			{
				array[i] = new Vector3D(pathData.Points[i].X, pathData.Points[i].Y, z);
			}
			path3D = new Path3D(array);
			path3D.m_types = pathData.Types;
			path3D.m_brush = br;
		}
		return path3D;
	}

	public static Path3D FromGraphicsPath(GraphicsPath gp, double z, Pen pen)
	{
		Path3D path3D = null;
		if (gp.PointCount > 0)
		{
			PathData pathData = gp.PathData;
			Vector3D[] array = new Vector3D[pathData.Points.Length];
			for (int i = 0; i < pathData.Points.Length; i++)
			{
				array[i] = new Vector3D(pathData.Points[i].X, pathData.Points[i].Y, z);
			}
			path3D = new Path3D(array);
			path3D.m_types = pathData.Types;
			path3D.m_pen = pen;
		}
		return path3D;
	}

	public static Path3D FromGraphicsPath(GraphicsPath gp, double z, BrushInfo br)
	{
		Path3D path3D = null;
		if (gp.PointCount > 0)
		{
			PathData pathData = gp.PathData;
			Vector3D[] array = new Vector3D[pathData.Points.Length];
			for (int i = 0; i < pathData.Points.Length; i++)
			{
				array[i] = new Vector3D(pathData.Points[i].X, pathData.Points[i].Y, z);
			}
			path3D = new Path3D(array);
			path3D.m_types = pathData.Types;
			path3D.m_brInfo = br;
		}
		return path3D;
	}

	public static Path3D FromGraphicsPath(GraphicsPath gp, Plane3D plane, double z, Brush br, Pen pen)
	{
		Path3D path3D = null;
		if (gp.PointCount > 0)
		{
			PathData pathData = gp.PathData;
			Vector3D[] array = new Vector3D[pathData.Points.Length];
			for (int i = 0; i < pathData.Points.Length; i++)
			{
				array[i] = new Vector3D(pathData.Points[i].X, pathData.Points[i].Y, z);
			}
			path3D = new Path3D(plane);
			path3D.m_points = array;
			path3D.m_types = pathData.Types;
			path3D.m_pen = pen;
			path3D.m_brush = br;
		}
		return path3D;
	}

	public GraphicsPath GetPath(PointF[] pts)
	{
		return new GraphicsPath(pts, m_types);
	}

	public override void Draw(Graphics3D g3d)
	{
		PointF[] array = new PointF[m_points.Length];
		for (int i = 0; i < m_points.Length; i++)
		{
			array[i] = g3d.Transform.ToScreen(m_points[i]);
		}
		GraphicsPath graphicsPath = new GraphicsPath(array, m_types);
		int coef = 0;
		if (g3d.Light)
		{
			Vector3D vector3D = !g3d.LightPosition;
			vector3D.Normalize();
			coef = (int)((double)g3d.LightCoeficient * (m_normal & vector3D));
		}
		if (m_brInfo != null)
		{
			FillPolygon(g3d.Graphics, m_brInfo, graphicsPath, coef);
		}
		if (m_brush != null)
		{
			FillPolygon(g3d.Graphics, m_brush, graphicsPath, coef);
		}
		if (m_pen != null)
		{
			g3d.Graphics.DrawPath(m_pen, graphicsPath);
		}
	}

	public override Polygon Clone()
	{
		return new Path3D(this);
	}
}
