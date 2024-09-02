using System;
using System.ComponentModel;

namespace SkiaSharp;

public class SKMatrix44 : SKObject
{
	public unsafe SKMatrix Matrix
	{
		get
		{
			SKMatrix result = default(SKMatrix);
			SkiaApi.sk_matrix44_to_matrix(Handle, &result);
			return result;
		}
	}

	public SKMatrix44TypeMask Type => SkiaApi.sk_matrix44_get_type(Handle);

	public float this[int row, int column]
	{
		get
		{
			return SkiaApi.sk_matrix44_get(Handle, row, column);
		}
		set
		{
			SkiaApi.sk_matrix44_set(Handle, row, column, value);
		}
	}

	public bool IsInvertible => SkiaApi.sk_matrix44_invert(Handle, IntPtr.Zero);

	internal SKMatrix44(IntPtr x, bool owns)
		: base(x, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_matrix44_destroy(Handle);
	}

	public SKMatrix44()
		: this(SkiaApi.sk_matrix44_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKMatrix44 instance.");
		}
	}

	public SKMatrix44(SKMatrix44 src)
		: this(IntPtr.Zero, owns: true)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		Handle = SkiaApi.sk_matrix44_new_copy(src.Handle);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKMatrix44 instance.");
		}
	}

	public SKMatrix44(SKMatrix44 a, SKMatrix44 b)
		: this(IntPtr.Zero, owns: true)
	{
		if (a == null)
		{
			throw new ArgumentNullException("a");
		}
		if (b == null)
		{
			throw new ArgumentNullException("b");
		}
		Handle = SkiaApi.sk_matrix44_new_concat(a.Handle, b.Handle);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKMatrix44 instance.");
		}
	}

	public unsafe SKMatrix44(SKMatrix src)
		: this(SkiaApi.sk_matrix44_new_matrix(&src), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKMatrix44 instance.");
		}
	}

	public static SKMatrix44 CreateIdentity()
	{
		SKMatrix44 sKMatrix = new SKMatrix44();
		sKMatrix.SetIdentity();
		return sKMatrix;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static SKMatrix44 CreateTranslate(float x, float y, float z)
	{
		return CreateTranslation(x, y, z);
	}

	public static SKMatrix44 CreateTranslation(float x, float y, float z)
	{
		SKMatrix44 sKMatrix = new SKMatrix44();
		sKMatrix.SetTranslate(x, y, z);
		return sKMatrix;
	}

	public static SKMatrix44 CreateScale(float x, float y, float z)
	{
		SKMatrix44 sKMatrix = new SKMatrix44();
		sKMatrix.SetScale(x, y, z);
		return sKMatrix;
	}

	public static SKMatrix44 CreateRotation(float x, float y, float z, float radians)
	{
		SKMatrix44 sKMatrix = new SKMatrix44();
		sKMatrix.SetRotationAbout(x, y, z, radians);
		return sKMatrix;
	}

	public static SKMatrix44 CreateRotationDegrees(float x, float y, float z, float degrees)
	{
		SKMatrix44 sKMatrix = new SKMatrix44();
		sKMatrix.SetRotationAboutDegrees(x, y, z, degrees);
		return sKMatrix;
	}

	public static SKMatrix44 FromRowMajor(float[] src)
	{
		SKMatrix44 sKMatrix = new SKMatrix44();
		sKMatrix.SetRowMajor(src);
		return sKMatrix;
	}

	public static SKMatrix44 FromColumnMajor(float[] src)
	{
		SKMatrix44 sKMatrix = new SKMatrix44();
		sKMatrix.SetColumnMajor(src);
		return sKMatrix;
	}

	public float[] ToColumnMajor()
	{
		float[] array = new float[16];
		ToColumnMajor(array);
		return array;
	}

	public unsafe void ToColumnMajor(float[] dst)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		if (dst.Length != 16)
		{
			throw new ArgumentException("The destination array must be 16 entries.", "dst");
		}
		fixed (float* dst2 = dst)
		{
			SkiaApi.sk_matrix44_as_col_major(Handle, dst2);
		}
	}

	public float[] ToRowMajor()
	{
		float[] array = new float[16];
		ToRowMajor(array);
		return array;
	}

	public unsafe void ToRowMajor(float[] dst)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		if (dst.Length != 16)
		{
			throw new ArgumentException("The destination array must be 16 entries.", "dst");
		}
		fixed (float* dst2 = dst)
		{
			SkiaApi.sk_matrix44_as_row_major(Handle, dst2);
		}
	}

	public static bool Equal(SKMatrix44 left, SKMatrix44 right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		return SkiaApi.sk_matrix44_equals(left.Handle, right.Handle);
	}

	public void SetIdentity()
	{
		SkiaApi.sk_matrix44_set_identity(Handle);
	}

	public unsafe void SetColumnMajor(float[] src)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		if (src.Length != 16)
		{
			throw new ArgumentException("The source array must be 16 entries.", "src");
		}
		fixed (float* dst = src)
		{
			SkiaApi.sk_matrix44_set_col_major(Handle, dst);
		}
	}

	public unsafe void SetRowMajor(float[] src)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		if (src.Length != 16)
		{
			throw new ArgumentException("The source array must be 16 entries.", "src");
		}
		fixed (float* dst = src)
		{
			SkiaApi.sk_matrix44_set_row_major(Handle, dst);
		}
	}

	public unsafe void Set3x3ColumnMajor(float[] src)
	{
		if (src.Length != 9)
		{
			throw new ArgumentException("The source array must be 9 entries.", "src");
		}
		float* dst = stackalloc float[9]
		{
			src[0],
			src[3],
			src[6],
			src[1],
			src[4],
			src[7],
			src[2],
			src[5],
			src[8]
		};
		SkiaApi.sk_matrix44_set_3x3_row_major(Handle, dst);
	}

	public unsafe void Set3x3RowMajor(float[] src)
	{
		if (src.Length != 9)
		{
			throw new ArgumentException("The source array must be 9 entries.", "src");
		}
		fixed (float* dst = src)
		{
			SkiaApi.sk_matrix44_set_3x3_row_major(Handle, dst);
		}
	}

	public void SetTranslate(float dx, float dy, float dz)
	{
		SkiaApi.sk_matrix44_set_translate(Handle, dx, dy, dz);
	}

	public void SetScale(float sx, float sy, float sz)
	{
		SkiaApi.sk_matrix44_set_scale(Handle, sx, sy, sz);
	}

	public void SetRotationAboutDegrees(float x, float y, float z, float degrees)
	{
		SkiaApi.sk_matrix44_set_rotate_about_degrees(Handle, x, y, z, degrees);
	}

	public void SetRotationAbout(float x, float y, float z, float radians)
	{
		SkiaApi.sk_matrix44_set_rotate_about_radians(Handle, x, y, z, radians);
	}

	public void SetRotationAboutUnit(float x, float y, float z, float radians)
	{
		SkiaApi.sk_matrix44_set_rotate_about_radians_unit(Handle, x, y, z, radians);
	}

	public void SetConcat(SKMatrix44 a, SKMatrix44 b)
	{
		if (a == null)
		{
			throw new ArgumentNullException("a");
		}
		if (b == null)
		{
			throw new ArgumentNullException("b");
		}
		SkiaApi.sk_matrix44_set_concat(Handle, a.Handle, b.Handle);
	}

	public void PreTranslate(float dx, float dy, float dz)
	{
		SkiaApi.sk_matrix44_pre_translate(Handle, dx, dy, dz);
	}

	public void PostTranslate(float dx, float dy, float dz)
	{
		SkiaApi.sk_matrix44_post_translate(Handle, dx, dy, dz);
	}

	public void PreScale(float sx, float sy, float sz)
	{
		SkiaApi.sk_matrix44_pre_scale(Handle, sx, sy, sz);
	}

	public void PostScale(float sx, float sy, float sz)
	{
		SkiaApi.sk_matrix44_post_scale(Handle, sx, sy, sz);
	}

	public void PreConcat(SKMatrix44 m)
	{
		if (m == null)
		{
			throw new ArgumentNullException("m");
		}
		SkiaApi.sk_matrix44_pre_concat(Handle, m.Handle);
	}

	public void PostConcat(SKMatrix44 m)
	{
		if (m == null)
		{
			throw new ArgumentNullException("m");
		}
		SkiaApi.sk_matrix44_post_concat(Handle, m.Handle);
	}

	public SKMatrix44 Invert()
	{
		SKMatrix44 sKMatrix = new SKMatrix44();
		if (!Invert(sKMatrix))
		{
			sKMatrix.Dispose();
			sKMatrix = null;
		}
		return sKMatrix;
	}

	public bool Invert(SKMatrix44 inverse)
	{
		if (inverse == null)
		{
			throw new ArgumentNullException("inverse");
		}
		return SkiaApi.sk_matrix44_invert(Handle, inverse.Handle);
	}

	public void Transpose()
	{
		SkiaApi.sk_matrix44_transpose(Handle);
	}

	public float[] MapScalars(float x, float y, float z, float w)
	{
		float[] srcVector = new float[4] { x, y, z, w };
		float[] array = new float[4];
		MapScalars(srcVector, array);
		return array;
	}

	public float[] MapScalars(float[] srcVector4)
	{
		float[] array = new float[4];
		MapScalars(srcVector4, array);
		return array;
	}

	public unsafe void MapScalars(float[] srcVector4, float[] dstVector4)
	{
		if (srcVector4 == null)
		{
			throw new ArgumentNullException("srcVector4");
		}
		if (srcVector4.Length != 4)
		{
			throw new ArgumentException("The source vector array must be 4 entries.", "srcVector4");
		}
		if (dstVector4 == null)
		{
			throw new ArgumentNullException("dstVector4");
		}
		if (dstVector4.Length != 4)
		{
			throw new ArgumentException("The destination vector array must be 4 entries.", "dstVector4");
		}
		fixed (float* src = srcVector4)
		{
			fixed (float* dst = dstVector4)
			{
				SkiaApi.sk_matrix44_map_scalars(Handle, src, dst);
			}
		}
	}

	public SKPoint MapPoint(SKPoint src)
	{
		return MapPoints(new SKPoint[1] { src })[0];
	}

	public SKPoint[] MapPoints(SKPoint[] src)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		int num = src.Length;
		int num2 = num * 2;
		float[] array = new float[num2];
		int num3 = 0;
		int num4 = 0;
		while (num3 < num)
		{
			array[num4] = src[num3].X;
			array[num4 + 1] = src[num3].Y;
			num3++;
			num4 += 2;
		}
		float[] array2 = MapVector2(array);
		SKPoint[] array3 = new SKPoint[num];
		int num5 = 0;
		int num6 = 0;
		while (num5 < num)
		{
			array3[num5].X = array2[num6];
			array3[num5].Y = array2[num6 + 1];
			num5++;
			num6 += 4;
		}
		return array3;
	}

	public float[] MapVector2(float[] src2)
	{
		if (src2 == null)
		{
			throw new ArgumentNullException("src2");
		}
		if (src2.Length % 2 != 0)
		{
			throw new ArgumentException("The source vector array must be a set of pairs.", "src2");
		}
		float[] array = new float[src2.Length * 2];
		MapVector2(src2, array);
		return array;
	}

	public unsafe void MapVector2(float[] src2, float[] dst4)
	{
		if (src2 == null)
		{
			throw new ArgumentNullException("src2");
		}
		if (src2.Length % 2 != 0)
		{
			throw new ArgumentException("The source vector array must be a set of pairs.", "src2");
		}
		if (dst4 == null)
		{
			throw new ArgumentNullException("dst4");
		}
		if (dst4.Length % 4 != 0)
		{
			throw new ArgumentException("The destination vector array must be a set quads.", "dst4");
		}
		if (src2.Length / 2 != dst4.Length / 4)
		{
			throw new ArgumentException("The source vector array must have the same number of pairs as the destination vector array has quads.", "dst4");
		}
		fixed (float* src3 = src2)
		{
			fixed (float* dst5 = dst4)
			{
				SkiaApi.sk_matrix44_map2(Handle, src3, src2.Length / 2, dst5);
			}
		}
	}

	public bool Preserves2DAxisAlignment(float epsilon)
	{
		return SkiaApi.sk_matrix44_preserves_2d_axis_alignment(Handle, epsilon);
	}

	public double Determinant()
	{
		return SkiaApi.sk_matrix44_determinant(Handle);
	}

	public static implicit operator SKMatrix44(SKMatrix matrix)
	{
		return new SKMatrix44(matrix);
	}

	internal static SKMatrix44 GetObject(IntPtr handle, bool owns = true)
	{
		return SKObject.GetOrAddObject(handle, owns, (IntPtr h, bool o) => new SKMatrix44(h, o));
	}
}
