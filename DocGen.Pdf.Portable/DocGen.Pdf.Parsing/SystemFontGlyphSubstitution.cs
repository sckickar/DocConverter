using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontGlyphSubstitution : SystemFontTrueTypeTableBase
{
	private ushort scriptListOffset;

	private ushort featureListOffset;

	private ushort lookupListOffset;

	private SystemFontScriptList scriptList;

	private SystemFontLookupList lookupList;

	private SystemFontFeatureList featureList;

	internal override uint Tag => SystemFontTags.GSUB_TABLE;

	public SystemFontGlyphSubstitution(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	private void ReadTable<T>(T table, ushort offset) where T : SystemFontTableBase
	{
		if (offset != 0)
		{
			long offset2 = (table.Offset = base.Offset + offset);
			base.Reader.BeginReadingBlock();
			base.Reader.Seek(offset2, SeekOrigin.Begin);
			table.Read(base.Reader);
			base.Reader.EndReadingBlock();
		}
	}

	private void ReadScriptList()
	{
		scriptList = new SystemFontScriptList(base.FontSource);
		ReadTable(scriptList, scriptListOffset);
	}

	private void ReadFeatureList()
	{
		featureList = new SystemFontFeatureList(base.FontSource);
		ReadTable(featureList, featureListOffset);
	}

	private void ReadLookupList()
	{
		lookupList = new SystemFontLookupList(base.FontSource);
		ReadTable(lookupList, lookupListOffset);
	}

	public SystemFontScript GetScript(uint tag)
	{
		if (scriptList == null)
		{
			ReadScriptList();
		}
		return scriptList.GetScript(tag);
	}

	public SystemFontFeature GetFeature(ushort index)
	{
		if (featureList == null)
		{
			ReadFeatureList();
		}
		return featureList.GetFeature(index);
	}

	public SystemFontLookup GetLookup(ushort index)
	{
		if (lookupList == null)
		{
			ReadLookupList();
		}
		return lookupList.GetLookup(index);
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadFixed();
		scriptListOffset = reader.ReadUShort();
		featureListOffset = reader.ReadUShort();
		lookupListOffset = reader.ReadUShort();
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		ReadScriptList();
		ReadFeatureList();
		ReadLookupList();
		if (scriptList == null || featureList == null || lookupList == null)
		{
			writer.Write(0);
			return;
		}
		writer.Write(1);
		scriptList.Write(writer);
		featureList.Write(writer);
		lookupList.Write(writer);
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		if (reader.Read() > 0)
		{
			scriptList = new SystemFontScriptList(base.FontSource);
			featureList = new SystemFontFeatureList(base.FontSource);
			lookupList = new SystemFontLookupList(base.FontSource);
			scriptList.Import(reader);
			featureList.Import(reader);
			lookupList.Import(reader);
		}
	}
}
