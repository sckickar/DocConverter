using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontScriptList : SystemFontTableBase
{
	private Dictionary<uint, SystemFontScriptRecord> scriptRecords;

	private Dictionary<uint, SystemFontScript> scripts;

	public SystemFontScriptList(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	private SystemFontScript ReadScript(SystemFontOpenTypeFontReader reader, SystemFontScriptRecord record)
	{
		reader.BeginReadingBlock();
		long offset = base.Offset + record.ScriptOffset;
		reader.Seek(offset, SeekOrigin.Begin);
		SystemFontScript systemFontScript = new SystemFontScript(base.FontSource, record.ScriptTag);
		systemFontScript.Offset = offset;
		systemFontScript.Read(reader);
		reader.EndReadingBlock();
		return systemFontScript;
	}

	public SystemFontScript GetScript(uint tag)
	{
		if (!scripts.TryGetValue(tag, out SystemFontScript value) && scriptRecords != null)
		{
			value = ((!scriptRecords.TryGetValue(tag, out SystemFontScriptRecord value2) && !scriptRecords.TryGetValue(SystemFontTags.DEFAULT_TABLE_SCRIPT_TAG, out value2)) ? null : ReadScript(base.Reader, value2));
			scripts[tag] = value;
		}
		return value;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		scriptRecords = new Dictionary<uint, SystemFontScriptRecord>(num);
		scripts = new Dictionary<uint, SystemFontScript>(num);
		for (int i = 0; i < num; i++)
		{
			SystemFontScriptRecord systemFontScriptRecord = new SystemFontScriptRecord();
			systemFontScriptRecord.Read(reader);
			scriptRecords[systemFontScriptRecord.ScriptTag] = systemFontScriptRecord;
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort((ushort)scriptRecords.Count);
		foreach (uint key in scriptRecords.Keys)
		{
			ReadScript(base.Reader, scriptRecords[key]).Write(writer);
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		scripts = new Dictionary<uint, SystemFontScript>(num);
		for (int i = 0; i < num; i++)
		{
			uint num2 = reader.ReadULong();
			if (num2 != SystemFontTags.NULL_TAG)
			{
				SystemFontScript systemFontScript = new SystemFontScript(base.FontSource, num2);
				systemFontScript.Import(reader);
				scripts[num2] = systemFontScript;
			}
		}
	}
}
