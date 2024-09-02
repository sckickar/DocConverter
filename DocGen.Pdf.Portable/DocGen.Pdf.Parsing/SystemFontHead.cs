using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontHead : SystemFontTrueTypeTableBase
{
	private ushort macStyle;

	internal override uint Tag => SystemFontTags.HEAD_TABLE;

	public ushort Flags { get; private set; }

	public short GlyphDataFormat { get; private set; }

	public ushort UnitsPerEm { get; private set; }

	public Rect BBox { get; private set; }

	public short IndexToLocFormat { get; private set; }

	public bool IsBold => CheckMacStyle(0);

	public bool IsItalic => CheckMacStyle(1);

	public SystemFontHead(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	private bool CheckMacStyle(byte bit)
	{
		return (macStyle & (1 << (int)bit)) != 0;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadFixed();
		reader.ReadFixed();
		reader.ReadULong();
		reader.ReadULong();
		Flags = reader.ReadUShort();
		UnitsPerEm = reader.ReadUShort();
		reader.ReadLongDateTime();
		reader.ReadLongDateTime();
		BBox = new Rect(new Point(reader.ReadShort(), reader.ReadShort()), new Point(reader.ReadShort(), reader.ReadShort()));
		macStyle = reader.ReadUShort();
		reader.ReadUShort();
		reader.ReadShort();
		IndexToLocFormat = reader.ReadShort();
		reader.ReadShort();
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(UnitsPerEm);
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		UnitsPerEm = reader.ReadUShort();
	}
}
