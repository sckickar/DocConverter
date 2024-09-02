namespace DocGen.Pdf.Parsing;

internal class SystemFontInitFeatureInfo : SystemFontFeatureInfo
{
	public override uint Tag => SystemFontTags.FEATURE_INITIAL_FORMS;

	public override FeatureType Type => FeatureType.SingleSubstitution;

	public override bool ShouldApply(SystemFontGlyphInfo glyphInfo)
	{
		return glyphInfo.Form == SystemFontGlyphForm.Initial;
	}
}
