using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

internal static class DelegateProxies
{
	public unsafe static readonly DestroyProxyDelegate ReleaseDelegateProxy = ReleaseDelegateProxyImplementation;

	public unsafe static readonly DestroyProxyDelegate ReleaseDelegateProxyForMulti = ReleaseDelegateProxyImplementationForMulti;

	public unsafe static readonly ReferenceTableProxyDelegate GetTableDelegateProxy = GetTableDelegateProxyImplementation;

	public unsafe static readonly FontGetFontExtentsProxyDelegate FontExtentsProxy = FontExtentsProxyImplementation;

	public unsafe static readonly FontGetNominalGlyphProxyDelegate NominalGlyphProxy = NominalGlyphProxyImplementation;

	public unsafe static readonly FontGetVariationGlyphProxyDelegate VariationGlyphProxy = VariationGlyphProxyImplementation;

	public unsafe static readonly FontGetNominalGlyphsProxyDelegate NominalGlyphsProxy = NominalGlyphsProxyImplementation;

	public unsafe static readonly FontGetGlyphAdvanceProxyDelegate GlyphAdvanceProxy = GlyphAdvanceProxyImplementation;

	public unsafe static readonly FontGetGlyphAdvancesProxyDelegate GlyphAdvancesProxy = GlyphAdvancesProxyImplementation;

	public unsafe static readonly FontGetGlyphOriginProxyDelegate GlyphOriginProxy = GlyphOriginProxyImplementation;

	public unsafe static readonly FontGetGlyphKerningProxyDelegate GlyphKerningProxy = GlyphKerningProxyImplementation;

	public unsafe static readonly FontGetGlyphExtentsProxyDelegate GlyphExtentsProxy = GlyphExtentsProxyImplementation;

	public unsafe static readonly FontGetGlyphContourPointProxyDelegate GlyphContourPointProxy = GlyphContourPointProxyImplementation;

	public unsafe static readonly FontGetGlyphNameProxyDelegate GlyphNameProxy = GlyphNameProxyImplementation;

	public unsafe static readonly FontGetGlyphFromNameProxyDelegate GlyphFromNameProxy = GlyphFromNameProxyImplementation;

	public unsafe static readonly UnicodeCombiningClassProxyDelegate CombiningClassProxy = CombiningClassProxyImplementation;

	public unsafe static readonly UnicodeGeneralCategoryProxyDelegate GeneralCategoryProxy = GeneralCategoryProxyImplementation;

	public unsafe static readonly UnicodeMirroringProxyDelegate MirroringProxy = MirroringProxyImplementation;

	public unsafe static readonly UnicodeScriptProxyDelegate ScriptProxy = ScriptProxyImplementation;

	public unsafe static readonly UnicodeComposeProxyDelegate ComposeProxy = ComposeProxyImplementation;

	public unsafe static readonly UnicodeDecomposeProxyDelegate DecomposeProxy = DecomposeProxyImplementation;

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

	[MonoPInvokeCallback(typeof(DestroyProxyDelegate))]
	private unsafe static void ReleaseDelegateProxyImplementation(void* context)
	{
		GCHandle gch;
		ReleaseDelegate releaseDelegate = Get<ReleaseDelegate>((IntPtr)context, out gch);
		try
		{
			releaseDelegate();
		}
		finally
		{
			gch.Free();
		}
	}

	[MonoPInvokeCallback(typeof(ReferenceTableProxyDelegate))]
	private unsafe static IntPtr GetTableDelegateProxyImplementation(IntPtr face, uint tag, void* context)
	{
		GetMultiUserData<GetTableDelegate, Face>((IntPtr)context, out var wrappedDelegate, out var userData, out var _);
		return wrappedDelegate(userData, tag)?.Handle ?? IntPtr.Zero;
	}

	[MonoPInvokeCallback(typeof(DestroyProxyDelegate))]
	private unsafe static void ReleaseDelegateProxyImplementationForMulti(void* context)
	{
		GCHandle gch;
		ReleaseDelegate multi = GetMulti<ReleaseDelegate>((IntPtr)context, out gch);
		try
		{
			multi?.Invoke();
		}
		finally
		{
			gch.Free();
		}
	}

	[MonoPInvokeCallback(typeof(FontGetFontExtentsProxyDelegate))]
	private unsafe static bool FontExtentsProxyImplementation(IntPtr font, void* fontData, FontExtents* extents, void* context)
	{
		GCHandle gch;
		FontExtentsDelegate multi = GetMulti<FontExtentsDelegate>((IntPtr)context, out gch);
		FontUserData multiUserData = GetMultiUserData<FontUserData>((IntPtr)fontData, out gch);
		FontExtents extents2;
		bool result = multi(multiUserData.Font, multiUserData.FontData, out extents2);
		if (extents != null)
		{
			*extents = extents2;
		}
		return result;
	}

	[MonoPInvokeCallback(typeof(FontGetNominalGlyphProxyDelegate))]
	private unsafe static bool NominalGlyphProxyImplementation(IntPtr font, void* fontData, uint unicode, uint* glyph, void* context)
	{
		GCHandle gch;
		NominalGlyphDelegate multi = GetMulti<NominalGlyphDelegate>((IntPtr)context, out gch);
		FontUserData multiUserData = GetMultiUserData<FontUserData>((IntPtr)fontData, out gch);
		uint glyph2;
		bool result = multi(multiUserData.Font, multiUserData.FontData, unicode, out glyph2);
		if (glyph != null)
		{
			*glyph = glyph2;
		}
		return result;
	}

	[MonoPInvokeCallback(typeof(FontGetNominalGlyphsProxyDelegate))]
	private unsafe static uint NominalGlyphsProxyImplementation(IntPtr font, void* fontData, uint count, uint* firstUnicode, uint unicodeStride, uint* firstGlyph, uint glyphStride, void* context)
	{
		GCHandle gch;
		NominalGlyphsDelegate multi = GetMulti<NominalGlyphsDelegate>((IntPtr)context, out gch);
		ReadOnlySpan<uint> codepoints = new ReadOnlySpan<uint>(firstUnicode, (int)count);
		Span<uint> glyphs = new Span<uint>(firstGlyph, (int)count);
		FontUserData multiUserData = GetMultiUserData<FontUserData>((IntPtr)fontData, out gch);
		return multi(multiUserData.Font, multiUserData.FontData, count, codepoints, glyphs);
	}

	[MonoPInvokeCallback(typeof(FontGetVariationGlyphProxyDelegate))]
	private unsafe static bool VariationGlyphProxyImplementation(IntPtr font, void* fontData, uint unicode, uint variationSelector, uint* glyph, void* context)
	{
		GCHandle gch;
		VariationGlyphDelegate multi = GetMulti<VariationGlyphDelegate>((IntPtr)context, out gch);
		FontUserData multiUserData = GetMultiUserData<FontUserData>((IntPtr)fontData, out gch);
		uint glyph2;
		bool result = multi(multiUserData.Font, multiUserData.FontData, unicode, variationSelector, out glyph2);
		if (glyph != null)
		{
			*glyph = glyph2;
		}
		return result;
	}

	[MonoPInvokeCallback(typeof(FontGetGlyphAdvanceProxyDelegate))]
	private unsafe static int GlyphAdvanceProxyImplementation(IntPtr font, void* fontData, uint glyph, void* context)
	{
		GCHandle gch;
		GlyphAdvanceDelegate multi = GetMulti<GlyphAdvanceDelegate>((IntPtr)context, out gch);
		FontUserData multiUserData = GetMultiUserData<FontUserData>((IntPtr)fontData, out gch);
		return multi(multiUserData.Font, multiUserData.FontData, glyph);
	}

	[MonoPInvokeCallback(typeof(FontGetGlyphAdvancesProxyDelegate))]
	private unsafe static void GlyphAdvancesProxyImplementation(IntPtr font, void* fontData, uint count, uint* firstGlyph, uint glyphStride, int* firstAdvance, uint advanceStride, void* context)
	{
		GCHandle gch;
		GlyphAdvancesDelegate multi = GetMulti<GlyphAdvancesDelegate>((IntPtr)context, out gch);
		ReadOnlySpan<uint> glyphs = new ReadOnlySpan<uint>(firstGlyph, (int)count);
		Span<int> advances = new Span<int>(firstAdvance, (int)count);
		FontUserData multiUserData = GetMultiUserData<FontUserData>((IntPtr)fontData, out gch);
		multi(multiUserData.Font, multiUserData.FontData, count, glyphs, advances);
	}

	[MonoPInvokeCallback(typeof(FontGetGlyphOriginProxyDelegate))]
	private unsafe static bool GlyphOriginProxyImplementation(IntPtr font, void* fontData, uint glyph, int* x, int* y, void* context)
	{
		GCHandle gch;
		GlyphOriginDelegate multi = GetMulti<GlyphOriginDelegate>((IntPtr)context, out gch);
		FontUserData multiUserData = GetMultiUserData<FontUserData>((IntPtr)fontData, out gch);
		int x2;
		int y2;
		bool result = multi(multiUserData.Font, multiUserData.FontData, glyph, out x2, out y2);
		if (x != null)
		{
			*x = x2;
		}
		if (y != null)
		{
			*y = y2;
		}
		return result;
	}

	[MonoPInvokeCallback(typeof(FontGetGlyphKerningProxyDelegate))]
	private unsafe static int GlyphKerningProxyImplementation(IntPtr font, void* fontData, uint firstGlyph, uint secondGlyph, void* context)
	{
		GCHandle gch;
		GlyphKerningDelegate multi = GetMulti<GlyphKerningDelegate>((IntPtr)context, out gch);
		FontUserData multiUserData = GetMultiUserData<FontUserData>((IntPtr)fontData, out gch);
		return multi(multiUserData.Font, multiUserData.FontData, firstGlyph, secondGlyph);
	}

	[MonoPInvokeCallback(typeof(FontGetGlyphExtentsProxyDelegate))]
	private unsafe static bool GlyphExtentsProxyImplementation(IntPtr font, void* fontData, uint glyph, GlyphExtents* extents, void* context)
	{
		GCHandle gch;
		GlyphExtentsDelegate multi = GetMulti<GlyphExtentsDelegate>((IntPtr)context, out gch);
		FontUserData multiUserData = GetMultiUserData<FontUserData>((IntPtr)fontData, out gch);
		GlyphExtents extents2;
		bool result = multi(multiUserData.Font, multiUserData.FontData, glyph, out extents2);
		if (extents != null)
		{
			*extents = extents2;
		}
		return result;
	}

	[MonoPInvokeCallback(typeof(FontGetGlyphContourPointProxyDelegate))]
	private unsafe static bool GlyphContourPointProxyImplementation(IntPtr font, void* fontData, uint glyph, uint pointIndex, int* x, int* y, void* context)
	{
		GCHandle gch;
		GlyphContourPointDelegate multi = GetMulti<GlyphContourPointDelegate>((IntPtr)context, out gch);
		FontUserData multiUserData = GetMultiUserData<FontUserData>((IntPtr)fontData, out gch);
		int x2;
		int y2;
		bool result = multi(multiUserData.Font, multiUserData.FontData, glyph, pointIndex, out x2, out y2);
		if (x != null)
		{
			*x = x2;
		}
		if (y != null)
		{
			*y = y2;
		}
		return result;
	}

	[MonoPInvokeCallback(typeof(FontGetGlyphNameProxyDelegate))]
	private unsafe static bool GlyphNameProxyImplementation(IntPtr font, void* fontData, uint glyph, void* nameBuffer, uint size, void* context)
	{
		GCHandle gch;
		GlyphNameDelegate multi = GetMulti<GlyphNameDelegate>((IntPtr)context, out gch);
		FontUserData multiUserData = GetMultiUserData<FontUserData>((IntPtr)fontData, out gch);
		string name;
		bool result = multi(multiUserData.Font, multiUserData.FontData, glyph, out name);
		ReadOnlySpan<char> readOnlySpan = name.AsSpan();
		Span<char> destination = new Span<char>(nameBuffer, (int)size);
		readOnlySpan.CopyTo(destination);
		return result;
	}

	[MonoPInvokeCallback(typeof(FontGetGlyphFromNameProxyDelegate))]
	private unsafe static bool GlyphFromNameProxyImplementation(IntPtr font, void* fontData, void* name, int len, uint* glyph, void* context)
	{
		GCHandle gch;
		GlyphFromNameDelegate multi = GetMulti<GlyphFromNameDelegate>((IntPtr)context, out gch);
		FontUserData multiUserData = GetMultiUserData<FontUserData>((IntPtr)fontData, out gch);
		string name2 = ((len < 0) ? new string((char*)name) : new string((char*)name, 0, len));
		uint glyph2;
		bool result = multi(multiUserData.Font, multiUserData.FontData, name2, out glyph2);
		if (glyph != null)
		{
			*glyph = glyph2;
		}
		return result;
	}

	[MonoPInvokeCallback(typeof(UnicodeCombiningClassProxyDelegate))]
	private unsafe static int CombiningClassProxyImplementation(IntPtr ufuncs, uint unicode, void* context)
	{
		GetMultiUserData<CombiningClassDelegate, UnicodeFunctions>((IntPtr)context, out var wrappedDelegate, out var userData, out var _);
		return (int)wrappedDelegate(userData, unicode);
	}

	[MonoPInvokeCallback(typeof(UnicodeGeneralCategoryProxyDelegate))]
	private unsafe static int GeneralCategoryProxyImplementation(IntPtr ufuncs, uint unicode, void* context)
	{
		GetMultiUserData<GeneralCategoryDelegate, UnicodeFunctions>((IntPtr)context, out var wrappedDelegate, out var userData, out var _);
		return (int)wrappedDelegate(userData, unicode);
	}

	[MonoPInvokeCallback(typeof(UnicodeMirroringProxyDelegate))]
	private unsafe static uint MirroringProxyImplementation(IntPtr ufuncs, uint unicode, void* context)
	{
		GetMultiUserData<MirroringDelegate, UnicodeFunctions>((IntPtr)context, out var wrappedDelegate, out var userData, out var _);
		return wrappedDelegate(userData, unicode);
	}

	[MonoPInvokeCallback(typeof(UnicodeScriptProxyDelegate))]
	private unsafe static uint ScriptProxyImplementation(IntPtr ufuncs, uint unicode, void* context)
	{
		GetMultiUserData<ScriptDelegate, UnicodeFunctions>((IntPtr)context, out var wrappedDelegate, out var userData, out var _);
		return wrappedDelegate(userData, unicode);
	}

	[MonoPInvokeCallback(typeof(UnicodeComposeProxyDelegate))]
	private unsafe static bool ComposeProxyImplementation(IntPtr ufuncs, uint a, uint b, uint* ab, void* context)
	{
		GetMultiUserData<ComposeDelegate, UnicodeFunctions>((IntPtr)context, out var wrappedDelegate, out var userData, out var _);
		uint ab2;
		bool result = wrappedDelegate(userData, a, b, out ab2);
		if (ab != null)
		{
			*ab = ab2;
		}
		return result;
	}

	[MonoPInvokeCallback(typeof(UnicodeDecomposeProxyDelegate))]
	private unsafe static bool DecomposeProxyImplementation(IntPtr ufuncs, uint ab, uint* a, uint* b, void* context)
	{
		GetMultiUserData<DecomposeDelegate, UnicodeFunctions>((IntPtr)context, out var wrappedDelegate, out var userData, out var _);
		uint a2;
		uint b2;
		bool result = wrappedDelegate(userData, ab, out a2, out b2);
		if (a != null)
		{
			*a = a2;
		}
		if (b != null)
		{
			*b = b2;
		}
		return result;
	}
}
