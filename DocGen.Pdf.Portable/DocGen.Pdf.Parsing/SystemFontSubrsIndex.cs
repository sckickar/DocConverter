using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontSubrsIndex : SystemFontIndex
{
	private readonly int charstringType;

	private byte[][] subrs;

	private ushort bias;

	public byte[] this[int index] => GetSubr(index + bias);

	public SystemFontSubrsIndex(SystemFontCFFFontFile fontFile, int charstringType, long offset)
		: base(fontFile, offset)
	{
		this.charstringType = charstringType;
	}

	private byte[] ReadSubr(SystemFontCFFFontReader reader, uint offset, int length)
	{
		reader.BeginReadingBlock();
		long num = base.DataOffset + offset;
		reader.Seek(num, SeekOrigin.Begin);
		byte[] array = new byte[length];
		reader.Read(array, length);
		reader.EndReadingBlock();
		return array;
	}

	private byte[] GetSubr(int index)
	{
		if (subrs[index] == null)
		{
			subrs[index] = ReadSubr(base.Reader, base.Offsets[index], GetDataLength(index));
		}
		return subrs[index];
	}

	public override void Read(SystemFontCFFFontReader reader)
	{
		base.Read(reader);
		if (base.Count != 0)
		{
			subrs = new byte[base.Count][];
			ushort count = base.Count;
			if (charstringType == 1)
			{
				bias = 0;
			}
			else if (count < 1240)
			{
				bias = 107;
			}
			else if (count < 33900)
			{
				bias = 1131;
			}
			else
			{
				bias = 32768;
			}
		}
	}
}
