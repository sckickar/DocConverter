using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp;

public class SKTypeface : SKObject, ISKReferenceCounted
{
	private sealed class SKTypefaceStatic : SKTypeface
	{
		internal SKTypefaceStatic(IntPtr x)
			: base(x, owns: false)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	private static readonly SKTypeface defaultTypeface;

	private SKFont font;

	public static SKTypeface Default => defaultTypeface;

	public string FamilyName => (string)SKString.GetObject(SkiaApi.sk_typeface_get_family_name(Handle));

	public SKFontStyle FontStyle => SKFontStyle.GetObject(SkiaApi.sk_typeface_get_fontstyle(Handle));

	public int FontWeight => SkiaApi.sk_typeface_get_font_weight(Handle);

	public int FontWidth => SkiaApi.sk_typeface_get_font_width(Handle);

	public SKFontStyleSlant FontSlant => SkiaApi.sk_typeface_get_font_slant(Handle);

	public bool IsBold => FontStyle.Weight >= 600;

	public bool IsItalic => FontStyle.Slant != SKFontStyleSlant.Upright;

	public bool IsFixedPitch => SkiaApi.sk_typeface_is_fixed_pitch(Handle);

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FontWeight and FontSlant instead.")]
	public SKTypefaceStyle Style
	{
		get
		{
			SKTypefaceStyle sKTypefaceStyle = SKTypefaceStyle.Normal;
			if (FontWeight >= 600)
			{
				sKTypefaceStyle |= SKTypefaceStyle.Bold;
			}
			if (FontSlant != 0)
			{
				sKTypefaceStyle |= SKTypefaceStyle.Italic;
			}
			return sKTypefaceStyle;
		}
	}

	public int UnitsPerEm => SkiaApi.sk_typeface_get_units_per_em(Handle);

	public int GlyphCount => SkiaApi.sk_typeface_count_glyphs(Handle);

	public int TableCount => SkiaApi.sk_typeface_count_tables(Handle);

	static SKTypeface()
	{
		defaultTypeface = new SKTypefaceStatic(SkiaApi.sk_typeface_ref_default());
	}

	internal static void EnsureStaticInstanceAreInitialized()
	{
	}

	internal SKTypeface(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public static SKTypeface CreateDefault()
	{
		return GetObject(SkiaApi.sk_typeface_create_default());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromFamilyName(string, SKFontStyleWeight, SKFontStyleWidth, SKFontStyleSlant) instead.")]
	public static SKTypeface FromFamilyName(string familyName, SKTypefaceStyle style)
	{
		SKFontStyleWeight weight = (style.HasFlag(SKTypefaceStyle.Bold) ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal);
		SKFontStyleSlant slant = (style.HasFlag(SKTypefaceStyle.Italic) ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
		return FromFamilyName(familyName, weight, SKFontStyleWidth.Normal, slant);
	}

	public static SKTypeface FromFamilyName(string familyName, int weight, int width, SKFontStyleSlant slant)
	{
		return FromFamilyName(familyName, new SKFontStyle(weight, width, slant));
	}

	public static SKTypeface FromFamilyName(string familyName)
	{
		return FromFamilyName(familyName, SKFontStyle.Normal);
	}

	public unsafe static SKTypeface FromFamilyName(string familyName, SKFontStyle style)
	{
		if (style == null)
		{
			throw new ArgumentNullException("style");
		}
		fixed (byte* value = StringUtilities.GetEncodedText(familyName, SKTextEncoding.Utf8, addNull: true))
		{
			SKTypeface @object = GetObject(SkiaApi.sk_typeface_create_from_name(new IntPtr(value), style.Handle));
			@object?.PreventPublicDisposal();
			return @object;
		}
	}

	public static SKTypeface FromFamilyName(string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
	{
		return FromFamilyName(familyName, (int)weight, (int)width, slant);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public static SKTypeface FromTypeface(SKTypeface typeface, SKTypefaceStyle style)
	{
		if (typeface == null)
		{
			throw new ArgumentNullException("typeface");
		}
		SKFontStyleWeight weight = (style.HasFlag(SKTypefaceStyle.Bold) ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal);
		SKFontStyleWidth width = SKFontStyleWidth.Normal;
		SKFontStyleSlant slant = (style.HasFlag(SKTypefaceStyle.Italic) ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
		return SKFontManager.Default.MatchTypeface(typeface, new SKFontStyle(weight, width, slant));
	}

	public unsafe static SKTypeface FromFile(string path, int index = 0)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		fixed (byte* path2 = StringUtilities.GetEncodedText(path, SKTextEncoding.Utf8, addNull: true))
		{
			return GetObject(SkiaApi.sk_typeface_create_from_file(path2, index));
		}
	}

	public static SKTypeface FromStream(Stream stream, int index = 0)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return FromStream(new SKManagedStream(stream, disposeManagedStream: true), index);
	}

	public static SKTypeface FromStream(SKStreamAsset stream, int index = 0)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (stream is SKManagedStream sKManagedStream)
		{
			stream = sKManagedStream.ToMemoryStream();
			sKManagedStream.Dispose();
		}
		SKTypeface @object = GetObject(SkiaApi.sk_typeface_create_from_stream(stream.Handle, index));
		stream.RevokeOwnership(@object);
		return @object;
	}

	public static SKTypeface FromData(SKData data, int index = 0)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return GetObject(SkiaApi.sk_typeface_create_from_data(data.Handle, index));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetGlyphs(string, out ushort[]) instead.")]
	public int CharsToGlyphs(string chars, out ushort[] glyphs)
	{
		return GetGlyphs(chars, out glyphs);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetGlyphs(IntPtr, int, SKTextEncoding, out ushort[]) instead.")]
	public int CharsToGlyphs(IntPtr str, int strlen, SKEncoding encoding, out ushort[] glyphs)
	{
		return GetGlyphs(str, strlen, encoding, out glyphs);
	}

	public uint[] GetTableTags()
	{
		if (!TryGetTableTags(out var tags))
		{
			throw new Exception("Unable to read the tables for the file.");
		}
		return tags;
	}

	public unsafe bool TryGetTableTags(out uint[] tags)
	{
		uint[] array = new uint[TableCount];
		fixed (uint* tags2 = array)
		{
			if (SkiaApi.sk_typeface_get_table_tags(Handle, tags2) == 0)
			{
				tags = null;
				return false;
			}
		}
		tags = array;
		return true;
	}

	public int GetTableSize(uint tag)
	{
		return (int)SkiaApi.sk_typeface_get_table_size(Handle, tag);
	}

	public byte[] GetTableData(uint tag)
	{
		if (!TryGetTableData(tag, out var tableData))
		{
			throw new Exception("Unable to read the data table.");
		}
		return tableData;
	}

	public unsafe bool TryGetTableData(uint tag, out byte[] tableData)
	{
		int tableSize = GetTableSize(tag);
		byte[] array = new byte[tableSize];
		fixed (byte* ptr = array)
		{
			if (!TryGetTableData(tag, 0, tableSize, (IntPtr)ptr))
			{
				tableData = null;
				return false;
			}
		}
		tableData = array;
		return true;
	}

	public unsafe bool TryGetTableData(uint tag, int offset, int length, IntPtr tableData)
	{
		IntPtr intPtr = SkiaApi.sk_typeface_get_table_data(Handle, tag, (IntPtr)offset, (IntPtr)length, (void*)tableData);
		return intPtr != IntPtr.Zero;
	}

	public int CountGlyphs(string str)
	{
		return GetFont().CountGlyphs(str);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CountGlyphs(string) instead.")]
	public int CountGlyphs(string str, SKEncoding encoding)
	{
		return GetFont().CountGlyphs(str);
	}

	public int CountGlyphs(ReadOnlySpan<char> str)
	{
		return GetFont().CountGlyphs(str);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CountGlyphs(byte[], SKTextEncoding) instead.")]
	public int CountGlyphs(byte[] str, SKEncoding encoding)
	{
		return GetFont().CountGlyphs(str, encoding.ToTextEncoding());
	}

	public int CountGlyphs(byte[] str, SKTextEncoding encoding)
	{
		return GetFont().CountGlyphs(str, encoding);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CountGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
	public int CountGlyphs(ReadOnlySpan<byte> str, SKEncoding encoding)
	{
		return GetFont().CountGlyphs(str, encoding.ToTextEncoding());
	}

	public int CountGlyphs(ReadOnlySpan<byte> str, SKTextEncoding encoding)
	{
		return GetFont().CountGlyphs(str, encoding);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CountGlyphs(IntPtr, int, SKTextEncoding) instead.")]
	public int CountGlyphs(IntPtr str, int strLen, SKEncoding encoding)
	{
		return CountGlyphs(str, strLen, encoding.ToTextEncoding());
	}

	public int CountGlyphs(IntPtr str, int strLen, SKTextEncoding encoding)
	{
		return GetFont().CountGlyphs(str, strLen * encoding.GetCharacterByteSize(), encoding);
	}

	public ushort GetGlyph(int codepoint)
	{
		return GetFont().GetGlyph(codepoint);
	}

	public ushort[] GetGlyphs(ReadOnlySpan<int> codepoints)
	{
		return GetFont().GetGlyphs(codepoints);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetGlyphs(string) instead.")]
	public int GetGlyphs(string text, out ushort[] glyphs)
	{
		glyphs = GetGlyphs(text);
		return glyphs.Length;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetGlyphs(string) instead.")]
	public int GetGlyphs(string text, SKEncoding encoding, out ushort[] glyphs)
	{
		return GetGlyphs(text, out glyphs);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetGlyphs(byte[], SKTextEncoding) instead.")]
	public int GetGlyphs(byte[] text, SKEncoding encoding, out ushort[] glyphs)
	{
		return GetGlyphs(text.AsSpan(), encoding, out glyphs);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
	public int GetGlyphs(ReadOnlySpan<byte> text, SKEncoding encoding, out ushort[] glyphs)
	{
		glyphs = GetGlyphs(text, encoding);
		return glyphs.Length;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetGlyphs(IntPtr, int, SKTextEncoding) instead.")]
	public int GetGlyphs(IntPtr text, int length, SKEncoding encoding, out ushort[] glyphs)
	{
		glyphs = GetGlyphs(text, length, encoding);
		return glyphs.Length;
	}

	public ushort[] GetGlyphs(string text)
	{
		return GetGlyphs(text.AsSpan());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetGlyphs(string) instead.")]
	public ushort[] GetGlyphs(string text, SKEncoding encoding)
	{
		return GetGlyphs(text.AsSpan());
	}

	public ushort[] GetGlyphs(ReadOnlySpan<char> text)
	{
		using SKFont sKFont = ToFont();
		return sKFont.GetGlyphs(text);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
	public ushort[] GetGlyphs(byte[] text, SKEncoding encoding)
	{
		return GetGlyphs(text.AsSpan(), encoding.ToTextEncoding());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
	public ushort[] GetGlyphs(ReadOnlySpan<byte> text, SKEncoding encoding)
	{
		return GetGlyphs(text, encoding.ToTextEncoding());
	}

	public ushort[] GetGlyphs(ReadOnlySpan<byte> text, SKTextEncoding encoding)
	{
		using SKFont sKFont = ToFont();
		return sKFont.GetGlyphs(text, encoding);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetGlyphs(IntPtr, int, SKTextEncoding) instead.")]
	public ushort[] GetGlyphs(IntPtr text, int length, SKEncoding encoding)
	{
		return GetGlyphs(text, length, encoding.ToTextEncoding());
	}

	public ushort[] GetGlyphs(IntPtr text, int length, SKTextEncoding encoding)
	{
		using SKFont sKFont = ToFont();
		return sKFont.GetGlyphs(text, length * encoding.GetCharacterByteSize(), encoding);
	}

	public bool ContainsGlyph(int codepoint)
	{
		return GetFont().ContainsGlyph(codepoint);
	}

	public bool ContainsGlyphs(ReadOnlySpan<int> codepoints)
	{
		return GetFont().ContainsGlyphs(codepoints);
	}

	public bool ContainsGlyphs(string text)
	{
		return GetFont().ContainsGlyphs(text);
	}

	public bool ContainsGlyphs(ReadOnlySpan<char> text)
	{
		return GetFont().ContainsGlyphs(text);
	}

	public bool ContainsGlyphs(ReadOnlySpan<byte> text, SKTextEncoding encoding)
	{
		return ContainsGlyphs(text, encoding);
	}

	public bool ContainsGlyphs(IntPtr text, int length, SKTextEncoding encoding)
	{
		return GetFont().ContainsGlyphs(text, length * encoding.GetCharacterByteSize(), encoding);
	}

	internal SKFont GetFont()
	{
		return font ?? (font = SKObject.OwnedBy(new SKFont(this), this));
	}

	public SKFont ToFont()
	{
		return new SKFont(this);
	}

	public SKFont ToFont(float size, float scaleX = 1f, float skewX = 0f)
	{
		return new SKFont(this, size, scaleX, skewX);
	}

	public SKStreamAsset OpenStream()
	{
		int ttcIndex;
		return OpenStream(out ttcIndex);
	}

	public unsafe SKStreamAsset OpenStream(out int ttcIndex)
	{
		fixed (int* ttcIndex2 = &ttcIndex)
		{
			return SKStreamAsset.GetObject(SkiaApi.sk_typeface_open_stream(Handle, ttcIndex2));
		}
	}

	public unsafe int[] GetKerningPairAdjustments(ReadOnlySpan<ushort> glyphs)
	{
		int[] array = new int[glyphs.Length];
		fixed (ushort* glyphs2 = glyphs)
		{
			fixed (int* adjustments = array)
			{
				SkiaApi.sk_typeface_get_kerning_pair_adjustments(Handle, glyphs2, glyphs.Length, adjustments);
			}
		}
		return array;
	}

	internal static SKTypeface GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKTypeface(h, o));
	}
}
