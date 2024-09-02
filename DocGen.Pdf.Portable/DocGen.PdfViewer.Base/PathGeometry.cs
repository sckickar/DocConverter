using System;
using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class PathGeometry
{
	public List<PathFigure> Figures { get; set; }

	public FillRule FillRule { get; set; }

	public Matrix TransformMatrix { get; set; }

	public bool IsEmpty
	{
		get
		{
			if (Figures != null)
			{
				return Figures.Count == 0;
			}
			return true;
		}
	}

	internal static PathGeometry CreateRectangle(Rect rect)
	{
		PathGeometry pathGeometry = new PathGeometry();
		PathFigure item = new PathFigure
		{
			StartPoint = new Point(rect.Left, rect.Top),
			IsClosed = true,
			IsFilled = true,
			Segments = 
			{
				(PathSegment)new LineSegment
				{
					Point = new Point(rect.Right, rect.Top)
				},
				(PathSegment)new LineSegment
				{
					Point = new Point(rect.Right, rect.Bottom)
				},
				(PathSegment)new LineSegment
				{
					Point = new Point(rect.Left, rect.Bottom)
				}
			}
		};
		pathGeometry.Figures.Add(item);
		return pathGeometry;
	}

	private static void Compare(Point point, ref double minX, ref double maxX, ref double minY, ref double maxY)
	{
		minX = Math.Min(point.X, minX);
		maxX = Math.Max(point.X, maxX);
		minY = Math.Min(point.Y, minY);
		maxY = Math.Max(point.Y, maxY);
	}

	public PathGeometry()
	{
		Figures = new List<PathFigure>();
		TransformMatrix = Matrix.Identity;
	}

	public PathGeometry Clone()
	{
		PathGeometry pathGeometry = new PathGeometry();
		pathGeometry.FillRule = FillRule;
		pathGeometry.TransformMatrix = TransformMatrix;
		foreach (PathFigure figure in Figures)
		{
			pathGeometry.Figures.Add(figure.Clone());
		}
		return pathGeometry;
	}

	public Rect GetBoundingRect()
	{
		double minX = double.PositiveInfinity;
		double minY = double.PositiveInfinity;
		double maxX = double.NegativeInfinity;
		double maxY = double.NegativeInfinity;
		foreach (PathFigure figure in Figures)
		{
			Compare(figure.StartPoint, ref minX, ref maxX, ref minY, ref maxY);
			foreach (PathSegment segment in figure.Segments)
			{
				if (segment is LineSegment)
				{
					Compare(((LineSegment)segment).Point, ref minX, ref maxX, ref minY, ref maxY);
				}
				else if (segment is BezierSegment)
				{
					BezierSegment obj = (BezierSegment)segment;
					Compare(obj.Point1, ref minX, ref maxX, ref minY, ref maxY);
					Compare(obj.Point2, ref minX, ref maxX, ref minY, ref maxY);
					Compare(obj.Point3, ref minX, ref maxX, ref minY, ref maxY);
				}
			}
		}
		return new Rect(new Point(minX, minY), new Point(maxX, maxY));
	}
}
