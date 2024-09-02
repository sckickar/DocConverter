using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete]
internal class SKSimplePixelSerializer : SKPixelSerializer
{
	private readonly Func<IntPtr, IntPtr, bool> onUseEncodedData;

	private readonly Func<SKPixmap, SKData> onEncode;

	public SKSimplePixelSerializer(Func<IntPtr, IntPtr, bool> onUseEncodedData, Func<SKPixmap, SKData> onEncode)
	{
		this.onUseEncodedData = onUseEncodedData;
		this.onEncode = onEncode;
	}

	protected override SKData OnEncode(SKPixmap pixmap)
	{
		return onEncode?.Invoke(pixmap) ?? null;
	}

	protected override bool OnUseEncodedData(IntPtr data, IntPtr length)
	{
		return onUseEncodedData?.Invoke(data, length) ?? false;
	}
}
