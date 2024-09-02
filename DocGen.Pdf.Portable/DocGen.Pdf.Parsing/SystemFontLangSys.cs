using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontLangSys : SystemFontTableBase
{
	private ushort reqFeatureIndex;

	private ushort[] featureIndices;

	private List<Tuple<SystemFontFeatureInfo, SystemFontLookup>> lookups;

	private IEnumerable<Tuple<SystemFontFeatureInfo, SystemFontLookup>> Lookups
	{
		get
		{
			if (lookups == null)
			{
				IEnumerable<Tuple<SystemFontFeatureInfo, ushort>> lookupIndices = GetLookupIndices();
				lookups = new List<Tuple<SystemFontFeatureInfo, SystemFontLookup>>(SystemFontEnumerable.Count(lookupIndices));
				foreach (Tuple<SystemFontFeatureInfo, ushort> item in lookupIndices)
				{
					lookups.Add(new Tuple<SystemFontFeatureInfo, SystemFontLookup>(item.Item1, base.FontSource.GetLookup(item.Item2)));
				}
			}
			return lookups;
		}
	}

	public SystemFontLangSys(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	private int Compare(Tuple<SystemFontFeatureInfo, ushort> left, Tuple<SystemFontFeatureInfo, ushort> right)
	{
		return left.Item2.CompareTo(right.Item2);
	}

	private IEnumerable<SystemFontFeature> GetFeatures()
	{
		List<SystemFontFeature> list = new List<SystemFontFeature>(featureIndices.Length);
		for (int i = 0; i < featureIndices.Length; i++)
		{
			SystemFontFeature feature = base.FontSource.GetFeature(featureIndices[i]);
			if (feature != null)
			{
				list.Add(feature);
			}
		}
		if (reqFeatureIndex != ushort.MaxValue)
		{
			list.Add(base.FontSource.GetFeature(reqFeatureIndex));
		}
		return list;
	}

	private IEnumerable<Tuple<SystemFontFeatureInfo, ushort>> GetLookupIndices()
	{
		List<Tuple<SystemFontFeatureInfo, ushort>> list = new List<Tuple<SystemFontFeatureInfo, ushort>>();
		foreach (SystemFontFeature feature in GetFeatures())
		{
			ushort[] lookupsListIndices = feature.LookupsListIndices;
			foreach (ushort item in lookupsListIndices)
			{
				list.Add(new Tuple<SystemFontFeatureInfo, ushort>(feature.FeatureInfo, item));
			}
		}
		list.Sort(Compare);
		return list;
	}

	public SystemFontGlyphsSequence Apply(SystemFontGlyphsSequence glyphIDs)
	{
		SystemFontGlyphsSequence systemFontGlyphsSequence = glyphIDs;
		foreach (Tuple<SystemFontFeatureInfo, SystemFontLookup> lookup in Lookups)
		{
			systemFontGlyphsSequence = lookup.Item1.ApplyLookup(lookup.Item2, systemFontGlyphsSequence);
		}
		return systemFontGlyphsSequence;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadUShort();
		reqFeatureIndex = reader.ReadUShort();
		ushort num = reader.ReadUShort();
		featureIndices = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			featureIndices[i] = reader.ReadUShort();
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(reqFeatureIndex);
		writer.WriteUShort((ushort)featureIndices.Length);
		for (int i = 0; i < featureIndices.Length; i++)
		{
			writer.WriteUShort(featureIndices[i]);
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		reqFeatureIndex = reader.ReadUShort();
		ushort num = reader.ReadUShort();
		featureIndices = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			featureIndices[i] = reader.ReadUShort();
		}
	}
}
