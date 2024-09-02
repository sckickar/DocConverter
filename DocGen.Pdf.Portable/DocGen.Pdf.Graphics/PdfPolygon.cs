using System;
using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfPolygon : PdfFillElement
{
	private List<PointF> m_points;

	public PointF[] Points
	{
		get
		{
			if (m_points == null)
			{
				m_points = new List<PointF>();
			}
			return m_points.ToArray();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Points");
			}
			if (m_points == null)
			{
				m_points = new List<PointF>();
			}
			m_points.Clear();
			m_points.AddRange(value);
		}
	}

	public int Count => Points.Length;

	public PdfPolygon(PointF[] points)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		Points = points;
	}

	public PdfPolygon(PdfPen pen, PointF[] points)
		: base(pen)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		Points = points;
	}

	public PdfPolygon(PdfBrush brush, PointF[] points)
		: base(brush)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		Points = points;
	}

	public PdfPolygon(PdfPen pen, PdfBrush brush, PointF[] points)
		: base(pen, brush)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		Points = points;
	}

	protected PdfPolygon()
	{
		m_points = new List<PointF>();
	}

	public void AddPoint(PointF point)
	{
		m_points.Add(point);
	}

	protected override RectangleF GetBoundsInternal()
	{
		RectangleF result = RectangleF.Empty;
		if (Points.Length != 0)
		{
			PointF[] points = Points;
			float num = points[0].X;
			float num2 = points[0].X;
			float num3 = points[0].Y;
			float num4 = points[0].Y;
			for (int i = 1; i < points.Length; i++)
			{
				PointF pointF = points[i];
				num = Math.Min(num, pointF.X);
				num2 = Math.Max(num2, pointF.X);
				num3 = Math.Min(num3, pointF.Y);
				num4 = Math.Max(num4, pointF.Y);
			}
			result = new RectangleF(num, num3, num2 - num, num4 - num3);
		}
		return result;
	}

	protected override void DrawInternal(PdfGraphics graphics)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		if (base.PdfTag != null)
		{
			graphics.Tag = base.PdfTag;
		}
		graphics.DrawPolygon(ObtainPen(), base.Brush, Points);
	}
}
