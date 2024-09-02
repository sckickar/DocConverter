namespace DocGen.Pdf.Parsing;

internal interface ISystemFontEncoding
{
	string GetGlyphName(SystemFontCFFFontFile file, ushort index);
}
