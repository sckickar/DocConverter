using System;

namespace SkiaSharp;

public class GRRecordingContext : SKObject, ISKReferenceCounted
{
	public GRBackend Backend => SkiaApi.gr_recording_context_get_backend(Handle).FromNative();

	internal GRRecordingContext(IntPtr h, bool owns)
		: base(h, owns)
	{
	}

	public int GetMaxSurfaceSampleCount(SKColorType colorType)
	{
		return SkiaApi.gr_recording_context_get_max_surface_sample_count_for_color_type(Handle, colorType.ToNative());
	}

	internal static GRRecordingContext GetObject(IntPtr handle, bool owns = true, bool unrefExisting = true)
	{
		return SKObject.GetOrAddObject(handle, owns, unrefExisting, (IntPtr h, bool o) => new GRRecordingContext(h, o));
	}
}
