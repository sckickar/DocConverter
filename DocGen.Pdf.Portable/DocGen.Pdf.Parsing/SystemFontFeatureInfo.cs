namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontFeatureInfo
{
	internal enum FeatureType
	{
		SingleSubstitution,
		MultipleSubstitution,
		Unsupported
	}

	public abstract uint Tag { get; }

	public abstract FeatureType Type { get; }

	internal static SystemFontFeatureInfo CreateFeatureInfo(uint featureTag)
	{
		if (featureTag == SystemFontTags.FEATURE_INITIAL_FORMS)
		{
			return new SystemFontInitFeatureInfo();
		}
		if (featureTag == SystemFontTags.FEATURE_TERMINAL_FORMS)
		{
			return new SystemFontFinalFeatureInfo();
		}
		if (featureTag == SystemFontTags.FEATURE_ISOLATED_FORMS)
		{
			return new SystemFontIsolatedFeatureInfo();
		}
		if (featureTag == SystemFontTags.FEATURE_MEDIAL_FORMS)
		{
			return new SystemFontMedialFeatureInfo();
		}
		if (featureTag == SystemFontTags.FEATURE_REQUIRED_LIGATURES)
		{
			return new SystemFontRLigFeatureInfo();
		}
		if (featureTag == SystemFontTags.FEATURE_STANDARD_LIGATURES)
		{
			return new SystemFontLigaFeatureInfo();
		}
		return null;
	}

	private static SystemFontGlyphsSequence ApplyMultipleSubstitutionLookup(SystemFontLookup lookup, SystemFontGlyphsSequence glyphIDs)
	{
		return lookup.Apply(glyphIDs);
	}

	public abstract bool ShouldApply(SystemFontGlyphInfo glyphIndex);

	public SystemFontGlyphsSequence ApplyLookup(SystemFontLookup lookup, SystemFontGlyphsSequence glyphIDs)
	{
		if (lookup == null)
		{
			return glyphIDs;
		}
		return Type switch
		{
			FeatureType.SingleSubstitution => ApplySingleGlyphSubstitutionLookup(lookup, glyphIDs), 
			FeatureType.MultipleSubstitution => ApplyMultipleSubstitutionLookup(lookup, glyphIDs), 
			_ => glyphIDs, 
		};
	}

	private SystemFontGlyphsSequence ApplySingleGlyphSubstitutionLookup(SystemFontLookup lookup, SystemFontGlyphsSequence glyphIDs)
	{
		SystemFontGlyphsSequence systemFontGlyphsSequence = new SystemFontGlyphsSequence();
		foreach (SystemFontGlyphInfo glyphID in glyphIDs)
		{
			if (ShouldApply(glyphID))
			{
				systemFontGlyphsSequence.AddRange(lookup.Apply(new SystemFontGlyphsSequence(1) { glyphID }));
			}
			else
			{
				systemFontGlyphsSequence.Add(glyphID);
			}
		}
		return systemFontGlyphsSequence;
	}
}
