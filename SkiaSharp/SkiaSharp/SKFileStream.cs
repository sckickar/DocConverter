using System;

namespace SkiaSharp;

public class SKFileStream : SKStreamAsset
{
	public bool IsValid => SkiaApi.sk_filestream_is_valid(Handle);

	internal SKFileStream(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKFileStream(string path)
		: base(CreateNew(path), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKFileStream instance.");
		}
	}

	private unsafe static IntPtr CreateNew(string path)
	{
		fixed (byte* path2 = StringUtilities.GetEncodedText(path, SKTextEncoding.Utf8, addNull: true))
		{
			return SkiaApi.sk_filestream_new(path2);
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_filestream_destroy(Handle);
	}

	public static bool IsPathSupported(string path)
	{
		return true;
	}

	public static SKStreamAsset OpenStream(string path)
	{
		SKFileStream sKFileStream = new SKFileStream(path);
		if (!sKFileStream.IsValid)
		{
			sKFileStream.Dispose();
			sKFileStream = null;
		}
		return sKFileStream;
	}
}
