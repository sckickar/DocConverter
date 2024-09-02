namespace DocGen.PdfViewer.Base;

internal interface IBuildCharacterOwner
{
	byte[] GetSubr(int index);

	byte[] GetGlobalSubr(int index);

	GlyphInfo GetGlyphData(string glyphName);
}
