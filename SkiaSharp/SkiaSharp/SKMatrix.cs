using System;
using System.ComponentModel;

namespace SkiaSharp;

public struct SKMatrix : IEquatable<SKMatrix>
{
	private class Indices
	{
		public const int ScaleX = 0;

		public const int SkewX = 1;

		public const int TransX = 2;

		public const int SkewY = 3;

		public const int ScaleY = 4;

		public const int TransY = 5;

		public const int Persp0 = 6;

		public const int Persp1 = 7;

		public const int Persp2 = 8;

		public const int Count = 9;
	}

	private float scaleX;

	private float skewX;

	private float transX;

	private float skewY;

	private float scaleY;

	private float transY;

	private float persp0;

	private float persp1;

	private float persp2;

	internal const float DegreesToRadians = (float)Math.PI / 180f;

	public static readonly SKMatrix Empty;

	public static readonly SKMatrix Identity = new SKMatrix
	{
		scaleX = 1f,
		scaleY = 1f,
		persp2 = 1f
	};

	public float ScaleX
	{
		readonly get
		{
			return scaleX;
		}
		set
		{
			scaleX = value;
		}
	}

	public float SkewX
	{
		readonly get
		{
			return skewX;
		}
		set
		{
			skewX = value;
		}
	}

	public float TransX
	{
		readonly get
		{
			return transX;
		}
		set
		{
			transX = value;
		}
	}

	public float SkewY
	{
		readonly get
		{
			return skewY;
		}
		set
		{
			skewY = value;
		}
	}

	public float ScaleY
	{
		readonly get
		{
			return scaleY;
		}
		set
		{
			scaleY = value;
		}
	}

	public float TransY
	{
		readonly get
		{
			return transY;
		}
		set
		{
			transY = value;
		}
	}

	public float Persp0
	{
		readonly get
		{
			return persp0;
		}
		set
		{
			persp0 = value;
		}
	}

	public float Persp1
	{
		readonly get
		{
			return persp1;
		}
		set
		{
			persp1 = value;
		}
	}

	public float Persp2
	{
		readonly get
		{
			return persp2;
		}
		set
		{
			persp2 = value;
		}
	}

	public readonly bool IsIdentity => Equals(Identity);

	public float[] Values
	{
		readonly get
		{
			return new float[9] { scaleX, skewX, transX, skewY, scaleY, transY, persp0, persp1, persp2 };
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Values");
			}
			if (value.Length != 9)
			{
				throw new ArgumentException($"The matrix array must have a length of {9}.", "Values");
			}
			scaleX = value[0];
			skewX = value[1];
			transX = value[2];
			skewY = value[3];
			scaleY = value[4];
			transY = value[5];
			persp0 = value[6];
			persp1 = value[7];
			persp2 = value[8];
		}
	}

	public unsafe readonly bool IsInvertible
	{
		get
		{
			fixed (SKMatrix* matrix = &this)
			{
				return SkiaApi.sk_matrix_try_invert(matrix, null);
			}
		}
	}

	public readonly bool Equals(SKMatrix obj)
	{
		if (scaleX == obj.scaleX && skewX == obj.skewX && transX == obj.transX && skewY == obj.skewY && scaleY == obj.scaleY && transY == obj.transY && persp0 == obj.persp0 && persp1 == obj.persp1)
		{
			return persp2 == obj.persp2;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKMatrix obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKMatrix left, SKMatrix right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKMatrix left, SKMatrix right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(scaleX);
		hashCode.Add(skewX);
		hashCode.Add(transX);
		hashCode.Add(skewY);
		hashCode.Add(scaleY);
		hashCode.Add(transY);
		hashCode.Add(persp0);
		hashCode.Add(persp1);
		hashCode.Add(persp2);
		return hashCode.ToHashCode();
	}

	public SKMatrix(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 9)
		{
			throw new ArgumentException($"The matrix array must have a length of {9}.", "values");
		}
		scaleX = values[0];
		skewX = values[1];
		transX = values[2];
		skewY = values[3];
		scaleY = values[4];
		transY = values[5];
		persp0 = values[6];
		persp1 = values[7];
		persp2 = values[8];
	}

	public SKMatrix(float scaleX, float skewX, float transX, float skewY, float scaleY, float transY, float persp0, float persp1, float persp2)
	{
		this.scaleX = scaleX;
		this.skewX = skewX;
		this.transX = transX;
		this.skewY = skewY;
		this.scaleY = scaleY;
		this.transY = transY;
		this.persp0 = persp0;
		this.persp1 = persp1;
		this.persp2 = persp2;
	}

	public readonly void GetValues(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 9)
		{
			throw new ArgumentException($"The matrix array must have a length of {9}.", "values");
		}
		values[0] = scaleX;
		values[1] = skewX;
		values[2] = transX;
		values[3] = skewY;
		values[4] = scaleY;
		values[5] = transY;
		values[6] = persp0;
		values[7] = persp1;
		values[8] = persp2;
	}

	public static SKMatrix CreateIdentity()
	{
		SKMatrix result = default(SKMatrix);
		result.scaleX = 1f;
		result.scaleY = 1f;
		result.persp2 = 1f;
		return result;
	}

	public static SKMatrix CreateTranslation(float x, float y)
	{
		if (x == 0f && y == 0f)
		{
			return Identity;
		}
		SKMatrix result = default(SKMatrix);
		result.scaleX = 1f;
		result.scaleY = 1f;
		result.transX = x;
		result.transY = y;
		result.persp2 = 1f;
		return result;
	}

	public static SKMatrix CreateScale(float x, float y)
	{
		if (x == 1f && y == 1f)
		{
			return Identity;
		}
		SKMatrix result = default(SKMatrix);
		result.scaleX = x;
		result.scaleY = y;
		result.persp2 = 1f;
		return result;
	}

	public static SKMatrix CreateScale(float x, float y, float pivotX, float pivotY)
	{
		if (x == 1f && y == 1f)
		{
			return Identity;
		}
		float num = pivotX - x * pivotX;
		float num2 = pivotY - y * pivotY;
		SKMatrix result = default(SKMatrix);
		result.scaleX = x;
		result.scaleY = y;
		result.transX = num;
		result.transY = num2;
		result.persp2 = 1f;
		return result;
	}

	public static SKMatrix CreateRotation(float radians)
	{
		if (radians == 0f)
		{
			return Identity;
		}
		float sin = (float)Math.Sin(radians);
		float cos = (float)Math.Cos(radians);
		SKMatrix matrix = Identity;
		SetSinCos(ref matrix, sin, cos);
		return matrix;
	}

	public static SKMatrix CreateRotation(float radians, float pivotX, float pivotY)
	{
		if (radians == 0f)
		{
			return Identity;
		}
		float sin = (float)Math.Sin(radians);
		float cos = (float)Math.Cos(radians);
		SKMatrix matrix = Identity;
		SetSinCos(ref matrix, sin, cos, pivotX, pivotY);
		return matrix;
	}

	public static SKMatrix CreateRotationDegrees(float degrees)
	{
		if (degrees == 0f)
		{
			return Identity;
		}
		return CreateRotation(degrees * ((float)Math.PI / 180f));
	}

	public static SKMatrix CreateRotationDegrees(float degrees, float pivotX, float pivotY)
	{
		if (degrees == 0f)
		{
			return Identity;
		}
		return CreateRotation(degrees * ((float)Math.PI / 180f), pivotX, pivotY);
	}

	public static SKMatrix CreateSkew(float x, float y)
	{
		if (x == 0f && y == 0f)
		{
			return Identity;
		}
		SKMatrix result = default(SKMatrix);
		result.scaleX = 1f;
		result.skewX = x;
		result.skewY = y;
		result.scaleY = 1f;
		result.persp2 = 1f;
		return result;
	}

	public static SKMatrix CreateScaleTranslation(float sx, float sy, float tx, float ty)
	{
		if (sx == 0f && sy == 0f && tx == 0f && ty == 0f)
		{
			return Identity;
		}
		SKMatrix result = default(SKMatrix);
		result.scaleX = sx;
		result.skewX = 0f;
		result.transX = tx;
		result.skewY = 0f;
		result.scaleY = sy;
		result.transY = ty;
		result.persp0 = 0f;
		result.persp1 = 0f;
		result.persp2 = 1f;
		return result;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateIdentity() instead.")]
	public static SKMatrix MakeIdentity()
	{
		return CreateIdentity();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateScale(float, float) instead.")]
	public static SKMatrix MakeScale(float sx, float sy)
	{
		return CreateScale(sx, sy);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateScale(float, float, float, float) instead.")]
	public static SKMatrix MakeScale(float sx, float sy, float pivotX, float pivotY)
	{
		return CreateScale(sx, sy, pivotX, pivotY);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateTranslation(float, float) instead.")]
	public static SKMatrix MakeTranslation(float dx, float dy)
	{
		return CreateTranslation(dx, dy);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRotation(float) instead.")]
	public static SKMatrix MakeRotation(float radians)
	{
		return CreateRotation(radians);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRotation(float, float, float) instead.")]
	public static SKMatrix MakeRotation(float radians, float pivotx, float pivoty)
	{
		return CreateRotation(radians, pivotx, pivoty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRotationDegrees(float) instead.")]
	public static SKMatrix MakeRotationDegrees(float degrees)
	{
		return CreateRotationDegrees(degrees);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRotationDegrees(float, float, float) instead.")]
	public static SKMatrix MakeRotationDegrees(float degrees, float pivotx, float pivoty)
	{
		return CreateRotationDegrees(degrees, pivotx, pivoty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateSkew(float, float) instead.")]
	public static SKMatrix MakeSkew(float sx, float sy)
	{
		return CreateSkew(sx, sy);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateScaleTranslation(float, float, float, float) instead.")]
	public void SetScaleTranslate(float sx, float sy, float tx, float ty)
	{
		scaleX = sx;
		skewX = 0f;
		transX = tx;
		skewY = 0f;
		scaleY = sy;
		transY = ty;
		persp0 = 0f;
		persp1 = 0f;
		persp2 = 1f;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRotation(float, float, float) instead.")]
	public static void Rotate(ref SKMatrix matrix, float radians, float pivotx, float pivoty)
	{
		float sin = (float)Math.Sin(radians);
		float cos = (float)Math.Cos(radians);
		SetSinCos(ref matrix, sin, cos, pivotx, pivoty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRotationDegrees(float, float, float) instead.")]
	public static void RotateDegrees(ref SKMatrix matrix, float degrees, float pivotx, float pivoty)
	{
		float sin = (float)Math.Sin(degrees * ((float)Math.PI / 180f));
		float cos = (float)Math.Cos(degrees * ((float)Math.PI / 180f));
		SetSinCos(ref matrix, sin, cos, pivotx, pivoty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRotation(float) instead.")]
	public static void Rotate(ref SKMatrix matrix, float radians)
	{
		float sin = (float)Math.Sin(radians);
		float cos = (float)Math.Cos(radians);
		SetSinCos(ref matrix, sin, cos);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRotationDegrees(float) instead.")]
	public static void RotateDegrees(ref SKMatrix matrix, float degrees)
	{
		float sin = (float)Math.Sin(degrees * ((float)Math.PI / 180f));
		float cos = (float)Math.Cos(degrees * ((float)Math.PI / 180f));
		SetSinCos(ref matrix, sin, cos);
	}

	public unsafe readonly bool TryInvert(out SKMatrix inverse)
	{
		fixed (SKMatrix* result = &inverse)
		{
			fixed (SKMatrix* matrix = &this)
			{
				return SkiaApi.sk_matrix_try_invert(matrix, result);
			}
		}
	}

	public readonly SKMatrix Invert()
	{
		if (TryInvert(out var inverse))
		{
			return inverse;
		}
		return Empty;
	}

	public unsafe static SKMatrix Concat(SKMatrix first, SKMatrix second)
	{
		SKMatrix result = default(SKMatrix);
		SkiaApi.sk_matrix_concat(&result, &first, &second);
		return result;
	}

	public unsafe readonly SKMatrix PreConcat(SKMatrix matrix)
	{
		SKMatrix result = this;
		SkiaApi.sk_matrix_pre_concat(&result, &matrix);
		return result;
	}

	public unsafe readonly SKMatrix PostConcat(SKMatrix matrix)
	{
		SKMatrix result = this;
		SkiaApi.sk_matrix_post_concat(&result, &matrix);
		return result;
	}

	public unsafe static void Concat(ref SKMatrix target, SKMatrix first, SKMatrix second)
	{
		fixed (SKMatrix* result = &target)
		{
			SkiaApi.sk_matrix_concat(result, &first, &second);
		}
	}

	public unsafe static void Concat(ref SKMatrix target, ref SKMatrix first, ref SKMatrix second)
	{
		fixed (SKMatrix* result = &target)
		{
			fixed (SKMatrix* first2 = &first)
			{
				fixed (SKMatrix* second2 = &second)
				{
					SkiaApi.sk_matrix_concat(result, first2, second2);
				}
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use PreConcat(SKMatrix) instead.")]
	public unsafe static void PreConcat(ref SKMatrix target, SKMatrix matrix)
	{
		fixed (SKMatrix* result = &target)
		{
			SkiaApi.sk_matrix_pre_concat(result, &matrix);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use PreConcat(SKMatrix) instead.")]
	public unsafe static void PreConcat(ref SKMatrix target, ref SKMatrix matrix)
	{
		fixed (SKMatrix* result = &target)
		{
			fixed (SKMatrix* matrix2 = &matrix)
			{
				SkiaApi.sk_matrix_pre_concat(result, matrix2);
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use PostConcat(SKMatrix) instead.")]
	public unsafe static void PostConcat(ref SKMatrix target, SKMatrix matrix)
	{
		fixed (SKMatrix* result = &target)
		{
			SkiaApi.sk_matrix_post_concat(result, &matrix);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use PostConcat(SKMatrix) instead.")]
	public unsafe static void PostConcat(ref SKMatrix target, ref SKMatrix matrix)
	{
		fixed (SKMatrix* result = &target)
		{
			fixed (SKMatrix* matrix2 = &matrix)
			{
				SkiaApi.sk_matrix_post_concat(result, matrix2);
			}
		}
	}

	public unsafe readonly SKRect MapRect(SKRect source)
	{
		SKRect result = default(SKRect);
		fixed (SKMatrix* matrix = &this)
		{
			SkiaApi.sk_matrix_map_rect(matrix, &result, &source);
		}
		return result;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use MapRect(SKRect) instead.")]
	public unsafe static void MapRect(ref SKMatrix matrix, out SKRect dest, ref SKRect source)
	{
		fixed (SKMatrix* matrix2 = &matrix)
		{
			fixed (SKRect* dest2 = &dest)
			{
				fixed (SKRect* source2 = &source)
				{
					SkiaApi.sk_matrix_map_rect(matrix2, dest2, source2);
				}
			}
		}
	}

	public readonly SKPoint MapPoint(SKPoint point)
	{
		return MapPoint(point.X, point.Y);
	}

	public unsafe readonly SKPoint MapPoint(float x, float y)
	{
		SKPoint result = default(SKPoint);
		fixed (SKMatrix* matrix = &this)
		{
			SkiaApi.sk_matrix_map_xy(matrix, x, y, &result);
		}
		return result;
	}

	public unsafe readonly void MapPoints(SKPoint[] result, SKPoint[] points)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		if (result.Length != points.Length)
		{
			throw new ArgumentException("Buffers must be the same size.");
		}
		fixed (SKMatrix* matrix = &this)
		{
			fixed (SKPoint* dst = result)
			{
				fixed (SKPoint* src = points)
				{
					SkiaApi.sk_matrix_map_points(matrix, dst, src, result.Length);
				}
			}
		}
	}

	public readonly SKPoint[] MapPoints(SKPoint[] points)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		SKPoint[] result = new SKPoint[points.Length];
		MapPoints(result, points);
		return result;
	}

	public readonly SKPoint MapVector(SKPoint vector)
	{
		return MapVector(vector.X, vector.Y);
	}

	public unsafe readonly SKPoint MapVector(float x, float y)
	{
		SKPoint result = default(SKPoint);
		fixed (SKMatrix* matrix = &this)
		{
			SkiaApi.sk_matrix_map_vector(matrix, x, y, &result);
		}
		return result;
	}

	public unsafe readonly void MapVectors(SKPoint[] result, SKPoint[] vectors)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		if (vectors == null)
		{
			throw new ArgumentNullException("vectors");
		}
		if (result.Length != vectors.Length)
		{
			throw new ArgumentException("Buffers must be the same size.");
		}
		fixed (SKMatrix* matrix = &this)
		{
			fixed (SKPoint* dst = result)
			{
				fixed (SKPoint* src = vectors)
				{
					SkiaApi.sk_matrix_map_vectors(matrix, dst, src, result.Length);
				}
			}
		}
	}

	public readonly SKPoint[] MapVectors(SKPoint[] vectors)
	{
		if (vectors == null)
		{
			throw new ArgumentNullException("vectors");
		}
		SKPoint[] result = new SKPoint[vectors.Length];
		MapVectors(result, vectors);
		return result;
	}

	public unsafe readonly float MapRadius(float radius)
	{
		fixed (SKMatrix* matrix = &this)
		{
			return SkiaApi.sk_matrix_map_radius(matrix, radius);
		}
	}

	private static void SetSinCos(ref SKMatrix matrix, float sin, float cos)
	{
		matrix.scaleX = cos;
		matrix.skewX = 0f - sin;
		matrix.transX = 0f;
		matrix.skewY = sin;
		matrix.scaleY = cos;
		matrix.transY = 0f;
		matrix.persp0 = 0f;
		matrix.persp1 = 0f;
		matrix.persp2 = 1f;
	}

	private static void SetSinCos(ref SKMatrix matrix, float sin, float cos, float pivotx, float pivoty)
	{
		float c = 1f - cos;
		matrix.scaleX = cos;
		matrix.skewX = 0f - sin;
		matrix.transX = Dot(sin, pivoty, c, pivotx);
		matrix.skewY = sin;
		matrix.scaleY = cos;
		matrix.transY = Dot(0f - sin, pivotx, c, pivoty);
		matrix.persp0 = 0f;
		matrix.persp1 = 0f;
		matrix.persp2 = 1f;
	}

	private static float Dot(float a, float b, float c, float d)
	{
		return a * b + c * d;
	}

	private static float Cross(float a, float b, float c, float d)
	{
		return a * b - c * d;
	}
}
