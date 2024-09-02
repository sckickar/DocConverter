using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp;

public class SKCodec : SKObject, ISKSkipObjectRegistration
{
	public static int MinBufferedBytesNeeded => (int)SkiaApi.sk_codec_min_buffered_bytes_needed();

	public unsafe SKImageInfo Info
	{
		get
		{
			SKImageInfoNative native = default(SKImageInfoNative);
			SkiaApi.sk_codec_get_info(Handle, &native);
			return SKImageInfoNative.ToManaged(ref native);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use EncodedOrigin instead.")]
	public SKCodecOrigin Origin => (SKCodecOrigin)EncodedOrigin;

	public SKEncodedOrigin EncodedOrigin => SkiaApi.sk_codec_get_origin(Handle);

	public SKEncodedImageFormat EncodedFormat => SkiaApi.sk_codec_get_encoded_format(Handle);

	public byte[] Pixels
	{
		get
		{
			byte[] pixels;
			SKCodecResult pixels2 = GetPixels(out pixels);
			if (pixels2 != 0 && pixels2 != SKCodecResult.IncompleteInput)
			{
				throw new Exception(pixels2.ToString());
			}
			return pixels;
		}
	}

	public int RepetitionCount => SkiaApi.sk_codec_get_repetition_count(Handle);

	public int FrameCount => SkiaApi.sk_codec_get_frame_count(Handle);

	public unsafe SKCodecFrameInfo[] FrameInfo
	{
		get
		{
			int num = SkiaApi.sk_codec_get_frame_count(Handle);
			SKCodecFrameInfo[] array = new SKCodecFrameInfo[num];
			fixed (SKCodecFrameInfo* frameInfo = array)
			{
				SkiaApi.sk_codec_get_frame_info(Handle, frameInfo);
			}
			return array;
		}
	}

	public SKCodecScanlineOrder ScanlineOrder => SkiaApi.sk_codec_get_scanline_order(Handle);

	public int NextScanline => SkiaApi.sk_codec_next_scanline(Handle);

	internal SKCodec(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_codec_destroy(Handle);
	}

	public unsafe SKSizeI GetScaledDimensions(float desiredScale)
	{
		SKSizeI result = default(SKSizeI);
		SkiaApi.sk_codec_get_scaled_dimensions(Handle, desiredScale, &result);
		return result;
	}

	public unsafe bool GetValidSubset(ref SKRectI desiredSubset)
	{
		fixed (SKRectI* desiredSubset2 = &desiredSubset)
		{
			return SkiaApi.sk_codec_get_valid_subset(Handle, desiredSubset2);
		}
	}

	public unsafe bool GetFrameInfo(int index, out SKCodecFrameInfo frameInfo)
	{
		fixed (SKCodecFrameInfo* frameInfo2 = &frameInfo)
		{
			return SkiaApi.sk_codec_get_frame_info_for_index(Handle, index, frameInfo2);
		}
	}

	public SKCodecResult GetPixels(out byte[] pixels)
	{
		return GetPixels(Info, out pixels);
	}

	public SKCodecResult GetPixels(SKImageInfo info, out byte[] pixels)
	{
		pixels = new byte[info.BytesSize];
		return GetPixels(info, pixels);
	}

	public unsafe SKCodecResult GetPixels(SKImageInfo info, byte[] pixels)
	{
		if (pixels == null)
		{
			throw new ArgumentNullException("pixels");
		}
		fixed (byte* ptr = pixels)
		{
			return GetPixels(info, (IntPtr)ptr, info.RowBytes, SKCodecOptions.Default);
		}
	}

	public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels)
	{
		return GetPixels(info, pixels, info.RowBytes, SKCodecOptions.Default);
	}

	public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, SKCodecOptions options)
	{
		return GetPixels(info, pixels, info.RowBytes, options);
	}

	public unsafe SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options)
	{
		if (pixels == IntPtr.Zero)
		{
			throw new ArgumentNullException("pixels");
		}
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		SKCodecOptionsInternal sKCodecOptionsInternal = default(SKCodecOptionsInternal);
		sKCodecOptionsInternal.fZeroInitialized = options.ZeroInitialized;
		sKCodecOptionsInternal.fSubset = null;
		sKCodecOptionsInternal.fFrameIndex = options.FrameIndex;
		sKCodecOptionsInternal.fPriorFrame = options.PriorFrame;
		SKCodecOptionsInternal sKCodecOptionsInternal2 = sKCodecOptionsInternal;
		SKRectI sKRectI = default(SKRectI);
		if (options.HasSubset)
		{
			sKRectI = options.Subset.Value;
			sKCodecOptionsInternal2.fSubset = &sKRectI;
		}
		return SkiaApi.sk_codec_get_pixels(Handle, &sKImageInfoNative, (void*)pixels, (IntPtr)rowBytes, &sKCodecOptionsInternal2);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
	public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount)
	{
		return GetPixels(info, pixels, rowBytes, options);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, SKCodecOptions) instead.")]
	public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount)
	{
		return GetPixels(info, pixels, info.RowBytes, options);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr) instead.")]
	public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, IntPtr colorTable, ref int colorTableCount)
	{
		return GetPixels(info, pixels, info.RowBytes, SKCodecOptions.Default);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
	public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount)
	{
		return GetPixels(info, pixels, rowBytes, options);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, SKCodecOptions) instead.")]
	public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount)
	{
		return GetPixels(info, pixels, info.RowBytes, options);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr) instead.")]
	public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, SKColorTable colorTable, ref int colorTableCount)
	{
		return GetPixels(info, pixels, info.RowBytes, SKCodecOptions.Default);
	}

	public unsafe SKCodecResult StartIncrementalDecode(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options)
	{
		if (pixels == IntPtr.Zero)
		{
			throw new ArgumentNullException("pixels");
		}
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		SKCodecOptionsInternal sKCodecOptionsInternal = default(SKCodecOptionsInternal);
		sKCodecOptionsInternal.fZeroInitialized = options.ZeroInitialized;
		sKCodecOptionsInternal.fSubset = null;
		sKCodecOptionsInternal.fFrameIndex = options.FrameIndex;
		sKCodecOptionsInternal.fPriorFrame = options.PriorFrame;
		SKCodecOptionsInternal sKCodecOptionsInternal2 = sKCodecOptionsInternal;
		SKRectI sKRectI = default(SKRectI);
		if (options.HasSubset)
		{
			sKRectI = options.Subset.Value;
			sKCodecOptionsInternal2.fSubset = &sKRectI;
		}
		return SkiaApi.sk_codec_start_incremental_decode(Handle, &sKImageInfoNative, (void*)pixels, (IntPtr)rowBytes, &sKCodecOptionsInternal2);
	}

	public unsafe SKCodecResult StartIncrementalDecode(SKImageInfo info, IntPtr pixels, int rowBytes)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		return SkiaApi.sk_codec_start_incremental_decode(Handle, &sKImageInfoNative, (void*)pixels, (IntPtr)rowBytes, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use StartIncrementalDecode(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
	public SKCodecResult StartIncrementalDecode(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount)
	{
		return StartIncrementalDecode(info, pixels, rowBytes, options);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use StartIncrementalDecode(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
	public SKCodecResult StartIncrementalDecode(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount)
	{
		return StartIncrementalDecode(info, pixels, rowBytes, options);
	}

	public unsafe SKCodecResult IncrementalDecode(out int rowsDecoded)
	{
		fixed (int* rowsDecoded2 = &rowsDecoded)
		{
			return SkiaApi.sk_codec_incremental_decode(Handle, rowsDecoded2);
		}
	}

	public unsafe SKCodecResult IncrementalDecode()
	{
		return SkiaApi.sk_codec_incremental_decode(Handle, null);
	}

	public unsafe SKCodecResult StartScanlineDecode(SKImageInfo info, SKCodecOptions options)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		SKCodecOptionsInternal sKCodecOptionsInternal = default(SKCodecOptionsInternal);
		sKCodecOptionsInternal.fZeroInitialized = options.ZeroInitialized;
		sKCodecOptionsInternal.fSubset = null;
		sKCodecOptionsInternal.fFrameIndex = options.FrameIndex;
		sKCodecOptionsInternal.fPriorFrame = options.PriorFrame;
		SKCodecOptionsInternal sKCodecOptionsInternal2 = sKCodecOptionsInternal;
		SKRectI sKRectI = default(SKRectI);
		if (options.HasSubset)
		{
			sKRectI = options.Subset.Value;
			sKCodecOptionsInternal2.fSubset = &sKRectI;
		}
		return SkiaApi.sk_codec_start_scanline_decode(Handle, &sKImageInfoNative, &sKCodecOptionsInternal2);
	}

	public unsafe SKCodecResult StartScanlineDecode(SKImageInfo info)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		return SkiaApi.sk_codec_start_scanline_decode(Handle, &sKImageInfoNative, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use StartScanlineDecode(SKImageInfo, SKCodecOptions) instead.")]
	public SKCodecResult StartScanlineDecode(SKImageInfo info, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount)
	{
		return StartScanlineDecode(info, options);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use StartScanlineDecode(SKImageInfo, SKCodecOptions) instead.")]
	public SKCodecResult StartScanlineDecode(SKImageInfo info, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount)
	{
		return StartScanlineDecode(info, options);
	}

	public unsafe int GetScanlines(IntPtr dst, int countLines, int rowBytes)
	{
		if (dst == IntPtr.Zero)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_codec_get_scanlines(Handle, (void*)dst, countLines, (IntPtr)rowBytes);
	}

	public bool SkipScanlines(int countLines)
	{
		return SkiaApi.sk_codec_skip_scanlines(Handle, countLines);
	}

	public int GetOutputScanline(int inputScanline)
	{
		return SkiaApi.sk_codec_output_scanline(Handle, inputScanline);
	}

	public static SKCodec Create(string filename)
	{
		SKCodecResult result;
		return Create(filename, out result);
	}

	public static SKCodec Create(string filename, out SKCodecResult result)
	{
		SKStreamAsset sKStreamAsset = SKFileStream.OpenStream(filename);
		if (sKStreamAsset == null)
		{
			result = SKCodecResult.InternalError;
			return null;
		}
		return Create(sKStreamAsset, out result);
	}

	public static SKCodec Create(Stream stream)
	{
		SKCodecResult result;
		return Create(stream, out result);
	}

	public static SKCodec Create(Stream stream, out SKCodecResult result)
	{
		return Create(WrapManagedStream(stream), out result);
	}

	public static SKCodec Create(SKStream stream)
	{
		SKCodecResult result;
		return Create(stream, out result);
	}

	public unsafe static SKCodec Create(SKStream stream, out SKCodecResult result)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (stream is SKFileStream { IsValid: false })
		{
			throw new ArgumentException("File stream was not valid.", "stream");
		}
		fixed (SKCodecResult* result2 = &result)
		{
			SKCodec @object = GetObject(SkiaApi.sk_codec_new_from_stream(stream.Handle, result2));
			stream.RevokeOwnership(@object);
			return @object;
		}
	}

	public static SKCodec Create(SKData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return GetObject(SkiaApi.sk_codec_new_from_data(data.Handle));
	}

	internal static SKStream WrapManagedStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (stream.CanSeek)
		{
			return new SKManagedStream(stream, disposeManagedStream: true);
		}
		return new SKFrontBufferedManagedStream(stream, MinBufferedBytesNeeded, disposeUnderlyingStream: true);
	}

	internal static SKCodec GetObject(IntPtr handle)
	{
		if (!(handle == IntPtr.Zero))
		{
			return new SKCodec(handle, owns: true);
		}
		return null;
	}
}
