using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp;

public abstract class SKAbstractManagedStream : SKStreamAsset
{
	private static readonly SKManagedStreamDelegates delegates;

	private int fromNative;

	unsafe static SKAbstractManagedStream()
	{
		delegates = new SKManagedStreamDelegates
		{
			fRead = ReadInternal,
			fPeek = PeekInternal,
			fIsAtEnd = IsAtEndInternal,
			fHasPosition = HasPositionInternal,
			fHasLength = HasLengthInternal,
			fRewind = RewindInternal,
			fGetPosition = GetPositionInternal,
			fSeek = SeekInternal,
			fMove = MoveInternal,
			fGetLength = GetLengthInternal,
			fDuplicate = DuplicateInternal,
			fFork = ForkInternal,
			fDestroy = DestroyInternal
		};
		SkiaApi.sk_managedstream_set_procs(delegates);
	}

	protected SKAbstractManagedStream()
		: this(owns: true)
	{
	}

	protected unsafe SKAbstractManagedStream(bool owns)
		: base(IntPtr.Zero, owns)
	{
		IntPtr intPtr = DelegateProxies.CreateUserData(this, makeWeak: true);
		Handle = SkiaApi.sk_managedstream_new((void*)intPtr);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		if (Interlocked.CompareExchange(ref fromNative, 0, 0) == 0)
		{
			SkiaApi.sk_managedstream_destroy(Handle);
		}
	}

	protected abstract IntPtr OnRead(IntPtr buffer, IntPtr size);

	protected abstract IntPtr OnPeek(IntPtr buffer, IntPtr size);

	protected abstract bool OnIsAtEnd();

	protected abstract bool OnHasPosition();

	protected abstract bool OnHasLength();

	protected abstract bool OnRewind();

	protected abstract IntPtr OnGetPosition();

	protected abstract IntPtr OnGetLength();

	protected abstract bool OnSeek(IntPtr position);

	protected abstract bool OnMove(int offset);

	protected abstract IntPtr OnCreateNew();

	protected virtual IntPtr OnFork()
	{
		IntPtr intPtr = OnCreateNew();
		SkiaApi.sk_stream_seek(intPtr, SkiaApi.sk_stream_get_position(Handle));
		return intPtr;
	}

	protected virtual IntPtr OnDuplicate()
	{
		return OnCreateNew();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamReadProxyDelegate))]
	private unsafe static IntPtr ReadInternal(IntPtr s, void* context, void* buffer, IntPtr size)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		return userData.OnRead((IntPtr)buffer, size);
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamPeekProxyDelegate))]
	private unsafe static IntPtr PeekInternal(IntPtr s, void* context, void* buffer, IntPtr size)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		return userData.OnPeek((IntPtr)buffer, size);
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamIsAtEndProxyDelegate))]
	private unsafe static bool IsAtEndInternal(IntPtr s, void* context)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		return userData.OnIsAtEnd();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamHasPositionProxyDelegate))]
	private unsafe static bool HasPositionInternal(IntPtr s, void* context)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		return userData.OnHasPosition();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamHasLengthProxyDelegate))]
	private unsafe static bool HasLengthInternal(IntPtr s, void* context)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		return userData.OnHasLength();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamRewindProxyDelegate))]
	private unsafe static bool RewindInternal(IntPtr s, void* context)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		return userData.OnRewind();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamGetPositionProxyDelegate))]
	private unsafe static IntPtr GetPositionInternal(IntPtr s, void* context)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		return userData.OnGetPosition();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamSeekProxyDelegate))]
	private unsafe static bool SeekInternal(IntPtr s, void* context, IntPtr position)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		return userData.OnSeek(position);
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamMoveProxyDelegate))]
	private unsafe static bool MoveInternal(IntPtr s, void* context, int offset)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		return userData.OnMove(offset);
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamGetLengthProxyDelegate))]
	private unsafe static IntPtr GetLengthInternal(IntPtr s, void* context)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		return userData.OnGetLength();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamDuplicateProxyDelegate))]
	private unsafe static IntPtr DuplicateInternal(IntPtr s, void* context)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		return userData.OnDuplicate();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamForkProxyDelegate))]
	private unsafe static IntPtr ForkInternal(IntPtr s, void* context)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		return userData.OnFork();
	}

	[MonoPInvokeCallback(typeof(SKManagedStreamDestroyProxyDelegate))]
	private unsafe static void DestroyInternal(IntPtr s, void* context)
	{
		GCHandle gch;
		SKAbstractManagedStream userData = DelegateProxies.GetUserData<SKAbstractManagedStream>((IntPtr)context, out gch);
		if (userData != null)
		{
			Interlocked.Exchange(ref userData.fromNative, 1);
			userData.Dispose();
		}
		gch.Free();
	}
}
