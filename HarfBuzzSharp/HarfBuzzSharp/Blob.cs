using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

public class Blob : NativeObject
{
	private class StaticBlob : Blob
	{
		public StaticBlob(IntPtr handle)
			: base(handle)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	private static readonly Lazy<Blob> emptyBlob = new Lazy<Blob>(() => new StaticBlob(HarfBuzzApi.hb_blob_get_empty()));

	public static Blob Empty => emptyBlob.Value;

	public int Length => (int)HarfBuzzApi.hb_blob_get_length(Handle);

	public int FaceCount => (int)HarfBuzzApi.hb_face_count(Handle);

	public bool IsImmutable => HarfBuzzApi.hb_blob_is_immutable(Handle);

	internal Blob(IntPtr handle)
		: base(handle)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Blob(IntPtr, int, MemoryMode, ReleaseDelegate) instead.")]
	public Blob(IntPtr data, uint length, MemoryMode mode, object userData, BlobReleaseDelegate releaseDelegate)
		: this(data, (int)length, mode, delegate
		{
			releaseDelegate?.Invoke(userData);
		})
	{
	}

	public Blob(IntPtr data, int length, MemoryMode mode)
		: this(data, length, mode, null)
	{
	}

	public Blob(IntPtr data, int length, MemoryMode mode, ReleaseDelegate releaseDelegate)
		: this(Create(data, length, mode, releaseDelegate))
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeHandler()
	{
		if (Handle != IntPtr.Zero)
		{
			HarfBuzzApi.hb_blob_destroy(Handle);
		}
	}

	public void MakeImmutable()
	{
		HarfBuzzApi.hb_blob_make_immutable(Handle);
	}

	public unsafe Stream AsStream()
	{
		uint num = default(uint);
		void* pointer = HarfBuzzApi.hb_blob_get_data(Handle, &num);
		return new UnmanagedMemoryStream((byte*)pointer, num);
	}

	public unsafe ReadOnlySpan<byte> AsSpan()
	{
		uint length = default(uint);
		void* pointer = HarfBuzzApi.hb_blob_get_data(Handle, &length);
		return new ReadOnlySpan<byte>(pointer, (int)length);
	}

	public static Blob FromFile(string fileName)
	{
		if (!File.Exists(fileName))
		{
			throw new FileNotFoundException("Unable to find file.", fileName);
		}
		IntPtr handle = HarfBuzzApi.hb_blob_create_from_file(fileName);
		return new Blob(handle);
	}

	public unsafe static Blob FromStream(Stream stream)
	{
		MemoryStream ms = new MemoryStream();
		try
		{
			stream.CopyTo(ms);
			byte[] array = ms.ToArray();
			fixed (byte* ptr = array)
			{
				return new Blob((IntPtr)ptr, array.Length, MemoryMode.ReadOnly, delegate
				{
					ms.Dispose();
				});
			}
		}
		finally
		{
			if (ms != null)
			{
				((IDisposable)ms).Dispose();
			}
		}
	}

	private unsafe static IntPtr Create(IntPtr data, int length, MemoryMode mode, ReleaseDelegate releaseProc)
	{
		GCHandle gch;
		IntPtr contextPtr;
		DestroyProxyDelegate destroy = DelegateProxies.Create(releaseProc, DelegateProxies.ReleaseDelegateProxy, out gch, out contextPtr);
		return HarfBuzzApi.hb_blob_create((void*)data, (uint)length, mode, (void*)contextPtr, destroy);
	}
}
