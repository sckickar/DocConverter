using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public class SKFrontBufferedManagedStream : SKAbstractManagedStream
{
	private SKStream stream;

	private bool disposeStream;

	private readonly bool hasLength;

	private readonly int streamLength;

	private readonly int bufferLength;

	private byte[] frontBuffer;

	private int bufferedSoFar;

	private int offset;

	public SKFrontBufferedManagedStream(Stream managedStream, int bufferSize)
		: this(managedStream, bufferSize, disposeUnderlyingStream: false)
	{
	}

	public SKFrontBufferedManagedStream(Stream managedStream, int bufferSize, bool disposeUnderlyingStream)
		: this(new SKManagedStream(managedStream, disposeUnderlyingStream), bufferSize, disposeUnderlyingStream: true)
	{
	}

	public SKFrontBufferedManagedStream(SKStream nativeStream, int bufferSize)
		: this(nativeStream, bufferSize, disposeUnderlyingStream: false)
	{
	}

	public SKFrontBufferedManagedStream(SKStream nativeStream, int bufferSize, bool disposeUnderlyingStream)
	{
		int num = (nativeStream.HasLength ? nativeStream.Length : 0);
		int num2 = (nativeStream.HasPosition ? nativeStream.Position : 0);
		disposeStream = disposeUnderlyingStream;
		stream = nativeStream;
		hasLength = nativeStream.HasPosition && nativeStream.HasLength;
		streamLength = num - num2;
		offset = 0;
		bufferedSoFar = 0;
		bufferLength = bufferSize;
		frontBuffer = new byte[bufferSize];
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

	protected override IntPtr OnRead(IntPtr buffer, IntPtr size)
	{
		int num = offset;
		if ((int)size > 0 && offset < bufferedSoFar)
		{
			int num2 = Math.Min((int)size, bufferedSoFar - offset);
			if (buffer != IntPtr.Zero)
			{
				Marshal.Copy(frontBuffer, offset, buffer, num2);
				buffer += num2;
			}
			offset += num2;
			size -= num2;
		}
		bool flag = false;
		if ((int)size > 0 && bufferedSoFar < bufferLength)
		{
			int num3 = Math.Min((int)size, bufferLength - bufferedSoFar);
			IntPtr intPtr = Marshal.AllocCoTaskMem(num3);
			int num4 = stream.Read(intPtr, num3);
			Marshal.Copy(intPtr, frontBuffer, offset, num4);
			Marshal.FreeCoTaskMem(intPtr);
			flag = num4 < num3;
			bufferedSoFar += num4;
			if (buffer != IntPtr.Zero)
			{
				Marshal.Copy(frontBuffer, offset, buffer, num4);
				buffer += num4;
			}
			offset += num4;
			size -= num4;
		}
		if ((int)size > 0 && !flag)
		{
			int num5 = stream.Read(buffer, (int)size);
			if (num5 > 0)
			{
				frontBuffer = null;
			}
			offset += num5;
			size -= num5;
		}
		return (IntPtr)(offset - num);
	}

	protected override IntPtr OnPeek(IntPtr buffer, IntPtr size)
	{
		if (offset >= bufferLength)
		{
			return (IntPtr)0;
		}
		int num = offset;
		int size2 = Math.Min((int)size, bufferLength - offset);
		int num2 = Read(buffer, size2);
		offset = num;
		return (IntPtr)num2;
	}

	protected override bool OnIsAtEnd()
	{
		if (offset < bufferedSoFar)
		{
			return false;
		}
		return stream.IsAtEnd;
	}

	protected override bool OnRewind()
	{
		if (offset <= bufferLength)
		{
			offset = 0;
			return true;
		}
		return false;
	}

	protected override bool OnHasLength()
	{
		return hasLength;
	}

	protected override IntPtr OnGetLength()
	{
		return (IntPtr)streamLength;
	}

	protected override bool OnHasPosition()
	{
		return false;
	}

	protected override IntPtr OnGetPosition()
	{
		return (IntPtr)0;
	}

	protected override bool OnSeek(IntPtr position)
	{
		return false;
	}

	protected override bool OnMove(int offset)
	{
		return false;
	}

	protected override IntPtr OnCreateNew()
	{
		return IntPtr.Zero;
	}
}
