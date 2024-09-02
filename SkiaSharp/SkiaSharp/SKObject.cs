using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public abstract class SKObject : SKNativeObject
{
	private readonly object locker = new object();

	private ConcurrentDictionary<IntPtr, SKObject> ownedObjects;

	private ConcurrentDictionary<IntPtr, SKObject> keepAliveObjects;

	internal ConcurrentDictionary<IntPtr, SKObject> OwnedObjects
	{
		get
		{
			if (ownedObjects == null)
			{
				lock (locker)
				{
					if (ownedObjects == null)
					{
						ownedObjects = new ConcurrentDictionary<IntPtr, SKObject>();
					}
				}
			}
			return ownedObjects;
		}
	}

	internal ConcurrentDictionary<IntPtr, SKObject> KeepAliveObjects
	{
		get
		{
			if (keepAliveObjects == null)
			{
				lock (locker)
				{
					if (keepAliveObjects == null)
					{
						keepAliveObjects = new ConcurrentDictionary<IntPtr, SKObject>();
					}
				}
			}
			return keepAliveObjects;
		}
	}

	public override IntPtr Handle
	{
		get
		{
			return base.Handle;
		}
		protected set
		{
			if (value == IntPtr.Zero)
			{
				DeregisterHandle(Handle, this);
				base.Handle = value;
			}
			else
			{
				base.Handle = value;
				RegisterHandle(Handle, this);
			}
		}
	}

	static SKObject()
	{
		SkiaSharpVersion.CheckNativeLibraryCompatible(throwIfIncompatible: true);
		SKColorSpace.EnsureStaticInstanceAreInitialized();
		SKData.EnsureStaticInstanceAreInitialized();
		SKFontManager.EnsureStaticInstanceAreInitialized();
		SKTypeface.EnsureStaticInstanceAreInitialized();
	}

	internal SKObject(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeUnownedManaged()
	{
		if (ownedObjects == null)
		{
			return;
		}
		foreach (KeyValuePair<IntPtr, SKObject> ownedObject in ownedObjects)
		{
			SKObject value = ownedObject.Value;
			if (value != null && !value.OwnsHandle)
			{
				value.DisposeInternal();
			}
		}
	}

	protected override void DisposeManaged()
	{
		if (ownedObjects != null)
		{
			foreach (KeyValuePair<IntPtr, SKObject> ownedObject in ownedObjects)
			{
				SKObject value = ownedObject.Value;
				if (value != null && value.OwnsHandle)
				{
					value.DisposeInternal();
				}
			}
			ownedObjects.Clear();
		}
		keepAliveObjects?.Clear();
	}

	protected override void DisposeNative()
	{
		if (this is ISKReferenceCounted obj)
		{
			obj.SafeUnRef();
		}
	}

	internal static TSkiaObject GetOrAddObject<TSkiaObject>(IntPtr handle, Func<IntPtr, bool, TSkiaObject> objectFactory) where TSkiaObject : SKObject
	{
		if (handle == IntPtr.Zero)
		{
			return null;
		}
		return HandleDictionary.GetOrAddObject(handle, owns: true, unrefExisting: true, objectFactory);
	}

	internal static TSkiaObject GetOrAddObject<TSkiaObject>(IntPtr handle, bool owns, Func<IntPtr, bool, TSkiaObject> objectFactory) where TSkiaObject : SKObject
	{
		if (handle == IntPtr.Zero)
		{
			return null;
		}
		return HandleDictionary.GetOrAddObject(handle, owns, unrefExisting: true, objectFactory);
	}

	internal static TSkiaObject GetOrAddObject<TSkiaObject>(IntPtr handle, bool owns, bool unrefExisting, Func<IntPtr, bool, TSkiaObject> objectFactory) where TSkiaObject : SKObject
	{
		if (handle == IntPtr.Zero)
		{
			return null;
		}
		return HandleDictionary.GetOrAddObject(handle, owns, unrefExisting, objectFactory);
	}

	internal static void RegisterHandle(IntPtr handle, SKObject instance)
	{
		if (!(handle == IntPtr.Zero) && instance != null)
		{
			HandleDictionary.RegisterHandle(handle, instance);
		}
	}

	internal static void DeregisterHandle(IntPtr handle, SKObject instance)
	{
		if (!(handle == IntPtr.Zero))
		{
			HandleDictionary.DeregisterHandle(handle, instance);
		}
	}

	internal static bool GetInstance<TSkiaObject>(IntPtr handle, out TSkiaObject instance) where TSkiaObject : SKObject
	{
		if (handle == IntPtr.Zero)
		{
			instance = null;
			return false;
		}
		return HandleDictionary.GetInstance<TSkiaObject>(handle, out instance);
	}

	internal void PreventPublicDisposal()
	{
		base.IgnorePublicDispose = true;
	}

	internal void RevokeOwnership(SKObject newOwner)
	{
		OwnsHandle = false;
		base.IgnorePublicDispose = true;
		if (newOwner == null)
		{
			DisposeInternal();
		}
		else
		{
			newOwner.OwnedObjects[Handle] = this;
		}
	}

	internal static T OwnedBy<T>(T child, SKObject owner) where T : SKObject
	{
		if (child != null)
		{
			owner.OwnedObjects[child.Handle] = child;
		}
		return child;
	}

	internal static T Owned<T>(T owner, SKObject child) where T : SKObject
	{
		if (child != null)
		{
			if (owner != null)
			{
				owner.OwnedObjects[child.Handle] = child;
			}
			else
			{
				child.Dispose();
			}
		}
		return owner;
	}

	internal static T Referenced<T>(T owner, SKObject child) where T : SKObject
	{
		if (child != null && owner != null)
		{
			owner.KeepAliveObjects[child.Handle] = child;
		}
		return owner;
	}

	internal static T[] PtrToStructureArray<T>(IntPtr intPtr, int count)
	{
		T[] array = new T[count];
		int num = Marshal.SizeOf<T>();
		for (int i = 0; i < count; i++)
		{
			IntPtr ptr = new IntPtr(intPtr.ToInt64() + i * num);
			array[i] = Marshal.PtrToStructure<T>(ptr);
		}
		return array;
	}

	internal static T PtrToStructure<T>(IntPtr intPtr, int index)
	{
		int num = Marshal.SizeOf<T>();
		IntPtr ptr = new IntPtr(intPtr.ToInt64() + index * num);
		return Marshal.PtrToStructure<T>(ptr);
	}
}
