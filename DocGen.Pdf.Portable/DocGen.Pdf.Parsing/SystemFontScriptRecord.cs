namespace DocGen.Pdf.Parsing;

internal class SystemFontScriptRecord
{
	public uint ScriptTag { get; private set; }

	public ushort ScriptOffset { get; private set; }

	public void Read(SystemFontOpenTypeFontReader reader)
	{
		ScriptTag = reader.ReadULong();
		ScriptOffset = reader.ReadUShort();
	}

	public override string ToString()
	{
		return SystemFontTags.GetStringFromTag(ScriptTag);
	}
}
