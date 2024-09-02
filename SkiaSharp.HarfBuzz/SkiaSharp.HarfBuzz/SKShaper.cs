using System;
using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz;

public class SKShaper : IDisposable
{
	public class Result
	{
		public uint[] Codepoints { get; }

		public uint[] Clusters { get; }

		public SKPoint[] Points { get; }

		public float Width { get; }

		public Result()
		{
			Codepoints = new uint[0];
			Clusters = new uint[0];
			Points = new SKPoint[0];
			Width = 0f;
		}

		public Result(uint[] codepoints, uint[] clusters, SKPoint[] points)
		{
			Codepoints = codepoints;
			Clusters = clusters;
			Points = points;
			Width = 0f;
		}

		public Result(uint[] codepoints, uint[] clusters, SKPoint[] points, float width)
		{
			Codepoints = codepoints;
			Clusters = clusters;
			Points = points;
			Width = width;
		}
	}

	internal const int FONT_SIZE_SCALE = 512;

	private Font font;

	private HarfBuzzSharp.Buffer buffer;

	public SKTypeface Typeface { get; private set; }

	public SKShaper(SKTypeface typeface)
	{
		Typeface = typeface ?? throw new ArgumentNullException("typeface");
		int ttcIndex;
		using (Blob blob = Typeface.OpenStream(out ttcIndex).ToHarfBuzzBlob())
		{
			using Face face = new Face(blob, ttcIndex);
			face.Index = ttcIndex;
			face.UnitsPerEm = Typeface.UnitsPerEm;
			font = new Font(face);
			font.SetScale(512, 512);
			font.SetFunctionsOpenType();
		}
		buffer = new HarfBuzzSharp.Buffer();
	}

	public void Dispose()
	{
		font?.Dispose();
		buffer?.Dispose();
	}

	public Result Shape(HarfBuzzSharp.Buffer buffer, SKPaint paint)
	{
		return Shape(buffer, 0f, 0f, paint);
	}

	public Result Shape(HarfBuzzSharp.Buffer buffer, float xOffset, float yOffset, SKPaint paint)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		font.Shape(buffer);
		int length = buffer.Length;
		GlyphInfo[] glyphInfos = buffer.GlyphInfos;
		GlyphPosition[] glyphPositions = buffer.GlyphPositions;
		float num = paint.TextSize / 512f;
		float num2 = num * paint.TextScaleX;
		SKPoint[] array = new SKPoint[length];
		uint[] array2 = new uint[length];
		uint[] array3 = new uint[length];
		float num3 = xOffset;
		for (int i = 0; i < length; i++)
		{
			array3[i] = glyphInfos[i].Codepoint;
			array2[i] = glyphInfos[i].Cluster;
			array[i] = new SKPoint(xOffset + (float)glyphPositions[i].XOffset * num2, yOffset - (float)glyphPositions[i].YOffset * num);
			xOffset += (float)glyphPositions[i].XAdvance * num2;
			yOffset += (float)glyphPositions[i].YAdvance * num;
		}
		float width = xOffset - num3;
		return new Result(array3, array2, array, width);
	}

	public Result Shape(string text, SKPaint paint)
	{
		return Shape(text, 0f, 0f, paint);
	}

	public Result Shape(string text, float xOffset, float yOffset, SKPaint paint)
	{
		if (string.IsNullOrEmpty(text))
		{
			return new Result();
		}
		using HarfBuzzSharp.Buffer buffer = new HarfBuzzSharp.Buffer();
		switch (paint.TextEncoding)
		{
		case SKTextEncoding.Utf8:
			buffer.AddUtf8(text);
			break;
		case SKTextEncoding.Utf16:
			buffer.AddUtf16(text);
			break;
		case SKTextEncoding.Utf32:
			buffer.AddUtf32(text);
			break;
		default:
			throw new NotSupportedException("TextEncoding of type GlyphId is not supported.");
		}
		buffer.GuessSegmentProperties();
		return Shape(buffer, xOffset, yOffset, paint);
	}
}
