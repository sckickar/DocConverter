using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontNameIndex : SystemFontIndex
{
	private string[] names;

	public string this[ushort sid] => GetString(sid);

	public SystemFontNameIndex(SystemFontCFFFontFile file, long offset)
		: base(file, offset)
	{
	}

	private string ReadString(SystemFontCFFFontReader reader, uint offset, int length)
	{
		reader.BeginReadingBlock();
		reader.Seek(base.DataOffset + offset, SeekOrigin.Begin);
		string result = reader.ReadString(length);
		reader.EndReadingBlock();
		return result;
	}

	private string GetString(int index)
	{
		if (names[index] == null)
		{
			names[index] = ReadString(base.Reader, base.Offsets[index], GetDataLength(index));
		}
		return names[index];
	}

	public override void Read(SystemFontCFFFontReader reader)
	{
		base.Read(reader);
		if (base.Count != 0)
		{
			names = new string[base.Count];
		}
	}
}
