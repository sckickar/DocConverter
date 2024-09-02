using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontFallbackRange
{
	private static List<SystemFontFallbackRange> fallbackRanges;

	public SystemFontRange[] Ranges { get; private set; }

	public string[] FallbackFontFamilies { get; private set; }

	private static void InitializeFallbackRanges()
	{
		fallbackRanges = new List<SystemFontFallbackRange>();
		fallbackRanges.Add(new SystemFontFallbackRange(new SystemFontRange[2]
		{
			new SystemFontRange(0, 591),
			new SystemFontRange(1024, 1327)
		}, new string[1] { "Times New Roman" }));
	}

	internal static SystemFontFallbackRange GetFallbackRange(char unicode)
	{
		foreach (SystemFontFallbackRange fallbackRange in fallbackRanges)
		{
			if (fallbackRange.FallsInRange(unicode))
			{
				return fallbackRange;
			}
		}
		return null;
	}

	static SystemFontFallbackRange()
	{
		InitializeFallbackRanges();
	}

	public SystemFontFallbackRange(SystemFontRange[] ranges, string[] fallbackFontFamilies)
	{
		Ranges = ranges;
		FallbackFontFamilies = fallbackFontFamilies;
	}

	public bool FallsInRange(char unicode)
	{
		SystemFontRange[] ranges = Ranges;
		for (int i = 0; i < ranges.Length; i++)
		{
			if (ranges[i].IsInRange(unicode))
			{
				return true;
			}
		}
		return false;
	}
}
