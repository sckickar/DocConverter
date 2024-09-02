namespace DocGen.Pdf.Parsing;

internal interface ISystemFontType1BuildCharHolder
{
	byte[] GetSubr(int index);

	byte[] GetGlobalSubr(int index);

	SystemFontGlyphData GetGlyphData(string glyphName);
}
