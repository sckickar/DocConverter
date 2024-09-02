using System;

namespace SkiaSharp;

public class SKFileWStream : SKWStream
{
	public bool IsValid => SkiaApi.sk_filewstream_is_valid(Handle);

	internal SKFileWStream(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKFileWStream(string path)
		: base(CreateNew(path), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKFileWStream instance.");
		}
	}

	private unsafe static IntPtr CreateNew(string path)
	{
		fixed (byte* path2 = StringUtilities.GetEncodedText(path, SKTextEncoding.Utf8, addNull: true))
		{
			return SkiaApi.sk_filewstream_new(path2);
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_filewstream_destroy(Handle);
	}

	public static bool IsPathSupported(string path)
	{
		return true;
	}

	public static SKWStream OpenStream(string path)
	{
		SKFileWStream sKFileWStream = new SKFileWStream(path);
		if (!sKFileWStream.IsValid)
		{
			sKFileWStream.Dispose();
			sKFileWStream = null;
		}
		return sKFileWStream;
	}
}
