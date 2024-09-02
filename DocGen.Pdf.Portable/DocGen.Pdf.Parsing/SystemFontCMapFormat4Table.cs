using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontCMapFormat4Table : SystemFontCMapTable
{
	private SystemFontSegment[] segments;

	private ushort firstCode;

	public override ushort FirstCode => firstCode;

	public override ushort GetGlyphId(ushort charCode)
	{
		SystemFontSegment[] array = segments;
		foreach (SystemFontSegment systemFontSegment in array)
		{
			if (systemFontSegment.IsInside(charCode))
			{
				return systemFontSegment.GetGlyphId(charCode);
			}
		}
		return 0;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadUShort();
		reader.ReadUShort();
		ushort num = (ushort)(reader.ReadUShort() / 2);
		reader.ReadUShort();
		reader.ReadUShort();
		reader.ReadUShort();
		ushort[] array = new ushort[num];
		ushort[] array2 = new ushort[num];
		short[] array3 = new short[num];
		ushort[] array4 = new ushort[num];
		segments = new SystemFontSegment[num];
		firstCode = ushort.MaxValue;
		for (int i = 0; i < num; i++)
		{
			array[i] = reader.ReadUShort();
		}
		reader.ReadUShort();
		for (int j = 0; j < num; j++)
		{
			array2[j] = reader.ReadUShort();
			if (firstCode > array2[j])
			{
				firstCode = array2[j];
			}
		}
		for (int k = 0; k < num; k++)
		{
			array3[k] = reader.ReadShort();
		}
		for (int l = 0; l < num; l++)
		{
			long position = reader.Position;
			array4[l] = reader.ReadUShort();
			if (array4[l] <= 0)
			{
				segments[l] = new SystemFontSegment(array2[l], array[l], array3[l]);
				continue;
			}
			position += array4[l];
			int num2 = array[l] - array2[l] + 1;
			ushort[] array5 = new ushort[num2];
			reader.BeginReadingBlock();
			reader.Seek(position, SeekOrigin.Begin);
			for (int m = 0; m < num2; m++)
			{
				array5[m] = reader.ReadUShort();
			}
			segments[l] = new SystemFontSegment(array2[l], array[l], array3[l], array5);
			reader.EndReadingBlock();
		}
	}

	public override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(4);
		writer.WriteUShort(firstCode);
		writer.WriteUShort((ushort)segments.Length);
		for (int i = 0; i < segments.Length; i++)
		{
			segments[i].Write(writer);
		}
	}

	public override void Import(SystemFontOpenTypeFontReader reader)
	{
		firstCode = reader.ReadUShort();
		ushort num = reader.ReadUShort();
		segments = new SystemFontSegment[num];
		for (int i = 0; i < num; i++)
		{
			SystemFontSegment systemFontSegment = new SystemFontSegment();
			systemFontSegment.Import(reader);
			segments[i] = systemFontSegment;
		}
	}
}
