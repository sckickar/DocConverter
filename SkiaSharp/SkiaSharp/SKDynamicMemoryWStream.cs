using System;
using System.IO;

namespace SkiaSharp;

public class SKDynamicMemoryWStream : SKWStream
{
	internal SKDynamicMemoryWStream(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKDynamicMemoryWStream()
		: base(SkiaApi.sk_dynamicmemorywstream_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKDynamicMemoryWStream instance.");
		}
	}

	public SKData CopyToData()
	{
		SKData sKData = SKData.Create(BytesWritten);
		CopyTo(sKData.Data);
		return sKData;
	}

	public SKStreamAsset DetachAsStream()
	{
		return SKStreamAsset.GetObject(SkiaApi.sk_dynamicmemorywstream_detach_as_stream(Handle));
	}

	public SKData DetachAsData()
	{
		return SKData.GetObject(SkiaApi.sk_dynamicmemorywstream_detach_as_data(Handle));
	}

	public unsafe void CopyTo(IntPtr data)
	{
		SkiaApi.sk_dynamicmemorywstream_copy_to(Handle, (void*)data);
	}

	public unsafe void CopyTo(Span<byte> data)
	{
		int bytesWritten = BytesWritten;
		if (data.Length < bytesWritten)
		{
			throw new Exception($"Not enough space to copy. Expected at least {bytesWritten}, but received {data.Length}.");
		}
		fixed (byte* ptr = data)
		{
			void* data2 = ptr;
			SkiaApi.sk_dynamicmemorywstream_copy_to(Handle, data2);
		}
	}

	public bool CopyTo(SKWStream dst)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_dynamicmemorywstream_write_to_stream(Handle, dst.Handle);
	}

	public bool CopyTo(Stream dst)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		using SKManagedWStream dst2 = new SKManagedWStream(dst);
		return CopyTo(dst2);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_dynamicmemorywstream_destroy(Handle);
	}
}
