using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("The Index8 color type and color table is no longer supported.")]
public class SKColorTable : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
{
	public const int MaxLength = 256;

	public int Count => SkiaApi.sk_colortable_count(Handle);

	public SKPMColor[] Colors
	{
		get
		{
			int count = Count;
			IntPtr intPtr = ReadColors();
			if (count == 0 || intPtr == IntPtr.Zero)
			{
				return new SKPMColor[0];
			}
			return SKObject.PtrToStructureArray<SKPMColor>(intPtr, count);
		}
	}

	public SKColor[] UnPreMultipledColors => SKPMColor.UnPreMultiply(Colors);

	public SKPMColor this[int index]
	{
		get
		{
			int count = Count;
			IntPtr intPtr = ReadColors();
			if (index < 0 || index >= count || intPtr == IntPtr.Zero)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return SKObject.PtrToStructure<SKPMColor>(intPtr, index);
		}
	}

	internal SKColorTable(IntPtr x, bool owns)
		: base(x, owns)
	{
	}

	public SKColorTable()
		: this(new SKPMColor[256])
	{
	}

	public SKColorTable(int count)
		: this(new SKPMColor[count])
	{
	}

	public SKColorTable(SKColor[] colors)
		: this(colors, colors.Length)
	{
	}

	public SKColorTable(SKColor[] colors, int count)
		: this(SKPMColor.PreMultiply(colors), count)
	{
	}

	public SKColorTable(SKPMColor[] colors)
		: this(colors, colors.Length)
	{
	}

	public SKColorTable(SKPMColor[] colors, int count)
		: this(CreateNew(colors, count), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKColorTable instance.");
		}
	}

	private unsafe static IntPtr CreateNew(SKPMColor[] colors, int count)
	{
		fixed (SKPMColor* colors2 = colors)
		{
			return SkiaApi.sk_colortable_new((uint*)colors2, count);
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public SKColor GetUnPreMultipliedColor(int index)
	{
		return SKPMColor.UnPreMultiply(this[index]);
	}

	public unsafe IntPtr ReadColors()
	{
		uint* ptr = default(uint*);
		SkiaApi.sk_colortable_read_colors(Handle, &ptr);
		return (IntPtr)ptr;
	}
}
