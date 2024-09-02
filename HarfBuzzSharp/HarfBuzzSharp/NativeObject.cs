using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

public class NativeObject : IDisposable
{
	private bool isDisposed;

	private readonly bool zero;

	public virtual IntPtr Handle { get; protected set; }

	internal NativeObject(IntPtr handle)
	{
		Handle = handle;
		zero = true;
	}

	internal NativeObject(IntPtr handle, bool zero)
	{
		Handle = handle;
		this.zero = zero;
	}

	~NativeObject()
	{
		Dispose(disposing: false);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
		{
			return;
		}
		isDisposed = true;
		if (disposing)
		{
			DisposeHandler();
			if (zero)
			{
				Handle = IntPtr.Zero;
			}
		}
	}

	protected virtual void DisposeHandler()
	{
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal static IntPtr StructureArrayToPtr<T>(IReadOnlyList<T> items)
	{
		int num = Marshal.SizeOf<T>();
		IntPtr result = Marshal.AllocCoTaskMem(num * items.Count);
		for (int i = 0; i < items.Count; i++)
		{
			Marshal.StructureToPtr(ptr: new IntPtr(result.ToInt64() + i * num), structure: items[i], fDeleteOld: true);
		}
		return result;
	}

	internal static IEnumerable<string> PtrToStringArray(IntPtr intPtr)
	{
		if (intPtr != IntPtr.Zero)
		{
			IntPtr intPtr2 = Marshal.ReadIntPtr(intPtr);
			while (intPtr2 != IntPtr.Zero)
			{
				yield return Marshal.PtrToStringAnsi(intPtr2);
				intPtr = new IntPtr(intPtr.ToInt64() + IntPtr.Size);
				intPtr2 = Marshal.ReadIntPtr(intPtr);
			}
		}
	}
}
