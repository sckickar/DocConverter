namespace DocGen.Pdf.Parsing;

internal class SystemFontFinalFeatureInfo : SystemFontFeatureInfo
{
	public override uint Tag => SystemFontTags.FEATURE_TERMINAL_FORMS;

	public override FeatureType Type => FeatureType.SingleSubstitution;

	public override bool ShouldApply(SystemFontGlyphInfo glyphInfo)
	{
		return glyphInfo.Form == SystemFontGlyphForm.Final;
	}
}
