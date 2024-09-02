using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public class SKManagedWStream : SKAbstractManagedWStream
{
	private Stream stream;

	private readonly bool disposeStream;

	public SKManagedWStream(Stream managedStream)
		: this(managedStream, disposeManagedStream: false)
	{
	}

	public SKManagedWStream(Stream managedStream, bool disposeManagedStream)
		: this(managedStream, disposeManagedStream, owns: true)
	{
	}

	private SKManagedWStream(Stream managedStream, bool disposeManagedStream, bool owns)
		: base(owns)
	{
		stream = managedStream;
		disposeStream = disposeManagedStream;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeManaged()
	{
		if (disposeStream && stream != null)
		{
			stream.Dispose();
			stream = null;
		}
		base.DisposeManaged();
	}

	protected override bool OnWrite(IntPtr buffer, IntPtr size)
	{
		int num = (int)size;
		Utils.RentedArray<byte> rentedArray = Utils.RentArray<byte>(num);
		try
		{
			if (buffer != IntPtr.Zero)
			{
				Marshal.Copy(buffer, (byte[])rentedArray, 0, num);
			}
			stream.Write((byte[])rentedArray, 0, num);
			return true;
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	protected override void OnFlush()
	{
		stream.Flush();
	}

	protected override IntPtr OnBytesWritten()
	{
		return (IntPtr)stream.Position;
	}
}
