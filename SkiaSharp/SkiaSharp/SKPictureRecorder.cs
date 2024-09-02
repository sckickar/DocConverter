using System;

namespace SkiaSharp;

public class SKPictureRecorder : SKObject, ISKSkipObjectRegistration
{
	public SKCanvas RecordingCanvas => SKObject.OwnedBy(SKCanvas.GetObject(SkiaApi.sk_picture_get_recording_canvas(Handle), owns: false), this);

	internal SKPictureRecorder(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKPictureRecorder()
		: this(SkiaApi.sk_picture_recorder_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKPictureRecorder instance.");
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_picture_recorder_delete(Handle);
	}

	public unsafe SKCanvas BeginRecording(SKRect cullRect)
	{
		return SKObject.OwnedBy(SKCanvas.GetObject(SkiaApi.sk_picture_recorder_begin_recording(Handle, &cullRect), owns: false), this);
	}

	public SKPicture EndRecording()
	{
		return SKPicture.GetObject(SkiaApi.sk_picture_recorder_end_recording(Handle));
	}

	public SKDrawable EndRecordingAsDrawable()
	{
		return SKDrawable.GetObject(SkiaApi.sk_picture_recorder_end_recording_as_drawable(Handle));
	}
}
