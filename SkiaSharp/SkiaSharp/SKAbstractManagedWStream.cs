using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp;

public abstract class SKAbstractManagedWStream : SKWStream
{
	private static readonly SKManagedWStreamDelegates delegates;

	private int fromNative;

	unsafe static SKAbstractManagedWStream()
	{
		delegates = new SKManagedWStreamDelegates
		{
			fWrite = WriteInternal,
			fFlush = FlushInternal,
			fBytesWritten = BytesWrittenInternal,
			fDestroy = DestroyInternal
		};
		SkiaApi.sk_managedwstream_set_procs(delegates);
	}

	protected SKAbstractManagedWStream()
		: this(owns: true)
	{
	}

	protected unsafe SKAbstractManagedWStream(bool owns)
		: base(IntPtr.Zero, owns)
	{
		IntPtr intPtr = DelegateProxies.CreateUserData(this, makeWeak: true);
		Handle = SkiaApi.sk_managedwstream_new((void*)intPtr);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		if (Interlocked.CompareExchange(ref fromNative, 0, 0) == 0)
		{
			SkiaApi.sk_managedwstream_destroy(Handle);
		}
	}

	protected abstract bool OnWrite(IntPtr buffer, IntPtr size);

	protected abstract void OnFlush();

	protected abstract IntPtr OnBytesWritten();

	[MonoPInvokeCallback(typeof(SKManagedWStreamWriteProxyDelegate))]
	private unsafe static bool WriteInternal(IntPtr s, void* context, void* buffer, IntPtr size)
	{
		GCHandle gch;
		SKAbstractManagedWStream userData = DelegateProxies.GetUserData<SKAbstractManagedWStream>((IntPtr)context, out gch);
		return userData.OnWrite((IntPtr)buffer, size);
	}

	[MonoPInvokeCallback(typeof(SKManagedWStreamFlushProxyDelegate))]
	private unsafe static void FlushInternal(IntPtr s, void* context)
	{
		GCHandle gch;
		SKAbstractManagedWStream userData = DelegateProxies.GetUserData<SKAbstractManagedWStream>((IntPtr)context, out gch);
		userData.OnFlush();
	}

	[MonoPInvokeCallback(typeof(SKManagedWStreamBytesWrittenProxyDelegate))]
	private unsafe static IntPtr BytesWrittenInternal(IntPtr s, void* context)
	{
		GCHandle gch;
		SKAbstractManagedWStream userData = DelegateProxies.GetUserData<SKAbstractManagedWStream>((IntPtr)context, out gch);
		return userData.OnBytesWritten();
	}

	[MonoPInvokeCallback(typeof(SKManagedWStreamDestroyProxyDelegate))]
	private unsafe static void DestroyInternal(IntPtr s, void* context)
	{
		GCHandle gch;
		SKAbstractManagedWStream userData = DelegateProxies.GetUserData<SKAbstractManagedWStream>((IntPtr)context, out gch);
		if (userData != null)
		{
			Interlocked.Exchange(ref userData.fromNative, 1);
			userData.Dispose();
		}
		gch.Free();
	}
}
