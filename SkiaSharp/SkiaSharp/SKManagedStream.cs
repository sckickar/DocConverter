using System;
using System.IO;

namespace SkiaSharp;

public class SKManagedStream : SKAbstractManagedStream
{
	private Stream stream;

	private bool isAsEnd;

	private bool disposeStream;

	private bool wasCopied;

	private WeakReference parent;

	private WeakReference child;

	public SKManagedStream(Stream managedStream)
		: this(managedStream, disposeManagedStream: false)
	{
	}

	public SKManagedStream(Stream managedStream, bool disposeManagedStream)
		: base(owns: true)
	{
		stream = managedStream ?? throw new ArgumentNullException("managedStream");
		disposeStream = disposeManagedStream;
	}

	public int CopyTo(SKWStream destination)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		int num = 0;
		Utils.RentedArray<byte> rentedArray = Utils.RentArray<byte>(81920);
		try
		{
			int num2;
			while ((num2 = stream.Read((byte[])rentedArray, 0, rentedArray.Length)) > 0)
			{
				destination.Write((byte[])rentedArray, num2);
				num += num2;
			}
			destination.Flush();
			return num;
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	public SKStreamAsset ToMemoryStream()
	{
		using SKDynamicMemoryWStream sKDynamicMemoryWStream = new SKDynamicMemoryWStream();
		CopyTo(sKDynamicMemoryWStream);
		return sKDynamicMemoryWStream.DetachAsStream();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeManaged()
	{
		SKManagedStream sKManagedStream = child?.Target as SKManagedStream;
		SKManagedStream sKManagedStream2 = parent?.Target as SKManagedStream;
		if (sKManagedStream != null && sKManagedStream2 != null)
		{
			sKManagedStream.parent = parent;
			sKManagedStream2.child = child;
		}
		else if (sKManagedStream != null)
		{
			sKManagedStream.parent = null;
		}
		else if (sKManagedStream2 != null)
		{
			sKManagedStream2.child = null;
			sKManagedStream2.wasCopied = false;
			sKManagedStream2.disposeStream = disposeStream;
			disposeStream = false;
		}
		parent = null;
		child = null;
		if (disposeStream && stream != null)
		{
			stream.Dispose();
			stream = null;
		}
		base.DisposeManaged();
	}

	private IntPtr OnReadManagedStream(IntPtr buffer, IntPtr size)
	{
		if ((int)size < 0)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		if (size == IntPtr.Zero)
		{
			return IntPtr.Zero;
		}
		Utils.RentedArray<byte> rentedArray = Utils.RentArray<byte>((int)size);
		try
		{
			int num = stream.Read(rentedArray.Array, 0, rentedArray.Length);
			if (buffer != IntPtr.Zero)
			{
				Span<byte> span = rentedArray.Span.Slice(0, num);
				Span<byte> destination = buffer.AsSpan(rentedArray.Length);
				span.CopyTo(destination);
			}
			if (!stream.CanSeek && (int)size > 0 && num <= (int)size)
			{
				isAsEnd = true;
			}
			return (IntPtr)num;
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	protected override IntPtr OnRead(IntPtr buffer, IntPtr size)
	{
		VerifyOriginal();
		return OnReadManagedStream(buffer, size);
	}

	protected override IntPtr OnPeek(IntPtr buffer, IntPtr size)
	{
		VerifyOriginal();
		if (!stream.CanSeek)
		{
			return (IntPtr)0;
		}
		long position = stream.Position;
		IntPtr result = OnReadManagedStream(buffer, size);
		stream.Position = position;
		return result;
	}

	protected override bool OnIsAtEnd()
	{
		VerifyOriginal();
		if (!stream.CanSeek)
		{
			return isAsEnd;
		}
		return stream.Position >= stream.Length;
	}

	protected override bool OnHasPosition()
	{
		VerifyOriginal();
		return stream.CanSeek;
	}

	protected override bool OnHasLength()
	{
		VerifyOriginal();
		return stream.CanSeek;
	}

	protected override bool OnRewind()
	{
		VerifyOriginal();
		if (!stream.CanSeek)
		{
			return false;
		}
		stream.Position = 0L;
		return true;
	}

	protected override IntPtr OnGetPosition()
	{
		VerifyOriginal();
		if (!stream.CanSeek)
		{
			return (IntPtr)0;
		}
		return (IntPtr)stream.Position;
	}

	protected override IntPtr OnGetLength()
	{
		VerifyOriginal();
		if (!stream.CanSeek)
		{
			return (IntPtr)0;
		}
		return (IntPtr)stream.Length;
	}

	protected override bool OnSeek(IntPtr position)
	{
		VerifyOriginal();
		if (!stream.CanSeek)
		{
			return false;
		}
		stream.Position = (long)position;
		return true;
	}

	protected override bool OnMove(int offset)
	{
		VerifyOriginal();
		if (!stream.CanSeek)
		{
			return false;
		}
		stream.Position += offset;
		return true;
	}

	protected override IntPtr OnCreateNew()
	{
		VerifyOriginal();
		return IntPtr.Zero;
	}

	protected override IntPtr OnDuplicate()
	{
		VerifyOriginal();
		if (!stream.CanSeek)
		{
			return IntPtr.Zero;
		}
		SKManagedStream sKManagedStream = new SKManagedStream(stream, disposeStream);
		sKManagedStream.parent = new WeakReference(this);
		wasCopied = true;
		disposeStream = false;
		child = new WeakReference(sKManagedStream);
		stream.Position = 0L;
		return sKManagedStream.Handle;
	}

	protected override IntPtr OnFork()
	{
		VerifyOriginal();
		SKManagedStream sKManagedStream = new SKManagedStream(stream, disposeStream);
		wasCopied = true;
		disposeStream = false;
		return sKManagedStream.Handle;
	}

	private void VerifyOriginal()
	{
		if (wasCopied)
		{
			throw new InvalidOperationException("This stream was duplicated or forked and cannot be read anymore.");
		}
	}
}
