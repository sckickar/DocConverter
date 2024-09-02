using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DocGen.Pdf.Parsing;

internal class SystemFontsManager
{
	private static readonly HashSet<string> fontsWithKerning;

	private static readonly HashSet<string> monospacedFonts;

	private static readonly List<string> systemFonts;

	static SystemFontsManager()
	{
		fontsWithKerning = new HashSet<string>();
		monospacedFonts = new HashSet<string>();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		using (StreamReader streamReader = new StreamReader(SystemFontDocumentsHelper.GetResourceStream("Fonts.meta")))
		{
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				if (!text.StartsWith("//"))
				{
					string[] array = text.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					string text2 = array[0];
					if (array[1] == "1")
					{
						monospacedFonts.Add(text2);
					}
					if (array[2] == "1")
					{
						fontsWithKerning.Add(text2);
					}
					for (int i = 3; i < array.Length; i++)
					{
						dictionary[array[i].ToLower()] = text2;
					}
				}
			}
		}
		systemFonts = new List<string>(new HashSet<string>());
		systemFonts.Sort();
	}

	public static IEnumerable<string> GetAvailableFonts()
	{
		return systemFonts;
	}

	public static bool HasKerning(string fontName)
	{
		return fontsWithKerning.Contains(fontName);
	}

	public static bool IsMonospaced(string fontFamily)
	{
		if (fontFamily == null)
		{
			return false;
		}
		string[] array = fontFamily.Split(new char[1] { ',' });
		foreach (string text in array)
		{
			if (!monospacedFonts.Contains(text.Trim()))
			{
				return false;
			}
		}
		return true;
	}

	private static string SanitizeFontFileName(string fileName)
	{
		fileName = fileName.ToLower();
		return Regex.Replace(Path.GetFileNameWithoutExtension(fileName), "_[0-9]+$", string.Empty) + Path.GetExtension(fileName);
	}
}
