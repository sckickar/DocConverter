using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using DocGen.Drawing;

namespace DocGen.PdfViewer.Base;

internal class StdFontsAssistant
{
	private const string Courier = "Courier";

	private const string CourierBold = "Courier-Bold";

	private const string CourierBoldOblique = "Courier-BoldOblique";

	private const string CourierOblique = "Courier-Oblique";

	private const string Helvetica = "Helvetica";

	private const string HelveticaBold = "Helvetica-Bold";

	private const string HelveticaBoldOblique = "Helvetica-BoldOblique";

	private const string HelveticaOblique = "Helvetica-Oblique";

	private const string TimesRoman = "Times-Roman";

	private const string TimesBold = "Times-Bold";

	private const string TimesBoldItalic = "Times-BoldItalic";

	private const string TimesItalic = "Times-Italic";

	private const string Symbol = "Symbol";

	private const string ZapfDingbats = "ZapfDingbats";

	private static readonly Dictionary<string, string> alternativeNames;

	internal static readonly Dictionary<string, StandardFontDescriptor> standardFontDescriptors;

	private static Dictionary<string, byte[]> standardFontStreams;

	private readonly Dictionary<string, Type1FontSource> standardFontSources;

	private StdFontsAssistant systemFontsManager;

	private object syncObj = new object();

	public StdFontsAssistant SystemFontsManager
	{
		get
		{
			if (systemFontsManager == null)
			{
				systemFontsManager = new StdFontsAssistant();
			}
			return systemFontsManager;
		}
	}

	static StdFontsAssistant()
	{
		standardFontDescriptors = new Dictionary<string, StandardFontDescriptor>();
		alternativeNames = new Dictionary<string, string>();
		InitializeStandardFontStreams();
		InitializeStandardFontDescriptors();
		InitializeAlternativeNames();
	}

	public StdFontsAssistant()
	{
		standardFontSources = new Dictionary<string, Type1FontSource>();
		InitializeStandardFontSources();
	}

	public static string StripFontName(string fontName)
	{
		int num = fontName.IndexOf("+");
		if (num > 0)
		{
			return fontName.Substring(num + 1).Split(Chars.FontFamilyDelimiters)[0];
		}
		return fontName.Split(Chars.FontFamilyDelimiters)[0];
	}

	public static void InitializeStandardFontStreams()
	{
		if (standardFontStreams == null)
		{
			standardFontStreams = new Dictionary<string, byte[]>();
			RegisterStandardFontStream("Courier", GetApplicationResourceStream("Courier"));
			RegisterStandardFontStream("Courier-Bold", GetApplicationResourceStream("Courier-Bold"));
			RegisterStandardFontStream("Courier-BoldOblique", GetApplicationResourceStream("Courier-BoldOblique"));
			RegisterStandardFontStream("Courier-Oblique", GetApplicationResourceStream("Courier-Oblique"));
			RegisterStandardFontStream("Helvetica", GetApplicationResourceStream("Helvetica"));
			RegisterStandardFontStream("Helvetica-Bold", GetApplicationResourceStream("Helvetica-Bold"));
			RegisterStandardFontStream("Helvetica-BoldOblique", GetApplicationResourceStream("Helvetica-BoldOblique"));
			RegisterStandardFontStream("Helvetica-Oblique", GetApplicationResourceStream("Helvetica-Oblique"));
			RegisterStandardFontStream("Times-Roman", GetApplicationResourceStream("Times-Roman"));
			RegisterStandardFontStream("Times-Bold", GetApplicationResourceStream("Times-Bold"));
			RegisterStandardFontStream("Times-BoldItalic", GetApplicationResourceStream("Times-BoldItalic"));
			RegisterStandardFontStream("Times-Italic", GetApplicationResourceStream("Times-Italic"));
			RegisterStandardFontStream("Symbol", GetApplicationResourceStream("Symbol"));
			RegisterStandardFontStream("ZapfDingbats", GetApplicationResourceStream("ZapfDingbats"));
		}
	}

	public static bool IsStandardFontName(string name)
	{
		if (!standardFontDescriptors.ContainsKey(name))
		{
			return alternativeNames.ContainsKey(name);
		}
		return true;
	}

	public static bool IsAlternativeStdFontAvailable(string name)
	{
		if (alternativeNames.ContainsKey(name))
		{
			return true;
		}
		return false;
	}

	public static StandardFontDescriptor GetStandardFontDescriptor(string fontName)
	{
		string standardFontName = GetStandardFontName(fontName);
		if (!standardFontDescriptors.TryGetValue(standardFontName, out StandardFontDescriptor value))
		{
			throw new ArgumentException("Font name is not a standard font.", fontName);
		}
		return value;
	}

	public Type1FontSource GetStandardFontSource(string fontName)
	{
		Monitor.Enter(syncObj);
		Type1FontSource value;
		try
		{
			string standardFontName = GetStandardFontName(fontName);
			if (!standardFontSources.TryGetValue(standardFontName, out value))
			{
				bool flag = false;
				foreach (KeyValuePair<string, byte[]> standardFontStream in standardFontStreams)
				{
					if (standardFontStream.Key == standardFontName)
					{
						RegisterStandardFontSource(standardFontStream.Key, CreateFontSource(standardFontStream.Value));
						value = standardFontSources[standardFontName];
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					throw new ArgumentException("Font name is not a standard font.", fontName);
				}
			}
		}
		finally
		{
			Monitor.Exit(syncObj);
		}
		return value;
	}

	public Type1FontSource GetType1FallbackFontSource(string fontName)
	{
		string styles = fontName.ToLower();
		bool flag = IsBold(styles);
		bool flag2 = IsItalic(styles);
		string fontName2 = "Helvetica";
		if (flag)
		{
			fontName2 = ((!flag2) ? "Helvetica-Bold" : "Helvetica-BoldOblique");
		}
		else if (flag2)
		{
			fontName2 = "Helvetica-Oblique";
		}
		return GetStandardFontSource(fontName2);
	}

	private static Stream GetApplicationResourceStream(string fontName)
	{
		try
		{
			Assembly assembly = Assembly.Load(new AssemblyName("DocGen.Pdf.Portable"));
			assembly.GetManifestResourceNames();
			return assembly.GetManifestResourceStream($"DocGen.Pdf.Portable.Resources.{fontName}.fnt");
		}
		catch
		{
			throw new InvalidOperationException("Cannot access the resource file");
		}
	}

	private static string GetStandardFontName(string fontName)
	{
		string result = fontName;
		lock (new object())
		{
			if (alternativeNames.ContainsKey(fontName))
			{
				result = alternativeNames[fontName];
			}
		}
		return result;
	}

	private static void RegisterStandardFontStream(string fontName, Stream stream)
	{
		standardFontStreams[fontName] = Extensions.ReadAllBytes(stream);
	}

	private static void RegisterStandardFontDescriptor(string name, StandardFontDescriptor fontDescriptor)
	{
		standardFontDescriptors[name] = fontDescriptor;
	}

	private static bool IsBold(string styles)
	{
		if (!string.IsNullOrEmpty(styles))
		{
			if (!styles.Contains("bold"))
			{
				return styles.Contains("bd");
			}
			return true;
		}
		return false;
	}

	private static bool IsItalic(string styles)
	{
		if (!string.IsNullOrEmpty(styles))
		{
			if (!styles.Contains("it"))
			{
				return styles.Contains("obl");
			}
			return true;
		}
		return false;
	}

	internal static void InitializeStandardFontDescriptors()
	{
		StandardFontDescriptor standardFontDescriptor = new StandardFontDescriptor();
		standardFontDescriptor.Ascent = 629.0;
		standardFontDescriptor.Descent = -157.0;
		StandardFontDescriptor standardFontDescriptor2 = new StandardFontDescriptor();
		standardFontDescriptor2.Ascent = 718.0;
		standardFontDescriptor2.Descent = -207.0;
		StandardFontDescriptor fontDescriptor = new StandardFontDescriptor();
		standardFontDescriptor2.Ascent = 683.0;
		standardFontDescriptor2.Descent = -217.0;
		StandardFontDescriptor fontDescriptor2 = new StandardFontDescriptor();
		standardFontDescriptor2.Ascent = 1000.0;
		standardFontDescriptor2.Descent = 0.0;
		RegisterStandardFontDescriptor("Courier", standardFontDescriptor);
		RegisterStandardFontDescriptor("Courier-Bold", standardFontDescriptor);
		RegisterStandardFontDescriptor("Courier-BoldOblique", standardFontDescriptor);
		RegisterStandardFontDescriptor("Courier-Oblique", standardFontDescriptor);
		RegisterStandardFontDescriptor("Helvetica", standardFontDescriptor2);
		RegisterStandardFontDescriptor("Helvetica-Bold", standardFontDescriptor2);
		RegisterStandardFontDescriptor("Helvetica-BoldOblique", standardFontDescriptor2);
		RegisterStandardFontDescriptor("Helvetica-Oblique", standardFontDescriptor2);
		RegisterStandardFontDescriptor("Times-Roman", fontDescriptor);
		RegisterStandardFontDescriptor("Times-Bold", fontDescriptor);
		RegisterStandardFontDescriptor("Times-BoldItalic", fontDescriptor);
		RegisterStandardFontDescriptor("Times-Italic", fontDescriptor);
		RegisterStandardFontDescriptor("Symbol", fontDescriptor2);
		RegisterStandardFontDescriptor("ZapfDingbats", fontDescriptor2);
	}

	private static void InitializeAlternativeNames()
	{
		RegisterAlternativeName("Helvetica", "Helvetica");
		RegisterAlternativeName("Helvetica-Bold", "Helvetica,Bold");
		RegisterAlternativeName("Helvetica-BoldOblique", "Helvetica,BoldItalic");
		RegisterAlternativeName("Helvetica-Oblique", "Helvetica,Italic");
		RegisterAlternativeName("Courier", "Courier New");
		RegisterAlternativeName("Courier-Bold", "Courier New,Bold");
		RegisterAlternativeName("Courier-BoldOblique", "Courier New,BoldItalic");
		RegisterAlternativeName("Courier-Oblique", "Courier New,Italic");
		RegisterAlternativeName("Times-Roman", "Times New Roman");
		RegisterAlternativeName("Times-Bold", "Times New Roman,Bold");
		RegisterAlternativeName("Times-BoldItalic", "Times New Roman,BoldItalic");
		RegisterAlternativeName("Times-Italic", "Times New Roman,Italic");
	}

	private static string GetFontStylesFromFontName(string fontName)
	{
		string[] array = fontName.Split(Chars.FontFamilyDelimiters);
		if (array.Length > 1)
		{
			return array[1];
		}
		return string.Empty;
	}

	private static void RegisterAlternativeName(string original, params string[] alternatives)
	{
		foreach (string key in alternatives)
		{
			alternativeNames[key] = original;
		}
	}

	private static Type1FontSource CreateFontSource(byte[] data)
	{
		try
		{
			return new Type1FontSource(data);
		}
		catch
		{
			throw new InvalidOperationException("Cannot create standard font.");
		}
	}

	private void InitializeStandardFontSources()
	{
		if (standardFontStreams != null && standardFontStreams.Count == 0)
		{
			standardFontStreams = new Dictionary<string, byte[]>();
			RegisterStandardFontStream("Courier", GetApplicationResourceStream("Courier"));
			RegisterStandardFontStream("Courier-Bold", GetApplicationResourceStream("Courier-Bold"));
			RegisterStandardFontStream("Courier-BoldOblique", GetApplicationResourceStream("Courier-BoldOblique"));
			RegisterStandardFontStream("Courier-Oblique", GetApplicationResourceStream("Courier-Oblique"));
			RegisterStandardFontStream("Helvetica", GetApplicationResourceStream("Helvetica"));
			RegisterStandardFontStream("Helvetica-Bold", GetApplicationResourceStream("Helvetica-Bold"));
			RegisterStandardFontStream("Helvetica-BoldOblique", GetApplicationResourceStream("Helvetica-BoldOblique"));
			RegisterStandardFontStream("Helvetica-Oblique", GetApplicationResourceStream("Helvetica-Oblique"));
			RegisterStandardFontStream("Times-Roman", GetApplicationResourceStream("Times-Roman"));
			RegisterStandardFontStream("Times-Bold", GetApplicationResourceStream("Times-Bold"));
			RegisterStandardFontStream("Times-BoldItalic", GetApplicationResourceStream("Times-BoldItalic"));
			RegisterStandardFontStream("Times-Italic", GetApplicationResourceStream("Times-Italic"));
			RegisterStandardFontStream("Symbol", GetApplicationResourceStream("Symbol"));
			RegisterStandardFontStream("ZapfDingbats", GetApplicationResourceStream("ZapfDingbats"));
			InitializeStandardFontDescriptors();
			InitializeAlternativeNames();
		}
	}

	private void RegisterStandardFontSource(string fontName, Type1FontSource fontSource)
	{
		standardFontSources[fontName] = fontSource;
	}

	public static void GetFontFamily(string fontName, out string fontFamily, out FontStyle fontStyle)
	{
		if (!PredefinedFontFamilies.TryGetFontFamily(fontName, out fontFamily))
		{
			fontFamily = StripFontName(fontName);
		}
		string text = GetFontStylesFromFontName(fontName).ToLower();
		fontStyle = FontStyle.Regular;
		if (IsBold(text) || text.Contains("black"))
		{
			fontStyle = FontStyle.Bold;
		}
		if (IsItalic(text))
		{
			fontStyle |= FontStyle.Italic;
		}
	}

	internal void Dispose()
	{
		if (standardFontDescriptors != null && standardFontDescriptors.Count > 0)
		{
			standardFontDescriptors.Clear();
		}
		if (alternativeNames != null && alternativeNames.Count > 0)
		{
			alternativeNames.Clear();
		}
		if (standardFontStreams != null && standardFontStreams.Count > 0)
		{
			standardFontStreams.Clear();
		}
		if (standardFontSources.Count > 0)
		{
			standardFontSources.Clear();
		}
	}
}
