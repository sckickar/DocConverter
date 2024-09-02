using System;

namespace SkiaSharp.HarfBuzz;

public static class CanvasExtensions
{
	public static void DrawShapedText(this SKCanvas canvas, string text, SKPoint p, SKPaint paint)
	{
		canvas.DrawShapedText(text, p.X, p.Y, paint);
	}

	public static void DrawShapedText(this SKCanvas canvas, string text, float x, float y, SKPaint paint)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		using SKShaper shaper = new SKShaper(paint.GetFont().Typeface);
		canvas.DrawShapedText(shaper, text, x, y, paint);
	}

	public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, SKPoint p, SKPaint paint)
	{
		canvas.DrawShapedText(shaper, text, p.X, p.Y, paint);
	}

	public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, float x, float y, SKPaint paint)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (canvas == null)
		{
			throw new ArgumentNullException("canvas");
		}
		if (shaper == null)
		{
			throw new ArgumentNullException("shaper");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		using SKFont sKFont = paint.ToFont();
		sKFont.Typeface = shaper.Typeface;
		SKShaper.Result result = shaper.Shape(text, x, y, paint);
		using SKTextBlobBuilder sKTextBlobBuilder = new SKTextBlobBuilder();
		SKPositionedRunBuffer sKPositionedRunBuffer = sKTextBlobBuilder.AllocatePositionedRun(sKFont, result.Codepoints.Length);
		Span<ushort> glyphSpan = sKPositionedRunBuffer.GetGlyphSpan();
		Span<SKPoint> positionSpan = sKPositionedRunBuffer.GetPositionSpan();
		for (int i = 0; i < result.Codepoints.Length; i++)
		{
			glyphSpan[i] = (ushort)result.Codepoints[i];
			positionSpan[i] = result.Points[i];
		}
		using SKTextBlob text2 = sKTextBlobBuilder.Build();
		float num = 0f;
		if (paint.TextAlign != 0)
		{
			float num2 = result.Width;
			if (paint.TextAlign == SKTextAlign.Center)
			{
				num2 *= 0.5f;
			}
			num -= num2;
		}
		canvas.DrawText(text2, num, 0f, paint);
	}
}
