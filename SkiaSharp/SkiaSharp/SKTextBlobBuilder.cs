using System;
using System.ComponentModel;

namespace SkiaSharp;

public class SKTextBlobBuilder : SKObject, ISKSkipObjectRegistration
{
	internal SKTextBlobBuilder(IntPtr x, bool owns)
		: base(x, owns)
	{
	}

	public SKTextBlobBuilder()
		: this(SkiaApi.sk_textblob_builder_new(), owns: true)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_textblob_builder_delete(Handle);
	}

	public SKTextBlob Build()
	{
		SKTextBlob @object = SKTextBlob.GetObject(SkiaApi.sk_textblob_builder_make(Handle));
		GC.KeepAlive(this);
		return @object;
	}

	public void AddRun(ReadOnlySpan<ushort> glyphs, SKFont font, SKPoint origin = default(SKPoint))
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKPositionedRunBuffer sKPositionedRunBuffer = AllocatePositionedRun(font, glyphs.Length);
		glyphs.CopyTo(sKPositionedRunBuffer.GetGlyphSpan());
		font.GetGlyphPositions(sKPositionedRunBuffer.GetGlyphSpan(), sKPositionedRunBuffer.GetPositionSpan(), origin);
	}

	public void AddHorizontalRun(ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<float> positions, float y)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKHorizontalRunBuffer sKHorizontalRunBuffer = AllocateHorizontalRun(font, glyphs.Length, y);
		glyphs.CopyTo(sKHorizontalRunBuffer.GetGlyphSpan());
		positions.CopyTo(sKHorizontalRunBuffer.GetPositionSpan());
	}

	public void AddPositionedRun(ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<SKPoint> positions)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKPositionedRunBuffer sKPositionedRunBuffer = AllocatePositionedRun(font, glyphs.Length);
		glyphs.CopyTo(sKPositionedRunBuffer.GetGlyphSpan());
		positions.CopyTo(sKPositionedRunBuffer.GetPositionSpan());
	}

	public void AddRotationScaleRun(ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRotationScaleRunBuffer sKRotationScaleRunBuffer = AllocateRotationScaleRun(font, glyphs.Length);
		glyphs.CopyTo(sKRotationScaleRunBuffer.GetGlyphSpan());
		positions.CopyTo(sKRotationScaleRunBuffer.GetRotationScaleSpan());
	}

	public void AddPathPositionedRun(ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<float> glyphWidths, ReadOnlySpan<SKPoint> glyphOffsets, SKPath path, SKTextAlign textAlign = SKTextAlign.Left)
	{
		using SKPathMeasure sKPathMeasure = new SKPathMeasure(path);
		float length = sKPathMeasure.Length;
		float num = glyphOffsets[glyphs.Length - 1].X + glyphWidths[glyphs.Length - 1];
		float num2 = (float)textAlign * 0.5f;
		float num3 = glyphOffsets[0].X + (length - num) * num2;
		int start = 0;
		int num4 = 0;
		Utils.RentedArray<SKRotationScaleMatrix> rentedArray = Utils.RentArray<SKRotationScaleMatrix>(glyphs.Length);
		try
		{
			for (int i = 0; i < glyphOffsets.Length; i++)
			{
				SKPoint sKPoint = glyphOffsets[i];
				float num5 = glyphWidths[i] * 0.5f;
				float num6 = num3 + sKPoint.X + num5;
				if (num6 >= 0f && num6 < length && sKPathMeasure.GetPositionAndTangent(num6, out var position, out var tangent))
				{
					if (num4 == 0)
					{
						start = i;
					}
					float x = tangent.X;
					float y = tangent.Y;
					float x2 = position.X;
					float y2 = position.Y;
					x2 -= x * num5;
					y2 -= y * num5;
					float y3 = sKPoint.Y;
					x2 -= y3 * y;
					y2 += y3 * x;
					rentedArray.Span[num4++] = new SKRotationScaleMatrix(x, y, x2, y2);
				}
			}
			ReadOnlySpan<ushort> glyphs2 = glyphs.Slice(start, num4);
			Span<SKRotationScaleMatrix> span = rentedArray.Span.Slice(0, num4);
			AddRotationScaleRun(glyphs2, font, span);
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	public unsafe SKRunBuffer AllocateRun(SKFont font, int count, float x, float y, SKRect? bounds = null)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRunBufferInternal buffer = default(SKRunBufferInternal);
		if (bounds.HasValue)
		{
			SKRect valueOrDefault = bounds.GetValueOrDefault();
			SkiaApi.sk_textblob_builder_alloc_run(Handle, font.Handle, count, x, y, &valueOrDefault, &buffer);
		}
		else
		{
			SkiaApi.sk_textblob_builder_alloc_run(Handle, font.Handle, count, x, y, null, &buffer);
		}
		return new SKRunBuffer(buffer, count);
	}

	public unsafe SKHorizontalRunBuffer AllocateHorizontalRun(SKFont font, int count, float y, SKRect? bounds = null)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRunBufferInternal buffer = default(SKRunBufferInternal);
		if (bounds.HasValue)
		{
			SKRect valueOrDefault = bounds.GetValueOrDefault();
			SkiaApi.sk_textblob_builder_alloc_run_pos_h(Handle, font.Handle, count, y, &valueOrDefault, &buffer);
		}
		else
		{
			SkiaApi.sk_textblob_builder_alloc_run_pos_h(Handle, font.Handle, count, y, null, &buffer);
		}
		return new SKHorizontalRunBuffer(buffer, count);
	}

	public unsafe SKPositionedRunBuffer AllocatePositionedRun(SKFont font, int count, SKRect? bounds = null)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRunBufferInternal buffer = default(SKRunBufferInternal);
		if (bounds.HasValue)
		{
			SKRect valueOrDefault = bounds.GetValueOrDefault();
			SkiaApi.sk_textblob_builder_alloc_run_pos(Handle, font.Handle, count, &valueOrDefault, &buffer);
		}
		else
		{
			SkiaApi.sk_textblob_builder_alloc_run_pos(Handle, font.Handle, count, null, &buffer);
		}
		return new SKPositionedRunBuffer(buffer, count);
	}

	public unsafe SKRotationScaleRunBuffer AllocateRotationScaleRun(SKFont font, int count)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKRunBufferInternal buffer = default(SKRunBufferInternal);
		SkiaApi.sk_textblob_builder_alloc_run_rsxform(Handle, font.Handle, count, &buffer);
		return new SKRotationScaleRunBuffer(buffer, count);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
	public void AddRun(SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters)
	{
		byte[] encodedText = StringUtilities.GetEncodedText(text, SKTextEncoding.Utf8);
		AddRun(font, x, y, glyphs, encodedText, clusters, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
	public void AddRun(SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters, SKRect bounds)
	{
		byte[] encodedText = StringUtilities.GetEncodedText(text, SKTextEncoding.Utf8);
		AddRun(font, x, y, (ReadOnlySpan<ushort>)glyphs, (ReadOnlySpan<byte>)encodedText, (ReadOnlySpan<uint>)clusters, (SKRect?)bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
	public void AddRun(SKPaint font, float x, float y, ushort[] glyphs)
	{
		AddRun(font, x, y, glyphs, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
	public void AddRun(SKPaint font, float x, float y, ushort[] glyphs, SKRect bounds)
	{
		AddRun(font, x, y, glyphs, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
	public void AddRun(SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters)
	{
		AddRun(font, x, y, glyphs, text, clusters, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
	public void AddRun(SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters, SKRect bounds)
	{
		AddRun(font, x, y, (ReadOnlySpan<ushort>)glyphs, (ReadOnlySpan<byte>)text, (ReadOnlySpan<uint>)clusters, (SKRect?)bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
	public void AddRun(SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs)
	{
		AddRun(font, x, y, glyphs, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
	public void AddRun(SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs, SKRect? bounds)
	{
		AddRun(font, x, y, glyphs, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
	public void AddRun(SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters)
	{
		AddRun(font, x, y, glyphs, text, clusters, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
	public void AddRun(SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters, SKRect? bounds)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (glyphs.IsEmpty)
		{
			throw new ArgumentNullException("glyphs");
		}
		if (!text.IsEmpty)
		{
			if (clusters.IsEmpty)
			{
				throw new ArgumentNullException("clusters");
			}
			if (glyphs.Length != clusters.Length)
			{
				throw new ArgumentException("The number of glyphs and clusters must be the same.");
			}
		}
		SKRunBuffer sKRunBuffer = AllocateRun(font, glyphs.Length, x, y, (!text.IsEmpty) ? text.Length : 0, bounds);
		sKRunBuffer.SetGlyphs(glyphs);
		if (!text.IsEmpty)
		{
			sKRunBuffer.SetText(text);
			sKRunBuffer.SetClusters(clusters);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
	public void AddHorizontalRun(SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters)
	{
		byte[] encodedText = StringUtilities.GetEncodedText(text, SKTextEncoding.Utf8);
		AddHorizontalRun(font, y, glyphs, positions, encodedText, clusters, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
	public void AddHorizontalRun(SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters, SKRect bounds)
	{
		byte[] encodedText = StringUtilities.GetEncodedText(text, SKTextEncoding.Utf8);
		AddHorizontalRun(font, y, (ReadOnlySpan<ushort>)glyphs, (ReadOnlySpan<float>)positions, (ReadOnlySpan<byte>)encodedText, (ReadOnlySpan<uint>)clusters, (SKRect?)bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
	public void AddHorizontalRun(SKPaint font, float y, ushort[] glyphs, float[] positions)
	{
		AddHorizontalRun(font, y, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
	public void AddHorizontalRun(SKPaint font, float y, ushort[] glyphs, float[] positions, SKRect bounds)
	{
		AddHorizontalRun(font, y, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
	public void AddHorizontalRun(SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters)
	{
		AddHorizontalRun(font, y, glyphs, positions, text, clusters, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
	public void AddHorizontalRun(SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters, SKRect bounds)
	{
		AddHorizontalRun(font, y, (ReadOnlySpan<ushort>)glyphs, (ReadOnlySpan<float>)positions, (ReadOnlySpan<byte>)text, (ReadOnlySpan<uint>)clusters, (SKRect?)bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
	public void AddHorizontalRun(SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions)
	{
		AddHorizontalRun(font, y, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
	public void AddHorizontalRun(SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions, SKRect? bounds)
	{
		AddHorizontalRun(font, y, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
	public void AddHorizontalRun(SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters)
	{
		AddHorizontalRun(font, y, glyphs, positions, text, clusters, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
	public void AddHorizontalRun(SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters, SKRect? bounds)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (glyphs.IsEmpty)
		{
			throw new ArgumentNullException("glyphs");
		}
		if (positions.IsEmpty)
		{
			throw new ArgumentNullException("positions");
		}
		if (glyphs.Length != positions.Length)
		{
			throw new ArgumentException("The number of glyphs and positions must be the same.");
		}
		if (!text.IsEmpty)
		{
			if (clusters.IsEmpty)
			{
				throw new ArgumentNullException("clusters");
			}
			if (glyphs.Length != clusters.Length)
			{
				throw new ArgumentException("The number of glyphs and clusters must be the same.");
			}
		}
		SKHorizontalRunBuffer sKHorizontalRunBuffer = AllocateHorizontalRun(font, glyphs.Length, y, (!text.IsEmpty) ? text.Length : 0, bounds);
		sKHorizontalRunBuffer.SetGlyphs(glyphs);
		sKHorizontalRunBuffer.SetPositions(positions);
		if (!text.IsEmpty)
		{
			sKHorizontalRunBuffer.SetText(text);
			sKHorizontalRunBuffer.SetClusters(clusters);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
	public void AddPositionedRun(SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters)
	{
		byte[] encodedText = StringUtilities.GetEncodedText(text, SKTextEncoding.Utf8);
		AddPositionedRun(font, glyphs, positions, encodedText, clusters, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
	public void AddPositionedRun(SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters, SKRect bounds)
	{
		byte[] encodedText = StringUtilities.GetEncodedText(text, SKTextEncoding.Utf8);
		AddPositionedRun(font, (ReadOnlySpan<ushort>)glyphs, (ReadOnlySpan<SKPoint>)positions, (ReadOnlySpan<byte>)encodedText, (ReadOnlySpan<uint>)clusters, (SKRect?)bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
	public void AddPositionedRun(SKPaint font, ushort[] glyphs, SKPoint[] positions)
	{
		AddPositionedRun(font, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
	public void AddPositionedRun(SKPaint font, ushort[] glyphs, SKPoint[] positions, SKRect bounds)
	{
		AddPositionedRun(font, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
	public void AddPositionedRun(SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters)
	{
		AddPositionedRun(font, glyphs, positions, text, clusters, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
	public void AddPositionedRun(SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters, SKRect bounds)
	{
		AddPositionedRun(font, (ReadOnlySpan<ushort>)glyphs, (ReadOnlySpan<SKPoint>)positions, (ReadOnlySpan<byte>)text, (ReadOnlySpan<uint>)clusters, (SKRect?)bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
	public void AddPositionedRun(SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions)
	{
		AddPositionedRun(font, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
	public void AddPositionedRun(SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions, SKRect? bounds)
	{
		AddPositionedRun(font, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
	public void AddPositionedRun(SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters)
	{
		AddPositionedRun(font, glyphs, positions, text, clusters, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
	public void AddPositionedRun(SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters, SKRect? bounds)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (glyphs.IsEmpty)
		{
			throw new ArgumentNullException("glyphs");
		}
		if (positions.IsEmpty)
		{
			throw new ArgumentNullException("positions");
		}
		if (glyphs.Length != positions.Length)
		{
			throw new ArgumentException("The number of glyphs and positions must be the same.");
		}
		if (!text.IsEmpty)
		{
			if (clusters.IsEmpty)
			{
				throw new ArgumentNullException("clusters");
			}
			if (glyphs.Length != clusters.Length)
			{
				throw new ArgumentException("The number of glyphs and clusters must be the same.");
			}
		}
		SKPositionedRunBuffer sKPositionedRunBuffer = AllocatePositionedRun(font, glyphs.Length, (!text.IsEmpty) ? text.Length : 0, bounds);
		sKPositionedRunBuffer.SetGlyphs(glyphs);
		sKPositionedRunBuffer.SetPositions(positions);
		if (!text.IsEmpty)
		{
			sKPositionedRunBuffer.SetText(text);
			sKPositionedRunBuffer.SetClusters(clusters);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
	public SKRunBuffer AllocateRun(SKPaint font, int count, float x, float y)
	{
		return AllocateRun(font, count, x, y, 0, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
	public SKRunBuffer AllocateRun(SKPaint font, int count, float x, float y, SKRect? bounds)
	{
		return AllocateRun(font, count, x, y, 0, bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
	public SKRunBuffer AllocateRun(SKPaint font, int count, float x, float y, int textByteCount)
	{
		return AllocateRun(font, count, x, y, textByteCount, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
	public unsafe SKRunBuffer AllocateRun(SKPaint font, int count, float x, float y, int textByteCount, SKRect? bounds)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKTextEncoding textEncoding = font.TextEncoding;
		try
		{
			font.TextEncoding = SKTextEncoding.GlyphId;
			SKRunBufferInternal buffer = default(SKRunBufferInternal);
			if (bounds.HasValue)
			{
				SKRect valueOrDefault = bounds.GetValueOrDefault();
				SkiaApi.sk_textblob_builder_alloc_run_text(Handle, font.GetFont().Handle, count, x, y, textByteCount, &valueOrDefault, &buffer);
			}
			else
			{
				SkiaApi.sk_textblob_builder_alloc_run_text(Handle, font.GetFont().Handle, count, x, y, textByteCount, null, &buffer);
			}
			return new SKRunBuffer(buffer, count, textByteCount);
		}
		finally
		{
			font.TextEncoding = textEncoding;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
	public SKHorizontalRunBuffer AllocateHorizontalRun(SKPaint font, int count, float y)
	{
		return AllocateHorizontalRun(font, count, y, 0, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
	public SKHorizontalRunBuffer AllocateHorizontalRun(SKPaint font, int count, float y, SKRect? bounds)
	{
		return AllocateHorizontalRun(font, count, y, 0, bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
	public SKHorizontalRunBuffer AllocateHorizontalRun(SKPaint font, int count, float y, int textByteCount)
	{
		return AllocateHorizontalRun(font, count, y, textByteCount, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
	public unsafe SKHorizontalRunBuffer AllocateHorizontalRun(SKPaint font, int count, float y, int textByteCount, SKRect? bounds)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKTextEncoding textEncoding = font.TextEncoding;
		try
		{
			font.TextEncoding = SKTextEncoding.GlyphId;
			SKRunBufferInternal buffer = default(SKRunBufferInternal);
			if (bounds.HasValue)
			{
				SKRect valueOrDefault = bounds.GetValueOrDefault();
				SkiaApi.sk_textblob_builder_alloc_run_text_pos_h(Handle, font.GetFont().Handle, count, y, textByteCount, &valueOrDefault, &buffer);
			}
			else
			{
				SkiaApi.sk_textblob_builder_alloc_run_text_pos_h(Handle, font.GetFont().Handle, count, y, textByteCount, null, &buffer);
			}
			return new SKHorizontalRunBuffer(buffer, count, textByteCount);
		}
		finally
		{
			font.TextEncoding = textEncoding;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
	public SKPositionedRunBuffer AllocatePositionedRun(SKPaint font, int count)
	{
		return AllocatePositionedRun(font, count, 0, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
	public SKPositionedRunBuffer AllocatePositionedRun(SKPaint font, int count, SKRect? bounds)
	{
		return AllocatePositionedRun(font, count, 0, bounds);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
	public SKPositionedRunBuffer AllocatePositionedRun(SKPaint font, int count, int textByteCount)
	{
		return AllocatePositionedRun(font, count, textByteCount, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
	public unsafe SKPositionedRunBuffer AllocatePositionedRun(SKPaint font, int count, int textByteCount, SKRect? bounds)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		SKTextEncoding textEncoding = font.TextEncoding;
		try
		{
			font.TextEncoding = SKTextEncoding.GlyphId;
			SKRunBufferInternal buffer = default(SKRunBufferInternal);
			if (bounds.HasValue)
			{
				SKRect valueOrDefault = bounds.GetValueOrDefault();
				SkiaApi.sk_textblob_builder_alloc_run_text_pos(Handle, font.GetFont().Handle, count, textByteCount, &valueOrDefault, &buffer);
			}
			else
			{
				SkiaApi.sk_textblob_builder_alloc_run_text_pos(Handle, font.GetFont().Handle, count, textByteCount, null, &buffer);
			}
			return new SKPositionedRunBuffer(buffer, count, textByteCount);
		}
		finally
		{
			font.TextEncoding = textEncoding;
		}
	}
}
