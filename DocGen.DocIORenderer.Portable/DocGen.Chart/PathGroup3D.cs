using System;
using System.Collections.Generic;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class PathGroup3D : Polygon
{
	private class PathItem
	{
		public Pen Pen;

		public Brush Brush;

		public BrushInfo BrushInfo;

		public int Index;

		public int Length;

		public byte[] Types;
	}

	private List<PathItem> m_items = new List<PathItem>();

	public PathGroup3D(double z)
		: base(new Vector3D(0.0, 0.0, 1.0), z)
	{
	}

	public void AddPath(GraphicsPath gp, Brush brush, BrushInfo brushInfo, Pen pen)
	{
		PathData pathData = gp.PathData;
		PathItem pathItem = new PathItem();
		pathItem.Brush = brush;
		pathItem.BrushInfo = brushInfo;
		pathItem.Pen = pen;
		pathItem.Index = ((m_points != null) ? m_points.Length : 0);
		pathItem.Types = pathData.Types;
		pathItem.Length = pathItem.Types.Length;
		List<Vector3D> list = ((m_points == null) ? new List<Vector3D>() : new List<Vector3D>(m_points));
		PointF[] points = pathData.Points;
		for (int i = 0; i < points.Length; i++)
		{
			PointF pointF = points[i];
			list.Add(new Vector3D(pointF.X, pointF.Y, m_d));
		}
		m_items.Add(pathItem);
		m_points = list.ToArray();
	}

	internal override Vector3D GetNormal(Matrix3D transform)
	{
		Vector3D result = transform & m_normal;
		result.Normalize();
		return result;
	}

	public override void Draw(Graphics3D g3d)
	{
		PointF[] array = new PointF[m_points.Length];
		for (int i = 0; i < m_points.Length; i++)
		{
			array[i] = g3d.Transform.ToScreen(m_points[i]);
		}
		int coef = 0;
		if (g3d.Light)
		{
			Vector3D vector3D = !g3d.LightPosition;
			vector3D.Normalize();
			coef = (int)((double)g3d.LightCoeficient * (m_normal & vector3D));
		}
		foreach (PathItem item in m_items)
		{
			PointF[] array2 = new PointF[item.Length];
			Array.Copy(array, item.Index, array2, 0, item.Length);
			GraphicsPath graphicsPath = new GraphicsPath(array2, item.Types);
			if (item.BrushInfo != null)
			{
				FillPolygon(g3d.Graphics, item.BrushInfo, graphicsPath, coef);
			}
			if (item.Brush != null)
			{
				FillPolygon(g3d.Graphics, item.Brush, graphicsPath, coef);
			}
			if (item.Pen != null)
			{
				g3d.Graphics.DrawPath(item.Pen, graphicsPath);
			}
		}
	}

	public override Polygon Clone()
	{
		return new PathGroup3D(m_d)
		{
			m_items = m_items
		};
	}
}
