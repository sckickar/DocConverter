using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontScript : SystemFontTableBase
{
	private ushort defaultLangSysOffset;

	private SystemFontLangSys defaultLangSys;

	public uint ScriptTag { get; private set; }

	public SystemFontLangSys DefaultLangSys
	{
		get
		{
			if (defaultLangSys == null && defaultLangSysOffset != 0)
			{
				defaultLangSys = ReadLangSys(base.Reader, defaultLangSysOffset);
			}
			return defaultLangSys;
		}
	}

	public SystemFontScript(SystemFontOpenTypeFontSourceBase fontFile, uint scriptTag)
		: base(fontFile)
	{
		ScriptTag = scriptTag;
	}

	private SystemFontLangSys ReadLangSys(SystemFontOpenTypeFontReader reader, ushort offset)
	{
		reader.BeginReadingBlock();
		reader.Seek(base.Offset + offset, SeekOrigin.Begin);
		SystemFontLangSys systemFontLangSys = new SystemFontLangSys(base.FontSource);
		systemFontLangSys.Read(reader);
		reader.EndReadingBlock();
		return systemFontLangSys;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		defaultLangSysOffset = reader.ReadUShort();
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		if (DefaultLangSys != null)
		{
			writer.WriteULong(ScriptTag);
			DefaultLangSys.Write(writer);
		}
		else
		{
			writer.WriteULong(SystemFontTags.NULL_TAG);
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		defaultLangSys = new SystemFontLangSys(base.FontSource);
		defaultLangSys.Import(reader);
	}

	public override string ToString()
	{
		return SystemFontTags.GetStringFromTag(ScriptTag);
	}
}
