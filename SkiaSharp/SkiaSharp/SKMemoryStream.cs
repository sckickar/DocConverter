using System;

namespace SkiaSharp;

public class SKMemoryStream : SKStreamMemory
{
	internal SKMemoryStream(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKMemoryStream()
		: this(SkiaApi.sk_memorystream_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKMemoryStream instance.");
		}
	}

	public SKMemoryStream(ulong length)
		: this(SkiaApi.sk_memorystream_new_with_length((IntPtr)(long)length), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKMemoryStream instance.");
		}
	}

	internal unsafe SKMemoryStream(IntPtr data, IntPtr length, bool copyData = false)
		: this(SkiaApi.sk_memorystream_new_with_data((void*)data, length, copyData), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKMemoryStream instance.");
		}
	}

	public SKMemoryStream(SKData data)
		: this(SkiaApi.sk_memorystream_new_with_skdata(data.Handle), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKMemoryStream instance.");
		}
	}

	public SKMemoryStream(byte[] data)
		: this()
	{
		SetMemory(data);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_memorystream_destroy(Handle);
	}

	internal unsafe void SetMemory(IntPtr data, IntPtr length, bool copyData = false)
	{
		SkiaApi.sk_memorystream_set_memory(Handle, (void*)data, length, copyData);
	}

	internal unsafe void SetMemory(byte[] data, IntPtr length, bool copyData = false)
	{
		fixed (byte* data2 = data)
		{
			SkiaApi.sk_memorystream_set_memory(Handle, data2, length, copyData);
		}
	}

	public void SetMemory(byte[] data)
	{
		SetMemory(data, (IntPtr)data.Length, copyData: true);
	}
}
