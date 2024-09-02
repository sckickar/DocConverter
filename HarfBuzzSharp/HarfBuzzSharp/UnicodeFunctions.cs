using System;

namespace HarfBuzzSharp;

public class UnicodeFunctions : NativeObject
{
	private class StaticUnicodeFunctions : UnicodeFunctions
	{
		public StaticUnicodeFunctions(IntPtr handle)
			: base(handle)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	private static readonly Lazy<UnicodeFunctions> defaultFunctions = new Lazy<UnicodeFunctions>(() => new StaticUnicodeFunctions(HarfBuzzApi.hb_unicode_funcs_get_default()));

	private static readonly Lazy<UnicodeFunctions> emptyFunctions = new Lazy<UnicodeFunctions>(() => new StaticUnicodeFunctions(HarfBuzzApi.hb_unicode_funcs_get_empty()));

	public static UnicodeFunctions Default => defaultFunctions.Value;

	public static UnicodeFunctions Empty => emptyFunctions.Value;

	public UnicodeFunctions Parent { get; }

	public bool IsImmutable => HarfBuzzApi.hb_unicode_funcs_is_immutable(Handle);

	internal UnicodeFunctions(IntPtr handle)
		: base(handle)
	{
	}

	public UnicodeFunctions(UnicodeFunctions parent)
		: base(IntPtr.Zero)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		if (parent.Handle == IntPtr.Zero)
		{
			throw new ArgumentException("Handle");
		}
		Parent = parent;
		Handle = HarfBuzzApi.hb_unicode_funcs_create(parent.Handle);
	}

	public void MakeImmutable()
	{
		HarfBuzzApi.hb_unicode_funcs_make_immutable(Handle);
	}

	public UnicodeCombiningClass GetCombiningClass(int unicode)
	{
		return GetCombiningClass((uint)unicode);
	}

	public UnicodeCombiningClass GetCombiningClass(uint unicode)
	{
		return HarfBuzzApi.hb_unicode_combining_class(Handle, unicode);
	}

	public UnicodeGeneralCategory GetGeneralCategory(int unicode)
	{
		return GetGeneralCategory((uint)unicode);
	}

	public UnicodeGeneralCategory GetGeneralCategory(uint unicode)
	{
		return HarfBuzzApi.hb_unicode_general_category(Handle, unicode);
	}

	public int GetMirroring(int unicode)
	{
		return (int)GetMirroring((uint)unicode);
	}

	public uint GetMirroring(uint unicode)
	{
		return HarfBuzzApi.hb_unicode_mirroring(Handle, unicode);
	}

	public Script GetScript(int unicode)
	{
		return GetScript((uint)unicode);
	}

	public Script GetScript(uint unicode)
	{
		return HarfBuzzApi.hb_unicode_script(Handle, unicode);
	}

	public bool TryCompose(int a, int b, out int ab)
	{
		uint ab2;
		bool result = TryCompose((uint)a, (uint)b, out ab2);
		ab = (int)ab2;
		return result;
	}

	public unsafe bool TryCompose(uint a, uint b, out uint ab)
	{
		fixed (uint* ab2 = &ab)
		{
			return HarfBuzzApi.hb_unicode_compose(Handle, a, b, ab2);
		}
	}

	public bool TryDecompose(int ab, out int a, out int b)
	{
		uint a2;
		uint b2;
		bool result = TryDecompose((uint)ab, out a2, out b2);
		a = (int)a2;
		b = (int)b2;
		return result;
	}

	public unsafe bool TryDecompose(uint ab, out uint a, out uint b)
	{
		fixed (uint* a2 = &a)
		{
			fixed (uint* b2 = &b)
			{
				return HarfBuzzApi.hb_unicode_decompose(Handle, ab, a2, b2);
			}
		}
	}

	public unsafe void SetCombiningClassDelegate(CombiningClassDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMultiUserData(del, destroy, this);
		HarfBuzzApi.hb_unicode_funcs_set_combining_class_func(Handle, DelegateProxies.CombiningClassProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetGeneralCategoryDelegate(GeneralCategoryDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMultiUserData(del, destroy, this);
		HarfBuzzApi.hb_unicode_funcs_set_general_category_func(Handle, DelegateProxies.GeneralCategoryProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetMirroringDelegate(MirroringDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMultiUserData(del, destroy, this);
		HarfBuzzApi.hb_unicode_funcs_set_mirroring_func(Handle, DelegateProxies.MirroringProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetScriptDelegate(ScriptDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMultiUserData(del, destroy, this);
		HarfBuzzApi.hb_unicode_funcs_set_script_func(Handle, DelegateProxies.ScriptProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetComposeDelegate(ComposeDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMultiUserData(del, destroy, this);
		HarfBuzzApi.hb_unicode_funcs_set_compose_func(Handle, DelegateProxies.ComposeProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetDecomposeDelegate(DecomposeDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMultiUserData(del, destroy, this);
		HarfBuzzApi.hb_unicode_funcs_set_decompose_func(Handle, DelegateProxies.DecomposeProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	private void VerifyParameters(Delegate del)
	{
		if ((object)del == null)
		{
			throw new ArgumentNullException("del");
		}
		if (IsImmutable)
		{
			throw new InvalidOperationException("UnicodeFunctions is immutable and can't be changed.");
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeHandler()
	{
		if (Handle != IntPtr.Zero)
		{
			HarfBuzzApi.hb_unicode_funcs_destroy(Handle);
		}
	}
}
