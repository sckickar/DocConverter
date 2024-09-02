using System;
using System.Collections.Generic;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontPathGeometry
{
	public List<SystemFontPathFigure> Figures { get; set; }

	public SystemFontFillRule FillRule { get; set; }

	public SystemFontMatrix TransformMatrix { get; set; }

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

	internal static SystemFontPathGeometry CreateRectangle(Rect rect)
	{
		SystemFontPathGeometry systemFontPathGeometry = new SystemFontPathGeometry();
		SystemFontPathFigure item = new SystemFontPathFigure
		{
			StartPoint = new Point(rect.Left, rect.Top),
			IsClosed = true,
			IsFilled = true,
			Segments = 
			{
				(SystemFontPathSegment)new SystemFontLineSegment
				{
					Point = new Point(rect.Right, rect.Top)
				},
				(SystemFontPathSegment)new SystemFontLineSegment
				{
					Point = new Point(rect.Right, rect.Bottom)
				},
				(SystemFontPathSegment)new SystemFontLineSegment
				{
					Point = new Point(rect.Left, rect.Bottom)
				}
			}
		};
		systemFontPathGeometry.Figures.Add(item);
		return systemFontPathGeometry;
	}

	private static void Compare(Point point, ref double minX, ref double maxX, ref double minY, ref double maxY)
	{
		minX = Math.Min(point.X, minX);
		maxX = Math.Max(point.X, maxX);
		minY = Math.Min(point.Y, minY);
		maxY = Math.Max(point.Y, maxY);
	}

	public SystemFontPathGeometry()
	{
		Figures = new List<SystemFontPathFigure>();
		TransformMatrix = SystemFontMatrix.Identity;
	}

	public SystemFontPathGeometry Clone()
	{
		SystemFontPathGeometry systemFontPathGeometry = new SystemFontPathGeometry();
		systemFontPathGeometry.FillRule = FillRule;
		systemFontPathGeometry.TransformMatrix = TransformMatrix;
		foreach (SystemFontPathFigure figure in Figures)
		{
			systemFontPathGeometry.Figures.Add(figure.Clone());
		}
		return systemFontPathGeometry;
	}

	public Rect GetBoundingRect()
	{
		double minX = double.PositiveInfinity;
		double minY = double.PositiveInfinity;
		double maxX = double.NegativeInfinity;
		double maxY = double.NegativeInfinity;
		foreach (SystemFontPathFigure figure in Figures)
		{
			Compare(figure.StartPoint, ref minX, ref maxX, ref minY, ref maxY);
			foreach (SystemFontPathSegment segment in figure.Segments)
			{
				if (segment is SystemFontLineSegment)
				{
					Compare(((SystemFontLineSegment)segment).Point, ref minX, ref maxX, ref minY, ref maxY);
				}
				else if (segment is SystemFontBezierSegment)
				{
					SystemFontBezierSegment obj = (SystemFontBezierSegment)segment;
					Compare(obj.Point1, ref minX, ref maxX, ref minY, ref maxY);
					Compare(obj.Point2, ref minX, ref maxX, ref minY, ref maxY);
					Compare(obj.Point3, ref minX, ref maxX, ref minY, ref maxY);
				}
			}
		}
		return new Rect(new Point(minX, minY), new Point(maxX, maxY));
	}
}
