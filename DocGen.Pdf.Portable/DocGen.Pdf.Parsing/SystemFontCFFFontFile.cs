using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontCFFFontFile
{
	private readonly SystemFontCFFFontReader reader;

	private SystemFontCFFFontSource fontSource;

	public SystemFontCFFFontSource FontSource => fontSource;

	public SystemFontHeader Header { get; private set; }

	public SystemFontNameIndex Name { get; private set; }

	public SystemFontTopIndex TopIndex { get; private set; }

	public SystemFontStringIndex StringIndex { get; private set; }

	public SystemFontSubrsIndex GlobalSubrs { get; private set; }

	public SystemFontCFFFontReader Reader => reader;

	public SystemFontCFFFontFile(byte[] data)
	{
		reader = new SystemFontCFFFontReader(data);
		Initialize();
	}

	public string ReadString(ushort sid)
	{
		return StringIndex[sid];
	}

	internal void ReadTable(SystemFontCFFTable table)
	{
		Reader.BeginReadingBlock();
		Reader.Seek(table.Offset, SeekOrigin.Begin);
		table.Read(Reader);
		Reader.EndReadingBlock();
	}

	private void Initialize()
	{
		Header = new SystemFontHeader(this);
		ReadTable(Header);
		Name = new SystemFontNameIndex(this, Header.HeaderSize);
		ReadTable(Name);
		TopIndex = new SystemFontTopIndex(this, Name.SkipOffset);
		ReadTable(TopIndex);
		StringIndex = new SystemFontStringIndex(this, TopIndex.SkipOffset);
		ReadTable(StringIndex);
		SystemFontTop systemFontTop = TopIndex[0];
		GlobalSubrs = new SystemFontSubrsIndex(this, systemFontTop.CharstringType, StringIndex.SkipOffset);
		ReadTable(GlobalSubrs);
		fontSource = new SystemFontCFFFontSource(this, systemFontTop);
	}
}
