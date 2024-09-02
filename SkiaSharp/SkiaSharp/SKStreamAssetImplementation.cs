using System;

namespace SkiaSharp;

internal class SKStreamAssetImplementation : SKStreamAsset
{
	internal SKStreamAssetImplementation(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_stream_asset_destroy(Handle);
	}
}
