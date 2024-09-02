using System;

namespace SkiaSharp;

public abstract class SKStreamAsset : SKStreamSeekable
{
	internal SKStreamAsset(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	internal new static SKStreamAsset GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (Func<IntPtr, bool, SKStreamAsset>)((IntPtr h, bool o) => new SKStreamAssetImplementation(h, o)));
	}
}
