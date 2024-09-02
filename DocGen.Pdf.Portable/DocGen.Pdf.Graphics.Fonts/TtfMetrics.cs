using DocGen.Pdf.Native;

namespace DocGen.Pdf.Graphics.Fonts;

internal struct TtfMetrics
{
	public int LineGap;

	public bool ContainsCFF;

	public bool IsSymbol;

	public RECT FontBox;

	public bool IsFixedPitch;

	public float ItalicAngle;

	public string PostScriptName;

	public string FontFamily;

	public float CapHeight;

	public float Leading;

	public float MacAscent;

	public float MacDescent;

	public float WinDescent;

	public float WinAscent;

	public float StemV;

	public float[] WidthTable;

	public int MacStyle;

	public float SubScriptSizeFactor;

	public float SuperscriptSizeFactor;

	public bool IsItalic => (MacStyle & 2) != 0;

	public bool IsBold => (MacStyle & 1) != 0;
}
