using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SkiaSharp.Internals;

namespace SkiaSharp;

public class SKData : SKObject, ISKNonVirtualReferenceCounted, ISKReferenceCounted
{
	private class SKDataStream : UnmanagedMemoryStream
	{
		private SKData host;

		private readonly bool disposeHost;

		public unsafe SKDataStream(SKData host, bool disposeHost = false)
			: base((byte*)(void*)host.Data, host.Size, host.Size, FileAccess.ReadWrite)
		{
			this.host = host;
			this.disposeHost = disposeHost;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposeHost)
			{
				host?.Dispose();
			}
			host = null;
		}
	}

	private sealed class SKDataStatic : SKData
	{
		internal SKDataStatic(IntPtr x)
			: base(x, owns: false)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	internal const int CopyBufferSize = 81920;

	private static readonly SKData empty;

	public static SKData Empty => empty;

	public bool IsEmpty => Size == 0;

	public long Size => (long)SkiaApi.sk_data_get_size(Handle);

	public unsafe IntPtr Data => (IntPtr)SkiaApi.sk_data_get_data(Handle);

	public unsafe Span<byte> Span => new Span<byte>((void*)Data, (int)Size);

	static SKData()
	{
		empty = new SKDataStatic(SkiaApi.sk_data_new_empty());
	}

	internal static void EnsureStaticInstanceAreInitialized()
	{
	}

	internal SKData(IntPtr x, bool owns)
		: base(x, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	void ISKNonVirtualReferenceCounted.ReferenceNative()
	{
		SkiaApi.sk_data_ref(Handle);
	}

	void ISKNonVirtualReferenceCounted.UnreferenceNative()
	{
		SkiaApi.sk_data_unref(Handle);
	}

	public static SKData CreateCopy(IntPtr bytes, int length)
	{
		return CreateCopy(bytes, (ulong)length);
	}

	public static SKData CreateCopy(IntPtr bytes, long length)
	{
		return CreateCopy(bytes, (ulong)length);
	}

	public unsafe static SKData CreateCopy(IntPtr bytes, ulong length)
	{
		if (!PlatformConfiguration.Is64Bit && length > uint.MaxValue)
		{
			throw new ArgumentOutOfRangeException("length", "The length exceeds the size of pointers.");
		}
		return GetObject(SkiaApi.sk_data_new_with_copy((void*)bytes, (IntPtr)(long)length));
	}

	public static SKData CreateCopy(byte[] bytes)
	{
		return CreateCopy(bytes, (ulong)bytes.Length);
	}

	public unsafe static SKData CreateCopy(byte[] bytes, ulong length)
	{
		fixed (byte* src = bytes)
		{
			return GetObject(SkiaApi.sk_data_new_with_copy(src, (IntPtr)(long)length));
		}
	}

	public unsafe static SKData CreateCopy(ReadOnlySpan<byte> bytes)
	{
		fixed (byte* ptr = bytes)
		{
			return CreateCopy((IntPtr)ptr, (ulong)bytes.Length);
		}
	}

	public static SKData Create(int size)
	{
		return GetObject(SkiaApi.sk_data_new_uninitialized((IntPtr)size));
	}

	public static SKData Create(long size)
	{
		return GetObject(SkiaApi.sk_data_new_uninitialized((IntPtr)size));
	}

	public static SKData Create(ulong size)
	{
		if (!PlatformConfiguration.Is64Bit && size > uint.MaxValue)
		{
			throw new ArgumentOutOfRangeException("size", "The size exceeds the size of pointers.");
		}
		return GetObject(SkiaApi.sk_data_new_uninitialized((IntPtr)(long)size));
	}

	public unsafe static SKData Create(string filename)
	{
		if (string.IsNullOrEmpty(filename))
		{
			throw new ArgumentException("The filename cannot be empty.", "filename");
		}
		fixed (byte* path = StringUtilities.GetEncodedText(filename, SKTextEncoding.Utf8, addNull: true))
		{
			return GetObject(SkiaApi.sk_data_new_from_file(path));
		}
	}

	public static SKData Create(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (stream.CanSeek)
		{
			return Create(stream, stream.Length - stream.Position);
		}
		using SKDynamicMemoryWStream sKDynamicMemoryWStream = new SKDynamicMemoryWStream();
		using (SKManagedStream sKManagedStream = new SKManagedStream(stream))
		{
			sKManagedStream.CopyTo(sKDynamicMemoryWStream);
		}
		return sKDynamicMemoryWStream.DetachAsData();
	}

	public static SKData Create(Stream stream, int length)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using SKManagedStream stream2 = new SKManagedStream(stream);
		return Create(stream2, length);
	}

	public static SKData Create(Stream stream, ulong length)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using SKManagedStream stream2 = new SKManagedStream(stream);
		return Create(stream2, length);
	}

	public static SKData Create(Stream stream, long length)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using SKManagedStream stream2 = new SKManagedStream(stream);
		return Create(stream2, length);
	}

	public static SKData Create(SKStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return Create(stream, stream.Length);
	}

	public static SKData Create(SKStream stream, int length)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return GetObject(SkiaApi.sk_data_new_from_stream(stream.Handle, (IntPtr)length));
	}

	public static SKData Create(SKStream stream, ulong length)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return GetObject(SkiaApi.sk_data_new_from_stream(stream.Handle, (IntPtr)(long)length));
	}

	public static SKData Create(SKStream stream, long length)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return GetObject(SkiaApi.sk_data_new_from_stream(stream.Handle, (IntPtr)length));
	}

	public static SKData Create(IntPtr address, int length)
	{
		return Create(address, length, null, null);
	}

	public static SKData Create(IntPtr address, int length, SKDataReleaseDelegate releaseProc)
	{
		return Create(address, length, releaseProc, null);
	}

	public unsafe static SKData Create(IntPtr address, int length, SKDataReleaseDelegate releaseProc, object context)
	{
		SKDataReleaseDelegate managedDel = ((releaseProc != null && context != null) ? ((SKDataReleaseDelegate)delegate(IntPtr addr, object _)
		{
			releaseProc(addr, context);
		}) : releaseProc);
		GCHandle gch;
		IntPtr contextPtr;
		SKDataReleaseProxyDelegate proc = DelegateProxies.Create(managedDel, DelegateProxies.SKDataReleaseDelegateProxy, out gch, out contextPtr);
		return GetObject(SkiaApi.sk_data_new_with_proc((void*)address, (IntPtr)length, proc, (void*)contextPtr));
	}

	internal static SKData FromCString(string str)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(str ?? string.Empty);
		return CreateCopy(bytes, (ulong)(bytes.Length + 1));
	}

	public SKData Subset(ulong offset, ulong length)
	{
		if (!PlatformConfiguration.Is64Bit)
		{
			if (length > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException("length", "The length exceeds the size of pointers.");
			}
			if (offset > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException("offset", "The offset exceeds the size of pointers.");
			}
		}
		return GetObject(SkiaApi.sk_data_new_subset(Handle, (IntPtr)(long)offset, (IntPtr)(long)length));
	}

	public byte[] ToArray()
	{
		byte[] result = AsSpan().ToArray();
		GC.KeepAlive(this);
		return result;
	}

	public Stream AsStream()
	{
		return new SKDataStream(this);
	}

	public Stream AsStream(bool streamDisposesData)
	{
		return new SKDataStream(this, streamDisposesData);
	}

	public unsafe ReadOnlySpan<byte> AsSpan()
	{
		return new ReadOnlySpan<byte>((void*)Data, (int)Size);
	}

	public void SaveTo(Stream target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		IntPtr data = Data;
		long size = Size;
		Utils.RentedArray<byte> rentedArray = Utils.RentArray<byte>(81920);
		try
		{
			long num = size;
			while (num > 0)
			{
				int num2 = (int)Math.Min(81920L, num);
				Marshal.Copy(data, (byte[])rentedArray, 0, num2);
				num -= num2;
				data += num2;
				target.Write((byte[])rentedArray, 0, num2);
			}
			GC.KeepAlive(this);
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	internal static SKData GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKData(h, o));
	}
}
