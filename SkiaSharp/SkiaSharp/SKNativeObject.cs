using System;
using System.Threading;

namespace SkiaSharp;

public abstract class SKNativeObject : IDisposable
{
	internal bool fromFinalizer;

	private int isDisposed;

	public virtual IntPtr Handle { get; protected set; }

	protected internal virtual bool OwnsHandle { get; protected set; }

	protected internal bool IgnorePublicDispose { get; set; }

	protected internal bool IsDisposed => isDisposed == 1;

	internal SKNativeObject(IntPtr handle)
		: this(handle, ownsHandle: true)
	{
	}

	internal SKNativeObject(IntPtr handle, bool ownsHandle)
	{
		Handle = handle;
		OwnsHandle = ownsHandle;
	}

	~SKNativeObject()
	{
		fromFinalizer = true;
		Dispose(disposing: false);
	}

	protected virtual void DisposeUnownedManaged()
	{
	}

	protected virtual void DisposeManaged()
	{
	}

	protected virtual void DisposeNative()
	{
	}

	protected virtual void Dispose(bool disposing)
	{
		if (Interlocked.CompareExchange(ref isDisposed, 1, 0) == 0)
		{
			if (disposing)
			{
				DisposeUnownedManaged();
			}
			if (Handle != IntPtr.Zero && OwnsHandle)
			{
				DisposeNative();
			}
			if (disposing)
			{
				DisposeManaged();
			}
			Handle = IntPtr.Zero;
		}
	}

	public void Dispose()
	{
		if (!IgnorePublicDispose)
		{
			DisposeInternal();
		}
	}

	protected internal void DisposeInternal()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
