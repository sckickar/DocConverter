using System;

namespace SkiaSharp;

internal static class SKObjectExtensions
{
	public static bool IsUnique(this IntPtr handle, bool isVirtual)
	{
		if (isVirtual)
		{
			return SkiaApi.sk_refcnt_unique(handle);
		}
		return SkiaApi.sk_nvrefcnt_unique(handle);
	}

	public static int GetReferenceCount(this IntPtr handle, bool isVirtual)
	{
		if (isVirtual)
		{
			return SkiaApi.sk_refcnt_get_ref_count(handle);
		}
		return SkiaApi.sk_nvrefcnt_get_ref_count(handle);
	}

	public static void SafeRef(this ISKReferenceCounted obj)
	{
		if (obj is ISKNonVirtualReferenceCounted iSKNonVirtualReferenceCounted)
		{
			iSKNonVirtualReferenceCounted.ReferenceNative();
		}
		else
		{
			SkiaApi.sk_refcnt_safe_unref(obj.Handle);
		}
	}

	public static void SafeUnRef(this ISKReferenceCounted obj)
	{
		if (obj is ISKNonVirtualReferenceCounted iSKNonVirtualReferenceCounted)
		{
			iSKNonVirtualReferenceCounted.UnreferenceNative();
		}
		else
		{
			SkiaApi.sk_refcnt_safe_unref(obj.Handle);
		}
	}

	public static int GetReferenceCount(this ISKReferenceCounted obj)
	{
		if (obj is ISKNonVirtualReferenceCounted)
		{
			return SkiaApi.sk_nvrefcnt_get_ref_count(obj.Handle);
		}
		return SkiaApi.sk_refcnt_get_ref_count(obj.Handle);
	}
}
