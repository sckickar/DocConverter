using System;
using System.ComponentModel;

namespace SkiaSharp;

public class GRBackendRenderTarget : SKObject, ISKSkipObjectRegistration
{
	public bool IsValid => SkiaApi.gr_backendrendertarget_is_valid(Handle);

	public int Width => SkiaApi.gr_backendrendertarget_get_width(Handle);

	public int Height => SkiaApi.gr_backendrendertarget_get_height(Handle);

	public int SampleCount => SkiaApi.gr_backendrendertarget_get_samples(Handle);

	public int StencilBits => SkiaApi.gr_backendrendertarget_get_stencils(Handle);

	public GRBackend Backend => SkiaApi.gr_backendrendertarget_get_backend(Handle).FromNative();

	public SKSizeI Size => new SKSizeI(Width, Height);

	public SKRectI Rect => new SKRectI(0, 0, Width, Height);

	internal GRBackendRenderTarget(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GRBackendRenderTarget(int, int, int, int, GRGlFramebufferInfo) instead.")]
	public GRBackendRenderTarget(GRBackend backend, GRBackendRenderTargetDesc desc)
		: this(IntPtr.Zero, owns: true)
	{
		switch (backend)
		{
		case GRBackend.Metal:
			throw new NotSupportedException();
		case GRBackend.OpenGL:
			CreateGl(glInfo: new GRGlFramebufferInfo((uint)(int)desc.RenderTargetHandle, desc.Config.ToGlSizedFormat()), width: desc.Width, height: desc.Height, sampleCount: desc.SampleCount, stencilBits: desc.StencilBits);
			break;
		case GRBackend.Vulkan:
			throw new NotSupportedException();
		case GRBackend.Dawn:
			throw new NotSupportedException();
		default:
			throw new ArgumentOutOfRangeException("backend");
		}
	}

	public GRBackendRenderTarget(int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo)
		: this(IntPtr.Zero, owns: true)
	{
		CreateGl(width, height, sampleCount, stencilBits, glInfo);
	}

	public GRBackendRenderTarget(int width, int height, int sampleCount, GRVkImageInfo vkImageInfo)
		: this(IntPtr.Zero, owns: true)
	{
		CreateVulkan(width, height, sampleCount, vkImageInfo);
	}

	private unsafe void CreateGl(int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo)
	{
		Handle = SkiaApi.gr_backendrendertarget_new_gl(width, height, sampleCount, stencilBits, &glInfo);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new GRBackendRenderTarget instance.");
		}
	}

	private unsafe void CreateVulkan(int width, int height, int sampleCount, GRVkImageInfo vkImageInfo)
	{
		Handle = SkiaApi.gr_backendrendertarget_new_vulkan(width, height, sampleCount, &vkImageInfo);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new GRBackendRenderTarget instance.");
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.gr_backendrendertarget_delete(Handle);
	}

	public GRGlFramebufferInfo GetGlFramebufferInfo()
	{
		if (!GetGlFramebufferInfo(out var glInfo))
		{
			return default(GRGlFramebufferInfo);
		}
		return glInfo;
	}

	public unsafe bool GetGlFramebufferInfo(out GRGlFramebufferInfo glInfo)
	{
		fixed (GRGlFramebufferInfo* glInfo2 = &glInfo)
		{
			return SkiaApi.gr_backendrendertarget_get_gl_framebufferinfo(Handle, glInfo2);
		}
	}
}
