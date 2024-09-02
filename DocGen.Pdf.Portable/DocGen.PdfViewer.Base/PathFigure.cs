using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class PathFigure
{
	public List<PathSegment> Segments { get; set; }

	public bool IsClosed { get; set; }

	public bool IsFilled { get; set; }

	public Point StartPoint { get; set; }

	public PathFigure()
	{
		Segments = new List<PathSegment>();
	}

	public PathFigure Clone()
	{
		PathFigure pathFigure = new PathFigure();
		pathFigure.IsClosed = IsClosed;
		pathFigure.IsFilled = IsFilled;
		pathFigure.StartPoint = StartPoint;
		foreach (PathSegment segment in Segments)
		{
			pathFigure.Segments.Add(segment.Clone());
		}
		return pathFigure;
	}

	internal void Transform(Matrix transformMatrix)
	{
		StartPoint = transformMatrix.Transform(StartPoint);
		foreach (PathSegment segment in Segments)
		{
			segment.Transform(transformMatrix);
		}
	}
}
