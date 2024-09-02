using System;

namespace SkiaSharp;

public class SKPathEffect : SKObject, ISKReferenceCounted
{
	internal SKPathEffect(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public static SKPathEffect CreateCompose(SKPathEffect outer, SKPathEffect inner)
	{
		if (outer == null)
		{
			throw new ArgumentNullException("outer");
		}
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		return GetObject(SkiaApi.sk_path_effect_create_compose(outer.Handle, inner.Handle));
	}

	public static SKPathEffect CreateSum(SKPathEffect first, SKPathEffect second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return GetObject(SkiaApi.sk_path_effect_create_sum(first.Handle, second.Handle));
	}

	public static SKPathEffect CreateDiscrete(float segLength, float deviation, uint seedAssist = 0u)
	{
		return GetObject(SkiaApi.sk_path_effect_create_discrete(segLength, deviation, seedAssist));
	}

	public static SKPathEffect CreateCorner(float radius)
	{
		return GetObject(SkiaApi.sk_path_effect_create_corner(radius));
	}

	public static SKPathEffect Create1DPath(SKPath path, float advance, float phase, SKPath1DPathEffectStyle style)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		return GetObject(SkiaApi.sk_path_effect_create_1d_path(path.Handle, advance, phase, style));
	}

	public unsafe static SKPathEffect Create2DLine(float width, SKMatrix matrix)
	{
		return GetObject(SkiaApi.sk_path_effect_create_2d_line(width, &matrix));
	}

	public unsafe static SKPathEffect Create2DPath(SKMatrix matrix, SKPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		return GetObject(SkiaApi.sk_path_effect_create_2d_path(&matrix, path.Handle));
	}

	public unsafe static SKPathEffect CreateDash(float[] intervals, float phase)
	{
		if (intervals == null)
		{
			throw new ArgumentNullException("intervals");
		}
		if (intervals.Length % 2 != 0)
		{
			throw new ArgumentException("The intervals must have an even number of entries.", "intervals");
		}
		fixed (float* intervals2 = intervals)
		{
			return GetObject(SkiaApi.sk_path_effect_create_dash(intervals2, intervals.Length, phase));
		}
	}

	public static SKPathEffect CreateTrim(float start, float stop)
	{
		return CreateTrim(start, stop, SKTrimPathEffectMode.Normal);
	}

	public static SKPathEffect CreateTrim(float start, float stop, SKTrimPathEffectMode mode)
	{
		return GetObject(SkiaApi.sk_path_effect_create_trim(start, stop, mode));
	}

	internal static SKPathEffect GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKPathEffect(h, o));
	}
}
