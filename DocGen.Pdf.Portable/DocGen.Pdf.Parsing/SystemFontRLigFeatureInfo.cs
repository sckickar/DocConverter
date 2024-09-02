namespace DocGen.Pdf.Parsing;

internal class SystemFontRLigFeatureInfo : SystemFontFeatureInfo
{
	public override uint Tag => SystemFontTags.FEATURE_REQUIRED_LIGATURES;

	public override FeatureType Type => FeatureType.MultipleSubstitution;

	public override bool ShouldApply(SystemFontGlyphInfo glyphInfo)
	{
		return false;
	}
}
