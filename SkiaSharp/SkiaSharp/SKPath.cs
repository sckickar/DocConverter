using System;
using System.ComponentModel;

namespace SkiaSharp;

public class SKPath : SKObject, ISKSkipObjectRegistration
{
	public class Iterator : SKObject, ISKSkipObjectRegistration
	{
		private readonly SKPath path;

		internal Iterator(SKPath path, bool forceClose)
			: base(SkiaApi.sk_path_create_iter(path.Handle, forceClose ? 1 : 0), owns: true)
		{
			this.path = path;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		protected override void DisposeNative()
		{
			SkiaApi.sk_path_iter_destroy(Handle);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use Next(SKPoint[]) instead.")]
		public SKPathVerb Next(SKPoint[] points, bool doConsumeDegenerates, bool exact)
		{
			return Next(points);
		}

		public SKPathVerb Next(SKPoint[] points)
		{
			return Next(new Span<SKPoint>(points));
		}

		public unsafe SKPathVerb Next(Span<SKPoint> points)
		{
			if (points == null)
			{
				throw new ArgumentNullException("points");
			}
			if (points.Length != 4)
			{
				throw new ArgumentException("Must be an array of four elements.", "points");
			}
			fixed (SKPoint* points2 = points)
			{
				return SkiaApi.sk_path_iter_next(Handle, points2);
			}
		}

		public float ConicWeight()
		{
			return SkiaApi.sk_path_iter_conic_weight(Handle);
		}

		public bool IsCloseLine()
		{
			return SkiaApi.sk_path_iter_is_close_line(Handle) != 0;
		}

		public bool IsCloseContour()
		{
			return SkiaApi.sk_path_iter_is_closed_contour(Handle) != 0;
		}
	}

	public class RawIterator : SKObject, ISKSkipObjectRegistration
	{
		private readonly SKPath path;

		internal RawIterator(SKPath path)
			: base(SkiaApi.sk_path_create_rawiter(path.Handle), owns: true)
		{
			this.path = path;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		protected override void DisposeNative()
		{
			SkiaApi.sk_path_rawiter_destroy(Handle);
		}

		public SKPathVerb Next(SKPoint[] points)
		{
			return Next(new Span<SKPoint>(points));
		}

		public unsafe SKPathVerb Next(Span<SKPoint> points)
		{
			if (points == null)
			{
				throw new ArgumentNullException("points");
			}
			if (points.Length != 4)
			{
				throw new ArgumentException("Must be an array of four elements.", "points");
			}
			fixed (SKPoint* points2 = points)
			{
				return SkiaApi.sk_path_rawiter_next(Handle, points2);
			}
		}

		public float ConicWeight()
		{
			return SkiaApi.sk_path_rawiter_conic_weight(Handle);
		}

		public SKPathVerb Peek()
		{
			return SkiaApi.sk_path_rawiter_peek(Handle);
		}
	}

	public class OpBuilder : SKObject, ISKSkipObjectRegistration
	{
		public OpBuilder()
			: base(SkiaApi.sk_opbuilder_new(), owns: true)
		{
		}

		public void Add(SKPath path, SKPathOp op)
		{
			SkiaApi.sk_opbuilder_add(Handle, path.Handle, op);
		}

		public bool Resolve(SKPath result)
		{
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			return SkiaApi.sk_opbuilder_resolve(Handle, result.Handle);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		protected override void DisposeNative()
		{
			SkiaApi.sk_opbuilder_destroy(Handle);
		}
	}

	public SKPathFillType FillType
	{
		get
		{
			return SkiaApi.sk_path_get_filltype(Handle);
		}
		set
		{
			SkiaApi.sk_path_set_filltype(Handle, value);
		}
	}

	public SKPathConvexity Convexity
	{
		get
		{
			if (!IsConvex)
			{
				return SKPathConvexity.Concave;
			}
			return SKPathConvexity.Convex;
		}
		[Obsolete]
		set
		{
		}
	}

	public bool IsConvex => SkiaApi.sk_path_is_convex(Handle);

	public bool IsConcave => !IsConvex;

	public bool IsEmpty => VerbCount == 0;

	public unsafe bool IsOval => SkiaApi.sk_path_is_oval(Handle, null);

	public bool IsRoundRect => SkiaApi.sk_path_is_rrect(Handle, IntPtr.Zero);

	public unsafe bool IsLine => SkiaApi.sk_path_is_line(Handle, null);

	public unsafe bool IsRect => SkiaApi.sk_path_is_rect(Handle, null, null, null);

	public SKPathSegmentMask SegmentMasks => (SKPathSegmentMask)SkiaApi.sk_path_get_segment_masks(Handle);

	public int VerbCount => SkiaApi.sk_path_count_verbs(Handle);

	public int PointCount => SkiaApi.sk_path_count_points(Handle);

	public SKPoint this[int index] => GetPoint(index);

	public SKPoint[] Points => GetPoints(PointCount);

	public unsafe SKPoint LastPoint
	{
		get
		{
			SKPoint result = default(SKPoint);
			SkiaApi.sk_path_get_last_point(Handle, &result);
			return result;
		}
	}

	public unsafe SKRect Bounds
	{
		get
		{
			SKRect result = default(SKRect);
			SkiaApi.sk_path_get_bounds(Handle, &result);
			return result;
		}
	}

	public SKRect TightBounds
	{
		get
		{
			if (GetTightBounds(out var result))
			{
				return result;
			}
			return SKRect.Empty;
		}
	}

	internal SKPath(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKPath()
		: this(SkiaApi.sk_path_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKPath instance.");
		}
	}

	public SKPath(SKPath path)
		: this(SkiaApi.sk_path_clone(path.Handle), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to copy the SKPath instance.");
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_path_delete(Handle);
	}

	public unsafe SKRect GetOvalBounds()
	{
		SKRect result = default(SKRect);
		if (SkiaApi.sk_path_is_oval(Handle, &result))
		{
			return result;
		}
		return SKRect.Empty;
	}

	public SKRoundRect GetRoundRect()
	{
		SKRoundRect sKRoundRect = new SKRoundRect();
		if (SkiaApi.sk_path_is_rrect(Handle, sKRoundRect.Handle))
		{
			return sKRoundRect;
		}
		sKRoundRect.Dispose();
		return null;
	}

	public unsafe SKPoint[] GetLine()
	{
		SKPoint[] array = new SKPoint[2];
		fixed (SKPoint* line = array)
		{
			if (SkiaApi.sk_path_is_line(Handle, line))
			{
				return array;
			}
			return null;
		}
	}

	public SKRect GetRect()
	{
		bool isClosed;
		SKPathDirection direction;
		return GetRect(out isClosed, out direction);
	}

	public unsafe SKRect GetRect(out bool isClosed, out SKPathDirection direction)
	{
		fixed (SKPathDirection* direction2 = &direction)
		{
			SKRect result = default(SKRect);
			byte b = default(byte);
			bool flag = SkiaApi.sk_path_is_rect(Handle, &result, &b, direction2);
			isClosed = b > 0;
			if (flag)
			{
				return result;
			}
			return SKRect.Empty;
		}
	}

	public unsafe SKPoint GetPoint(int index)
	{
		if (index < 0 || index >= PointCount)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		SKPoint result = default(SKPoint);
		SkiaApi.sk_path_get_point(Handle, index, &result);
		return result;
	}

	public SKPoint[] GetPoints(int max)
	{
		SKPoint[] array = new SKPoint[max];
		GetPoints(array, max);
		return array;
	}

	public unsafe int GetPoints(SKPoint[] points, int max)
	{
		fixed (SKPoint* points2 = points)
		{
			return SkiaApi.sk_path_get_points(Handle, points2, max);
		}
	}

	public bool Contains(float x, float y)
	{
		return SkiaApi.sk_path_contains(Handle, x, y);
	}

	public void Offset(SKPoint offset)
	{
		Offset(offset.X, offset.Y);
	}

	public void Offset(float dx, float dy)
	{
		Transform(SKMatrix.CreateTranslation(dx, dy));
	}

	public void MoveTo(SKPoint point)
	{
		SkiaApi.sk_path_move_to(Handle, point.X, point.Y);
	}

	public void MoveTo(float x, float y)
	{
		SkiaApi.sk_path_move_to(Handle, x, y);
	}

	public void RMoveTo(SKPoint point)
	{
		SkiaApi.sk_path_rmove_to(Handle, point.X, point.Y);
	}

	public void RMoveTo(float dx, float dy)
	{
		SkiaApi.sk_path_rmove_to(Handle, dx, dy);
	}

	public void LineTo(SKPoint point)
	{
		SkiaApi.sk_path_line_to(Handle, point.X, point.Y);
	}

	public void LineTo(float x, float y)
	{
		SkiaApi.sk_path_line_to(Handle, x, y);
	}

	public void RLineTo(SKPoint point)
	{
		SkiaApi.sk_path_rline_to(Handle, point.X, point.Y);
	}

	public void RLineTo(float dx, float dy)
	{
		SkiaApi.sk_path_rline_to(Handle, dx, dy);
	}

	public void QuadTo(SKPoint point0, SKPoint point1)
	{
		SkiaApi.sk_path_quad_to(Handle, point0.X, point0.Y, point1.X, point1.Y);
	}

	public void QuadTo(float x0, float y0, float x1, float y1)
	{
		SkiaApi.sk_path_quad_to(Handle, x0, y0, x1, y1);
	}

	public void RQuadTo(SKPoint point0, SKPoint point1)
	{
		SkiaApi.sk_path_rquad_to(Handle, point0.X, point0.Y, point1.X, point1.Y);
	}

	public void RQuadTo(float dx0, float dy0, float dx1, float dy1)
	{
		SkiaApi.sk_path_rquad_to(Handle, dx0, dy0, dx1, dy1);
	}

	public void ConicTo(SKPoint point0, SKPoint point1, float w)
	{
		SkiaApi.sk_path_conic_to(Handle, point0.X, point0.Y, point1.X, point1.Y, w);
	}

	public void ConicTo(float x0, float y0, float x1, float y1, float w)
	{
		SkiaApi.sk_path_conic_to(Handle, x0, y0, x1, y1, w);
	}

	public void RConicTo(SKPoint point0, SKPoint point1, float w)
	{
		SkiaApi.sk_path_rconic_to(Handle, point0.X, point0.Y, point1.X, point1.Y, w);
	}

	public void RConicTo(float dx0, float dy0, float dx1, float dy1, float w)
	{
		SkiaApi.sk_path_rconic_to(Handle, dx0, dy0, dx1, dy1, w);
	}

	public void CubicTo(SKPoint point0, SKPoint point1, SKPoint point2)
	{
		SkiaApi.sk_path_cubic_to(Handle, point0.X, point0.Y, point1.X, point1.Y, point2.X, point2.Y);
	}

	public void CubicTo(float x0, float y0, float x1, float y1, float x2, float y2)
	{
		SkiaApi.sk_path_cubic_to(Handle, x0, y0, x1, y1, x2, y2);
	}

	public void RCubicTo(SKPoint point0, SKPoint point1, SKPoint point2)
	{
		SkiaApi.sk_path_rcubic_to(Handle, point0.X, point0.Y, point1.X, point1.Y, point2.X, point2.Y);
	}

	public void RCubicTo(float dx0, float dy0, float dx1, float dy1, float dx2, float dy2)
	{
		SkiaApi.sk_path_rcubic_to(Handle, dx0, dy0, dx1, dy1, dx2, dy2);
	}

	public void ArcTo(SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy)
	{
		SkiaApi.sk_path_arc_to(Handle, r.X, r.Y, xAxisRotate, largeArc, sweep, xy.X, xy.Y);
	}

	public void ArcTo(float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
	{
		SkiaApi.sk_path_arc_to(Handle, rx, ry, xAxisRotate, largeArc, sweep, x, y);
	}

	public unsafe void ArcTo(SKRect oval, float startAngle, float sweepAngle, bool forceMoveTo)
	{
		SkiaApi.sk_path_arc_to_with_oval(Handle, &oval, startAngle, sweepAngle, forceMoveTo);
	}

	public void ArcTo(SKPoint point1, SKPoint point2, float radius)
	{
		SkiaApi.sk_path_arc_to_with_points(Handle, point1.X, point1.Y, point2.X, point2.Y, radius);
	}

	public void ArcTo(float x1, float y1, float x2, float y2, float radius)
	{
		SkiaApi.sk_path_arc_to_with_points(Handle, x1, y1, x2, y2, radius);
	}

	public void RArcTo(SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy)
	{
		SkiaApi.sk_path_rarc_to(Handle, r.X, r.Y, xAxisRotate, largeArc, sweep, xy.X, xy.Y);
	}

	public void RArcTo(float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
	{
		SkiaApi.sk_path_rarc_to(Handle, rx, ry, xAxisRotate, largeArc, sweep, x, y);
	}

	public void Close()
	{
		SkiaApi.sk_path_close(Handle);
	}

	public void Rewind()
	{
		SkiaApi.sk_path_rewind(Handle);
	}

	public void Reset()
	{
		SkiaApi.sk_path_reset(Handle);
	}

	public unsafe void AddRect(SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
	{
		SkiaApi.sk_path_add_rect(Handle, &rect, direction);
	}

	public unsafe void AddRect(SKRect rect, SKPathDirection direction, uint startIndex)
	{
		if (startIndex > 3)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Starting index must be in the range of 0..3 (inclusive).");
		}
		SkiaApi.sk_path_add_rect_start(Handle, &rect, direction, startIndex);
	}

	public void AddRoundRect(SKRoundRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
	{
		if (rect == null)
		{
			throw new ArgumentNullException("rect");
		}
		SkiaApi.sk_path_add_rrect(Handle, rect.Handle, direction);
	}

	public void AddRoundRect(SKRoundRect rect, SKPathDirection direction, uint startIndex)
	{
		if (rect == null)
		{
			throw new ArgumentNullException("rect");
		}
		SkiaApi.sk_path_add_rrect_start(Handle, rect.Handle, direction, startIndex);
	}

	public unsafe void AddOval(SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
	{
		SkiaApi.sk_path_add_oval(Handle, &rect, direction);
	}

	public unsafe void AddArc(SKRect oval, float startAngle, float sweepAngle)
	{
		SkiaApi.sk_path_add_arc(Handle, &oval, startAngle, sweepAngle);
	}

	public unsafe bool GetBounds(out SKRect rect)
	{
		bool isEmpty = IsEmpty;
		if (isEmpty)
		{
			rect = SKRect.Empty;
		}
		else
		{
			fixed (SKRect* param = &rect)
			{
				SkiaApi.sk_path_get_bounds(Handle, param);
			}
		}
		return !isEmpty;
	}

	public unsafe SKRect ComputeTightBounds()
	{
		SKRect result = default(SKRect);
		SkiaApi.sk_path_compute_tight_bounds(Handle, &result);
		return result;
	}

	public unsafe void Transform(SKMatrix matrix)
	{
		SkiaApi.sk_path_transform(Handle, &matrix);
	}

	public unsafe void Transform(SKMatrix matrix, SKPath destination)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		SkiaApi.sk_path_transform_to_dest(Handle, &matrix, destination.Handle);
	}

	public void AddPath(SKPath other, float dx, float dy, SKPathAddMode mode = SKPathAddMode.Append)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		SkiaApi.sk_path_add_path_offset(Handle, other.Handle, dx, dy, mode);
	}

	public unsafe void AddPath(SKPath other, ref SKMatrix matrix, SKPathAddMode mode = SKPathAddMode.Append)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		fixed (SKMatrix* matrix2 = &matrix)
		{
			SkiaApi.sk_path_add_path_matrix(Handle, other.Handle, matrix2, mode);
		}
	}

	public void AddPath(SKPath other, SKPathAddMode mode = SKPathAddMode.Append)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		SkiaApi.sk_path_add_path(Handle, other.Handle, mode);
	}

	public void AddPathReverse(SKPath other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		SkiaApi.sk_path_add_path_reverse(Handle, other.Handle);
	}

	public unsafe void AddRoundRect(SKRect rect, float rx, float ry, SKPathDirection dir = SKPathDirection.Clockwise)
	{
		SkiaApi.sk_path_add_rounded_rect(Handle, &rect, rx, ry, dir);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddRoundRect instead.")]
	public void AddRoundedRect(SKRect rect, float rx, float ry, SKPathDirection dir = SKPathDirection.Clockwise)
	{
		AddRoundRect(rect, rx, ry, dir);
	}

	public void AddCircle(float x, float y, float radius, SKPathDirection dir = SKPathDirection.Clockwise)
	{
		SkiaApi.sk_path_add_circle(Handle, x, y, radius, dir);
	}

	public unsafe void AddPoly(SKPoint[] points, bool close = true)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		fixed (SKPoint* points2 = points)
		{
			SkiaApi.sk_path_add_poly(Handle, points2, points.Length, close);
		}
	}

	public Iterator CreateIterator(bool forceClose)
	{
		return new Iterator(this, forceClose);
	}

	public RawIterator CreateRawIterator()
	{
		return new RawIterator(this);
	}

	public bool Op(SKPath other, SKPathOp op, SKPath result)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		return SkiaApi.sk_pathop_op(Handle, other.Handle, op, result.Handle);
	}

	public SKPath Op(SKPath other, SKPathOp op)
	{
		SKPath sKPath = new SKPath();
		if (Op(other, op, sKPath))
		{
			return sKPath;
		}
		sKPath.Dispose();
		return null;
	}

	public bool Simplify(SKPath result)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		return SkiaApi.sk_pathop_simplify(Handle, result.Handle);
	}

	public SKPath Simplify()
	{
		SKPath sKPath = new SKPath();
		if (Simplify(sKPath))
		{
			return sKPath;
		}
		sKPath.Dispose();
		return null;
	}

	public unsafe bool GetTightBounds(out SKRect result)
	{
		fixed (SKRect* result2 = &result)
		{
			return SkiaApi.sk_pathop_tight_bounds(Handle, result2);
		}
	}

	public bool ToWinding(SKPath result)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		return SkiaApi.sk_pathop_as_winding(Handle, result.Handle);
	}

	public SKPath ToWinding()
	{
		SKPath sKPath = new SKPath();
		if (ToWinding(sKPath))
		{
			return sKPath;
		}
		sKPath.Dispose();
		return null;
	}

	public string ToSvgPathData()
	{
		using SKString sKString = new SKString();
		SkiaApi.sk_path_to_svg_string(Handle, sKString.Handle);
		return (string)sKString;
	}

	public static SKPath ParseSvgPathData(string svgPath)
	{
		SKPath sKPath = new SKPath();
		if (!SkiaApi.sk_path_parse_svg_string(sKPath.Handle, svgPath))
		{
			sKPath.Dispose();
			sKPath = null;
		}
		return sKPath;
	}

	public static SKPoint[] ConvertConicToQuads(SKPoint p0, SKPoint p1, SKPoint p2, float w, int pow2)
	{
		ConvertConicToQuads(p0, p1, p2, w, out var pts, pow2);
		return pts;
	}

	public static int ConvertConicToQuads(SKPoint p0, SKPoint p1, SKPoint p2, float w, out SKPoint[] pts, int pow2)
	{
		int num = 1 << pow2;
		int num2 = 2 * num + 1;
		pts = new SKPoint[num2];
		return ConvertConicToQuads(p0, p1, p2, w, pts, pow2);
	}

	public unsafe static int ConvertConicToQuads(SKPoint p0, SKPoint p1, SKPoint p2, float w, SKPoint[] pts, int pow2)
	{
		if (pts == null)
		{
			throw new ArgumentNullException("pts");
		}
		fixed (SKPoint* pts2 = pts)
		{
			return SkiaApi.sk_path_convert_conic_to_quads(&p0, &p1, &p2, w, pts2, pow2);
		}
	}

	internal static SKPath GetObject(IntPtr handle, bool owns = true)
	{
		if (!(handle == IntPtr.Zero))
		{
			return new SKPath(handle, owns);
		}
		return null;
	}
}
