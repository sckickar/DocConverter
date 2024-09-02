using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

public class Font : NativeObject
{
	internal const int NameBufferLength = 128;

	public Font Parent { get; }

	public OpenTypeMetrics OpenTypeMetrics { get; }

	public unsafe string[] SupportedShapers => NativeObject.PtrToStringArray((IntPtr)HarfBuzzApi.hb_shape_list_shapers()).ToArray();

	public Font(Face face)
		: base(IntPtr.Zero)
	{
		if (face == null)
		{
			throw new ArgumentNullException("face");
		}
		Handle = HarfBuzzApi.hb_font_create(face.Handle);
		OpenTypeMetrics = new OpenTypeMetrics(this);
	}

	public Font(Font parent)
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
		Handle = HarfBuzzApi.hb_font_create_sub_font(parent.Handle);
		OpenTypeMetrics = new OpenTypeMetrics(this);
	}

	public void SetFontFunctions(FontFunctions fontFunctions)
	{
		SetFontFunctions(fontFunctions, null, null);
	}

	public void SetFontFunctions(FontFunctions fontFunctions, object fontData)
	{
		SetFontFunctions(fontFunctions, fontData, null);
	}

	public unsafe void SetFontFunctions(FontFunctions fontFunctions, object fontData, ReleaseDelegate destroy)
	{
		if (fontFunctions == null)
		{
			throw new ArgumentNullException("fontFunctions");
		}
		FontUserData userData = new FontUserData(this, fontData);
		IntPtr intPtr = DelegateProxies.CreateMultiUserData(destroy, userData);
		HarfBuzzApi.hb_font_set_funcs(Handle, fontFunctions.Handle, (void*)intPtr, DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	public unsafe void GetScale(out int xScale, out int yScale)
	{
		fixed (int* x_scale = &xScale)
		{
			fixed (int* y_scale = &yScale)
			{
				HarfBuzzApi.hb_font_get_scale(Handle, x_scale, y_scale);
			}
		}
	}

	public void SetScale(int xScale, int yScale)
	{
		HarfBuzzApi.hb_font_set_scale(Handle, xScale, yScale);
	}

	public unsafe bool TryGetHorizontalFontExtents(out FontExtents extents)
	{
		fixed (FontExtents* extents2 = &extents)
		{
			return HarfBuzzApi.hb_font_get_h_extents(Handle, extents2);
		}
	}

	public unsafe bool TryGetVerticalFontExtents(out FontExtents extents)
	{
		fixed (FontExtents* extents2 = &extents)
		{
			return HarfBuzzApi.hb_font_get_v_extents(Handle, extents2);
		}
	}

	public bool TryGetNominalGlyph(int unicode, out uint glyph)
	{
		return TryGetNominalGlyph((uint)unicode, out glyph);
	}

	public unsafe bool TryGetNominalGlyph(uint unicode, out uint glyph)
	{
		fixed (uint* glyph2 = &glyph)
		{
			return HarfBuzzApi.hb_font_get_nominal_glyph(Handle, unicode, glyph2);
		}
	}

	public bool TryGetVariationGlyph(int unicode, out uint glyph)
	{
		return TryGetVariationGlyph(unicode, 0u, out glyph);
	}

	public unsafe bool TryGetVariationGlyph(uint unicode, out uint glyph)
	{
		fixed (uint* glyph2 = &glyph)
		{
			return HarfBuzzApi.hb_font_get_variation_glyph(Handle, unicode, 0u, glyph2);
		}
	}

	public bool TryGetVariationGlyph(int unicode, uint variationSelector, out uint glyph)
	{
		return TryGetVariationGlyph((uint)unicode, variationSelector, out glyph);
	}

	public unsafe bool TryGetVariationGlyph(uint unicode, uint variationSelector, out uint glyph)
	{
		fixed (uint* glyph2 = &glyph)
		{
			return HarfBuzzApi.hb_font_get_variation_glyph(Handle, unicode, variationSelector, glyph2);
		}
	}

	public int GetHorizontalGlyphAdvance(uint glyph)
	{
		return HarfBuzzApi.hb_font_get_glyph_h_advance(Handle, glyph);
	}

	public int GetVerticalGlyphAdvance(uint glyph)
	{
		return HarfBuzzApi.hb_font_get_glyph_v_advance(Handle, glyph);
	}

	public unsafe int[] GetHorizontalGlyphAdvances(ReadOnlySpan<uint> glyphs)
	{
		fixed (uint* ptr = glyphs)
		{
			return GetHorizontalGlyphAdvances((IntPtr)ptr, glyphs.Length);
		}
	}

	public unsafe int[] GetHorizontalGlyphAdvances(IntPtr firstGlyph, int count)
	{
		int[] array = new int[count];
		fixed (int* first_advance = array)
		{
			HarfBuzzApi.hb_font_get_glyph_h_advances(Handle, (uint)count, (uint*)(void*)firstGlyph, 4u, first_advance, 4u);
		}
		return array;
	}

	public unsafe int[] GetVerticalGlyphAdvances(ReadOnlySpan<uint> glyphs)
	{
		fixed (uint* ptr = glyphs)
		{
			return GetVerticalGlyphAdvances((IntPtr)ptr, glyphs.Length);
		}
	}

	public unsafe int[] GetVerticalGlyphAdvances(IntPtr firstGlyph, int count)
	{
		int[] array = new int[count];
		fixed (int* first_advance = array)
		{
			HarfBuzzApi.hb_font_get_glyph_v_advances(Handle, (uint)count, (uint*)(void*)firstGlyph, 4u, first_advance, 4u);
		}
		return array;
	}

	public unsafe bool TryGetHorizontalGlyphOrigin(uint glyph, out int xOrigin, out int yOrigin)
	{
		fixed (int* x = &xOrigin)
		{
			fixed (int* y = &yOrigin)
			{
				return HarfBuzzApi.hb_font_get_glyph_h_origin(Handle, glyph, x, y);
			}
		}
	}

	public unsafe bool TryGetVerticalGlyphOrigin(uint glyph, out int xOrigin, out int yOrigin)
	{
		fixed (int* x = &xOrigin)
		{
			fixed (int* y = &yOrigin)
			{
				return HarfBuzzApi.hb_font_get_glyph_v_origin(Handle, glyph, x, y);
			}
		}
	}

	public int GetHorizontalGlyphKerning(uint leftGlyph, uint rightGlyph)
	{
		return HarfBuzzApi.hb_font_get_glyph_h_kerning(Handle, leftGlyph, rightGlyph);
	}

	public unsafe bool TryGetGlyphExtents(uint glyph, out GlyphExtents extents)
	{
		fixed (GlyphExtents* extents2 = &extents)
		{
			return HarfBuzzApi.hb_font_get_glyph_extents(Handle, glyph, extents2);
		}
	}

	public unsafe bool TryGetGlyphContourPoint(uint glyph, uint pointIndex, out int x, out int y)
	{
		fixed (int* x2 = &x)
		{
			fixed (int* y2 = &y)
			{
				return HarfBuzzApi.hb_font_get_glyph_contour_point(Handle, glyph, pointIndex, x2, y2);
			}
		}
	}

	public unsafe bool TryGetGlyphName(uint glyph, out string name)
	{
		ArrayPool<byte> shared = ArrayPool<byte>.Shared;
		byte[] array = shared.Rent(128);
		try
		{
			fixed (byte* ptr = array)
			{
				if (!HarfBuzzApi.hb_font_get_glyph_name(Handle, glyph, ptr, (uint)array.Length))
				{
					name = string.Empty;
					return false;
				}
				name = Marshal.PtrToStringAnsi((IntPtr)ptr);
				return true;
			}
		}
		finally
		{
			shared.Return(array);
		}
	}

	public unsafe bool TryGetGlyphFromName(string name, out uint glyph)
	{
		fixed (uint* glyph2 = &glyph)
		{
			return HarfBuzzApi.hb_font_get_glyph_from_name(Handle, name, name.Length, glyph2);
		}
	}

	public bool TryGetGlyph(int unicode, out uint glyph)
	{
		return TryGetGlyph((uint)unicode, 0u, out glyph);
	}

	public bool TryGetGlyph(uint unicode, out uint glyph)
	{
		return TryGetGlyph(unicode, 0u, out glyph);
	}

	public bool TryGetGlyph(int unicode, uint variationSelector, out uint glyph)
	{
		return TryGetGlyph((uint)unicode, variationSelector, out glyph);
	}

	public unsafe bool TryGetGlyph(uint unicode, uint variationSelector, out uint glyph)
	{
		fixed (uint* glyph2 = &glyph)
		{
			return HarfBuzzApi.hb_font_get_glyph(Handle, unicode, variationSelector, glyph2);
		}
	}

	public unsafe FontExtents GetFontExtentsForDirection(Direction direction)
	{
		FontExtents result = default(FontExtents);
		HarfBuzzApi.hb_font_get_extents_for_direction(Handle, direction, &result);
		return result;
	}

	public unsafe void GetGlyphAdvanceForDirection(uint glyph, Direction direction, out int x, out int y)
	{
		fixed (int* x2 = &x)
		{
			fixed (int* y2 = &y)
			{
				HarfBuzzApi.hb_font_get_glyph_advance_for_direction(Handle, glyph, direction, x2, y2);
			}
		}
	}

	public unsafe int[] GetGlyphAdvancesForDirection(ReadOnlySpan<uint> glyphs, Direction direction)
	{
		fixed (uint* ptr = glyphs)
		{
			return GetGlyphAdvancesForDirection((IntPtr)ptr, glyphs.Length, direction);
		}
	}

	public unsafe int[] GetGlyphAdvancesForDirection(IntPtr firstGlyph, int count, Direction direction)
	{
		int[] array = new int[count];
		fixed (int* first_advance = array)
		{
			HarfBuzzApi.hb_font_get_glyph_advances_for_direction(Handle, direction, (uint)count, (uint*)(void*)firstGlyph, 4u, first_advance, 4u);
		}
		return array;
	}

	public unsafe bool TryGetGlyphContourPointForOrigin(uint glyph, uint pointIndex, Direction direction, out int x, out int y)
	{
		fixed (int* x2 = &x)
		{
			fixed (int* y2 = &y)
			{
				return HarfBuzzApi.hb_font_get_glyph_contour_point_for_origin(Handle, glyph, pointIndex, direction, x2, y2);
			}
		}
	}

	public unsafe string GlyphToString(uint glyph)
	{
		ArrayPool<byte> shared = ArrayPool<byte>.Shared;
		byte[] array = shared.Rent(128);
		try
		{
			fixed (byte* ptr = array)
			{
				HarfBuzzApi.hb_font_glyph_to_string(Handle, glyph, ptr, (uint)array.Length);
				return Marshal.PtrToStringAnsi((IntPtr)ptr);
			}
		}
		finally
		{
			shared.Return(array);
		}
	}

	public unsafe bool TryGetGlyphFromString(string s, out uint glyph)
	{
		fixed (uint* glyph2 = &glyph)
		{
			return HarfBuzzApi.hb_font_glyph_from_string(Handle, s, -1, glyph2);
		}
	}

	public void SetFunctionsOpenType()
	{
		HarfBuzzApi.hb_ot_font_set_funcs(Handle);
	}

	public void Shape(Buffer buffer, params Feature[] features)
	{
		Shape(buffer, features, null);
	}

	public unsafe void Shape(Buffer buffer, IReadOnlyList<Feature> features, IReadOnlyList<string> shapers)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (buffer.Direction == Direction.Invalid)
		{
			throw new InvalidOperationException("Buffer's Direction must be valid.");
		}
		if (buffer.ContentType != ContentType.Unicode)
		{
			throw new InvalidOperationException("Buffer's ContentType must of type Unicode.");
		}
		void*[] array = null;
		if (shapers != null && shapers.Count > 0)
		{
			array = new void*[shapers.Count + 1];
			int i;
			for (i = 0; i < shapers.Count; i++)
			{
				array[i] = (void*)Marshal.StringToHGlobalAnsi(shapers[i]);
			}
			array[i] = null;
		}
		fixed (Feature* features2 = features?.ToArray())
		{
			fixed (void** shaper_list = array)
			{
				HarfBuzzApi.hb_shape_full(Handle, buffer.Handle, features2, (uint)(features?.Count ?? 0), shaper_list);
			}
		}
		if (array == null)
		{
			return;
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] != null)
			{
				Marshal.FreeHGlobal((IntPtr)array[j]);
			}
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
			HarfBuzzApi.hb_font_destroy(Handle);
		}
	}
}
