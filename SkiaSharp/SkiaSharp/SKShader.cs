using System;

namespace SkiaSharp;

public class SKShader : SKObject, ISKReferenceCounted
{
	internal SKShader(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public SKShader WithColorFilter(SKColorFilter filter)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		return GetObject(SkiaApi.sk_shader_with_color_filter(Handle, filter.Handle));
	}

	public unsafe SKShader WithLocalMatrix(SKMatrix localMatrix)
	{
		return GetObject(SkiaApi.sk_shader_with_local_matrix(Handle, &localMatrix));
	}

	public static SKShader CreateEmpty()
	{
		return GetObject(SkiaApi.sk_shader_new_empty());
	}

	public static SKShader CreateColor(SKColor color)
	{
		return GetObject(SkiaApi.sk_shader_new_color((uint)color));
	}

	public unsafe static SKShader CreateColor(SKColorF color, SKColorSpace colorspace)
	{
		if (colorspace == null)
		{
			throw new ArgumentNullException("colorspace");
		}
		return GetObject(SkiaApi.sk_shader_new_color4f(&color, colorspace.Handle));
	}

	public static SKShader CreateBitmap(SKBitmap src)
	{
		return CreateBitmap(src, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
	}

	public static SKShader CreateBitmap(SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.ToShader(tmx, tmy);
	}

	public static SKShader CreateBitmap(SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.ToShader(tmx, tmy, localMatrix);
	}

	public static SKShader CreateImage(SKImage src)
	{
		return CreateImage(src, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
	}

	public static SKShader CreateImage(SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.ToShader(tmx, tmy);
	}

	public static SKShader CreateImage(SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.ToShader(tmx, tmy, localMatrix);
	}

	public static SKShader CreatePicture(SKPicture src)
	{
		return CreatePicture(src, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
	}

	public static SKShader CreatePicture(SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.ToShader(tmx, tmy);
	}

	public static SKShader CreatePicture(SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKRect tile)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.ToShader(tmx, tmy, tile);
	}

	public static SKShader CreatePicture(SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix, SKRect tile)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.ToShader(tmx, tmy, localMatrix, tile);
	}

	public static SKShader CreateLinearGradient(SKPoint start, SKPoint end, SKColor[] colors, SKShaderTileMode mode)
	{
		return CreateLinearGradient(start, end, colors, null, mode);
	}

	public unsafe static SKShader CreateLinearGradient(SKPoint start, SKPoint end, SKColor[] colors, float[] colorPos, SKShaderTileMode mode)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		SKPoint* points = stackalloc SKPoint[2] { start, end };
		fixed (SKColor* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_linear_gradient(points, (uint*)colors2, colorPos2, colors.Length, mode, null));
			}
		}
	}

	public unsafe static SKShader CreateLinearGradient(SKPoint start, SKPoint end, SKColor[] colors, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		SKPoint* points = stackalloc SKPoint[2] { start, end };
		fixed (SKColor* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_linear_gradient(points, (uint*)colors2, colorPos2, colors.Length, mode, &localMatrix));
			}
		}
	}

	public static SKShader CreateLinearGradient(SKPoint start, SKPoint end, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode mode)
	{
		return CreateLinearGradient(start, end, colors, colorspace, null, mode);
	}

	public unsafe static SKShader CreateLinearGradient(SKPoint start, SKPoint end, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		SKPoint* points = stackalloc SKPoint[2] { start, end };
		fixed (SKColorF* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_linear_gradient_color4f(points, colors2, colorspace?.Handle ?? IntPtr.Zero, colorPos2, colors.Length, mode, null));
			}
		}
	}

	public unsafe static SKShader CreateLinearGradient(SKPoint start, SKPoint end, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		SKPoint* points = stackalloc SKPoint[2] { start, end };
		fixed (SKColorF* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_linear_gradient_color4f(points, colors2, colorspace?.Handle ?? IntPtr.Zero, colorPos2, colors.Length, mode, &localMatrix));
			}
		}
	}

	public static SKShader CreateRadialGradient(SKPoint center, float radius, SKColor[] colors, SKShaderTileMode mode)
	{
		return CreateRadialGradient(center, radius, colors, null, mode);
	}

	public unsafe static SKShader CreateRadialGradient(SKPoint center, float radius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		fixed (SKColor* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_radial_gradient(&center, radius, (uint*)colors2, colorPos2, colors.Length, mode, null));
			}
		}
	}

	public unsafe static SKShader CreateRadialGradient(SKPoint center, float radius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		fixed (SKColor* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_radial_gradient(&center, radius, (uint*)colors2, colorPos2, colors.Length, mode, &localMatrix));
			}
		}
	}

	public static SKShader CreateRadialGradient(SKPoint center, float radius, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode mode)
	{
		return CreateRadialGradient(center, radius, colors, colorspace, null, mode);
	}

	public unsafe static SKShader CreateRadialGradient(SKPoint center, float radius, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		fixed (SKColorF* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_radial_gradient_color4f(&center, radius, colors2, colorspace?.Handle ?? IntPtr.Zero, colorPos2, colors.Length, mode, null));
			}
		}
	}

	public unsafe static SKShader CreateRadialGradient(SKPoint center, float radius, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		fixed (SKColorF* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_radial_gradient_color4f(&center, radius, colors2, colorspace?.Handle ?? IntPtr.Zero, colorPos2, colors.Length, mode, &localMatrix));
			}
		}
	}

	public static SKShader CreateSweepGradient(SKPoint center, SKColor[] colors)
	{
		return CreateSweepGradient(center, colors, null, SKShaderTileMode.Clamp, 0f, 360f);
	}

	public static SKShader CreateSweepGradient(SKPoint center, SKColor[] colors, float[] colorPos)
	{
		return CreateSweepGradient(center, colors, colorPos, SKShaderTileMode.Clamp, 0f, 360f);
	}

	public static SKShader CreateSweepGradient(SKPoint center, SKColor[] colors, float[] colorPos, SKMatrix localMatrix)
	{
		return CreateSweepGradient(center, colors, colorPos, SKShaderTileMode.Clamp, 0f, 360f, localMatrix);
	}

	public static SKShader CreateSweepGradient(SKPoint center, SKColor[] colors, SKShaderTileMode tileMode, float startAngle, float endAngle)
	{
		return CreateSweepGradient(center, colors, null, tileMode, startAngle, endAngle);
	}

	public unsafe static SKShader CreateSweepGradient(SKPoint center, SKColor[] colors, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		fixed (SKColor* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_sweep_gradient(&center, (uint*)colors2, colorPos2, colors.Length, tileMode, startAngle, endAngle, null));
			}
		}
	}

	public unsafe static SKShader CreateSweepGradient(SKPoint center, SKColor[] colors, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix localMatrix)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		fixed (SKColor* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_sweep_gradient(&center, (uint*)colors2, colorPos2, colors.Length, tileMode, startAngle, endAngle, &localMatrix));
			}
		}
	}

	public static SKShader CreateSweepGradient(SKPoint center, SKColorF[] colors, SKColorSpace colorspace)
	{
		return CreateSweepGradient(center, colors, colorspace, null, SKShaderTileMode.Clamp, 0f, 360f);
	}

	public static SKShader CreateSweepGradient(SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos)
	{
		return CreateSweepGradient(center, colors, colorspace, colorPos, SKShaderTileMode.Clamp, 0f, 360f);
	}

	public static SKShader CreateSweepGradient(SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKMatrix localMatrix)
	{
		return CreateSweepGradient(center, colors, colorspace, colorPos, SKShaderTileMode.Clamp, 0f, 360f, localMatrix);
	}

	public static SKShader CreateSweepGradient(SKPoint center, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode tileMode, float startAngle, float endAngle)
	{
		return CreateSweepGradient(center, colors, colorspace, null, tileMode, startAngle, endAngle);
	}

	public unsafe static SKShader CreateSweepGradient(SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		fixed (SKColorF* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_sweep_gradient_color4f(&center, colors2, colorspace?.Handle ?? IntPtr.Zero, colorPos2, colors.Length, tileMode, startAngle, endAngle, null));
			}
		}
	}

	public unsafe static SKShader CreateSweepGradient(SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix localMatrix)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		fixed (SKColorF* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_sweep_gradient_color4f(&center, colors2, colorspace?.Handle ?? IntPtr.Zero, colorPos2, colors.Length, tileMode, startAngle, endAngle, &localMatrix));
			}
		}
	}

	public static SKShader CreateTwoPointConicalGradient(SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, SKShaderTileMode mode)
	{
		return CreateTwoPointConicalGradient(start, startRadius, end, endRadius, colors, null, mode);
	}

	public unsafe static SKShader CreateTwoPointConicalGradient(SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		fixed (SKColor* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_two_point_conical_gradient(&start, startRadius, &end, endRadius, (uint*)colors2, colorPos2, colors.Length, mode, null));
			}
		}
	}

	public unsafe static SKShader CreateTwoPointConicalGradient(SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		fixed (SKColor* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_two_point_conical_gradient(&start, startRadius, &end, endRadius, (uint*)colors2, colorPos2, colors.Length, mode, &localMatrix));
			}
		}
	}

	public static SKShader CreateTwoPointConicalGradient(SKPoint start, float startRadius, SKPoint end, float endRadius, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode mode)
	{
		return CreateTwoPointConicalGradient(start, startRadius, end, endRadius, colors, colorspace, null, mode);
	}

	public unsafe static SKShader CreateTwoPointConicalGradient(SKPoint start, float startRadius, SKPoint end, float endRadius, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		fixed (SKColorF* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_two_point_conical_gradient_color4f(&start, startRadius, &end, endRadius, colors2, colorspace?.Handle ?? IntPtr.Zero, colorPos2, colors.Length, mode, null));
			}
		}
	}

	public unsafe static SKShader CreateTwoPointConicalGradient(SKPoint start, float startRadius, SKPoint end, float endRadius, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		if (colorPos != null && colors.Length != colorPos.Length)
		{
			throw new ArgumentException("The number of colors must match the number of color positions.");
		}
		fixed (SKColorF* colors2 = colors)
		{
			fixed (float* colorPos2 = colorPos)
			{
				return GetObject(SkiaApi.sk_shader_new_two_point_conical_gradient_color4f(&start, startRadius, &end, endRadius, colors2, colorspace?.Handle ?? IntPtr.Zero, colorPos2, colors.Length, mode, &localMatrix));
			}
		}
	}

	public unsafe static SKShader CreatePerlinNoiseFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
	{
		return GetObject(SkiaApi.sk_shader_new_perlin_noise_fractal_noise(baseFrequencyX, baseFrequencyY, numOctaves, seed, null));
	}

	public static SKShader CreatePerlinNoiseFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize)
	{
		return CreatePerlinNoiseFractalNoise(baseFrequencyX, baseFrequencyY, numOctaves, seed, (SKSizeI)tileSize);
	}

	public unsafe static SKShader CreatePerlinNoiseFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI tileSize)
	{
		return GetObject(SkiaApi.sk_shader_new_perlin_noise_fractal_noise(baseFrequencyX, baseFrequencyY, numOctaves, seed, &tileSize));
	}

	public static SKShader CreatePerlinNoiseImprovedNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float z)
	{
		return GetObject(SkiaApi.sk_shader_new_perlin_noise_improved_noise(baseFrequencyX, baseFrequencyY, numOctaves, z));
	}

	public unsafe static SKShader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
	{
		return GetObject(SkiaApi.sk_shader_new_perlin_noise_turbulence(baseFrequencyX, baseFrequencyY, numOctaves, seed, null));
	}

	public static SKShader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize)
	{
		return CreatePerlinNoiseTurbulence(baseFrequencyX, baseFrequencyY, numOctaves, seed, (SKSizeI)tileSize);
	}

	public unsafe static SKShader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI tileSize)
	{
		return GetObject(SkiaApi.sk_shader_new_perlin_noise_turbulence(baseFrequencyX, baseFrequencyY, numOctaves, seed, &tileSize));
	}

	public static SKShader CreateCompose(SKShader shaderA, SKShader shaderB)
	{
		return CreateCompose(shaderA, shaderB, SKBlendMode.SrcOver);
	}

	public static SKShader CreateCompose(SKShader shaderA, SKShader shaderB, SKBlendMode mode)
	{
		if (shaderA == null)
		{
			throw new ArgumentNullException("shaderA");
		}
		if (shaderB == null)
		{
			throw new ArgumentNullException("shaderB");
		}
		return GetObject(SkiaApi.sk_shader_new_blend(mode, shaderA.Handle, shaderB.Handle));
	}

	public static SKShader CreateLerp(float weight, SKShader dst, SKShader src)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return GetObject(SkiaApi.sk_shader_new_lerp(weight, dst.Handle, src.Handle));
	}

	public static SKShader CreateColorFilter(SKShader shader, SKColorFilter filter)
	{
		if (shader == null)
		{
			throw new ArgumentNullException("shader");
		}
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		return shader.WithColorFilter(filter);
	}

	public static SKShader CreateLocalMatrix(SKShader shader, SKMatrix localMatrix)
	{
		if (shader == null)
		{
			throw new ArgumentNullException("shader");
		}
		return shader.WithLocalMatrix(localMatrix);
	}

	internal static SKShader GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKShader(h, o));
	}
}
