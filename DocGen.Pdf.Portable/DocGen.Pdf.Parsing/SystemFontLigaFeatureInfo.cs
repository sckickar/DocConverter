namespace DocGen.Pdf.Parsing;

internal class SystemFontLigaFeatureInfo : SystemFontFeatureInfo
{
	public override uint Tag => SystemFontTags.FEATURE_STANDARD_LIGATURES;

	public override FeatureType Type => FeatureType.MultipleSubstitution;

	public override bool ShouldApply(SystemFontGlyphInfo glyphInfo)
	{
		return false;
	}
}
