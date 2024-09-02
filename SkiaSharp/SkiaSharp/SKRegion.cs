using System;

namespace SkiaSharp;

public class SKRegion : SKObject, ISKSkipObjectRegistration
{
	public class RectIterator : SKObject, ISKSkipObjectRegistration
	{
		private readonly SKRegion region;

		internal RectIterator(SKRegion region)
			: base(SkiaApi.sk_region_iterator_new(region.Handle), owns: true)
		{
			this.region = region;
		}

		protected override void DisposeNative()
		{
			SkiaApi.sk_region_iterator_delete(Handle);
		}

		public unsafe bool Next(out SKRectI rect)
		{
			if (SkiaApi.sk_region_iterator_done(Handle))
			{
				rect = SKRectI.Empty;
				return false;
			}
			fixed (SKRectI* rect2 = &rect)
			{
				SkiaApi.sk_region_iterator_rect(Handle, rect2);
			}
			SkiaApi.sk_region_iterator_next(Handle);
			return true;
		}
	}

	public class ClipIterator : SKObject, ISKSkipObjectRegistration
	{
		private readonly SKRegion region;

		private readonly SKRectI clip;

		internal unsafe ClipIterator(SKRegion region, SKRectI clip)
			: base(SkiaApi.sk_region_cliperator_new(region.Handle, &clip), owns: true)
		{
			this.region = region;
			this.clip = clip;
		}

		protected override void DisposeNative()
		{
			SkiaApi.sk_region_cliperator_delete(Handle);
		}

		public unsafe bool Next(out SKRectI rect)
		{
			if (SkiaApi.sk_region_cliperator_done(Handle))
			{
				rect = SKRectI.Empty;
				return false;
			}
			fixed (SKRectI* rect2 = &rect)
			{
				SkiaApi.sk_region_iterator_rect(Handle, rect2);
			}
			SkiaApi.sk_region_cliperator_next(Handle);
			return true;
		}
	}

	public class SpanIterator : SKObject, ISKSkipObjectRegistration
	{
		internal SpanIterator(SKRegion region, int y, int left, int right)
			: base(SkiaApi.sk_region_spanerator_new(region.Handle, y, left, right), owns: true)
		{
		}

		protected override void DisposeNative()
		{
			SkiaApi.sk_region_spanerator_delete(Handle);
		}

		public unsafe bool Next(out int left, out int right)
		{
			int num = default(int);
			int num2 = default(int);
			if (SkiaApi.sk_region_spanerator_next(Handle, &num, &num2))
			{
				left = num;
				right = num2;
				return true;
			}
			left = 0;
			right = 0;
			return false;
		}
	}

	public bool IsEmpty => SkiaApi.sk_region_is_empty(Handle);

	public bool IsRect => SkiaApi.sk_region_is_rect(Handle);

	public bool IsComplex => SkiaApi.sk_region_is_complex(Handle);

	public unsafe SKRectI Bounds
	{
		get
		{
			SKRectI result = default(SKRectI);
			SkiaApi.sk_region_get_bounds(Handle, &result);
			return result;
		}
	}

	internal SKRegion(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKRegion()
		: this(SkiaApi.sk_region_new(), owns: true)
	{
	}

	public SKRegion(SKRegion region)
		: this()
	{
		SetRegion(region);
	}

	public SKRegion(SKRectI rect)
		: this()
	{
		SetRect(rect);
	}

	public SKRegion(SKPath path)
		: this()
	{
		SetPath(path);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_region_delete(Handle);
	}

	public SKPath GetBoundaryPath()
	{
		SKPath sKPath = new SKPath();
		if (!SkiaApi.sk_region_get_boundary_path(Handle, sKPath.Handle))
		{
			sKPath.Dispose();
			sKPath = null;
		}
		return sKPath;
	}

	public bool Contains(SKPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		using SKRegion src = new SKRegion(path);
		return Contains(src);
	}

	public bool Contains(SKRegion src)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return SkiaApi.sk_region_contains(Handle, src.Handle);
	}

	public bool Contains(SKPointI xy)
	{
		return SkiaApi.sk_region_contains_point(Handle, xy.X, xy.Y);
	}

	public bool Contains(int x, int y)
	{
		return SkiaApi.sk_region_contains_point(Handle, x, y);
	}

	public unsafe bool Contains(SKRectI rect)
	{
		return SkiaApi.sk_region_contains_rect(Handle, &rect);
	}

	public unsafe bool QuickContains(SKRectI rect)
	{
		return SkiaApi.sk_region_quick_contains(Handle, &rect);
	}

	public unsafe bool QuickReject(SKRectI rect)
	{
		return SkiaApi.sk_region_quick_reject_rect(Handle, &rect);
	}

	public bool QuickReject(SKRegion region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		return SkiaApi.sk_region_quick_reject(Handle, region.Handle);
	}

	public bool QuickReject(SKPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		using SKRegion region = new SKRegion(path);
		return QuickReject(region);
	}

	public bool Intersects(SKPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		using SKRegion region = new SKRegion(path);
		return Intersects(region);
	}

	public bool Intersects(SKRegion region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		return SkiaApi.sk_region_intersects(Handle, region.Handle);
	}

	public unsafe bool Intersects(SKRectI rect)
	{
		return SkiaApi.sk_region_intersects_rect(Handle, &rect);
	}

	public void SetEmpty()
	{
		SkiaApi.sk_region_set_empty(Handle);
	}

	public bool SetRegion(SKRegion region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		return SkiaApi.sk_region_set_region(Handle, region.Handle);
	}

	public unsafe bool SetRect(SKRectI rect)
	{
		return SkiaApi.sk_region_set_rect(Handle, &rect);
	}

	public unsafe bool SetRects(SKRectI[] rects)
	{
		if (rects == null)
		{
			throw new ArgumentNullException("rects");
		}
		fixed (SKRectI* rects2 = rects)
		{
			return SkiaApi.sk_region_set_rects(Handle, rects2, rects.Length);
		}
	}

	public bool SetPath(SKPath path, SKRegion clip)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (clip == null)
		{
			throw new ArgumentNullException("clip");
		}
		return SkiaApi.sk_region_set_path(Handle, path.Handle, clip.Handle);
	}

	public bool SetPath(SKPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		using SKRegion sKRegion = new SKRegion();
		SKRectI rect = SKRectI.Ceiling(path.Bounds);
		if (!rect.IsEmpty)
		{
			sKRegion.SetRect(rect);
		}
		return SkiaApi.sk_region_set_path(Handle, path.Handle, sKRegion.Handle);
	}

	public void Translate(int x, int y)
	{
		SkiaApi.sk_region_translate(Handle, x, y);
	}

	public unsafe bool Op(SKRectI rect, SKRegionOperation op)
	{
		return SkiaApi.sk_region_op_rect(Handle, &rect, op);
	}

	public bool Op(int left, int top, int right, int bottom, SKRegionOperation op)
	{
		return Op(new SKRectI(left, top, right, bottom), op);
	}

	public bool Op(SKRegion region, SKRegionOperation op)
	{
		return SkiaApi.sk_region_op(Handle, region.Handle, op);
	}

	public bool Op(SKPath path, SKRegionOperation op)
	{
		using SKRegion region = new SKRegion(path);
		return Op(region, op);
	}

	public RectIterator CreateRectIterator()
	{
		return new RectIterator(this);
	}

	public ClipIterator CreateClipIterator(SKRectI clip)
	{
		return new ClipIterator(this, clip);
	}

	public SpanIterator CreateSpanIterator(int y, int left, int right)
	{
		return new SpanIterator(this, y, left, right);
	}
}
