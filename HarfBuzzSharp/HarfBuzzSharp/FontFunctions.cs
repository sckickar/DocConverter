using System;

namespace HarfBuzzSharp;

public class FontFunctions : NativeObject
{
	private class StaticFontFunctions : FontFunctions
	{
		public StaticFontFunctions(IntPtr handle)
			: base(handle)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	private static readonly Lazy<FontFunctions> emptyFontFunctions = new Lazy<FontFunctions>(() => new StaticFontFunctions(HarfBuzzApi.hb_font_funcs_get_empty()));

	public static FontFunctions Empty => emptyFontFunctions.Value;

	public bool IsImmutable => HarfBuzzApi.hb_font_funcs_is_immutable(Handle);

	public FontFunctions()
		: this(HarfBuzzApi.hb_font_funcs_create())
	{
	}

	internal FontFunctions(IntPtr handle)
		: base(handle)
	{
	}

	public void MakeImmutable()
	{
		HarfBuzzApi.hb_font_funcs_make_immutable(Handle);
	}

	public unsafe void SetHorizontalFontExtentsDelegate(FontExtentsDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_font_h_extents_func(Handle, DelegateProxies.FontExtentsProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetVerticalFontExtentsDelegate(FontExtentsDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_font_v_extents_func(Handle, DelegateProxies.FontExtentsProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetNominalGlyphDelegate(NominalGlyphDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_nominal_glyph_func(Handle, DelegateProxies.NominalGlyphProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetNominalGlyphsDelegate(NominalGlyphsDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_nominal_glyphs_func(Handle, DelegateProxies.NominalGlyphsProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetVariationGlyphDelegate(VariationGlyphDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_variation_glyph_func(Handle, DelegateProxies.VariationGlyphProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetHorizontalGlyphAdvanceDelegate(GlyphAdvanceDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_glyph_h_advance_func(Handle, DelegateProxies.GlyphAdvanceProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetVerticalGlyphAdvanceDelegate(GlyphAdvanceDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_glyph_v_advance_func(Handle, DelegateProxies.GlyphAdvanceProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetHorizontalGlyphAdvancesDelegate(GlyphAdvancesDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_glyph_h_advances_func(Handle, DelegateProxies.GlyphAdvancesProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetVerticalGlyphAdvancesDelegate(GlyphAdvancesDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_glyph_v_advances_func(Handle, DelegateProxies.GlyphAdvancesProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetHorizontalGlyphOriginDelegate(GlyphOriginDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_glyph_h_origin_func(Handle, DelegateProxies.GlyphOriginProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetVerticalGlyphOriginDelegate(GlyphOriginDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_glyph_v_origin_func(Handle, DelegateProxies.GlyphOriginProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetHorizontalGlyphKerningDelegate(GlyphKerningDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_glyph_h_kerning_func(Handle, DelegateProxies.GlyphKerningProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetGlyphExtentsDelegate(GlyphExtentsDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_glyph_extents_func(Handle, DelegateProxies.GlyphExtentsProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetGlyphContourPointDelegate(GlyphContourPointDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_glyph_contour_point_func(Handle, DelegateProxies.GlyphContourPointProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetGlyphNameDelegate(GlyphNameDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_glyph_name_func(Handle, DelegateProxies.GlyphNameProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void SetGlyphFromNameDelegate(GlyphFromNameDelegate del, ReleaseDelegate destroy = null)
	{
		VerifyParameters(del);
		IntPtr intPtr = DelegateProxies.CreateMulti(del, destroy);
		HarfBuzzApi.hb_font_funcs_set_glyph_from_name_func(Handle, DelegateProxies.GlyphFromNameProxy, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeHandler()
	{
		if (Handle != IntPtr.Zero)
		{
			HarfBuzzApi.hb_font_funcs_destroy(Handle);
		}
	}

	private void VerifyParameters(Delegate del)
	{
		if ((object)del == null)
		{
			throw new ArgumentNullException("del");
		}
		if (IsImmutable)
		{
			throw new InvalidOperationException("FontFunctions is immutable and can't be changed.");
		}
	}
}
