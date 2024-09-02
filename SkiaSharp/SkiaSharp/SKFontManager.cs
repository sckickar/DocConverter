using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkiaSharp;

public class SKFontManager : SKObject, ISKReferenceCounted
{
	private sealed class SKFontManagerStatic : SKFontManager
	{
		internal SKFontManagerStatic(IntPtr x)
			: base(x, owns: false)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	private static readonly SKFontManager defaultManager;

	public static SKFontManager Default => defaultManager;

	public int FontFamilyCount => SkiaApi.sk_fontmgr_count_families(Handle);

	public IEnumerable<string> FontFamilies
	{
		get
		{
			int count = FontFamilyCount;
			for (int i = 0; i < count; i++)
			{
				yield return GetFamilyName(i);
			}
		}
	}

	static SKFontManager()
	{
		defaultManager = new SKFontManagerStatic(SkiaApi.sk_fontmgr_ref_default());
	}

	internal static void EnsureStaticInstanceAreInitialized()
	{
	}

	internal SKFontManager(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public string GetFamilyName(int index)
	{
		using SKString sKString = new SKString();
		SkiaApi.sk_fontmgr_get_family_name(Handle, index, sKString.Handle);
		return (string)sKString;
	}

	public string[] GetFontFamilies()
	{
		return FontFamilies.ToArray();
	}

	public SKFontStyleSet GetFontStyles(int index)
	{
		return SKFontStyleSet.GetObject(SkiaApi.sk_fontmgr_create_styleset(Handle, index));
	}

	public unsafe SKFontStyleSet GetFontStyles(string familyName)
	{
		fixed (byte* value = StringUtilities.GetEncodedText(familyName, SKTextEncoding.Utf8, addNull: true))
		{
			return SKFontStyleSet.GetObject(SkiaApi.sk_fontmgr_match_family(Handle, new IntPtr(value)));
		}
	}

	public SKTypeface MatchFamily(string familyName)
	{
		return MatchFamily(familyName, SKFontStyle.Normal);
	}

	public unsafe SKTypeface MatchFamily(string familyName, SKFontStyle style)
	{
		if (style == null)
		{
			throw new ArgumentNullException("style");
		}
		fixed (byte* value = StringUtilities.GetEncodedText(familyName, SKTextEncoding.Utf8, addNull: true))
		{
			SKTypeface @object = SKTypeface.GetObject(SkiaApi.sk_fontmgr_match_family_style(Handle, new IntPtr(value), style.Handle));
			@object?.PreventPublicDisposal();
			return @object;
		}
	}

	public SKTypeface MatchTypeface(SKTypeface face, SKFontStyle style)
	{
		if (face == null)
		{
			throw new ArgumentNullException("face");
		}
		if (style == null)
		{
			throw new ArgumentNullException("style");
		}
		SKTypeface @object = SKTypeface.GetObject(SkiaApi.sk_fontmgr_match_face_style(Handle, face.Handle, style.Handle));
		@object?.PreventPublicDisposal();
		return @object;
	}

	public unsafe SKTypeface CreateTypeface(string path, int index = 0)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		fixed (byte* path2 = StringUtilities.GetEncodedText(path, SKTextEncoding.Utf8, addNull: true))
		{
			return SKTypeface.GetObject(SkiaApi.sk_fontmgr_create_from_file(Handle, path2, index));
		}
	}

	public SKTypeface CreateTypeface(Stream stream, int index = 0)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return CreateTypeface(new SKManagedStream(stream, disposeManagedStream: true), index);
	}

	public SKTypeface CreateTypeface(SKStreamAsset stream, int index = 0)
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
		SKTypeface @object = SKTypeface.GetObject(SkiaApi.sk_fontmgr_create_from_stream(Handle, stream.Handle, index));
		stream.RevokeOwnership(@object);
		return @object;
	}

	public SKTypeface CreateTypeface(SKData data, int index = 0)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return SKTypeface.GetObject(SkiaApi.sk_fontmgr_create_from_data(Handle, data.Handle, index));
	}

	public SKTypeface MatchCharacter(char character)
	{
		return MatchCharacter(null, SKFontStyle.Normal, null, character);
	}

	public SKTypeface MatchCharacter(int character)
	{
		return MatchCharacter(null, SKFontStyle.Normal, null, character);
	}

	public SKTypeface MatchCharacter(string familyName, char character)
	{
		return MatchCharacter(familyName, SKFontStyle.Normal, null, character);
	}

	public SKTypeface MatchCharacter(string familyName, int character)
	{
		return MatchCharacter(familyName, SKFontStyle.Normal, null, character);
	}

	public SKTypeface MatchCharacter(string familyName, string[] bcp47, char character)
	{
		return MatchCharacter(familyName, SKFontStyle.Normal, bcp47, character);
	}

	public SKTypeface MatchCharacter(string familyName, string[] bcp47, int character)
	{
		return MatchCharacter(familyName, SKFontStyle.Normal, bcp47, character);
	}

	public SKTypeface MatchCharacter(string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string[] bcp47, char character)
	{
		return MatchCharacter(familyName, new SKFontStyle(weight, width, slant), bcp47, character);
	}

	public SKTypeface MatchCharacter(string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string[] bcp47, int character)
	{
		return MatchCharacter(familyName, new SKFontStyle(weight, width, slant), bcp47, character);
	}

	public SKTypeface MatchCharacter(string familyName, int weight, int width, SKFontStyleSlant slant, string[] bcp47, int character)
	{
		return MatchCharacter(familyName, new SKFontStyle(weight, width, slant), bcp47, character);
	}

	public unsafe SKTypeface MatchCharacter(string familyName, SKFontStyle style, string[] bcp47, int character)
	{
		if (style == null)
		{
			throw new ArgumentNullException("style");
		}
		if (familyName == null)
		{
			familyName = string.Empty;
		}
		fixed (byte* value = StringUtilities.GetEncodedText(familyName, SKTextEncoding.Utf8, addNull: true))
		{
			SKTypeface @object = SKTypeface.GetObject(SkiaApi.sk_fontmgr_match_family_style_character(Handle, new IntPtr(value), style.Handle, bcp47, (bcp47 != null) ? bcp47.Length : 0, character));
			@object?.PreventPublicDisposal();
			return @object;
		}
	}

	public static SKFontManager CreateDefault()
	{
		return GetObject(SkiaApi.sk_fontmgr_create_default());
	}

	internal static SKFontManager GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKFontManager(h, o));
	}
}
