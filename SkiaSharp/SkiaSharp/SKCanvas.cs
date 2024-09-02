using System;
using System.ComponentModel;

namespace SkiaSharp;

public class SKCanvas : SKObject
{
	private const int PatchCornerCount = 4;

	private const int PatchCubicsCount = 12;

	private const double RadiansCircle = Math.PI * 2.0;

	private const double DegreesCircle = 360.0;

	public SKRect LocalClipBounds
	{
		get
		{
			GetLocalClipBounds(out var bounds);
			return bounds;
		}
	}

	public SKRectI DeviceClipBounds
	{
		get
		{
			GetDeviceClipBounds(out var bounds);
			return bounds;
		}
	}

	public bool IsClipEmpty => SkiaApi.sk_canvas_is_clip_empty(Handle);

	public bool IsClipRect => SkiaApi.sk_canvas_is_clip_rect(Handle);

	public unsafe SKMatrix TotalMatrix
	{
		get
		{
			SKMatrix result = default(SKMatrix);
			SkiaApi.sk_canvas_get_total_matrix(Handle, &result);
			return result;
		}
	}

	public int SaveCount => SkiaApi.sk_canvas_get_save_count(Handle);

	internal SKCanvas(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKCanvas(SKBitmap bitmap)
		: this(IntPtr.Zero, owns: true)
	{
		if (bitmap == null)
		{
			throw new ArgumentNullException("bitmap");
		}
		Handle = SkiaApi.sk_canvas_new_from_bitmap(bitmap.Handle);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_canvas_destroy(Handle);
	}

	public void Discard()
	{
		SkiaApi.sk_canvas_discard(Handle);
	}

	public unsafe bool QuickReject(SKRect rect)
	{
		return SkiaApi.sk_canvas_quick_reject(Handle, &rect);
	}

	public bool QuickReject(SKPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (!path.IsEmpty)
		{
			return QuickReject(path.Bounds);
		}
		return true;
	}

	public int Save()
	{
		if (Handle == IntPtr.Zero)
		{
			throw new ObjectDisposedException("SKCanvas");
		}
		return SkiaApi.sk_canvas_save(Handle);
	}

	public unsafe int SaveLayer(SKRect limit, SKPaint paint)
	{
		return SkiaApi.sk_canvas_save_layer(Handle, &limit, paint?.Handle ?? IntPtr.Zero);
	}

	public unsafe int SaveLayer(SKPaint paint)
	{
		return SkiaApi.sk_canvas_save_layer(Handle, null, paint?.Handle ?? IntPtr.Zero);
	}

	public int SaveLayer()
	{
		return SaveLayer(null);
	}

	public void DrawColor(SKColor color, SKBlendMode mode = SKBlendMode.Src)
	{
		SkiaApi.sk_canvas_draw_color(Handle, (uint)color, mode);
	}

	public void DrawColor(SKColorF color, SKBlendMode mode = SKBlendMode.Src)
	{
		SkiaApi.sk_canvas_draw_color4f(Handle, color, mode);
	}

	public void DrawLine(SKPoint p0, SKPoint p1, SKPaint paint)
	{
		DrawLine(p0.X, p0.Y, p1.X, p1.Y, paint);
	}

	public void DrawLine(float x0, float y0, float x1, float y1, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_line(Handle, x0, y0, x1, y1, paint.Handle);
	}

	public void Clear()
	{
		Clear(SKColors.Empty);
	}

	public void Clear(SKColor color)
	{
		SkiaApi.sk_canvas_clear(Handle, (uint)color);
	}

	public void Clear(SKColorF color)
	{
		SkiaApi.sk_canvas_clear_color4f(Handle, color);
	}

	public void Restore()
	{
		SkiaApi.sk_canvas_restore(Handle);
	}

	public void RestoreToCount(int count)
	{
		SkiaApi.sk_canvas_restore_to_count(Handle, count);
	}

	public void Translate(float dx, float dy)
	{
		if (dx != 0f || dy != 0f)
		{
			SkiaApi.sk_canvas_translate(Handle, dx, dy);
		}
	}

	public void Translate(SKPoint point)
	{
		if (!point.IsEmpty)
		{
			SkiaApi.sk_canvas_translate(Handle, point.X, point.Y);
		}
	}

	public void Scale(float s)
	{
		if (s != 1f)
		{
			SkiaApi.sk_canvas_scale(Handle, s, s);
		}
	}

	public void Scale(float sx, float sy)
	{
		if (sx != 1f || sy != 1f)
		{
			SkiaApi.sk_canvas_scale(Handle, sx, sy);
		}
	}

	public void Scale(SKPoint size)
	{
		if (!size.IsEmpty)
		{
			SkiaApi.sk_canvas_scale(Handle, size.X, size.Y);
		}
	}

	public void Scale(float sx, float sy, float px, float py)
	{
		if (sx != 1f || sy != 1f)
		{
			Translate(px, py);
			Scale(sx, sy);
			Translate(0f - px, 0f - py);
		}
	}

	public void RotateDegrees(float degrees)
	{
		if ((double)degrees % 360.0 != 0.0)
		{
			SkiaApi.sk_canvas_rotate_degrees(Handle, degrees);
		}
	}

	public void RotateRadians(float radians)
	{
		if ((double)radians % (Math.PI * 2.0) != 0.0)
		{
			SkiaApi.sk_canvas_rotate_radians(Handle, radians);
		}
	}

	public void RotateDegrees(float degrees, float px, float py)
	{
		if ((double)degrees % 360.0 != 0.0)
		{
			Translate(px, py);
			RotateDegrees(degrees);
			Translate(0f - px, 0f - py);
		}
	}

	public void RotateRadians(float radians, float px, float py)
	{
		if ((double)radians % (Math.PI * 2.0) != 0.0)
		{
			Translate(px, py);
			RotateRadians(radians);
			Translate(0f - px, 0f - py);
		}
	}

	public void Skew(float sx, float sy)
	{
		if (sx != 0f || sy != 0f)
		{
			SkiaApi.sk_canvas_skew(Handle, sx, sy);
		}
	}

	public void Skew(SKPoint skew)
	{
		if (!skew.IsEmpty)
		{
			SkiaApi.sk_canvas_skew(Handle, skew.X, skew.Y);
		}
	}

	public unsafe void Concat(ref SKMatrix m)
	{
		fixed (SKMatrix* param = &m)
		{
			SkiaApi.sk_canvas_concat(Handle, param);
		}
	}

	public unsafe void ClipRect(SKRect rect, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
	{
		SkiaApi.sk_canvas_clip_rect_with_operation(Handle, &rect, operation, antialias);
	}

	public void ClipRoundRect(SKRoundRect rect, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
	{
		if (rect == null)
		{
			throw new ArgumentNullException("rect");
		}
		SkiaApi.sk_canvas_clip_rrect_with_operation(Handle, rect.Handle, operation, antialias);
	}

	public void ClipPath(SKPath path, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		SkiaApi.sk_canvas_clip_path_with_operation(Handle, path.Handle, operation, antialias);
	}

	public void ClipRegion(SKRegion region, SKClipOperation operation = SKClipOperation.Intersect)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		SkiaApi.sk_canvas_clip_region(Handle, region.Handle, operation);
	}

	public unsafe bool GetLocalClipBounds(out SKRect bounds)
	{
		fixed (SKRect* cbounds = &bounds)
		{
			return SkiaApi.sk_canvas_get_local_clip_bounds(Handle, cbounds);
		}
	}

	public unsafe bool GetDeviceClipBounds(out SKRectI bounds)
	{
		fixed (SKRectI* cbounds = &bounds)
		{
			return SkiaApi.sk_canvas_get_device_clip_bounds(Handle, cbounds);
		}
	}

	public void DrawPaint(SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_paint(Handle, paint.Handle);
	}

	public void DrawRegion(SKRegion region, SKPaint paint)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_region(Handle, region.Handle, paint.Handle);
	}

	public void DrawRect(float x, float y, float w, float h, SKPaint paint)
	{
		DrawRect(SKRect.Create(x, y, w, h), paint);
	}

	public unsafe void DrawRect(SKRect rect, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
	}

	public void DrawRoundRect(SKRoundRect rect, SKPaint paint)
	{
		if (rect == null)
		{
			throw new ArgumentNullException("rect");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_rrect(Handle, rect.Handle, paint.Handle);
	}

	public void DrawRoundRect(float x, float y, float w, float h, float rx, float ry, SKPaint paint)
	{
		DrawRoundRect(SKRect.Create(x, y, w, h), rx, ry, paint);
	}

	public unsafe void DrawRoundRect(SKRect rect, float rx, float ry, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_round_rect(Handle, &rect, rx, ry, paint.Handle);
	}

	public void DrawRoundRect(SKRect rect, SKSize r, SKPaint paint)
	{
		DrawRoundRect(rect, r.Width, r.Height, paint);
	}

	public void DrawOval(float cx, float cy, float rx, float ry, SKPaint paint)
	{
		DrawOval(new SKRect(cx - rx, cy - ry, cx + rx, cy + ry), paint);
	}

	public void DrawOval(SKPoint c, SKSize r, SKPaint paint)
	{
		DrawOval(c.X, c.Y, r.Width, r.Height, paint);
	}

	public unsafe void DrawOval(SKRect rect, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_oval(Handle, &rect, paint.Handle);
	}

	public void DrawCircle(float cx, float cy, float radius, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_circle(Handle, cx, cy, radius, paint.Handle);
	}

	public void DrawCircle(SKPoint c, float radius, SKPaint paint)
	{
		DrawCircle(c.X, c.Y, radius, paint);
	}

	public void DrawPath(SKPath path, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		SkiaApi.sk_canvas_draw_path(Handle, path.Handle, paint.Handle);
	}

	public unsafe void DrawPoints(SKPointMode mode, SKPoint[] points, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		fixed (SKPoint* param = points)
		{
			SkiaApi.sk_canvas_draw_points(Handle, mode, (IntPtr)points.Length, param, paint.Handle);
		}
	}

	public void DrawPoint(SKPoint p, SKPaint paint)
	{
		DrawPoint(p.X, p.Y, paint);
	}

	public void DrawPoint(float x, float y, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_point(Handle, x, y, paint.Handle);
	}

	public void DrawPoint(SKPoint p, SKColor color)
	{
		DrawPoint(p.X, p.Y, color);
	}

	public void DrawPoint(float x, float y, SKColor color)
	{
		using SKPaint paint = new SKPaint
		{
			Color = color,
			BlendMode = SKBlendMode.Src
		};
		DrawPoint(x, y, paint);
	}

	public void DrawImage(SKImage image, SKPoint p, SKPaint paint = null)
	{
		DrawImage(image, p.X, p.Y, paint);
	}

	public void DrawImage(SKImage image, float x, float y, SKPaint paint = null)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		SkiaApi.sk_canvas_draw_image(Handle, image.Handle, x, y, paint?.Handle ?? IntPtr.Zero);
	}

	public unsafe void DrawImage(SKImage image, SKRect dest, SKPaint paint = null)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		SkiaApi.sk_canvas_draw_image_rect(Handle, image.Handle, null, &dest, paint?.Handle ?? IntPtr.Zero);
	}

	public unsafe void DrawImage(SKImage image, SKRect source, SKRect dest, SKPaint paint = null)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		SkiaApi.sk_canvas_draw_image_rect(Handle, image.Handle, &source, &dest, paint?.Handle ?? IntPtr.Zero);
	}

	public void DrawPicture(SKPicture picture, float x, float y, SKPaint paint = null)
	{
		SKMatrix matrix = SKMatrix.CreateTranslation(x, y);
		DrawPicture(picture, ref matrix, paint);
	}

	public void DrawPicture(SKPicture picture, SKPoint p, SKPaint paint = null)
	{
		DrawPicture(picture, p.X, p.Y, paint);
	}

	public unsafe void DrawPicture(SKPicture picture, ref SKMatrix matrix, SKPaint paint = null)
	{
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		fixed (SKMatrix* param = &matrix)
		{
			SkiaApi.sk_canvas_draw_picture(Handle, picture.Handle, param, paint?.Handle ?? IntPtr.Zero);
		}
	}

	public unsafe void DrawPicture(SKPicture picture, SKPaint paint = null)
	{
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		SkiaApi.sk_canvas_draw_picture(Handle, picture.Handle, null, paint?.Handle ?? IntPtr.Zero);
	}

	public unsafe void DrawDrawable(SKDrawable drawable, ref SKMatrix matrix)
	{
		if (drawable == null)
		{
			throw new ArgumentNullException("drawable");
		}
		fixed (SKMatrix* param = &matrix)
		{
			SkiaApi.sk_canvas_draw_drawable(Handle, drawable.Handle, param);
		}
	}

	public void DrawDrawable(SKDrawable drawable, float x, float y)
	{
		if (drawable == null)
		{
			throw new ArgumentNullException("drawable");
		}
		SKMatrix matrix = SKMatrix.CreateTranslation(x, y);
		DrawDrawable(drawable, ref matrix);
	}

	public void DrawDrawable(SKDrawable drawable, SKPoint p)
	{
		if (drawable == null)
		{
			throw new ArgumentNullException("drawable");
		}
		SKMatrix matrix = SKMatrix.CreateTranslation(p.X, p.Y);
		DrawDrawable(drawable, ref matrix);
	}

	public void DrawBitmap(SKBitmap bitmap, SKPoint p, SKPaint paint = null)
	{
		DrawBitmap(bitmap, p.X, p.Y, paint);
	}

	public void DrawBitmap(SKBitmap bitmap, float x, float y, SKPaint paint = null)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		DrawImage(image, x, y, paint);
	}

	public void DrawBitmap(SKBitmap bitmap, SKRect dest, SKPaint paint = null)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		DrawImage(image, dest, paint);
	}

	public void DrawBitmap(SKBitmap bitmap, SKRect source, SKRect dest, SKPaint paint = null)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		DrawImage(image, source, dest, paint);
	}

	public void DrawSurface(SKSurface surface, SKPoint p, SKPaint paint = null)
	{
		DrawSurface(surface, p.X, p.Y, paint);
	}

	public void DrawSurface(SKSurface surface, float x, float y, SKPaint paint = null)
	{
		if (surface == null)
		{
			throw new ArgumentNullException("surface");
		}
		surface.Draw(this, x, y, paint);
	}

	public void DrawText(SKTextBlob text, float x, float y, SKPaint paint)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_text_blob(Handle, text.Handle, x, y, paint.Handle);
	}

	public void DrawText(string text, SKPoint p, SKPaint paint)
	{
		DrawText(text, p.X, p.Y, paint);
	}

	public void DrawText(string text, float x, float y, SKPaint paint)
	{
		DrawText(text, x, y, paint.GetFont(), paint);
	}

	public void DrawText(string text, float x, float y, SKFont font, SKPaint paint)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (paint.TextAlign != 0)
		{
			float num = font.MeasureText(text);
			if (paint.TextAlign == SKTextAlign.Center)
			{
				num *= 0.5f;
			}
			x -= num;
		}
		using SKTextBlob sKTextBlob = SKTextBlob.Create(text, font);
		if (sKTextBlob != null)
		{
			DrawText(sKTextBlob, x, y, paint);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
	public void DrawText(byte[] text, SKPoint p, SKPaint paint)
	{
		DrawText(text, p.X, p.Y, paint);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
	public void DrawText(byte[] text, float x, float y, SKPaint paint)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (paint.TextAlign != 0)
		{
			float num = paint.MeasureText(text);
			if (paint.TextAlign == SKTextAlign.Center)
			{
				num *= 0.5f;
			}
			x -= num;
		}
		using SKTextBlob sKTextBlob = SKTextBlob.Create(text, paint.TextEncoding, paint.GetFont());
		if (sKTextBlob != null)
		{
			DrawText(sKTextBlob, x, y, paint);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
	public void DrawText(IntPtr buffer, int length, SKPoint p, SKPaint paint)
	{
		DrawText(buffer, length, p.X, p.Y, paint);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
	public void DrawText(IntPtr buffer, int length, float x, float y, SKPaint paint)
	{
		if (buffer == IntPtr.Zero && length != 0)
		{
			throw new ArgumentNullException("buffer");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (paint.TextAlign != 0)
		{
			float num = paint.MeasureText(buffer, length);
			if (paint.TextAlign == SKTextAlign.Center)
			{
				num *= 0.5f;
			}
			x -= num;
		}
		using SKTextBlob sKTextBlob = SKTextBlob.Create(buffer, length, paint.TextEncoding, paint.GetFont());
		if (sKTextBlob != null)
		{
			DrawText(sKTextBlob, x, y, paint);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
	public void DrawPositionedText(string text, SKPoint[] points, SKPaint paint)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		using SKTextBlob sKTextBlob = SKTextBlob.CreatePositioned(text, paint.GetFont(), points);
		if (sKTextBlob != null)
		{
			DrawText(sKTextBlob, 0f, 0f, paint);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
	public void DrawPositionedText(byte[] text, SKPoint[] points, SKPaint paint)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		using SKTextBlob sKTextBlob = SKTextBlob.CreatePositioned(text, paint.TextEncoding, paint.GetFont(), points);
		if (sKTextBlob != null)
		{
			DrawText(sKTextBlob, 0f, 0f, paint);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
	public void DrawPositionedText(IntPtr buffer, int length, SKPoint[] points, SKPaint paint)
	{
		if (buffer == IntPtr.Zero && length != 0)
		{
			throw new ArgumentNullException("buffer");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		using SKTextBlob sKTextBlob = SKTextBlob.CreatePositioned(buffer, length, paint.TextEncoding, paint.GetFont(), points);
		if (sKTextBlob != null)
		{
			DrawText(sKTextBlob, 0f, 0f, paint);
		}
	}

	public void DrawTextOnPath(string text, SKPath path, SKPoint offset, SKPaint paint)
	{
		DrawTextOnPath(text, path, offset, warpGlyphs: true, paint);
	}

	public void DrawTextOnPath(string text, SKPath path, float hOffset, float vOffset, SKPaint paint)
	{
		DrawTextOnPath(text, path, new SKPoint(hOffset, vOffset), warpGlyphs: true, paint);
	}

	public void DrawTextOnPath(string text, SKPath path, SKPoint offset, bool warpGlyphs, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		DrawTextOnPath(text, path, offset, warpGlyphs, paint.GetFont(), paint);
	}

	public void DrawTextOnPath(string text, SKPath path, SKPoint offset, bool warpGlyphs, SKFont font, SKPaint paint)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (warpGlyphs)
		{
			using (SKPath path2 = font.GetTextPathOnPath(text, path, paint.TextAlign, offset))
			{
				DrawPath(path2, paint);
				return;
			}
		}
		using SKTextBlob sKTextBlob = SKTextBlob.CreatePathPositioned(text, font, path, paint.TextAlign, offset);
		if (sKTextBlob != null)
		{
			DrawText(sKTextBlob, 0f, 0f, paint);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use DrawTextOnPath(string, SKPath, SKPoint, SKPaint) instead.")]
	public void DrawTextOnPath(byte[] text, SKPath path, SKPoint offset, SKPaint paint)
	{
		DrawTextOnPath(text, path, offset.X, offset.Y, paint);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use DrawTextOnPath(string, SKPath, float, float, SKPaint) instead.")]
	public unsafe void DrawTextOnPath(byte[] text, SKPath path, float hOffset, float vOffset, SKPaint paint)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		fixed (byte* ptr = text)
		{
			DrawTextOnPath((IntPtr)ptr, text.Length, path, hOffset, vOffset, paint);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use DrawTextOnPath(string, SKPath, SKPoint, SKPaint) instead.")]
	public void DrawTextOnPath(IntPtr buffer, int length, SKPath path, SKPoint offset, SKPaint paint)
	{
		DrawTextOnPath(buffer, length, path, offset.X, offset.Y, paint);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use DrawTextOnPath(string, SKPath, float, float, SKPaint) instead.")]
	public void DrawTextOnPath(IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint)
	{
		if (buffer == IntPtr.Zero && length != 0)
		{
			throw new ArgumentNullException("buffer");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SKFont font = paint.GetFont();
		using SKPath path2 = font.GetTextPathOnPath(buffer, length, paint.TextEncoding, path, paint.TextAlign, new SKPoint(hOffset, vOffset));
		DrawPath(path2, paint);
	}

	public void Flush()
	{
		SkiaApi.sk_canvas_flush(Handle);
	}

	public unsafe void DrawAnnotation(SKRect rect, string key, SKData value)
	{
		fixed (byte* key2 = StringUtilities.GetEncodedText(key, SKTextEncoding.Utf8, addNull: true))
		{
			SkiaApi.sk_canvas_draw_annotation(base.Handle, &rect, key2, value?.Handle ?? IntPtr.Zero);
		}
	}

	public unsafe void DrawUrlAnnotation(SKRect rect, SKData value)
	{
		SkiaApi.sk_canvas_draw_url_annotation(Handle, &rect, value?.Handle ?? IntPtr.Zero);
	}

	public SKData DrawUrlAnnotation(SKRect rect, string value)
	{
		SKData sKData = SKData.FromCString(value);
		DrawUrlAnnotation(rect, sKData);
		return sKData;
	}

	public unsafe void DrawNamedDestinationAnnotation(SKPoint point, SKData value)
	{
		SkiaApi.sk_canvas_draw_named_destination_annotation(Handle, &point, value?.Handle ?? IntPtr.Zero);
	}

	public SKData DrawNamedDestinationAnnotation(SKPoint point, string value)
	{
		SKData sKData = SKData.FromCString(value);
		DrawNamedDestinationAnnotation(point, sKData);
		return sKData;
	}

	public unsafe void DrawLinkDestinationAnnotation(SKRect rect, SKData value)
	{
		SkiaApi.sk_canvas_draw_link_destination_annotation(Handle, &rect, value?.Handle ?? IntPtr.Zero);
	}

	public SKData DrawLinkDestinationAnnotation(SKRect rect, string value)
	{
		SKData sKData = SKData.FromCString(value);
		DrawLinkDestinationAnnotation(rect, sKData);
		return sKData;
	}

	public void DrawBitmapNinePatch(SKBitmap bitmap, SKRectI center, SKRect dst, SKPaint paint = null)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		DrawImageNinePatch(image, center, dst, paint);
	}

	public unsafe void DrawImageNinePatch(SKImage image, SKRectI center, SKRect dst, SKPaint paint = null)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (!SKRect.Create(image.Width, image.Height).Contains(center))
		{
			throw new ArgumentException("Center rectangle must be contained inside the image bounds.", "center");
		}
		SkiaApi.sk_canvas_draw_image_nine(Handle, image.Handle, &center, &dst, paint?.Handle ?? IntPtr.Zero);
	}

	public void DrawBitmapLattice(SKBitmap bitmap, int[] xDivs, int[] yDivs, SKRect dst, SKPaint paint = null)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		DrawImageLattice(image, xDivs, yDivs, dst, paint);
	}

	public void DrawImageLattice(SKImage image, int[] xDivs, int[] yDivs, SKRect dst, SKPaint paint = null)
	{
		SKLattice sKLattice = default(SKLattice);
		sKLattice.XDivs = xDivs;
		sKLattice.YDivs = yDivs;
		SKLattice lattice = sKLattice;
		DrawImageLattice(image, lattice, dst, paint);
	}

	public void DrawBitmapLattice(SKBitmap bitmap, SKLattice lattice, SKRect dst, SKPaint paint = null)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		DrawImageLattice(image, lattice, dst, paint);
	}

	public unsafe void DrawImageLattice(SKImage image, SKLattice lattice, SKRect dst, SKPaint paint = null)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (lattice.XDivs == null)
		{
			throw new ArgumentNullException("XDivs");
		}
		if (lattice.YDivs == null)
		{
			throw new ArgumentNullException("YDivs");
		}
		fixed (int* fXDivs = lattice.XDivs)
		{
			fixed (int* fYDivs = lattice.YDivs)
			{
				fixed (SKLatticeRectType* fRectTypes = lattice.RectTypes)
				{
					fixed (SKColor* fColors = lattice.Colors)
					{
						SKLatticeInternal sKLatticeInternal = default(SKLatticeInternal);
						sKLatticeInternal.fBounds = null;
						sKLatticeInternal.fRectTypes = fRectTypes;
						sKLatticeInternal.fXCount = lattice.XDivs.Length;
						sKLatticeInternal.fXDivs = fXDivs;
						sKLatticeInternal.fYCount = lattice.YDivs.Length;
						sKLatticeInternal.fYDivs = fYDivs;
						sKLatticeInternal.fColors = (uint*)fColors;
						SKLatticeInternal sKLatticeInternal2 = sKLatticeInternal;
						if (lattice.Bounds.HasValue)
						{
							SKRectI value = lattice.Bounds.Value;
							sKLatticeInternal2.fBounds = &value;
						}
						SkiaApi.sk_canvas_draw_image_lattice(Handle, image.Handle, &sKLatticeInternal2, &dst, paint?.Handle ?? IntPtr.Zero);
					}
				}
			}
		}
	}

	public void ResetMatrix()
	{
		SkiaApi.sk_canvas_reset_matrix(Handle);
	}

	public unsafe void SetMatrix(SKMatrix matrix)
	{
		SkiaApi.sk_canvas_set_matrix(Handle, &matrix);
	}

	public void DrawVertices(SKVertexMode vmode, SKPoint[] vertices, SKColor[] colors, SKPaint paint)
	{
		SKVertices vertices2 = SKVertices.CreateCopy(vmode, vertices, colors);
		DrawVertices(vertices2, SKBlendMode.Modulate, paint);
	}

	public void DrawVertices(SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKPaint paint)
	{
		SKVertices vertices2 = SKVertices.CreateCopy(vmode, vertices, texs, colors);
		DrawVertices(vertices2, SKBlendMode.Modulate, paint);
	}

	public void DrawVertices(SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, ushort[] indices, SKPaint paint)
	{
		SKVertices vertices2 = SKVertices.CreateCopy(vmode, vertices, texs, colors, indices);
		DrawVertices(vertices2, SKBlendMode.Modulate, paint);
	}

	public void DrawVertices(SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKBlendMode mode, ushort[] indices, SKPaint paint)
	{
		SKVertices vertices2 = SKVertices.CreateCopy(vmode, vertices, texs, colors, indices);
		DrawVertices(vertices2, mode, paint);
	}

	public void DrawVertices(SKVertices vertices, SKBlendMode mode, SKPaint paint)
	{
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_vertices(Handle, vertices.Handle, mode, paint.Handle);
	}

	public unsafe void DrawArc(SKRect oval, float startAngle, float sweepAngle, bool useCenter, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_arc(Handle, &oval, startAngle, sweepAngle, useCenter, paint.Handle);
	}

	public void DrawRoundRectDifference(SKRoundRect outer, SKRoundRect inner, SKPaint paint)
	{
		if (outer == null)
		{
			throw new ArgumentNullException("outer");
		}
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_drrect(Handle, outer.Handle, inner.Handle, paint.Handle);
	}

	public unsafe void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKPaint paint)
	{
		DrawAtlas(atlas, sprites, transforms, null, SKBlendMode.Dst, null, paint);
	}

	public unsafe void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKPaint paint)
	{
		DrawAtlas(atlas, sprites, transforms, colors, mode, null, paint);
	}

	public unsafe void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKRect cullRect, SKPaint paint)
	{
		DrawAtlas(atlas, sprites, transforms, colors, mode, &cullRect, paint);
	}

	private unsafe void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKRect* cullRect, SKPaint paint)
	{
		if (atlas == null)
		{
			throw new ArgumentNullException("atlas");
		}
		if (sprites == null)
		{
			throw new ArgumentNullException("sprites");
		}
		if (transforms == null)
		{
			throw new ArgumentNullException("transforms");
		}
		if (transforms.Length != sprites.Length)
		{
			throw new ArgumentException("The number of transforms must match the number of sprites.", "transforms");
		}
		if (colors != null && colors.Length != sprites.Length)
		{
			throw new ArgumentException("The number of colors must match the number of sprites.", "colors");
		}
		fixed (SKRect* tex = sprites)
		{
			fixed (SKRotationScaleMatrix* xform = transforms)
			{
				fixed (SKColor* colors2 = colors)
				{
					SkiaApi.sk_canvas_draw_atlas(Handle, atlas.Handle, xform, tex, (uint*)colors2, transforms.Length, mode, cullRect, paint.Handle);
				}
			}
		}
	}

	public void DrawPatch(SKPoint[] cubics, SKColor[] colors, SKPoint[] texCoords, SKPaint paint)
	{
		DrawPatch(cubics, colors, texCoords, SKBlendMode.Modulate, paint);
	}

	public unsafe void DrawPatch(SKPoint[] cubics, SKColor[] colors, SKPoint[] texCoords, SKBlendMode mode, SKPaint paint)
	{
		if (cubics == null)
		{
			throw new ArgumentNullException("cubics");
		}
		if (cubics.Length != 12)
		{
			throw new ArgumentException($"Cubics must have a length of {12}.", "cubics");
		}
		if (colors != null && colors.Length != 4)
		{
			throw new ArgumentException($"Colors must have a length of {4}.", "colors");
		}
		if (texCoords != null && texCoords.Length != 4)
		{
			throw new ArgumentException($"Texture coordinates must have a length of {4}.", "texCoords");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		fixed (SKPoint* cubics2 = cubics)
		{
			fixed (SKColor* colors2 = colors)
			{
				fixed (SKPoint* texCoords2 = texCoords)
				{
					SkiaApi.sk_canvas_draw_patch(Handle, cubics2, (uint*)colors2, texCoords2, mode, paint.Handle);
				}
			}
		}
	}

	internal static SKCanvas GetObject(IntPtr handle, bool owns = true, bool unrefExisting = true)
	{
		return SKObject.GetOrAddObject(handle, owns, unrefExisting, (IntPtr h, bool o) => new SKCanvas(h, o));
	}
}
