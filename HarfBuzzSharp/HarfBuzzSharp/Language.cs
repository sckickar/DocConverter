using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

public class Language : NativeObject
{
	private class StaticLanguage : Language
	{
		public StaticLanguage(IntPtr handle)
			: base(handle)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	private static readonly Lazy<Language> defaultLanguage = new Lazy<Language>(() => new StaticLanguage(HarfBuzzApi.hb_language_get_default()));

	public static Language Default => defaultLanguage.Value;

	public string Name { get; }

	internal Language(IntPtr handle)
		: base(handle)
	{
	}

	public Language(CultureInfo culture)
		: this(culture.TwoLetterISOLanguageName)
	{
	}

	public unsafe Language(string name)
		: base(IntPtr.Zero)
	{
		Handle = HarfBuzzApi.hb_language_from_string(name, -1);
		Name = Marshal.PtrToStringAnsi((IntPtr)HarfBuzzApi.hb_language_to_string(Handle));
	}

	public override string ToString()
	{
		return Name;
	}

	public override bool Equals(object obj)
	{
		if (obj is Language language)
		{
			return Handle == language.Handle;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (Name == null)
		{
			return 0;
		}
		return Name.GetHashCode();
	}
}
