using System;
using System.Collections.Generic;
using SkiaSharp.Internals;

namespace SkiaSharp;

internal static class HandleDictionary
{
	private static readonly Type SkipObjectRegistrationType = typeof(ISKSkipObjectRegistration);

	internal static readonly Dictionary<IntPtr, WeakReference> instances = new Dictionary<IntPtr, WeakReference>();

	internal static readonly IPlatformLock instancesLock = PlatformLock.Create();

	internal static bool GetInstance<TSkiaObject>(IntPtr handle, out TSkiaObject instance) where TSkiaObject : SKObject
	{
		if (handle == IntPtr.Zero)
		{
			instance = null;
			return false;
		}
		if (SkipObjectRegistrationType.IsAssignableFrom(typeof(TSkiaObject)))
		{
			instance = null;
			return false;
		}
		instancesLock.EnterReadLock();
		try
		{
			return GetInstanceNoLocks<TSkiaObject>(handle, out instance);
		}
		finally
		{
			instancesLock.ExitReadLock();
		}
	}

	internal static TSkiaObject GetOrAddObject<TSkiaObject>(IntPtr handle, bool owns, bool unrefExisting, Func<IntPtr, bool, TSkiaObject> objectFactory) where TSkiaObject : SKObject
	{
		if (handle == IntPtr.Zero)
		{
			return null;
		}
		if (SkipObjectRegistrationType.IsAssignableFrom(typeof(TSkiaObject)))
		{
			return objectFactory(handle, owns);
		}
		instancesLock.EnterUpgradeableReadLock();
		try
		{
			if (GetInstanceNoLocks<TSkiaObject>(handle, out var instance))
			{
				if (unrefExisting && instance is ISKReferenceCounted obj)
				{
					obj.SafeUnRef();
				}
				return instance;
			}
			return objectFactory(handle, owns);
		}
		finally
		{
			instancesLock.ExitUpgradeableReadLock();
		}
	}

	private static bool GetInstanceNoLocks<TSkiaObject>(IntPtr handle, out TSkiaObject instance) where TSkiaObject : SKObject
	{
		if (instances.TryGetValue(handle, out var value) && value.IsAlive && value.Target is TSkiaObject val && !val.IsDisposed)
		{
			instance = val;
			return true;
		}
		instance = null;
		return false;
	}

	internal static void RegisterHandle(IntPtr handle, SKObject instance)
	{
		if (handle == IntPtr.Zero || instance == null || instance is ISKSkipObjectRegistration)
		{
			return;
		}
		SKObject sKObject = null;
		instancesLock.EnterWriteLock();
		try
		{
			if (instances.TryGetValue(handle, out var value) && value.Target is SKObject { IsDisposed: false } sKObject2)
			{
				sKObject = sKObject2;
			}
			instances[handle] = new WeakReference(instance);
		}
		finally
		{
			instancesLock.ExitWriteLock();
		}
		sKObject?.DisposeInternal();
	}

	internal static void DeregisterHandle(IntPtr handle, SKObject instance)
	{
		if (handle == IntPtr.Zero || instance is ISKSkipObjectRegistration)
		{
			return;
		}
		instancesLock.EnterWriteLock();
		try
		{
			if (instances.TryGetValue(handle, out var value) && (!value.IsAlive || value.Target == instance))
			{
				instances.Remove(handle);
			}
		}
		finally
		{
			instancesLock.ExitWriteLock();
		}
	}
}
