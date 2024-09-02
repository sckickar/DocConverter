namespace DocGen.Pdf.Parsing;

internal class SystemFontSequence
{
	public ushort[] Subsitutes { get; private set; }

	public void Read(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		Subsitutes = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			Subsitutes[i] = reader.ReadUShort();
		}
	}

	internal void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort((ushort)Subsitutes.Length);
		for (int i = 0; i < Subsitutes.Length; i++)
		{
			writer.WriteUShort(Subsitutes[i]);
		}
	}

	internal void Import(SystemFontOpenTypeFontReader reader)
	{
		Read(reader);
	}
}
