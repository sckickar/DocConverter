using System;

namespace SkiaSharp;

public class SKRoundRect : SKObject, ISKSkipObjectRegistration
{
	public unsafe SKRect Rect
	{
		get
		{
			SKRect result = default(SKRect);
			SkiaApi.sk_rrect_get_rect(Handle, &result);
			return result;
		}
	}

	public SKPoint[] Radii => new SKPoint[4]
	{
		GetRadii(SKRoundRectCorner.UpperLeft),
		GetRadii(SKRoundRectCorner.UpperRight),
		GetRadii(SKRoundRectCorner.LowerRight),
		GetRadii(SKRoundRectCorner.LowerLeft)
	};

	public SKRoundRectType Type => SkiaApi.sk_rrect_get_type(Handle);

	public float Width => SkiaApi.sk_rrect_get_width(Handle);

	public float Height => SkiaApi.sk_rrect_get_height(Handle);

	public bool IsValid => SkiaApi.sk_rrect_is_valid(Handle);

	public bool AllCornersCircular => CheckAllCornersCircular(0.00024414062f);

	internal SKRoundRect(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKRoundRect()
		: this(SkiaApi.sk_rrect_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKRoundRect instance.");
		}
		SetEmpty();
	}

	public SKRoundRect(SKRect rect)
		: this(SkiaApi.sk_rrect_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKRoundRect instance.");
		}
		SetRect(rect);
	}

	public SKRoundRect(SKRect rect, float radius)
		: this(rect, radius, radius)
	{
	}

	public SKRoundRect(SKRect rect, float xRadius, float yRadius)
		: this(SkiaApi.sk_rrect_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKRoundRect instance.");
		}
		SetRect(rect, xRadius, yRadius);
	}

	public SKRoundRect(SKRoundRect rrect)
		: this(SkiaApi.sk_rrect_new_copy(rrect.Handle), owns: true)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_rrect_delete(Handle);
	}

	public bool CheckAllCornersCircular(float tolerance)
	{
		SKPoint radii = GetRadii(SKRoundRectCorner.UpperLeft);
		SKPoint radii2 = GetRadii(SKRoundRectCorner.UpperRight);
		SKPoint radii3 = GetRadii(SKRoundRectCorner.LowerRight);
		SKPoint radii4 = GetRadii(SKRoundRectCorner.LowerLeft);
		if (Utils.NearlyEqual(radii.X, radii.Y, tolerance) && Utils.NearlyEqual(radii2.X, radii2.Y, tolerance) && Utils.NearlyEqual(radii3.X, radii3.Y, tolerance))
		{
			return Utils.NearlyEqual(radii4.X, radii4.Y, tolerance);
		}
		return false;
	}

	public void SetEmpty()
	{
		SkiaApi.sk_rrect_set_empty(Handle);
	}

	public unsafe void SetRect(SKRect rect)
	{
		SkiaApi.sk_rrect_set_rect(Handle, &rect);
	}

	public unsafe void SetRect(SKRect rect, float xRadius, float yRadius)
	{
		SkiaApi.sk_rrect_set_rect_xy(Handle, &rect, xRadius, yRadius);
	}

	public unsafe void SetOval(SKRect rect)
	{
		SkiaApi.sk_rrect_set_oval(Handle, &rect);
	}

	public unsafe void SetNinePatch(SKRect rect, float leftRadius, float topRadius, float rightRadius, float bottomRadius)
	{
		SkiaApi.sk_rrect_set_nine_patch(Handle, &rect, leftRadius, topRadius, rightRadius, bottomRadius);
	}

	public unsafe void SetRectRadii(SKRect rect, SKPoint[] radii)
	{
		if (radii == null)
		{
			throw new ArgumentNullException("radii");
		}
		if (radii.Length != 4)
		{
			throw new ArgumentException("Radii must have a length of 4.", "radii");
		}
		fixed (SKPoint* radii2 = radii)
		{
			SkiaApi.sk_rrect_set_rect_radii(Handle, &rect, radii2);
		}
	}

	public unsafe bool Contains(SKRect rect)
	{
		return SkiaApi.sk_rrect_contains(Handle, &rect);
	}

	public unsafe SKPoint GetRadii(SKRoundRectCorner corner)
	{
		SKPoint result = default(SKPoint);
		SkiaApi.sk_rrect_get_radii(Handle, corner, &result);
		return result;
	}

	public void Deflate(SKSize size)
	{
		Deflate(size.Width, size.Height);
	}

	public void Deflate(float dx, float dy)
	{
		SkiaApi.sk_rrect_inset(Handle, dx, dy);
	}

	public void Inflate(SKSize size)
	{
		Inflate(size.Width, size.Height);
	}

	public void Inflate(float dx, float dy)
	{
		SkiaApi.sk_rrect_outset(Handle, dx, dy);
	}

	public void Offset(SKPoint pos)
	{
		Offset(pos.X, pos.Y);
	}

	public void Offset(float dx, float dy)
	{
		SkiaApi.sk_rrect_offset(Handle, dx, dy);
	}

	public unsafe bool TryTransform(SKMatrix matrix, out SKRoundRect transformed)
	{
		IntPtr intPtr = SkiaApi.sk_rrect_new();
		if (SkiaApi.sk_rrect_transform(Handle, &matrix, intPtr))
		{
			transformed = new SKRoundRect(intPtr, owns: true);
			return true;
		}
		SkiaApi.sk_rrect_delete(intPtr);
		transformed = null;
		return false;
	}

	public SKRoundRect Transform(SKMatrix matrix)
	{
		if (TryTransform(matrix, out var transformed))
		{
			return transformed;
		}
		return null;
	}
}
