namespace DocGen.Pdf.Parsing;

internal class SystemFontIsolatedFeatureInfo : SystemFontFeatureInfo
{
	public override uint Tag => SystemFontTags.FEATURE_ISOLATED_FORMS;

	public override FeatureType Type => FeatureType.SingleSubstitution;

	public override bool ShouldApply(SystemFontGlyphInfo glyphInfo)
	{
		return glyphInfo.Form == SystemFontGlyphForm.Isolated;
	}
}
