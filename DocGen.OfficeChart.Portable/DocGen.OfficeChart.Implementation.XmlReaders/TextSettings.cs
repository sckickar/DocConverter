using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation.XmlReaders;

internal class TextSettings
{
	public string FontName;

	public float? FontSize;

	public bool? Bold;

	public bool? Italic;

	public bool? Underline;

	public bool? Striked;

	public string Language;

	public Color? FontColor;

	public int Baseline;

	public bool? HasLatin;

	public bool? HasComplexScripts;

	public bool? HasEastAsianFont;

	public string ActualFontName;

	internal bool? ShowSizeProperties;

	internal Dictionary<string, Stream> PreservedElements = new Dictionary<string, Stream>();

	internal bool HasCapitalization;

	internal float KerningValue;

	internal float SpacingValue;
}
