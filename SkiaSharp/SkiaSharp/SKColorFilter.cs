using System;

namespace SkiaSharp;

public class SKColorFilter : SKObject, ISKReferenceCounted
{
	public const int ColorMatrixSize = 20;

	public const int TableMaxLength = 256;

	internal SKColorFilter(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public static SKColorFilter CreateBlendMode(SKColor c, SKBlendMode mode)
	{
		return GetObject(SkiaApi.sk_colorfilter_new_mode((uint)c, mode));
	}

	public static SKColorFilter CreateLighting(SKColor mul, SKColor add)
	{
		return GetObject(SkiaApi.sk_colorfilter_new_lighting((uint)mul, (uint)add));
	}

	public static SKColorFilter CreateCompose(SKColorFilter outer, SKColorFilter inner)
	{
		if (outer == null)
		{
			throw new ArgumentNullException("outer");
		}
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		return GetObject(SkiaApi.sk_colorfilter_new_compose(outer.Handle, inner.Handle));
	}

	public unsafe static SKColorFilter CreateColorMatrix(float[] matrix)
	{
		if (matrix == null)
		{
			throw new ArgumentNullException("matrix");
		}
		if (matrix.Length != 20)
		{
			throw new ArgumentException("Matrix must have a length of 20.", "matrix");
		}
		fixed (float* array = matrix)
		{
			return GetObject(SkiaApi.sk_colorfilter_new_color_matrix(array));
		}
	}

	public static SKColorFilter CreateLumaColor()
	{
		return GetObject(SkiaApi.sk_colorfilter_new_luma_color());
	}

	public unsafe static SKColorFilter CreateTable(byte[] table)
	{
		if (table == null)
		{
			throw new ArgumentNullException("table");
		}
		if (table.Length != 256)
		{
			throw new ArgumentException($"Table must have a length of {256}.", "table");
		}
		fixed (byte* table2 = table)
		{
			return GetObject(SkiaApi.sk_colorfilter_new_table(table2));
		}
	}

	public unsafe static SKColorFilter CreateTable(byte[] tableA, byte[] tableR, byte[] tableG, byte[] tableB)
	{
		if (tableA != null && tableA.Length != 256)
		{
			throw new ArgumentException($"Table A must have a length of {256}.", "tableA");
		}
		if (tableR != null && tableR.Length != 256)
		{
			throw new ArgumentException($"Table R must have a length of {256}.", "tableR");
		}
		if (tableG != null && tableG.Length != 256)
		{
			throw new ArgumentException($"Table G must have a length of {256}.", "tableG");
		}
		if (tableB != null && tableB.Length != 256)
		{
			throw new ArgumentException($"Table B must have a length of {256}.", "tableB");
		}
		fixed (byte* tableA2 = tableA)
		{
			fixed (byte* tableR2 = tableR)
			{
				fixed (byte* tableG2 = tableG)
				{
					fixed (byte* tableB2 = tableB)
					{
						return GetObject(SkiaApi.sk_colorfilter_new_table_argb(tableA2, tableR2, tableG2, tableB2));
					}
				}
			}
		}
	}

	public unsafe static SKColorFilter CreateHighContrast(SKHighContrastConfig config)
	{
		return GetObject(SkiaApi.sk_colorfilter_new_high_contrast(&config));
	}

	public static SKColorFilter CreateHighContrast(bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast)
	{
		return CreateHighContrast(new SKHighContrastConfig(grayscale, invertStyle, contrast));
	}

	internal static SKColorFilter GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKColorFilter(h, o));
	}
}
