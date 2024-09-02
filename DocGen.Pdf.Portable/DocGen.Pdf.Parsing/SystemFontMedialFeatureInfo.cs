namespace DocGen.Pdf.Parsing;

internal class SystemFontMedialFeatureInfo : SystemFontFeatureInfo
{
	public override uint Tag => SystemFontTags.FEATURE_MEDIAL_FORMS;

	public override FeatureType Type => FeatureType.SingleSubstitution;

	public override bool ShouldApply(SystemFontGlyphInfo glyphInfo)
	{
		return glyphInfo.Form == SystemFontGlyphForm.Medial;
	}
}
