using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

internal class SkiaApi
{
	private const string SKIA = "libSkiaSharp";

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_backendrendertarget_delete(IntPtr rendertarget);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern GRBackendNative gr_backendrendertarget_get_backend(IntPtr rendertarget);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool gr_backendrendertarget_get_gl_framebufferinfo(IntPtr rendertarget, GRGlFramebufferInfo* glInfo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int gr_backendrendertarget_get_height(IntPtr rendertarget);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int gr_backendrendertarget_get_samples(IntPtr rendertarget);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int gr_backendrendertarget_get_stencils(IntPtr rendertarget);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int gr_backendrendertarget_get_width(IntPtr rendertarget);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool gr_backendrendertarget_is_valid(IntPtr rendertarget);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_backendrendertarget_new_gl(int width, int height, int samples, int stencils, GRGlFramebufferInfo* glInfo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_backendrendertarget_new_metal(int width, int height, int samples, GRMtlTextureInfoNative* mtlInfo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_backendrendertarget_new_vulkan(int width, int height, int samples, GRVkImageInfo* vkImageInfo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_backendtexture_delete(IntPtr texture);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern GRBackendNative gr_backendtexture_get_backend(IntPtr texture);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool gr_backendtexture_get_gl_textureinfo(IntPtr texture, GRGlTextureInfo* glInfo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int gr_backendtexture_get_height(IntPtr texture);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int gr_backendtexture_get_width(IntPtr texture);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool gr_backendtexture_has_mipmaps(IntPtr texture);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool gr_backendtexture_is_valid(IntPtr texture);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_backendtexture_new_gl(int width, int height, [MarshalAs(UnmanagedType.I1)] bool mipmapped, GRGlTextureInfo* glInfo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_backendtexture_new_metal(int width, int height, [MarshalAs(UnmanagedType.I1)] bool mipmapped, GRMtlTextureInfoNative* mtlInfo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_backendtexture_new_vulkan(int width, int height, GRVkImageInfo* vkInfo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_direct_context_abandon_context(IntPtr context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_direct_context_dump_memory_statistics(IntPtr context, IntPtr dump);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_direct_context_flush(IntPtr context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_direct_context_flush_and_submit(IntPtr context, [MarshalAs(UnmanagedType.I1)] bool syncCpu);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_direct_context_free_gpu_resources(IntPtr context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr gr_direct_context_get_resource_cache_limit(IntPtr context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void gr_direct_context_get_resource_cache_usage(IntPtr context, int* maxResources, IntPtr* maxResourceBytes);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool gr_direct_context_is_abandoned(IntPtr context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr gr_direct_context_make_gl(IntPtr glInterface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_direct_context_make_gl_with_options(IntPtr glInterface, GRContextOptionsNative* options);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_direct_context_make_metal(void* device, void* queue);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_direct_context_make_metal_with_options(void* device, void* queue, GRContextOptionsNative* options);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr gr_direct_context_make_vulkan(GRVkBackendContextNative vkBackendContext);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_direct_context_make_vulkan_with_options(GRVkBackendContextNative vkBackendContext, GRContextOptionsNative* options);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_direct_context_perform_deferred_cleanup(IntPtr context, long ms);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_direct_context_purge_unlocked_resources(IntPtr context, [MarshalAs(UnmanagedType.I1)] bool scratchResourcesOnly);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_direct_context_purge_unlocked_resources_bytes(IntPtr context, IntPtr bytesToPurge, [MarshalAs(UnmanagedType.I1)] bool preferScratchResources);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_direct_context_release_resources_and_abandon_context(IntPtr context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_direct_context_reset_context(IntPtr context, uint state);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_direct_context_set_resource_cache_limit(IntPtr context, IntPtr maxResourceBytes);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool gr_direct_context_submit(IntPtr context, [MarshalAs(UnmanagedType.I1)] bool syncCpu);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_glinterface_assemble_gl_interface(void* ctx, GRGlGetProcProxyDelegate get);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_glinterface_assemble_gles_interface(void* ctx, GRGlGetProcProxyDelegate get);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_glinterface_assemble_interface(void* ctx, GRGlGetProcProxyDelegate get);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr gr_glinterface_assemble_webgl_interface(void* ctx, GRGlGetProcProxyDelegate get);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr gr_glinterface_create_native_interface();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool gr_glinterface_has_extension(IntPtr glInterface, [MarshalAs(UnmanagedType.LPStr)] string extension);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_glinterface_unref(IntPtr glInterface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool gr_glinterface_validate(IntPtr glInterface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern GRBackendNative gr_recording_context_get_backend(IntPtr context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int gr_recording_context_get_max_surface_sample_count_for_color_type(IntPtr context, SKColorTypeNative colorType);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_recording_context_unref(IntPtr context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void gr_vk_extensions_delete(IntPtr extensions);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool gr_vk_extensions_has_extension(IntPtr extensions, [MarshalAs(UnmanagedType.LPStr)] string ext, uint minVersion);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void gr_vk_extensions_init(IntPtr extensions, GRVkGetProcProxyDelegate getProc, void* userData, IntPtr instance, IntPtr physDev, uint instanceExtensionCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] instanceExtensions, uint deviceExtensionCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] deviceExtensions);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr gr_vk_extensions_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_bitmap_destructor(IntPtr cbitmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_bitmap_erase(IntPtr cbitmap, uint color);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_bitmap_erase_rect(IntPtr cbitmap, uint color, SKRectI* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_bitmap_extract_alpha(IntPtr cbitmap, IntPtr dst, IntPtr paint, SKPointI* offset);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_bitmap_extract_subset(IntPtr cbitmap, IntPtr dst, SKRectI* subset);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* sk_bitmap_get_addr(IntPtr cbitmap, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern ushort* sk_bitmap_get_addr_16(IntPtr cbitmap, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint* sk_bitmap_get_addr_32(IntPtr cbitmap, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern byte* sk_bitmap_get_addr_8(IntPtr cbitmap, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_bitmap_get_byte_count(IntPtr cbitmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_bitmap_get_info(IntPtr cbitmap, SKImageInfoNative* info);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint sk_bitmap_get_pixel_color(IntPtr cbitmap, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_bitmap_get_pixel_colors(IntPtr cbitmap, uint* colors);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* sk_bitmap_get_pixels(IntPtr cbitmap, IntPtr* length);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_bitmap_get_row_bytes(IntPtr cbitmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_bitmap_install_mask_pixels(IntPtr cbitmap, SKMask* cmask);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_bitmap_install_pixels(IntPtr cbitmap, SKImageInfoNative* cinfo, void* pixels, IntPtr rowBytes, SKBitmapReleaseProxyDelegate releaseProc, void* context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_bitmap_install_pixels_with_pixmap(IntPtr cbitmap, IntPtr cpixmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_bitmap_is_immutable(IntPtr cbitmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_bitmap_is_null(IntPtr cbitmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_bitmap_make_shader(IntPtr cbitmap, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix* cmatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_bitmap_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_bitmap_notify_pixels_changed(IntPtr cbitmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_bitmap_peek_pixels(IntPtr cbitmap, IntPtr cpixmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_bitmap_ready_to_draw(IntPtr cbitmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_bitmap_reset(IntPtr cbitmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_bitmap_set_immutable(IntPtr cbitmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_bitmap_set_pixels(IntPtr cbitmap, void* pixels);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_bitmap_swap(IntPtr cbitmap, IntPtr cother);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_bitmap_try_alloc_pixels(IntPtr cbitmap, SKImageInfoNative* requestedInfo, IntPtr rowBytes);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_bitmap_try_alloc_pixels_with_flags(IntPtr cbitmap, SKImageInfoNative* requestedInfo, uint flags);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_clear(IntPtr param0, uint param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_clear_color4f(IntPtr param0, SKColorF param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_clip_path_with_operation(IntPtr t, IntPtr crect, SKClipOperation op, [MarshalAs(UnmanagedType.I1)] bool doAA);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_clip_rect_with_operation(IntPtr t, SKRect* crect, SKClipOperation op, [MarshalAs(UnmanagedType.I1)] bool doAA);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_clip_region(IntPtr canvas, IntPtr region, SKClipOperation op);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_clip_rrect_with_operation(IntPtr t, IntPtr crect, SKClipOperation op, [MarshalAs(UnmanagedType.I1)] bool doAA);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_concat(IntPtr param0, SKMatrix* param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_destroy(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_discard(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_annotation(IntPtr t, SKRect* rect, void* key, IntPtr value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_arc(IntPtr ccanvas, SKRect* oval, float startAngle, float sweepAngle, [MarshalAs(UnmanagedType.I1)] bool useCenter, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_atlas(IntPtr ccanvas, IntPtr atlas, SKRotationScaleMatrix* xform, SKRect* tex, uint* colors, int count, SKBlendMode mode, SKRect* cullRect, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_circle(IntPtr param0, float cx, float cy, float rad, IntPtr param4);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_color(IntPtr ccanvas, uint color, SKBlendMode mode);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_color4f(IntPtr ccanvas, SKColorF color, SKBlendMode mode);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_drawable(IntPtr param0, IntPtr param1, SKMatrix* param2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_drrect(IntPtr ccanvas, IntPtr outer, IntPtr inner, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_image(IntPtr param0, IntPtr param1, float x, float y, IntPtr param4);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_image_lattice(IntPtr t, IntPtr image, SKLatticeInternal* lattice, SKRect* dst, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_image_nine(IntPtr t, IntPtr image, SKRectI* center, SKRect* dst, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_image_rect(IntPtr param0, IntPtr param1, SKRect* src, SKRect* dst, IntPtr param4);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_line(IntPtr ccanvas, float x0, float y0, float x1, float y1, IntPtr cpaint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_link_destination_annotation(IntPtr t, SKRect* rect, IntPtr value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_named_destination_annotation(IntPtr t, SKPoint* point, IntPtr value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_oval(IntPtr param0, SKRect* param1, IntPtr param2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_paint(IntPtr param0, IntPtr param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_patch(IntPtr ccanvas, SKPoint* cubics, uint* colors, SKPoint* texCoords, SKBlendMode mode, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_path(IntPtr param0, IntPtr param1, IntPtr param2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_picture(IntPtr param0, IntPtr param1, SKMatrix* param2, IntPtr param3);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_point(IntPtr param0, float param1, float param2, IntPtr param3);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_points(IntPtr param0, SKPointMode param1, IntPtr param2, SKPoint* param3, IntPtr param4);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_rect(IntPtr param0, SKRect* param1, IntPtr param2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_region(IntPtr param0, IntPtr param1, IntPtr param2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_round_rect(IntPtr param0, SKRect* param1, float rx, float ry, IntPtr param4);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_rrect(IntPtr param0, IntPtr param1, IntPtr param2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_simple_text(IntPtr ccanvas, void* text, IntPtr byte_length, SKTextEncoding encoding, float x, float y, IntPtr cfont, IntPtr cpaint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_text_blob(IntPtr param0, IntPtr text, float x, float y, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_draw_url_annotation(IntPtr t, SKRect* rect, IntPtr value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_draw_vertices(IntPtr ccanvas, IntPtr vertices, SKBlendMode mode, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_flush(IntPtr ccanvas);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_canvas_get_device_clip_bounds(IntPtr t, SKRectI* cbounds);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_canvas_get_local_clip_bounds(IntPtr t, SKRect* cbounds);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_canvas_get_save_count(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_get_total_matrix(IntPtr ccanvas, SKMatrix* matrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_canvas_is_clip_empty(IntPtr ccanvas);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_canvas_is_clip_rect(IntPtr ccanvas);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_canvas_new_from_bitmap(IntPtr bitmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_canvas_quick_reject(IntPtr param0, SKRect* param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_reset_matrix(IntPtr ccanvas);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_restore(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_restore_to_count(IntPtr param0, int saveCount);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_rotate_degrees(IntPtr param0, float degrees);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_rotate_radians(IntPtr param0, float radians);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_canvas_save(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern int sk_canvas_save_layer(IntPtr param0, SKRect* param1, IntPtr param2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_scale(IntPtr param0, float sx, float sy);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_canvas_set_matrix(IntPtr ccanvas, SKMatrix* matrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_skew(IntPtr param0, float sx, float sy);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_canvas_translate(IntPtr param0, float dx, float dy);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_nodraw_canvas_destroy(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_nodraw_canvas_new(int width, int height);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_nway_canvas_add_canvas(IntPtr param0, IntPtr canvas);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_nway_canvas_destroy(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_nway_canvas_new(int width, int height);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_nway_canvas_remove_all(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_nway_canvas_remove_canvas(IntPtr param0, IntPtr canvas);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_overdraw_canvas_destroy(IntPtr canvas);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_overdraw_canvas_new(IntPtr canvas);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_codec_destroy(IntPtr codec);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKEncodedImageFormat sk_codec_get_encoded_format(IntPtr codec);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_codec_get_frame_count(IntPtr codec);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_codec_get_frame_info(IntPtr codec, SKCodecFrameInfo* frameInfo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_codec_get_frame_info_for_index(IntPtr codec, int index, SKCodecFrameInfo* frameInfo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_codec_get_info(IntPtr codec, SKImageInfoNative* info);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKEncodedOrigin sk_codec_get_origin(IntPtr codec);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern SKCodecResult sk_codec_get_pixels(IntPtr codec, SKImageInfoNative* info, void* pixels, IntPtr rowBytes, SKCodecOptionsInternal* options);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_codec_get_repetition_count(IntPtr codec);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_codec_get_scaled_dimensions(IntPtr codec, float desiredScale, SKSizeI* dimensions);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKCodecScanlineOrder sk_codec_get_scanline_order(IntPtr codec);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern int sk_codec_get_scanlines(IntPtr codec, void* dst, int countLines, IntPtr rowBytes);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_codec_get_valid_subset(IntPtr codec, SKRectI* desiredSubset);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern SKCodecResult sk_codec_incremental_decode(IntPtr codec, int* rowsDecoded);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_codec_min_buffered_bytes_needed();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_codec_new_from_data(IntPtr data);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_codec_new_from_stream(IntPtr stream, SKCodecResult* result);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_codec_next_scanline(IntPtr codec);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_codec_output_scanline(IntPtr codec, int inputScanline);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_codec_skip_scanlines(IntPtr codec, int countLines);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern SKCodecResult sk_codec_start_incremental_decode(IntPtr codec, SKImageInfoNative* info, void* pixels, IntPtr rowBytes, SKCodecOptionsInternal* options);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern SKCodecResult sk_codec_start_scanline_decode(IntPtr codec, SKImageInfoNative* info, SKCodecOptionsInternal* options);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_colorfilter_new_color_matrix(float* array);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_colorfilter_new_compose(IntPtr outer, IntPtr inner);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_colorfilter_new_high_contrast(SKHighContrastConfig* config);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_colorfilter_new_lighting(uint mul, uint add);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_colorfilter_new_luma_color();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_colorfilter_new_mode(uint c, SKBlendMode mode);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_colorfilter_new_table(byte* table);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_colorfilter_new_table_argb(byte* tableA, byte* tableR, byte* tableG, byte* tableB);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_colorfilter_unref(IntPtr filter);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_color4f_from_color(uint color, SKColorF* color4f);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint sk_color4f_to_color(SKColorF* color4f);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_colorspace_equals(IntPtr src, IntPtr dst);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_colorspace_gamma_close_to_srgb(IntPtr colorspace);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_colorspace_gamma_is_linear(IntPtr colorspace);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_colorspace_icc_profile_delete(IntPtr profile);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern byte* sk_colorspace_icc_profile_get_buffer(IntPtr profile, uint* size);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_colorspace_icc_profile_get_to_xyzd50(IntPtr profile, SKColorSpaceXyz* toXYZD50);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_colorspace_icc_profile_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_colorspace_icc_profile_parse(void* buffer, IntPtr length, IntPtr profile);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_colorspace_is_numerical_transfer_fn(IntPtr colorspace, SKColorSpaceTransferFn* transferFn);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_colorspace_is_srgb(IntPtr colorspace);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_colorspace_make_linear_gamma(IntPtr colorspace);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_colorspace_make_srgb_gamma(IntPtr colorspace);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_colorspace_new_icc(IntPtr profile);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_colorspace_new_rgb(SKColorSpaceTransferFn* transferFn, SKColorSpaceXyz* toXYZD50);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_colorspace_new_srgb();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_colorspace_new_srgb_linear();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_colorspace_primaries_to_xyzd50(SKColorSpacePrimaries* primaries, SKColorSpaceXyz* toXYZD50);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_colorspace_ref(IntPtr colorspace);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_colorspace_to_profile(IntPtr colorspace, IntPtr profile);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_colorspace_to_xyzd50(IntPtr colorspace, SKColorSpaceXyz* toXYZD50);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern float sk_colorspace_transfer_fn_eval(SKColorSpaceTransferFn* transferFn, float x);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_colorspace_transfer_fn_invert(SKColorSpaceTransferFn* src, SKColorSpaceTransferFn* dst);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colorspace_transfer_fn_named_2dot2(SKColorSpaceTransferFn* transferFn);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colorspace_transfer_fn_named_hlg(SKColorSpaceTransferFn* transferFn);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colorspace_transfer_fn_named_linear(SKColorSpaceTransferFn* transferFn);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colorspace_transfer_fn_named_pq(SKColorSpaceTransferFn* transferFn);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colorspace_transfer_fn_named_rec2020(SKColorSpaceTransferFn* transferFn);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colorspace_transfer_fn_named_srgb(SKColorSpaceTransferFn* transferFn);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_colorspace_unref(IntPtr colorspace);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colorspace_xyz_concat(SKColorSpaceXyz* a, SKColorSpaceXyz* b, SKColorSpaceXyz* result);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_colorspace_xyz_invert(SKColorSpaceXyz* src, SKColorSpaceXyz* dst);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colorspace_xyz_named_adobe_rgb(SKColorSpaceXyz* xyz);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colorspace_xyz_named_display_p3(SKColorSpaceXyz* xyz);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colorspace_xyz_named_rec2020(SKColorSpaceXyz* xyz);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colorspace_xyz_named_srgb(SKColorSpaceXyz* xyz);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colorspace_xyz_named_xyz(SKColorSpaceXyz* xyz);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_colortable_count(IntPtr ctable);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_colortable_new(uint* colors, int count);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_colortable_read_colors(IntPtr ctable, uint** colors);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_colortable_unref(IntPtr ctable);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern byte* sk_data_get_bytes(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* sk_data_get_data(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_data_get_size(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_data_new_empty();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_data_new_from_file(void* path);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_data_new_from_stream(IntPtr stream, IntPtr length);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_data_new_subset(IntPtr src, IntPtr offset, IntPtr length);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_data_new_uninitialized(IntPtr size);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_data_new_with_copy(void* src, IntPtr length);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_data_new_with_proc(void* ptr, IntPtr length, SKDataReleaseProxyDelegate proc, void* ctx);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_data_ref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_data_unref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_document_abort(IntPtr document);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_document_begin_page(IntPtr document, float width, float height, SKRect* content);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_document_close(IntPtr document);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_document_create_pdf_from_stream(IntPtr stream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_document_create_pdf_from_stream_with_metadata(IntPtr stream, SKDocumentPdfMetadataInternal* metadata);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_document_create_xps_from_stream(IntPtr stream, float dpi);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_document_end_page(IntPtr document);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_document_unref(IntPtr document);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_drawable_draw(IntPtr param0, IntPtr param1, SKMatrix* param2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_drawable_get_bounds(IntPtr param0, SKRect* param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint sk_drawable_get_generation_id(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_drawable_new_picture_snapshot(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_drawable_notify_drawing_changed(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_drawable_unref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_font_break_text(IntPtr font, void* text, IntPtr byteLength, SKTextEncoding encoding, float maxWidth, float* measuredWidth, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_delete(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKFontEdging sk_font_get_edging(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKFontHinting sk_font_get_hinting(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern float sk_font_get_metrics(IntPtr font, SKFontMetrics* metrics);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_font_get_path(IntPtr font, ushort glyph, IntPtr path);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_font_get_paths(IntPtr font, ushort* glyphs, int count, SKGlyphPathProxyDelegate glyphPathProc, void* context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_font_get_pos(IntPtr font, ushort* glyphs, int count, SKPoint* pos, SKPoint* origin);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float sk_font_get_scale_x(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float sk_font_get_size(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float sk_font_get_skew_x(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_font_get_typeface(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_font_get_widths_bounds(IntPtr font, ushort* glyphs, int count, float* widths, SKRect* bounds, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_font_get_xpos(IntPtr font, ushort* glyphs, int count, float* xpos, float origin);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_font_is_baseline_snap(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_font_is_embedded_bitmaps(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_font_is_embolden(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_font_is_force_auto_hinting(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_font_is_linear_metrics(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_font_is_subpixel(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern float sk_font_measure_text(IntPtr font, void* text, IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_font_measure_text_no_return(IntPtr font, void* text, IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint, float* measuredWidth);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_font_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_font_new_with_values(IntPtr typeface, float size, float scaleX, float skewX);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_set_baseline_snap(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_set_edging(IntPtr font, SKFontEdging value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_set_embedded_bitmaps(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_set_embolden(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_set_force_auto_hinting(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_set_hinting(IntPtr font, SKFontHinting value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_set_linear_metrics(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_set_scale_x(IntPtr font, float value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_set_size(IntPtr font, float value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_set_skew_x(IntPtr font, float value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_set_subpixel(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_font_set_typeface(IntPtr font, IntPtr value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern int sk_font_text_to_glyphs(IntPtr font, void* text, IntPtr byteLength, SKTextEncoding encoding, ushort* glyphs, int maxGlyphCount);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern ushort sk_font_unichar_to_glyph(IntPtr font, int uni);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_font_unichars_to_glyphs(IntPtr font, int* uni, int count, ushort* glyphs);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_text_utils_get_path(void* text, IntPtr length, SKTextEncoding encoding, float x, float y, IntPtr font, IntPtr path);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_text_utils_get_pos_path(void* text, IntPtr length, SKTextEncoding encoding, SKPoint* pos, IntPtr font, IntPtr path);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKColorTypeNative sk_colortype_get_default_8888();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_nvrefcnt_get_ref_count(IntPtr refcnt);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_nvrefcnt_safe_ref(IntPtr refcnt);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_nvrefcnt_safe_unref(IntPtr refcnt);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_nvrefcnt_unique(IntPtr refcnt);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_refcnt_get_ref_count(IntPtr refcnt);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_refcnt_safe_ref(IntPtr refcnt);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_refcnt_safe_unref(IntPtr refcnt);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_refcnt_unique(IntPtr refcnt);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_version_get_increment();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_version_get_milestone();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* sk_version_get_string();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_graphics_dump_memory_statistics(IntPtr dump);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_graphics_get_font_cache_count_limit();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_graphics_get_font_cache_count_used();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_graphics_get_font_cache_limit();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_graphics_get_font_cache_point_size_limit();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_graphics_get_font_cache_used();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_graphics_get_resource_cache_single_allocation_byte_limit();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_graphics_get_resource_cache_total_byte_limit();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_graphics_get_resource_cache_total_bytes_used();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_graphics_init();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_graphics_purge_all_caches();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_graphics_purge_font_cache();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_graphics_purge_resource_cache();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_graphics_set_font_cache_count_limit(int count);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_graphics_set_font_cache_limit(IntPtr bytes);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_graphics_set_font_cache_point_size_limit(int maxPointSize);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_graphics_set_resource_cache_single_allocation_byte_limit(IntPtr newLimit);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_graphics_set_resource_cache_total_byte_limit(IntPtr newLimit);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_image_encode(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_image_encode_specific(IntPtr cimage, SKEncodedImageFormat encoder, int quality);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKAlphaType sk_image_get_alpha_type(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKColorTypeNative sk_image_get_color_type(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_image_get_colorspace(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_image_get_height(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint sk_image_get_unique_id(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_image_get_width(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_image_is_alpha_only(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_image_is_lazy_generated(IntPtr image);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_image_is_texture_backed(IntPtr image);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_image_is_valid(IntPtr image, IntPtr context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_image_make_non_texture_image(IntPtr cimage);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_image_make_raster_image(IntPtr cimage);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_image_make_shader(IntPtr param0, SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix* localMatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_image_make_subset(IntPtr cimage, SKRectI* subset);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_image_make_texture_image(IntPtr cimage, IntPtr context, [MarshalAs(UnmanagedType.I1)] bool mipmapped);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_image_make_with_filter(IntPtr cimage, IntPtr context, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_image_make_with_filter_legacy(IntPtr cimage, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_image_new_from_adopted_texture(IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_image_new_from_bitmap(IntPtr cbitmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_image_new_from_encoded(IntPtr encoded);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_image_new_from_picture(IntPtr picture, SKSizeI* dimensions, SKMatrix* matrix, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_image_new_from_texture(IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace, SKImageTextureReleaseProxyDelegate releaseProc, void* releaseContext);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_image_new_raster(IntPtr pixmap, SKImageRasterReleaseProxyDelegate releaseProc, void* context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_image_new_raster_copy(SKImageInfoNative* param0, void* pixels, IntPtr rowBytes);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_image_new_raster_copy_with_pixmap(IntPtr pixmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_image_new_raster_data(SKImageInfoNative* cinfo, IntPtr pixels, IntPtr rowBytes);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_image_peek_pixels(IntPtr image, IntPtr pixmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_image_read_pixels(IntPtr image, SKImageInfoNative* dstInfo, void* dstPixels, IntPtr dstRowBytes, int srcX, int srcY, SKImageCachingHint cachingHint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_image_read_pixels_into_pixmap(IntPtr image, IntPtr dst, int srcX, int srcY, SKImageCachingHint cachingHint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_image_ref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_image_ref_encoded(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_image_scale_pixels(IntPtr image, IntPtr dst, SKFilterQuality quality, SKImageCachingHint cachingHint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_image_unref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_imagefilter_croprect_destructor(IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint sk_imagefilter_croprect_get_flags(IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_imagefilter_croprect_get_rect(IntPtr cropRect, SKRect* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_croprect_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_croprect_new_with_rect(SKRect* rect, uint flags);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_alpha_threshold(IntPtr region, float innerThreshold, float outerThreshold, IntPtr input);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_arithmetic(float k1, float k2, float k3, float k4, [MarshalAs(UnmanagedType.I1)] bool enforcePMColor, IntPtr background, IntPtr foreground, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_blur(float sigmaX, float sigmaY, SKShaderTileMode tileMode, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_color_filter(IntPtr cf, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_compose(IntPtr outer, IntPtr inner);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_dilate(float radiusX, float radiusY, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_displacement_map_effect(SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, IntPtr displacement, IntPtr color, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_distant_lit_diffuse(SKPoint3* direction, uint lightColor, float surfaceScale, float kd, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_distant_lit_specular(SKPoint3* direction, uint lightColor, float surfaceScale, float ks, float shininess, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_drop_shadow(float dx, float dy, float sigmaX, float sigmaY, uint color, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_drop_shadow_only(float dx, float dy, float sigmaX, float sigmaY, uint color, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_erode(float radiusX, float radiusY, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_image_source(IntPtr image, SKRect* srcRect, SKRect* dstRect, SKFilterQuality filterQuality);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_image_source_default(IntPtr image);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_magnifier(SKRect* src, float inset, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_matrix(SKMatrix* matrix, SKFilterQuality quality, IntPtr input);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_matrix_convolution(SKSizeI* kernelSize, float* kernel, float gain, float bias, SKPointI* kernelOffset, SKShaderTileMode tileMode, [MarshalAs(UnmanagedType.I1)] bool convolveAlpha, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_merge(IntPtr* filters, int count, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_offset(float dx, float dy, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_paint(IntPtr paint, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_picture(IntPtr picture);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_picture_with_croprect(IntPtr picture, SKRect* cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_point_lit_diffuse(SKPoint3* location, uint lightColor, float surfaceScale, float kd, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_point_lit_specular(SKPoint3* location, uint lightColor, float surfaceScale, float ks, float shininess, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_spot_lit_diffuse(SKPoint3* location, SKPoint3* target, float specularExponent, float cutoffAngle, uint lightColor, float surfaceScale, float kd, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_spot_lit_specular(SKPoint3* location, SKPoint3* target, float specularExponent, float cutoffAngle, uint lightColor, float surfaceScale, float ks, float shininess, IntPtr input, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_imagefilter_new_tile(SKRect* src, SKRect* dst, IntPtr input);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_imagefilter_new_xfermode(SKBlendMode mode, IntPtr background, IntPtr foreground, IntPtr cropRect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_imagefilter_unref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern byte* sk_mask_alloc_image(IntPtr bytes);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_mask_compute_image_size(SKMask* cmask);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_mask_compute_total_image_size(SKMask* cmask);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_mask_free_image(void* image);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* sk_mask_get_addr(SKMask* cmask, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern byte* sk_mask_get_addr_1(SKMask* cmask, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern uint* sk_mask_get_addr_32(SKMask* cmask, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern byte* sk_mask_get_addr_8(SKMask* cmask, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern ushort* sk_mask_get_addr_lcd_16(SKMask* cmask, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_mask_is_empty(SKMask* cmask);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_maskfilter_new_blur(SKBlurStyle param0, float sigma);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_maskfilter_new_blur_with_flags(SKBlurStyle param0, float sigma, [MarshalAs(UnmanagedType.I1)] bool respectCTM);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_maskfilter_new_clip(byte min, byte max);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_maskfilter_new_gamma(float gamma);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_maskfilter_new_shader(IntPtr cshader);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_maskfilter_new_table(byte* table);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_maskfilter_ref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_maskfilter_unref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_3dview_apply_to_canvas(IntPtr cview, IntPtr ccanvas);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_3dview_destroy(IntPtr cview);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float sk_3dview_dot_with_normal(IntPtr cview, float dx, float dy, float dz);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_3dview_get_matrix(IntPtr cview, SKMatrix* cmatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_3dview_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_3dview_restore(IntPtr cview);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_3dview_rotate_x_degrees(IntPtr cview, float degrees);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_3dview_rotate_x_radians(IntPtr cview, float radians);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_3dview_rotate_y_degrees(IntPtr cview, float degrees);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_3dview_rotate_y_radians(IntPtr cview, float radians);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_3dview_rotate_z_degrees(IntPtr cview, float degrees);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_3dview_rotate_z_radians(IntPtr cview, float radians);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_3dview_save(IntPtr cview);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_3dview_translate(IntPtr cview, float x, float y, float z);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix_concat(SKMatrix* result, SKMatrix* first, SKMatrix* second);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix_map_points(SKMatrix* matrix, SKPoint* dst, SKPoint* src, int count);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern float sk_matrix_map_radius(SKMatrix* matrix, float radius);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix_map_rect(SKMatrix* matrix, SKRect* dest, SKRect* source);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix_map_vector(SKMatrix* matrix, float x, float y, SKPoint* result);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix_map_vectors(SKMatrix* matrix, SKPoint* dst, SKPoint* src, int count);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix_map_xy(SKMatrix* matrix, float x, float y, SKPoint* result);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix_post_concat(SKMatrix* result, SKMatrix* matrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix_pre_concat(SKMatrix* result, SKMatrix* matrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_matrix_try_invert(SKMatrix* matrix, SKMatrix* result);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix44_as_col_major(IntPtr matrix, float* dst);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix44_as_row_major(IntPtr matrix, float* dst);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_destroy(IntPtr matrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern double sk_matrix44_determinant(IntPtr matrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_matrix44_equals(IntPtr matrix, IntPtr other);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float sk_matrix44_get(IntPtr matrix, int row, int col);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKMatrix44TypeMask sk_matrix44_get_type(IntPtr matrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_matrix44_invert(IntPtr matrix, IntPtr inverse);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix44_map_scalars(IntPtr matrix, float* src, float* dst);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix44_map2(IntPtr matrix, float* src2, int count, float* dst4);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_matrix44_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_matrix44_new_concat(IntPtr a, IntPtr b);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_matrix44_new_copy(IntPtr src);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_matrix44_new_identity();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_matrix44_new_matrix(SKMatrix* src);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_post_concat(IntPtr matrix, IntPtr m);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_post_scale(IntPtr matrix, float sx, float sy, float sz);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_post_translate(IntPtr matrix, float dx, float dy, float dz);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_pre_concat(IntPtr matrix, IntPtr m);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_pre_scale(IntPtr matrix, float sx, float sy, float sz);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_pre_translate(IntPtr matrix, float dx, float dy, float dz);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_matrix44_preserves_2d_axis_alignment(IntPtr matrix, float epsilon);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_set(IntPtr matrix, int row, int col, float value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix44_set_3x3_row_major(IntPtr matrix, float* dst);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix44_set_col_major(IntPtr matrix, float* dst);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_set_concat(IntPtr matrix, IntPtr a, IntPtr b);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_set_identity(IntPtr matrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_set_rotate_about_degrees(IntPtr matrix, float x, float y, float z, float degrees);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_set_rotate_about_radians(IntPtr matrix, float x, float y, float z, float radians);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_set_rotate_about_radians_unit(IntPtr matrix, float x, float y, float z, float radians);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix44_set_row_major(IntPtr matrix, float* dst);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_set_scale(IntPtr matrix, float sx, float sy, float sz);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_set_translate(IntPtr matrix, float dx, float dy, float dz);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_matrix44_to_matrix(IntPtr matrix, SKMatrix* dst);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_matrix44_transpose(IntPtr matrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_paint_clone(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_delete(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKBlendMode sk_paint_get_blendmode(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint sk_paint_get_color(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_paint_get_color4f(IntPtr paint, SKColorF* color);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_paint_get_colorfilter(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_paint_get_fill_path(IntPtr param0, IntPtr src, IntPtr dst, SKRect* cullRect, float resScale);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKFilterQuality sk_paint_get_filter_quality(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_paint_get_imagefilter(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_paint_get_maskfilter(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_paint_get_path_effect(IntPtr cpaint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_paint_get_shader(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKStrokeCap sk_paint_get_stroke_cap(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKStrokeJoin sk_paint_get_stroke_join(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float sk_paint_get_stroke_miter(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float sk_paint_get_stroke_width(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKPaintStyle sk_paint_get_style(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_paint_is_antialias(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_paint_is_dither(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_paint_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_reset(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_antialias(IntPtr param0, [MarshalAs(UnmanagedType.I1)] bool param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_blendmode(IntPtr param0, SKBlendMode param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_color(IntPtr param0, uint param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_paint_set_color4f(IntPtr paint, SKColorF* color, IntPtr colorspace);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_colorfilter(IntPtr param0, IntPtr param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_dither(IntPtr param0, [MarshalAs(UnmanagedType.I1)] bool param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_filter_quality(IntPtr param0, SKFilterQuality param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_imagefilter(IntPtr param0, IntPtr param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_maskfilter(IntPtr param0, IntPtr param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_path_effect(IntPtr cpaint, IntPtr effect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_shader(IntPtr param0, IntPtr param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_stroke_cap(IntPtr param0, SKStrokeCap param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_stroke_join(IntPtr param0, SKStrokeJoin param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_stroke_miter(IntPtr param0, float miter);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_stroke_width(IntPtr param0, float width);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_paint_set_style(IntPtr param0, SKPaintStyle param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_opbuilder_add(IntPtr builder, IntPtr path, SKPathOp op);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_opbuilder_destroy(IntPtr builder);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_opbuilder_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_opbuilder_resolve(IntPtr builder, IntPtr result);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_add_arc(IntPtr cpath, SKRect* crect, float startAngle, float sweepAngle);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_add_circle(IntPtr param0, float x, float y, float radius, SKPathDirection dir);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_add_oval(IntPtr param0, SKRect* param1, SKPathDirection param2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_add_path(IntPtr cpath, IntPtr other, SKPathAddMode add_mode);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_add_path_matrix(IntPtr cpath, IntPtr other, SKMatrix* matrix, SKPathAddMode add_mode);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_add_path_offset(IntPtr cpath, IntPtr other, float dx, float dy, SKPathAddMode add_mode);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_add_path_reverse(IntPtr cpath, IntPtr other);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_add_poly(IntPtr cpath, SKPoint* points, int count, [MarshalAs(UnmanagedType.I1)] bool close);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_add_rect(IntPtr param0, SKRect* param1, SKPathDirection param2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_add_rect_start(IntPtr cpath, SKRect* crect, SKPathDirection cdir, uint startIndex);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_add_rounded_rect(IntPtr param0, SKRect* param1, float param2, float param3, SKPathDirection param4);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_add_rrect(IntPtr param0, IntPtr param1, SKPathDirection param2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_add_rrect_start(IntPtr param0, IntPtr param1, SKPathDirection param2, uint param3);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_arc_to(IntPtr param0, float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_arc_to_with_oval(IntPtr param0, SKRect* oval, float startAngle, float sweepAngle, [MarshalAs(UnmanagedType.I1)] bool forceMoveTo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_arc_to_with_points(IntPtr param0, float x1, float y1, float x2, float y2, float radius);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_path_clone(IntPtr cpath);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_close(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_compute_tight_bounds(IntPtr param0, SKRect* param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_conic_to(IntPtr param0, float x0, float y0, float x1, float y1, float w);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_path_contains(IntPtr cpath, float x, float y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern int sk_path_convert_conic_to_quads(SKPoint* p0, SKPoint* p1, SKPoint* p2, float w, SKPoint* pts, int pow2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_path_count_points(IntPtr cpath);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_path_count_verbs(IntPtr cpath);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_path_create_iter(IntPtr cpath, int forceClose);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_path_create_rawiter(IntPtr cpath);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_cubic_to(IntPtr param0, float x0, float y0, float x1, float y1, float x2, float y2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_delete(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_get_bounds(IntPtr param0, SKRect* param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKPathFillType sk_path_get_filltype(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_path_get_last_point(IntPtr cpath, SKPoint* point);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_get_point(IntPtr cpath, int index, SKPoint* point);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern int sk_path_get_points(IntPtr cpath, SKPoint* points, int max);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint sk_path_get_segment_masks(IntPtr cpath);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_path_is_convex(IntPtr cpath);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_path_is_line(IntPtr cpath, SKPoint* line);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_path_is_oval(IntPtr cpath, SKRect* bounds);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_path_is_rect(IntPtr cpath, SKRect* rect, byte* isClosed, SKPathDirection* direction);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_path_is_rrect(IntPtr cpath, IntPtr bounds);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float sk_path_iter_conic_weight(IntPtr iterator);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_iter_destroy(IntPtr iterator);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_path_iter_is_close_line(IntPtr iterator);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_path_iter_is_closed_contour(IntPtr iterator);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern SKPathVerb sk_path_iter_next(IntPtr iterator, SKPoint* points);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_line_to(IntPtr param0, float x, float y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_move_to(IntPtr param0, float x, float y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_path_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_path_parse_svg_string(IntPtr cpath, [MarshalAs(UnmanagedType.LPStr)] string str);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_quad_to(IntPtr param0, float x0, float y0, float x1, float y1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_rarc_to(IntPtr param0, float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float sk_path_rawiter_conic_weight(IntPtr iterator);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_rawiter_destroy(IntPtr iterator);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern SKPathVerb sk_path_rawiter_next(IntPtr iterator, SKPoint* points);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKPathVerb sk_path_rawiter_peek(IntPtr iterator);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_rconic_to(IntPtr param0, float dx0, float dy0, float dx1, float dy1, float w);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_rcubic_to(IntPtr param0, float dx0, float dy0, float dx1, float dy1, float dx2, float dy2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_reset(IntPtr cpath);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_rewind(IntPtr cpath);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_rline_to(IntPtr param0, float dx, float yd);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_rmove_to(IntPtr param0, float dx, float dy);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_rquad_to(IntPtr param0, float dx0, float dy0, float dx1, float dy1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_set_filltype(IntPtr param0, SKPathFillType param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_to_svg_string(IntPtr cpath, IntPtr str);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_transform(IntPtr cpath, SKMatrix* cmatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_path_transform_to_dest(IntPtr cpath, SKMatrix* cmatrix, IntPtr destination);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_pathmeasure_destroy(IntPtr pathMeasure);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float sk_pathmeasure_get_length(IntPtr pathMeasure);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_pathmeasure_get_matrix(IntPtr pathMeasure, float distance, SKMatrix* matrix, SKPathMeasureMatrixFlags flags);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_pathmeasure_get_pos_tan(IntPtr pathMeasure, float distance, SKPoint* position, SKPoint* tangent);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_pathmeasure_get_segment(IntPtr pathMeasure, float start, float stop, IntPtr dst, [MarshalAs(UnmanagedType.I1)] bool startWithMoveTo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_pathmeasure_is_closed(IntPtr pathMeasure);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_pathmeasure_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_pathmeasure_new_with_path(IntPtr path, [MarshalAs(UnmanagedType.I1)] bool forceClosed, float resScale);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_pathmeasure_next_contour(IntPtr pathMeasure);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_pathmeasure_set_path(IntPtr pathMeasure, IntPtr path, [MarshalAs(UnmanagedType.I1)] bool forceClosed);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_pathop_as_winding(IntPtr path, IntPtr result);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_pathop_op(IntPtr one, IntPtr two, SKPathOp op, IntPtr result);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_pathop_simplify(IntPtr path, IntPtr result);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_pathop_tight_bounds(IntPtr path, SKRect* result);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_path_effect_create_1d_path(IntPtr path, float advance, float phase, SKPath1DPathEffectStyle style);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_path_effect_create_2d_line(float width, SKMatrix* matrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_path_effect_create_2d_path(SKMatrix* matrix, IntPtr path);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_path_effect_create_compose(IntPtr outer, IntPtr inner);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_path_effect_create_corner(float radius);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_path_effect_create_dash(float* intervals, int count, float phase);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_path_effect_create_discrete(float segLength, float deviation, uint seedAssist);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_path_effect_create_sum(IntPtr first, IntPtr second);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_path_effect_create_trim(float start, float stop, SKTrimPathEffectMode mode);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_path_effect_unref(IntPtr t);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_picture_deserialize_from_data(IntPtr data);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_picture_deserialize_from_memory(void* buffer, IntPtr length);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_picture_deserialize_from_stream(IntPtr stream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_picture_get_cull_rect(IntPtr param0, SKRect* param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_picture_get_recording_canvas(IntPtr crec);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint sk_picture_get_unique_id(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_picture_make_shader(IntPtr src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix* localMatrix, SKRect* tile);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_picture_recorder_begin_recording(IntPtr param0, SKRect* param1);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_picture_recorder_delete(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_picture_recorder_end_recording(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_picture_recorder_end_recording_as_drawable(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_picture_recorder_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_picture_ref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_picture_serialize_to_data(IntPtr picture);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_picture_serialize_to_stream(IntPtr picture, IntPtr stream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_picture_unref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_color_get_bit_shift(int* a, int* r, int* g, int* b);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint sk_color_premultiply(uint color);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_color_premultiply_array(uint* colors, int size, uint* pmcolors);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint sk_color_unpremultiply(uint pmcolor);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_color_unpremultiply_array(uint* pmcolors, int size, uint* colors);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_jpegencoder_encode(IntPtr dst, IntPtr src, SKJpegEncoderOptions* options);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_pixmap_destructor(IntPtr cpixmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_pixmap_encode_image(IntPtr dst, IntPtr src, SKEncodedImageFormat encoder, int quality);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_pixmap_erase_color(IntPtr cpixmap, uint color, SKRectI* subset);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_pixmap_erase_color4f(IntPtr cpixmap, SKColorF* color, IntPtr colorspace, SKRectI* subset);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_pixmap_extract_subset(IntPtr cpixmap, IntPtr result, SKRectI* subset);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_pixmap_get_info(IntPtr cpixmap, SKImageInfoNative* cinfo);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint sk_pixmap_get_pixel_color(IntPtr cpixmap, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* sk_pixmap_get_pixels(IntPtr cpixmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* sk_pixmap_get_pixels_with_xy(IntPtr cpixmap, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_pixmap_get_row_bytes(IntPtr cpixmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* sk_pixmap_get_writable_addr(IntPtr cpixmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_pixmap_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_pixmap_new_with_params(SKImageInfoNative* cinfo, void* addr, IntPtr rowBytes);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_pixmap_read_pixels(IntPtr cpixmap, SKImageInfoNative* dstInfo, void* dstPixels, IntPtr dstRowBytes, int srcX, int srcY);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_pixmap_reset(IntPtr cpixmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_pixmap_reset_with_params(IntPtr cpixmap, SKImageInfoNative* cinfo, void* addr, IntPtr rowBytes);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_pixmap_scale_pixels(IntPtr cpixmap, IntPtr dst, SKFilterQuality quality);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_pngencoder_encode(IntPtr dst, IntPtr src, SKPngEncoderOptions* options);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_swizzle_swap_rb(uint* dest, uint* src, int count);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_webpencoder_encode(IntPtr dst, IntPtr src, SKWebpEncoderOptions* options);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_region_cliperator_delete(IntPtr iter);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_cliperator_done(IntPtr iter);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_region_cliperator_new(IntPtr region, SKRectI* clip);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_region_cliperator_next(IntPtr iter);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_region_cliperator_rect(IntPtr iter, SKRectI* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_contains(IntPtr r, IntPtr region);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_contains_point(IntPtr r, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_region_contains_rect(IntPtr r, SKRectI* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_region_delete(IntPtr r);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_get_boundary_path(IntPtr r, IntPtr path);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_region_get_bounds(IntPtr r, SKRectI* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_intersects(IntPtr r, IntPtr src);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_region_intersects_rect(IntPtr r, SKRectI* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_is_complex(IntPtr r);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_is_empty(IntPtr r);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_is_rect(IntPtr r);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_region_iterator_delete(IntPtr iter);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_iterator_done(IntPtr iter);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_region_iterator_new(IntPtr region);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_region_iterator_next(IntPtr iter);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_region_iterator_rect(IntPtr iter, SKRectI* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_iterator_rewind(IntPtr iter);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_region_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_op(IntPtr r, IntPtr region, SKRegionOperation op);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_region_op_rect(IntPtr r, SKRectI* rect, SKRegionOperation op);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_region_quick_contains(IntPtr r, SKRectI* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_quick_reject(IntPtr r, IntPtr region);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_region_quick_reject_rect(IntPtr r, SKRectI* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_set_empty(IntPtr r);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_set_path(IntPtr r, IntPtr t, IntPtr clip);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_region_set_rect(IntPtr r, SKRectI* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_region_set_rects(IntPtr r, SKRectI* rects, int count);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_region_set_region(IntPtr r, IntPtr region);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_region_spanerator_delete(IntPtr iter);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_region_spanerator_new(IntPtr region, int y, int left, int right);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_region_spanerator_next(IntPtr iter, int* left, int* right);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_region_translate(IntPtr r, int x, int y);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_rrect_contains(IntPtr rrect, SKRect* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_rrect_delete(IntPtr rrect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float sk_rrect_get_height(IntPtr rrect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_rrect_get_radii(IntPtr rrect, SKRoundRectCorner corner, SKPoint* radii);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_rrect_get_rect(IntPtr rrect, SKRect* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKRoundRectType sk_rrect_get_type(IntPtr rrect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float sk_rrect_get_width(IntPtr rrect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_rrect_inset(IntPtr rrect, float dx, float dy);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_rrect_is_valid(IntPtr rrect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_rrect_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_rrect_new_copy(IntPtr rrect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_rrect_offset(IntPtr rrect, float dx, float dy);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_rrect_outset(IntPtr rrect, float dx, float dy);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_rrect_set_empty(IntPtr rrect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_rrect_set_nine_patch(IntPtr rrect, SKRect* rect, float leftRad, float topRad, float rightRad, float bottomRad);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_rrect_set_oval(IntPtr rrect, SKRect* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_rrect_set_rect(IntPtr rrect, SKRect* rect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_rrect_set_rect_radii(IntPtr rrect, SKRect* rect, SKPoint* radii);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_rrect_set_rect_xy(IntPtr rrect, SKRect* rect, float xRad, float yRad);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_rrect_transform(IntPtr rrect, SKMatrix* matrix, IntPtr dest);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_runtimeeffect_get_child_name(IntPtr effect, int index, IntPtr name);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_runtimeeffect_get_children_count(IntPtr effect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_runtimeeffect_get_uniform_from_index(IntPtr effect, int index);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_runtimeeffect_get_uniform_from_name(IntPtr effect, void* name, IntPtr len);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_runtimeeffect_get_uniform_name(IntPtr effect, int index, IntPtr name);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_runtimeeffect_get_uniform_size(IntPtr effect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_runtimeeffect_get_uniforms_count(IntPtr effect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_runtimeeffect_make(IntPtr sksl, IntPtr error);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_runtimeeffect_make_color_filter(IntPtr effect, IntPtr uniforms, IntPtr* children, IntPtr childCount);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_runtimeeffect_make_shader(IntPtr effect, IntPtr uniforms, IntPtr* children, IntPtr childCount, SKMatrix* localMatrix, [MarshalAs(UnmanagedType.I1)] bool isOpaque);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_runtimeeffect_uniform_get_offset(IntPtr variable);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_runtimeeffect_uniform_get_size_in_bytes(IntPtr variable);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_runtimeeffect_unref(IntPtr effect);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_shader_new_blend(SKBlendMode mode, IntPtr dst, IntPtr src);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_shader_new_color(uint color);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_shader_new_color4f(SKColorF* color, IntPtr colorspace);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_shader_new_empty();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_shader_new_lerp(float t, IntPtr dst, IntPtr src);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_shader_new_linear_gradient(SKPoint* points, uint* colors, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_shader_new_linear_gradient_color4f(SKPoint* points, SKColorF* colors, IntPtr colorspace, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_shader_new_perlin_noise_fractal_noise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI* tileSize);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_shader_new_perlin_noise_improved_noise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float z);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_shader_new_perlin_noise_turbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI* tileSize);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_shader_new_radial_gradient(SKPoint* center, float radius, uint* colors, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_shader_new_radial_gradient_color4f(SKPoint* center, float radius, SKColorF* colors, IntPtr colorspace, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_shader_new_sweep_gradient(SKPoint* center, uint* colors, float* colorPos, int colorCount, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix* localMatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_shader_new_sweep_gradient_color4f(SKPoint* center, SKColorF* colors, IntPtr colorspace, float* colorPos, int colorCount, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix* localMatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_shader_new_two_point_conical_gradient(SKPoint* start, float startRadius, SKPoint* end, float endRadius, uint* colors, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_shader_new_two_point_conical_gradient_color4f(SKPoint* start, float startRadius, SKPoint* end, float endRadius, SKColorF* colors, IntPtr colorspace, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_shader_ref(IntPtr shader);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_shader_unref(IntPtr shader);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_shader_with_color_filter(IntPtr shader, IntPtr filter);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_shader_with_local_matrix(IntPtr shader, SKMatrix* localMatrix);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_dynamicmemorywstream_copy_to(IntPtr cstream, void* data);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_dynamicmemorywstream_destroy(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_dynamicmemorywstream_detach_as_data(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_dynamicmemorywstream_detach_as_stream(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_dynamicmemorywstream_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_dynamicmemorywstream_write_to_stream(IntPtr cstream, IntPtr dst);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_filestream_destroy(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_filestream_is_valid(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_filestream_new(void* path);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_filewstream_destroy(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_filewstream_is_valid(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_filewstream_new(void* path);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_memorystream_destroy(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_memorystream_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_memorystream_new_with_data(void* data, IntPtr length, [MarshalAs(UnmanagedType.I1)] bool copyData);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_memorystream_new_with_length(IntPtr length);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_memorystream_new_with_skdata(IntPtr data);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_memorystream_set_memory(IntPtr cmemorystream, void* data, IntPtr length, [MarshalAs(UnmanagedType.I1)] bool copyData);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_stream_asset_destroy(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_stream_destroy(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_stream_duplicate(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_stream_fork(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_stream_get_length(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* sk_stream_get_memory_base(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_stream_get_position(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_stream_has_length(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_stream_has_position(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_stream_is_at_end(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_stream_move(IntPtr cstream, int offset);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_stream_peek(IntPtr cstream, void* buffer, IntPtr size);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_stream_read(IntPtr cstream, void* buffer, IntPtr size);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_stream_read_bool(IntPtr cstream, byte* buffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_stream_read_s16(IntPtr cstream, short* buffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_stream_read_s32(IntPtr cstream, int* buffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_stream_read_s8(IntPtr cstream, sbyte* buffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_stream_read_u16(IntPtr cstream, ushort* buffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_stream_read_u32(IntPtr cstream, uint* buffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_stream_read_u8(IntPtr cstream, byte* buffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_stream_rewind(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_stream_seek(IntPtr cstream, IntPtr position);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_stream_skip(IntPtr cstream, IntPtr size);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_wstream_bytes_written(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_wstream_flush(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_wstream_get_size_of_packed_uint(IntPtr value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_newline(IntPtr cstream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_wstream_write(IntPtr cstream, void* buffer, IntPtr size);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_write_16(IntPtr cstream, ushort value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_write_32(IntPtr cstream, uint value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_write_8(IntPtr cstream, byte value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_write_bigdec_as_text(IntPtr cstream, long value, int minDigits);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_write_bool(IntPtr cstream, [MarshalAs(UnmanagedType.I1)] bool value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_write_dec_as_text(IntPtr cstream, int value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_write_hex_as_text(IntPtr cstream, uint value, int minDigits);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_write_packed_uint(IntPtr cstream, IntPtr value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_write_scalar(IntPtr cstream, float value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_write_scalar_as_text(IntPtr cstream, float value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_write_stream(IntPtr cstream, IntPtr input, IntPtr length);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_wstream_write_text(IntPtr cstream, [MarshalAs(UnmanagedType.LPStr)] string value);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_string_destructor(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void* sk_string_get_c_str(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_string_get_size(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_string_new_empty();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_string_new_with_copy(void* src, IntPtr length);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_surface_draw(IntPtr surface, IntPtr canvas, float x, float y, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_surface_flush(IntPtr surface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_surface_flush_and_submit(IntPtr surface, [MarshalAs(UnmanagedType.I1)] bool syncCpu);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_surface_get_canvas(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_surface_get_props(IntPtr surface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_surface_get_recording_context(IntPtr surface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_surface_new_backend_render_target(IntPtr context, IntPtr target, GRSurfaceOrigin origin, SKColorTypeNative colorType, IntPtr colorspace, IntPtr props);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_surface_new_backend_texture(IntPtr context, IntPtr texture, GRSurfaceOrigin origin, int samples, SKColorTypeNative colorType, IntPtr colorspace, IntPtr props);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_surface_new_image_snapshot(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_surface_new_image_snapshot_with_crop(IntPtr surface, SKRectI* bounds);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_surface_new_metal_layer(IntPtr context, void* layer, GRSurfaceOrigin origin, int sampleCount, SKColorTypeNative colorType, IntPtr colorspace, IntPtr props, void** drawable);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_surface_new_metal_view(IntPtr context, void* mtkView, GRSurfaceOrigin origin, int sampleCount, SKColorTypeNative colorType, IntPtr colorspace, IntPtr props);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_surface_new_null(int width, int height);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_surface_new_raster(SKImageInfoNative* param0, IntPtr rowBytes, IntPtr param2);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_surface_new_raster_direct(SKImageInfoNative* param0, void* pixels, IntPtr rowBytes, SKSurfaceRasterReleaseProxyDelegate releaseProc, void* context, IntPtr props);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_surface_new_render_target(IntPtr context, [MarshalAs(UnmanagedType.I1)] bool budgeted, SKImageInfoNative* cinfo, int sampleCount, GRSurfaceOrigin origin, IntPtr props, [MarshalAs(UnmanagedType.I1)] bool shouldCreateWithMips);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_surface_peek_pixels(IntPtr surface, IntPtr pixmap);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_surface_read_pixels(IntPtr surface, SKImageInfoNative* dstInfo, void* dstPixels, IntPtr dstRowBytes, int srcX, int srcY);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_surface_unref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_surfaceprops_delete(IntPtr props);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint sk_surfaceprops_get_flags(IntPtr props);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKPixelGeometry sk_surfaceprops_get_pixel_geometry(IntPtr props);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_surfaceprops_new(uint flags, SKPixelGeometry geometry);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_svgcanvas_create_with_stream(SKRect* bounds, IntPtr stream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_svgcanvas_create_with_writer(SKRect* bounds, IntPtr writer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_textblob_builder_alloc_run(IntPtr builder, IntPtr font, int count, float x, float y, SKRect* bounds, SKRunBufferInternal* runbuffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_textblob_builder_alloc_run_pos(IntPtr builder, IntPtr font, int count, SKRect* bounds, SKRunBufferInternal* runbuffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_textblob_builder_alloc_run_pos_h(IntPtr builder, IntPtr font, int count, float y, SKRect* bounds, SKRunBufferInternal* runbuffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_textblob_builder_alloc_run_rsxform(IntPtr builder, IntPtr font, int count, SKRunBufferInternal* runbuffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_textblob_builder_alloc_run_text(IntPtr builder, IntPtr font, int count, float x, float y, int textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_textblob_builder_alloc_run_text_pos(IntPtr builder, IntPtr font, int count, int textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_textblob_builder_alloc_run_text_pos_h(IntPtr builder, IntPtr font, int count, float y, int textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_textblob_builder_delete(IntPtr builder);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_textblob_builder_make(IntPtr builder);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_textblob_builder_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_textblob_get_bounds(IntPtr blob, SKRect* bounds);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern int sk_textblob_get_intercepts(IntPtr blob, float* bounds, float* intervals, IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint sk_textblob_get_unique_id(IntPtr blob);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_textblob_ref(IntPtr blob);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_textblob_unref(IntPtr blob);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_fontmgr_count_families(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontmgr_create_default();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontmgr_create_from_data(IntPtr param0, IntPtr data, int index);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_fontmgr_create_from_file(IntPtr param0, void* path, int index);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontmgr_create_from_stream(IntPtr param0, IntPtr stream, int index);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontmgr_create_styleset(IntPtr param0, int index);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_fontmgr_get_family_name(IntPtr param0, int index, IntPtr familyName);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontmgr_match_face_style(IntPtr param0, IntPtr face, IntPtr style);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontmgr_match_family(IntPtr param0, IntPtr familyName);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontmgr_match_family_style(IntPtr param0, IntPtr familyName, IntPtr style);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontmgr_match_family_style_character(IntPtr param0, IntPtr familyName, IntPtr style, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] bcp47, int bcp47Count, int character);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontmgr_ref_default();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_fontmgr_unref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_fontstyle_delete(IntPtr fs);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKFontStyleSlant sk_fontstyle_get_slant(IntPtr fs);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_fontstyle_get_weight(IntPtr fs);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_fontstyle_get_width(IntPtr fs);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontstyle_new(int weight, int width, SKFontStyleSlant slant);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontstyleset_create_empty();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontstyleset_create_typeface(IntPtr fss, int index);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_fontstyleset_get_count(IntPtr fss);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_fontstyleset_get_style(IntPtr fss, int index, IntPtr fs, IntPtr style);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_fontstyleset_match_style(IntPtr fss, IntPtr style);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_fontstyleset_unref(IntPtr fss);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_typeface_copy_table_data(IntPtr typeface, uint tag);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_typeface_count_glyphs(IntPtr typeface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_typeface_count_tables(IntPtr typeface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_typeface_create_default();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_typeface_create_from_data(IntPtr data, int index);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_typeface_create_from_file(void* path, int index);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_typeface_create_from_name(IntPtr familyName, IntPtr style);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_typeface_create_from_stream(IntPtr stream, int index);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_typeface_get_family_name(IntPtr typeface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKFontStyleSlant sk_typeface_get_font_slant(IntPtr typeface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_typeface_get_font_weight(IntPtr typeface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_typeface_get_font_width(IntPtr typeface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_typeface_get_fontstyle(IntPtr typeface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal unsafe static extern bool sk_typeface_get_kerning_pair_adjustments(IntPtr typeface, ushort* glyphs, int count, int* adjustments);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_typeface_get_table_data(IntPtr typeface, uint tag, IntPtr offset, IntPtr length, void* data);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_typeface_get_table_size(IntPtr typeface, uint tag);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern int sk_typeface_get_table_tags(IntPtr typeface, uint* tags);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int sk_typeface_get_units_per_em(IntPtr typeface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool sk_typeface_is_fixed_pitch(IntPtr typeface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_typeface_open_stream(IntPtr typeface, int* ttcIndex);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_typeface_ref_default();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern ushort sk_typeface_unichar_to_glyph(IntPtr typeface, int unichar);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern void sk_typeface_unichars_to_glyphs(IntPtr typeface, int* unichars, int count, ushort* glyphs);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_typeface_unref(IntPtr typeface);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_vertices_make_copy(SKVertexMode vmode, int vertexCount, SKPoint* positions, SKPoint* texs, uint* colors, int indexCount, ushort* indices);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_vertices_ref(IntPtr cvertices);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_vertices_unref(IntPtr cvertices);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_xmlstreamwriter_delete(IntPtr writer);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_xmlstreamwriter_new(IntPtr stream);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_compatpaint_clone(IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_compatpaint_delete(IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_compatpaint_get_font(IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKTextAlign sk_compatpaint_get_text_align(IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern SKTextEncoding sk_compatpaint_get_text_encoding(IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_compatpaint_make_font(IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_compatpaint_new();

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr sk_compatpaint_new_with_font(IntPtr font);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_compatpaint_reset(IntPtr paint);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_compatpaint_set_text_align(IntPtr paint, SKTextAlign align);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_compatpaint_set_text_encoding(IntPtr paint, SKTextEncoding encoding);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_manageddrawable_new(void* context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_manageddrawable_set_procs(SKManagedDrawableDelegates procs);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_manageddrawable_unref(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_managedstream_destroy(IntPtr s);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_managedstream_new(void* context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_managedstream_set_procs(SKManagedStreamDelegates procs);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_managedwstream_destroy(IntPtr s);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_managedwstream_new(void* context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_managedwstream_set_procs(SKManagedWStreamDelegates procs);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_managedtracememorydump_delete(IntPtr param0);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal unsafe static extern IntPtr sk_managedtracememorydump_new([MarshalAs(UnmanagedType.I1)] bool detailed, [MarshalAs(UnmanagedType.I1)] bool dumpWrapped, void* context);

	[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void sk_managedtracememorydump_set_procs(SKManagedTraceMemoryDumpDelegates procs);
}
