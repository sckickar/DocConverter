using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontTopIndex : SystemFontIndex
{
	private SystemFontTop[] tops;

	public SystemFontTop this[int index] => GetTop(index);

	public SystemFontTopIndex(SystemFontCFFFontFile file, long offset)
		: base(file, offset)
	{
	}

	private SystemFontTop ReadTop(SystemFontCFFFontReader reader, uint offset, int length)
	{
		reader.BeginReadingBlock();
		long num = base.DataOffset + offset;
		reader.Seek(num, SeekOrigin.Begin);
		SystemFontTop systemFontTop = new SystemFontTop(base.File, num, length);
		systemFontTop.Read(reader);
		reader.EndReadingBlock();
		return systemFontTop;
	}

	private SystemFontTop GetTop(int index)
	{
		if (tops[index] == null)
		{
			tops[index] = ReadTop(base.Reader, base.Offsets[index], GetDataLength(index));
		}
		return tops[index];
	}

	public override void Read(SystemFontCFFFontReader reader)
	{
		base.Read(reader);
		if (base.Count != 0)
		{
			tops = new SystemFontTop[base.Count];
		}
	}
}
