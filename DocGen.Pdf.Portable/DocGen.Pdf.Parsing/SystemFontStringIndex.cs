using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontStringIndex : SystemFontIndex
{
	private string[] strings;

	public string this[ushort sid]
	{
		get
		{
			if (SystemFontStandardStrings.IsStandardString(sid))
			{
				return SystemFontStandardStrings.GetStandardString(sid);
			}
			sid = (ushort)(sid - SystemFontStandardStrings.StandardStringsCount);
			return GetString(sid);
		}
	}

	public SystemFontStringIndex(SystemFontCFFFontFile file, long offset)
		: base(file, offset)
	{
	}

	private string ReadString(SystemFontCFFFontReader reader, uint offset, int length)
	{
		reader.BeginReadingBlock();
		long num = base.DataOffset + offset;
		reader.Seek(num, SeekOrigin.Begin);
		string result = reader.ReadString(length);
		reader.EndReadingBlock();
		return result;
	}

	private string GetString(int index)
	{
		if (strings[index] == null)
		{
			strings[index] = ReadString(base.Reader, base.Offsets[index], GetDataLength(index));
		}
		return strings[index];
	}

	public override void Read(SystemFontCFFFontReader reader)
	{
		base.Read(reader);
		if (base.Count != 0)
		{
			strings = new string[base.Count];
		}
	}
}
