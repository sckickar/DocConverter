using System.Collections.Generic;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontPathFigure
{
	public List<SystemFontPathSegment> Segments { get; set; }

	public bool IsClosed { get; set; }

	public bool IsFilled { get; set; }

	public Point StartPoint { get; set; }

	public SystemFontPathFigure()
	{
		Segments = new List<SystemFontPathSegment>();
	}

	public SystemFontPathFigure Clone()
	{
		SystemFontPathFigure systemFontPathFigure = new SystemFontPathFigure();
		systemFontPathFigure.IsClosed = IsClosed;
		systemFontPathFigure.IsFilled = IsFilled;
		systemFontPathFigure.StartPoint = StartPoint;
		foreach (SystemFontPathSegment segment in Segments)
		{
			systemFontPathFigure.Segments.Add(segment.Clone());
		}
		return systemFontPathFigure;
	}

	internal void Transform(SystemFontMatrix transformMatrix)
	{
		StartPoint = transformMatrix.Transform(StartPoint);
		foreach (SystemFontPathSegment segment in Segments)
		{
			segment.Transform(transformMatrix);
		}
	}
}
