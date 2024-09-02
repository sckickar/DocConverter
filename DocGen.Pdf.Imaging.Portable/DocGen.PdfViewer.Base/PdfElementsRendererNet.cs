using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.PdfViewer.Base;

internal class PdfElementsRendererNet
{
	private Dictionary<int, Dictionary<ushort, GraphicsPath>> glyphsCache = new Dictionary<int, Dictionary<ushort, GraphicsPath>>();

	private Dictionary<PathGeometry, GraphicsPath> shapesCache = new Dictionary<PathGeometry, GraphicsPath>();

	private readonly object syncObj = new object();

	public static DocGen.Drawing.Matrix GetTransformationMatrix(Matrix transform)
	{
		return new DocGen.Drawing.Matrix((float)transform.M11, (float)transform.M12, (float)transform.M21, (float)transform.M22, (float)transform.OffsetX, (float)transform.OffsetY);
	}

	public GraphicsPath GetGeometry(PathGeometry geometry)
	{
		lock (syncObj)
		{
			if (!shapesCache.TryGetValue(geometry, out var value))
			{
				value = GetGeometry(geometry, Matrix.Identity);
				shapesCache[geometry] = value;
			}
			return value;
		}
	}

	public GraphicsPath GetGeometry(PathGeometry geometry, Matrix transform)
	{
		lock (syncObj)
		{
			DocGen.Drawing.Matrix transformationMatrix = GetTransformationMatrix(transform);
			GraphicsPath graphicsPath = new GraphicsPath();
			foreach (PathFigure figure in geometry.Figures)
			{
				graphicsPath.StartFigure();
				PointF pointF = new PointF((float)figure.StartPoint.X, (float)figure.StartPoint.Y);
				foreach (PathSegment segment in figure.Segments)
				{
					if (segment is LineSegment)
					{
						LineSegment lineSegment = (LineSegment)segment;
						PointF[] array = new PointF[2]
						{
							pointF,
							new PointF((float)lineSegment.Point.X, (float)lineSegment.Point.Y)
						};
						transformationMatrix.TransformPoints(array);
						graphicsPath.AddLine(array[0], array[1]);
						pointF = new PointF((float)lineSegment.Point.X, (float)lineSegment.Point.Y);
					}
					else if (segment is BezierSegment)
					{
						BezierSegment bezierSegment = segment as BezierSegment;
						PointF[] array2 = new PointF[4]
						{
							pointF,
							new PointF((float)bezierSegment.Point1.X, (float)bezierSegment.Point1.Y),
							new PointF((float)bezierSegment.Point2.X, (float)bezierSegment.Point2.Y),
							new PointF((float)bezierSegment.Point3.X, (float)bezierSegment.Point3.Y)
						};
						transformationMatrix.TransformPoints(array2);
						graphicsPath.AddBezier(array2[0], array2[1], array2[2], array2[3]);
						pointF = new PointF((float)bezierSegment.Point3.X, (float)bezierSegment.Point3.Y);
					}
				}
				if (figure.IsClosed)
				{
					graphicsPath.CloseFigure();
				}
			}
			return graphicsPath;
		}
	}

	public GraphicsPath RenderGlyph(Glyph glyph)
	{
		lock (syncObj)
		{
			if (!glyphsCache.TryGetValue(glyph.FontId, out var value))
			{
				value = new Dictionary<ushort, GraphicsPath>();
				glyphsCache[glyph.FontId] = value;
			}
			GraphicsPath value2 = null;
			if (!value.TryGetValue(glyph.GlyphId, out value2))
			{
				value2 = RenderGlyph(glyph, Matrix.Identity);
				value[glyph.GlyphId] = value2;
			}
			return value2;
		}
	}

	private GraphicsPath RenderGlyph(Glyph g, Matrix transform)
	{
		lock (syncObj)
		{
			DocGen.Drawing.Matrix transformationMatrix = GetTransformationMatrix(transform);
			GraphicsPath graphicsPath = new GraphicsPath();
			if (g.Outlines == null)
			{
				return graphicsPath;
			}
			foreach (PathFigure outline in g.Outlines)
			{
				graphicsPath.StartFigure();
				PointF pointF = new PointF((float)outline.StartPoint.X, (float)outline.StartPoint.Y);
				foreach (PathSegment segment in outline.Segments)
				{
					if (segment is LineSegment)
					{
						LineSegment lineSegment = (LineSegment)segment;
						PointF[] array = new PointF[2]
						{
							pointF,
							new PointF((float)lineSegment.Point.X, (float)lineSegment.Point.Y)
						};
						transformationMatrix.TransformPoints(array);
						graphicsPath.AddLine(array[0], array[1]);
						pointF = new PointF((float)lineSegment.Point.X, (float)lineSegment.Point.Y);
					}
					else if (segment is BezierSegment)
					{
						BezierSegment bezierSegment = segment as BezierSegment;
						PointF[] array2 = new PointF[4]
						{
							pointF,
							new PointF((float)bezierSegment.Point1.X, (float)bezierSegment.Point1.Y),
							new PointF((float)bezierSegment.Point2.X, (float)bezierSegment.Point2.Y),
							new PointF((float)bezierSegment.Point3.X, (float)bezierSegment.Point3.Y)
						};
						transformationMatrix.TransformPoints(array2);
						graphicsPath.AddBezier(array2[0], array2[1], array2[2], array2[3]);
						pointF = new PointF((float)bezierSegment.Point3.X, (float)bezierSegment.Point3.Y);
					}
					else if (segment is QuadraticBezierSegment)
					{
						QuadraticBezierSegment quadraticBezierSegment = segment as QuadraticBezierSegment;
						PointF[] array3 = new PointF[3]
						{
							pointF,
							new PointF((float)quadraticBezierSegment.Point1.X, (float)quadraticBezierSegment.Point1.Y),
							new PointF((float)quadraticBezierSegment.Point2.X, (float)quadraticBezierSegment.Point2.Y)
						};
						transformationMatrix.TransformPoints(array3);
						graphicsPath.AddBezier(array3[0], array3[1], array3[2], array3[2]);
						pointF = new PointF((float)quadraticBezierSegment.Point2.X, (float)quadraticBezierSegment.Point2.Y);
					}
				}
				if (outline.IsClosed)
				{
					graphicsPath.CloseFigure();
				}
			}
			return graphicsPath;
		}
	}

	public void ClearCache()
	{
		glyphsCache.Clear();
		shapesCache.Clear();
	}
}
