using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

internal class HarfBuzzApi
{
	private const string HARFBUZZ = "libHarfBuzzSharp";

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_blob_copy_writable_or_fail(IntPtr blob);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr hb_blob_create(void* data, uint length, MemoryMode mode, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_blob_create_from_file([MarshalAs(UnmanagedType.LPStr)] string file_name);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr hb_blob_create_from_file_or_fail(void* file_name);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr hb_blob_create_or_fail(void* data, uint length, MemoryMode mode, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_blob_create_sub_blob(IntPtr parent, uint offset, uint length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_blob_destroy(IntPtr blob);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* hb_blob_get_data(IntPtr blob, uint* length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* hb_blob_get_data_writable(IntPtr blob, uint* length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_blob_get_empty();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_blob_get_length(IntPtr blob);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_blob_is_immutable(IntPtr blob);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_blob_make_immutable(IntPtr blob);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_blob_reference(IntPtr blob);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_add(IntPtr buffer, uint codepoint, uint cluster);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_buffer_add_codepoints(IntPtr buffer, uint* text, int text_length, uint item_offset, int item_length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_buffer_add_latin1(IntPtr buffer, byte* text, int text_length, uint item_offset, int item_length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_buffer_add_utf16(IntPtr buffer, ushort* text, int text_length, uint item_offset, int item_length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_buffer_add_utf32(IntPtr buffer, uint* text, int text_length, uint item_offset, int item_length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_buffer_add_utf8(IntPtr buffer, void* text, int text_length, uint item_offset, int item_length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_buffer_allocation_successful(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_append(IntPtr buffer, IntPtr source, uint start, uint end);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_clear_contents(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_buffer_create();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_buffer_deserialize_glyphs(IntPtr buffer, [MarshalAs(UnmanagedType.LPStr)] string buf, int buf_len, void** end_ptr, IntPtr font, SerializeFormat format);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_buffer_deserialize_unicode(IntPtr buffer, void* buf, int buf_len, void** end_ptr, SerializeFormat format);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_destroy(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern BufferDiffFlags hb_buffer_diff(IntPtr buffer, IntPtr reference, uint dottedcircle_glyph, uint position_fuzz);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern ClusterLevel hb_buffer_get_cluster_level(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern ContentType hb_buffer_get_content_type(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern Direction hb_buffer_get_direction(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_buffer_get_empty();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern BufferFlags hb_buffer_get_flags(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern GlyphInfo* hb_buffer_get_glyph_infos(IntPtr buffer, uint* length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern GlyphPosition* hb_buffer_get_glyph_positions(IntPtr buffer, uint* length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_buffer_get_invisible_glyph(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_buffer_get_language(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_buffer_get_length(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_buffer_get_replacement_codepoint(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_buffer_get_script(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_buffer_get_unicode_funcs(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_guess_segment_properties(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_buffer_has_positions(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_normalize_glyphs(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_buffer_pre_allocate(IntPtr buffer, uint size);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_buffer_reference(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_reset(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_reverse(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_reverse_clusters(IntPtr buffer);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_reverse_range(IntPtr buffer, uint start, uint end);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_buffer_serialize(IntPtr buffer, uint start, uint end, void* buf, uint buf_size, uint* buf_consumed, IntPtr font, SerializeFormat format, SerializeFlag flags);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern SerializeFormat hb_buffer_serialize_format_from_string(void* str, int len);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* hb_buffer_serialize_format_to_string(SerializeFormat format);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_buffer_serialize_glyphs(IntPtr buffer, uint start, uint end, void* buf, uint buf_size, uint* buf_consumed, IntPtr font, SerializeFormat format, SerializeFlag flags);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void** hb_buffer_serialize_list_formats();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_buffer_serialize_unicode(IntPtr buffer, uint start, uint end, void* buf, uint buf_size, uint* buf_consumed, SerializeFormat format, SerializeFlag flags);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_set_cluster_level(IntPtr buffer, ClusterLevel cluster_level);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_set_content_type(IntPtr buffer, ContentType content_type);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_set_direction(IntPtr buffer, Direction direction);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_set_flags(IntPtr buffer, BufferFlags flags);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_set_invisible_glyph(IntPtr buffer, uint invisible);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_set_language(IntPtr buffer, IntPtr language);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_buffer_set_length(IntPtr buffer, uint length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_buffer_set_message_func(IntPtr buffer, BufferMessageProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_set_replacement_codepoint(IntPtr buffer, uint replacement);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_set_script(IntPtr buffer, uint script);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_buffer_set_unicode_funcs(IntPtr buffer, IntPtr unicode_funcs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern GlyphFlags hb_glyph_info_get_glyph_flags(GlyphInfo* info);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern byte hb_color_get_alpha(uint color);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern byte hb_color_get_blue(uint color);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern byte hb_color_get_green(uint color);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern byte hb_color_get_red(uint color);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern Direction hb_direction_from_string(void* str, int len);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* hb_direction_to_string(Direction direction);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_feature_from_string([MarshalAs(UnmanagedType.LPStr)] string str, int len, Feature* feature);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_feature_to_string(Feature* feature, void* buf, uint size);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_language_from_string([MarshalAs(UnmanagedType.LPStr)] string str, int len);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_language_get_default();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* hb_language_to_string(IntPtr language);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_script_from_iso15924_tag(uint tag);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_script_from_string([MarshalAs(UnmanagedType.LPStr)] string str, int len);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern Direction hb_script_get_horizontal_direction(uint script);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_script_to_iso15924_tag(uint script);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_tag_from_string(void* str, int len);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_tag_to_string(uint tag, void* buf);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_variation_from_string(void* str, int len, Variation* variation);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_variation_to_string(Variation* variation, void* buf, uint size);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_face_builder_add_table(IntPtr face, uint tag, IntPtr blob);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_face_builder_create();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_face_collect_unicodes(IntPtr face, IntPtr @out);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_face_collect_variation_selectors(IntPtr face, IntPtr @out);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_face_collect_variation_unicodes(IntPtr face, uint variation_selector, IntPtr @out);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_face_count(IntPtr blob);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_face_create(IntPtr blob, uint index);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr hb_face_create_for_tables(ReferenceTableProxyDelegate reference_table_func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_face_destroy(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_face_get_empty();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_face_get_glyph_count(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_face_get_index(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_face_get_table_tags(IntPtr face, uint start_offset, uint* table_count, uint* table_tags);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_face_get_upem(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_face_is_immutable(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_face_make_immutable(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_face_reference(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_face_reference_blob(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_face_reference_table(IntPtr face, uint tag);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_face_set_glyph_count(IntPtr face, uint glyph_count);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_face_set_index(IntPtr face, uint index);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_face_set_upem(IntPtr face, uint upem);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_add_glyph_origin_for_direction(IntPtr font, uint glyph, Direction direction, int* x, int* y);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_font_create(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_font_create_sub_font(IntPtr parent);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_font_destroy(IntPtr font);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_font_funcs_create();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_font_funcs_destroy(IntPtr ffuncs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_font_funcs_get_empty();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_font_funcs_is_immutable(IntPtr ffuncs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_font_funcs_make_immutable(IntPtr ffuncs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_font_funcs_reference(IntPtr ffuncs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_font_h_extents_func(IntPtr ffuncs, FontGetFontExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_font_v_extents_func(IntPtr ffuncs, FontGetFontExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_glyph_contour_point_func(IntPtr ffuncs, FontGetGlyphContourPointProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_glyph_extents_func(IntPtr ffuncs, FontGetGlyphExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_glyph_from_name_func(IntPtr ffuncs, FontGetGlyphFromNameProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_glyph_h_advance_func(IntPtr ffuncs, FontGetGlyphAdvanceProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_glyph_h_advances_func(IntPtr ffuncs, FontGetGlyphAdvancesProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_glyph_h_kerning_func(IntPtr ffuncs, FontGetGlyphKerningProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_glyph_h_origin_func(IntPtr ffuncs, FontGetGlyphOriginProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_glyph_name_func(IntPtr ffuncs, FontGetGlyphNameProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_glyph_v_advance_func(IntPtr ffuncs, FontGetGlyphAdvanceProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_glyph_v_advances_func(IntPtr ffuncs, FontGetGlyphAdvancesProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_glyph_v_origin_func(IntPtr ffuncs, FontGetGlyphOriginProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_nominal_glyph_func(IntPtr ffuncs, FontGetNominalGlyphProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_nominal_glyphs_func(IntPtr ffuncs, FontGetNominalGlyphsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_funcs_set_variation_glyph_func(IntPtr ffuncs, FontGetVariationGlyphProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_font_get_empty();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_get_extents_for_direction(IntPtr font, Direction direction, FontExtents* extents);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_font_get_face(IntPtr font);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_glyph(IntPtr font, uint unicode, uint variation_selector, uint* glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_get_glyph_advance_for_direction(IntPtr font, uint glyph, Direction direction, int* x, int* y);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_get_glyph_advances_for_direction(IntPtr font, Direction direction, uint count, uint* first_glyph, uint glyph_stride, int* first_advance, uint advance_stride);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_glyph_contour_point(IntPtr font, uint glyph, uint point_index, int* x, int* y);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_glyph_contour_point_for_origin(IntPtr font, uint glyph, uint point_index, Direction direction, int* x, int* y);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_glyph_extents(IntPtr font, uint glyph, GlyphExtents* extents);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_glyph_extents_for_origin(IntPtr font, uint glyph, Direction direction, GlyphExtents* extents);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_glyph_from_name(IntPtr font, [MarshalAs(UnmanagedType.LPStr)] string name, int len, uint* glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int hb_font_get_glyph_h_advance(IntPtr font, uint glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_get_glyph_h_advances(IntPtr font, uint count, uint* first_glyph, uint glyph_stride, int* first_advance, uint advance_stride);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int hb_font_get_glyph_h_kerning(IntPtr font, uint left_glyph, uint right_glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_glyph_h_origin(IntPtr font, uint glyph, int* x, int* y);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_get_glyph_kerning_for_direction(IntPtr font, uint first_glyph, uint second_glyph, Direction direction, int* x, int* y);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_glyph_name(IntPtr font, uint glyph, void* name, uint size);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_get_glyph_origin_for_direction(IntPtr font, uint glyph, Direction direction, int* x, int* y);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int hb_font_get_glyph_v_advance(IntPtr font, uint glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_get_glyph_v_advances(IntPtr font, uint count, uint* first_glyph, uint glyph_stride, int* first_advance, uint advance_stride);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_glyph_v_origin(IntPtr font, uint glyph, int* x, int* y);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_h_extents(IntPtr font, FontExtents* extents);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_nominal_glyph(IntPtr font, uint unicode, uint* glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_font_get_nominal_glyphs(IntPtr font, uint count, uint* first_unicode, uint unicode_stride, uint* first_glyph, uint glyph_stride);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_font_get_parent(IntPtr font);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_get_ppem(IntPtr font, uint* x_ppem, uint* y_ppem);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float hb_font_get_ptem(IntPtr font);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_get_scale(IntPtr font, int* x_scale, int* y_scale);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_v_extents(IntPtr font, FontExtents* extents);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern int* hb_font_get_var_coords_normalized(IntPtr font, uint* length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_get_variation_glyph(IntPtr font, uint unicode, uint variation_selector, uint* glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_font_glyph_from_string(IntPtr font, [MarshalAs(UnmanagedType.LPStr)] string s, int len, uint* glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_glyph_to_string(IntPtr font, uint glyph, void* s, uint size);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_font_is_immutable(IntPtr font);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_font_make_immutable(IntPtr font);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_font_reference(IntPtr font);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_font_set_face(IntPtr font, IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_set_funcs(IntPtr font, IntPtr klass, void* font_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_set_funcs_data(IntPtr font, void* font_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_font_set_parent(IntPtr font, IntPtr parent);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_font_set_ppem(IntPtr font, uint x_ppem, uint y_ppem);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_font_set_ptem(IntPtr font, float ptem);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_font_set_scale(IntPtr font, int x_scale, int y_scale);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_set_var_coords_design(IntPtr font, float* coords, uint coords_length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_set_var_coords_normalized(IntPtr font, int* coords, uint coords_length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_font_set_var_named_instance(IntPtr font, uint instance_index);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_set_variations(IntPtr font, Variation* variations, uint variations_length);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_font_subtract_glyph_origin_for_direction(IntPtr font, uint glyph, Direction direction, int* x, int* y);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_map_allocation_successful(IntPtr map);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_map_clear(IntPtr map);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_map_create();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_map_del(IntPtr map, uint key);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_map_destroy(IntPtr map);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_map_get(IntPtr map, uint key);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_map_get_empty();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_map_get_population(IntPtr map);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_map_has(IntPtr map, uint key);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_map_is_empty(IntPtr map);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_map_reference(IntPtr map);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_map_set(IntPtr map, uint key, uint value);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_color_glyph_get_layers(IntPtr face, uint glyph, uint start_offset, uint* layer_count, OpenTypeColorLayer* layers);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_ot_color_glyph_reference_png(IntPtr font, uint glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_ot_color_glyph_reference_svg(IntPtr face, uint glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_ot_color_has_layers(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_ot_color_has_palettes(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_ot_color_has_png(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_ot_color_has_svg(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern OpenTypeNameId hb_ot_color_palette_color_get_name_id(IntPtr face, uint color_index);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_color_palette_get_colors(IntPtr face, uint palette_index, uint start_offset, uint* color_count, uint* colors);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_ot_color_palette_get_count(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern OpenTypeColorPaletteFlags hb_ot_color_palette_get_flags(IntPtr face, uint palette_index);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern OpenTypeNameId hb_ot_color_palette_get_name_id(IntPtr face, uint palette_index);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_ot_font_set_funcs(IntPtr font);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_ot_layout_collect_features(IntPtr face, uint table_tag, uint* scripts, uint* languages, uint* features, IntPtr feature_indexes);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_ot_layout_collect_lookups(IntPtr face, uint table_tag, uint* scripts, uint* languages, uint* features, IntPtr lookup_indexes);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_layout_feature_get_characters(IntPtr face, uint table_tag, uint feature_index, uint start_offset, uint* char_count, uint* characters);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_layout_feature_get_lookups(IntPtr face, uint table_tag, uint feature_index, uint start_offset, uint* lookup_count, uint* lookup_indexes);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_ot_layout_feature_get_name_ids(IntPtr face, uint table_tag, uint feature_index, OpenTypeNameId* label_id, OpenTypeNameId* tooltip_id, OpenTypeNameId* sample_id, uint* num_named_parameters, OpenTypeNameId* first_param_id);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_layout_feature_with_variations_get_lookups(IntPtr face, uint table_tag, uint feature_index, uint variations_index, uint start_offset, uint* lookup_count, uint* lookup_indexes);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_layout_get_attach_points(IntPtr face, uint glyph, uint start_offset, uint* point_count, uint* point_array);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_ot_layout_get_baseline(IntPtr font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, uint script_tag, uint language_tag, int* coord);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern OpenTypeLayoutGlyphClass hb_ot_layout_get_glyph_class(IntPtr face, uint glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_ot_layout_get_glyphs_in_class(IntPtr face, OpenTypeLayoutGlyphClass klass, IntPtr glyphs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_layout_get_ligature_carets(IntPtr font, Direction direction, uint glyph, uint start_offset, uint* caret_count, int* caret_array);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_ot_layout_get_size_params(IntPtr face, uint* design_size, uint* subfamily_id, OpenTypeNameId* subfamily_name_id, uint* range_start, uint* range_end);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_ot_layout_has_glyph_classes(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_ot_layout_has_positioning(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_ot_layout_has_substitution(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_ot_layout_language_find_feature(IntPtr face, uint table_tag, uint script_index, uint language_index, uint feature_tag, uint* feature_index);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_layout_language_get_feature_indexes(IntPtr face, uint table_tag, uint script_index, uint language_index, uint start_offset, uint* feature_count, uint* feature_indexes);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_layout_language_get_feature_tags(IntPtr face, uint table_tag, uint script_index, uint language_index, uint start_offset, uint* feature_count, uint* feature_tags);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_ot_layout_language_get_required_feature(IntPtr face, uint table_tag, uint script_index, uint language_index, uint* feature_index, uint* feature_tag);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_ot_layout_language_get_required_feature_index(IntPtr face, uint table_tag, uint script_index, uint language_index, uint* feature_index);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_ot_layout_lookup_collect_glyphs(IntPtr face, uint table_tag, uint lookup_index, IntPtr glyphs_before, IntPtr glyphs_input, IntPtr glyphs_after, IntPtr glyphs_output);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_layout_lookup_get_glyph_alternates(IntPtr face, uint lookup_index, uint glyph, uint start_offset, uint* alternate_count, uint* alternate_glyphs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_ot_layout_lookup_substitute_closure(IntPtr face, uint lookup_index, IntPtr glyphs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_ot_layout_lookup_would_substitute(IntPtr face, uint lookup_index, uint* glyphs, uint glyphs_length, [MarshalAs(UnmanagedType.I1)] bool zero_context);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_ot_layout_lookups_substitute_closure(IntPtr face, IntPtr lookups, IntPtr glyphs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_layout_script_get_language_tags(IntPtr face, uint table_tag, uint script_index, uint start_offset, uint* language_count, uint* language_tags);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_ot_layout_script_select_language(IntPtr face, uint table_tag, uint script_index, uint language_count, uint* language_tags, uint* language_index);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_ot_layout_table_find_feature_variations(IntPtr face, uint table_tag, int* coords, uint num_coords, uint* variations_index);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_ot_layout_table_find_script(IntPtr face, uint table_tag, uint script_tag, uint* script_index);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_layout_table_get_feature_tags(IntPtr face, uint table_tag, uint start_offset, uint* feature_count, uint* feature_tags);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_ot_layout_table_get_lookup_count(IntPtr face, uint table_tag);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_layout_table_get_script_tags(IntPtr face, uint table_tag, uint start_offset, uint* script_count, uint* script_tags);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_ot_layout_table_select_script(IntPtr face, uint table_tag, uint script_count, uint* script_tags, uint* script_index, uint* chosen_script);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_ot_tag_to_language(uint tag);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_ot_tag_to_script(uint tag);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_ot_tags_from_script_and_language(uint script, IntPtr language, uint* script_count, uint* script_tags, uint* language_count, uint* language_tags);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_ot_tags_to_script_and_language(uint script_tag, uint language_tag, uint* script, IntPtr* language);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int hb_ot_math_get_constant(IntPtr font, OpenTypeMathConstant constant);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_math_get_glyph_assembly(IntPtr font, uint glyph, Direction direction, uint start_offset, uint* parts_count, OpenTypeMathGlyphPart* parts, int* italics_correction);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int hb_ot_math_get_glyph_italics_correction(IntPtr font, uint glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int hb_ot_math_get_glyph_kerning(IntPtr font, uint glyph, OpenTypeMathKern kern, int correction_height);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int hb_ot_math_get_glyph_top_accent_attachment(IntPtr font, uint glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_math_get_glyph_variants(IntPtr font, uint glyph, Direction direction, uint start_offset, uint* variants_count, OpenTypeMathGlyphVariant* variants);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int hb_ot_math_get_min_connector_overlap(IntPtr font, Direction direction);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_ot_math_has_data(IntPtr face);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_ot_math_is_glyph_extended_shape(IntPtr face, uint glyph);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_meta_get_entry_tags(IntPtr face, uint start_offset, uint* entries_count, OpenTypeMetaTag* entries);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_ot_meta_reference_entry(IntPtr face, OpenTypeMetaTag meta_tag);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_ot_metrics_get_position(IntPtr font, OpenTypeMetricsTag metrics_tag, int* position);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float hb_ot_metrics_get_variation(IntPtr font, OpenTypeMetricsTag metrics_tag);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int hb_ot_metrics_get_x_variation(IntPtr font, OpenTypeMetricsTag metrics_tag);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int hb_ot_metrics_get_y_variation(IntPtr font, OpenTypeMetricsTag metrics_tag);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_name_get_utf16(IntPtr face, OpenTypeNameId name_id, IntPtr language, uint* text_size, ushort* text);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_name_get_utf32(IntPtr face, OpenTypeNameId name_id, IntPtr language, uint* text_size, uint* text);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint hb_ot_name_get_utf8(IntPtr face, OpenTypeNameId name_id, IntPtr language, uint* text_size, void* text);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern OpenTypeNameEntry* hb_ot_name_list_names(IntPtr face, uint* num_entries);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_ot_shape_glyphs_closure(IntPtr font, IntPtr buffer, Feature* features, uint num_features, IntPtr glyphs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_ot_shape_plan_collect_lookups(IntPtr shape_plan, uint table_tag, IntPtr lookup_indexes);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_set_add(IntPtr set, uint codepoint);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_set_add_range(IntPtr set, uint first, uint last);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_set_allocation_successful(IntPtr set);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_set_clear(IntPtr set);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_set_copy(IntPtr set);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_set_create();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_set_del(IntPtr set, uint codepoint);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_set_del_range(IntPtr set, uint first, uint last);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_set_destroy(IntPtr set);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_set_get_empty();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_set_get_max(IntPtr set);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_set_get_min(IntPtr set);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_set_get_population(IntPtr set);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_set_has(IntPtr set, uint codepoint);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_set_intersect(IntPtr set, IntPtr other);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_set_is_empty(IntPtr set);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_set_is_equal(IntPtr set, IntPtr other);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_set_is_subset(IntPtr set, IntPtr larger_set);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_set_next(IntPtr set, uint* codepoint);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_set_next_range(IntPtr set, uint* first, uint* last);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_set_previous(IntPtr set, uint* codepoint);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_set_previous_range(IntPtr set, uint* first, uint* last);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_set_reference(IntPtr set);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_set_set(IntPtr set, IntPtr other);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_set_subtract(IntPtr set, IntPtr other);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_set_symmetric_difference(IntPtr set, IntPtr other);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_set_union(IntPtr set, IntPtr other);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_shape(IntPtr font, IntPtr buffer, Feature* features, uint num_features);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_shape_full(IntPtr font, IntPtr buffer, Feature* features, uint num_features, void** shaper_list);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void** hb_shape_list_shapers();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern UnicodeCombiningClass hb_unicode_combining_class(IntPtr ufuncs, uint unicode);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_unicode_compose(IntPtr ufuncs, uint a, uint b, uint* ab);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool hb_unicode_decompose(IntPtr ufuncs, uint ab, uint* a, uint* b);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_unicode_funcs_create(IntPtr parent);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_unicode_funcs_destroy(IntPtr ufuncs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_unicode_funcs_get_default();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_unicode_funcs_get_empty();

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_unicode_funcs_get_parent(IntPtr ufuncs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_unicode_funcs_is_immutable(IntPtr ufuncs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void hb_unicode_funcs_make_immutable(IntPtr ufuncs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr hb_unicode_funcs_reference(IntPtr ufuncs);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_unicode_funcs_set_combining_class_func(IntPtr ufuncs, UnicodeCombiningClassProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_unicode_funcs_set_compose_func(IntPtr ufuncs, UnicodeComposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_unicode_funcs_set_decompose_func(IntPtr ufuncs, UnicodeDecomposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_unicode_funcs_set_general_category_func(IntPtr ufuncs, UnicodeGeneralCategoryProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_unicode_funcs_set_mirroring_func(IntPtr ufuncs, UnicodeMirroringProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_unicode_funcs_set_script_func(IntPtr ufuncs, UnicodeScriptProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern UnicodeGeneralCategory hb_unicode_general_category(IntPtr ufuncs, uint unicode);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_unicode_mirroring(IntPtr ufuncs, uint unicode);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint hb_unicode_script(IntPtr ufuncs, uint unicode);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void hb_version(uint* major, uint* minor, uint* micro);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool hb_version_atleast(uint major, uint minor, uint micro);

	[DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* hb_version_string();
}
