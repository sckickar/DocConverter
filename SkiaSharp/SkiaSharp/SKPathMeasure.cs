using System;

namespace SkiaSharp;

public class SKPathMeasure : SKObject, ISKSkipObjectRegistration
{
	public float Length => SkiaApi.sk_pathmeasure_get_length(Handle);

	public bool IsClosed => SkiaApi.sk_pathmeasure_is_closed(Handle);

	internal SKPathMeasure(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKPathMeasure()
		: this(SkiaApi.sk_pathmeasure_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKPathMeasure instance.");
		}
	}

	public SKPathMeasure(SKPath path, bool forceClosed = false, float resScale = 1f)
		: this(IntPtr.Zero, owns: true)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		Handle = SkiaApi.sk_pathmeasure_new_with_path(path.Handle, forceClosed, resScale);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKPathMeasure instance.");
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_pathmeasure_destroy(Handle);
	}

	public void SetPath(SKPath path)
	{
		SetPath(path, forceClosed: false);
	}

	public void SetPath(SKPath path, bool forceClosed)
	{
		SkiaApi.sk_pathmeasure_set_path(Handle, path?.Handle ?? IntPtr.Zero, forceClosed);
	}

	public unsafe bool GetPositionAndTangent(float distance, out SKPoint position, out SKPoint tangent)
	{
		fixed (SKPoint* position2 = &position)
		{
			fixed (SKPoint* tangent2 = &tangent)
			{
				return SkiaApi.sk_pathmeasure_get_pos_tan(Handle, distance, position2, tangent2);
			}
		}
	}

	public SKPoint GetPosition(float distance)
	{
		if (!GetPosition(distance, out var position))
		{
			return SKPoint.Empty;
		}
		return position;
	}

	public unsafe bool GetPosition(float distance, out SKPoint position)
	{
		fixed (SKPoint* position2 = &position)
		{
			return SkiaApi.sk_pathmeasure_get_pos_tan(Handle, distance, position2, null);
		}
	}

	public SKPoint GetTangent(float distance)
	{
		if (!GetTangent(distance, out var tangent))
		{
			return SKPoint.Empty;
		}
		return tangent;
	}

	public unsafe bool GetTangent(float distance, out SKPoint tangent)
	{
		fixed (SKPoint* tangent2 = &tangent)
		{
			return SkiaApi.sk_pathmeasure_get_pos_tan(Handle, distance, null, tangent2);
		}
	}

	public SKMatrix GetMatrix(float distance, SKPathMeasureMatrixFlags flags)
	{
		if (!GetMatrix(distance, out var matrix, flags))
		{
			return SKMatrix.Empty;
		}
		return matrix;
	}

	public unsafe bool GetMatrix(float distance, out SKMatrix matrix, SKPathMeasureMatrixFlags flags)
	{
		fixed (SKMatrix* matrix2 = &matrix)
		{
			return SkiaApi.sk_pathmeasure_get_matrix(Handle, distance, matrix2, flags);
		}
	}

	public bool GetSegment(float start, float stop, SKPath dst, bool startWithMoveTo)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_pathmeasure_get_segment(Handle, start, stop, dst.Handle, startWithMoveTo);
	}

	public SKPath GetSegment(float start, float stop, bool startWithMoveTo)
	{
		SKPath sKPath = new SKPath();
		if (!GetSegment(start, stop, sKPath, startWithMoveTo))
		{
			sKPath.Dispose();
			sKPath = null;
		}
		return sKPath;
	}

	public bool NextContour()
	{
		return SkiaApi.sk_pathmeasure_next_contour(Handle);
	}
}
