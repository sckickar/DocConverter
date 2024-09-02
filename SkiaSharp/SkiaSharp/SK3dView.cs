using System;

namespace SkiaSharp;

public class SK3dView : SKObject, ISKSkipObjectRegistration
{
	public SKMatrix Matrix
	{
		get
		{
			SKMatrix matrix = SKMatrix.Identity;
			GetMatrix(ref matrix);
			return matrix;
		}
	}

	internal SK3dView(IntPtr x, bool owns)
		: base(x, owns)
	{
	}

	public SK3dView()
		: this(SkiaApi.sk_3dview_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SK3dView instance.");
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_3dview_destroy(Handle);
	}

	public unsafe void GetMatrix(ref SKMatrix matrix)
	{
		fixed (SKMatrix* cmatrix = &matrix)
		{
			SkiaApi.sk_3dview_get_matrix(Handle, cmatrix);
		}
	}

	public void Save()
	{
		SkiaApi.sk_3dview_save(Handle);
	}

	public void Restore()
	{
		SkiaApi.sk_3dview_restore(Handle);
	}

	public void Translate(float x, float y, float z)
	{
		SkiaApi.sk_3dview_translate(Handle, x, y, z);
	}

	public void TranslateX(float x)
	{
		Translate(x, 0f, 0f);
	}

	public void TranslateY(float y)
	{
		Translate(0f, y, 0f);
	}

	public void TranslateZ(float z)
	{
		Translate(0f, 0f, z);
	}

	public void RotateXDegrees(float degrees)
	{
		SkiaApi.sk_3dview_rotate_x_degrees(Handle, degrees);
	}

	public void RotateYDegrees(float degrees)
	{
		SkiaApi.sk_3dview_rotate_y_degrees(Handle, degrees);
	}

	public void RotateZDegrees(float degrees)
	{
		SkiaApi.sk_3dview_rotate_z_degrees(Handle, degrees);
	}

	public void RotateXRadians(float radians)
	{
		SkiaApi.sk_3dview_rotate_x_radians(Handle, radians);
	}

	public void RotateYRadians(float radians)
	{
		SkiaApi.sk_3dview_rotate_y_radians(Handle, radians);
	}

	public void RotateZRadians(float radians)
	{
		SkiaApi.sk_3dview_rotate_z_radians(Handle, radians);
	}

	public float DotWithNormal(float dx, float dy, float dz)
	{
		return SkiaApi.sk_3dview_dot_with_normal(Handle, dx, dy, dz);
	}

	public void ApplyToCanvas(SKCanvas canvas)
	{
		if (canvas == null)
		{
			throw new ArgumentNullException("canvas");
		}
		SkiaApi.sk_3dview_apply_to_canvas(Handle, canvas.Handle);
	}
}
