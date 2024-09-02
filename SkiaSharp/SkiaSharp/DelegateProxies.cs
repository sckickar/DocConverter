using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SkiaSharp;

internal static class DelegateProxies
{
	public unsafe static readonly SKBitmapReleaseProxyDelegate SKBitmapReleaseDelegateProxy = SKBitmapReleaseDelegateProxyImplementation;

	public unsafe static readonly SKDataReleaseProxyDelegate SKDataReleaseDelegateProxy = SKDataReleaseDelegateProxyImplementation;

	public unsafe static readonly SKImageRasterReleaseProxyDelegate SKImageRasterReleaseDelegateProxy = SKImageRasterReleaseDelegateProxyImplementation;

	public unsafe static readonly SKImageRasterReleaseProxyDelegate SKImageRasterReleaseDelegateProxyForCoTaskMem = SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem;

	public unsafe static readonly SKImageTextureReleaseProxyDelegate SKImageTextureReleaseDelegateProxy = SKImageTextureReleaseDelegateProxyImplementation;

	public unsafe static readonly SKSurfaceRasterReleaseProxyDelegate SKSurfaceReleaseDelegateProxy = SKSurfaceReleaseDelegateProxyImplementation;

	public unsafe static readonly GRGlGetProcProxyDelegate GRGlGetProcDelegateProxy = GRGlGetProcDelegateProxyImplementation;

	public unsafe static readonly GRVkGetProcProxyDelegate GRVkGetProcDelegateProxy = GRVkGetProcDelegateProxyImplementation;

	public unsafe static readonly SKGlyphPathProxyDelegate SKGlyphPathDelegateProxy = SKGlyphPathDelegateProxyImplementation;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Create<T>(object managedDel, T nativeDel, out GCHandle gch, out IntPtr contextPtr)
	{
		if (managedDel == null)
		{
			gch = default(GCHandle);
			contextPtr = IntPtr.Zero;
			return default(T);
		}
		gch = GCHandle.Alloc(managedDel);
		contextPtr = GCHandle.ToIntPtr(gch);
		return nativeDel;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Create(object managedDel, out GCHandle gch, out IntPtr contextPtr)
	{
		if (managedDel == null)
		{
			gch = default(GCHandle);
			contextPtr = IntPtr.Zero;
		}
		else
		{
			gch = GCHandle.Alloc(managedDel);
			contextPtr = GCHandle.ToIntPtr(gch);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Get<T>(IntPtr contextPtr, out GCHandle gch)
	{
		if (contextPtr == IntPtr.Zero)
		{
			gch = default(GCHandle);
			return default(T);
		}
		gch = GCHandle.FromIntPtr(contextPtr);
		return (T)gch.Target;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntPtr CreateUserData(object userData, bool makeWeak = false)
	{
		userData = (makeWeak ? new WeakReference(userData) : userData);
		UserDataDelegate managedDel = () => userData;
		Create(managedDel, out var _, out var contextPtr);
		return contextPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetUserData<T>(IntPtr contextPtr, out GCHandle gch)
	{
		UserDataDelegate userDataDelegate = Get<UserDataDelegate>(contextPtr, out gch);
		object obj = userDataDelegate();
		if (!(obj is WeakReference weakReference))
		{
			return (T)obj;
		}
		return (T)weakReference.Target;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntPtr CreateMulti<T1, T2>(T1 wrappedDelegate1, T2 wrappedDelegate2) where T1 : Delegate where T2 : Delegate
	{
		GetMultiDelegateDelegate managedDel = delegate(Type type)
		{
			if (type == typeof(T1))
			{
				return wrappedDelegate1;
			}
			if (type == typeof(T2))
			{
				return wrappedDelegate2;
			}
			throw new ArgumentOutOfRangeException("type");
		};
		Create(managedDel, out var _, out var contextPtr);
		return contextPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntPtr CreateMulti<T1, T2, T3>(T1 wrappedDelegate1, T2 wrappedDelegate2, T3 wrappedDelegate3) where T1 : Delegate where T2 : Delegate where T3 : Delegate
	{
		GetMultiDelegateDelegate managedDel = delegate(Type type)
		{
			if (type == typeof(T1))
			{
				return wrappedDelegate1;
			}
			if (type == typeof(T2))
			{
				return wrappedDelegate2;
			}
			if (type == typeof(T3))
			{
				return wrappedDelegate3;
			}
			throw new ArgumentOutOfRangeException("type");
		};
		Create(managedDel, out var _, out var contextPtr);
		return contextPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetMulti<T>(IntPtr contextPtr, out GCHandle gch) where T : Delegate
	{
		GetMultiDelegateDelegate getMultiDelegateDelegate = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		return (T)getMultiDelegateDelegate(typeof(T));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetMulti<T1, T2>(IntPtr contextPtr, out T1 wrappedDelegate1, out T2 wrappedDelegate2, out GCHandle gch) where T1 : Delegate where T2 : Delegate
	{
		GetMultiDelegateDelegate getMultiDelegateDelegate = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		wrappedDelegate1 = (T1)getMultiDelegateDelegate(typeof(T1));
		wrappedDelegate2 = (T2)getMultiDelegateDelegate(typeof(T2));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetMulti<T1, T2, T3>(IntPtr contextPtr, out T1 wrappedDelegate1, out T2 wrappedDelegate2, out T3 wrappedDelegate3, out GCHandle gch) where T1 : Delegate where T2 : Delegate where T3 : Delegate
	{
		GetMultiDelegateDelegate getMultiDelegateDelegate = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		wrappedDelegate1 = (T1)getMultiDelegateDelegate(typeof(T1));
		wrappedDelegate2 = (T2)getMultiDelegateDelegate(typeof(T2));
		wrappedDelegate3 = (T3)getMultiDelegateDelegate(typeof(T3));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntPtr CreateMultiUserData<T>(T wrappedDelegate, object userData, bool makeWeak = false) where T : Delegate
	{
		userData = (makeWeak ? new WeakReference(userData) : userData);
		UserDataDelegate userDataDelegate = () => userData;
		GetMultiDelegateDelegate managedDel = delegate(Type type)
		{
			if (type == typeof(T))
			{
				return wrappedDelegate;
			}
			if (type == typeof(UserDataDelegate))
			{
				return userDataDelegate;
			}
			throw new ArgumentOutOfRangeException("type");
		};
		Create(managedDel, out var _, out var contextPtr);
		return contextPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntPtr CreateMultiUserData<T1, T2>(T1 wrappedDelegate1, T2 wrappedDelegate2, object userData, bool makeWeak = false) where T1 : Delegate where T2 : Delegate
	{
		userData = (makeWeak ? new WeakReference(userData) : userData);
		UserDataDelegate userDataDelegate = () => userData;
		GetMultiDelegateDelegate managedDel = delegate(Type type)
		{
			if (type == typeof(T1))
			{
				return wrappedDelegate1;
			}
			if (type == typeof(T2))
			{
				return wrappedDelegate2;
			}
			if (type == typeof(UserDataDelegate))
			{
				return userDataDelegate;
			}
			throw new ArgumentOutOfRangeException("type");
		};
		Create(managedDel, out var _, out var contextPtr);
		return contextPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntPtr CreateMultiUserData<T1, T2, T3>(T1 wrappedDelegate1, T2 wrappedDelegate2, T3 wrappedDelegate3, object userData, bool makeWeak = false) where T1 : Delegate where T2 : Delegate where T3 : Delegate
	{
		userData = (makeWeak ? new WeakReference(userData) : userData);
		UserDataDelegate userDataDelegate = () => userData;
		GetMultiDelegateDelegate managedDel = delegate(Type type)
		{
			if (type == typeof(T1))
			{
				return wrappedDelegate1;
			}
			if (type == typeof(T2))
			{
				return wrappedDelegate2;
			}
			if (type == typeof(T3))
			{
				return wrappedDelegate3;
			}
			if (type == typeof(UserDataDelegate))
			{
				return userDataDelegate;
			}
			throw new ArgumentOutOfRangeException("type");
		};
		Create(managedDel, out var _, out var contextPtr);
		return contextPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TUserData GetMultiUserData<TUserData>(IntPtr contextPtr, out GCHandle gch)
	{
		GetMultiDelegateDelegate multi = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		return GetUserData<TUserData>(multi);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetMultiUserData<T, TUserData>(IntPtr contextPtr, out T wrappedDelegate, out TUserData userData, out GCHandle gch) where T : Delegate
	{
		GetMultiDelegateDelegate getMultiDelegateDelegate = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		wrappedDelegate = (T)getMultiDelegateDelegate(typeof(T));
		userData = GetUserData<TUserData>(getMultiDelegateDelegate);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetMultiUserData<T1, T2, TUserData>(IntPtr contextPtr, out T1 wrappedDelegate1, out T2 wrappedDelegate2, out TUserData userData, out GCHandle gch) where T1 : Delegate where T2 : Delegate
	{
		GetMultiDelegateDelegate getMultiDelegateDelegate = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		wrappedDelegate1 = (T1)getMultiDelegateDelegate(typeof(T1));
		wrappedDelegate2 = (T2)getMultiDelegateDelegate(typeof(T2));
		userData = GetUserData<TUserData>(getMultiDelegateDelegate);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetMultiUserData<T1, T2, T3, TUserData>(IntPtr contextPtr, out T1 wrappedDelegate1, out T2 wrappedDelegate2, out T3 wrappedDelegate3, out TUserData userData, out GCHandle gch) where T1 : Delegate where T2 : Delegate where T3 : Delegate
	{
		GetMultiDelegateDelegate getMultiDelegateDelegate = Get<GetMultiDelegateDelegate>(contextPtr, out gch);
		wrappedDelegate1 = (T1)getMultiDelegateDelegate(typeof(T1));
		wrappedDelegate2 = (T2)getMultiDelegateDelegate(typeof(T2));
		wrappedDelegate3 = (T3)getMultiDelegateDelegate(typeof(T3));
		userData = GetUserData<TUserData>(getMultiDelegateDelegate);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static TUserData GetUserData<TUserData>(GetMultiDelegateDelegate multi)
	{
		UserDataDelegate userDataDelegate = (UserDataDelegate)multi(typeof(UserDataDelegate));
		object obj = userDataDelegate();
		if (!(obj is WeakReference weakReference))
		{
			return (TUserData)obj;
		}
		return (TUserData)weakReference.Target;
	}

	[MonoPInvokeCallback(typeof(SKBitmapReleaseProxyDelegate))]
	private unsafe static void SKBitmapReleaseDelegateProxyImplementation(void* address, void* context)
	{
		GCHandle gch;
		SKBitmapReleaseDelegate sKBitmapReleaseDelegate = Get<SKBitmapReleaseDelegate>((IntPtr)context, out gch);
		try
		{
			sKBitmapReleaseDelegate((IntPtr)address, null);
		}
		finally
		{
			gch.Free();
		}
	}

	[MonoPInvokeCallback(typeof(SKDataReleaseProxyDelegate))]
	private unsafe static void SKDataReleaseDelegateProxyImplementation(void* address, void* context)
	{
		GCHandle gch;
		SKDataReleaseDelegate sKDataReleaseDelegate = Get<SKDataReleaseDelegate>((IntPtr)context, out gch);
		try
		{
			sKDataReleaseDelegate((IntPtr)address, null);
		}
		finally
		{
			gch.Free();
		}
	}

	[MonoPInvokeCallback(typeof(SKImageRasterReleaseProxyDelegate))]
	private unsafe static void SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem(void* pixels, void* context)
	{
		Marshal.FreeCoTaskMem((IntPtr)pixels);
	}

	[MonoPInvokeCallback(typeof(SKImageRasterReleaseProxyDelegate))]
	private unsafe static void SKImageRasterReleaseDelegateProxyImplementation(void* pixels, void* context)
	{
		GCHandle gch;
		SKImageRasterReleaseDelegate sKImageRasterReleaseDelegate = Get<SKImageRasterReleaseDelegate>((IntPtr)context, out gch);
		try
		{
			sKImageRasterReleaseDelegate((IntPtr)pixels, null);
		}
		finally
		{
			gch.Free();
		}
	}

	[MonoPInvokeCallback(typeof(SKImageTextureReleaseProxyDelegate))]
	private unsafe static void SKImageTextureReleaseDelegateProxyImplementation(void* context)
	{
		GCHandle gch;
		SKImageTextureReleaseDelegate sKImageTextureReleaseDelegate = Get<SKImageTextureReleaseDelegate>((IntPtr)context, out gch);
		try
		{
			sKImageTextureReleaseDelegate(null);
		}
		finally
		{
			gch.Free();
		}
	}

	[MonoPInvokeCallback(typeof(SKSurfaceRasterReleaseProxyDelegate))]
	private unsafe static void SKSurfaceReleaseDelegateProxyImplementation(void* address, void* context)
	{
		GCHandle gch;
		SKSurfaceReleaseDelegate sKSurfaceReleaseDelegate = Get<SKSurfaceReleaseDelegate>((IntPtr)context, out gch);
		try
		{
			sKSurfaceReleaseDelegate((IntPtr)address, null);
		}
		finally
		{
			gch.Free();
		}
	}

	[MonoPInvokeCallback(typeof(GRGlGetProcProxyDelegate))]
	private unsafe static IntPtr GRGlGetProcDelegateProxyImplementation(void* context, void* name)
	{
		GCHandle gch;
		GRGlGetProcedureAddressDelegate gRGlGetProcedureAddressDelegate = Get<GRGlGetProcedureAddressDelegate>((IntPtr)context, out gch);
		return gRGlGetProcedureAddressDelegate(Marshal.PtrToStringAnsi((IntPtr)name));
	}

	[MonoPInvokeCallback(typeof(GRVkGetProcProxyDelegate))]
	private unsafe static IntPtr GRVkGetProcDelegateProxyImplementation(void* context, void* name, IntPtr instance, IntPtr device)
	{
		GCHandle gch;
		GRVkGetProcedureAddressDelegate gRVkGetProcedureAddressDelegate = Get<GRVkGetProcedureAddressDelegate>((IntPtr)context, out gch);
		return gRVkGetProcedureAddressDelegate(Marshal.PtrToStringAnsi((IntPtr)name), instance, device);
	}

	[MonoPInvokeCallback(typeof(SKGlyphPathProxyDelegate))]
	private unsafe static void SKGlyphPathDelegateProxyImplementation(IntPtr pathOrNull, SKMatrix* matrix, void* context)
	{
		GCHandle gch;
		SKGlyphPathDelegate sKGlyphPathDelegate = Get<SKGlyphPathDelegate>((IntPtr)context, out gch);
		SKPath @object = SKPath.GetObject(pathOrNull, owns: false);
		sKGlyphPathDelegate(@object, *matrix);
	}
}
