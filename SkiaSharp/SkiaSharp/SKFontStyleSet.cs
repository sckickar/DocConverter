using System;
using System.Collections;
using System.Collections.Generic;

namespace SkiaSharp;

public class SKFontStyleSet : SKObject, ISKReferenceCounted, IEnumerable<SKFontStyle>, IEnumerable, IReadOnlyCollection<SKFontStyle>, IReadOnlyList<SKFontStyle>
{
	public int Count => SkiaApi.sk_fontstyleset_get_count(Handle);

	public SKFontStyle this[int index] => GetStyle(index);

	internal SKFontStyleSet(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKFontStyleSet()
		: this(SkiaApi.sk_fontstyleset_create_empty(), owns: true)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public string GetStyleName(int index)
	{
		using SKString sKString = new SKString();
		SkiaApi.sk_fontstyleset_get_style(Handle, index, IntPtr.Zero, sKString.Handle);
		GC.KeepAlive(this);
		return (string)sKString;
	}

	public SKTypeface CreateTypeface(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the set.", "index");
		}
		SKTypeface @object = SKTypeface.GetObject(SkiaApi.sk_fontstyleset_create_typeface(Handle, index));
		@object?.PreventPublicDisposal();
		GC.KeepAlive(this);
		return @object;
	}

	public SKTypeface CreateTypeface(SKFontStyle style)
	{
		if (style == null)
		{
			throw new ArgumentNullException("style");
		}
		SKTypeface @object = SKTypeface.GetObject(SkiaApi.sk_fontstyleset_match_style(Handle, style.Handle));
		@object?.PreventPublicDisposal();
		GC.KeepAlive(this);
		return @object;
	}

	public IEnumerator<SKFontStyle> GetEnumerator()
	{
		return GetStyles().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetStyles().GetEnumerator();
	}

	private IEnumerable<SKFontStyle> GetStyles()
	{
		int count = Count;
		for (int i = 0; i < count; i++)
		{
			yield return GetStyle(i);
		}
	}

	private SKFontStyle GetStyle(int index)
	{
		SKFontStyle sKFontStyle = new SKFontStyle();
		SkiaApi.sk_fontstyleset_get_style(Handle, index, sKFontStyle.Handle, IntPtr.Zero);
		return sKFontStyle;
	}

	internal static SKFontStyleSet GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKFontStyleSet(h, o));
	}
}
