using System;
using System.ComponentModel;
using SkiaSharp.Internals;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete]
public abstract class SKPixelSerializer : SKObject, ISKSkipObjectRegistration
{
	protected SKPixelSerializer()
		: base(IntPtr.Zero, owns: false)
	{
	}

	public bool UseEncodedData(IntPtr data, ulong length)
	{
		if (!PlatformConfiguration.Is64Bit && length > uint.MaxValue)
		{
			throw new ArgumentOutOfRangeException("length", "The length exceeds the size of pointers.");
		}
		return OnUseEncodedData(data, (IntPtr)(long)length);
	}

	public SKData Encode(SKPixmap pixmap)
	{
		if (pixmap == null)
		{
			throw new ArgumentNullException("pixmap");
		}
		return OnEncode(pixmap);
	}

	protected abstract bool OnUseEncodedData(IntPtr data, IntPtr length);

	protected abstract SKData OnEncode(SKPixmap pixmap);

	public static SKPixelSerializer Create(Func<SKPixmap, SKData> onEncode)
	{
		return new SKSimplePixelSerializer(null, onEncode);
	}

	public static SKPixelSerializer Create(Func<IntPtr, IntPtr, bool> onUseEncodedData, Func<SKPixmap, SKData> onEncode)
	{
		return new SKSimplePixelSerializer(onUseEncodedData, onEncode);
	}
}
