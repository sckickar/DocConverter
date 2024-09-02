namespace DocGen.Pdf.Parsing;

internal interface ISystemFontBuildCharHolder
{
	byte[] GetSubr(int index);

	byte[] GetGlobalSubr(int index);

	SystemFontType1GlyphData GetGlyphData(string glyphName);
}
