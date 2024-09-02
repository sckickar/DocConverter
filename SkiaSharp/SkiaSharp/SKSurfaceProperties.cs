using System;
using System.ComponentModel;

namespace SkiaSharp;

public class SKSurfaceProperties : SKObject
{
	public SKSurfacePropsFlags Flags => (SKSurfacePropsFlags)SkiaApi.sk_surfaceprops_get_flags(Handle);

	public SKPixelGeometry PixelGeometry => SkiaApi.sk_surfaceprops_get_pixel_geometry(Handle);

	public bool IsUseDeviceIndependentFonts => Flags.HasFlag(SKSurfacePropsFlags.UseDeviceIndependentFonts);

	internal SKSurfaceProperties(IntPtr h, bool owns)
		: base(h, owns)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public SKSurfaceProperties(SKSurfaceProps props)
		: this(props.Flags, props.PixelGeometry)
	{
	}

	public SKSurfaceProperties(SKPixelGeometry pixelGeometry)
		: this(0u, pixelGeometry)
	{
	}

	public SKSurfaceProperties(uint flags, SKPixelGeometry pixelGeometry)
		: this(SkiaApi.sk_surfaceprops_new(flags, pixelGeometry), owns: true)
	{
	}

	public SKSurfaceProperties(SKSurfacePropsFlags flags, SKPixelGeometry pixelGeometry)
		: this(SkiaApi.sk_surfaceprops_new((uint)flags, pixelGeometry), owns: true)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_surfaceprops_delete(Handle);
	}

	internal static SKSurfaceProperties GetObject(IntPtr handle, bool owns = true)
	{
		return SKObject.GetOrAddObject(handle, owns, (IntPtr h, bool o) => new SKSurfaceProperties(h, o));
	}
}
