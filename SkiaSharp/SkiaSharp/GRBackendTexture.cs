using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public class GRBackendTexture : SKObject, ISKSkipObjectRegistration
{
	[Obsolete]
	internal struct GRTextureInfoObsolete
	{
		public uint fTarget;

		public uint fID;
	}

	public bool IsValid => SkiaApi.gr_backendtexture_is_valid(Handle);

	public int Width => SkiaApi.gr_backendtexture_get_width(Handle);

	public int Height => SkiaApi.gr_backendtexture_get_height(Handle);

	public bool HasMipMaps => SkiaApi.gr_backendtexture_has_mipmaps(Handle);

	public GRBackend Backend => SkiaApi.gr_backendtexture_get_backend(Handle).FromNative();

	public SKSizeI Size => new SKSizeI(Width, Height);

	public SKRectI Rect => new SKRectI(0, 0, Width, Height);

	internal GRBackendTexture(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GRBackendTexture(int, int, bool, GRGlTextureInfo) instead.")]
	public GRBackendTexture(GRGlBackendTextureDesc desc)
		: this(IntPtr.Zero, owns: true)
	{
		GRGlTextureInfo textureHandle = desc.TextureHandle;
		if (textureHandle.Format == 0)
		{
			textureHandle.Format = desc.Config.ToGlSizedFormat();
		}
		CreateGl(desc.Width, desc.Height, mipmapped: false, textureHandle);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GRBackendTexture(int, int, bool, GRGlTextureInfo) instead.")]
	public GRBackendTexture(GRBackendTextureDesc desc)
		: this(IntPtr.Zero, owns: true)
	{
		IntPtr textureHandle = desc.TextureHandle;
		GRTextureInfoObsolete gRTextureInfoObsolete = Marshal.PtrToStructure<GRTextureInfoObsolete>(textureHandle);
		CreateGl(glInfo: new GRGlTextureInfo(gRTextureInfoObsolete.fTarget, gRTextureInfoObsolete.fID, desc.Config.ToGlSizedFormat()), width: desc.Width, height: desc.Height, mipmapped: false);
	}

	public GRBackendTexture(int width, int height, bool mipmapped, GRGlTextureInfo glInfo)
		: this(IntPtr.Zero, owns: true)
	{
		CreateGl(width, height, mipmapped, glInfo);
	}

	public GRBackendTexture(int width, int height, GRVkImageInfo vkInfo)
		: this(IntPtr.Zero, owns: true)
	{
		CreateVulkan(width, height, vkInfo);
	}

	private unsafe void CreateGl(int width, int height, bool mipmapped, GRGlTextureInfo glInfo)
	{
		Handle = SkiaApi.gr_backendtexture_new_gl(width, height, mipmapped, &glInfo);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new GRBackendTexture instance.");
		}
	}

	private unsafe void CreateVulkan(int width, int height, GRVkImageInfo vkInfo)
	{
		Handle = SkiaApi.gr_backendtexture_new_vulkan(width, height, &vkInfo);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new GRBackendTexture instance.");
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.gr_backendtexture_delete(Handle);
	}

	public GRGlTextureInfo GetGlTextureInfo()
	{
		if (!GetGlTextureInfo(out var glInfo))
		{
			return default(GRGlTextureInfo);
		}
		return glInfo;
	}

	public unsafe bool GetGlTextureInfo(out GRGlTextureInfo glInfo)
	{
		fixed (GRGlTextureInfo* glInfo2 = &glInfo)
		{
			return SkiaApi.gr_backendtexture_get_gl_textureinfo(Handle, glInfo2);
		}
	}
}
