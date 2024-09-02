using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp;

public class SKDrawable : SKObject, ISKReferenceCounted
{
	private static readonly SKManagedDrawableDelegates delegates;

	private int fromNative;

	public uint GenerationId => SkiaApi.sk_drawable_get_generation_id(Handle);

	public unsafe SKRect Bounds
	{
		get
		{
			SKRect result = default(SKRect);
			SkiaApi.sk_drawable_get_bounds(Handle, &result);
			return result;
		}
	}

	unsafe static SKDrawable()
	{
		delegates = new SKManagedDrawableDelegates
		{
			fDraw = DrawInternal,
			fGetBounds = GetBoundsInternal,
			fNewPictureSnapshot = NewPictureSnapshotInternal,
			fDestroy = DestroyInternal
		};
		SkiaApi.sk_manageddrawable_set_procs(delegates);
	}

	protected SKDrawable()
		: this(owns: true)
	{
	}

	protected unsafe SKDrawable(bool owns)
		: base(IntPtr.Zero, owns)
	{
		IntPtr intPtr = DelegateProxies.CreateUserData(this, makeWeak: true);
		Handle = SkiaApi.sk_manageddrawable_new((void*)intPtr);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKDrawable instance.");
		}
	}

	internal SKDrawable(IntPtr x, bool owns)
		: base(x, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		if (Interlocked.CompareExchange(ref fromNative, 0, 0) == 0)
		{
			SkiaApi.sk_drawable_unref(Handle);
		}
	}

	public unsafe void Draw(SKCanvas canvas, ref SKMatrix matrix)
	{
		fixed (SKMatrix* param = &matrix)
		{
			SkiaApi.sk_drawable_draw(Handle, canvas.Handle, param);
		}
	}

	public void Draw(SKCanvas canvas, float x, float y)
	{
		SKMatrix matrix = SKMatrix.CreateTranslation(x, y);
		Draw(canvas, ref matrix);
	}

	public SKPicture Snapshot()
	{
		return SKPicture.GetObject(SkiaApi.sk_drawable_new_picture_snapshot(Handle), owns: true, unrefExisting: false);
	}

	public void NotifyDrawingChanged()
	{
		SkiaApi.sk_drawable_notify_drawing_changed(Handle);
	}

	protected virtual void OnDraw(SKCanvas canvas)
	{
	}

	protected virtual SKRect OnGetBounds()
	{
		return SKRect.Empty;
	}

	protected virtual SKPicture OnSnapshot()
	{
		using SKPictureRecorder sKPictureRecorder = new SKPictureRecorder();
		SKCanvas canvas = sKPictureRecorder.BeginRecording(Bounds);
		Draw(canvas, 0f, 0f);
		return sKPictureRecorder.EndRecording();
	}

	[MonoPInvokeCallback(typeof(SKManagedDrawableDrawProxyDelegate))]
	private unsafe static void DrawInternal(IntPtr d, void* context, IntPtr canvas)
	{
		GCHandle gch;
		SKDrawable userData = DelegateProxies.GetUserData<SKDrawable>((IntPtr)context, out gch);
		userData.OnDraw(SKCanvas.GetObject(canvas, owns: false));
	}

	[MonoPInvokeCallback(typeof(SKManagedDrawableGetBoundsProxyDelegate))]
	private unsafe static void GetBoundsInternal(IntPtr d, void* context, SKRect* rect)
	{
		GCHandle gch;
		SKDrawable userData = DelegateProxies.GetUserData<SKDrawable>((IntPtr)context, out gch);
		SKRect sKRect = userData.OnGetBounds();
		*rect = sKRect;
	}

	[MonoPInvokeCallback(typeof(SKManagedDrawableNewPictureSnapshotProxyDelegate))]
	private unsafe static IntPtr NewPictureSnapshotInternal(IntPtr d, void* context)
	{
		GCHandle gch;
		SKDrawable userData = DelegateProxies.GetUserData<SKDrawable>((IntPtr)context, out gch);
		return userData.OnSnapshot()?.Handle ?? IntPtr.Zero;
	}

	[MonoPInvokeCallback(typeof(SKManagedDrawableDestroyProxyDelegate))]
	private unsafe static void DestroyInternal(IntPtr d, void* context)
	{
		GCHandle gch;
		SKDrawable userData = DelegateProxies.GetUserData<SKDrawable>((IntPtr)context, out gch);
		if (userData != null)
		{
			Interlocked.Exchange(ref userData.fromNative, 1);
			userData.Dispose();
		}
		gch.Free();
	}

	internal static SKDrawable GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKDrawable(h, o));
	}
}
